using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey.quests;

/// <summary> A runtime/delivered message. Content fields will be null unless they are overriding statically declared values </summary>
public class PlayMsg
{
    [JsonIgnore, AllowNull] public PlayQuest parent;

    public string id;
    public DateTimeOffset received;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? from;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? subject;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? body;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? chapter;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[]? actions;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool read;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? replied;

    public override string ToString()
    {
        return $"{id}:{subject ?? body} ({received:z})";
    }
}
