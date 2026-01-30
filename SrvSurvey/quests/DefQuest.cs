namespace SrvSurvey.quests;

/// <summary> A static definition of a quest </summary>
public class DefQuest
{
    public required string id;
    public required float ver;
    public required string publisher;
    public required string title;
    public string? desc;

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
}
