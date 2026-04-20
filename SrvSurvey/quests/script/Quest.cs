using Lua;
using System.Globalization;

namespace SrvSurvey.quests.script;

[LuaObject]
public partial class Quest
{
    private PlayQuest pq;

    public Quest(PlayQuest pq)
    {
        this.pq = pq;
    }

    [LuaMember]
    public void complete()
    {
        pq.complete();
    }

    [LuaMember]
    public void fail()
    {
        pq.fail();
    }

    [LuaMember]
    public void startChapter(string id)
    {
        pq.startChapter(id);
    }

    [LuaMember]
    public void nextChapter(string id)
    {
        pq.startChapter(id);
        pq.stopChapter(pq.invokingChapter!.id);
    }

    [LuaMember]
    public void stopChapter(string id)
    {
        pq.stopChapter(id);
    }

    [LuaMember]
    public void set(string name, LuaValue val)
    {
        pq.setVar(name, val);
    }

    [LuaMember]
    public LuaValue get(string name)
    {
        return pq.getVar(name);
    }

    [LuaMember]
    public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
    {
        // do we have a prepared message?
        var defMsg = id == null ? null : pq.quest.msgs.Find(m => m.id == id);

        // create a delivered message from it, overriding strings as necessary
        var chapter = this.pq.invokingChapter?.id;
        if (defMsg?.actions != null && chapter == null) throw new Exception($"Chapter must be set when using messages with actions. Id: {defMsg.id}");

        var playMsg = new PlayMsg()
        {
            id = defMsg?.id ?? DateTimeOffset.UtcNow.ToString("yyyyMMddhhmmss"),
            from = from ?? defMsg?.from,
            subject = subject ?? defMsg?.subject,
            body = body ?? defMsg?.body,
            received = DateTimeOffset.UtcNow,
            chapter = chapter,
            actions = defMsg?.actions?.Keys.ToArray(),
        };

        // store nothing if the following values match the template Msg
        if (defMsg != null)
        {
            if (playMsg.from == defMsg.from) playMsg.from = null;
            if (playMsg.subject == defMsg.subject) playMsg.subject = null;
            if (playMsg.body == defMsg.body) playMsg.body = null;
        }

        // TODO: delay by seconds when sending any messages? (But this messes with dirty analysis, and means things happen when invokeingChapter may be null
        pq.sendMsg(playMsg);
    }

    [LuaMember]
    public bool deleteMsg(string id)
    {
        return pq.deleteMsg(id);
    }

    [LuaMember]
    public void tag(LuaValue val)
    {
        var tags = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToHashSet()
            : new() { val.ToString() };

        this.tags(tags);
    }

    private void tags(HashSet<string> tags)
    {
        foreach (var tag in tags)
            if (!string.IsNullOrWhiteSpace(tag))
                pq.dirty |= pq.tags.Add(tag);
    }

    [LuaMember]
    public void untag(LuaValue val)
    {
        var tags = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToHashSet()
            : new() { val.ToString() };

        untags(tags);
    }

    private void untags(HashSet<string> tags)
    {
        foreach (var tag in tags)
            if (!string.IsNullOrWhiteSpace(tag))
                pq.dirty |= pq.tags.Remove(tag);
    }

    [LuaMember]
    public void setTags(LuaValue val)
    {
        var tags = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToHashSet()
            : new() { val.ToString() };

        clearTags();
        this.tags(tags);
    }

    [LuaMember]
    public void clearTags()
    {
        pq.dirty |= pq.tags.Any();
        pq.tags.Clear();
    }

    [LuaMember]
    public void trackLocation(string name, double lat, double @long, float size)
    {
        pq.bodyLocations[name] = new LatLong3(lat, @long, size);
        pq.dirty = true;
    }

    [LuaMember]
    public void clearLocation(string name)
    {
        pq.dirty |= pq.bodyLocations.Remove(name);
    }

    [LuaMember]
    public void clearAllLocations()
    {
        pq.dirty |= pq.bodyLocations.Any();
        pq.bodyLocations.Clear();
    }

    [LuaMember]
    public void keepLast(LuaValue val)
    {
        var ids = val.Type == LuaValueType.Table
            ? val.Read<LuaTable>().ToList().Select(kv => kv.Value.ToString()).ToHashSet()
            : new() { val.ToString() };

        pq.keepLast(ids);
    }

    [LuaMember]
    public void setRoute(string id, double w, LuaTable latLongs)
    {
        this.pq.routes.RemoveAll(pr => pr.id == id);
        var waypoints = latLongs.ToList().Select(kv => kv.Value.ToString().Split(",", StringSplitOptions.TrimEntries).Select(t => double.Parse(t, CultureInfo.InvariantCulture)).ToArray()).ToList();
        this.pq.routes.Add(new()
        {
            id = id,
            w = w,
            wp = waypoints,
        });
        this.pq.dirty = true;
    }

    [LuaMember]
    public void clearRoute(string id)
    {
        this.pq.routes.RemoveAll(pr => pr.id == id);
        this.pq.dirty = true;
    }
}