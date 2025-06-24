using Microsoft.Extensions.Options;

namespace GenAIExpertEngineAPI.Services
{
    public class ExpertRegistryService
    {
        public IReadOnlyDictionary<string, ExpertDefinition> Experts { get; }

        // The constructor now takes IOptions, which is provided by the DI container
        public ExpertRegistryService(IOptions<List<ExpertDefinition>> expertOptions)
        {
            // The .Value property gives us the List<ExpertDefinition> that was loaded from experts.json
            Experts = expertOptions.Value.ToDictionary(e => e.Name, e => e);
        }

        public ExpertDefinition? GetExpertByIntent(string intentName)
        {
            // Find the first expert that has a matching IntentName
            return Experts.Values.FirstOrDefault(e => e.IntentName == intentName);
        }

        public List<ExpertDefinition> GetAllExperts()
        {
            // Return all experts as a list
            return Experts.Values.ToList();
        }
    }

    public enum ExpertType
    {
        AI_RAG,
        LOCAL_DATA,
        NARRATIVE_ONLY
    }

    public class ExpertDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string IntentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ExpertType Type { get; set; }
        public string? CorpusId { get; set; }
        public string? DataSourceKey { get; set; } // Keep for future use
    }
}
