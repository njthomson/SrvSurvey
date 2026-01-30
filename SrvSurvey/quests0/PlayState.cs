using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.quests0.scripting;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SrvSurvey.quests0
{
    /// <summary> The runtime/persisted state of all quests for a player </summary>
    internal class PlayState : Data
    {
        #region static loading code

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

        private static async Task<PlayState> load(string fid)
        {
            Game.log($"PlayState.load: fid: {fid}");

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
                        pc.parent = pq;
                        pc.scriptState = await prepScript(conduit, pc.id);
                        pc.pushScriptVars();
                    }

                    foreach (var pm in pq.msgs)
                        pm.parent = pq;
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
                var trimmed = line.Trim();
                if (trimmed.StartsWith("using") || trimmed.Contains("System.") || trimmed.StartsWith("#"))
                {
                    code.AppendLine("// " + line);
                    continue;
                }
                code.AppendLine(line);

                if (trimmed == "void onStart()")
                    setFuncs.Add($"setFunc(\"onStart\", new Action<object>((o) => onStart()));");

                var parts = onJournals.Matches(line).FirstOrDefault()?.Groups;
                if (parts?.Count == 2)
                {
                    var eventName = parts[1].Value;
                    // eg: onJournal("Docked", new Action<JournalEntry>((o) => on2((Docked)o)));
                    setFuncs.Add($"setFunc(\"{eventName}\", new Action<object>((o) => onJournal(({eventName})o)));");
                }

                if (trimmed.StartsWith("void onMsgAction("))
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
                "System.Collections.Generic",
                "System.Text.RegularExpressions",
                "Newtonsoft.Json.Linq",
                "SrvSurvey",
                "SrvSurvey.quests.scripting",
            });

        private static async Task<ScriptState> prepScript(Conduit c, string name)
        {
            string script = c.pq.quest.chapters[name];
            var code = prepCode(script.Split(Environment.NewLine));

            var sg = new scripting.ChapterScript(c, name);

            // Compile and run once to initialize
            var compiled = CSharpScript.Create(code, scriptOptions, typeof(scripting.ChapterScript));

            var state = await compiled.RunAsync(sg);
            return state;
        }

        #endregion

        #region data members

        public string fid;
        public List<PlayQuest> activeQuests = [];
        // TODO: store cmdr level variables?

        /* TODO: paused and completed quests? Maybe ...
         * 
         * public List<PlayQuest> quests = [];
         * 
         * public [JsonIgnore] List<PlayQuest> activeQuests => quests.Where(pq => pq.active).ToList();
         */

        #endregion

        [JsonIgnore] public int messagesTotal => activeQuests.Sum(q => q.msgs.Count);
        [JsonIgnore] public int messagesUnread => activeQuests.Sum(pq => pq.unreadMessageCount);

        public PlayQuest? get(string id)
        {
            return activeQuests.Find(x => x.id == id);
        }

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

            // import strings from .json files?
            var stringsPath = Path.Combine(folder, "strings.json");
            if (File.Exists(stringsPath))
                newQuest.strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(stringsPath))!;

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
                    pc.pullScriptVars();

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

            if (issues.Length == 0)
            {
                try
                {
                    if (!newPlayQuest.startTime.HasValue)
                        newPlayQuest.startTime = DateTimeOffset.UtcNow;

                    // initialize first chapter, if needed
                    var chapter = newPlayQuest.chapters.Find(pc => pc.id == newQuest.firstChapter);
                    if (chapter == null) throw new Exception($"Bad firstChapter id: {newQuest.firstChapter}");
                    if (!chapter.startTime.HasValue)
                        newPlayQuest.startChapter(newQuest.firstChapter);
                }
                catch (Exception ex)
                {
                    issues.AppendLine($"start first chapter failed: {ex.Message}");
                }
            }

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

                //BaseForm.get<FormPlayComms>()?.onQuestChanged(newPlayQuest);
                //PlotBase2.invalidate(nameof(PlotQuestMini));
                onComplete(newPlayQuest);
            }
        }

        static Regex badObjectiveIDs = new Regex(@"\bobjective.\w+\(\s*""(.+?)""");
        static Regex badMsgIDs = new Regex(@"\.sendMsg\(\s*""(.+?)""");
        static Regex badChapters = new Regex(@"\bchapter.start\(\s*""(.+?)""|\bchapter.stop\(\s*""(.+?)""");

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
                if (line.Trim().StartsWith("//")) continue;

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

        public bool isTagged(string tag)
        {
            foreach (var pq in activeQuests)
                if (pq.tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }

    /// <summary> A bespoke form shown when side-loading/parsing a quest fails </summary>
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

    /// <summary> The runtime/persisted state for an individual quest </summary>
    public class PlayQuest
    {
        [JsonIgnore] internal PlayState parent;
        [JsonIgnore] public Quest quest;
        [JsonIgnore] public Conduit conduit;
        [JsonIgnore] public string? watchFolder;

        #region data members

        public string id;
        /// <summary> The values of variables extracted from running scripts </summary>
        public Dictionary<string, object> vars = [];

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

        //public DateTimeOffset watermark; // TODO: DO we really want this? Processing missed journal events will be problematic as we cannot know if they take advantage of live values from status.json or NavRoute.json, etc

        /// <summary> The set of 'tags' (system or station names) to highlight in various overlays </summary>
        public HashSet<string> tags = [];

        /// <summary> The set of actively tracked body locations </summary>
        public Dictionary<string, LatLong3> bodyLocations = [];

        /// <summary> The current state of all chapters </summary>
        public List<PlayChapter> chapters = [];

        /// <summary> Delivered messages </summary>
        public List<PlayMsg> msgs = [];

        /// <summary> A map of last seen journal events </summary>
        [JsonConverter(typeof(PlayQuest.KeptLastsJsonConverter))]
        public Dictionary<string, IJournalEntry?> keptLasts = [];

        #endregion

        /// <summary> Returns true if this quest is currently active </summary>
        [JsonIgnore] public bool active => !endTime.HasValue && startTime.HasValue;

        [JsonIgnore] public int unreadMessageCount => msgs.Count(m => !m.read);

        public PlayMsg? getMsg(string id)
        {
            return msgs.Find(x => x.id == id);
        }

        public override string ToString()
        {
            return $"playQuest:{id}";
        }

        /// <summary> Called by Game.cs so chapters may process journal events as they happen </summary>
        public bool processJournalEntry(IJournalEntry entry)
        {
            conduit.dirty = false;

            // trigger function if chapters care about it
            foreach (var pc in chapters)
            {
                if (!pc.active) continue;

                var key = $"{pc.id}/{entry.@event}";
                var func = conduit.funcs.GetValueOrDefault(key)!;
                if (func != null)
                {
                    try
                    {
                        Game.log($"Invoking: {pc.parent.id}/{key}");
                        func(entry);
                    }
                    catch (Exception ex)
                    {
                        Game.log($"Quest script error: {ex.Message}\r\n\t{ex.StackTrace}");
                    }
                }
            }

            // keep last references? (always after processing)
            if (keptLasts.ContainsKey(entry.@event))
            {
                keptLasts[entry.@event] = entry;
                conduit.dirty = true;
            }

            return updateIfDirty();
        }

        /// <summary> Returns true if quest states were dirty, after saving to a file </summary>
        public bool updateIfDirty(bool force = false)
        {
            if (!conduit.dirty && !force) return false;

            foreach (var pc in chapters)
                pc.pullScriptVars();

            this.parent.Save();

            //BaseForm.get<FormPlayComms>()?.onQuestChanged(this);
            //PlotBase2.invalidate(nameof(PlotQuestMini));

            conduit.dirty = false;
            return true;
        }

        public void complete()
        {
            this.endTime = DateTimeOffset.UtcNow;
        }

        public void fail()
        {
            this.endTime = DateTimeOffset.UtcNow;

            /* TODO: what does it really mean for the whole quest to be completed successfully or not?
             * Maybe we have `string endReason` with something descriptive for players to know success or not
             */
        }

        public void startChapter(string id)
        {
            var chapter = chapters.Find(c => c.id == id);
            if (chapter == null) throw new Exception($"Bad chapter id: {id}");

            // skip if already active
            if (chapter.active) return;

            var func = conduit.funcs.GetValueOrDefault($"{id}/onStart");
            Game.log($"Starting chapter: {id}, run onStart: {func != null}");
            chapter.startTime = DateTimeOffset.UtcNow;
            chapter.endTime = null;

            // run the onStart func if it exists
            if (func != null)
                func(null!);

            conduit.dirty = true;
        }

        public void stopChapter(string id)
        {
            var chapter = chapters.Find(c => c.id == id);
            if (chapter == null) throw new Exception($"Bad chapter id: {id}");

            Game.log($"Stopping chapter: {id}");
            chapter.endTime = DateTimeOffset.UtcNow;

            conduit.dirty = true;
        }

        public void sendMsg(PlayMsg newMsg)
        {
            newMsg.parent = this;
            if (newMsg.from == null && quest.msgs.Find(m => m.id == newMsg.id) == null)
                throw new Exception($"Bad message: {newMsg.id}");

            // remove any prior entries with the same id
            msgs.RemoveAll(m => m.id == newMsg.id);

            msgs.Add(newMsg);
            conduit.dirty = true;
        }

        /// <summary> Called by Quest Comms when a player hit a message reply button </summary>
        public void invokeMessageAction(string msgId, string actionId)
        {
            var pm = msgs.Find(m => m.id == msgId);
            if (pm == null) throw new Exception($"Message not found, id: {msgId}");
            var chapterId = pm.chapter!;

            // invoke the action
            var func = conduit.funcs.GetValueOrDefault($"{chapterId}/onMsgAction");
            if (func == null) throw new Exception($"Missing function 'void onMsgAction(string id)' in chapter: {chapterId}");

            func((string)actionId);
            pm.replied = actionId;
            conduit.dirty = true;

            // update any UX
            //BaseForm.get<FormPlayComms>()?.onQuestChanged(this);
            //PlotBase2.invalidate(nameof(PlotQuestMini));
        }

        public void deleteMsg(string id)
        {
            msgs.RemoveAll(m => m.id == id);
            conduit.dirty = true;

            // update any UX
            //BaseForm.get<FormPlayComms>()?.onQuestChanged(this);
            //PlotBase2.invalidate(nameof(PlotQuestMini));
        }

        public void keepLast(params string[] names)
        {
            var newKeptLasts = new Dictionary<string, IJournalEntry?>();

            // (keeping any prior references)
            foreach (var name in names)
                newKeptLasts[name] = this.keptLasts.GetValueOrDefault(name);

            this.keptLasts = newKeptLasts;
        }

        class KeptLastsJsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var raw = serializer.Deserialize<Dictionary<string, JObject?>>(reader);
                if (raw == null) throw new Exception($"Unexpected value: ");

                var keptLasts = raw.ToDictionary(kv => kv.Key, kv => JournalFile.hydrate(kv.Value) as IJournalEntry);
                return keptLasts;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var keptLasts = value as Dictionary<string, IJournalEntry?>;
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
    }

    /// <summary> The runtime/persisted state for an individual quest objective </summary>
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

                // eg: "visible"
                // eg: "complete"
                // eg: "visible,1,10"
                var parts = txt.Split(',', StringSplitOptions.TrimEntries);
                return new PlayObjective()
                {
                    state = Enum.Parse<PlayObjective.State>(parts[0]),
                    current = parts.Length != 3 ? 0 : int.Parse(parts[1]),
                    total = parts.Length != 3 ? 0 : int.Parse(parts[2]),
                };
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var objective = value as PlayObjective;
                if (objective == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                // only include numbers if they are both not zero
                var json = objective.current == 0 && objective.total == 0
                    ? $"{objective.state}"
                    : $"{objective.state},{objective.current},{objective.total}";

                writer.WriteValue(json);
            }
        }
    }

    /// <summary> The runtime/persisted state of a chapter </summary>
    public class PlayChapter
    {
        [JsonIgnore] public PlayQuest parent;
        [JsonIgnore] public ScriptState scriptState;

        #region data members

        public string id;
        /// <summary> When this chapter last started </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? startTime;
        /// <summary> When this chapter last ended </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? endTime;
        /// <summary> Persisted values of script variables </summary>
        public Dictionary<string, object> vars = [];

        #endregion

        /// <summary> Returns true if this chapter is currently active </summary>
        [JsonIgnore] public bool active => !endTime.HasValue && startTime.HasValue;

        /// <summary> Pull variable values from scriptState into a dictionary, ready for saving. Ignore any variables named with an initial `_` </summary>
        public void pullScriptVars()
        {
            foreach (var scriptVar in scriptState.Variables)
                if (!scriptVar.Name.StartsWith("_"))
                    this.vars[$"{scriptVar.Name}|{scriptVar.Type}"] = scriptVar.Value;
        }

        /// <summary> Push saved variable values into scriptState </summary>
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

    /// <summary> A runtime/delivered message. Content fields will be null unless they are overriding statically declared values </summary>
    public class PlayMsg
    {
        [JsonIgnore] public PlayQuest parent;

        public string id;
        public DateTimeOffset received;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? from;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? subject;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? body;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? chapter;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[]? actions;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool read;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? replied;

        public override string ToString()
        {
            return $"{id}:{subject ?? body} ({received:z})";
        }

        public static PlayMsg send(Msg? msg = null, string? from = null, string? subject = null, string? body = null, string? chapter = null)
        {
            if (msg?.actions != null && chapter == null) throw new Exception($"Chapter must be set when using messages with actions. Id: {msg.id}");

            var newMsg = new PlayMsg()
            {
                id = msg?.id ?? DateTimeOffset.UtcNow.ToString("yyyyMMddhhmmss"),
                received = DateTimeOffset.UtcNow,
                chapter = chapter,
                actions = msg?.actions?.Keys.ToArray(),
            };

            // store nothing if the following values match the template Msg

            newMsg.from = from == null || from == msg?.from ? null : from ?? msg!.from;
            newMsg.subject = subject == null || subject == msg?.subject ? null : subject ?? msg!.subject;
            newMsg.body = body == null || body == msg?.body ? null : body ?? msg!.body;

            return newMsg;
        }
    }

    /// <summary> Represents a trackabale location at some lat/long value, with a given size radius. </summary>
    [JsonConverter(typeof(LatLong3.JsonConverter))]
    public class LatLong3
    {
        public double lat;
        public double @long;
        public double size;

        public LatLong3() { }

        public LatLong3(double lat, double @long, double size)
        {
            this.lat = lat;
            this.@long = @long;
            this.size = size;
        }

        public override string ToString()
        {
            return $"{lat},{@long},{size}";
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                // eg: "12.34,56.78,50"
                var parts = txt.Split(',', StringSplitOptions.TrimEntries);
                return new LatLong3()
                {
                    lat = double.Parse(parts[0], CultureInfo.InvariantCulture),
                    @long = double.Parse(parts[1], CultureInfo.InvariantCulture),
                    size = double.Parse(parts[2], CultureInfo.InvariantCulture),
                };
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is LatLong3)
                    writer.WriteValue(value.ToString());
                else
                    throw new Exception($"Unexpected value: {value?.GetType().Name}");
            }
        }
    }
}
