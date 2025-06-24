using System.Text;
using System.Text.Json;

namespace GenAIExpertEngineAPI.Services
{
    public class CharacterState
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        private int additionalLanguages = 0; // Default value for additional languages
        public PersonalityBackground? personalityBackground { get; set; } = new PersonalityBackground();
        public CharacterClass CharacterClass { get; set; }
        public Alignment Alignment { get; set; }
        public CharacterRace? Race { get; set; }   
        public string CharacterName { get; set; } = string.Empty;
        public List<Languages> KnownLanguages { get; set; } = new List<Languages>();
        public Dictionary<AbilityType, AbilityScore> AbilityScores { get; set; }
        public ExperienceState? Experience { get; set; }
        public HealthState? Health { get; set; }
        public SavingThrows? SavingThrows { get; set; }
        public CombatState? Combat { get; set; }
        public Spells? Spells { get; set; }
        public WealthState? Wealth { get; set; }
        public string Literacy => GetLiteracy(); // Property to get literacy based on Intelligence score
        public int AdditionalLanguages => GetAdditionalLanguages(); // Property to get additional languages based on Intelligence score
        public int MaxRetainers => GetMaxRetainers(); // Property to get max retainers based on Charisma score
        public int RetainerLoyalty => GetRetainerLoyalty(); // Property to get retainer loyalty based on Charisma score
        public int NpcReactionBonus => GetNpcReactionBonus(); // Property to get NPC reaction bonus based on Charisma score
        public string OpenDoorChance => GetOpenDoorChance(); // Property to get open door chance based on Strength score

        public CharacterState(GameSystemRegistryService gameSystemRegistry)
        {
            _gameSystemRegistry = gameSystemRegistry;
            AbilityScores = new Dictionary<AbilityType, AbilityScore>
            {
                { AbilityType.Str, new AbilityScore(9, gameSystemRegistry) },
                { AbilityType.Dex, new AbilityScore(9, gameSystemRegistry) },
                { AbilityType.Con, new AbilityScore(9, gameSystemRegistry) },
                { AbilityType.Int, new AbilityScore(9, gameSystemRegistry) },
                { AbilityType.Wis, new AbilityScore(9, gameSystemRegistry) },
                { AbilityType.Cha, new AbilityScore(9, gameSystemRegistry) }
            };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Current Character State:\n");
            sb.AppendLine($"  Name: {CharacterName}");
            if (Race.HasValue) // Updated null check to use nullable type  
            {
                sb.AppendLine($"  Race: {Race.Value}");
            }
            sb.AppendLine($"  Alignment: {Alignment}");
            sb.AppendLine($"  Class: {CharacterClass}");
            if (Experience != null) // Added null check for Experience
            {
                sb.AppendLine($"  {Experience.ToString()}");
            }
            else
            {
                sb.AppendLine("  ExperienceState is not Initialized");
            }
            if (AbilityScores.Any())
            {
                sb.AppendLine("  Ability Scores:");
                foreach (KeyValuePair<AbilityType, AbilityScore> score in AbilityScores)
                {
                    sb.AppendLine($"    {score.Key}: {score.Value.Value} ({score.Value.Modifier})");
                }
            }
            else
            {
                sb.AppendLine("  Ability Scores: None");
            }
            if (Health != null) // Added null check for Health
            {
                sb.AppendLine($"  {Health.ToString()}");
            }
            else
            {
                sb.AppendLine("  HealthState is not Initialized");
            }

            if (personalityBackground != null) // Added null check for personalityBackground
            {
                sb.AppendLine($"  {personalityBackground.ToString()}");
            }
            else
            {
                sb.AppendLine("  PersonalityBackground is not Initialized");
            }

            if (SavingThrows != null) // Added null check for SavingThrows
            {
                sb.AppendLine($"  {SavingThrows.ToString()}");
            }
            else
            {
                sb.AppendLine("  SavingThrows is not Initialized");
            }

            if (Combat != null) // Added null check for Combat
            {
                sb.AppendLine($"  {Combat.ToString()}");
            }
            else
            {
                sb.AppendLine("  CombatState is not Initialized");
            }
            if (Spells != null && Spells.Level1 > 0) // Added null check for Spells
            {
                sb.AppendLine($"  Spells: {Spells.ToString()}");
            }
            if (Wealth != null) // Added null check for Wealth
            {
                sb.AppendLine($"  Wealth: {Wealth.ToString()}");
            }
            else
            {
                sb.AppendLine("  WealthState is not Initialized");
            }
            if (KnownLanguages.Count == 0)
            {
                sb.AppendLine("  Known Languages: None");
            }
            else
            {
                sb.AppendLine("  Known Languages: " + string.Join(", ", KnownLanguages));
            }
            sb.AppendLine($"  Literacy: {GetLiteracy()}");
            if (additionalLanguages > 0)
            {
                sb.AppendLine($"  Additional Learnable Languages: {additionalLanguages}");
                sb.AppendLine("  Available Languages: " + string.Join(", ", GetAvailableLanguages()));
            }
            sb.AppendLine($"  Max Retainers: {GetMaxRetainers()}");
            sb.AppendLine($"  Retainer Loyalty: {GetRetainerLoyalty()}");
            sb.AppendLine($"  NPC Reaction Bonus: {GetNpcReactionBonus()}");
            sb.AppendLine($"  Open Door Chance: {GetOpenDoorChance()}");

            return sb.ToString();
        }

        public string ToJson()
        {
            // Use System.Text.Json to serialize the public state of the CharacterState object
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                // Ignore cycles and allow serialization of fields/properties that may be null
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            return JsonSerializer.Serialize(this, options);
        }

        private void Initialize()
        {
            Experience = new ExperienceState(this, _gameSystemRegistry);
            Health = new HealthState(this, _gameSystemRegistry);
            SavingThrows = new SavingThrows(_gameSystemRegistry);
            Combat = new CombatState(this, _gameSystemRegistry);
            Spells = new Spells(this, _gameSystemRegistry);
            Wealth = new WealthState(_gameSystemRegistry);
        }

        public CharacterClass GetCharacterClass()
        {
            return CharacterClass;
        }

        public string GetCharacterRace()
        {
            return Race?.ToString() ?? string.Empty;
        }

        public Dictionary<AbilityType, AbilityScore> GetAbilityScores()
        {
            return AbilityScores;
        }

        public int GetAbilityScore(AbilityType ability)
        {
            if (AbilityScores.ContainsKey(ability))
            {
                return AbilityScores[ability].Value;
            }
            else
            {
                throw new ArgumentException($"Ability type {ability} does not exist in the character state.");
            }
        }

        public string GetLiteracy()
        {
            return _gameSystemRegistry.GetLiteracyByIntelligence(GetAbilityScore(AbilityType.Int));
        }

        public int GetAdditionalLanguages()
        {
            return _gameSystemRegistry.GetAdditionalLanguagesByIntelligence(GetAbilityScore(AbilityType.Int));
        }

        public int GetMaxRetainers()
        {
            return _gameSystemRegistry.GetMaxRetainersByCharisma(GetAbilityScore(AbilityType.Cha));
        }

        public int GetRetainerLoyalty()
        {
            return _gameSystemRegistry.GetRetainerLoyaltyByCharisma(GetAbilityScore(AbilityType.Cha));
        }

        public int GetNpcReactionBonus()
        {
            return _gameSystemRegistry.GetNpcReactionBonusByCharisma(GetAbilityScore(AbilityType.Cha));
        }

        public string GetOpenDoorChance()
        {
            return _gameSystemRegistry.GetOpenDoorChanceByStrength(GetAbilityScore(AbilityType.Str));
        }

        public void SetCharacterName(string v)
        {
            CharacterName = v;
        }

        public void SetCharacterRace(CharacterRace v)
        {
            Race = v;
        }

        public void SetCharacterClass(CharacterClass characterClass)
        {
            if (CharacterClass == characterClass)
            {
                return; // No change in character class
            }
            if (Race.HasValue)
            {
                // Additional logic for Race if needed
            }
            CharacterClass = characterClass;

            Initialize(); // Initialize other states based on the character class
            SetKnownLanguages();
            additionalLanguages = GetAdditionalLanguages();

            // Ensure Experience is initialized before calling SetXPModifier
            if (Experience == null)
            {
                Experience = new ExperienceState(this, _gameSystemRegistry);
            }
            Experience.SetXPModifier(this); // Set XP multiplier based on character class
        }

        public void SetAlignment(Alignment v)
        {
            Alignment = v;
        }

        public void GainExperience(int xpGained)
        {
            if (Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            Experience.GainExperience(xpGained, this);
        }

        public void SetAbilityScore(AbilityType abilityType, int value)
        {
            if (AbilityScores.ContainsKey(abilityType))
            {
                AbilityScores[abilityType].Value = value;
            }
            else
            {
                throw new ArgumentException($"Ability type {abilityType} does not exist in the character state.");
            }
        }

        private void SetKnownLanguages()
        {
            List<string> languages = _gameSystemRegistry.GetStartingLanguagesByClass(CharacterClass.ToString());

            KnownLanguages.Clear(); // Clear existing languages as this is an initializing method
            foreach (string language in languages)
            {
                if(Enum.TryParse<Languages>(language, out Languages lang))
                {
                    KnownLanguages.Add(lang);
                }
                else
                {
                    throw new ArgumentException($"Language {language} is not a valid enum value.");
                }
            }
        }

        private List<Languages> GetAvailableLanguages()
        {
            List<Languages> availableLanguages = new List<Languages>();
            foreach (Languages language in Enum.GetValues<Languages>()) 
            {
                if (!KnownLanguages.Contains(language) && !language.ToString().Contains("Secret"))
                {
                    availableLanguages.Add(language);
                }
            }
            return availableLanguages;
        }

        public void SetAdditionalLanguage(Languages language = Languages.Common)
        {
            if(additionalLanguages <= 0)
            {
                return; // No additional languages available to set
            }
            else if (language == Languages.Common || language == Languages.Alignment)
            {
                //roll for random language if Common or Alignment is selected
                do
                {
                    language = GetRandomLanguage();
                } while (KnownLanguages.Contains(language) || language.ToString().Contains("Secret"));
            }

            if (!KnownLanguages.Contains(language) && !language.ToString().Contains("Secret"))
            {
                KnownLanguages.Add(language);
                additionalLanguages--; // Decrease the count of additional languages available
            }
            else
            {
                throw new ArgumentException($"Language {language} is already known or is a secret language.");
            }           
        }

        private static Languages GetRandomLanguage()
        {
            Languages language;
            Languages[] languages = Enum.GetValues<Languages>();
            Random random = Dice.random;
            language = languages[random.Next(languages.Length)];
            return language;
        }
    }

    public class AbilityScore
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public int Value { get; set; } = 9; // Default value for ability scores
        public int Modifier { get; set; }

        public AbilityScore(int value, GameSystemRegistryService gameSystemRegistry)
        {
            Value = value;
            Modifier = GetModifier(value); // Calculate modifier based on the value
            _gameSystemRegistry = gameSystemRegistry;
        }

        public void SetAbilityScore(int value)
        {
            Value = value;
            Modifier = GetModifier(value);
        }

        public override string ToString()
        {
            return $"{Value} ({Modifier})";
        }

        private int GetModifier(int value)
        {
            return _gameSystemRegistry.GetAbilityScoreModifier(value);
        }       
    }

    public class PersonalityBackground
    {
        public Background Background { get; set; } = new Background();
        public Personality Personality { get; set; } = new Personality();

        public PersonalityBackground() { }

        public override string ToString()
        {
            return $"{Background.ToString()}\n{Personality.ToString()}";
        }
    }

    public class Background
    {
        public string ParentageAndBirth { get; set; } = string.Empty;
        public string FamilySituation { get; set; } = string.Empty;
        public string PhysicalDescription { get; set; } = string.Empty;
        public string UpbringingAndKeyEvents { get; set; } = string.Empty;

        public Background() { }

        public override string ToString()
        {
            return $"Background: {ParentageAndBirth}, {FamilySituation}, {PhysicalDescription}, {UpbringingAndKeyEvents}";
        }
    }

    public class Personality
    {
        public string CoreTrait { get; set; } = string.Empty;
        public string QuirkOrHabit { get; set; } = string.Empty;
        public string Ideal { get; set; } = string.Empty;
        public string Bond { get; set; } = string.Empty;
        public string FlawOrFear { get; set; } = string.Empty;
        public string AmbitionOrDesire { get; set; } = string.Empty;
        public string SocialStyle { get; set; } = string.Empty;
        public string EmotionalExpression { get; set; } = string.Empty;

        public Personality() { }

        public override string ToString()
        {
            return $"Personality: {CoreTrait}, {QuirkOrHabit}, {Ideal}, {Bond}, {FlawOrFear}, {AmbitionOrDesire}, {SocialStyle}, {EmotionalExpression}";
        }
    }

    public class HealthState
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int TempHP { get; set; } = 0; // Temporary hit points, if applicable
        public DiceType HitDiceType { get; set; } = DiceType.D6; // Default dice for HP calculation, can be adjusted based on class
        public int HitDiceCount { get; set; } = 1; // Default number of hit dice, can be adjusted based on class
        public int MaxHitDiceModifier { get; set; } = 0; // Default max hit dice modifier, can be adjusted based on class


        public override string ToString()
        {
            return $"Health: {CurrentHP}/{MaxHP} HP (Temp: {TempHP})\nHitDiceType: {HitDiceType.ToString()} Number of HD: {HitDiceCount}";
        }

        public HealthState(CharacterState character, GameSystemRegistryService gameSystemRegistry)
        {
            MaxHP = RollHealth() + character.GetAbilityScores()[AbilityType.Con].Modifier;
            CurrentHP = MaxHP;
            HitDiceType = GetHitDiceType(character.GetCharacterClass());
            _gameSystemRegistry = gameSystemRegistry;
        }

        public void Update(CharacterState character)
        {
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            HitDiceCount = character.Experience.Level;
            MaxHitDiceModifier = GetHitDiceModifier(character.GetCharacterClass(), character.Experience.Level);
        }

        public int RollHealth(int roll = 0) 
        {
            if(roll <= 0)
            {
                roll = Dice.RollDie(HitDiceType);
            }
            return roll;
        }

        public DiceType GetHitDiceType(CharacterClass characterClass)
        {
            string diceType = _gameSystemRegistry.GetHitDiceType(characterClass.ToString());
            DiceType parsedDiceType = DiceType.D6; // Default to d6 if parsing fails
            Enum.TryParse(diceType, out parsedDiceType);
            return parsedDiceType;
        }

        public int GetHitDiceModifier(CharacterClass characterClass, int level)
        {
            return _gameSystemRegistry.GetHitDiceModifiers(characterClass.ToString(), level);
        }
    }

    public class ExperienceState
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public int CurrentXP { get; set; }
        public int Level { get; set; } = 1; // Default level starts at 1
        public int XPToNextLevel { get; set; } = 0;
        public float XPMultiplier { get; set; } = 1.0f;
        private int LevelMax = 14; // Default max level, can be adjusted based on class

        public override string ToString()
        {
            return $"Experience: {CurrentXP} XP (Level {Level}, Next Level: {XPToNextLevel} XP) XPMultiplier: {XPMultiplier}";
        }

        public ExperienceState(CharacterState character, GameSystemRegistryService gameSystemRegistry)
        {
            CurrentXP = 0;
            SetMaxLevel(character.GetCharacterClass()); // Set the max level based on character class
            _gameSystemRegistry = gameSystemRegistry;
        }

        public void GainExperience(int xpGained, CharacterState character)
        {
            if (xpGained < 0)
            {
                throw new ArgumentException("Experience gained cannot be negative.");
            }
            CurrentXP += (int)(xpGained * XPMultiplier); // Apply XP multiplier
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            character.Experience.Update(character); // Update experience state after gaining XP
        }

        public void Update(CharacterState character)
        {
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            XPToNextLevel = GetXPForNextLevel(character.GetCharacterClass(), character.Experience.Level);
            while (CurrentXP >= XPToNextLevel && XPToNextLevel > 0 && Level < LevelMax)
            {
                levelUp(character); // Increment Level
                // After leveling up, recalculate XP needed for the *new* next level to re-enter loop.
                XPToNextLevel = GetXPForNextLevel(character.GetCharacterClass(), character.Experience.Level);
            }
            if (Level >= LevelMax)
            {
                XPToNextLevel = 0;
            }
        }

        private void levelUp(CharacterState character)
        {
            if(Level < LevelMax)
            {
                Level++;

                if (character.Health == null)
                {
                    throw new InvalidOperationException("Health is not initialized.");
                }
                character.Health.Update(character); // Update health state after leveling up

                if (character.Health.MaxHitDiceModifier <= 0)
                {
                    character.Health.MaxHP += character.Health.RollHealth() + character.GetAbilityScores()[AbilityType.Con].Modifier;
                }
                else
                {
                    character.Health.MaxHP += character.Health.MaxHitDiceModifier;
                }
                character.Health.CurrentHP = character.Health.MaxHP; // Reset current HP to max after leveling up

                if (character.SavingThrows == null)
                {
                    throw new InvalidOperationException("SavingThrows is not initialized.");
                }
                character.SavingThrows.Update(character); // Update saving throws after leveling up

                if (character.Spells == null)
                {
                    throw new InvalidOperationException("Spells is not initialized.");
                }
                character.Spells.Update(character); // Update spells after leveling up

                if (character.Combat == null)
                {
                    throw new InvalidOperationException("Combat is not initialized.");
                }
                character.Combat.Update(character); // Update combat state after leveling up
            }

        }

        private void SetMaxLevel(CharacterClass characterClass)
        {
            LevelMax = _gameSystemRegistry.GetMaxLevel(characterClass.ToString());
        }

        public int GetXPForNextLevel(CharacterClass characterClass, int currentLevel)
        {
            if(currentLevel < 1)
            {
                return 0; // No XP needed for level 0
            }
            else if (currentLevel == LevelMax)
            {
                return 0; // No XP needed for levels above max
            }

            return _gameSystemRegistry.GetXPForNextLevel(characterClass.ToString(), currentLevel);
        }

        public void SetXPModifier(CharacterState character)
        {
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            ExperienceState experienceState = character.Experience;

            // Get ability scores
            int strScore = character.GetAbilityScore(AbilityType.Str);
            int dexScore = character.GetAbilityScore(AbilityType.Dex);
            int conScore = character.GetAbilityScore(AbilityType.Con);
            int intScore = character.GetAbilityScore(AbilityType.Int);
            int wisScore = character.GetAbilityScore(AbilityType.Wis);
            int chaScore = character.GetAbilityScore(AbilityType.Cha);

            // Get the prime requisites rule for the current character class
            var primeRequisitesRule = _gameSystemRegistry.GetPrimeRequisitesRule(character.GetCharacterClass().ToString());

            float calculatedMultiplier = 0.0f; // Default to no bonus

            if (primeRequisitesRule != null)
            {
                if (primeRequisitesRule.Conditions != null && primeRequisitesRule.Conditions.Any())
                {
                    // This class has complex conditions (e.g., Elf, Halfling, Barbarian, etc.)
                    foreach (var condition in primeRequisitesRule.Conditions)
                    {
                        bool meetsCondition = true;
                        // Check each ability score condition if specified in the JSON
                        if (condition.str_min.HasValue && strScore < condition.str_min.Value) meetsCondition = false;
                        if (condition.dex_min.HasValue && dexScore < condition.dex_min.Value) meetsCondition = false;
                        if (condition.con_min.HasValue && conScore < condition.con_min.Value) meetsCondition = false;
                        if (condition.int_min.HasValue && intScore < condition.int_min.Value) meetsCondition = false;
                        if (condition.wis_min.HasValue && wisScore < condition.wis_min.Value) meetsCondition = false;
                        if (condition.cha_min.HasValue && chaScore < condition.cha_min.Value) meetsCondition = false;

                        if (meetsCondition)
                        {
                            calculatedMultiplier = condition.multiplier;
                            break; // Apply the first met condition and stop
                        }
                    }
                }
                else if (primeRequisitesRule.primary_ability != "None")
                {
                    // This class uses a single primary ability (e.g., Cleric, Fighter, MagicUser)
                    if (Enum.TryParse(primeRequisitesRule.primary_ability, out AbilityType primaryAbilityType))
                    {
                        int primaryAbilityScore = character.GetAbilityScore(primaryAbilityType);
                        calculatedMultiplier = _gameSystemRegistry.GetXPModifier(character.GetCharacterClass().ToString(), primaryAbilityScore);
                    }
                }
            }
            // If primeRequisitesRule is null, or has no conditions/primary ability,
            // calculatedMultiplier remains 0.0f, matching your default behavior.

            experienceState.XPMultiplier = calculatedMultiplier;
        }
    }

    public class SavingThrows
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public int DeathPoison { get; set; }
        public int Wands { get; set; }
        public int ParalysisPetrify { get; set; }
        public int BreathAttacks { get; set; }
        public int SpellsRodsStaves { get; set; }

        public SavingThrows(GameSystemRegistryService gameSystemRegistry)
        {
            DeathPoison = 20; // Default values for other classes
            Wands = 20;
            ParalysisPetrify = 20;
            BreathAttacks = 20;
            SpellsRodsStaves = 20;
            _gameSystemRegistry = gameSystemRegistry;
        }

        public SavingThrows(int deathPoison, int wands, int paralysisPetrify, int breathAttacks, int spellsRodsStaves, GameSystemRegistryService gameSystemRegistry)
        {
            _gameSystemRegistry = gameSystemRegistry;
            DeathPoison = deathPoison;
            Wands = wands;
            ParalysisPetrify = paralysisPetrify;
            BreathAttacks = breathAttacks;
            SpellsRodsStaves = spellsRodsStaves;
        }

        public override string ToString()
        {
            return $"Saving Throws:\nDeath/Poison: {DeathPoison}\nWands: {Wands}\nParalysis/Petrify: {ParalysisPetrify}\nBreath Attacks: {BreathAttacks}\nSpells/Rods/Staves: {SpellsRodsStaves}";
        }

        public void Update(CharacterState character)
        {
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            SavingThrows savingThrows = GetSavingThrows(character.GetCharacterClass(), character.Experience.Level);

            DeathPoison = savingThrows.DeathPoison;
            Wands = savingThrows.Wands;
            ParalysisPetrify = savingThrows.ParalysisPetrify;
            BreathAttacks = savingThrows.BreathAttacks;
            SpellsRodsStaves = savingThrows.SpellsRodsStaves;
        }

        public SavingThrows GetSavingThrows(CharacterClass characterClass, int level)
        {
            SavingThrowValues savingThrowValues = _gameSystemRegistry.GetSavingThrows(characterClass.ToString(), level);
            if (savingThrowValues == null)
            {
                throw new InvalidOperationException($"No saving throw values found for class {characterClass} at level {level}.");
            }
            return new SavingThrows(
                savingThrowValues.DeathPoison,
                savingThrowValues.Wands,
                savingThrowValues.ParalysisPetrify,
                savingThrowValues.BreathAttacks,
                savingThrowValues.SpellsRodsStaves,
                _gameSystemRegistry);
        }
    }

    public class CombatState
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public int ToHitAC0 { get; set; } = 20; // To-hit bonus against AC 0
        public int ToHitACC0 { get; set; } = 0; // To-hit bonus against ACC 0
        public int MeleeDamageBonus { get; set; } = 0; // melee damage bonus
        public int MeleeHitBonus { get; set; } = 0; // melee hit bonus
        public int MissileHitBonus { get; set; } = 0; // missile hit bonus
        public int InitiativeBonus { get; set; } = 0; // initiative bonus

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Combat State:\n");
            sb.AppendLine($"  THAC0: {ToHitAC0} [{ToHitACC0}]");
            sb.AppendLine($"  Melee Attack Bonuses: {MeleeHitBonus} to hit, {MeleeDamageBonus} damage");
            sb.AppendLine($"  Missile Attack Bonus: {MissileHitBonus} to hit");
            sb.AppendLine($"  Initiative Bonus: {InitiativeBonus}");
            return sb.ToString();
        }

        public CombatState(CharacterState character, GameSystemRegistryService gameSystemRegistry) 
        { 
            _gameSystemRegistry = gameSystemRegistry; // Store the game system registry service
            Update(character); // Initialize with level 1 values
        }

        public void Update(CharacterState character)
        {
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            ToHitAC0 = GetToHitAC(character.GetCharacterClass(), character.Experience.Level);
            ToHitACC0 = GetToHitACC(character.GetCharacterClass(), character.Experience.Level);

            SetMeleeDamageBonus(character);
            SetMeleeHitBonus(character);
            SetMissileHitBonus(character);
            SetInitiativeBonus(character);
        }

        private void SetMeleeDamageBonus(CharacterState character)
        {
            MeleeDamageBonus = character.GetAbilityScores()[AbilityType.Str].Modifier;
        }

        private void SetMeleeHitBonus(CharacterState character)
        {
            MeleeHitBonus = character.GetAbilityScores()[AbilityType.Str].Modifier;
        }

        private void SetMissileHitBonus(CharacterState character)
        {
            MissileHitBonus = character.GetAbilityScores()[AbilityType.Dex].Modifier;
        }

        private void SetInitiativeBonus(CharacterState character)
        {
            InitiativeBonus = character.GetAbilityScores()[AbilityType.Dex].Modifier;
        }

        public int GetToHitAC(CharacterClass characterClass, int level)
        {
            return _gameSystemRegistry.GetToHitAC(characterClass.ToString(), level);
        }

        public int GetToHitACC(CharacterClass characterClass, int level)
        {
            return _gameSystemRegistry.GetToHitACC(characterClass.ToString(), level);
        }
    }

    public class Spells
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        // Number of spells available at each level
        public int Level1 { get; set; } // Number of spells available at level 1
        public int Level2 { get; set; } // Number of spells available at level 2
        public int Level3 { get; set; } // Number of spells available at level 3
        public int Level4 { get; set; } // Number of spells available at level 4
        public int Level5 { get; set; } // Number of spells available at level 5
        public int Level6 { get; set; } // Number of spells available at level 6

        public Spells(CharacterState character, GameSystemRegistryService gameSystemRegistry) 
        {
            _gameSystemRegistry = gameSystemRegistry; // Store the game system registry service
            Update(character); // Initialize with level 1 values
        }

        public Spells(int level1, int level2, int level3, int level4, int level5, int level6, GameSystemRegistryService gameSystemRegistry)
        {
            _gameSystemRegistry = gameSystemRegistry; // Store the game system registry service
            Level1 = level1;
            Level2 = level2;
            Level3 = level3;
            Level4 = level4;
            Level5 = level5;
            Level6 = level6;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Level1 == 0 && Level2 == 0 && Level3 == 0 && Level4 == 0 && Level5 == 0 && Level6 == 0)
            {
                return "No spells available.";
            }
            else if (Level1 > 0)
            {
                sb.AppendLine($"Level 1 Spells: {Level1}");
            }
            if (Level2 > 0)
            {
                sb.AppendLine($"Level 2 Spells: {Level2}");
            }
            if (Level3 > 0)
            {
                sb.AppendLine($"Level 3 Spells: {Level3}");
            }
            if (Level4 > 0)
            {
                sb.AppendLine($"Level 4 Spells: {Level4}");
            }
            if (Level5 > 0)
            {
                sb.AppendLine($"Level 5 Spells: {Level5}");
            }
            if (Level6 > 0)
            {
                sb.AppendLine($"Level 6 Spells: {Level6}");
            }
            return sb.ToString();
        }

        public void Update(CharacterState character)
        {
            // Update the number of spells based on character class and level
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            Spells spellCount = GetSpellCount(character.GetCharacterClass(), character.Experience.Level);

            Level1 = spellCount.Level1;
            Level2 = spellCount.Level2;
            Level3 = spellCount.Level3;
            Level4 = spellCount.Level4;
            Level5 = spellCount.Level5;
            Level6 = spellCount.Level6;
        }

        public Spells GetSpellCount(CharacterClass characterClass, int level)
        {
            int[] spells = _gameSystemRegistry.GetSpellsPerLevel(characterClass.ToString(), level);
            if (spells == null || spells.Length < 6)
            {
                throw new InvalidOperationException($"No spell counts found for class {characterClass} at level {level}.");
            }
            return new Spells(
                spells[0], // Level 1 spells
                spells[1], // Level 2 spells
                spells[2], // Level 3 spells
                spells[3], // Level 4 spells
                spells[4], // Level 5 spells
                spells[5], // Level 6 spells
                _gameSystemRegistry); 
        }
    }

    public enum MagicType
    {
        None, // No magic
        Arcane, // Arcane magic (e.g., magic-user, illusionist)
        Divine // Divine magic (e.g., clerics, paladins)
    }

    public class Item
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Weight { get; set; } // Weight in coins or other unit
        public int Value { get; set; } // Value in coins or other unit
        public override string ToString()
        {
            return $"{Name} ({Weight} weight, {Value} value): {Description}";
        }
    }

    public class Weapon : Item
    {
        public int Damage { get; set; } // Damage dealt by the weapon
        public bool isBlunt { get; set; } = false; // Whether the weapon is a blunt weapon and usable by Cleric (e.g., mace, war hammer)
        public bool IsMelee { get; set; } = true; // Whether the weapon is used in melee combat
        public bool IsMissile { get; set; } = false; // Whether the weapon can be used as a missile
        public bool IsTwoHanded { get; set; } = false; // Whether the weapon requires two hands to wield
        public bool IsSlow { get; set; } = false; // Whether the weapon is slow to use
        public bool IsSplash { get; set; } = false; // Whether the weapon is a splash weapon (e.g., oil flask)
        public bool isBrace { get; set; } = false; // Whether the weapon can be braced against the ground
        public bool canCharge { get; set; } = false; // Whether the weapon can be used on horseback to charge an opponent
        public bool isReload { get; set; } = false; // Whether the weapon requires reloading (e.g., crossbow)
        public Range Range { get; set; } = new Range(); // Ranges for missile weapons
        public bool isSilver { get; set; } = false; // Whether the weapon is made of silver, useful against certain creatures (e.g., werewolves, wights)

        public Weapon(string name, int damage, string description = "")
        {
            Name = name;
            Damage = damage;
            Description = description;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}");
            if (!string.IsNullOrWhiteSpace(Description))
            {
                sb.Append($": {Description}");
            }
            sb.AppendLine($"  Damage: {Damage} (Blunt: {isBlunt})");
            sb.AppendLine("  Qualities:");
            if (IsMelee)
            {
                sb.AppendLine("     Melee");
            }
            if (IsMissile)
            {
                sb.AppendLine($"     Missile ({Range.ToString()})");
            }
            if (IsTwoHanded)
            {
                sb.AppendLine("     Two-handed");
            }
            if (IsSlow)
            {
                sb.AppendLine("     Slow");
            }
            if (IsSplash)
            {
                sb.AppendLine("     Splash weapon");
            }
            if (isBrace)
            {
                sb.AppendLine("     Brace");
            }
            if (canCharge)
            {
                sb.AppendLine("     Charge");
            }
            if (isReload)
            {
                sb.AppendLine("     Reload");
            }
            if (isSilver)
            {
                sb.AppendLine("     Silver");
            }
            return sb.ToString();
        }
    }

    public class Range
    {
        public string Short { get; set; } = "5’–10’"; // e.g., "5’–50’"
        public string Medium { get; set; } = "11’–20’"; // e.g., "51’–100’"
        public string Long { get; set; } = "21’–30’"; // e.g., "101’–150’"
        public override string ToString()
        {
            return $"{Short} / {Medium} / {Long}";
        }
    }

    public class Ammunition : Item
    {
        public int Quantity { get; set; } // Number of ammunition items (e.g., arrows, bolts)
        public bool isSilver { get; set; } = false; // Whether the ammunition is made of silver, useful against certain creatures (e.g., werewolves, wights)
        public Ammunition(string name, int quantity, int weight, int value, string description = "")
        {
            Name = name;
            Quantity = quantity;
            Weight = weight;
            Value = value;
            Description = description;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}");
            if (!string.IsNullOrWhiteSpace(Description))
            {
                sb.Append($": {Description}");
            }
            sb.AppendLine($"  Quantity: {Quantity}, Weight: {Weight} coins, Value: {Value} gp");
            if (isSilver)
            {
                sb.AppendLine("  Silver tipped");
            }
            return sb.ToString();
        }
    }

    public class Armour : Item
    {
        public int AC { get; set; } // Armour Class
        public int AAC { get; set; } // Ascending Armour Class
        public int ACBonus { get; set; } = 0; // Armour Class bonus for shields
        public ArmourType armourType { get; set; } = ArmourType.None; // Type of armour (e.g., light armour, heavy armour.)
        public Armour(string name, int ac, int cost, int weight, ArmourType armourType = ArmourType.None, string description = "")
        {
            Name = name;
            AC = ac;
            Value = cost;
            Weight = weight;
            Description = description;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{Name}");
            if (!string.IsNullOrWhiteSpace(Description))
            {
                sb.Append($": {Description}");
            }
            if (ACBonus == 0)
            {
                sb.AppendLine($"  Armour Class: {AC} [{AAC}]");
            }
            else
            {
                sb.AppendLine($"  Shield Bonus: {ACBonus} AC");
            }
            sb.AppendLine($"  Value: {Value} gp, Weight: {Weight} coins");
            return sb.ToString();
        }
    }

    public class EquipmentState
    {
        public List<Item> Equipped { get; set; } = new List<Item>();
        public List<Item> Carrying { get; set; } = new List<Item>();
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Equipped Items:");
            if (Equipped.Any())
            {
                foreach (var item in Equipped)
                {
                    sb.AppendLine($"  - {item.ToString()}");
                }
            }
            else
            {
                sb.AppendLine("  None");
            }
            sb.AppendLine("Carrying Items:");
            if (Carrying.Any())
            {
                foreach (var item in Carrying)
                {
                    sb.AppendLine($"  - {item.ToString()}");
                }
            }
            else
            {
                sb.AppendLine("  None");
            }
            return sb.ToString();
        }
    }

    public class WealthState
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public int Gold { get; set; } = 0; // Amount of gold coins
        public int Platinum { get; set; } = 0; // Amount of platinum coins
        public int Electrum { get; set; } = 0; // Amount of electrum coins
        public int Silver { get; set; } = 0; // Amount of silver coins
        public int Copper { get; set; } = 0; // Amount of copper coins
        public int TotalValueInGP
        {
            get
            {
                return Platinum * 5 + Electrum * 2 + Silver / 10 + Copper / 100 + Gold;
            }
        }
        public Domain domain { get; set; } = new Domain(); // Domain information, if applicable

        public override string ToString()
        {
            return $"Wealth: {TotalValueInGP} GP (Gold: {Gold}, Platinum: {Platinum}, Electrum: {Electrum}, Silver: {Silver}, Copper: {Copper})";
        }

        public WealthState(GameSystemRegistryService gameSystemRegistry)
        {
            _gameSystemRegistry = gameSystemRegistry;
        }

        public int ConvertCoinToGold(CoinType type, int coin)
        {
            float conversion = _gameSystemRegistry.GetCoinConversionRate(type.ToString());
            return (int)Math.Floor(coin * conversion);
        }

        public class Domain
        {
            public string Name { get; set; } = string.Empty; // Name of the domain
            public string Description { get; set; } = string.Empty; // Description of the domain
            public int Population { get; set; } = 0; // Number of inhabitants in the domain
            public int TaxIncome { get; set; } = 0; // Annual tax income from the domain
            public List<Item> Structures { get; set; } = new List<Item>(); // List of structures in the domain

            public Domain() { }
        }
    }

    public enum AbilityType
    {
        Str,
        Dex,
        Con,
        Int,
        Wis,
        Cha
    }

    public enum Alignment
    {
        Lawful,
        Chaotic,
        Neutral
    }

    public enum Languages
    {
        Common,
        Alignment,
        Bugbear,
        Doppelgänger,
        Dragon,
        Dwarvish,
        Elvish,
        Gargoyle,
        Gnoll,
        Gnomish,
        Goblin,
        Halfling,
        Harpy,
        Hobgoblin,
        Kobold,
        LizardMan,
        Medusa,
        Minotaur,
        Ogre,
        Orcish,
        Pixie,
        HumanDialect,
        Deepcommon,
        SecretSpider,
        SecretBurrowingMammals,
        SecretEarthElemental
    }

    public enum CharacterClass
    {
        Cleric,
        Dwarf,
        Elf,
        Fighter,
        Halfling,
        MagicUser,
        Thief,
        Acrobat,
        Assassin,
        Barbarian,
        Bard,
        Drow,
        Druid,
        Duergar,
        Gnome,
        HalfElf,
        HalfOrc,
        Illusionist,
        Knight,
        Paladin,
        Ranger,
        Svirfneblin,
        Necromancer
    }

    public enum CharacterRace
    {
        Dwarf,
        Elf,
        Human,
        Halfling,
        Drow,
        Duergar,
        Gnome,
        HalfElf,
        HalfOrc,
        Svirfneblin
    }

    public enum ArmourType
    {
        None, // No armour
        Light, // Light armour (e.g., leather)
        Heavy // Heavy armour (e.g., chainmail, plate mail)
    }

    public enum CoinType
    {
        Gold,
        Platinum,
        Electrum,
        Silver,
        Copper
    }
}
