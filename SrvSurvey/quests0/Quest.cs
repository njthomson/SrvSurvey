using Newtonsoft.Json;

namespace SrvSurvey.quests0
{
    /// <summary> A static definition of a quest </summary>
    public class Quest
    {
        public required string id;
        public required float ver;
        public required string publisher;
        public required string title;
        public string desc;

        /// <summary> The ID of the chapter to run when the quest is initialized </summary>
        public required string firstChapter;
        /// <summary> All potential objectives </summary>
        public Dictionary<string, string> objectives = [];
        /// <summary> A map of arbitrary ID to display-strings. (One day this would be extended to support multi-languages) </summary>
        public Dictionary<string, string> strings = [];
        /// <summary> All potential messages </summary>
        public List<Msg> msgs = [];
        /// <summary> A map of chapter IDs to their script code </summary>
        public Dictionary<string, string> chapters = [];

        public override string ToString()
        {
            return $"quest:{id} '{title}'";
        }
    }

    /// <summary> A static definition of a message that can be delivered </summary>
    public class Msg
    {
        public required string id;
        public required string from;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? subject;
        public required string body;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string>? actions;
    }
}
