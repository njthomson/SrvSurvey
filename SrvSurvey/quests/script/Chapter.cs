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
    public async Task stop()
    {
        await pc.pq.stopChapter(pc.id);
    }
}

