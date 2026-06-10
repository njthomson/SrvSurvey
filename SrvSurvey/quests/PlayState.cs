using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.forms;
using SrvSurvey.forms.playComms;
using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.plotters;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Data = SrvSurvey.game.Data;

namespace SrvSurvey.quests;

// TODO: common code file?

internal class PlayState : Data
{
    #region static + loading code

    private static string folder = Path.Combine(Program.dataFolder, "quests");

    public static PlayState? current;

    public static Task<PlayState> loadAsync(string fid)
    {
        return Task.Run(async () =>
        {
            try
            {
                current = await PlayState.loadInner(fid);
                PlayState.updateUI();
            }
            catch (Exception ex)
            {
                Game.log($"PlayState.loadAsync: {ex}");
                Program.defer(() =>
                {
                    FormErrorSubmit.Show(ex);
                });
                throw;
            }
            return current;
        });
    }

    private static async Task<PlayState> loadInner(string fid)
    {
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var filepath = Path.Combine(folder, fid + ".json");
        PlayState? ps = null;
        if (!File.Exists(filepath))
        {
            // create new empty state
            ps = new PlayState()
            {
                fid = fid,
                cmdr = CommanderSettings.Load(fid, true, null).commander,
                filepath = filepath,
            };
            ps.Save(true);
        }
        else
        {
            // parse existing state
            ps = Data.Load<PlayState>(filepath)!;

            // temporary, just for a few weeks
            if (string.IsNullOrWhiteSpace(ps.cmdr))
            {
                ps.cmdr = CommanderSettings.Load(fid, true, null).commander;
                ps.Save(true);
            }

            // load state for all quests and hydrate them
            var loadedQuests = await Game.rcc.loadCmdrQuests(fid, game.RavenColonial.QuestState.active);
            Game.log($"Loaded {loadedQuests.Length} quests");
            foreach (var pq in loadedQuests)
            {
                if (pq.quest == null) { Debugger.Break(); continue; } // TODO: maybe throw or warn?
                await ps.initQuest(pq, false);
            }

            // and a devQuest?
            if (ps.devRef != null && ps.devQuest != null)
            {
                var questPath = Path.Combine(folder, $"dev-{ps.devRef.id}.json");
                if (!File.Exists(questPath)) throw new Exception($"Missing! {questPath}");
                var questJson = File.ReadAllText(questPath);
                ps.devQuest.quest = JsonConvert.DeserializeObject<DefQuest>(questJson)!;
                ps.devQuest.dev = true;

                await ps.initQuest(ps.devQuest, false);
            }
        }

        Game.log(ps.activeQuests.Select(pq => $"{pq.id}: {pq.quest.title}").formatWithHeader($"Initialized {ps.activeQuests.Count} active quests"));

        return ps;
    }

    public static void updateUI(PlayQuest? pq = null)
    {
        if (Program.control.InvokeRequired)
        {
            Program.defer(() => updateUI(pq));
            return;
        }
        PlotBase2.renderAll(null, true);
        BaseForm.get<FormPlayComms>()?.onQuestChanged(pq);
        BaseForm.get<FormPlayDev>()?.onQuestChanged(pq);
        BaseForm.get<FormPlayComms2>()?.Invalidate();
    }

    #endregion

    #region data members

    public string fid = "";
    public string cmdr = "";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public QuestRef? devRef;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public PlayQuest? devQuest;

    // TODO: store cmdr level variables?

    #endregion

    [JsonIgnore] public readonly List<PlayQuest> activeQuests = [];
    [JsonIgnore] public int messagesTotal => activeQuests.Sum(q => q.msgs.Count);
    [JsonIgnore] public int messagesUnread => activeQuests.Sum(pq => pq.unreadMessageCount);

    public override void Save()
    {
        Debugger.Break();
        this.Save(false); // ok
    }

    public void Save(bool localOnly)
    {
        base.Save();
        if (!localOnly)
        {
            // space this out so we're not hitting the server too often
            Debugger.Break(); // does this still happen?
        }
    }

    /// <summary> Onboard a cmdr to a new quest </summary>
    public async Task<PlayQuest> activateQuest(string publisher, string id)
    {
        var q = await Game.rcc.activateQuest(this.fid, publisher, id);
        if (q == null) throw new Exception($"Cannot activate quest by: {publisher} / {id}");
        Game.log($"Activating NEW quest: {q.publisher} / {q.id} / {q.id}");

        var pq = new PlayQuest()
        {
            parent = this,
            quest = q,
            startTime = DateTime.UtcNow,
            // quest must be null, so we download from the server
        };
        pq.chapters = q.chapters.Keys.Select(id => new PlayChapter(id, pq)).ToHashSet(); // what about this?

        await initQuest(pq, true);

        PlayState.updateUI(pq);
        FormPlayComms2.fetchCmdrQuests(this.fid).justDoIt();
        return pq;
    }

    public async Task<PlayQuest> resumeQuest(string publisher, string id)
    {
        var success = await Game.rcc.setQuestState(this.fid, publisher, id, QuestState.active);
        if (!success) throw new Exception($"Cannot resume quest by: {publisher} / {id}");

        var loadedQuests = await Game.rcc.loadCmdrQuests(fid, game.RavenColonial.QuestState.active);
        var pq = loadedQuests.FirstOrDefault(q => q.publisher == publisher && q.id == id);
        if (pq == null) throw new Exception($"Cannot find quest by: {publisher} / {id}");
        Game.log($"Resuming quest: {pq.publisher} / {pq.id} / {pq.ver}");

        await initQuest(pq, false);

        PlayState.updateUI(pq);
        FormPlayComms2.fetchCmdrQuests(this.fid).justDoIt();
        return pq;
    }

    public async Task removeQuest(PlayQuest pq, QuestState newState)
    {
        if (!pq.dev)
        {
            var success = newState == QuestState.unknown
                ? await Game.rcc.deleteQuest(this.fid, pq.publisher, pq.id)
                : await Game.rcc.setQuestState(this.fid, pq.publisher, pq.id, newState);

            // update server if not a devQuest
            if (success)
                this.activeQuests.Remove(pq);
            else
                Debugger.Break(); // would this ever happen?
        }
        else if (pq.dev || (this.devQuest != null && this.devQuest.ToString() == pq.ToString()))
        {
            // otherwise, remove and save local file
            this.devRef = null;
            this.devQuest = null;
            this.Save(true);
            this.activeQuests.Remove(pq);
        }
        else
        {
            Debugger.Break(); // would this ever happen?
        }

        FormPlayComms2.fetchCmdrQuests(this.fid).justDoIt();
    }

    private static void setPriorKepts(PlayQuest pq)
    {
        if (!pq.keptLasts.ContainsKey(nameof(Docked)))
        {
            Game.activeGame?.journals?.walkDeep(true, entry =>
            {
                if (entry is Docked)
                {
                    pq.keptLasts[nameof(Docked)] = JObject.FromObject(entry);
                    return true;
                }
                return false;
            });
        }
        if (!pq.keptLasts.ContainsKey(nameof(FSDJump)))
        {
            Game.activeGame?.journals?.walkDeep(true, entry =>
            {
                if (entry is FSDJump)
                {
                    pq.keptLasts[nameof(FSDJump)] = JObject.FromObject(entry);
                    return true;
                }
                return false;
            });
        }
    }

    private async Task initQuest(PlayQuest pq, bool startFirstChapterAndSave)
    {
        pq.parent = this;

        // fetch always last known's
        setPriorKepts(pq);

        foreach (var pm in pq.msgs)
            pm.parent = pq;

        foreach (var pc in pq.chapters)
        {
            pc.pq = pq;
            if (pc.active)
                await pc.load();
        }

        if (startFirstChapterAndSave)
        {
            // start first chapter?
            var firstChapter = pq.chapters.FirstOrDefault(c => c.id == pq.quest.firstChapter);
            if (firstChapter != null && firstChapter.endTime == null)
            {
                pq.startChapter(pq.quest.firstChapter);
                await pq.startChapters();
            }
        }

        // remove any prior versions
        this.activeQuests.RemoveAll(x => x.publisher == pq.publisher && x.id == pq.id);
        this.activeQuests.Add(pq);

        if (startFirstChapterAndSave)
            await pq.save();
    }

    public async Task<PlayQuest> sideLoad(string folder)
    {
        Game.log($"Begin: sideLoad quest from: {folder}");

        // import quest.json
        var dq = JsonConvert.DeserializeObject<DefQuest>(File.ReadAllText(Path.Combine(folder, "quest.json")))!;
        if (dq.publisher.Contains("|") || dq.id.Contains("|")) throw new Exception($"Quest publisher or ID cannot contain '|' characters");

        // import messages from .md files
        var msgFiles = Directory.GetFiles(folder, "*.md");
        foreach (var filepath in msgFiles)
            dq.msgs.Add(parseMsgMd(filepath));

        // import strings from .json files?
        var stringsPath = Path.Combine(folder, "strings.json");
        if (File.Exists(stringsPath))
            dq.strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(stringsPath))!;

        // prepare states
        var pq = new PlayQuest()
        {
            parent = this,
            quest = dq,
            dev = true,
            watchFolder = folder,
            startTime = DateTime.UtcNow,
        };

        // import chapter scripts
        var scriptFiles = Directory.GetFiles(folder, "*.lua");
        foreach (var filepath in scriptFiles)
        {
            var filename = Path.GetFileNameWithoutExtension(filepath);
            var src = File.ReadAllText(filepath);
            dq.chapters[filename] = src;
            pq.chapters.Add(new PlayChapter(filename, pq));
        }

        // validate data from json
        if (!dq.chapters.ContainsKey(dq.firstChapter))
            throw new Exception($"First chapter script not found: {dq.firstChapter}.lua");

        // "publish" the quest srcs into a json file for later use
        var questJson = JsonConvert.SerializeObject(dq, Formatting.Indented);
        var questFilepath = Path.Combine(PlayState.folder, $"dev-{dq.id}.json");
        Data.saveWithRetry(questFilepath, questJson, true);

        // preserve prior values
        if (devQuest != null && devQuest.publisher == pq.publisher && devQuest.id == pq.id)
        {
            // preserve state from previous PlayQuest
            foreach (var (k, v) in devQuest.objectives) pq.objectives[k] = v;
            foreach (var (k, v) in devQuest.vars) pq.vars[k] = v;
            foreach (var t in devQuest.tags) pq.tags.Add(t);
            foreach (var (k, v) in devQuest.bodyLocations) pq.bodyLocations[k] = v;
            foreach (var (k, v) in devQuest.keptLasts) pq.keptLasts[k] = v;
            foreach (var pr in devQuest.routes) pq.routes.Add(pr);

            foreach (var oc in devQuest.chapters)
            {
                var pc = pq.chapters.FirstOrDefault(c => c.id == oc.id);
                if (pc == null) continue;
                pc.startTime = oc.startTime;
                pc.endTime = oc.endTime;
                foreach (var (k, v) in oc.vars) pc.vars[k] = v;
                pc.pushScriptVars();
            }

            foreach (var om in devQuest.msgs)
            {
                var idx = pq.msgs.FindIndex(m => m.id == om.id);
                if (idx == -1)
                    pq.msgs.Add(om);
                else
                    pq.msgs[idx] = om;
            }
        }

        this.devRef = QuestRef.from(dq);
        this.devQuest = pq;
        await initQuest(pq, true); // (true means save)

        PlayState.updateUI(pq);
        return pq;
    }

    private DefMsg parseMsgMd(string filepath)
    {
        var lines = File.ReadAllLines(filepath);
        var body = new StringBuilder();
        var msg = new DefMsg()
        {
            id = Path.GetFileNameWithoutExtension(filepath),
            from = "",
            body = "",
            actions = new(),
        };
        var firstBlankLine = true;
        foreach (var line in lines)
        {
            if (line.StartsWith("from:", StringComparison.OrdinalIgnoreCase))
                msg.from = line.Substring("from:".Length).Trim();
            else if (line.StartsWith("subject:", StringComparison.OrdinalIgnoreCase))
                msg.subject = line.Substring("subject:".Length).Trim();
            else if (line.StartsWith("action:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Substring("action:".Length).Split(':', StringSplitOptions.TrimEntries)!;
                msg.actions.Add(parts[0], parts[1]);
            }
            else if (line.StartsWith("tags:", StringComparison.OrdinalIgnoreCase))
            {
                msg.tags = JsonConvert.DeserializeObject<HashSet<string>>(line.Substring("tags:".Length));
            }
            else
            {
                if (line == "" && firstBlankLine)
                    firstBlankLine = false;
                else
                    body.AppendLine(line);
            }
        }
        msg.body = body.ToString();
        if (msg.actions.Count == 0) msg.actions = null;

        return msg;
    }

    public async Task processJournalEntry(JObject raw)
    {
        var eventName = raw.Value<string>("event");

        // special case: replace with the relevant file contents
        switch (eventName)
        {
            case nameof(Cargo): raw = JObject.FromObject(Game.activeGame!.cargoFile); break;
            case nameof(Market): raw = JObject.FromObject(Game.activeGame!.marketFile); break;
            case nameof(NavRoute): raw = JObject.FromObject(Game.activeGame!.navRoute); break;
            case "Backpack":
            case "ModulesInfo":
            case "Outfitting":
            case "ShipLocker":
            case "Shipyard":
            case "FCMaterials":
                {
                    using var reader = new JsonTextReader(Data.openSharedStreamReader(Path.Combine(Game.settings.watchedJournalFolder, $"{eventName}.json")));
                    raw = await JObject.LoadAsync(reader);
                    break;
                }
        }

        var tbl = raw.toTbl();

        // process all ...
        foreach (var pq in activeQuests.ToList())
            await pq.processJournalEntry(tbl, raw);

        // ... then save afterwards
        var dirtyQuests = this.activeQuests.Where(pq => pq.dirty).ToList();
        foreach (var pq in dirtyQuests)
            await pq.save();
    }

    public PlayQuest? get(string id)
    {
        return activeQuests.FirstOrDefault(x => x.id == id);
    }

    public bool isTagged(string tag)
    {
        foreach (var pq in activeQuests)
            if (pq.tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                return true;

        return false;
    }

    public static async Task enableGaltea1(string questId)
    {
        try
        {
            var cmdr = CommanderSettings.LoadCurrentOrLast();
            if (cmdr == null) return;

            if (cmdr?.rccApiKey == null)
            {
                MessageBox.Show("Before you can use quests, you must set your Raven Colonial api-key in settings, tab: External Data", "Activate Quest?");
                return;
            }

            var rslt = MessageBox.Show($"Would you like to activate the '{questId}' sample quest?\r\n\r\n(This will reset any prior progress)", "Activate Quest?", MessageBoxButtons.YesNo);
            if (rslt != DialogResult.Yes) return;

            if (!Game.settings.enableQuests)
            {
                Game.settings.enableQuests = true;
                Game.settings.Save();
            }

            PlayState.current ??= await PlayState.loadAsync(cmdr.fid);

            Task.Run(() => PlayState.current.activateQuest("Grinning2001", questId)).justDoIt();

            MessageBox.Show("The quest is ready!\r\n\r\n- Look in the top/right corner of the game for visual queues\r\n\r\n- It is strongly recommended to set an easy key-chord for 'questShow'\r\n\r\n- To interact with quests: use that key-chord or new button on the main window (with 2 squares diagonal, below the giant Colonise button)", "Quest activated: " + questId);
        }
        catch (Exception ex)
        {
            FormErrorSubmit.Show(ex);
        }
    }
}

[JsonConverter(typeof(QuestRef.QuestRefJsonConverter))]
public class QuestRef
{
    public string publisher;
    public string id;
    public double ver;

    public static QuestRef from(DefQuest q)
    {
        return new QuestRef()
        {
            publisher = q.publisher,
            id = q.id,
            ver = q.ver,
        };
    }

    public static QuestRef parse(string txt)
    {
        if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

        var parts = txt.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new QuestRef()
        {
            publisher = parts[0],
            id = parts[1],
            ver = double.Parse(parts[2], CultureInfo.InvariantCulture)
        };
    }

    public override string ToString()
    {
        return $"{publisher}|{id}|{ver.ToString(CultureInfo.InvariantCulture)}";
    }

    class QuestRefJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType) { return false; }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var txt = serializer.Deserialize<string>(reader)!;
            return QuestRef.parse(txt);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var qr = value as QuestRef;
            if (qr == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

            // serialize joined by '|'
            var txt = $"{qr.publisher}|{qr.id}|{qr.ver.ToString(CultureInfo.InvariantCulture)}";
            writer.WriteValue(txt);
        }
    }
}