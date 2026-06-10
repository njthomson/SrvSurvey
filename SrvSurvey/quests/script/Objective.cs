using Lua;
using SrvSurvey.game;

namespace SrvSurvey.quests.script;

[LuaObject]
public partial class Objective
{
    private PlayQuest pq;

    public Objective(PlayQuest pq)
    {
        this.pq = pq;
    }

    private void setState(string[] ids, PlayObjective.State? newState, int current = -1, int total = -1)
    {
        Game.log($"[{pq.id}] SO.setState: ({newState}, {current}, {total}) => [{string.Join(", ", ids)}]");

        var dirty = false;
        foreach (var id in ids)
        {
            if (id == null) continue;
            if (!pq.quest.objectives.ContainsKey(id)) throw new Exception($"Unknown objective ID: {id}");

            var obj = pq.objectives.init(id);

            if (obj.state != newState && newState.HasValue)
            {
                obj.state = newState.Value;
                dirty = true;
            }
            if (obj.current != current && current >= 0)
            {
                obj.current = current;
                dirty = true;
            }
            if (obj.total != total && total >= 0)
            {
                obj.total = total;
                dirty = true;
            }
        }
        pq.dirty |= dirty;
    }

    [LuaMember]
    public void complete(LuaValue val)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        setState(ids, PlayObjective.State.complete);
    }

    [LuaMember]
    public void failed(LuaValue val)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        setState(ids, PlayObjective.State.failed);
    }

    [LuaMember]
    public void hide(LuaValue val)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        setState(ids, PlayObjective.State.hidden);
    }

    [LuaMember]
    public void show(LuaValue val, int current = -1, int total = -1)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        setState(ids, PlayObjective.State.visible, current, total);
    }

    [LuaMember]
    public void progress(LuaValue val, int current, int total)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        setState(ids, null, current, total);
    }

    [LuaMember]
    public void remove(LuaValue val)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        Game.log($"[{pq.id}] SO.remove: [{string.Join(", ", ids)}]");

        foreach (var id in ids)
            pq.dirty |= pq.objectives.Remove(id);
    }

    [LuaMember]
    public bool isActive(string id)
    {
        Game.log($"[{pq.id}] SO.isActive: {id}");
        var isVisible = pq.objectives.GetValueOrDefault(id)?.state == PlayObjective.State.visible;
        return isVisible;
    }

    [LuaMember]
    public bool check(LuaValue val, string state)
    {
        if (!Enum.TryParse<PlayObjective.State>(state, out var isState))
            throw new Exception($"Bad objective state: '{state}'. Try: " + string.Join(',', Enum.GetNames<PlayObjective.State>()));

        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToArray()
            : new[] { val.ToString() };

        Game.log($"[{pq.id}] SO.check: [{string.Join(", ", ids)}] == {state}");

        foreach (var id in ids)
        {
            if (!pq.quest.objectives.ContainsKey(id)) throw new Exception($"Unknown objective ID: {id}");

            var objState = pq.objectives.GetValueOrDefault(id)?.state ?? default;
            if (objState != isState)
                return false;
        }
        return true;
    }

    [LuaMember]
    public int getCurrent(string id)
    {
        if (!pq.quest.objectives.ContainsKey(id)) throw new Exception($"Unknown objective ID: {id}");
        return pq.objectives.GetValueOrDefault(id)?.current ?? default;
    }

    [LuaMember]
    public int getTotal(string id)
    {
        if (!pq.quest.objectives.ContainsKey(id)) throw new Exception($"Unknown objective ID: {id}");
        return pq.objectives.GetValueOrDefault(id)?.total ?? default;
    }
}
