using Microsoft.Extensions.Options;
using System.Data;
using System.Text.Json.Serialization;

namespace GenAIExpertEngineAPI.Services
{
    public class GameSystemRegistryService
    {
        private readonly GameSystemConfiguration _gameSystemData; //

        public GameSystemRegistryService(IOptions<GameSystemConfiguration> gameSystemOptions) //
        {
            _gameSystemData = gameSystemOptions.Value; //
        }

        public GameSystemConfiguration GetGameSystemData()
        {
            return _gameSystemData;
        }

        public List<string> GetAllAbilityTypes()
        {
            return _gameSystemData.Enums.AbilityTypes; // Return the list of ability types
        }

        public List<string> GetAllAlignments()
        {
            return _gameSystemData.Enums.Alignments; // Return the list of alignments
        }

        public List<string> GetAllLanguages()
        {
            return _gameSystemData.Enums.Languages; // Return the list of languages
        }

        public List<string> GetAllCharacterClasses()
        {
            return _gameSystemData.Enums.CharacterClasses; // Return the list of character classes
        }

        public List<string> GetAllRaces() //
        {
            return _gameSystemData.Enums.CharacterRaces;
        }

        public List<string> GetAllMagicTypes()
        {
            return _gameSystemData.Enums.MagicTypes; // Return the list of magic types
        }

        public List<string> GetAllSpellTypes()
        {
            return _gameSystemData.Enums.SpellTypes; // Return the list of spell types
        }

        public List<string> GetAllArmourTypes()
        {
            return _gameSystemData.Enums.ArmourTypes; // Return the list of armour types
        }

        public List<string> GetAllCoinTypes()
        {
            return _gameSystemData.Enums.CoinTypes; // Return the list of coin types
        }

        public int GetAbilityScoreModifier(int score)
        {
            AbilityScoreModifierRule? modifierRule = _gameSystemData.Rules.AbilityScoreModifiers
                .FirstOrDefault(r => r.value_min <= score && r.value_max >= score);
            return modifierRule?.modifier ?? 0; // Return 0 if no rule found
        }

        public int GetMaxRetainersByCharisma(int charismaScore)
        {
            MaxRetainersByCharisma? rule = _gameSystemData.Rules.MaxRetainersByCharisma
                .FirstOrDefault(r => r.cha_score == charismaScore || (r.cha_min <= charismaScore && r.cha_max >= charismaScore));
            return rule?.max_retainers ?? 0; // Return 0 if no rule found
        }

        public int GetRetainerLoyaltyByCharisma(int charismaScore)
        {
            RetainerLoyaltyByCharisma? rule = _gameSystemData.Rules.RetainerLoyaltyByCharisma
                .FirstOrDefault(r => r.cha_score == charismaScore || (r.cha_min <= charismaScore && r.cha_max >= charismaScore));
            return rule?.loyalty ?? 0; // Return 0 if no rule found
        }

        public int GetNpcReactionBonusByCharisma(int charismaScore)
        {
            NpcReactionBonusByCharisma? rule = _gameSystemData.Rules.NpcReactionBonusByCharisma
                .FirstOrDefault(r => r.cha_min <= charismaScore && r.cha_max >= charismaScore);
            return rule?.bonus ?? 0; // Return 0 if no rule found
        }

        public string GetOpenDoorChanceByStrength(int strengthScore)
        {
            OpenDoorChanceByStrength? rule = _gameSystemData.Rules.OpenDoorChanceByStrength
                .FirstOrDefault(r => r.str_min <= strengthScore && r.str_max >= strengthScore);
            return rule?.chance ?? "1-in-6"; // Default chance if no rule found
        }

        public string GetLiteracyByIntelligence(int intelligenceScore)
        {
            LiteracyByIntelligence? rule = _gameSystemData.Rules.LiteracyByIntelligence
                .FirstOrDefault(r => r.int_min <= intelligenceScore && r.int_max >= intelligenceScore);
            return rule?.literacy ?? "UNKNOWN"; // Default literacy if no rule found
        }

        public int GetAdditionalLanguagesByIntelligence(int intelligenceScore)
        {
            AdditionalLanguagesByIntelligence? rule = _gameSystemData.Rules.AdditionalLanguagesByIntelligence
                .FirstOrDefault(r => r.int_score == intelligenceScore || (r.int_min <= intelligenceScore && r.int_max >= intelligenceScore));
            return rule?.languages ?? 0; // Return 0 if no rule found
        }

        public string GetHitDiceType(string characterClass)
        {
            if (_gameSystemData.Rules.HitDiceType.TryGetValue(characterClass, out var hitDiceType))
            {
                return hitDiceType; // Return the hit dice type (e.g., D6, D8)
            }
            return "D6"; // Or throw an exception, depending on desired behavior
        }

        public int GetHitDiceModifiers(string characterClass, int level)
        {
            if (_gameSystemData.Rules.HitDiceModifiers.TryGetValue(characterClass, out var hitDiceModifiers))
            {
                var modifier = hitDiceModifiers.FirstOrDefault(m => level >= m.level_min && level <= m.level_max);
                if (modifier != null)
                {
                    return modifier.modifier; // Return the hit dice modifier for the class and level
                }
            }
            // Fallback if needed, e.g., a "Default" entry
            return 0; // Return 0 or throw an exception if no rule found
        }

        public int GetMaxLevel(string characterClass)
        {
            if (_gameSystemData.Rules.MaxLevel.TryGetValue(characterClass, out var maxLevel))
            {
                return maxLevel; // Return the maximum level for the class
            }
            return 0; // Or throw an exception, depending on desired behavior
        }

        public int GetXPForNextLevel(string characterClass, int currentLevel)
        {
            if (_gameSystemData.Rules.XPForNextLevel.TryGetValue(characterClass, out var xpRules))
            {
                var rule = xpRules.FirstOrDefault(r => r.current_level == currentLevel);
                if (rule != null)
                {
                    return rule.xp_required_for_next_level;
                }
            }
            // Fallback for default or unconfigured classes
            if (_gameSystemData.Rules.XPForNextLevel.TryGetValue("Default", out var defaultXpRules))
            {
                var defaultRule = defaultXpRules.FirstOrDefault(r => r.current_level == currentLevel);
                if (defaultRule != null)
                {
                    return defaultRule.xp_required_for_next_level;
                }
            }
            return 0; // Or throw an exception, depending on desired behavior
        }

        public float GetXPModifier(string characterClass, int score)
        {
            if (_gameSystemData.Rules.XPModifiersByClass.TryGetValue(characterClass, out var xpModifiers))
            {
                var modifier = xpModifiers.FirstOrDefault(m => score >= m.score_min && score <= m.score_max);
                if (modifier != null)
                {
                    return modifier.multiplier; // Return the XP modifier multiplier
                }
            }
            // Fallback for default or unconfigured classes
            if (_gameSystemData.Rules.XPModifiersByClass.TryGetValue("Default", out var defaultXpModifiers))
            {
                var defaultModifier = defaultXpModifiers.FirstOrDefault(m => score >= m.score_min && score <= m.score_max);
                if (defaultModifier != null)
                {
                    return defaultModifier.multiplier;
                }
            }
            return 1.0f; // Default multiplier if no rule found
        }

        public PrimeRequisitesRule GetPrimeRequisitesRule(string characterClassName)
        {
            if (_gameSystemData.Rules.PrimeRequisitesRule.TryGetValue(characterClassName, out var rule))
            {
                return rule; // Return the specific rule for the class
            }
            // Fallback to "Default" if the specific class rule is not found
            if (_gameSystemData.Rules.PrimeRequisitesRule.TryGetValue("Default", out var defaultRule))
            {
                return defaultRule;
            }
            return new PrimeRequisitesRule(); // Return an empty rule if no default exists
        }

        public int GetToHitAC(string characterClass, int level)
        {
            if (_gameSystemData.Rules.CombatToHitACByClass.TryGetValue(characterClass, out var toHitRules))
            {
                var rule = toHitRules.FirstOrDefault(r => level >= r.level_min && level <= r.level_max);
                if (rule != null)
                {
                    return rule.to_hit_ac0;
                }
            }
            // Fallback if needed, e.g., a "Default" entry
            return 19; // Return 0 or throw an exception if no rule found
        }

        public int GetToHitACC(string characterClass, int level)
        {
            if (_gameSystemData.Rules.CombatToHitACCByClass.TryGetValue(characterClass, out var toHitRules))
            {
                var rule = toHitRules.FirstOrDefault(r => level >= r.level_min && level <= r.level_max);
                if (rule != null)
                {
                    return rule.to_hit_acc0;
                }
            }
            // Fallback if needed, e.g., a "Default" entry
            return 0; // Return 0 or throw an exception if no rule found
        }

        public List<string> GetStartingLanguagesByClass(string characterClass)
        {
            if (_gameSystemData.Rules.StartingLanguagesByClass.TryGetValue(characterClass, out var languages))
            {
                return languages; // Return the list of starting languages for the class
            }
            // Fallback if needed, e.g., a "Default" entry
            return new List<string>(); // Return empty list or throw an exception if no rule found
        }

        public int[] GetSpellsPerLevel(string characterClass, int level)
        {
            if (_gameSystemData.Rules.SpellsPerLevelByClass.TryGetValue(characterClass, out var spellsPerLevel))
            {
                var spellRule = spellsPerLevel.FirstOrDefault(s => s.Level == level);
                if (spellRule != null)
                {
                    return spellRule.SpellsPerLevelList; // Return the array of spells per level
                }
            }
            // Fallback if needed, e.g., a "Default" entry
            return new int[] { 0, 0, 0, 0, 0, 0 }; // Return default or throw
        }

        public string GetMagicType(string className)
        {
            if(_gameSystemData.Rules.MagicTypeByClass.TryGetValue(className, out var magicType))
            {
                return magicType;
            }
            return "None"; // Default magic type if class not found
        }

        public string GetSpellType(string className)
        {
            if (_gameSystemData.Rules.SpellTypeByClass.TryGetValue(className, out var spellType))
            {
                return spellType; // Return the spell type for the class
            }
            return "None"; // Default spell type if class not found
        }

        public List<string> GetSpellList(string className, int spellLevel)
        {
            if (_gameSystemData.Rules.SpellLists.TryGetValue(className, out var spellLevels))
            {
                if (spellLevels.TryGetValue(spellLevel, out var spells))
                {
                    return spells; // Return the list of spells for the specified class and level
                }
            }
            // Fallback if needed, e.g., a "Default" entry
            return new List<string>(); // Return empty list or throw if no rule found
        }

        public SavingThrowValues GetSavingThrows(string characterClass, int level)
        {
            if (_gameSystemData.Rules.SavingThrowsByClassAndLevel.TryGetValue(characterClass, out var saveRules))
            {
                var rule = saveRules.FirstOrDefault(r => level >= r.level_min && level <= r.level_max);
                if (rule != null)
                {
                    return rule.saves;
                }
            }
            // Fallback if needed, e.g., a "Default" entry
            return new SavingThrowValues(); // Return default or throw
        }

        public string GetTurningResult(string className, int level, string monsterHD)
        {
            // Handle "11+" and "13+" for level
            string levelKey = level.ToString();
            if (level >= 11 && className == "Cleric")
            {
                levelKey = "11+";
            }
            else if (level >= 13 && className == "Paladin")
            {
                levelKey = "13+";
            }

            if (_gameSystemData.Rules.ClassTurningTable.Data.TryGetValue(levelKey, out var levelData))
            {
                if (levelData.TryGetValue(monsterHD, out var result))
                {
                    return result;
                }
            }
            return "-"; // Default to failure if entry not found
        }

        public float GetCoinConversionRate(string coinType)
        {
            if (_gameSystemData.Rules.CoinConversionRates.TryGetValue(coinType, out var conversionRate))
            {
                return conversionRate; // Return the conversion rate for the coin type
            }
            return 1.0f; // Default conversion rate if not found (e.g., 1:1 for gold)
        }
    }

    public class GameSystemConfiguration
    {
        public string GameSystemName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public GameSystemRules Rules { get; set; } = new GameSystemRules();
        public GameSystemEnums Enums { get; set; } = new GameSystemEnums();
        public GameSystemSpells Spells { get; set; } = new GameSystemSpells();
        public GameSystemMonsters Monsters { get; set; } = new GameSystemMonsters();
    }

    public class GameSystemEnums
    {
        [JsonPropertyName("AbilityType")]
        public List<string> AbilityTypes { get; set; } = new List<string>();

        [JsonPropertyName("Alignment")]
        public List<string> Alignments { get; set; } = new List<string>();

        [JsonPropertyName("Languages")]
        public List<string> Languages { get; set; } = new List<string>();

        [JsonPropertyName("CharacterClass")]
        public List<string> CharacterClasses { get; set; } = new List<string>();

        [JsonPropertyName("CharacterRace")]
        public List<string> CharacterRaces { get; set; } = new List<string>();

        [JsonPropertyName("MagicType")]
        public List<string> MagicTypes { get; set; } = new List<string>();

        [JsonPropertyName("SpellType")]
        public List<string> SpellTypes { get; set; } = new List<string>();

        [JsonPropertyName("ArmourType")]
        public List<string> ArmourTypes { get; set; } = new List<string>();

        [JsonPropertyName("CoinType")]
        public List<string> CoinTypes { get; set; } = new List<string>();

        // Add methods for quick lookup (e.g., using HashSets for O(1) average lookup)
        private Dictionary<string, HashSet<string>> _enumValueSets = new Dictionary<string, HashSet<string>>();

        public void InitializeLookupSets()
        {
            _enumValueSets["AbilityType"] = new HashSet<string>(AbilityTypes, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["Alignment"] = new HashSet<string>(Alignments, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["Languages"] = new HashSet<string>(Languages, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["CharacterClass"] = new HashSet<string>(CharacterClasses, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["CharacterRace"] = new HashSet<string>(CharacterRaces, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["MagicType"] = new HashSet<string>(MagicTypes, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["SpellType"] = new HashSet<string>(SpellTypes, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["ArmourType"] = new HashSet<string>(ArmourTypes, StringComparer.OrdinalIgnoreCase);
            _enumValueSets["CoinType"] = new HashSet<string>(CoinTypes, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsValidEnumValue(string enumTypeName, string value)
        {
            if (_enumValueSets.TryGetValue(enumTypeName, out var validValues))
            {
                return validValues.Contains(value);
            }
            return false; // Unknown enum type
        }

        public List<string> GetValidValues(string enumTypeName)
        {
            if (_enumValueSets.TryGetValue(enumTypeName, out var validValues))
            {
                return validValues.ToList(); // Return a copy of the list
            }
            return new List<string>(); // Unknown enum type
        }
    }

    public class GameSystemRules
    {
        public List<MaxRetainersByCharisma> MaxRetainersByCharisma { get; set; } = new List<MaxRetainersByCharisma>();
        public List<RetainerLoyaltyByCharisma> RetainerLoyaltyByCharisma { get; set; } = new List<RetainerLoyaltyByCharisma>();
        public List<NpcReactionBonusByCharisma> NpcReactionBonusByCharisma { get; set; } = new List<NpcReactionBonusByCharisma>();
        public List<OpenDoorChanceByStrength> OpenDoorChanceByStrength { get; set; } = new List<OpenDoorChanceByStrength>();
        public List<LiteracyByIntelligence> LiteracyByIntelligence { get; set; } = new List<LiteracyByIntelligence>();
        public List<AdditionalLanguagesByIntelligence> AdditionalLanguagesByIntelligence { get; set; } = new List<AdditionalLanguagesByIntelligence>();
        public List<AbilityScoreModifierRule> AbilityScoreModifiers { get; set; } = new List<AbilityScoreModifierRule>();
        public Dictionary<string, string> HitDiceType { get; set; } = new Dictionary<string, string>(); // Maps class names to hit dice type (e.g., D4, D6, D8)
        public Dictionary<string, List<HitDiceModifier>> HitDiceModifiers { get; set; } = new Dictionary<string, List<HitDiceModifier>>(); // Maps class names to hit dice modifiers by level
        public Dictionary<string, int> MaxLevel { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, List<XPModifierRules>> XPModifiersByClass { get; set; } = new Dictionary<string, List<XPModifierRules>>(); // Maps class names to XP modifier rules
        public Dictionary<string, List<XPForNextLevel>> XPForNextLevel { get; set; } = new Dictionary<string, List<XPForNextLevel>>(); // Maps class names to XP required for next level
        public Dictionary<string, PrimeRequisitesRule> PrimeRequisitesRule { get; set; } = new Dictionary<string, PrimeRequisitesRule>(); // Maps class names to prime requisites
        public Dictionary<string, List<CombatToHitAC>> CombatToHitACByClass { get; set; } = new Dictionary<string, List<CombatToHitAC>>(); // Maps class names to combat to-hit rules
        public Dictionary<string, List<CombatToHitACC>> CombatToHitACCByClass { get; set; } = new Dictionary<string, List<CombatToHitACC>>(); // Maps class names to combat to-hit rules for ascending AC
        public Dictionary<string, List<string>> StartingLanguagesByClass { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, string> MagicTypeByClass { get; set; } = new Dictionary<string, string>(); // Maps class names to magic types (e.g., Arcane, Divine)
        public Dictionary<string, string> SpellTypeByClass { get; set; } = new Dictionary<string, string>(); // Maps class names to spell types
        public Dictionary<string, List<SpellsPerLevel>> SpellsPerLevelByClass { get; set; } = new Dictionary<string, List<SpellsPerLevel>>(); // Maps class names to spells per level
        public Dictionary<string, Dictionary<int, List<string>>> SpellLists { get; set; } = new Dictionary<string, Dictionary<int, List<string>>>();
        public Dictionary<string, List<SavingThrowRule>> SavingThrowsByClassAndLevel { get; set; } = new Dictionary<string, List<SavingThrowRule>>();
        public Dictionary<string, float> CoinConversionRates { get; set; } = new Dictionary<string, float>(); // Maps coin types to conversion rates to gold
        public ClassTurningTable ClassTurningTable { get; set; } = new ClassTurningTable(); // Turning table for clerics and paladins
    }

    public class GameSystemSpells
    {
        public Dictionary<string, List<Spell>> SpellsByClass { get; set; } = new Dictionary<string, List<Spell>>();
    }

    public class GameSystemMonsters
    {
        public Dictionary<string, Monster> Monsters { get; set; } = new Dictionary<string, Monster>();
    }    

    public class MaxRetainersByCharisma
    {
        public int cha_score { get; set; } = 0; // For exact match
        public int cha_min { get; set; }
        public int cha_max { get; set; }
        public int max_retainers { get; set; } = 0;
    }

    public class RetainerLoyaltyByCharisma
    {
        public int cha_score { get; set; } = 0; // For exact match
        public int cha_min { get; set; }
        public int cha_max { get; set; }
        public int loyalty { get; set; } = 0;
    }

    public class NpcReactionBonusByCharisma
    {
        public int cha_min { get; set; }
        public int cha_max { get; set; }
        public int bonus { get; set; } = 0;
    }

    public class OpenDoorChanceByStrength
    {
        public int str_min { get; set; }
        public int str_max { get; set; }
        public string chance { get; set; } = "1-in-6";
    }

    public class LiteracyByIntelligence
    {
        public int int_min { get; set; }
        public int int_max { get; set; }
        public string literacy { get; set; } = "UNKOWN";
    }

    public class AdditionalLanguagesByIntelligence
    {
        public int int_score { get; set; } = 0; // For exact match
        public int int_min { get; set; }
        public int int_max { get; set; }
        public int languages { get; set; } = 0;
    }

    public class AbilityScoreModifierRule
    {
        public int value_min { get; set; }
        public int value_max { get; set; }
        public int modifier { get; set; } = 0;
    }

    public class HitDiceModifier
    {
        public int level_min { get; set; }
        public int level_max { get; set; }
        public int modifier { get; set; } = 0; // Modifier to apply to hit dice
    }

    public class XPForNextLevel
    {
        public int current_level { get; set; }
        public int xp_required_for_next_level { get; set; } // XP required to reach the next level
    }

    public class XPModifierRules
    {
        public int score_min { get; set; }
        public int score_max { get; set; }
        public float multiplier { get; set; } = 1.0f; // Default multiplier is 1.0 (no change)
    }

    public class PrimeRequisitesRule
    {
        public string? primary_ability { get; set; }
        public string? secondary_ability { get; set; } // Optional secondary ability
        public List<PrimeRequisitesConditions> Conditions { get; set; } = new List<PrimeRequisitesConditions>();
    }

    public class PrimeRequisitesConditions
    {
        public int? int_min { get; set; } = null; // Minimum Intelligence
        public int? str_min { get; set; } = null; // Minimum Strength
        public int? dex_min { get; set; } = null; // Minimum Dexterity
        public int? cha_min { get; set; } = null; // Minimum Charisma
        public int? wis_min { get; set; } = null; // Minimum Wisdom
        public int? con_min { get; set; } = null; // Minimum Constitution
        public float multiplier { get; set; } = 1.0f; // Multiplier for XP or other effects
    }

    public class CombatToHitAC
    {
        public int level_min { get; set; }
        public int level_max { get; set; }
        public int to_hit_ac0 { get; set; } // To hit AC 0
    }

    public class CombatToHitACC
    {
        public int level_min { get; set; }
        public int level_max { get; set; }
        public int to_hit_acc0 { get; set; } // To hit Ascneding AC bonus
    }

    public class SavingThrowRule
    {
        public int level_min { get; set; }
        public int level_max { get; set; }
        public SavingThrowValues saves { get; set; } = new SavingThrowValues();
    }

    public class SavingThrowValues
    {
        public int DeathPoison { get; set; }
        public int Wands { get; set; }
        public int ParalysisPetrify { get; set; }
        public int BreathAttacks { get; set; }
        public int SpellsRodsStaves { get; set; }
    }

    public class SpellsPerLevel
    {
        public int Level { get; set; }
        public int[] SpellsPerLevelList { get; set; } = new int[] { 0, 0, 0, 0, 0, 0 }; // List of spells per level, indexed by spell level
    }
    public class ClassTurningTable
    {
        public List<string> ClericLevels { get; set; } = new List<string>();
        public List<string> MonsterHitDiceCategories { get; set; } = new List<string>();
        public Dictionary<string, Dictionary<string, string>> Data { get; set; } = new Dictionary<string, Dictionary<string, string>>();
    }
}