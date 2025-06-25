using System.Text.Json.Serialization;

namespace GenAIExpertEngineAPI.Services
{
    public class MonsterState
    {

    }

    public class Monster
    {
        [JsonPropertyName("monster_name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("monster_type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("stat_block")]
        public StatBlock StatBlock { get; set; } = new StatBlock();
        [JsonPropertyName("special_abilities")]
        public List<string> SpecialAbilities { get; set; } = new List<string>();
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("habitat")]
        public string? Habitat { get; set; } // Optional habitat information
        [JsonPropertyName("social_organization")]
        public string? SocialOrganization { get; set; } // Optional social organization information

    }

    public class StatBlock
    {
        [JsonPropertyName("Armour_Class")]
        public string ArmourClass { get; set; } = "Unknown";
        [JsonPropertyName("Hit_Dice")]
        public string HitDice { get; set; } = "Unknown";
        [JsonPropertyName("Attacks_Usable_Per_Round")]
        public string AttacksUsablePerRound { get; set; } = "Unknown";
        [JsonPropertyName("Attack_Description")]
        public string AttackDescription { get; set; } = "Unknown";
        [JsonPropertyName("Attack_Roll_to_Hit_AC_0")]
        public string AttackRollToHitAC0 { get; set; } = "Unknown";
        [JsonPropertyName("Movement_Rate")]
        public string MovementRate { get; set; } = "Unknown";
        [JsonPropertyName("Movement_Rates")]
        public MovementRates MovementRates { get; set; } = new MovementRates();
        [JsonPropertyName("Saving_Throw_Values")]
        public SavingThrowValues SavingThrowValues { get; set; } = new SavingThrowValues();
        [JsonPropertyName("Morale_Rating")]
        public int MoraleRating { get; set; } = 0;
        [JsonPropertyName("Alignment")]
        public string Alignment { get; set; } = "Neutral";
        [JsonPropertyName("XP_Award")]
        public int XPAward { get; set; } = 0;
        [JsonPropertyName("Number_Appearing")]
        public string NumberAppearing { get; set; } = "Unknown";
        [JsonPropertyName("Number_Appearing_Details")]
        public NumberAppearingDetails NumberAppearingDetails { get; set; } = new NumberAppearingDetails();
        [JsonPropertyName("Treasure_Type")]
        public string TreasureType { get; set; } = "None";
        [JsonPropertyName("Treasure_Type_Code")]
        public string TreasureTypeCode { get; set; } = "None";
    }

    public class MovementRates
    {
        [JsonPropertyName("Base")]
        public int Base { get; set; } = 0; // Base movement rate in feet
        [JsonPropertyName("Encounter")]
        public int Encounter { get; set; } = 0; // Movement rate for encounters in feet
        [JsonPropertyName("Units")]
        public string Units { get; set; } = "feet"; // Units of measurement
    }
    public class NumberAppearingDetails
    {
        [JsonPropertyName("Dungeon_Wandering")]
        public string DungeonWandering { get; set; } = "Unknown"; // e.g., "2d4"
        [JsonPropertyName("Dungeon_Lair")]
        public string DungeonLair { get; set; } = "Unknown"; // e.g., "4d6"
        [JsonPropertyName("Wilderness_Wandering")]
        public string WildernessWandering { get; set; } = "Unknown"; // e.g., "4d6"
        [JsonPropertyName("Wilderness_Lair_Multiplier")]
        public string? WildernessLairMultiplier { get; set; }
    }
}
