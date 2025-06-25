using GenAIExpertEngineAPI.Classes;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenAIExpertEngineAPI.Services
{
    public class CharacterState
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        private int additionalLanguages = 0; // Default value for additional languages
        public PersonalityBackground? personalityBackground { get; set; } = new PersonalityBackground();
        public string CharacterClass { get; set; } = string.Empty; 
        public string Alignment { get; set; } = string.Empty;
        public string? Race { get; set; }   
        public string CharacterName { get; set; } = string.Empty;
        public List<string> KnownLanguages { get; set; } = new List<string>();
        public List<AbilityScore> AbilityScores { get; set; } = new List<AbilityScore>();
        public ExperienceState? Experience { get; set; }
        public HealthState? Health { get; set; }
        public SavingThrows? SavingThrows { get; set; }
        public CombatState? Combat { get; set; }
        public Spells? Spells { get; set; }
        public WealthState? Wealth { get; set; }

        public CharacterState(GameSystemRegistryService gameSystemRegistry)
        {
            _gameSystemRegistry = gameSystemRegistry;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Current Character State:\n");
            sb.AppendLine($"  Name: {CharacterName}");
            if (!string.IsNullOrEmpty(Race)) // Updated null check to use nullable type  
            {
                sb.AppendLine($"  Race: {Race}");
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
                foreach (AbilityScore score in AbilityScores)
                {
                    sb.AppendLine($"    {score.Name}: {score.Value} ({score.Modifier})");
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
            if (Spells != null && Spells.Level1Max > 0) // Added null check for Spells
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
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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

        public string GetCharacterClass()
        {
            return CharacterClass.ToString();
        }

        public string GetCharacterRace()
        {
            return Race?.ToString() ?? string.Empty;
        }

        public List<AbilityScore> GetAbilityScores()
        {
            return AbilityScores;
        }

        public int GetAbilityScoreValue(string ability)
        {
            List<string> abilityTypes = _gameSystemRegistry.GetAllAbilityTypes();
            if (!abilityTypes.Contains(ability, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid ability type: {ability}. Valid types are: {string.Join(", ", abilityTypes)}");
            }

            return AbilityScores.FirstOrDefault(x => x.Name.Equals(ability, StringComparison.OrdinalIgnoreCase))?.Value ?? 0;
        }
        
        public AbilityScore GetAbilityScore(string ability)
        {
            List<string> abilityTypes = _gameSystemRegistry.GetAllAbilityTypes();
            if (!abilityTypes.Contains(ability, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid ability type: {ability}. Valid types are: {string.Join(", ", abilityTypes)}");
            }
            return AbilityScores.FirstOrDefault(x => x.Name.Equals(ability, StringComparison.OrdinalIgnoreCase)) ??
                   throw new ArgumentException($"Ability score for {ability} not found.");
        }
        
        public string GetLiteracy()
        {
            return _gameSystemRegistry.GetLiteracyByIntelligence(GetAbilityScoreValue("Int"));
        }

        public int GetAdditionalLanguages()
        {
            return _gameSystemRegistry.GetAdditionalLanguagesByIntelligence(GetAbilityScoreValue("Int"));
        }

        public int GetMaxRetainers()
        {
            return _gameSystemRegistry.GetMaxRetainersByCharisma(GetAbilityScoreValue("Cha"));
        }

        public int GetRetainerLoyalty()
        {
            return _gameSystemRegistry.GetRetainerLoyaltyByCharisma(GetAbilityScoreValue("Cha"));
        }

        public int GetNpcReactionBonus()
        {
            return _gameSystemRegistry.GetNpcReactionBonusByCharisma(GetAbilityScoreValue("Cha"));
        }

        public string GetOpenDoorChance()
        {
            return _gameSystemRegistry.GetOpenDoorChanceByStrength(GetAbilityScoreValue("Str"));
        }

        public void SetCharacterName(string v)
        {
            CharacterName = v;
        }

        public void SetCharacterRace(string v)
        {
            List<string> validRaces = _gameSystemRegistry.GetAllRaces();
            if (!validRaces.Contains(v, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid ability type: {v}. Valid types are: {string.Join(", ", validRaces)}");
            }
            Race = v;
        }

        public void SetCharacterClass(string characterClass)
        {
            List<string> validClasses = _gameSystemRegistry.GetAllCharacterClasses();
            if (!validClasses.Contains(characterClass, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid ability type: {characterClass}. Valid types are: {string.Join(", ", validClasses)}");
            }

            if (GetCharacterClass() == characterClass)
            {
                return; // No change in character class
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

        public void SetAlignment(string v)
        {
            List<string> validAlignments = _gameSystemRegistry.GetAllRaces();
            if (!validAlignments.Contains(v, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid ability type: {v}. Valid types are: {string.Join(", ", validAlignments)}");
            }
            Alignment = v;
        }

        public void SetAbilityScore(string abilityType, int value)
        {
            GetAbilityScore(abilityType).SetAbilityScore(value);
        }

        private void SetKnownLanguages()
        {
            List<string> languages = _gameSystemRegistry.GetStartingLanguagesByClass(GetCharacterClass());
            
            KnownLanguages.Clear(); // Clear existing languages as this is an initializing method
            KnownLanguages = languages;
        }

        private List<string> GetAvailableLanguages()
        {
            List<string> availableLanguages = new List<string>();
            List<string> validLanguages = _gameSystemRegistry.GetAllLanguages();
            foreach (string language in validLanguages)
            {
                if (!KnownLanguages.Contains(language, StringComparer.OrdinalIgnoreCase) && !language.Contains("Secret", StringComparison.OrdinalIgnoreCase))
                {
                    availableLanguages.Add(language);
                }
            }
            return availableLanguages;
        }

        public void SetAdditionalLanguage(string language = "Common")
        {
            if(additionalLanguages <= 0)
            {
                return; // No additional languages available to set
            }
            else if (language == "Common" || language == "Alignment")
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

        private string GetRandomLanguage()
        {
            string language;
            string[] languages = GetAvailableLanguages().ToArray();
            Random random = Dice.random;
            language = languages[random.Next(languages.Length)];
            return language;
        }

        public void GainExperience(int xpGained)
        {
            if (Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            Experience.GainExperience(xpGained, this);
        }

        public void TakeDamage(int damage)
        {
            if (Health == null)
            {
                throw new InvalidOperationException("Health is not initialized.");
            }
            Health.TakeDamage(damage);
        }

        public void Heal(int amount)
        {
            if (Health == null)
            {
                throw new InvalidOperationException("Health is not initialized.");
            }
            Health.Heal(amount);
        }

        public void GainTempHP(int tempHP)
        {
            if (Health == null)
            {
                throw new InvalidOperationException("Health is not initialized.");
            }
            Health.GainTempHP(tempHP);
        }
    }

    public class AbilityScore
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public string Name { get; private set; }
        public int Value { get; set; } = 9; // Default value for ability scores
        public int Modifier { get; set; }

        public AbilityScore(string name, int value, GameSystemRegistryService gameSystemRegistry)
        {
            Name = name;
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
            MaxHP = RollHealth() + character.GetAbilityScore("Con").Modifier;
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

        public DiceType GetHitDiceType(string characterClass)
        {
            string diceType = _gameSystemRegistry.GetHitDiceType(characterClass.ToString());
            DiceType parsedDiceType = DiceType.D6; // Default to d6 if parsing fails
            Enum.TryParse(diceType, out parsedDiceType);
            return parsedDiceType;
        }

        public int GetHitDiceModifier(string characterClass, int level)
        {
            return _gameSystemRegistry.GetHitDiceModifiers(characterClass.ToString(), level);
        }

        public void TakeDamage(int damage)
        {
            if (damage > 0)
            {
                if (TempHP > 0)
                {
                    while (damage > 0 && TempHP > 0)
                    {
                        TempHP--;
                        damage--; // Reduce temporary HP by the damage amount
                    }
                }

                // Apply damage to current HP after temporary HP is reduced
                CurrentHP -= damage;
                if (CurrentHP < 0)
                {
                    CurrentHP = 0; // Ensure HP does not go below 0
                }
            }            
        }

        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > MaxHP)
            {
                CurrentHP = MaxHP; // Ensure HP does not exceed MaxHP
            }
        }

        public void GainTempHP(int tempHP)
        {
            if (tempHP < 0)
            {
                throw new ArgumentException("Temporary HP cannot be negative.");
            }
            TempHP += tempHP; // Add temporary HP
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
                    character.Health.MaxHP += character.Health.RollHealth() + character.GetAbilityScore("Con").Modifier;
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

        private void SetMaxLevel(string characterClass)
        {
            LevelMax = _gameSystemRegistry.GetMaxLevel(characterClass);
        }

        public int GetXPForNextLevel(string characterClass, int currentLevel)
        {
            if(currentLevel < 1)
            {
                return 0; // No XP needed for level 0
            }
            else if (currentLevel == LevelMax)
            {
                return 0; // No XP needed for levels above max
            }

            return _gameSystemRegistry.GetXPForNextLevel(characterClass, currentLevel);
        }

        public void SetXPModifier(CharacterState character)
        {
            if (character.Experience == null)
            {
                throw new InvalidOperationException("Experience is not initialized.");
            }
            ExperienceState experienceState = character.Experience;

            // Get ability scores
            int strScore = character.GetAbilityScoreValue("Str");
            int dexScore = character.GetAbilityScoreValue("Dex");
            int conScore = character.GetAbilityScoreValue("Con");
            int intScore = character.GetAbilityScoreValue("Int");
            int wisScore = character.GetAbilityScoreValue("Wis");
            int chaScore = character.GetAbilityScoreValue("Cha");

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
                else if (!string.IsNullOrEmpty(primeRequisitesRule.primary_ability) && primeRequisitesRule.primary_ability != "None")
                {
                    // This class uses a single primary ability (e.g., Cleric, Fighter, MagicUser)
                    int primaryAbilityScore = character.GetAbilityScoreValue(primeRequisitesRule.primary_ability);
                    calculatedMultiplier = _gameSystemRegistry.GetXPModifier(character.GetCharacterClass().ToString(), primaryAbilityScore);
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

        public SavingThrows GetSavingThrows(string characterClass, int level)
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
            MeleeDamageBonus = character.GetAbilityScore("Str").Modifier;
        }

        private void SetMeleeHitBonus(CharacterState character)
        {
            MeleeHitBonus = character.GetAbilityScore("Str").Modifier;
        }

        private void SetMissileHitBonus(CharacterState character)
        {
            MissileHitBonus = character.GetAbilityScore("Dex").Modifier;
        }

        private void SetInitiativeBonus(CharacterState character)
        {
            InitiativeBonus = character.GetAbilityScore("Dex").Modifier;
        }

        public int GetToHitAC(string characterClass, int level)
        {
            return _gameSystemRegistry.GetToHitAC(characterClass, level);
        }

        public int GetToHitACC(string characterClass, int level)
        {
            return _gameSystemRegistry.GetToHitACC(characterClass, level);
        }
    }

    public class Spells
    {
        private readonly GameSystemRegistryService _gameSystemRegistry;
        public string MagicType { get; set; } = "None"; // Default magic type, can be set based on character class
        public string SpellType { get; set; } = "None"; // Default spell type, can be set based on character class
        public List<Spell> SpellBook { get; set; } = new List<Spell>();
        // Number of spells available at each level
        public int Level1Max { get; set; } // Number of spells available at level 1
        public int Level2Max { get; set; } // Number of spells available at level 2
        public int Level3Max { get; set; } // Number of spells available at level 3
        public int Level4Max { get; set; } // Number of spells available at level 4
        public int Level5Max { get; set; } // Number of spells available at level 5
        public int Level6Max { get; set; } // Number of spells available at level 6
        Dictionary<int, string> SpellList { get; set; } = new Dictionary<int, string>(); // List of spells available to the character
        public string[] Level1SpellSlots { get; set; } = new string[0]; // Array to hold level 1 spell slots
        public string[] Level2SpellSlots { get; set; } = new string[0]; // Array to hold level 2 spell slots
        public string[] Level3SpellSlots { get; set; } = new string[0]; // Array to hold level 3 spell slots
        public string[] Level4SpellSlots { get; set; } = new string[0]; // Array to hold level 4 spell slots
        public string[] Level5SpellSlots { get; set; } = new string[0]; // Array to hold level 5 spell slots
        public string[] Level6SpellSlots { get; set; } = new string[0]; // Array to hold level 6 spell slots

        public Spells(CharacterState character, GameSystemRegistryService gameSystemRegistry) 
        {
            _gameSystemRegistry = gameSystemRegistry; // Store the game system registry service
            Update(character); // Initialize with level 1 values
        }

        public Spells(int level1, int level2, int level3, int level4, int level5, int level6, GameSystemRegistryService gameSystemRegistry)
        {
            _gameSystemRegistry = gameSystemRegistry; // Store the game system registry service
            Level1Max = level1;
            Level2Max = level2;
            Level3Max = level3;
            Level4Max = level4;
            Level5Max = level5;
            Level6Max = level6;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Level1Max == 0 && Level2Max == 0 && Level3Max == 0 && Level4Max == 0 && Level5Max == 0 && Level6Max == 0)
            {
                return "No spells available.";
            }
            else if (Level1Max > 0)
            {
                sb.AppendLine($"Level 1 Spells: {Level1Max}");
            }
            if (Level2Max > 0)
            {
                sb.AppendLine($"Level 2 Spells: {Level2Max}");
            }
            if (Level3Max > 0)
            {
                sb.AppendLine($"Level 3 Spells: {Level3Max}");
            }
            if (Level4Max > 0)
            {
                sb.AppendLine($"Level 4 Spells: {Level4Max}");
            }
            if (Level5Max > 0)
            {
                sb.AppendLine($"Level 5 Spells: {Level5Max}");
            }
            if (Level6Max > 0)
            {
                sb.AppendLine($"Level 6 Spells: {Level6Max}");
            }
            return sb.ToString();
        }

        public string GetMagicType()
        {
            return MagicType.ToString();
        }

        public void MemorizeSpell(CharacterState character, string spellName)
        {
            List<Spell> spellList = new List<Spell>();
            if (MagicType == "Divine" && character.Experience != null)
            {
                for(int i = 1; i <= character.Experience.Level; i++)
                {
                    List<string> spellsAtLevel = GetSpellList(character.GetCharacterClass(), i);
                    foreach (string name in spellsAtLevel)
                    {
                        Spell? foundSpell = SpellBook.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (foundSpell != null)
                        {
                            spellList.Add(foundSpell);
                        }
                    }
                }
            }
            else
            {
                spellList = SpellBook; // Use the spell book for arcane magic
            }

            //find spell in spelllist

            //if there is an available memory slot at the spell level then memorize the spell

        }

        public void Update(CharacterState character)
        {
            // Update the number of spells based on character class and level
            if (character.Experience == null)
            {
                throw new InvalidOperationException("CharacterClass has not been set.");
            }
            //Set spells based on character class and level
            SetSpells(character);

            // Set the magic type based on character class
            SetMagicType(character.GetCharacterClass());

            // Set the spell type based on character class
            SetSpellType(character.GetCharacterClass());
        }

        private void SetSpells(CharacterState character)
        {
            if (character.Experience != null)
            {
                Spells spellCount = GetSpellCount(character.GetCharacterClass(), character.Experience.Level);

                Level1Max = spellCount.Level1Max;
                Level2Max = spellCount.Level2Max;
                Level3Max = spellCount.Level3Max;
                Level4Max = spellCount.Level4Max;
                Level5Max = spellCount.Level5Max;
                Level6Max = spellCount.Level6Max; 
            }
        }

        public Spells GetSpellCount(string characterClass, int level)
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

        public void SetMagicType(string characterClass)
        {
            string magicType = _gameSystemRegistry.GetMagicType(characterClass.ToString());
            MagicType = magicType;
        }

        public void SetSpellType(string characterClass)
        {
            string spellType = _gameSystemRegistry.GetSpellType(characterClass.ToString());
            SpellType = spellType;
        }

        public List<string> GetSpellList(string characterClass, int level)
        {
            return _gameSystemRegistry.GetSpellList(characterClass.ToString(), level);
        }
    }

    public class Spell
    {
        [JsonPropertyName("spell_type")]
        public string spellType { get; set; } = "Cleric"; // Type of spell (e.g., MagicUser, Cleric, etc.)
        [JsonPropertyName("spell_name")]
        public string Name { get; set; } = string.Empty; // Name of the spell
        [JsonPropertyName("spell_level")]
        public int Level { get; set; } // Spell level
        [JsonPropertyName("spell_duration")]
        public string Duration { get; set; } = string.Empty; // Duration of the spell effect
        [JsonPropertyName("spell_range")]
        public string Range { get; set; } = string.Empty; // Range of the spell
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty; // Description of the spell
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
        public string armourType { get; set; } = "None"; // Type of armour (e.g., light armour, heavy armour.)
        public Armour(string name, int ac, int cost, int weight, string armourType = "None", string description = "")
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

        public int ConvertCoinToGold(string type, int coin)
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
}
