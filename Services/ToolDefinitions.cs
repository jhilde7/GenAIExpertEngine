using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenAIExpertEngineAPI.Services
{
    public class ToolDefinitions
    {
        private static readonly Schema AbilityScoreValueSchema = new Schema
        {
            Type = "object",
            Properties = new Dictionary<string, Schema>
            {
                { "Value", new Schema { Type = "integer", Description = "The numeric value for the ability score (e.g., 18)." } }
            },
            Required = new List<string> { "Value" } // 'Value' is always required for an AbilityScore object
        };
        

        public static List<Tool> GetCharacterManagementTools()
        {
            return new List<Tool>
            {
                new Tool
                {
                    FunctionDeclarations = new List<FunctionDeclaration>
                    {
                        // GetAbilityScoreValue tool
                        new FunctionDeclaration
                        {
                            Name = "GetAbilityScoreValue",
                            Description = "Retrieves the numeric value of a specific ability score (e.g., Strength, Dexterity) for the current character.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "abilityType", new Schema { Type = "string", Description = "The type of ability score to retrieve (e.g., 'Str', 'Dex', 'Con', 'Int', 'Wis', 'Cha'). Must match one of the AbilityType enum values." } }
                                },
                                Required = new List<string> { "conversationId", "abilityType" }
                            }
                        },
                        // GetCharacterClassName tool
                        new FunctionDeclaration
                        {
                            Name = "GetCharacterClassName",
                            Description = "Retrieves the name of the current character's class.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetCharacterRace tool
                        new FunctionDeclaration
                        {
                            Name = "GetCharacterRace",
                            Description = "Retrieves the name of the current character's race.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // UpdateAbilityScores tool
                        new FunctionDeclaration
                        {
                            Name = "UpdateAbilityScores",
                            Description = "Updates one or more ability scores for a character. Provide ability types as keys and objects with a 'Value' property as values.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "scores", new Schema
                                        {
                                            Type = "object",
                                            Description = "A dictionary where keys are AbilityType strings (e.g., 'Str', 'Dex') and values are objects with a 'Value' property (e.g., { 'Value': 18 }).",
                                            Properties = new Dictionary<string, Schema> // Explicitly list properties for common ability types
                                            {
                                                { "Str", AbilityScoreValueSchema }, // Using the reusable schema
                                                { "Dex", AbilityScoreValueSchema },
                                                { "Con", AbilityScoreValueSchema },
                                                { "Int", AbilityScoreValueSchema },
                                                { "Wis", AbilityScoreValueSchema },
                                                { "Cha", AbilityScoreValueSchema }
                                            },
                                        }
                                    }
                                },
                                Required = new List<string> { "conversationId", "scores" }
                            }
                        },
                        // UpdateAbilityScore tool
                        new FunctionDeclaration
                        {
                            Name = "UpdateAbilityScore",
                            Description = "Updates a single ability score for a character.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "abilityType", new Schema { Type = "string", Description = "The type of ability score to update (e.g., 'Str', 'Dex', 'Con', 'Int', 'Wis', 'Cha'). Must match one of the AbilityType enum values." } },
                                    { "value", new Schema { Type = "integer", Description = "The new numeric value for the ability score (e.g., 18)." } }
                                },
                                Required = new List<string> { "conversationId", "abilityType", "value" }
                            }
                        },
                        // UpdateCharacterClass tool
                        new FunctionDeclaration
                        {
                            Name = "UpdateCharacterClass",
                            Description = "Sets the player's character class. **CRITICAL: Use this tool when the player explicitly states their desired character class, like 'I want to be a Thief' or 'My character is a Wizard'.**", // Added emphasis
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "className", new Schema { Type = "string", Description = "The character's class name (e.g., 'Fighter', 'MagicUser', 'Cleric'). Must match one of the CharacterClass enum values." } }
                                },
                                Required = new List<string> { "conversationId", "className" }
                            }
                        },
                        // UpdateRace tool
                        new FunctionDeclaration
                        {
                            Name = "UpdateRace",
                            Description = "Sets the player's character race. **CRITICAL: Use this tool when the player explicitly states their desired character race, like 'I want to be a Drow' or 'My character is an Elf'.**", // Added emphasis
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "raceName", new Schema { Type = "string", Description = "The character's race name (e.g., 'Human', 'Elf', 'Dwarf'). Must match one of the CharacterRace enum values." } }
                                },
                                Required = new List<string> { "conversationId", "raceName" }
                            }
                        },
                        // UpdateCharacterName tool
                        new FunctionDeclaration
                        {
                            Name = "UpdateCharacterName",
                            Description = "Sets or updates the character's name.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "name", new Schema { Type = "string", Description = "The new name for the character." } }
                                },
                                Required = new List<string> { "conversationId", "name" }
                            }
                        },
                        // UpdateCharacterAlignment tool
                        new FunctionDeclaration
                        {
                            Name = "UpdateCharacterAlignment",
                            Description = "Sets or updates the character's alignment.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "alignment", new Schema { Type = "string", Description = "The character's alignment (e.g., 'Lawful', 'Chaotic', 'Neutral'). Must match one of the Alignment enum values." } }
                                },
                                Required = new List<string> { "conversationId", "alignment" }
                            }
                        },
                        // UpdateCharacterExperience tool (Corrected typo from UpdateChracterExperience)
                        new FunctionDeclaration
                        {
                            Name = "UpdateCharacterExperience",
                            Description = "Adds designated experience points to the character, which may lead to leveling up.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "experience", new Schema { Type = "integer", Description = "The amount of experience points to add (e.g., 1000)." } }
                                },
                                Required = new List<string> { "conversationId", "experience" }
                            }
                        },
                        // AddAdditionalLanguage tool
                        new FunctionDeclaration
                        {
                            Name = "AddAdditionalLanguage",
                            Description = "Adds an additional language to the character's known languages. Can be a specific language or a random one if not specified (defaults to random if 'Common' is passed).",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "languages", new Schema { Type = "string", Description = "The language to add (e.g., 'Elvish', 'Dwarvish'). If omitted or 'Common' is passed, a random non-secret language will be chosen. Must match one of the Languages enum values."} }
                                },
                                Required = new List<string> { "conversationId" } // Only conversationId is strictly required, 'languages' is optional
                            }
                        },
                        // GetAbilityScores tool
                        new FunctionDeclaration
                        {
                            Name = "GetAbilityScores",
                            Description = "Retrieves all ability scores (Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma) for a character.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetLiteracyState tool
                        new FunctionDeclaration
                        {
                            Name = "GetLiteracyState",
                            Description = "Retrieves the character's literacy status (Illiterate, Basic, Literate).",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetAdditionalLanguages tool
                        new FunctionDeclaration
                        {
                            Name = "GetAdditionalLanguages",
                            Description = "Retrieves the number of additional languages a character can learn based on their Intelligence.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetMaxRetainers tool (Corrected typo from GetmaxRetainers)
                        new FunctionDeclaration
                        {
                            Name = "GetMaxRetainers",
                            Description = "Retrieves the maximum number of retainers a character can have based on their Charisma score.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetRetainerLoyalty tool
                        new FunctionDeclaration
                        {
                            Name = "GetRetainerLoyalty",
                            Description = "Retrieves the current loyalty score of retainers for a character based on their Charisma score.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetNpcReactionBonus tool
                        new FunctionDeclaration
                        {
                            Name = "GetNpcReactionBonus",
                            Description = "Retrieves the NPC (Non-Player Character) reaction bonus for a character based on their Charisma score.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // GetOpenDoorChance tool
                        new FunctionDeclaration
                        {
                            Name = "GetOpenDoorChance",
                            Description = "Retrieves a character's chance to open stuck doors.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } }
                                },
                                Required = new List<string> { "conversationId" }
                            }
                        },
                        // RollDice tool
                        new FunctionDeclaration
                        {
                            Name = "RollDice",
                            Description = "Executes a dice roll for the game. Call this tool to perform dice rolls like 1d20 or 4d6, receiving the total result back. Use this whenever the user requests a specific dice roll.",
                            Parameters = new Schema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, Schema>
                                {
                                    { "conversationId", new Schema { Type = "string", Description = "The unique identifier for the current conversation/character." } },
                                    { "numberOfDice", new Schema { Type = "integer", Description = "The number of dice to roll." } },
                                    { "diceType", new Schema { Type = "string", Description = "The type of die to roll (e.g., 'D20', 'D6'). Must match one of the DiceType enum values." } }
                                },
                                Required = new List<string> { "numberOfDice", "diceType" }
                            }
                        }
                    }
                }
            };
        }
    }
}