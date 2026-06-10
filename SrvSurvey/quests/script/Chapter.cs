using Lua;

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
    public void stop()
    {
        pc.pq.stopChapter(pc.id);
    }
}

