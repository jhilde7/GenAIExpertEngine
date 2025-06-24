using System.Collections.Concurrent;
using System.ComponentModel;
using CSharpToJsonSchema;

namespace GenAIExpertEngineAPI.Services
{
    [GenerateJsonSchema(GoogleFunctionTool = true)]
    public interface ICharacterStateManager
    {
        [Description("Updates a character's ability scores.")]
        Task<string> UpdateAbilityScores(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("A dictionary of ability types (e.g., Strength, Dexterity) and their corresponding scores.")] Dictionary<AbilityType, AbilityScore> scores);

        [Description("Updates the character's specified ability score.")]
        Task<string> UpdateAbilityScore(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The type of ability score to update (e.g., Strength, Intelligence).")] AbilityType abilityType,
            [Description("The new integer value for the ability score.")] int value);

        [Description("Updates a character's class.")]
        Task<string> UpdateCharacterClass(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new class name for the character (e.g., Fighter, MagicUser).")] CharacterClass className);

        [Description("Updates a character's race.")]
        Task<string> UpdateRace(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new race name for the character (e.g., Human, Elf).")] CharacterRace raceName);

        [Description("Updates a character's name.")]
        Task<string> UpdateCharacterName(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new name for the character.")] string name);

        [Description("Updates a character's alignment.")]
        Task<string> UpdateCharacterAlignment(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new alignment for the character (e.g., Lawful, Neutral, Chaotic).")] Alignment alignment);

        [Description("Adds designated experience to the character.")]
        Task<string> UpdateChracterExperience( // Note: "Chracter" typo from original class. Consider correcting to "Character"
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The amount of experience to add to the character.")] int experience);

        [Description("Adds an additional language to the character's known languages. A random language is selected if not specified.")]
        Task<string> AddAdditionalLanguage(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The specific language to add (e.g., Elvish, Dwarvish). Defaults to Common if not specified.")] Languages languages = Languages.Common);

        [Description("Retrieves the character state as a summarized string.")]
        Task<string> GetCharacterStateAsString(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the entire character state as a JSON string.")]
        Task<string> GetCharacterStateAsJson(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the character's class name.")]
        Task<string> GetCharacterClassName(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the character's race name.")]
        Task<string?> GetCharacterRace(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the value of a specific ability score for a character.")]
        Task<string> GetAbilityScoreValue( // Returns as string to allow JSON output
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The type of ability score to retrieve (e.g., Strength, Wisdom).")] AbilityType abilityType);

        [Description("Retrieves all ability scores for a character.")]
        Task<string> GetAbilityScores(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the character's literacy state.")]
        Task<string> GetLiteracyState(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the number of additional languages a character can learn.")]
        Task<string> GetAdditionalLanguages(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the maximum number of retainers a character can have based on their charisma score.")]
        Task<string> GetMaxRetainers(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the current loyalty of retainers for a character based on their charisma score.")]
        Task<string> GetRetainerLoyalty(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves the NPC reaction bonus for a character based on their charisma score.")]
        Task<string> GetNpcReactionBonus(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Retrieves a character's chance to open stuck doors.")]
        Task<string> GetOpenDoorChance(
            [Description("The unique identifier for the conversation or character session.")] string conversationId);

        [Description("Rolls the specified number of dice with the given sides (e.g., 'd6', 'd20').")]
        Task<string> RollDice(
            [Description("The number of dice to roll.")] int numberOfDice,
            [Description("The type of dice to roll (e.g., 'd4', 'd6', 'd20').")] string diceType);
    }
    public class CharacterStateManager : ICharacterStateManager
    {
        
        private readonly GameSystemRegistryService _gameSystemRegistry;
        private readonly ConcurrentDictionary<string, CharacterState> _characterStates = new();
        private readonly ILogger<CharacterStateManager> _logger;

        public CharacterStateManager(ILogger<CharacterStateManager> logger, GameSystemRegistryService gameSystemRegistry)
        {
            _logger = logger;
            _gameSystemRegistry = gameSystemRegistry;
        }

        /// <summary>
        /// Retrieves the character state for a given conversation. Creates a new one if it doesn't exist.
        /// </summary>
        public CharacterState GetCharacterState(string conversationId)
        {
            return _characterStates.GetOrAdd(conversationId, _ => new CharacterState(_gameSystemRegistry));
        }

        /// <summary>
        /// Updates a character's ability scores.
        /// </summary>
        public Task<string> UpdateAbilityScores(string conversationId, Dictionary<AbilityType, AbilityScore> scores)
        {
            var state = GetCharacterState(conversationId);
            foreach (var kvp in scores)
            {
                state.SetAbilityScore(kvp.Key, kvp.Value.Value);
                _logger.LogInformation($"Updated {kvp.Key} score for {conversationId} to {kvp.Value.Value}.");
            }
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates the character's specified ability score.
        /// </summary>
        public Task<string> UpdateAbilityScore(string conversationId, AbilityType abilityType, int value)
        {
            var state = GetCharacterState(conversationId);
            state.SetAbilityScore(abilityType, value);
            _logger.LogInformation($"Updated {abilityType} score for {conversationId} to {value}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates a character's class.
        /// </summary>
        public Task<string> UpdateCharacterClass(string conversationId, CharacterClass className)
        {
            var state = GetCharacterState(conversationId);
            state.SetCharacterClass(className);
            _logger.LogInformation($"Updated character class for {conversationId} to {className}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates a character's race.
        /// </summary>
        public Task<string> UpdateRace(string conversationId, CharacterRace raceName)
        {
            var state = GetCharacterState(conversationId);
            state.SetCharacterRace(raceName);
            _logger.LogInformation($"Updated character race for {conversationId} to {raceName}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary
        /// Updates a character's name.
        /// </summary>
        public Task<string> UpdateCharacterName(string conversationId, string name)
        {
            var state = GetCharacterState(conversationId);
            state.SetCharacterName(name);
            _logger.LogInformation($"Updated character name for {conversationId} to {name}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates a character's alignment.
        /// </summary>
        public Task<string> UpdateCharacterAlignment(string conversationId, Alignment alignment)
        {
            var state = GetCharacterState(conversationId);
            state.SetAlignment(alignment);
            _logger.LogInformation($"Updated character alignment for {conversationId} to {alignment}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Adds designated experience to the character
        /// </summary>
        public Task<string> UpdateChracterExperience(string conversationId, int experience)
        {
            var state = GetCharacterState(conversationId);
            state.GainExperience(experience);
            _logger.LogInformation($"Updated character experience for {conversationId} to {experience}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Adds an additional language to the character's known languages based on available additional languages.
        /// You can pass a language or use the default which will trigger a random selection
        /// </summary>
        public Task<string> AddAdditionalLanguage(string conversationId, Languages languages = Languages.Common)
        {
            var state = GetCharacterState(conversationId);
            state.SetAdditionalLanguage(languages);
            _logger.LogInformation($"Updated known languages for {conversationId} to {string.Join(", ", languages)}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Retrieves the character state as a string.
        /// </summary>
        public Task<string> GetCharacterStateAsString(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            _logger.LogInformation($"Retrieved character state for {conversationId}");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Retrieves the character state as a JSON string.
        /// </summary>
        public Task<string> GetCharacterStateAsJson(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            string json = System.Text.Json.JsonSerializer.Serialize(state, ApplicationJsonSerializerContext.Default.CharacterState);
            _logger.LogInformation($"Serialized character state for {conversationId} to JSON.");
            return Task.FromResult(json);
        }

        /// <summary>
        /// Retrieves the character's class name as a string.
        /// </summary>
        public Task<string> GetCharacterClassName(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            var className = state.GetCharacterClass().ToString();
            _logger.LogInformation($"Retrieved character class: {className} for {conversationId}");
            return Task.FromResult(className);
        }

        /// <summary>
        /// Retrieves the character's race name as a string.
        /// </summary>
        public Task<string?> GetCharacterRace(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            var raceName = state.GetCharacterRace();
            _logger.LogInformation($"Retrieved character race: {raceName ?? "not set"} for {conversationId}");
            return Task.FromResult(raceName);
        }

        /// <summary>
        /// Retrieves the value of a specific ability score for a character.
        /// </summary>
        public Task<string> GetAbilityScoreValue(string conversationId, AbilityType abilityType)
        {
            var state = GetCharacterState(conversationId);
            int value = state.GetAbilityScore(abilityType);
            _logger.LogInformation($"Retrieved {abilityType} score: {value} for {conversationId}");
            return Task.FromResult(value.ToString());
        }

        /// <summary>
        /// Retrieves all values of ability score for a character.
        /// </summary>
        public Task<string> GetAbilityScores(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            var scores = state.GetAbilityScores();
            _logger.LogInformation($"Retrieved ability scores for {conversationId}: {string.Join(", ", scores.Select(kvp => $"{kvp.Key}: {kvp.Value.Value}"))}");
            //convert dictionary to string
            string scoresString = string.Join(", ", scores.Select(kvp => $"{kvp.Key}: {kvp.Value.Value}\n"));
            return Task.FromResult(scoresString);
        }

        /// <summary>
        /// Retrieves the characters literacy state.
        /// </summary>
        public Task<string> GetLiteracyState(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            string literacyState = state.GetLiteracy();
            _logger.LogInformation($"Retrieved literacy state for {conversationId}: {literacyState}");
            return Task.FromResult(literacyState);
        }

        /// <summary>
        /// Retrieves the number of additional languages a character can learn.
        /// </summary>
         
        public Task<string> GetAdditionalLanguages(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            int additionalLanguages = state.GetAdditionalLanguages();
            _logger.LogInformation($"Retrieved additional languages for {conversationId}: {additionalLanguages}");
            return Task.FromResult(additionalLanguages.ToString());
        }

        /// <summary>
        /// Retrieves the maximum number of retainers a character can have based on their charisma score.
        /// </summary>
        public Task<string> GetMaxRetainers(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            int maxRetainers = state.GetMaxRetainers();
            _logger.LogInformation($"Retrieved max retainers for {conversationId}: {maxRetainers}");
            return Task.FromResult(maxRetainers.ToString());
        }

        /// <summary>
        /// Retrieves the current loyalty of retainers for a character based on their charisma score.
        /// </summary>
        public Task<string> GetRetainerLoyalty(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            int currentRetainers = state.GetRetainerLoyalty();
            _logger.LogInformation($"Retrieved current retainers for {conversationId}: {currentRetainers}");
            return Task.FromResult(currentRetainers.ToString());
        }

        /// <summary>
        /// Retrieves the NPC reaction bonus for a character based on their charisma score.
        /// </summary>
        public Task<string> GetNpcReactionBonus(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            int npcReactionBonus = state.GetNpcReactionBonus();
            _logger.LogInformation($"Retrieved NPC reaction bonus for {conversationId}: {npcReactionBonus}");
            return Task.FromResult(npcReactionBonus.ToString());
        }

        /// <summary>
        /// Retrieves a characters chance to open stuck doors.
        /// </summary>
        public Task<string> GetOpenDoorChance(string conversationId)
        {
            var state = GetCharacterState(conversationId);
            string openDoorChance = state.GetOpenDoorChance();
            _logger.LogInformation($"Retrieved open door chance for {conversationId}: {openDoorChance}");
            return Task.FromResult(openDoorChance);
        }

        /// <summary>
        /// Roll the specified number of dice with the given sides.
        ///  </summary>
        public Task<string> RollDice(int numberOfDice, string diceType)
        {
            Enum.TryParse<DiceType>(diceType, true, out var parsedDiceType);
            int rollResult = Dice.RollDice(parsedDiceType, numberOfDice);
            _logger.LogInformation($"Rolled {numberOfDice}{diceType.ToString()}: {rollResult}");
            return Task.FromResult(rollResult.ToString());
        }
    }
}
