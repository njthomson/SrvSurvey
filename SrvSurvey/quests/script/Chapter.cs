using Lua;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Text;

namespace SrvSurvey.quests.script;

[LuaObject]
public partial class Chapter
{
    private PlayChapter pc;

    public Chapter(PlayChapter pc)
    {
        this.pc = pc;
    }

    [LuaMember]
    public async Task stop()
    {
        await pc.pq.stopChapter(pc.id);
    }

    [LuaMember]
    public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
    {
        // do we have a pre-published message?
        var msg = id == null ? null : pc.pq.quest.msgs.Find(m => m.id == id);

        // create a delivered message from it, with chapterId, overriding strings as necessary
        var newMsg = PlayMsg.send(msg, from, subject, body, pc.id);
        pc.pq.sendMsg(newMsg);

        Game.log($"S_chapter.sendMsg [{pc.id}]: {id}: {newMsg.subject ?? newMsg.body}");
    }
}

