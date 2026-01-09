using Newtonsoft.Json;

namespace SrvSurvey.quests
{
    public class Quest
    {
        public string id;
        public float ver;
        public string publisher;
        public string title;
        public string desc;
        public string? longDesc;
        public string firstChapter;
        public Dictionary<string, string> objectives = new();
        /// <summary> Potential messages </summary>
        public List<Msg> msgs = new();
        public Dictionary<string, string> chapters = new();

        public override string ToString()
        {
            return $"quest:{id} '{title}'";
        }
    }

    public class Msg
    {
        public string id;
        public string from;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? subject;
        public string body;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[]? actions;
    }
}
