using GenAIExpertEngineAPI.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

// A simple class to represent one turn in the chat
public class ChatMessage
{
    public string? Author { get; set; } // "user" or "ai"
    public string? Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// This service will store conversation histories in memory
public class ConversationHistoryService
{
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();
    private readonly GeminiService _geminiService; // Injected to use for summarization
    private readonly ILogger<ConversationHistoryService> _logger; // Injected for logging

    // Configuration for summarization behavior
    private const int MAX_RAW_MESSAGES = 5; // Max number of individual messages to keep before attempting to summarize older ones
    private const int MIN_MESSAGES_TO_SUMMARIZE_BATCH = 5; // Minimum number of messages in a batch to consider summarizing

    public ConversationHistoryService(GeminiService geminiService, ILogger<ConversationHistoryService> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Adds a message to the conversation history and triggers summarization if needed.
    /// </summary>
    /// <param name="conversationId">The ID of the conversation.</param>
    /// <param name="message">The chat message to add.</param>
    public void AddMessage(string conversationId, ChatMessage message)
    {
        lock(_conversations.AddOrUpdate(
                conversationId,
                new List<ChatMessage> { message },
                (key, existingList) =>
                {
                    existingList.Add(message);

                    // Trigger summarization if history is too long and not already processing
                    if (existingList.Count > MAX_RAW_MESSAGES && existingList.Last().Author != "summarizing_in_progress") // Prevent re-summarizing a summary marker
                    {
                        // Fire and forget, or handle within a dedicated background task
                        _ = SummarizeAndCompactHistoryAsync(conversationId, existingList);
                    }

                    return existingList;
                }
            ));
    }

    /// <summary>
    /// Retrieves the current conversation history for a given ID.
    /// </summary>
    /// <param name="conversationId">The ID of the conversation.</param>
    /// <returns>A list of chat messages, potentially including summaries.</returns>
    public List<ChatMessage> GetHistory(string conversationId)
    {
        // Retrieve the history. It will contain a mix of raw messages and summary messages.
        return _conversations.TryGetValue(conversationId, out List<ChatMessage>? history) ?
               history.OrderBy(m => m.Timestamp).ToList() : // Order by timestamp to maintain chronological order
               new List<ChatMessage>();
    }

    private async Task SummarizeAndCompactHistoryAsync(string conversationId, List<ChatMessage> history)
    {
        // Simple approach: Take the oldest MESSAGES_TO_SUMMARIZE messages that are not already summaries
        var messagesToSummarize = history
            .Where(m => m.Author != "ai_summary")
            .ToList();

        if (!messagesToSummarize.Any())
        {
            return; // Nothing to summarize
        }

        try
        {
            var summaryPrompt = BuildSummaryPrompt(messagesToSummarize);
            _logger.LogInformation($"Summarizing history for conversation {conversationId}");

            // Use a lower temperature for factual summaries
            string summaryText = await _geminiService.GenerateSimpleTextAsync(summaryPrompt, temperature: 0.2f);

            if (!string.IsNullOrWhiteSpace(summaryText))
            {
                // Update history in a thread-safe way
                lock(_conversations.AddOrUpdate(
                    conversationId,
                    new List<ChatMessage>(), // Should not happen with AddOrUpdate
                    (key, existingList) =>
                    {
                        // Remove the messages that were summarized
                        foreach (var msg in messagesToSummarize)
                        {
                            existingList.Remove(msg);
                        }
                        // Remove the temporary marker
                        existingList.RemoveAll(m => m.Author == "summarizing_in_progress");

                        // Add the new summary message at the beginning of the raw messages
                        existingList.Insert(0, new ChatMessage { Author = "ai_summary", Content = $"Conversation Summary: {summaryText}" });

                        // Optional: Further prune if still too long after adding summary (e.g., beyond MAX_RAW_MESSAGES)
                        // This ensures the total raw messages + summary doesn't grow indefinitely
                        while (existingList.Count(m => m.Author != "ai_summary") > MAX_RAW_MESSAGES)
                        {
                            var oldestRaw = existingList.FirstOrDefault(m => m.Author != "ai_summary");
                            if (oldestRaw != null) existingList.Remove(oldestRaw);
                            else break; // Should not happen if logic is correct
                        }

                        return existingList;
                    }
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to summarize history for conversation {conversationId}");
            // Remove the temporary marker even on error
            history.RemoveAll(m => m.Author == "summarizing_in_progress");
        }
    }

    /// <summary>
    /// Builds the prompt for the LLM to summarize a given list of chat messages.
    /// </summary>
    private string BuildSummaryPrompt(List<ChatMessage> messages)
    {
        StringBuilder promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("You are an AI assistant specialized in summarizing conversations for a game master. Your task is to provide a concise, factual summary of the following chat history.");
        promptBuilder.AppendLine("Focus on key events, decisions, outcomes, and important pieces of information that have been established (e.g., character names, locations visited, actions taken, rules clarified).");
        promptBuilder.AppendLine("Do NOT include any new narrative, questions, or interpretations. Be purely factual and brief. Do not use quotes or recreate dialogue.");
        promptBuilder.AppendLine("Keep the summary as short as possible while retaining all critical information. If there's nothing significant to summarize from the provided messages, just return a very short phrase like 'No new key events.'");
        promptBuilder.AppendLine("\n--- CONVERSATION TO SUMMARIZE ---");
        foreach (var message in messages.OrderBy(m => m.Timestamp)) // Ensure messages are ordered for summarization
        {
            promptBuilder.AppendLine($"{message.Author}: {message.Content}");
        }
        promptBuilder.AppendLine("--- END CONVERSATION TO SUMMARIZE ---");
        promptBuilder.AppendLine("\nProvide the summary now:");
        return promptBuilder.ToString();
    }
}