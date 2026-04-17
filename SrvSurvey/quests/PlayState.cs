using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using System.Text;

namespace SrvSurvey.quests;

internal class PlayState : Data
{
    #region static + loading code

    private static string folder = Path.Combine(Program.dataFolder, "quests");

    public static Task<PlayState?> loadAsync(string fid)
    {
        return Task.Run(async () =>
        {
            PlayState? newState = null;
            try
            {
                newState = await PlayState.load(fid);
            }
            catch (Exception ex)
            {
                Game.log($"PlayState.loadAsync: {ex}");
                Program.defer(() =>
                {
                    FormErrorSubmit.Show(ex);
                });
            }
            return newState;
        });
    }

    public static async Task<PlayState> load(string fid)
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
                filepath = filepath,
            };
            ps.Save();
        }
        else
        {
            // parse existing state
            ps = Data.Load<PlayState>(filepath)!;

            foreach (var pq in ps.activeQuests)
            {
                pq.parent = ps;

                // TODO: use a server for this ...
                var questPath = Path.Combine(folder, $"{pq.id}.json");
                if (!File.Exists(questPath)) throw new Exception($"Missing! {questPath}");

                var questJson = File.ReadAllText(questPath);
                pq.quest = JsonConvert.DeserializeObject<DefQuest>(questJson)!;

                foreach (var pm in pq.msgs)
                    pm.parent = pq;

                foreach (var pc in pq.chapters)
                {
                    pc.pq = pq;

                    if (pc.active)
                        await pc.load();
                }

            }
        }

        Game.log(ps.activeQuests.Select(pq => $"{pq.id}: {pq.quest.title}").formatWithHeader($"Initialized {ps.activeQuests.Count} active quests"));

        return ps;
    }

    public static void updateUI(PlayQuest? pq = null)
    {
        PlotBase2.renderAll(null, true);
        BaseForm.get<FormPlayComms>()?.onQuestChanged(pq);
        BaseForm.get<FormPlayDev>()?.onQuestChanged(pq);
    }

    #endregion

    #region data members

    public string fid = "F123";
    public List<PlayQuest> activeQuests = [];

    // TODO: store cmdr level variables?

    /* TODO: paused and completed quests? Maybe ...
     * 
     * public List<PlayQuest> quests = [];
     * 
     * public [JsonIgnore] List<PlayQuest> activeQuests => quests.Where(pq => pq.active).ToList();
     */

    #endregion

    [JsonIgnore] public bool dirty;
    [JsonIgnore] public int messagesTotal => activeQuests.Sum(q => q.msgs.Count);
    [JsonIgnore] public int messagesUnread => activeQuests.Sum(pq => pq.unreadMessageCount);


    public async Task<PlayQuest> sideLoad(string folder)
    {
        Game.log($"Begin: Importing quest from: {folder}");
        var issues = new StringBuilder();

        // import quest.json
        var newQuest = JsonConvert.DeserializeObject<DefQuest>(File.ReadAllText(Path.Combine(folder, "quest.json")))!;

        /*// import messages from .json files -- OLD --
        var msgFiles = Directory.GetFiles(folder, "msg*.json");
        foreach (var filepath in msgFiles)
        {
            var msgs = JsonConvert.DeserializeObject<DefMsg[]>(File.ReadAllText(filepath))!;

            // check for duplicate IDs
            var priorIDs = newQuest.msgs.Select(m => m.id);
            var dupeIDs = msgs.Where(m => priorIDs.Contains(m.id)).Select(m => m.id);
            if (dupeIDs.Any())
            {
                Game.log($"Duplicate msg IDs: [{dupeIDs}], in: {filepath.Replace(folder, "\"")}");
                continue;
            }

            newQuest.msgs.AddRange(msgs);
        } // */
        // import messages from .md files -- NEW --
        var msgFiles2 = Directory.GetFiles(folder, "*.md");
        foreach (var filepath in msgFiles2)
        {
            var msg = parseMsgMd(filepath);

            // check for duplicate IDs ?
            //var priorIDs = newQuest.msgs.Select(m => m.id);
            //var dupeIDs = msgs.Where(m => priorIDs.Contains(m.id)).Select(m => m.id);
            //if (dupeIDs.Any())
            //{
            //    Game.log($"Duplicate msg IDs: [{dupeIDs}], in: {filepath.Replace(folder, "\"")}");
            //    continue;
            //}
            newQuest.msgs.Add(msg);
        }
        // --

        // import strings from .json files?
        var stringsPath = Path.Combine(folder, "strings.json");
        if (File.Exists(stringsPath))
            newQuest.strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(stringsPath))!;

        // prepare states
        var pq = new PlayQuest()
        {
            parent = this,
            quest = newQuest,
            id = newQuest.id,
            watchFolder = folder,
            startTime = DateTime.UtcNow,
        };

        // import chapter scripts
        var scriptFiles = Directory.GetFiles(folder, "*.lua");
        foreach (var filepath in scriptFiles)
        {
            var filename = Path.GetFileNameWithoutExtension(filepath);
            var src = File.ReadAllText(filepath);
            newQuest.chapters[filename] = src;

            var pc = new PlayChapter()
            {
                id = filename,
                pq = pq,
            };

            pq.chapters.Add(pc);
        }

        // validate data from json
        if (!newQuest.chapters.ContainsKey(newQuest.firstChapter))
            throw new Exception($"First chapter script not found: {newQuest.firstChapter}.lua");

        // "publish" the quest srcs into a json file for later use
        var questJson = JsonConvert.SerializeObject(newQuest, Formatting.Indented);
        var questFilepath = Path.Combine(PlayState.folder, $"{newQuest.id}.json");
        Data.saveWithRetry(questFilepath, questJson, true);

        var oldPQ = activeQuests.Find(pq => pq.watchFolder == folder || pq.id == newQuest.id);

        activeQuests.RemoveAll(x => x.id == pq.id);
        activeQuests.Add(pq);

        if (oldPQ != null)
        {
            // preserve state from previous PlayQuest
            foreach (var (k, v) in oldPQ.objectives) pq.objectives[k] = v;
            foreach (var (k, v) in oldPQ.vars) pq.vars[k] = v;
            foreach (var t in oldPQ.tags) pq.tags.Add(t);
            foreach (var (k, v) in oldPQ.bodyLocations) pq.bodyLocations[k] = v;
            foreach (var (k, v) in oldPQ.keptLasts) pq.keptLasts[k] = v;

            foreach (var oc in oldPQ.chapters)
            {
                var pc = pq.chapters.FirstOrDefault(c => c.id == oc.id);
                if (pc == null) continue;
                pc.startTime = oc.startTime;
                pc.endTime = oc.endTime;
                foreach (var (k, v) in oc.vars) pc.vars[k] = v;
                pc.pushScriptVars();
            }

            foreach (var om in oldPQ.msgs)
            {
                var idx = pq.msgs.FindIndex(m => m.id == om.id);
                if (idx == -1)
                    pq.msgs.Add(om);
                else
                    pq.msgs[idx] = om;
            }
        }

        // fetch always last known's
        setPriorKepts(pq);

        foreach (var pc in pq.chapters)
        {
            if (pc.active)
                await pc.load();
        }

        // start first chapter?
        var firstChapter = pq.chapters.FirstOrDefault(c => c.id == newQuest.firstChapter);
        if (firstChapter != null && firstChapter.endTime == null)
            await pq.startChapter(newQuest.firstChapter);

        this.Save();

        PlayState.updateUI(pq);
        return pq;
    }

    private void setPriorKepts(PlayQuest pq)
    {
        if (!pq.keptLasts.ContainsKey(nameof(Docked)))
        {
            Game.activeGame?.journals?.walkDeep(true, entry =>
            {
                if (entry is Docked)
                {
                    pq.keptLasts[nameof(Docked)] = JournalEntry.toLua(entry);
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
                    pq.keptLasts[nameof(FSDJump)] = JournalEntry.toLua(entry);
                    return true;
                }
                return false;
            });
        }
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
        if (dirty) this.Save();
        dirty = false;

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
                raw = JObject.Load(new JsonTextReader(Data.openSharedStreamReader(Path.Combine(Game.settings.watchedJournalFolder, $"{eventName}.json"))));
                break;
        }

        var tbl = raw.toTbl();

        foreach (var pq in activeQuests.ToList())
            dirty |= await pq.processJournalEntry(tbl, raw);

        if (dirty)
            this.Save();
    }

    public PlayQuest? get(string id)
    {
        return activeQuests.Find(x => x.id == id);
    }

    public bool isTagged(string tag)
    {
        foreach (var pq in activeQuests)
            if (pq.tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                return true;

        return false;
    }
}
