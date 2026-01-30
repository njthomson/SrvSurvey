using Lua;
using Lua.Runtime;
using Lua.Standard;
using Newtonsoft.Json;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey.quests;

/// <summary> The runtime/persisted state of a chapter </summary>
public class PlayChapter
{
    [JsonIgnore, AllowNull] public PlayQuest pq;
    [AllowNull] private LuaState state;
    [AllowNull] private LuaClosure closure;

    private HashSet<string> journalFuncs = [];
    private HashSet<string> varNames = [];

    #region data members

    public required string id;

    /// <summary> When this chapter last started </summary>
    public DateTimeOffset? startTime;

    /// <summary> When this chapter last ended </summary>
    public DateTimeOffset? endTime;

    /// <summary> Persisted values of script variables </summary>
    public Dictionary<string, LuaValue> vars = [];

    #endregion

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
            // TODO: confirm the name matches a known journal event name
            journalFuncs.Add(name);
        }

        // capture variable names (anything added since priorNames that is not a function)
        varNames = env
            .Where(x => x.Key.Type == LuaValueType.String && x.Value.Type != LuaValueType.Function && !priorNames.Contains(x.Key))
            .Select(x => x.Key.Read<string>())
            .ToHashSet();

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

        var onStartFunc = state.Environment["onStart"];
        if (onStartFunc.Type != LuaValueType.Function) throw new Exception($"Bad LuaType. Expected Function, got: {onStartFunc.Type}");

        Game.log($"Starting chapter: {id}, run onStart: {onStartFunc != LuaValue.Nil}");
        await state.CallAsync(onStartFunc, new LuaValue[] { });
    }

    public void stop()
    {
        if (!active) return;

        Game.log($"Stopping chapter: {id}");
        endTime = DateTimeOffset.UtcNow;
        state = null;
    }

    public async Task<bool> processJournalEntry(LuaTable entry)
    {
        var eventName = entry["event"].ToString();
        if (!active || !journalFuncs.Contains(eventName)) return false;

        var funcName = $"on_{eventName}";
        if (!state.Environment.ContainsKey(funcName))
            throw new Exception($"Missing function: {funcName} ?");

        var func = state.Environment[funcName].Read<LuaFunction>();
        try
        {
            Game.log($"[{pq?.id}/{id}] Invoking: /on_{eventName}");
            var rslt = await state.CallAsync(func, new LuaValue[] { entry });
            var shouldSave = rslt.Length > 0 && rslt[0] != LuaValue.Nil && rslt[0].ToString() != "false";
            return shouldSave;
        }
        catch (Exception ex)
        {
            Game.log($"Quest script error: {ex.Message}\r\n\t{ex.StackTrace}");
            //Debugger.Break();
            return false;
        }
    }

    /// <summary> Called by Quest Comms when a player hit a message reply button </summary>
    public async Task invokeMessageAction(string msgId, string actionId)
    {
        if (!active) throw new Exception($"Cannot invoke message action: {actionId}, chapter not active: {id}");

        var pm = pq.msgs.Find(m => m.id == msgId);
        if (pm == null) throw new Exception($"Message not found, id: {msgId}");
        var chapterId = pm.chapter!;

        // invoke the action
        var onMsgAction = state.Environment["onMsgAction"];
        if (onMsgAction.Type != LuaValueType.Function) throw new Exception($"Missing function 'onMsgAction(string id)' in chapter: {chapterId}");

        Game.log($"Invoking msg actionId: {msgId}/{actionId} in chapter: {id}");
        await state.CallAsync(onMsgAction, new LuaValue[] { actionId });

        // remember which reply was used
        pm.replied = actionId;
        pq.dirty = true;

        // update any UX
        BaseForm.get<FormPlayComms>()?.onQuestChanged(pq);
        PlotBase2.invalidate(nameof(PlotQuestMini));
    }
}
