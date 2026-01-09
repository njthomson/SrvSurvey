using SrvSurvey.game;

namespace SrvSurvey.quests.scripting
{
    public class ScriptGlobals
    {
        public Q quest;
        public C chapter;
        public O objective;

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
        }

        // functions availble to scripting ...

        public void setFunc(string name, Action<object> func)
        {
            Game.log($"setFunc [{id}]: {name}");
            c.funcs[$"{id}.{name}"] = func;
        }

        //public void keepLast(string eventName)
        //{
        //    Game.log($"setFunc [{id}]: {eventName}");
        //    // TODO: ...
        //}

        public class Q
        {
            Conduit c;
            ScriptGlobals sg;

            public Q(Conduit c, ScriptGlobals sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void complete()
            {
                Game.log($"Q.complete [{sg.id}]");
                c.pq.complete();
            }

            public void fail()
            {
                Game.log($"Q.fail [{sg.id}]");
                c.pq.fail();
            }

            public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
            {
                // do we have a pre-published message?
                var msg = id == null ? null : c.pq.quest.msgs.Find(m => m.id == id);
                if (msg?.actions != null)
                    throw new Exception($"quest.sendMsg() does not accept messages with actions");

                // create a delivered message from it, overriding strings as necessary
                var newMsg = PlayMsg.send(msg, from, subject, body);
                c.pq.msgs.Add(newMsg);

                Game.log($"Q.sendMsg [{sg.id}]: {id}: {newMsg.subject ?? newMsg.body}");
            }
        }

        public class C
        {
            Conduit c;
            ScriptGlobals sg;

            public C(Conduit c, ScriptGlobals sg)
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

                // create a delivered message from it, overriding strings as necessary
                var newMsg = PlayMsg.send(msg, from, subject, body);
                c.pq.msgs.Add(newMsg);

                Game.log($"C.sendMsg [{sg.id}]: {id}: {newMsg.subject ?? newMsg.body}");
            }
        }

        public class O
        {
            Conduit c;
            ScriptGlobals sg;

            public O(Conduit c, ScriptGlobals sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void show(string name)
            {
                Game.log($"O.show [{sg.id}]: {name}");
                c.pq.objectives[name].state = PlayObjective.State.visible;
            }

            public void hide(string name)
            {
                Game.log($"O.hide [{sg.id}]: {name}");
                c.pq.objectives[name].state = PlayObjective.State.hidden;
            }

            public void complete(string name)
            {
                Game.log($"O.complete [{sg.id}]: {name}");
                c.pq.objectives[name].state = PlayObjective.State.complete;
            }

            public void fail(string name)
            {
                Game.log($"O.fail [{sg.id}]: {name}");
                c.pq.objectives[name].state = PlayObjective.State.failed;
            }

            public void progress(string name, float progress)
            {
                Game.log($"O.progress [{sg.id}]: {name} => {progress}");
                c.pq.objectives[name].progress = progress;
            }
        }
    }

    public class Conduit
    {
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
