using Lua;

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
    public async Task startChapter(string id)
    {
        await pq.startChapter(id);
    }

    [LuaMember]
    public async Task nextChapter(string id)
    {
        await pq.startChapter(id);
        await pq.stopChapter(pq.invokingChapter!.id);
    }

    [LuaMember]
    public async Task stopChapter(string id)
    {
        await pq.stopChapter(id);
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

        pq.sendMsg(playMsg);
    }

    [LuaMember]
    public bool deleteMsg(string id)
    {
        return pq.deleteMsg(id);
    }

    [LuaMember]
    public void tag(string t1, string? t2 = null, string? t3 = null, string? t4 = null, string? t5 = null, string? t6 = null, string? t7 = null, string? t8 = null)
    {
        tags(t1, t2, t3, t4, t5, t6, t7, t8);
    }

    public void tags(params string?[] tags)
    {
        foreach (var tag in tags)
            if (!string.IsNullOrWhiteSpace(tag))
                pq.dirty |= pq.tags.Add(tag);
    }

    [LuaMember]
    public void untag(string t1, string? t2 = null, string? t3 = null, string? t4 = null, string? t5 = null, string? t6 = null, string? t7 = null, string? t8 = null)
    {
        untags(t1, t2, t3, t4, t5, t6, t7, t8);
    }

    public void untags(params string?[] tags)
    {
        foreach (var tag in tags)
            if (!string.IsNullOrWhiteSpace(tag))
                pq.dirty |= pq.tags.Remove(tag);
    }

    [LuaMember]
    public void setTags(string t1, string? t2 = null, string? t3 = null, string? t4 = null, string? t5 = null, string? t6 = null, string? t7 = null, string? t8 = null)
    {
        clearTags();
        this.tags(t1, t2, t3, t4, t5, t6, t7, t8);
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
    public void keepLast(string e1, string? e2 = null, string? e3 = null, string? e4 = null, string? e5 = null, string? e6 = null, string? e7 = null, string? e8 = null)
    {
        var names = new string?[] { e1, e2, e3, e4, e5, e6, e7, e8 }
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToArray();

        pq.keepLast(names);
    }
}
