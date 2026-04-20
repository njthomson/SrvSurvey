using Lua;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey;
using SrvSurvey.game;
using System.Diagnostics.CodeAnalysis;
using System.Media;

namespace SrvSurvey.quests;

public class PlayQuest
{
    [JsonIgnore, AllowNull] internal PlayState parent;
    [JsonIgnore, AllowNull] public DefQuest quest;
    [JsonIgnore] public string? watchFolder;
    [JsonIgnore] public bool dev;
    [JsonIgnore] public bool dirty;
    [JsonIgnore] public PlayChapter? invokingChapter;
    [JsonIgnore] public readonly HashSet<string> chaptersToStart = [];
    [JsonIgnore] public readonly HashSet<string> chaptersToStop = [];

    #region data members

    public string publisher => quest?.publisher!;
    public string id => quest?.id!;
    public double ver => quest?.ver ?? 0;

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
    [JsonConverter(typeof(PlayQuest.KeptLastsJsonConverter))]
    public Dictionary<string, JObject?> keptLasts = [];

    public List<PlayRoute> routes = [];

    #endregion

    /// <summary> Returns true if this quest is currently active </summary>
    [JsonIgnore] public bool active => !endTime.HasValue && startTime.HasValue;

    [JsonIgnore] public int unreadMessageCount => msgs.Count(m => !m.read);

    public override string ToString()
    {
        return $"{publisher}|{id}|{ver}";
    }

    private void log(string msg)
    {
        Game.log($"[{this.id}/{this.invokingChapter?.id}/{this.invokingChapter?.invokingFunc}] {msg}");
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
            keptLasts[eventName] = raw;
            dirty = true;
        }

        // do any pending operations - these we do after everything above
        foreach (var pc in activeChapters)
            pc.doPendings();

        return await updateIfDirty(triggered);
    }

    /// <summary> Returns true if quest states were dirty, after saving to a file </summary>
    public async Task<bool> updateIfDirty(bool force = false)
    {
        if (!dirty && !force) return false;

        if (chaptersToStop.Count > 0)
            await stopChapters();
        if (chaptersToStart.Count > 0)
            await startChapters();

        foreach (var pc in chapters)
            if (pc.active)
                pc.pullScriptVars();

        this.parent.Save(false);

        PlayState.updateUI(this);

        dirty = false;
        return true;
    }

    public void complete()
    {
        this.log("PQ.complete");
        this.endTime = DateTimeOffset.UtcNow;
        this.dirty = true;
    }

    public void fail()
    {
        this.log("PQ.fail");
        this.endTime = DateTimeOffset.UtcNow;
        this.dirty = true;

        /* TODO: what does it really mean for the whole quest to be completed successfully or not?
         * Maybe we have `string endReason` with something descriptive for players to know success or not
         */
    }

    public void startChapter(string id)
    {
        chaptersToStart.Add(id);
        dirty = true;
    }

    public async Task startChapters()
    {
        var id = chaptersToStart.FirstOrDefault();
        while (id != null)
        {
            this.log($"PQ.startChapter: {id}");
            var chapter = chapters.FirstOrDefault(c => c.id == id);
            if (chapter == null) throw new Exception($"Bad chapter id: {id}");

            if (!chapter.active)
                await chapter.start();

            chaptersToStart.Remove(id);
            id = chaptersToStart.FirstOrDefault();
        }
    }

    public void stopChapter(string id)
    {
        chaptersToStop.Add(id);
        dirty = true;
    }

    private async Task stopChapters()
    {
        var id = chaptersToStop.FirstOrDefault();
        while (id != null)
        {
            this.log($"PQ.stopChapter: {id}");
            var chapter = chapters.FirstOrDefault(c => c.id == id);
            if (chapter == null) throw new Exception($"Bad chapter id: {id}");

            // TODO: check if this is the only running chapter?

            if (chapter.active)
                chapter.stop();

            chaptersToStop.Remove(id);
            id = chaptersToStop.FirstOrDefault();
        }
    }

    public void setVar(string name, LuaValue val)
    {
        this.log($"PQ.setVar: {name}");

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
        if (!vars.ContainsKey(name))
            this.log($"PQ.getVar: NOT STORED: '{name}'");

        return vars.GetValueOrDefault(name);
    }

    public void sendMsg(PlayMsg newMsg)
    {
        this.log($"PQ.sendMsg: {newMsg.id}");
        newMsg.parent = this;
        if (newMsg.from == null && quest.msgs.Find(m => m.id == newMsg.id) == null)
            throw new Exception($"Bad message: {newMsg.id}");

        // remove any prior entries with the same id
        msgs.RemoveAll(m => m.id == newMsg.id);

        msgs.Add(newMsg);
        dirty = true;

        playChime();
    }

    public static void playChime()
    {
        var filepath = @"C:\Windows\Media\Windows Proximity Notification.wav";
        if (File.Exists(filepath))
        {
            using (SoundPlayer player = new SoundPlayer(filepath))
            {
                player.Play();
            }
        }
    }

    public PlayMsg? getMsg(string id)
    {
        return msgs.Find(x => x.id == id);
    }

    public bool deleteMsg(string id)
    {
        this.log($"PQ.deleteMsg: {id}");
        var count = msgs.RemoveAll(m => m.id == id);
        dirty |= count > 0;

        PlayState.updateUI(this);

        return count > 0;
    }

    /// <summary> Called by Quest Comms when a player hit a message reply button </summary>
    public async Task invokeMessageAction(string msgId, string actionId)
    {
        this.log($"PQ.invokeMessageAction: {msgId}");
        var pm = msgs.Find(m => m.id == msgId);
        if (pm == null) throw new Exception($"Message not found, id: {msgId}");
        var chapterId = pm.chapter!;

        var chapter = chapters.FirstOrDefault(c => c.id == chapterId);
        if (chapter == null) throw new Exception($"Bad chapter id: {id}");

        await chapter.invokeMessageAction(msgId, actionId);
    }


    public void keepLast(HashSet<string> names)
    {
        names.Add(nameof(Docked));
        names.Add(nameof(FSDJump));

        this.log($"PQ.keepLast: {string.Join(',', names)}");
        // confirm something is changing
        if (names.Count == keptLasts.Count && keptLasts.Keys.All(n => names.Contains(n)))
            return;

        var newKeptLasts = new Dictionary<string, JObject?>();

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
        if (!keptLasts.ContainsKey(eventName))
            this.log($"PQ.getLast: NOT TRACKING: '{eventName}'");

        var last = keptLasts.GetValueOrDefault(eventName);
        if (last == null)
            return LuaValue.Nil;
        else
            return last.toTbl();
    }

    class KeptLastsJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType) { return false; }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize<Dictionary<string, JObject?>>(reader);
            if (obj == null) throw new Exception($"Unexpected value: ");

            return obj;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var keptLasts = value as Dictionary<string, JObject?>;
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

    public class PlayRoute
    {
        public string id;
        public double w;
        public List<double[]> wp;
    }

    public class Comparer : IEqualityComparer<PlayQuest>
    {
        public bool Equals(PlayQuest? x, PlayQuest? y)
        {
            return x?.ToString() == y?.ToString();
        }

        public int GetHashCode(PlayQuest pq)
        {
            return pq.ToString().GetHashCode();
        }
    }
}
