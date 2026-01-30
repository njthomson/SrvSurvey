using Newtonsoft.Json;

namespace SrvSurvey.quests;

/// <summary> A static definition of a message that can be delivered </summary>
public class DefMsg
{
    public required string id;
    public required string from;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? subject;
    public required string body;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Dictionary<string, string>? actions;
}
