using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
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

                    foreach (var pc in pq.chapters)
                    {
                        pc.scriptState = await prepScript(conduit, pc.id);
                        pc.pushScriptVars();
                    }
                }

                Game.log(ps.activeQuests.Select(pq => $"{pq.id}: {pq.quest.title}").formatWithHeader($"Initialized {ps.activeQuests.Count} active quests"));
            }

            return ps;
        }

        static Regex onJournals = new Regex(@"void onJournal\s*\(\s*(\w+)\s+.+\)");

        private static string prepCode(string[] lines)
        {
            var code = new StringBuilder();

            var setFuncs = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("using") || line.Contains("System.")) continue;
                code.AppendLine(line);

                if (line.Trim() == "void onStart()")
                    setFuncs.Add($"setFunc(\"onStart\", new Action<object>((o) => onStart()));");

                var parts = onJournals.Matches(line).FirstOrDefault()?.Groups;
                if (parts?.Count == 2)
                {
                    var eventName = parts[1].Value;
                    // eg: onJournal("Docked", new Action<JournalEntry>((o) => on2((Docked)o)));
                    setFuncs.Add($"setFunc(\"{eventName}\", new Action<object>((o) => onJournal(({eventName})o)));");
                }

                if (line.Trim().StartsWith("void onMsgAction("))
                    setFuncs.Add($"setFunc(\"onMsgAction\", new Action<object>((o) => onMsgAction((string)o)));");
            }

            code.AppendLine();
            foreach (var line in setFuncs)
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
            var code = prepCode(script.Split(Environment.NewLine));

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

        public async Task importFolder(string folder, Action<PlayQuest?> onComplete)
        {
            Game.log($"Begin: Importing quest from: {folder}");
            var issues = new StringBuilder();

            // import quest.json
            var newQuest = JsonConvert.DeserializeObject<Quest>(File.ReadAllText(Path.Combine(folder, "quest.json")))!;

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

            // import strings from .json files
            newQuest.strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(folder, "strings.json")))!;

            // import chapter scripts
            var scriptFiles = Directory.GetFiles(folder, "*.csx");
            foreach (var filepath in scriptFiles)
            {
                var name = Path.GetFileNameWithoutExtension(filepath);
                var script = File.ReadAllText(filepath);
                newQuest.chapters[name] = script;
            }

            // validate data from json
            if (!newQuest.chapters.ContainsKey(newQuest.firstChapter))
            {
                issues.AppendLine($"{Environment.NewLine}File: quest.json{Environment.NewLine}----------------------------------------");
                issues.AppendLine($"Script not found: '{newQuest.firstChapter}.csx'");
            }

            // prepare states
            var newPlayQuest = new PlayQuest()
            {
                parent = this,
                quest = newQuest,
                id = newQuest.id,
                watchFolder = folder,
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
                    newPlayQuest.chapters.Add(pc);

                    // store initial variables
                    foreach (var var in state.Variables)
                        pc.vars[$"{var.Name}|{var.Type}"] = var.Value;

                    // check for mismatched IDs
                    validateScript(newQuest, id, state.Script.Code, issues);
                }
                catch (CompilationErrorException ex)
                {
                    issues.AppendLine($"{Environment.NewLine}File: {id}.json{Environment.NewLine}----------------------------------------");
                    foreach (var x in ex.Diagnostics)
                        issues.AppendLine(x.ToString());

                    Game.log($"Compilation error in chapter: {id}\n\t" + string.Join(Environment.NewLine, ex.Diagnostics));
                    continue;
                }
            }

            // initialize first chapter
            newPlayQuest.startChapter(newQuest.firstChapter);

            Game.log($"Parsed quest '{newPlayQuest.quest.title}' ({newPlayQuest.id}) from: {folder}");
            if (issues.Length > 0)
            {
                // There were issues with the script
                issues.Insert(0, $"Problems importing folder:{Environment.NewLine}{folder}{Environment.NewLine}");
                issues.Replace("ScriptGlobals.S_", "");

                BaseForm.show<FormImportProblems>(f =>
                {
                    f.txt.Text = issues.ToString();
                    f.onRetry = new Action(() => importFolder(folder, onComplete).justDoIt());
                    f.FormClosed += (s, e) =>
                    {
                        if (!f.retrying)
                        {
                            onComplete(null);
                            Game.log($"Failed: Importing quest from: {folder}");
                        }
                    };
                });
            }
            else
            {
                // No issues - add/replace it on the Cmdr
                var idx = activeQuests.FindIndex(x => x.id == newPlayQuest.id);
                if (idx < 0)
                    this.activeQuests.Add(newPlayQuest);
                else
                    this.activeQuests[idx] = newPlayQuest;

                this.Save();
                Game.log($"Success: Importing quest from: {folder}");

                // TODO: use a server for this ...
                var pubFilepath = Path.Combine(PlayState.folder, $"{newQuest.id}.json");
                File.WriteAllText(pubFilepath, JsonConvert.SerializeObject(newQuest, Formatting.Indented));

                BaseForm.get<FormPlayComms>()?.onQuestChanged(newPlayQuest);
                PlotBase2.invalidate(nameof(PlotQuestMini));
                onComplete(newPlayQuest);
            }
        }

        static Regex badObjectiveIDs = new Regex(@"\bobjective.\w+\(\s*""(.+)""");
        static Regex badMsgIDs = new Regex(@"\.sendMsg\(\s*""(.+)""");
        static Regex badChapters = new Regex(@"\bchapter.start\(\s*""(.+)""|\bchapter.stop\(\s*""(.+)""");

        private void validateScript(Quest q, string filename, string code, StringBuilder issues)
        {
            var fileHeaderAdded = false;
            var addHeaderLineIfNeeded = () =>
            {
                if (!fileHeaderAdded)
                {
                    fileHeaderAdded = true;
                    issues.AppendLine($"{Environment.NewLine}File: {filename}.csx{Environment.NewLine}----------------------------------------");
                }
            };

            var lines = code.Split("\n");
            for (int n = 1; n < lines.Length; n++)
            {
                var line = lines[n - 1];

                // bad objective names
                foreach (var grp in badObjectiveIDs.Matches(line).Select(m => m.Groups[1]))
                {
                    if (!q.objectives.ContainsKey(grp.Value))
                    {
                        addHeaderLineIfNeeded();
                        issues.AppendLine($"({n}, {grp.Index}): Bad objective name: '{grp.Value}'");
                    }
                }

                // bad msg IDs
                foreach (var grp in badMsgIDs.Matches(line).Select(m => m.Groups[1]))
                {
                    if (!q.msgs.Any(m => m.id == grp.Value))
                    {
                        addHeaderLineIfNeeded();
                        issues.AppendLine($"({n}, {grp.Index}): Bad msg ID: '{grp.Value}'");
                    }
                }

                // bad chapter IDs
                foreach (var grp in badChapters.Matches(line).Select(m => m.Groups[1].Length > 0 ? m.Groups[1] : m.Groups[2]))
                {
                    if (!q.chapters.ContainsKey(grp.Value))
                    {
                        addHeaderLineIfNeeded();
                        issues.AppendLine($"({n}, {grp.Index}): Bad chapter ID: '{grp.Value}'");
                    }
                }
            }
        }
    }

    [Draggable, TrackPosition]
    class FormImportProblems : SizableForm
    {
        public TextBox txt;
        public Button btn;
        public Action onRetry;
        public bool retrying;

        public FormImportProblems()
        {
            Text = "Import failed";
            Width = 1000;
            Height = 400;
            StartPosition = FormStartPosition.CenterScreen;

            txt = new TextBox()
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
            };
            txt.SelectionStart = 0;

            btn = new Button()
            {
                Text = "Retry",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Top = 8,
                Width = 80,
                Left = 880,
            };
            btn.Click += Btn_Click;

            this.Controls.Add(btn);
            this.Controls.Add(txt);
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            retrying = true;
            Program.defer(() => this.onRetry());
            this.Close();
        }
    }

    public class PlayQuest
    {
        [JsonIgnore] internal PlayState parent;
        [JsonIgnore] public Quest quest;
        [JsonIgnore] public Conduit conduit;
        [JsonIgnore] public string? watchFolder;

        //[JsonIgnore] public Dictionary<string, IJournalEntry entry> priorEvents = new(); // <-- TODO

        public string id;
        public HashSet<string> activeChapters = new();
        public Dictionary<string, object> vars = new();
        public Dictionary<string, PlayObjective> objectives = new();
        public DateTime watermark; // <-- TODO
        /// <summary> Delivered messages </summary>
        public List<PlayChapter> chapters = new();
        public List<PlayMsg> msgs = new();

        [JsonIgnore] public int unreadMessageCount => msgs.Count(m => !m.read);

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
                foreach (var pc in chapters)
                {
                    // store initial variables
                    foreach (var var in pc.scriptState.Variables)
                        pc.vars[$"{var.Name}|{var.Type}"] = var.Value;
                }

                BaseForm.get<FormPlayComms>()?.onQuestChanged(this);
                PlotBase2.invalidate(nameof(PlotQuestMini));
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
            var func = conduit.funcs.GetValueOrDefault($"{id}.onStart");

            Game.log($"Starting chapter: {id}, run onStart: {func != null}");
            activeChapters.Add(id);

            // run the init func if it exists
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

    [JsonConverter(typeof(PlayObjective.JsonConverter))]
    public class PlayObjective
    {
        public State state;
        public int current;
        public int total;

        public override string ToString()
        {
            return $"{state},{current},{total}";
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum State
        {
            visible,
            hidden,
            complete,
            failed,
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                // eg: "visible|1|10"
                var parts = txt.Split(',');
                return new PlayObjective()
                {
                    state = Enum.Parse<PlayObjective.State>(parts[0]),
                    current = int.Parse(parts[1]),
                    total = int.Parse(parts[2]),
                };
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is PlayObjective)
                    writer.WriteValue(value.ToString());
                else
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");
            }
        }
    }

    public class PlayChapter
    {
        [JsonIgnore] public PlayQuest parent;
        [JsonIgnore] public ScriptState scriptState;

        public string id;
        public Dictionary<string, object> vars = new();

        public void pushScriptVars()
        {
            foreach (var key in vars.Keys)
            {
                var parts = key.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var name = parts[0];
                var type = Type.GetType(parts[1])!;
                var raw = vars[key];
                // re-cast the value as the desired type
                var val = JToken.FromObject(raw).ToObject(type);

                var match = scriptState.GetVariable(name);
                if (match != null)
                    match.Value = val;
                else
                    Game.log($"Cannot add unknown variable: {name}");
            }
        }
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

        public override string ToString()
        {
            return $"{id}:{subject ?? body}";
        }

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
