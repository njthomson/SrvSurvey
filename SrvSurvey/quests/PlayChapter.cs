using Lua;
using Lua.Runtime;
using Lua.Standard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SrvSurvey.quests;

/// <summary> The runtime/persisted state of a chapter </summary>
public class PlayChapter
{
    [JsonIgnore] public PlayQuest pq;
    [AllowNull] private LuaState state;
    [AllowNull] private LuaClosure closure;

    private HashSet<string> journalFuncs = [];
    private HashSet<string> varNames = [];
    private List<Action> pending = [];

    #region data members

    public required string id;

    /// <summary> When this chapter last started </summary>
    public DateTimeOffset? startTime;

    /// <summary> When this chapter last ended </summary>
    public DateTimeOffset? endTime;

    /// <summary> Persisted values of script variables </summary>
    public Dictionary<string, LuaValue> vars = [];

    #endregion

    [SetsRequiredMembers]
    public PlayChapter(string id, PlayQuest pq)
    {
        this.id = id;
        this.pq = pq;
    }

    private string? src => pq.quest.chapters.GetValueOrDefault(id);

    /// <summary> Returns true if this chapter is currently active </summary>
    [JsonIgnore] public bool active => !endTime.HasValue && startTime.HasValue;

    public override string ToString()
    {
        return $"chapterId: {id}";
    }

    public async Task<LuaState> load()
    {
        if (string.IsNullOrEmpty(src) || !active)
            return state;
        Game.log($"PlayChapter.load: {id}");

        // prep state
        state = LuaState.Create();
        state.Environment["chapterId"] = this.id;
        await prepScriptLibraries();

        // collect names of everything in Environment prior to loading the script
        var priorNames = state.Environment
            .Select(x => x.Key)
            .ToHashSet();

        // compile and run the script
        closure = state.Load(src, "@optimized");
        await state.RunAsync(closure);
        var env = state.GetCurrentEnvironment();

        // capture functions intended for Journal events, with suitable names "on_xxx"
        journalFuncs.Clear();
        foreach (var (key, value) in env)
        {
            if (value.Type != LuaValueType.Function) continue;
            var name = key.Read<string>();

            if (!name.StartsWith("on_")) continue;
            name = name.Substring(3);
            journalFuncs.Add(name);
        }

        // capture variable names (anything added since priorNames that is not a function)
        varNames = env
            .Where(x => x.Key.Type == LuaValueType.String && x.Value.Type != LuaValueType.Function && !priorNames.Contains(x.Key))
            .Select(x => x.Key.Read<string>())
            .ToHashSet();

        // push first: to preserve prior values, then pull: to capture anything new
        pushScriptVars();
        pullScriptVars();

        return state;
    }

    private async Task prepScriptLibraries()
    {
        state.OpenStandardLibraries(); // <-- REVISIT

        // re-map 'print' to our own lodding
        state.Environment["print"] = new LuaFunction("func1", (context, ct) =>
        {
            var args = context.Arguments.ToArray().Select(x => x.ToString()).ToList();
            Game.log($"-- {pq?.id}/{id} -- " + string.Join(", ", args));
            return new();
        });

        // inject a helper for getting array lengths
        await state.DoStringAsync(@"
function arrlen(tt)
    local count = 0
    for _ in pairs(tt) do
        count = count + 1
    end
    return count
end");

        // add our own library methods
        state.Environment["quest"] = new script.Quest(this.pq);
        state.Environment["objective"] = new script.Objective(this.pq);
        state.Environment["chapter"] = new script.Chapter(this);
        state.Environment["cmdr"] = new script.Cmdr(this);
    }

    /// <summary> Pull variable values into a dictionary, ready for saving.
    public void pullScriptVars()
    {
        if (!active || state == null) return;

        var env = state.GetCurrentEnvironment();
        foreach (var name in varNames)
            this.vars[name] = env[name];
    }

    /// <summary> Push saved variable values into script state </summary>
    public void pushScriptVars()
    {
        if (state == null) return;

        foreach (var (key, value) in vars)
            state.Environment[key] = value;
    }

    public async Task start()
    {
        if (active) return;

        // (re)set time fields
        startTime = DateTimeOffset.UtcNow;
        endTime = null;

        if (state == null)
            state = await this.load();

        var hasOnStart = state.Environment.ContainsKey(LuaFunc.onStart);
        Game.log($"Starting chapter: {id}, run onStart: {hasOnStart}");
        if (hasOnStart)
        {
            await invokeLuaFunc(LuaFunc.onStart, new LuaValue[] { });
            pq.dirty = true;
        }
    }

    public void stop()
    {
        if (!active) return;

        if (state.IsRunning)
        {
            // do this later if we are actively running
            pending.Add(() =>
            {
                stop();
            });
            return;
        }

        // wait if it is still running !
        Game.log($"Stopping chapter: {id}");
        endTime = DateTimeOffset.UtcNow;
        state.Dispose();
        state = null;

        pq.dirty = true;
    }

    public void doPendings()
    {
        if (pending.Count == 0) return;

        foreach (var t in pending)
        {
            t();
        }

        pending.Clear();
    }

    internal async Task<string> runDebug(string code)
    {
        if (!active || state == null) return "Chapter not active";

        var rslt = await state.DoStringAsync(code);
        var json = JsonConvert.SerializeObject(rslt.FirstOrDefault());

        doPendings();

        return json;
    }

    public async Task<bool> processJournalEntry(LuaTable entry, JObject raw)
    {
        var eventName = entry["event"].ToString();
        if (!active || !journalFuncs.Contains(eventName)) return false;

        var funcName = $"on_{eventName}";
        if (!state.Environment.ContainsKey(funcName)) throw new Exception($"Missing function: {funcName} ?");

        // invoke the function
        var shouldSave = await invokeLuaFunc(funcName, new LuaValue[] { entry });

        // special case for easier emote/gesture processing
        if (eventName == "ReceiveText" && state.Environment.ContainsKey(LuaFunc.onEmote))
        {
            var msg = entry["Message"].ToString();
            if (msg.StartsWith("$HumanoidEmote_"))
                shouldSave |= await processHumanoidEmoteMessage(msg);
        }

        return shouldSave;
    }

    private async Task<bool> invokeLuaFunc(string funcName, LuaValue[] args)
    {
        try
        {
            if (!state.Environment[funcName].TryRead<LuaFunction>(out var func))
                throw new Exception($"Missing function '{funcName}'\r\n\t(args: {string.Join(", ", args)})");

            Game.log($"[{pq.id}/{id}] Invoking: {funcName}");
            if (this.pq.invokingChapter != null) Debugger.Break();
            this.pq.invokingChapter = this;
            var rslt = await state.CallAsync(func, args);
            var shouldSave = rslt.Length > 0 && rslt[0] != LuaValue.Nil && rslt[0].ToString() != "false";
            return shouldSave;
        }
        catch (Exception ex)
        {
            Game.log($"Quest script error: {ex.Message}\r\n\t{ex.StackTrace}");
            if (DialogResult.Yes == MessageBox.Show(ex.Message + "\r\n\r\nDebug?", "LUA Error", MessageBoxButtons.YesNo))
                Debugger.Break();
            return false;
        }
        finally
        {
            this.pq.invokingChapter = null;
        }
    }

    private static Regex emotePart = new Regex("=(.+)$", RegexOptions.Compiled);
    private static Regex actionPart = new Regex("_(.+?)_", RegexOptions.Compiled);

    private async Task<bool> processHumanoidEmoteMessage(string msg)
    {
        //msg = "$HumanoidEmote_TargetMessage:#player=$cmdr_decorate:#name=Grinning2001;:#targetedAction=$HumanoidEmote_wave_Action_Targeted;:#target=Rodger Reese;";
        //msg = "$HumanoidEmote_TargetMessage:#player=$npc_name_decorate:#name=Connie Dodson;:#targetedAction=$HumanoidEmote_wave_Action_Targeted;:#target=$cmdr_decorate:#name=Grinning2001;;";
        //msg = "$HumanoidEmote_TargetMessage:#player=$cmdr_decorate:#name=Grinning2001;:#targetedAction=$HumanoidEmote_agree_Action_Targeted;:#target=$npc_name_decorate:#name=Jane Christensen;;";
        //msg = "$HumanoidEmote_DefaultMessage:#player=$npc_name_decorate:#name=Briley Hartman;:#action=$HumanoidEmote_wave_Action;;";

        var parts = msg.Split([':', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.RemoveEmptyEntries);
        /* Splits into either:
         *      0:   $HumanoidEmote_TargetMessage
         *      1:   #player=$npc_name_decorate
         *      2:   #name=Connie Dodson
         *      3:   #targetedAction=$HumanoidEmote_wave_Action_Targeted
         *      4:   #target=$cmdr_decorate
         *      5:   #name=Grinning2001
         * 
         * or into:
         *      0:   $HumanoidEmote_DefaultMessage
         *      1:   #player=$npc_name_decorate
         *      2:   #name=Briley Hartman
         *      3:   #action=$HumanoidEmote_wave_Action
         */

        var actor = emotePart.Match(parts[2]).Groups.Values.Last().Value;

        var action = emotePart.Match(parts[3]).Groups.Values.Last().Value;
        action = actionPart.Match(action).Groups.Values.Last().Value;

        var target = parts.Length < 5 ? "" : emotePart.Match(parts.Last()).Groups.Values.Last().Value;

        return await invokeLuaFunc(LuaFunc.onEmote, [actor, action, target]);
    }

    /// <summary> Called by Quest Comms when a player hit a message reply button </summary>
    public async Task invokeMessageAction(string msgId, string actionId)
    {
        if (!active) throw new Exception($"Cannot invoke message action: {actionId}, chapter not active: {id}");

        var pm = pq.msgs.Find(m => m.id == msgId);
        if (pm == null) throw new Exception($"Message not found, id: {msgId}");
        var chapterId = pm.chapter!;

        // invoke the action
        await invokeLuaFunc(LuaFunc.onMsgAction, new LuaValue[] { actionId, msgId });

        // remember which reply was used
        pm.replied = actionId;
        pq.dirty = true;

        PlayState.updateUI(pq);
    }
}
