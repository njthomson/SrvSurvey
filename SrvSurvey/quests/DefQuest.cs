using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SrvSurvey.quests;

/// <summary> A static definition of a quest </summary>
public class DefQuest
{
    public required string id;
    public required double ver;
    public required string publisher;
    public required string title;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? subTitle;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? desc;

    public HashSet<string>? tags;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public QuestDuration duration;
    /// <summary> Only show for members of these squadrons </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public HashSet<string>? onlySquadrons;
    /// <summary> Only show for these cmdrs </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public HashSet<string>? onlyCmdrs;
    /// <summary> Do not show in the catalog </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool hidden;

    /// <summary> The ID of the chapter to run when the quest is initialized </summary>
    public required string firstChapter;
    /// <summary> All potential objectives </summary>
    public Dictionary<string, string> objectives = [];
    /// <summary> A map of arbitrary ID to display-strings. (One day this would be extended to support multi-languages) </summary>
    public Dictionary<string, string> strings = [];
    /// <summary> All potential messages </summary>
    public List<DefMsg> msgs = [];
    /// <summary> A map of chapter IDs to their script code </summary>
    public Dictionary<string, string> chapters = [];

    public override string ToString()
    {
        return $"quest:{id} '{title}'";
    }

    public static Dictionary<QuestDuration, string> mapQuestDuration = new()
    {
        { QuestDuration.Unknown , "No value specified" },
        { QuestDuration.Short , "Within an evening" },
        { QuestDuration.Medium , "Needs a few days" },
        { QuestDuration.Long , "Might be a week or two" },
        { QuestDuration.Extended , "It may never end" },
    };

    public bool equals(DefQuest? other)
    {
        return this.publisher == other?.publisher && this.id == other?.id;
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum QuestDuration
{
    Unknown,
    Short,
    Medium,
    Long,
    Extended
}
