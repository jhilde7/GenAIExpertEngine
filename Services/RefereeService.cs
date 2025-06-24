using GenerativeAI.Tools;
using GenerativeAI.Types;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenAIExpertEngineAPI.Services
{
    public class RefereeResponseOutput
    {
        public string Narrative { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    public class RefereeService
    {
        private readonly OrchestratorService _orchestratorService;
        private readonly ExpertRegistryService _expertRegistry;
        private readonly GeminiService _geminiService;
        private readonly ConversationHistoryService _historyService;
        private readonly ICharacterStateManager _characterStateManager;
        private readonly ILogger<RefereeService> _logger;

        public RefereeService(ExpertRegistryService expertRegistry,
            OrchestratorService orchestratorService,
            GeminiService geminiService,
            ConversationHistoryService historyService,
            ICharacterStateManager characterStateManager,
            ILogger<RefereeService> logger)
        {
            _orchestratorService = orchestratorService;
            _expertRegistry = expertRegistry;
            _geminiService = geminiService;
            _historyService = historyService;
            _characterStateManager = characterStateManager;
            _logger = logger;
        }

        /// <summary>
        /// Processes a user's query by first determining factual needs, retrieving facts,
        /// and then formulating a grounded, narrative, conversational response.
        /// </summary>
        public async Task<RefereeResponseOutput> GenerateRefereeResponseAsync(string conversationId, string userQuery)
        {
            // Get the history for this conversation
            List<ChatMessage> history = _historyService.GetHistory(conversationId);

            // Initial Fact-Check on User Query ---
            OrchestrationIntent intent = await _orchestratorService.DetermineQueryIntentAsync(userQuery);
            List<FactCheckRequest> userQueryFactChecks = intent.UserQueryFactChecks; // Get initial fact checks from orchestrator
            bool requiresToolAidedResponse = intent.RequiresToolAidedResponse; // Orchestrator's decision


            string llmRawResponse;
            RefereeResponseOutput finalRefereeResponseOutput;

            Dictionary<string, string> initialExpertFactResults = new Dictionary<string, string>();
            if (userQueryFactChecks != null && userQueryFactChecks.Any() && !requiresToolAidedResponse) // Only process initial facts if not purely narrative and not tool-aided
            {
                foreach (FactCheckRequest factCheck in userQueryFactChecks.Where(fc => fc.ExpertName != "Narrative Expert"))
                {
                    if (string.IsNullOrWhiteSpace(factCheck?.Query))
                    {
                        _logger.LogWarning("FactCheckRequest from initial user query with null or empty Query encountered. Skipping.");
                        continue;
                    }
                    string expertAnswer = await _orchestratorService.ProcessUserQueryAsync(factCheck.Query, history);
                    if (!string.IsNullOrEmpty(expertAnswer) && expertAnswer != "Information not available in the provided data.")
                    {
                        initialExpertFactResults[factCheck.Query] = expertAnswer;
                    }
                }
            }

            if (!requiresToolAidedResponse)
            {
                _logger.LogInformation("RefereeService: Query is purely narrative. Generating simple text response.");
                // For purely narrative queries, generate a simple creative text response.
                llmRawResponse = await _geminiService.GenerateSimpleTextAsync(BuildCreativeNarrativePrompt(userQuery, history), 0.7f);
                finalRefereeResponseOutput = ParseLlmResponseForSuggestions(llmRawResponse);
            }
            else
            {
                _logger.LogInformation("RefereeService: Query may require tool usage or expert interaction. Attempting tool-aided response.");
                // For other queries, attempt to generate a response with tools enabled.
                llmRawResponse = await HandleToolAidedResponseAsync(conversationId, userQuery, history);
                finalRefereeResponseOutput = ParseLlmResponseForSuggestions(llmRawResponse);
            }

            // Fact-check the *generated narrative* for accuracy.
            // Only fact-check the narrative the AI actually produced, not the user's initial query.
            List<FactCheckRequest> responseFactChecks = await FactCheckRequest.GetFactCheckResponseAsync(_logger, _geminiService, _expertRegistry, finalRefereeResponseOutput.Narrative);

            if (responseFactChecks != null && responseFactChecks.Any())
            {
                Dictionary<string, string> expertCorrections = new Dictionary<string, string>();
                foreach (FactCheckRequest factCheck in responseFactChecks.Where(fc => fc.ExpertName != "Narrative Expert"))
                {
                    if (string.IsNullOrWhiteSpace(factCheck?.Query))
                    {
                        _logger.LogWarning("FactCheckRequest from AI response with null or empty Query encountered. Skipping.");
                        continue;
                    }
                    string expertAnswer = await _orchestratorService.ProcessUserQueryAsync(factCheck.Query, history);
                    // Store the answer only if it's not empty and not the "information not available" message
                    if (!string.IsNullOrEmpty(expertAnswer) && expertAnswer != "Information not available in the provided data.")
                    {
                        expertCorrections[factCheck.Query] = expertAnswer;
                    }
                }

                if (expertCorrections.Any())
                {
                    // Call a dedicated refinement method for self-correction.
                    string refinedNarrative = await RefineResponseForAccuracyAsync(finalRefereeResponseOutput.Narrative, expertCorrections);
                    finalRefereeResponseOutput.Narrative = refinedNarrative;
                }
            }

            // Save the conversation turn to history
            _historyService.AddMessage(conversationId, new ChatMessage { Author = "user", Content = userQuery });
            _historyService.AddMessage(conversationId, new ChatMessage { Author = "ai", Content = finalRefereeResponseOutput.Narrative });

            return finalRefereeResponseOutput;
        }

        /// <summary>
        /// Handles a conversation turn that might involve Gemini making tool calls.
        /// It iteratively processes FunctionCallParts and sends FunctionResponseParts back.
        /// </summary>
        private async Task<string> HandleToolAidedResponseAsync(string conversationId, string userQuery, List<ChatMessage> history)
        {
            List<Content> currentContents = new List<Content>();
            if (history != null && history.Any())
            {
                foreach (var message in history)
                {
                    string role = message.Author?.ToLower() == "user" ? "user" : "model";
                    currentContents.Add(new Content(message.Content ?? string.Empty, role));
                }
            }
            else
            {
                history = new List<ChatMessage>();
            }
            currentContents.Add(new Content(userQuery, "user"));

            GenerateContentResponse? geminiResponse = null;
            string finalNarrative = "I'm thinking..."; // Initialize with a default placeholder
            int maxIterations = 8;
            int iteration = 0;

            string currentPromptForGemini = userQuery; // The initial prompt is the user's query
            StringBuilder toolAidedSystemInstruction = new StringBuilder();
            toolAidedSystemInstruction.AppendLine("You are the Referee, the Game Master for a tabletop RPG. Your primary goal is to facilitate the game by responding to the player's actions, describing the world, and managing game mechanics.");
            toolAidedSystemInstruction.AppendLine("**URGENT AND NON-NEGOTIABLE:** You *must* prioritize the use of your provided tools for actions that directly correspond to their functionalities. If a player's query or statement can be resolved by a tool, you **MUST** call that tool and await its result.");
            toolAidedSystemInstruction.AppendLine("Specifically:");
            toolAidedSystemInstruction.AppendLine("- If a player asks to roll dice (e.g., 'Roll 4d6', 'I want to roll a d20'), you **MUST** call the `RollDice` tool.");
            toolAidedSystemInstruction.AppendLine("- If a player states their desired character race (e.g., 'I want to be a Drow', 'My race is Human'), you **MUST** call the `SetCharacterRace` tool.");
            toolAidedSystemInstruction.AppendLine("- If a player states their desired character class (e.g., 'I want to be a Thief', 'My class is Fighter'), you **MUST** call the `SetCharacterClass` tool.");
            toolAidedSystemInstruction.AppendLine("Do not simulate tool actions or generate fake results. Wait for the tool's actual output and then weave it into your narrative.");
            toolAidedSystemInstruction.AppendLine("Your tone should be helpful, descriptive, and engaging. Be concise and to the point where appropriate. Prioritize direct answers and actionable steps for rules and mechanics.");
            toolAidedSystemInstruction.AppendLine("You are now ready to respond to the player's query. Begin with the following user query or tool response:");

            while (iteration++ < maxIterations)
            {
                StringBuilder currentTurnPromptBuilder = new StringBuilder();
                currentTurnPromptBuilder.AppendLine("---"); // Separator for clarity
                currentTurnPromptBuilder.AppendLine($"User Query (or Tool Response): {currentPromptForGemini}"); // This holds the original query or the tool result.

                string contextualPrompt = toolAidedSystemInstruction.ToString() + currentTurnPromptBuilder.ToString();

                _logger.LogInformation($"DEBUG: Calling GeminiService.GenerateContentWithToolsAsync for convo '{conversationId}' with prompt (first 100 chars): '{currentTurnPromptBuilder.ToString().Substring(0, Math.Min(currentTurnPromptBuilder.Length, 100))}'");
                _logger.LogInformation($"DEBUG: Current conversation history length: {history.Count}");
                
                // CSharpToJsonSchema generates these extensions typically
                // These methods usually reside in a generated static class or a specific extension namespace
                // The exact namespace might vary, often it's CSharpToJsonSchema.Internal or similar,
                // depending on your CSharpToJsonSchema package version and setup.
                //var characterFunctionTool = _characterStateManager.AsGoogleFunctionTool();

                geminiResponse = await _geminiService.GenerateContentWithToolsAsync(
                    prompt: contextualPrompt,
                    history: history//,
                    //characterFunctionTool: genericFunctionTool,
                    //temperature: 0.7f
                );

                if (geminiResponse?.Candidates == null || !geminiResponse.Candidates.Any())
                {
                    finalNarrative = "I couldn't generate a response at this moment. Please try rephrasing your request.";
                    _logger.LogWarning("Gemini did not return any candidates in HandleToolAidedResponseAsync.");
                    break;
                }

                if (geminiResponse.Candidates == null || !geminiResponse.Candidates.Any()) // Removed ?. before Candidates.Count to avoid NRE. Candidates is nullable.
                {
                    finalNarrative = "I couldn't generate a response at this moment (no candidates returned). Please try rephrasing your request.";
                    _logger.LogWarning($"DEBUG: Gemini did not return any candidates in HandleToolAidedResponseAsync for convo '{conversationId}'.");
                    break;
                }

                var candidate = geminiResponse.Candidates.First(); // Safe to call .First() here if Candidates is not null/empty
                if (candidate.Content?.Parts == null || !candidate.Content.Parts.Any())
                {
                    finalNarrative = "My systems are having trouble responding (empty content parts). Could you please try again?";
                    _logger.LogWarning($"DEBUG: Gemini candidate content parts are empty in HandleToolAidedResponseAsync for convo '{conversationId}'.");
                    break;
                }

                var firstPart = candidate.Content.Parts.First();

                if (!string.IsNullOrEmpty(firstPart.Text))
                {
                    finalNarrative = firstPart.Text;
                    _logger.LogInformation($"DEBUG: Gemini returned TEXT response for convo '{conversationId}': {finalNarrative.Substring(0, Math.Min(finalNarrative.Length, 100))}");
                    break;
                }
                else if (firstPart.FunctionCall != null)
                {
                    var functionCall = firstPart.FunctionCall;
                    JsonObject? argsObject = functionCall.Args?.AsObject();
                    _logger.LogInformation($"DEBUG: Gemini returned FUNCTION_CALL for convo '{conversationId}': Name='{firstPart.FunctionCall.Name}', Args='{argsObject?.ToJsonString()}'");

                    if (argsObject == null)
                    {
                        finalNarrative = $"Tool call failed: Arguments for '{functionCall.Name}' are not a valid JSON object.";
                        _logger.LogError(finalNarrative);
                        break;
                    }

                    string errorMessage = string.Empty;
                    object? toolResult = null; // Declare toolResult outside the try block

                    try
                    {
                        MethodInfo? method = typeof(CharacterStateManager).GetMethod(functionCall.Name);
                        if (method == null)
                        {
                            errorMessage = $"Tool call failed: Function '{functionCall.Name}' not found.";
                            _logger.LogError(errorMessage);
                            finalNarrative = errorMessage;
                            break;
                        }

                        var parameters = new List<object?>();
                        foreach (ParameterInfo paramInfo in method.GetParameters())
                        {
                            object? argValue = null;
                            if (paramInfo.Name != null && argsObject.ContainsKey(paramInfo.Name))
                            {
                                JsonNode? argNode = argsObject[paramInfo.Name];
                                if (argNode != null)
                                {
                                    try
                                    {
                                        var type = paramInfo.ParameterType;
                                        if (type == typeof(string))
                                            argValue = argNode.GetValue<string>();
                                        else if (type == typeof(int))
                                            argValue = argNode.GetValue<int>();
                                        else if (type == typeof(bool))
                                            argValue = argNode.GetValue<bool>();
                                        else if (type == typeof(double))
                                            argValue = argNode.GetValue<double>();
                                        else if (type.IsEnum)
                                        {
                                            if (!Enum.TryParse(type, argNode.GetValue<string>(), true, out var enumValue))
                                                throw new ArgumentException($"Could not parse '{argNode.GetValue<string>()}' to enum '{type.Name}' for parameter '{paramInfo.Name}'.");
                                            argValue = enumValue;
                                        }
                                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                                        {
                                            argValue = argNode.Deserialize(type, ApplicationJsonSerializerContext.Default); // Use context for deserialization
                                        }
                                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) // Handle Dictionary
                                        {
                                            argValue = argNode.Deserialize(type, ApplicationJsonSerializerContext.Default); // Use context for deserialization
                                        }
                                        else // For complex custom types like AbilityScore
                                        {
                                            argValue = argNode.Deserialize(type, ApplicationJsonSerializerContext.Default);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errorMessage = $"There was an issue processing one of the tool's inputs ('{paramInfo.Name}'): {ex.Message}";
                                        _logger.LogError(ex, errorMessage);
                                        break; // Break from parameter loop
                                    }
                                }
                                else
                                {
                                    // Only if not conversationId, as conversationId is injected manually above
                                    if (!paramInfo.Name.Equals("conversationId", StringComparison.OrdinalIgnoreCase) && !paramInfo.IsOptional)
                                    {
                                        errorMessage = $"Tool call failed: Required parameter '{paramInfo.Name}' is missing or null.";
                                        _logger.LogError(errorMessage);
                                        break; // Break from parameter loop
                                    }
                                    // Handle missing optional parameters or conversationId which is manually added
                                    parameters.Add(Type.Missing); // Represents a missing optional parameter
                                    continue; // Skip to next parameter
                                }
                            }
                            else if (!paramInfo.IsOptional) // If parameter not found in argsObject and not optional
                            {
                                errorMessage = $"Tool call failed: Required parameter '{paramInfo.Name}' is missing from AI's arguments.";
                                _logger.LogError(errorMessage);
                                break; // Break from parameter loop
                            }
                            parameters.Add(argValue);
                        }

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            finalNarrative = errorMessage;
                            break; // Break from main tool loop
                        }

                        toolResult = method.Invoke(_characterStateManager, parameters.ToArray());
                        string resultLog = toolResult != null ? 
                            JsonSerializer.Serialize(toolResult, ApplicationJsonSerializerContext.Default.GetTypeInfo(toolResult.GetType())) 
                            : "null";
                        _logger.LogInformation($"Tool '{functionCall.Name}' executed successfully. Result: {resultLog}");

                        // Send tool response back to Gemini for follow-up narrative
                        // This part needs to ensure Gemini receives the tool's *actual* output.
                        // The `userQuery` for subsequent iterations should be the tool response.
                        userQuery = JsonSerializer.Serialize(
                            new { tool_response = toolResult },
                            ApplicationJsonSerializerContext.Default.DictionaryStringString
                        );
                        // Append tool response to currentContents for next iteration
                        currentContents.Add(new Content
                        {
                            Role = "function",
                            Parts = { 
                                new Part { 
                                    FunctionResponse = new FunctionResponse { 
                                        Name = functionCall.Name, Response = new JsonObject {
                                            ["result"] = JsonValue.Create(resultLog)
                                        } 
                                    } 
                                } 
                            }
                        });
                        // IMPORTANT: Continue the loop. Don't break here unless maxIterations reached or a terminal text response is given.
                        // The `prompt` for the next call needs to be the FunctionResponse.
                        continue; // Continue loop to send tool response back to Gemini

                    }
                    catch (TargetInvocationException tie) // Catch exceptions from the invoked method
                    {
                        errorMessage = $"Tool execution error for '{functionCall.Name}': {tie.InnerException?.Message ?? tie.Message}";
                        _logger.LogError(tie.InnerException ?? tie, errorMessage);
                        finalNarrative = errorMessage;
                        break;
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"An unexpected error occurred during tool execution setup for '{functionCall.Name}': {ex.Message}";
                        _logger.LogError(ex, errorMessage);
                        finalNarrative = errorMessage;
                        break;
                    }
                }
                else
                {
                    finalNarrative = "I received an unexpected response from the AI (neither text nor function call).";
                    _logger.LogWarning($"Gemini response part is neither text nor function call. Part details: {JsonSerializer.Serialize(firstPart)}");
                    break;
                }
            }

            if (iteration > maxIterations)
            {
                finalNarrative = "The game master seems to be in deep thought, unable to resolve the action. Could you rephrase your last request?";
                _logger.LogWarning("Tool call loop exceeded maximum iterations without a final narrative.");
            }
            else if (string.IsNullOrEmpty(finalNarrative))
            {
                // This covers cases where initial response was empty text, or no suggestions were provided.
                _logger.LogInformation("Attempting to generate final creative narrative and suggestions as fallback.");
                string creativePrompt = BuildCreativeNarrativePrompt(userQuery, history); // Pass original userQuery for creative prompt
                finalNarrative = await _geminiService.GenerateSimpleTextAsync(creativePrompt, 0.7f);
            }

            // After the loop, if finalNarrative doesn't contain a prompt for suggestions already,
            // we need to explicitly ask Gemini for a creative response with suggestions.
            if (!finalNarrative.Contains("SUGGESTIONS:", StringComparison.OrdinalIgnoreCase))
            {
                // Create a prompt to get a creative narrative with suggestions
                string creativePrompt = BuildCreativeNarrativePrompt(userQuery, history); // Reuse existing prompt builder
                string creativeResponse = await _geminiService.GenerateSimpleTextAsync(creativePrompt, 0.7f); // Higher temperature for creativity
            }

            return finalNarrative;
        }

        /// <summary>
        /// Parses the raw LLM string response into a RefereeResponseOutput object.
        /// It extracts the main narrative and any structured suggestions.
        /// </summary>
        private RefereeResponseOutput ParseLlmResponseForSuggestions(string llmRawResponse)
        {
            var output = new RefereeResponseOutput();
            if (string.IsNullOrEmpty(llmRawResponse))
            {
                output.Narrative = "I encountered an issue generating a response.";
                return output;
            }

            var lines = llmRawResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            StringBuilder narrativeBuilder = new StringBuilder();
            int suggestionLineIndex = -1;

            // Find the line that starts with "SUGGESTIONS:"
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith("SUGGESTIONS:", StringComparison.OrdinalIgnoreCase))
                {
                    suggestionLineIndex = i;
                    break;
                }
            }

            if (suggestionLineIndex != -1)
            {
                // Everything before SUGGESTIONS: is narrative
                for (int i = 0; i < suggestionLineIndex; i++)
                {
                    narrativeBuilder.AppendLine(lines[i]);
                }

                // Parse the JSON array from the suggestion line
                string jsonPart = lines[suggestionLineIndex].Substring(lines[suggestionLineIndex].IndexOf("SUGGESTIONS:", StringComparison.OrdinalIgnoreCase) + "SUGGESTIONS:".Length).Trim();
                try
                {
                    // Use ApplicationJsonSerializerContext for AOT compatibility
                    var context = ApplicationJsonSerializerContext.Default; // Assuming this context is available
                    var typeInfo = context.GetTypeInfo(typeof(List<string>));
                    if (typeInfo is null)
                    {
                        _logger.LogError("JSON TypeInfo for List<string> not found in ApplicationJsonSerializerContext for parsing suggestions.");
                    }
                    else
                    {
                        var suggestionsList = (List<string>?)JsonSerializer.Deserialize(jsonPart, typeInfo);
                        if (suggestionsList != null)
                        {
                            output.Suggestions.AddRange(suggestionsList);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, $"Failed to parse suggestions JSON from LLM response: {jsonPart}. Treating as part of narrative.");
                    // If JSON parsing fails, treat the whole suggestions line as part of the narrative
                    narrativeBuilder.AppendLine(lines[suggestionLineIndex]);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred during suggestions parsing: {ex.Message}. Treating as part of narrative.");
                    narrativeBuilder.AppendLine(lines[suggestionLineIndex]);
                }
            }
            else
            {
                // No SUGGESTIONS: line found, entire response is narrative
                foreach (var line in lines)
                {
                    narrativeBuilder.AppendLine(line);
                }
            }

            output.Narrative = narrativeBuilder.ToString().Trim();
            return output;
        }

        /// <summary>
        /// private method for refining an *existing AI response* based on fact-checks
        /// </summary>
        private async Task<string> RefineResponseForAccuracyAsync(string existingAiResponse, Dictionary<string, string> newFactChecks)
        {
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are the Referee, the Game Master for a tabletop RPG.");
            promptBuilder.AppendLine("Your tone is generally helpful and engaging. When correcting or adding factual information, be direct, precise, and highly accurate, minimizing extraneous narrative or conversational filler.");
            promptBuilder.AppendLine("CRITICAL: Be concise and to the point. Incorporate corrections smoothly. Provide only the revised response, without preambles or explanations about the revision process.");
            promptBuilder.Append("When the player asks to roll dice (e.g., 'Roll 4d6', 'Roll a d20'), you MUST call the RollDice tool with the appropriate numberOfDice and diceType. Do not narrate the rolling process yourself; wait for the tool's result, then incorporate it into your response.");
            promptBuilder.AppendLine("You previously generated the following response:");
            promptBuilder.AppendLine($"\"\"\"{existingAiResponse}\"\"\"");

            promptBuilder.AppendLine("\nUpon review, the following factual points related to your response need correction or additional grounding from game experts:");
            foreach (KeyValuePair<string, string> kvp in newFactChecks)
            {
                promptBuilder.AppendLine($"- For the query \"{kvp.Key ?? "null"}\", the expert provided: {kvp.Value ?? "null"}");
            }
            promptBuilder.AppendLine("\nPlease revise your previous response to incorporate these new factual details, correct any inaccuracies, and ensure it remains consistent with the game's rules. Maintain your helpful, descriptive, and engaging GM persona.");

            return await _geminiService.GenerateSimpleTextAsync(promptBuilder.ToString(), 0.2f);
        }


        /// <summary>
        /// Constructs a prompt for generating a creative narrative response when no specific facts are required.
        /// </summary>
        private string BuildCreativeNarrativePrompt(string originalQuery, List<ChatMessage> history)
        {
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are the Referee, the Game Master for a tabletop RPG.");
            promptBuilder.AppendLine("Your tone is helpful, descriptive, and engaging. Focus on immersing the player in the game world.");
            promptBuilder.AppendLine("CRITICAL: While descriptive, be concise and to the point where appropriate. Avoid excessive elaboration or tangential details unless specifically requested. Focus on essential narrative elements and atmosphere.");
            promptBuilder.Append("When the player asks to roll dice (e.g., 'Roll 4d6', 'Roll a d20'), you MUST call the RollDice tool with the appropriate numberOfDice and diceType. Do not narrate the rolling process yourself; wait for the tool's result, then incorporate it into your response.");
            promptBuilder.AppendLine("A player has asked a purely narrative or descriptive question, or a question for which no specific factual data was found. Your task is to respond creatively and descriptively, enhancing the game world and atmosphere, based on the player's query and the conversation history.");

            if (history.Any())
            {
                promptBuilder.AppendLine("\n--- BEGIN RECENT CONVERSATION HISTORY ---");
                foreach (ChatMessage message in history)
                {
                    promptBuilder.AppendLine($"{message.Author}: {message.Content}");
                }
                promptBuilder.AppendLine("--- END RECENT CONVERSATION HISTORY ---\n");
            }

            promptBuilder.AppendLine($"Based on the history, provide a creative, narrative response to the player's latest query: \"{originalQuery}\"");
            promptBuilder.AppendLine("\nAfter your main response, include a JSON array of up to 3 relevant suggested next actions or questions for the player. The JSON array should be labeled 'SUGGESTIONS:' on a new line after your main response. If no suggestions are appropriate, provide an empty array `[]`.");
            promptBuilder.AppendLine("Example Output Format:");
            promptBuilder.AppendLine("{Your narrative response here.}");
            promptBuilder.AppendLine("SUGGESTIONS: [\"Examine the door\", \"Check for traps\", \"Go back upstairs\"]");

            return promptBuilder.ToString();
        }
    }

    public class FactCheckRequest
    {
        [JsonPropertyName("expertName")]
        public string ExpertName { get; set; } = string.Empty; // Initialize to prevent null warnings

        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty; // Initialize to prevent null warnings

        public static async Task<List<FactCheckRequest>> GetFactCheckRequestsAsync(ILogger<RefereeService> logger, GeminiService geminiService, ExpertRegistryService expertRegistryService, string userQuery)
        {
            string factCheckingPrompt = BuildFactCheckPrompt(expertRegistryService, userQuery);
            // Handle potential null return from GetStructuredJsonAsync
            return await geminiService.GetStructuredJsonAsync<List<FactCheckRequest>>(factCheckingPrompt) ?? new List<FactCheckRequest>();
        }

        public static async Task<List<FactCheckRequest>> GetFactCheckResponseAsync(ILogger<RefereeService> logger, GeminiService geminiService, ExpertRegistryService expertRegistryService, string narrativeResponse)
        {
            string factCheckingPrompt = BuildResponseFactCheckPrompt(expertRegistryService, narrativeResponse);
            // Handle potential null return from GetStructuredJsonAsync
            return await geminiService.GetStructuredJsonAsync<List<FactCheckRequest>>(factCheckingPrompt) ?? new List<FactCheckRequest>();
        }

        private static string BuildFactCheckPrompt(ExpertRegistryService expertRegistryService, string userQuery)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Given the player's original query: \"{userQuery}\"");
            promptBuilder.AppendLine($"Identify any specific facts, rules, or data points mentioned or implied in the query that should be validated by the game's experts.");
            promptBuilder.AppendLine($"**Crucially, if the user expresses interest in or suggests a game concept, class, or rule (e.g., 'knight', 'casting spells', 'leveling up') that is explicitly covered by one of the available experts, you should formulate a query to that expert to provide factual information about that concept within the game system.**");
            promptBuilder.AppendLine($"**IMPORTANT: If the user explicitly declares a success or failure for a simple skill check (e.g., 'Isadora successfully picks the lock', 'I failed to persuade the guard'), prioritize narrating that outcome unless a specific game rule or context strictly prevents it. Only ask for a roll if the user requests it or if the rules absolutely mandate a roll for the stated action.**");
            promptBuilder.AppendLine($"For each identified need, suggest which expert (from the available list below) could verify it and formulate a concise query for that expert.");
            promptBuilder.AppendLine($"If the user's query is purely narrative, descriptive, or asks for creative improvisation (e.g., 'describe the tavern', 'what happens next?'), then include a single entry for the 'Narrative Expert' with the exact query '[NARRATIVE_ONLY_REQUEST]'.");
            promptBuilder.AppendLine($"If the query requires no expert interaction (neither factual nor narrative), respond with an empty JSON array `[]`.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Available Experts (Name: Description):");
            foreach (var expert in expertRegistryService.GetAllExperts())
            {
                promptBuilder.AppendLine($"- \"{expert.Name}\": \"{expert.Description}\"");
            }
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Output in a JSON array format like these examples:");
            promptBuilder.AppendLine("[");
            promptBuilder.AppendLine("  { \"expertName\": \"Character Expert\", \"query\": \"What are the starting stats for a new character?\" }");
            promptBuilder.AppendLine("]");
            promptBuilder.AppendLine("OR if purely narrative:");
            promptBuilder.AppendLine("[");
            promptBuilder.AppendLine("  { \"expertName\": \"Narrative Expert\", \"query\": \"[NARRATIVE_ONLY_REQUEST]\" }");
            promptBuilder.AppendLine("]");
            promptBuilder.AppendLine("OR if no expert interaction needed:");
            promptBuilder.AppendLine("[]");
            return promptBuilder.ToString();
        }

        private static string BuildResponseFactCheckPrompt(ExpertRegistryService expertRegistryService, string narrativeResponse)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Given this narrative response to the user: \"{narrativeResponse}\"");
            promptBuilder.AppendLine("You are an expert fact-checker for a tabletop RPG. Your task is to analyze the following narrative response and identify any specific facts, rules, or data points that should be validated by the game's experts.");
            promptBuilder.AppendLine($"**Crucially, if the user expresses interest in or suggests a game concept, class, or rule (e.g., 'knight', 'casting spells', 'leveling up') that is explicitly covered by one of the available experts, you should formulate a query to that expert to provide factual information about that concept within the game system.**");
            promptBuilder.AppendLine($"**IMPORTANT: If the user explicitly declares a success or failure for a simple skill check (e.g., 'Isadora successfully picks the lock', 'I failed to persuade the guard'), prioritize narrating that outcome unless a specific game rule or context strictly prevents it. Only ask for a roll if the user requests it or if the rules absolutely mandate a roll for the stated action.**");
            promptBuilder.AppendLine("Your response should be a JSON array of objects, each containing the expert name and a query to validate the fact.");
            promptBuilder.AppendLine($"If the response requires no factual validation from experts, respond with an empty JSON array `[]`."); // Adjusted wording
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"Available Experts (Name: Description):");
            foreach (var expert in expertRegistryService.GetAllExperts())
            {
                promptBuilder.AppendLine($"- \"{expert.Name}\": \"{expert.Description}\"");
            }
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Output in a JSON array format like these examples:");
            promptBuilder.AppendLine("[");
            promptBuilder.AppendLine("  { \"expertName\": \"Character Expert\", \"query\": \"What are the starting stats for a new character?\" }");
            promptBuilder.AppendLine("]");
            // Removed purely narrative and no interaction needed examples for response fact-check,
            // as this prompt is specifically for factual validation of an existing AI response.
            promptBuilder.AppendLine("OR if no expert interaction needed:");
            promptBuilder.AppendLine("[]");
            return promptBuilder.ToString();
        }
    }
}