using System.Reflection;
using System.Text.Json.Serialization;
using GenAIExpertEngineAPI.Controllers;
using GenAIExpertEngineAPI.Services;
using GenerativeAI.Types;


namespace GenAIExpertEngineAPI
{
    // This attribute tells the source generator about the types we need to serialize/deserialize.
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(object[]))]
    [JsonSerializable(typeof(UserQueryRequest))]
    [JsonSerializable(typeof(ChatMessage))]
    [JsonSerializable(typeof(QueryStep))]
    [JsonSerializable(typeof(List<QueryStep>))]
    [JsonSerializable(typeof(FactCheckRequest))]
    [JsonSerializable(typeof(List<FactCheckRequest>))]
    [JsonSerializable(typeof(ExpertQueryResponse))]
    [JsonSerializable(typeof(RefereeResponseOutput))]
    [JsonSerializable(typeof(Candidate))]
    [JsonSerializable(typeof(ParameterInfo))]
    [JsonSerializable(typeof(Part))]
    [JsonSerializable(typeof(CharacterState))]
    [JsonSerializable(typeof(Spell))]
    [JsonSerializable(typeof(Monster))]


    public partial class ApplicationJsonSerializerContext : JsonSerializerContext
    {
    }
}
