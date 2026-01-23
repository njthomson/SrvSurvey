using SrvSurvey.game;

namespace SrvSurvey.quests.scripting
{
    public class ScriptGlobals
    {
        public S_quest quest;
        public S_chapter chapter;
        public S_objective objective;
        public S_game game;

        /// <summary> The ID of the chapter </summary>
        string id;
        Conduit c;

        public ScriptGlobals(Conduit c, string id)
        {
            this.c = c;
            this.id = id;
            this.quest = new(c, this);
            this.chapter = new(c, this);
            this.objective = new(c, this);
            this.game = new(c, this);
        }

        // functions availble to scripting ...

        public void setFunc(string name, Action<object> func)
        {
            Game.log($"setFunc [{id}]: {name}");
            c.funcs[$"{id}/{name}"] = func;
        }

        public void trace(string txt)
        {
            Game.log(txt);
            // TODO: have a separate trace file per quest? Probably!
        }

        //public void keepLast(string eventName)
        //{
        //    Game.log($"setFunc [{id}]: {eventName}");
        //    // TODO: ...
        //}

        public class S_quest
        {
            Conduit c;
            ScriptGlobals sg;

            public S_quest(Conduit c, ScriptGlobals sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void complete()
            {
                Game.log($"Q.complete [{sg.id}]");
                c.pq.complete();
                c.dirty = true;
            }

            public void fail()
            {
                Game.log($"Q.fail [{sg.id}]");
                c.pq.fail();
                c.dirty = true;
            }

            public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
            {
                // do we have a pre-published message?
                var msg = id == null ? null : c.pq.quest.msgs.Find(m => m.id == id);
                if (msg?.actions != null)
                    throw new Exception($"quest.sendMsg() does not accept messages with actions");

                // create a delivered message from it, overriding strings as necessary
                var newMsg = PlayMsg.send(msg, from, subject, body);
                c.pq.sendMsg(newMsg);

                Game.log($"Q.sendMsg [{sg.id}]: {id}: {newMsg.subject ?? newMsg.body}");
            }

            public void tag(params string[] tags)
            {
                foreach(var tag in tags)
                    c.pq.tags.Add(tag);

                c.dirty = true;
            }

            public void untag(params string[] tags)
            {
                foreach (var tag in tags)
                    c.pq.tags.Remove(tag);

                c.dirty = true;
            }

            public void setTags(params string[] tags)
            {
                c.pq.tags.Clear();
                tag(tags);
            }

            public void clearTags()
            {
                c.pq.tags.Clear();
                c.dirty = true;
            }
        }

        public class S_chapter
        {
            Conduit c;
            ScriptGlobals sg;

            public S_chapter(Conduit c, ScriptGlobals sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void start(string name)
            {
                Game.log($"C.start [{sg.id}]: {name}");
                c.pq.startChapter(name);
            }

            public void stop(string? name = null)
            {
                name ??= sg.id;
                Game.log($"C.stop [{sg.id}]: {name}");
                c.pq.stopChapter(name);
            }

            public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
            {
                // do we have a pre-published message?
                var msg = id == null ? null : c.pq.quest.msgs.Find(m => m.id == id);

                // create a delivered message from it, with chapterId, overriding strings as necessary
                var newMsg = PlayMsg.send(msg, from, subject, body, this.sg.id);
                c.pq.sendMsg(newMsg);

                Game.log($"C.sendMsg [{sg.id}]: {id}: {newMsg.subject ?? newMsg.body}");
            }
        }

        public class S_objective
        {
            Conduit c;
            ScriptGlobals sg;

            public S_objective(Conduit c, ScriptGlobals sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void show(string name, int progress = -1, int total = -1)
            {
                Game.log($"O.show [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.visible;
                c.dirty = true;

                if (progress != -1 && total != -1)
                    this.progress(name, progress, total);
            }

            public void hide(string name)
            {
                Game.log($"O.hide [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.hidden;
                c.dirty = true;
            }

            public void complete(string name)
            {
                Game.log($"O.complete [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.complete;
                c.dirty = true;
            }

            public void fail(string name)
            {
                Game.log($"O.fail [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.failed;
                c.dirty = true;
            }

            public void progress(string name, int current, int total)
            {
                Game.log($"O.progress [{sg.id}]: {name} => {current} of {total}");
                c.pq.objectives.init(name);
                c.pq.objectives[name].current = current;
                c.pq.objectives[name].total = total;
                c.dirty = true;
            }

            public bool isActive(string name)
            {
                var isVisible = c.pq.objectives.GetValueOrDefault(name)?.state == PlayObjective.State.visible;
                return isVisible;
            }
        }

        public class S_game
        {
            Conduit c;
            ScriptGlobals sg;

            public S_game(Conduit c, ScriptGlobals sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public Status status => Game.activeGame!.status;

            public Docked? lastDocked => Game.activeGame!.lastEverDocked;

            // TODO: solve for arbitrary journal events?
        }
    }

    public class Conduit
    {
        public bool dirty = false;
        public readonly PlayQuest pq;
        public readonly Dictionary<string, Action<object>> funcs = new();

        public Conduit(PlayQuest pq)
        {
            if (pq.conduit != null) throw new Exception("Why do we already have Conduit?");
            this.pq = pq;
            pq.conduit = this;
        }
    }
}
