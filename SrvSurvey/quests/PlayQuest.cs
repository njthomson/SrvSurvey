using Lua;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey;
using SrvSurvey.game;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey.quests;

public class PlayQuest
{
    [JsonIgnore, AllowNull] internal PlayState parent;
    [JsonIgnore, AllowNull] public DefQuest quest;
    [JsonIgnore] public string? watchFolder;
    [JsonIgnore] public bool dev;
    [JsonIgnore] public bool dirty;
    [JsonIgnore] public PlayChapter? invokingChapter;

    #region data members

    public required string publisher;
    public required string id;
    public required double ver;

    /// <summary> The state of objectives </summary>
    public Dictionary<string, PlayObjective> objectives = [];

    /// <summary> When this quest began </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTimeOffset? startTime;

    /// <summary> When this quest ended </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTimeOffset? endTime;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool paused;

    /// <summary> The set of 'tags' (system or station names) to highlight in various overlays </summary>
    public HashSet<string> tags = [];

    /// <summary> The set of actively tracked body locations </summary>
    public Dictionary<string, LatLong3> bodyLocations = [];

    /// <summary> The current state of all chapters </summary>
    public HashSet<PlayChapter> chapters = [];

    /// <summary> Delivered messages </summary>
    public List<PlayMsg> msgs = [];

    /// <summary> The set of actively tracked body locations </summary>
    public Dictionary<string, LuaValue> vars = [];

    /// <summary> A map of last seen journal events </summary>
    //[JsonConverter(typeof(PlayQuest.KeptLastsJsonConverter))] // TODO: <<----
    public Dictionary<string, LuaTable?> keptLasts = [];

    #endregion

    /// <summary> Returns true if this quest is currently active </summary>
    [JsonIgnore] public bool active => !endTime.HasValue && startTime.HasValue;

    [JsonIgnore] public int unreadMessageCount => msgs.Count(m => !m.read);

    public override string ToString()
    {
        return $"questId: {id}";
    }

    /// <summary> Called by Game.cs so chapters may process journal events as they happen </summary>
    public async Task<bool> processJournalEntry(LuaTable entry, JObject raw)
    {
        this.dirty = false;

        // trigger functions on active chapters
        var triggered = false;
        var activeChapters = chapters.Where(pc => pc.active).ToList();
        foreach (var pc in activeChapters)
            triggered |= await pc.processJournalEntry(entry, raw);

        // keep last references? (always after processing)
        var eventName = entry["event"].ToString();
        if (keptLasts.ContainsKey(eventName) || eventName == "Docked" || eventName == "FSDJump")
        {
            keptLasts[eventName] = entry;
            dirty = true;
        }

        // do any pending operations - these we do after everything above
        foreach (var pc in activeChapters)
            pc.doPendings();

        return updateIfDirty(triggered);
    }

    /// <summary> Returns true if quest states were dirty, after saving to a file </summary>
    public bool updateIfDirty(bool force = false)
    {
        if (!dirty && !force) return false;

        foreach (var pc in chapters)
            if (pc.active)
                pc.pullScriptVars();

        this.parent.Save();

        PlayState.updateUI(this);

        dirty = false;
        return true;
    }

    public void complete()
    {
        Game.log($"[{this.id}] PQ.complete");
        this.endTime = DateTimeOffset.UtcNow;
        this.dirty = true;
    }

    public void fail()
    {
        Game.log($"[{this.id}] PQ.fail");
        this.endTime = DateTimeOffset.UtcNow;
        this.dirty = true;

        /* TODO: what does it really mean for the whole quest to be completed successfully or not?
         * Maybe we have `string endReason` with something descriptive for players to know success or not
         */
    }

    public async Task startChapter(string id)
    {
        Game.log($"[{this.id}] PQ.startChapter: {id}");
        var chapter = chapters.FirstOrDefault(c => c.id == id);
        if (chapter == null) throw new Exception($"Bad chapter id: {id}");

        if (chapter.active) return;

        await chapter.start();
        dirty = true;
    }

    public async Task stopChapter(string id)
    {
        Game.log($"[{this.id}] PQ.stopChapter: {id}");
        var chapter = chapters.FirstOrDefault(c => c.id == id);
        if (chapter == null) throw new Exception($"Bad chapter id: {id}");

        // TODO: check if this is the only running chapter?

        if (!chapter.active) return;

        chapter.stop();

        dirty = true;
    }

    public void setVar(string name, LuaValue val)
    {
        Game.log($"[{this.id}] PQ.setVar: {name}");

        if (val == LuaValue.Nil)
        {
            // remove if setting Nil
            dirty |= vars.Remove(name);
        }
        else if (!vars.ContainsKey(name) || vars[name] != val)
        {
            vars[name] = val;
            dirty = true;
        }
    }

    public LuaValue getVar(string name)
    {
        return vars.GetValueOrDefault(name);
    }

    public void sendMsg(PlayMsg newMsg)
    {
        Game.log($"[{this.id}] PQ.sendMsg: {newMsg.id}");
        newMsg.parent = this;
        if (newMsg.from == null && quest.msgs.Find(m => m.id == newMsg.id) == null)
            throw new Exception($"Bad message: {newMsg.id}");

        // remove any prior entries with the same id
        msgs.RemoveAll(m => m.id == newMsg.id);

        msgs.Add(newMsg);
        dirty = true;
    }

    public PlayMsg? getMsg(string id)
    {
        return msgs.Find(x => x.id == id);
    }

    public bool deleteMsg(string id)
    {
        Game.log($"[{this.id}] PQ.deleteMsg: {id}");
        var count = msgs.RemoveAll(m => m.id == id);
        dirty |= count > 0;

        PlayState.updateUI(this);

        return count > 0;
    }

    /// <summary> Called by Quest Comms when a player hit a message reply button </summary>
    public async Task invokeMessageAction(string msgId, string actionId)
    {
        Game.log($"[{this.id}] PQ.invokeMessageAction: {msgId}");
        var pm = msgs.Find(m => m.id == msgId);
        if (pm == null) throw new Exception($"Message not found, id: {msgId}");
        var chapterId = pm.chapter!;

        var chapter = chapters.FirstOrDefault(c => c.id == chapterId);
        if (chapter == null) throw new Exception($"Bad chapter id: {id}");

        await chapter.invokeMessageAction(msgId, actionId);
    }


    public void keepLast(params string?[] names)
    {
        Game.log($"[{this.id}] PQ.keepLast: {string.Join(',', names)}");
        // confirm something is changing
        if (names.Length == keptLasts.Count && keptLasts.Keys.All(n => string.IsNullOrWhiteSpace(n) || names.Contains(n)))
            return;

        var newKeptLasts = new Dictionary<string, LuaTable?>();

        // (keeping any prior references)
        foreach (var name in names)
            if (!string.IsNullOrWhiteSpace(name))
                newKeptLasts[name] = this.keptLasts.GetValueOrDefault(name);

        this.keptLasts = newKeptLasts;
        dirty = true;
    }

    public LuaValue getLast(string eventName)
    {
        // TODO: check if eventName is valid?

        var last = keptLasts.GetValueOrDefault(eventName);
        if (last == null)
            return LuaValue.Nil;
        else
            return last;
    }

    /*
    class KeptLastsJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType) { return false; }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var raw = serializer.Deserialize<Dictionary<string, JObject?>>(reader);
            if (raw == null) throw new Exception($"Unexpected value: ");

            var keptLasts = raw.ToDictionary(kv => kv.Key, kv => JournalFile.hydrate(kv.Value) as JournalEntry);
            return keptLasts;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var keptLasts = value as Dictionary<string, JournalEntry?>;
            if (keptLasts == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

            // serialize each kept entry as a single line (even though we generally serialize indented)
            writer.WriteStartObject();
            foreach (var (key, entry) in keptLasts)
            {
                writer.WritePropertyName(key);

                if (entry == null)
                    writer.WriteNull();
                else
                    writer.WriteRawValue(JsonConvert.SerializeObject(entry));
            }
            writer.WriteEndObject();
        }
    }
    */
}
