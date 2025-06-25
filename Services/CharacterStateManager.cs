using System.Collections.Concurrent;
using System.ComponentModel;
using CSharpToJsonSchema;
using GenAIExpertEngineAPI.Classes;

namespace GenAIExpertEngineAPI.Services
{
    [GenerateJsonSchema(GoogleFunctionTool = true, Strict = false)]
    public interface ICharacterStateManager
    {
        //-- ENUM SECTION --
        [Description("Retrieve a list of all classes available in the game system.")]
        Task<string> GetAllCharacterClassesAsync( CancellationToken cancellationToken = default);

        [Description("Retrieve a list of all races available in the game system.")]
        Task<string> GetAllRacesAsync( CancellationToken cancellationToken = default);

        [Description("Retrieve a list of all ability scores available in the game system.")]
        Task<string> GetAllAbilityTypesAsync( CancellationToken cancellationToken = default);

        [Description("Retrieve a list of all languages available in the game system.")]
        Task<string> GetAllLanguagesAsync( CancellationToken cancellationToken = default);

        [Description("Retrieve a list of all coin types available in the game system.")]
        Task<string> GetAllCoinTypesAsync( CancellationToken cancellationToken = default);

        [Description("Retrieves a list of all magic types available in the game system")]
        Task<string> GetAllMagicTypesAsync(CancellationToken cancellationToken = default);

        [Description("Retrieves a list of all spell types available in the game system")]
        Task<string> GetAllSpellTypesAsync(CancellationToken cancellationToken = default);
        //--END ENUM SECTION --

        [Description("Updates a character's ability scores.")]
        Task<string> UpdateAbilityScoresAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("A dictionary of ability types (e.g., Strength, Dexterity) and their corresponding scores.")] Dictionary<string, int> scores, CancellationToken cancellationToken = default);

        [Description("Updates the character's specified ability score.")]
        Task<string> UpdateAbilityScoreAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The type of ability score to update (e.g., Strength, Intelligence).")] string abilityType,
            [Description("The new integer value for the ability score.")] int value, CancellationToken cancellationToken = default);

        [Description("Updates a character's class.")]
        Task<string> UpdateCharacterClassAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new class name for the character (e.g., Fighter, MagicUser).")] string className, CancellationToken cancellationToken = default);

        [Description("Updates a character's race.")]
        Task<string> UpdateRaceAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new race name for the character (e.g., Human, Elf).")] string raceName, CancellationToken cancellationToken = default);

        [Description("Updates a character's name.")]
        Task<string> UpdateCharacterNameAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new name for the character.")] string name, CancellationToken cancellationToken = default);

        [Description("Updates a character's alignment.")]
        Task<string> UpdateCharacterAlignmentAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The new alignment for the character (e.g., Lawful, Neutral, Chaotic).")] string alignment, CancellationToken cancellationToken = default);

        [Description("Adds designated experience to the character.")]
        Task<string> UpdateCharacterExperienceAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The amount of experience to add to the character.")] int experience, CancellationToken cancellationToken = default);

        [Description("Adds an additional language to the character's known languages. A random language is selected if not specified.")]
        Task<string> AddAdditionalLanguageAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The specific language to add (e.g., Elvish, Dwarvish). Defaults to Common if not specified.")] string languages = "Common", CancellationToken cancellationToken = default);

        [Description("Retrieves the character state as a summarized string.")]
        Task<string> GetCharacterStateAsStringAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the entire character state as a JSON string.")]
        Task<string> GetCharacterStateAsJsonAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the character's class name.")]
        Task<string> GetCharacterClassNameAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the character's race name.")]
        Task<string?> GetCharacterRaceAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the value of a specific ability score for a character.")]
        Task<string> GetAbilityScoreValueAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId,
            [Description("The type of ability score to retrieve (e.g., Strength, Wisdom).")] string abilityType, CancellationToken cancellationToken = default);

        [Description("Retrieves all ability scores for a character.")]
        Task<string> GetAbilityScoresAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the character's literacy state.")]
        Task<string> GetLiteracyStateAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the number of additional languages a character can learn.")]
        Task<string> GetAdditionalLanguagesAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the maximum number of retainers a character can have based on their charisma score.")]
        Task<string> GetMaxRetainersAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the current loyalty of retainers for a character based on their charisma score.")]
        Task<string> GetRetainerLoyaltyAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves the NPC reaction bonus for a character based on their charisma score.")]
        Task<string> GetNpcReactionBonusAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Retrieves a character's chance to open stuck doors.")]
        Task<string> GetOpenDoorChanceAsync(
            [Description("The unique identifier for the conversation or character session.")] string conversationId, CancellationToken cancellationToken = default);

        [Description("Rolls the specified number of dice with the given sides (e.g., 'd6', 'd20').")]
        Task<string> RollDiceAsync(
            [Description("The number of dice to roll.")] int numberOfDice,
            [Description("The type of dice to roll (e.g., 'd4', 'd6', 'd20').")] string diceType, CancellationToken cancellationToken = default);
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
        public CharacterState GetCharacterState(string conversationId, CancellationToken cancellationToken = default)
        {
            return _characterStates.GetOrAdd(conversationId, _ => new CharacterState(_gameSystemRegistry));
        }

        /// <summary>
        /// Retrievs all classes available in the game system.
        /// </summary>
        public Task<string> GetAllCharacterClassesAsync(CancellationToken cancellationToken = default)
        {
            var classes = _gameSystemRegistry.GetAllCharacterClasses();
            _logger.LogInformation($"Retrieved all classes: {string.Join(", ", classes)}");
            return Task.FromResult(string.Join(", ", classes));
        }

        /// <summary>
        /// Retrieve a list of all races available in the game system.
        /// </summary>
        public Task<string> GetAllRacesAsync(CancellationToken cancellationToken = default)
        {
            var races = _gameSystemRegistry.GetAllRaces();
            _logger.LogInformation($"Retrieved all races: {string.Join(", ", races)}");
            return Task.FromResult(string.Join(", ", races));
        }

        /// <summary>
        /// Retrieve a list of all ability scores available in the game system.
        /// </summary>
        public Task<string> GetAllAbilityTypesAsync(CancellationToken cancellationToken = default)
        {
            var abilityScores = _gameSystemRegistry.GetAllAbilityTypes();
            _logger.LogInformation($"Retrieved all ability scores: {string.Join(", ", abilityScores)}");
            return Task.FromResult(string.Join(", ", abilityScores));
        }

        /// <summary>
        /// Retrieve a list of all additional languages available in the game system.
        /// </summary>
        public Task<string> GetAllLanguagesAsync(CancellationToken cancellationToken = default)
        {
            var additionalLanguages = _gameSystemRegistry.GetAllLanguages();
            _logger.LogInformation($"Retrieved all additional languages: {string.Join(", ", additionalLanguages)}");
            return Task.FromResult(string.Join(", ", additionalLanguages));
        }

        /// <summary>
        /// [Description("Retrieves a list of all magic types available in the game system")]
        /// </summary>
        public Task<string> GetAllMagicTypesAsync(CancellationToken cancellationToken = default)
        {
            var magicTypes = _gameSystemRegistry.GetAllMagicTypes();
            _logger.LogInformation($"Retrieved all magic types: {string.Join(", ", magicTypes)}");
            return Task.FromResult(string.Join(", ", magicTypes));
        }

        /// <summary>
        /// [Description("Retrieves a list of all spell types available in the game system")]
        /// </summary>
        public Task<string> GetAllSpellTypesAsync(CancellationToken cancellationToken = default)
        {
            var spellTypes = _gameSystemRegistry.GetAllSpellTypes();
            _logger.LogInformation($"Retrieved all spell types: {string.Join(", ", spellTypes)}");
            return Task.FromResult(string.Join(", ", spellTypes));
        }

        /// <summary>
        /// Retrieve a list of all coin types available in the game system.
        /// </summary>
        public Task<string> GetAllCoinTypesAsync(CancellationToken cancellationToken = default)
        {
            var coinTypes = _gameSystemRegistry.GetAllCoinTypes();
            _logger.LogInformation($"Retrieved all coin types: {string.Join(", ", coinTypes)}");
            return Task.FromResult(string.Join(", ", coinTypes));
        }


        /// <summary>
        /// Updates a character's ability scores.
        /// </summary>
        public Task<string> UpdateAbilityScoresAsync(string conversationId, Dictionary<string, int> scores, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            foreach (var kvp in scores)
            {
                state.SetAbilityScore(kvp.Key, kvp.Value);
                _logger.LogInformation($"Updated {kvp.Key} score for {conversationId} to {kvp.Value}.");
            }
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates the character's specified ability score.
        /// </summary>
        public Task<string> UpdateAbilityScoreAsync(string conversationId, string abilityType, int value, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            state.SetAbilityScore(abilityType, value);
            _logger.LogInformation($"Updated {abilityType} score for {conversationId} to {value}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates a character's class.
        /// </summary>
        public Task<string> UpdateCharacterClassAsync(string conversationId, string className, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            state.SetCharacterClass(className);
            _logger.LogInformation($"Updated character class for {conversationId} to {className}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates a character's race.
        /// </summary>
        public Task<string> UpdateRaceAsync(string conversationId, string raceName, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            state.SetCharacterRace(raceName);
            _logger.LogInformation($"Updated character race for {conversationId} to {raceName}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary
        /// Updates a character's name.
        /// </summary>
        public Task<string> UpdateCharacterNameAsync(string conversationId, string name, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            state.SetCharacterName(name);
            _logger.LogInformation($"Updated character name for {conversationId} to {name}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Updates a character's alignment.
        /// </summary>
        public Task<string> UpdateCharacterAlignmentAsync(string conversationId, string alignment, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            state.SetAlignment(alignment);
            _logger.LogInformation($"Updated character alignment for {conversationId} to {alignment}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Adds designated experience to the character
        /// </summary>
        public Task<string> UpdateCharacterExperienceAsync(string conversationId, int experience, CancellationToken cancellationToken = default)
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
        public Task<string> AddAdditionalLanguageAsync(string conversationId, string languages = "Common", CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            state.SetAdditionalLanguage(languages);
            _logger.LogInformation($"Updated known languages for {conversationId} to {string.Join(", ", languages)}.");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Retrieves the character state as a string.
        /// </summary>
        public Task<string> GetCharacterStateAsStringAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            _logger.LogInformation($"Retrieved character state for {conversationId}");
            return Task.FromResult(state.ToString());
        }

        /// <summary>
        /// Retrieves the character state as a JSON string.
        /// </summary>
        public Task<string> GetCharacterStateAsJsonAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            string json = System.Text.Json.JsonSerializer.Serialize(state, ApplicationJsonSerializerContext.Default.CharacterState);
            _logger.LogInformation($"Serialized character state for {conversationId} to JSON.");
            return Task.FromResult(json);
        }

        /// <summary>
        /// Retrieves the character's class name as a string.
        /// </summary>
        public Task<string> GetCharacterClassNameAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            var className = state.GetCharacterClass().ToString();
            _logger.LogInformation($"Retrieved character class: {className} for {conversationId}");
            return Task.FromResult(className);
        }

        /// <summary>
        /// Retrieves the character's race name as a string.
        /// </summary>
        public Task<string?> GetCharacterRaceAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            var raceName = state.GetCharacterRace();
            _logger.LogInformation($"Retrieved character race: {raceName ?? "not set"} for {conversationId}");
            return Task.FromResult(raceName);
        }

        /// <summary>
        /// Retrieves the value of a specific ability score for a character.
        /// </summary>
        public Task<string> GetAbilityScoreValueAsync(string conversationId, string abilityType, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            int value = state.GetAbilityScoreValue(abilityType);
            _logger.LogInformation($"Retrieved {abilityType} score: {value} for {conversationId}");
            return Task.FromResult(value.ToString());
        }

        /// <summary>
        /// Retrieves all values of ability score for a character.
        /// </summary>
        public Task<string> GetAbilityScoresAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            var scores = state.GetAbilityScores();
            scores.ForEach(kvp => _logger.LogInformation($"Retrieved {kvp.Name} score: {kvp.Value} ({kvp.Modifier}) for {conversationId}"));
            return Task.FromResult(string.Join(", ", scores.Select(kvp => $"{kvp.Name}: {kvp.Value} ({kvp.Modifier})")));
        }

        /// <summary>
        /// Retrieves the characters literacy state.
        /// </summary>
        public Task<string> GetLiteracyStateAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            string literacyState = state.GetLiteracy();
            _logger.LogInformation($"Retrieved literacy state for {conversationId}: {literacyState}");
            return Task.FromResult(literacyState);
        }

        /// <summary>
        /// Retrieves the number of additional languages a character can learn.
        /// </summary>
         
        public Task<string> GetAdditionalLanguagesAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            int additionalLanguages = state.GetAdditionalLanguages();
            _logger.LogInformation($"Retrieved additional languages for {conversationId}: {additionalLanguages}");
            return Task.FromResult(additionalLanguages.ToString());
        }

        /// <summary>
        /// Retrieves the maximum number of retainers a character can have based on their charisma score.
        /// </summary>
        public Task<string> GetMaxRetainersAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            int maxRetainers = state.GetMaxRetainers();
            _logger.LogInformation($"Retrieved max retainers for {conversationId}: {maxRetainers}");
            return Task.FromResult(maxRetainers.ToString());
        }

        /// <summary>
        /// Retrieves the current loyalty of retainers for a character based on their charisma score.
        /// </summary>
        public Task<string> GetRetainerLoyaltyAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            int currentRetainers = state.GetRetainerLoyalty();
            _logger.LogInformation($"Retrieved current retainers for {conversationId}: {currentRetainers}");
            return Task.FromResult(currentRetainers.ToString());
        }

        /// <summary>
        /// Retrieves the NPC reaction bonus for a character based on their charisma score.
        /// </summary>
        public Task<string> GetNpcReactionBonusAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            int npcReactionBonus = state.GetNpcReactionBonus();
            _logger.LogInformation($"Retrieved NPC reaction bonus for {conversationId}: {npcReactionBonus}");
            return Task.FromResult(npcReactionBonus.ToString());
        }

        /// <summary>
        /// Retrieves a characters chance to open stuck doors.
        /// </summary>
        public Task<string> GetOpenDoorChanceAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var state = GetCharacterState(conversationId);
            string openDoorChance = state.GetOpenDoorChance();
            _logger.LogInformation($"Retrieved open door chance for {conversationId}: {openDoorChance}");
            return Task.FromResult(openDoorChance);
        }

        /// <summary>
        /// Roll the specified number of dice with the given sides.
        ///  </summary>
        public Task<string> RollDiceAsync(int numberOfDice, string diceType, CancellationToken cancellationToken = default)
        {
            Enum.TryParse<DiceType>(diceType, true, out var parsedDiceType);
            int rollResult = Dice.RollDice(parsedDiceType, numberOfDice);
            _logger.LogInformation($"Rolled {numberOfDice}{diceType.ToString()}: {rollResult}");
            return Task.FromResult(rollResult.ToString());
        }
    }
}
