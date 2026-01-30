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

    private void setState(string id, PlayObjective.State newState)
    {
        var obj = pq.objectives.init(id);
        if (obj.state != newState)
        {
            obj.state = newState;
            pq.dirty = true;
        }
    }

    [LuaMember]
    public void complete(string id)
    {
        Game.log($"S_objective.complete [{pq.id}]: {id}");
        setState(id, PlayObjective.State.complete);
    }

    [LuaMember]
    public void failed(string id)
    {
        Game.log($"S_objective.complete [{pq.id}]: {id}");
        setState(id, PlayObjective.State.complete);
    }

    [LuaMember]
    public void hide(string id)
    {
        Game.log($"S_objective.complete [{pq.id}]: {id}");
        setState(id, PlayObjective.State.hidden);
    }

    [LuaMember]
    public void show(string id, int current = -1, int total = -1)
    {
        Game.log($"S_objective.show [{pq.id}]: {id}");
        setState(id, PlayObjective.State.visible);

        if (current != -1 && total != -1)
            this.progress(id, current, total);
    }

    [LuaMember]
    public void progress(string id, int current, int total)
    {
        Game.log($"S_objective.progress [{id}]: {id} => {current} of {total}");

        var obj = pq.objectives.init(id);

        if (obj.current != current)
        {
            obj.current = current;
            pq.dirty = true;
        }

        if (obj.total != total)
        {
            obj.total = total;
            pq.dirty = true;
        }
    }

    [LuaMember]
    public void remove(string id)
    {
        Game.log($"S_objective.remove[{pq.id}]: {id}");

        pq.dirty |= pq.objectives.Remove(id);
    }

    [LuaMember]
    public bool isActive(string id)
    {
        Game.log($"S_objective.remove[{pq.id}]: {id}");
        var isVisible = pq.objectives.GetValueOrDefault(id)?.state == PlayObjective.State.visible;
        return isVisible;
    }
}
