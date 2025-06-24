using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Types;
using System.Text;
using System.Text.Json;

// Assuming your project's namespace is GenAIExpertEngineAPI.Services
namespace GenAIExpertEngineAPI.Services
{
    public class GeminiService
    {
        private readonly VertexAI _vertexAI;
        private readonly ILogger<GeminiService> _logger;
        private readonly string? model; // Model name from configuration
        private readonly string defaultModel = "gemini-2.0-flash-001"; // Location from configuration
        public const string CRITICAL_INSTRUCTION =
            "CRITICAL INSTRUCTION: Your entire response MUST be ONLY the JSON object itself. " +
            "Do NOT include any explanatory text, greetings, or any characters before the opening { of the JSON or after the closing } of the JSON. " +
            "The output must be a single, raw, valid JSON object.\n";

        // This DI-friendly constructor sets up the main VertexAI client.
        // It assumes you have set up Google Cloud Application Default Credentials (ADC) on your machine.
        public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _logger = logger;
            try
            {
                string? projectId = configuration["GoogleCloud:ProjectId"];
                string? location = configuration["GoogleCloud:Location"];
                model = configuration["GoogleCloud:ModelName"];

                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(location))
                {
                    throw new InvalidOperationException("GoogleCloud:ProjectId and GoogleCloud:Location must be set in configuration.");
                }

                // Initialize the main VertexAI client.
                _vertexAI = new VertexAI(projectId, location);
                _logger.LogInformation($"GeminiService initialized for project {projectId} in {location}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize VertexAI client. Ensure Google Cloud credentials are configured.");
                throw;
            }
        }

        /// <summary>
        /// Generates content from Gemini, potentially using tools and/or RAG.
        /// This method returns the raw GenerateContentResponse to allow inspection of FunctionCalls.
        /// </summary>
        /// <param name="prompt">The main prompt text for the model (typically the user's current input or instructions).</param>
        /// <param name="history">Optional conversation history to provide context. Maps ChatMessage roles to Gemini roles.</param>
        /// <param name="tools">Optional list of tools (FunctionDeclarations) the model can use.</param>
        /// <param name="temperature">The creativity/randomness setting (0.0 to 1.0).</param>
        /// <returns>The raw GenerateContentResponse object from Gemini.</returns>
        public async Task<GenerateContentResponse> GenerateContentWithToolsAsync(
            string prompt,
            List<ChatMessage> history,
            IFunctionTool? characterFunctionTool = null,
            float temperature = 0.5f
        )
        {
            if (string.IsNullOrWhiteSpace(prompt) && (history == null || !history.Any()))
            {
                _logger.LogWarning("GenerateContentWithToolsAsync called with empty prompt and no history.");
                // Create a new instance of GenerateContentResponse with appropriate values
                return new GenerateContentResponse
                {
                    Candidates = new[] { new Candidate { Content = new Content("No content provided for generation.", "user") } }
                };
            }

            StringBuilder systemPrompt = new StringBuilder();
            systemPrompt.AppendLine("**CRITICAL TOOL USAGE INSTRUCTION:**");
            systemPrompt.AppendLine("1.  **Tool Priority:** You have access to specific game tools that perform actions or retrieve precise data. **You MUST use these tools whenever a user's request explicitly asks for an action or information that a tool can provide.** Do not narrate actions that can be performed by a tool; instead, call the tool, receive its result, and then integrate that result accurately into your narrative response.");
            systemPrompt.AppendLine("2.  **No Raw Output:** **DO NOT output raw `tool_code` blocks, `Result: X` text, or any direct mention of calling a tool (e.g., 'I will now call a tool') to the player.** This is internal. Your response to the player must always be natural language narrative.");
            systemPrompt.AppendLine("3.  **Dice Rolls:** When the user asks for a dice roll (e.g., 'Roll 4d6', 'Roll a d20', 'Generate [Ability] Score'), you **MUST** call the `RollDice` tool. Provide `numberOfDice` as an integer. **Crucially, provide `diceType` as a STRING literal that includes the 'D' prefix (e.g., 'D6', 'D20', 'D100'), matching the `DiceType` enum values. DO NOT use just the number for `diceType` (e.g., use 'D6', not 6).** For ability score generation call `RollDice(numberOfDice=1, diceType='D6')` 4 separate times, throwing out the lowest score, and use its output to provide the ability score to the player.");
            systemPrompt.AppendLine("4.  **Integrating Tool Results:** If the immediate preceding turn in the conversation history was a `function` role (meaning you just received a tool's result), your current task is to generate a natural language narrative response that *explains or uses the information provided in that tool response*. Do not ask more questions about the tool or its input if the tool call succeeded; simply explain the outcome to the player in character as the GM.");

            List<Content> contents = new List<Content>();

            // Add history, mapping ChatMessage roles to Gemini's expected roles (user/model)
            if (history != null && history.Any())
            {
                foreach (var message in history)
                {
                    string role = "";
                    if (message.Author?.ToLower() == "user")
                    {
                        role = "user";
                    }
                    else if (message.Author?.ToLower() == "ai" || message.Author?.ToLower() == "ai_summary" || message.Author?.ToLower() == "summarizing_in_progress")
                    {
                        role = "model";
                    }
                    else
                    {
                        // Fallback or error logging for unexpected roles
                        _logger.LogWarning($"Unexpected ChatMessage author role '{message.Author}'. Defaulting to 'model'.");
                        role = "model";
                    }
                    contents.Add(new Content(message.Content ?? string.Empty, role));
                }
            }

            // Add the current prompt as the latest user turn.
            contents.Add(new Content(prompt, "user"));

            GenerativeModel modelToUse = _vertexAI.CreateGenerativeModel(modelName: model ?? defaultModel, systemInstruction: systemPrompt.ToString());
            modelToUse.FunctionCallingBehaviour.AutoCallFunction = true;
            modelToUse.ToolConfig = new ToolConfig
            {
                FunctionCallingConfig = new FunctionCallingConfig
                {
                    Mode = FunctionCallingMode.ANY, // Automatically determine when to call functions
                }
            };
            if (characterFunctionTool != null)
            {
                modelToUse.FunctionTools.Clear(); // Clear any existing tools to avoid duplicates
                modelToUse.FunctionTools.Add(characterFunctionTool); // Add the provided character tools
            }


            GenerateContentRequest request = new GenerateContentRequest
            {
                Contents = contents,
                GenerationConfig = new GenerationConfig { Temperature = temperature }
            };

            try
            {
                _logger.LogInformation($"Generating content with tools for prompt (first 100 chars): '{prompt.Substring(0, Math.Min(prompt.Length, 100))}'");
                GenerateContentResponse response = await modelToUse.GenerateContentAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating content with tools/RAG from Gemini for prompt: '{prompt}'");
                return new GenerateContentResponse
                {
                    Candidates = new[] { new Candidate { Content = new Content("No content provided for generation.", "user") } }
                };
            }
        }

        /// <summary>
        /// Generates a response grounded on a specific RAG corpus.
        /// </summary>
        /// <param name="userQuery">The user's question/prompt.</param>
        /// <param name="corpusId">The full RAG corpus ID to use for grounding.</param>
        /// <returns>The grounded text response from Gemini.</returns>
        public async Task<string> GenerateGroundedResponseAsync(string userQuery, List<ChatMessage> history, string corpusId)
        {
            if (string.IsNullOrWhiteSpace(userQuery) || string.IsNullOrWhiteSpace(corpusId))
            {
                _logger.LogWarning("GenerateGroundedResponseAsync called with empty query or corpusId.");
                return "An internal error occurred: a query and corpus ID are required.";
            }

            try
            {
                _logger.LogInformation($"Generating grounded response for query '{userQuery}' using corpus '{corpusId}'");

                GenerativeModel modelForRag = _vertexAI.CreateGenerativeModel(
                    modelName: model,
                    corpusIdForRag: corpusId
                );

                StringBuilder promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("You are an expert AI for tabletop RPG knowledge, grounded solely in the provided JSON corpus. ");
                promptBuilder.AppendLine("Your task is to accurately retrieve, synthesize, and present information from the chunks.");
                promptBuilder.AppendLine("Instructions:");
                promptBuilder.AppendLine("Only use provided data. No external knowledge, no fabrication.");
                promptBuilder.AppendLine("Understand chunk structure: regular_text, character_level_progression (with class_name, level, xp_required, hit_dice, thac0, saving_throws, spell_slots), and monster_entry (with stat_block, special_abilities, description, etc.).");
                promptBuilder.AppendLine("Extract precisely: All numbers, names, and rules must match the source.");
                promptBuilder.AppendLine("Gracefully decline: If information isn't in the corpus, state \"Information not available in the provided data.\"");
                promptBuilder.AppendLine("Be clear and direct. Exclude all internal metadata (page numbers, timestamps, chunk IDs).");

                // Include conversation history for better context on follow-up questions.
                if (history != null && history.Any())
                {
                    promptBuilder.AppendLine("--- BEGIN RECENT CONVERSATION HISTORY ---");
                    foreach (var message in history)
                    {
                        promptBuilder.AppendLine($"{message.Author}: {message.Content}");
                    }
                    promptBuilder.AppendLine("--- END RECENT CONVERSATION HISTORY ---\n");
                }
                promptBuilder.AppendLine($"User Query: \"{userQuery}\"");
                // The instructional text is now separate from the user's direct query.
                string fullPrompt = promptBuilder.ToString();

                // Create a request with two parts: the instructional prompt and the clean user query.
                // This allows the RAG system to use the user's query for retrieval.
                GenerateContentRequest request = new GenerateContentRequest
                {
                    Contents = new List<Content>
                    {
                        new Content(fullPrompt , "user")
                    }
                };

                GenerateContentResponse response = await modelForRag.GenerateContentAsync(request);

                return response?.Text ?? "The model did not provide a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating grounded response from Gemini for query: '{userQuery}'");
                return $"Error communicating with Gemini: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates content and forces the output to be a single, raw JSON object.
        /// </summary>
        public async Task<string?> GenerateTextAsync(string promptText, float temperature = 0.5f)
        {
            if (string.IsNullOrWhiteSpace(promptText))
            {
                _logger.LogWarning("GenerateTextAsync called with empty or null prompt.");
                return null;
            }

            string fullPrompt = CRITICAL_INSTRUCTION + "\n" + promptText;

            try
            {
                // 1. Create a standard model instance without RAG.
                GenerativeModel standardModel = _vertexAI.CreateGenerativeModel(modelName: model ?? defaultModel);

                GenerateContentRequest request = new GenerateContentRequest
                {
                    Contents = new List<Content> { new Content(fullPrompt, "user") },
                    GenerationConfig = new GenerationConfig { Temperature = temperature }
                };

                // 2. Call the API.
                GenerateContentResponse response = await standardModel.GenerateContentAsync(request);
                string? responseText = response?.Text;

                if (!string.IsNullOrEmpty(responseText))
                {
                    responseText = responseText.Replace("```json", string.Empty).Replace("```", string.Empty).Trim();
                }

                return responseText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JSON text from Gemini.");
                return null;
            }
        }

        /// <summary>
        /// Generates a simple text response, ideal for classification or creative narration.
        /// </summary>
        /// <param name="prompt">The full prompt for the model.</param>
        /// <param name="temperature">The creativity/randomness setting. Defaults to 0.2f for consistent output.</param>
        /// <returns>The generated text content from Gemini.</returns>
        public async Task<string> GenerateSimpleTextAsync(string prompt, float temperature = 0.2f)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger.LogWarning("GenerateSimpleTextAsync called with empty or null prompt.");
                return string.Empty;
            }

            try
            {
                var standardModel = _vertexAI.CreateGenerativeModel(modelName: model ?? defaultModel);

                var request = new GenerateContentRequest
                {
                    Contents = new List<Content> { new Content(prompt, "user") },
                    GenerationConfig = new GenerationConfig
                    {
                        Temperature = temperature
                    }
                };

                var response = await standardModel.GenerateContentAsync(request);
                return response?.Text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating simple text from Gemini.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Sends a prompt to Gemini and attempts to parse the raw JSON text response into a specified C# type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON response into.</typeparam>
        /// <param name="prompt">The prompt to send to the Gemini model, requesting JSON output.</param>
        /// <param name="temperature">The creativity temperature for the model (0.0 to 1.0).</param>
        /// <returns>An instance of type T deserialized from the JSON response, or default(T) if parsing fails.</returns>
        public async Task<T?> GetStructuredJsonAsync<T>(string prompt, float temperature = 0.2f)
        {
            var jsonString = await GenerateTextAsync(prompt, temperature);

            if (string.IsNullOrEmpty(jsonString))
            {
                Console.WriteLine("Warning: Gemini returned an empty or null JSON string for structured output.");
                return default;
            }

            try
            {
                // Use source-generated context for AOT safety
                var context = ApplicationJsonSerializerContext.Default;
                var typeInfo = context.GetTypeInfo(typeof(T));
                if (typeInfo is null)
                {
                    Console.WriteLine($"JSON TypeInfo for {typeof(T).FullName} not found in ApplicationJsonSerializerContext.");
                    return default;
                }
                return (T?)JsonSerializer.Deserialize(jsonString, typeInfo);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                Console.WriteLine($"Raw JSON: {jsonString}");
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during JSON deserialization: {ex.Message}");
                return default;
            }
        }
    }
}