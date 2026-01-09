using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.quests.scripting;
using System.Text;
using System.Text.RegularExpressions;

namespace SrvSurvey.quests
{
    internal class PlayState : Data
    {
        #region static loading code

        private static string folder = Path.Combine(Program.dataFolder, "playStates");

        public static async Task<PlayState> load(string fid)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

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

                // rehydrate quest, scriptState and variables
                foreach (var pq in ps.activeQuests)
                {
                    pq.parent = ps;

                    // TODO: use a server for this ...
                    var questPath = Path.Combine(folder, $"{pq.id}.json");
                    if (!File.Exists(filepath)) throw new Exception($"Missing! {questPath}");

                    var json = File.ReadAllText(questPath);
                    pq.quest = JsonConvert.DeserializeObject<Quest>(json)!;
                    var conduit = new Conduit(pq);

                    foreach (var pc in pq.chapters.Values)
                    {
                        pc.scriptState = await prepScript(conduit, pc.id);
                        foreach (var key in pc.vars.Keys)
                        {
                            var parts = key.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            var name = parts[0];
                            var type = Type.GetType(parts[1])!;
                            var raw = pc.vars[key];
                            // re-cast the value as the desired type
                            var val = JToken.FromObject(raw).ToObject(type);
                            pc.scriptState.GetVariable(name).Value = val;
                        }
                    }
                }

                Game.log(ps.activeQuests.Select(pq => $"{pq.id}: {pq.quest.title}").formatWithHeader($"Initialized {ps.activeQuests.Count} active quests"));
            }

            return ps;
        }

        private static string prepCode(string[] lines)
        {
            var code = new StringBuilder();

            var subs = new Dictionary<string, string>();
            var r1 = new Regex(@"void on\s*\(\s*(\w+)\s+.+\)");

            foreach (var line in lines)
            {
                if (line.StartsWith("using") || line.Contains("System.")) continue;
                code.AppendLine(line);

                if (line == "void init()")
                    subs["init"] = $"setFunc(\"init\", new Action<object>((o) => init()));";

                var parts = r1.Matches(line).FirstOrDefault()?.Groups;
                if (parts?.Count == 2)
                {
                    var eventName = parts[1].Value;
                    // eg: on("Docked", new Action<JournalEntry>((o) => on2((Docked)o)));
                    subs[eventName] = $"setFunc(\"{eventName}\", new Action<object>((o) => on(({eventName})o)));";
                }
            }

            code.AppendLine("// Post generation ...");
            foreach (var line in subs.Values)
                code.AppendLine(line);

            return code.ToString();
        }

        private static ScriptOptions scriptOptions = ScriptOptions.Default
            .WithCheckOverflow(true)
            .WithAllowUnsafe(false)
            .AddReferences(typeof(PlayState).Assembly)
            .WithImports(new[] {
                "System",
                "Newtonsoft.Json.Linq",
                "SrvSurvey",
                "SrvSurvey.quests.scripting",
            });

        private static async Task<ScriptState> prepScript(Conduit c, string name)
        {
            string script = c.pq.quest.chapters[name];
            var code = prepCode(script.Split('\n', StringSplitOptions.TrimEntries));

            var sg = new scripting.ScriptGlobals(c, name);

            // Compile and run once to initialize
            var compiled = CSharpScript.Create(code, scriptOptions, typeof(scripting.ScriptGlobals));

            var state = await compiled.RunAsync(sg);
            return state;
        }

        #endregion

        #region data members

        public string fid;
        public List<PlayQuest> activeQuests = new();
        // TODO: store cmdr level variables?

        #endregion

        public void processJournalEntry(IJournalEntry entry)
        {
            var dirty = false;
            foreach (var q in activeQuests.ToList())
                dirty |= q.processJournalEntry(entry);

            if (dirty)
                this.Save();
        }

        public async Task<PlayQuest> importFolder(string folder)
        {
            var success = true;

            // import quest.json
            var newQuest = JsonConvert.DeserializeObject<Quest>(File.ReadAllText(Path.Combine(folder, "quest.json")))!;

            // import optional desc.md
            var descFilePath = Path.Combine(folder, "desc.md");
            if (File.Exists(descFilePath))
                newQuest.longDesc = File.ReadAllText(descFilePath);

            // import messages from .json files
            var msgFiles = Directory.GetFiles(folder, "msg*.json");
            foreach (var filepath in msgFiles)
            {
                var msgs = JsonConvert.DeserializeObject<Msg[]>(File.ReadAllText(filepath))!;

                // check for duplicate IDs
                var priorIDs = newQuest.msgs.Select(m => m.id);
                var dupeIDs = msgs.Where(m => priorIDs.Contains(m.id)).Select(m => m.id);
                if (dupeIDs.Any())
                {
                    Game.log($"Duplicate msg IDs: [{dupeIDs}], in: {filepath.Replace(folder, "\"")}");
                    continue;
                }

                newQuest.msgs.AddRange(msgs);
            }

            // import chapter scripts
            var scriptFiles = Directory.GetFiles(folder, "*.csx");
            foreach (var filepath in scriptFiles)
            {
                var name = Path.GetFileNameWithoutExtension(filepath);
                var script = File.ReadAllText(filepath);

                // can we roughly compile it?
                try
                {
                    var compiled = CSharpScript.Create(script, scriptOptions, typeof(scripting.ScriptGlobals));
                }
                catch (CompilationErrorException ex)
                {
                    success = false;
                    Game.log("Compilation error in chapter: " + string.Join(Environment.NewLine, ex.Diagnostics));
                    continue;
                }

                newQuest.chapters[name] = script;
            }

            // prepare states
            var newPlayQuest = new PlayQuest()
            {
                parent = this,
                quest = newQuest,
                id = newQuest.id,
            };
            var newConduit = new Conduit(newPlayQuest);

            foreach (var id in newQuest.chapters.Keys)
            {
                try
                {
                    var state = await prepScript(newConduit, id);
                    var pc = new PlayChapter()
                    {
                        id = id,
                        parent = newPlayQuest,
                        scriptState = state!,
                    };
                    newPlayQuest.chapters[id] = pc;

                    // store initial variables
                    foreach (var var in state.Variables)
                    {
                        pc.vars[$"{var.Name}|{var.Type}"] = var.Value;
                    }
                }
                catch (CompilationErrorException ex)
                {
                    success = false;
                    Game.log($"Compilation error in chapter: {id}\n\t" + string.Join(Environment.NewLine, ex.Diagnostics));
                    continue;
                }
            }

            // TODO: validate objective and msg IDs are not bogus

            // initialize first chapter
            newPlayQuest.startChapter(newQuest.firstChapter);

            Game.log($"Parsed quest '{newPlayQuest.quest.title}' ({newPlayQuest.id}) from: {folder}");
            if (success)
            {
                var idx = activeQuests.FindIndex(x => x.id == newPlayQuest.id);
                if (idx < 0)
                    this.activeQuests.Add(newPlayQuest);
                else
                    this.activeQuests[idx] = newPlayQuest;

                this.Save();

                // TODO: use a server for this ...
                var pubFilepath = Path.Combine(PlayState.folder, $"{newQuest.id}.json");
                File.WriteAllText(pubFilepath, JsonConvert.SerializeObject(newQuest, Formatting.Indented));

                BaseForm.get<FormPlayComms>()?.onQuestChanged(newPlayQuest);
            }

            // TODO: watch folder for further edits?
            // AND: allow the folder to be known to PlayQuest

            return newPlayQuest;
        }
    }

    public class PlayQuest
    {
        [JsonIgnore] internal PlayState parent;
        [JsonIgnore] public Quest quest;
        [JsonIgnore] public Conduit conduit;
        //[JsonIgnore] public Dictionary<string, IJournalEntry entry> priorEvents = new(); // <-- TODO

        public string id;
        public HashSet<string> activeChapters = new();
        public Dictionary<string, object> vars = new();
        public Dictionary<string, PlayObjective> objectives = new();
        public DateTime watermark; // <-- TODO
        /// <summary> Delivered messages </summary>
        public Dictionary<string, PlayChapter> chapters = new();
        public List<PlayMsg> msgs = new();

        public bool processJournalEntry(IJournalEntry entry)
        {
            conduit.dirty = false;
            // trigger function if chapters care about it
            foreach (var id in activeChapters.ToList())
            {
                var func = conduit.funcs.GetValueOrDefault($"{id}.{entry.@event}")!;
                if (func != null)
                    func(entry);
            }

            if (conduit.dirty)
            {
                foreach (var pc in chapters.Values)
                {
                    // store initial variables
                    foreach (var var in pc.scriptState.Variables)
                        pc.vars[$"{var.Name}|{var.Type}"] = var.Value;
                }

                BaseForm.get<FormPlayComms>()?.onQuestChanged(this);
            }

            return conduit.dirty;
        }

        public void complete()
        {
            // TODO: ...
        }

        public void fail()
        {
            // TODO: ...
        }

        public void startChapter(string id)
        {
            // skip if already active
            if (activeChapters.Contains(id)) return;

            Game.log($"Starting chapter: {id}");
            activeChapters.Add(id);

            // run the init func if it exists
            var func = conduit.funcs.GetValueOrDefault($"{id}.init");
            if (func != null)
                func(null!);

            conduit.dirty = true;
        }

        public void stopChapter(string id)
        {
            Game.log($"Stopping chapter: {id}");
            activeChapters.Remove(id);
            conduit.dirty = true;
        }

        public void sendMsg(PlayMsg newMsg)
        {
            if (newMsg.from == null && quest.msgs.Find(m => m.id == newMsg.id) == null)
                throw new Exception($"Bad message: {newMsg.id}");

            msgs.Add(newMsg);
            conduit.dirty = true;
        }
    }

    public class PlayObjective
    {
        public State state;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int current;
        public int total;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum State
        {
            visible,
            hidden,
            complete,
            failed,
        }
    }

    public class PlayChapter
    {
        [JsonIgnore] public PlayQuest parent;
        [JsonIgnore] public ScriptState scriptState;

        public string id;
        public Dictionary<string, object> vars = new();
    }

    public class PlayMsg
    {
        public string id;
        public DateTimeOffset received;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? from;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? subject;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? body;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[]? actions;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool read;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? replied;

        public static PlayMsg send(Msg? msg = null, string? from = null, string? subject = null, string? body = null)
        {
            var newMsg = new PlayMsg()
            {
                id = msg?.id ?? DateTimeOffset.UtcNow.ToString("yyyyMMddhhmmss"),
                received = DateTimeOffset.UtcNow,
                actions = msg?.actions,
            };

            // store nothing if the following values match the template Msg

            newMsg.from = from == null || from == msg?.from ? null : from ?? msg!.from;
            newMsg.subject = subject == null || subject == msg?.subject ? null : subject ?? msg!.subject;
            newMsg.body = body == null || body == msg?.body ? null : body ?? msg!.body;

            return newMsg;
        }
    }

}
