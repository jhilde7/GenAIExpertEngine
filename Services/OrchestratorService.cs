using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace GenAIExpertEngineAPI.Services
{
    public class OrchestrationIntent
    {
        public bool RequiresToolAidedResponse { get; set; }
        public List<FactCheckRequest> UserQueryFactChecks { get; set; } = new List<FactCheckRequest>();
    }
    public class OrchestratorService
    {
        private readonly GeminiService _geminiService;
        private readonly ExpertRegistryService _expertRegistry;
        private readonly ILogger<OrchestratorService> _logger;
        private readonly ILogger<RefereeService> _refereeLogger; // Added for logging in RefereeService
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public const string NARRATIVE_REQUEST_SIGNAL = "[NARRATIVE_REQUEST]";

        public OrchestratorService(GeminiService geminiService, ExpertRegistryService expertRegistry, IOptions<JsonOptions> jsonOptions, ILogger<OrchestratorService> logger, ILogger<RefereeService> refereeLogger)
        {
            _geminiService = geminiService;
            _expertRegistry = expertRegistry;
            _logger = logger;
            _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
            _refereeLogger = refereeLogger; // Use the injected logger for RefereeService
        }

        public async Task<OrchestrationIntent> DetermineQueryIntentAsync(string userQuery)
        {
            // Reuse the existing fact-checking logic to determine narrative vs. factual/tool-potential
            List<FactCheckRequest> userQueryFactChecks = await FactCheckRequest.GetFactCheckRequestsAsync(_refereeLogger, _geminiService, _expertRegistry, userQuery);

            bool isPurelyNarrative = userQueryFactChecks == null ||
                                     userQueryFactChecks.Count == 0 ||
                                     (userQueryFactChecks.Count == 1 &&
                                      userQueryFactChecks[0].ExpertName == "Narrative Expert" &&
                                      userQueryFactChecks[0].Query == OrchestratorService.NARRATIVE_REQUEST_SIGNAL);

            return new OrchestrationIntent
            {
                RequiresToolAidedResponse = !isPurelyNarrative, // If not purely narrative, it might require tools or experts
                UserQueryFactChecks = userQueryFactChecks // Pass this back so RefereeService can still use it for initial fact processing
            };
        }

        public async Task<string> ProcessUserQueryAsync(string userQuery, List<ChatMessage> history)
        {
            // 1. Get the JSON plan from Gemini
            string? planJson = await _geminiService.GenerateTextAsync(BuildQueryPlanPrompt(userQuery));
            if (string.IsNullOrWhiteSpace(planJson))
            {
                return "I could not devise a plan to answer that question.";
            }

            List<QueryStep>? planSteps = null;
            try
            {
                planSteps = JsonSerializer.Deserialize<List<QueryStep>>(planJson, _jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to deserialize the query plan from the AI. Raw JSON: {planJson}");
                return "I received an invalid query plan from the AI.";
            }

            if (planSteps == null || !planSteps.Any())
            {
                return "The generated plan was empty or invalid.";
            }

            Dictionary<int, string> stepResults = new Dictionary<int, string>();
            List<string> fullTextAnswers = new List<string>();

            // 2. Execute the plan step-by-step
            foreach (QueryStep? step in planSteps.OrderBy(s => s.Step))
            {
                string currentQuery = step.Query;

                // Replace placeholders with results from previous steps
                foreach (KeyValuePair<int, string> result in stepResults)
                {
                    currentQuery = currentQuery.Replace($"{{step_{result.Key}_result}}", result.Value);
                }

                if (_expertRegistry.Experts.TryGetValue(step.ExpertName, out ExpertDefinition? expert) && !string.IsNullOrWhiteSpace(expert.CorpusId))
                {
                    // Make the RAG call for the current step
                    string stepResponse = await _geminiService.GenerateGroundedResponseAsync(currentQuery, history, expert.CorpusId);
                    fullTextAnswers.Add(stepResponse);

                    // If future steps might need data from this one, parse out the key fact.
                    if (planSteps.Any(p => p.Query.Contains($"{{step_{step.Step}_result}}")))
                    {
                        string parsedFact = await ParseFactFromResultAsync(currentQuery, stepResponse);
                        stepResults[step.Step] = parsedFact;
                        _logger.LogInformation($"Step {step.Step}: Parsed fact '{parsedFact}' from response.");
                    }
                }
            }

            // 3. Synthesize and return the final, combined answer to the RefereeService
            return string.Join("\n\n", fullTextAnswers);
        }

        /// <summary>
        /// Uses the LLM to extract a single, specific piece of data from a natural language response.
        /// </summary>
        /// <param name="originalQuestion">The question that was asked to get the response.</param>
        /// <param name="llmResponse">The natural language response from the LLM.</param>
        /// <returns>The extracted key fact as a string.</returns>
        private async Task<string> ParseFactFromResultAsync(string originalQuestion, string llmResponse)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are a data extraction bot. Your only job is to extract a single, specific data value from a sentence in response to a question.");
            promptBuilder.AppendLine("Analyze the original question to understand what piece of data is being requested.");
            promptBuilder.AppendLine("Then, find that specific data value within the 'Sentence to Analyze'.");
            promptBuilder.AppendLine("Respond with ONLY the extracted value. Do not add any explanation, labels, or extra text.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Original Question: \"{originalQuestion}\"");
            promptBuilder.AppendLine($"Sentence to Analyze: \"{llmResponse}\"");
            promptBuilder.AppendLine("Your Response:");

            // Use a simple text call with low temperature for this precise task
            string fact = await _geminiService.GenerateSimpleTextAsync(promptBuilder.ToString(), 0.0f);

            // Return the trimmed fact.
            return fact.Trim();
        }

        private string BuildQueryPlanPrompt(string userQuery)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are a query planning expert for a TTRPG assistant. Your task is to decompose a user's question into a series of steps that can be executed by specialized experts.");
            promptBuilder.AppendLine("CRITICAL INSTRUCTION: You MUST create a plan that directly answers the 'CURRENT USER QUERY'. Do not invent topics or entities not mentioned by the user. If the user's query can be answered in a single step, create a one-step plan. Do not add unnecessary complexity.");
            promptBuilder.AppendLine("ENSURE ALL STRING VALUES WITHIN THE JSON (especially in 'query') ARE PROPERLY JSON-ESCAPED. THIS INCLUDES CHARACTERS LIKE '&', '\\', \"\"\", etc. For example, 'D&D' should be 'D&amp;D' or if the parser is strict, 'D\\u0026D'");
            promptBuilder.AppendLine("DO NOT include any explanatory text, greetings, or any characters before or after the JSON array.");
            promptBuilder.AppendLine("Analyze the user's query and the conversation history. Create a JSON array of 'step' objects. Each step must have an 'expert_name' and a 'query' for that expert.");
            promptBuilder.AppendLine("**IMPORTANT: You MUST ONLY use 'expert_name' values from the 'Available Experts' list provided below. Do NOT invent new expert names.**");
            promptBuilder.AppendLine("If a later step depends on the result of a previous step, use a placeholder like '{step_1_result}'.");
            
            promptBuilder.AppendLine("\n--- EXAMPLE ---");
            promptBuilder.AppendLine("User Query: \"I killed the troll, what do I get?\"");
            promptBuilder.AppendLine("Expected JSON Plan:");
            promptBuilder.AppendLine("[");
            promptBuilder.AppendLine("  { \"step\": 1, \"expert_name\": \"Monster Expert\", \"query\": \"What is the Treasure_Type of a Troll?\" },");
            promptBuilder.AppendLine("  { \"step\": 2, \"expert_name\": \"Treasure Expert\", \"query\": \"What is the breakdown of Treasure Type {step_1_result}?\" }");
            promptBuilder.AppendLine("]");
            promptBuilder.AppendLine("\n--- END EXAMPLE ---");

            // Dynamically list all available experts and their descriptions
            promptBuilder.AppendLine("\nAvailable Experts (Name: Description):");
            foreach (var expert in _expertRegistry.GetAllExperts())
            {
                promptBuilder.AppendLine($"- \"{expert.Name}\": \"{expert.Description}\"");
            }

            promptBuilder.AppendLine(userQuery);
            promptBuilder.AppendLine("\nGenerate the JSON plan now:");

            return promptBuilder.ToString();
        }
    }

    // This class represents a single step in the query plan generated by the AI.
    public class QueryStep
        {
            [JsonPropertyName("step")]
            public int Step { get; set; }

            [JsonPropertyName("expert_name")]
            public string ExpertName { get; set; } = string.Empty;

            [JsonPropertyName("query")]
            public string Query { get; set; } = string.Empty;
    }
}
