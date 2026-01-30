using Lua;
using SrvSurvey;
using SrvSurvey.game;

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
        Game.log($"S_quest.complete [{pq.id}]");
        pq.complete();
    }

    [LuaMember]
    public void fail()
    {
        Game.log($"S_quest.fail [{pq.id}]");
        pq.fail();
    }

    [LuaMember]
    public async Task startChapter(string id)
    {
        await pq.startChapter(id);
    }

    [LuaMember]
    public async Task stopChapter(string id)
    {
        await pq.stopChapter(id);
    }

    [LuaMember]
    public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
    {
        // do we have a pre-published message?
        var msg = id == null ? null : pq.quest.msgs.Find(m => m.id == id);
        if (msg?.actions != null)
            throw new Exception($"quest.sendMsg() does not accept messages with actions");

        // create a delivered message from it, overriding strings as necessary
        var newMsg = PlayMsg.send(msg, from, subject, body);
        pq.sendMsg(newMsg);

        Game.log($"S_quest.sendMsg [{pq.id}]: {id}: {newMsg.subject ?? newMsg.body}");
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

    [LuaMember]
    public LuaValue getLast(string eventName)
    {
        // TODO: check if eventName is valid?

        var last = pq.keptLasts.GetValueOrDefault(eventName);
        if (last == null)
            return LuaValue.Nil;

        return new LuaValue(last);
    }
}
