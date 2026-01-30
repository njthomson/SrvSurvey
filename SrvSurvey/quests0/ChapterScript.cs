using SrvSurvey.game;

namespace SrvSurvey.quests0.scripting
{
    /// <summary>
    /// The "script globals" reference given to running scripts, enabling them to call into SrvSurvey code. There is one instance per each active chapter.
    /// </summary>
    public class ChapterScript
    {
        public S_quest quest;
        public S_chapter chapter;
        public S_objective objective;
        public S_game game;

        /// <summary> The ID of the chapter </summary>
        private string id;
        private Conduit c;

        public ChapterScript(Conduit c, string id)
        {
            this.c = c;
            this.id = id;
            this.quest = new(c, this);
            this.chapter = new(c, this);
            this.objective = new(c, this);
            this.game = new(c, this);
        }

        // functions availble to scripting ...

        /// <summary> Used by PlayState infrastructure to track references to specific functions. This is not intended to be called by quest script authors </summary>
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


        public class S_quest : SQuest
        {
            Conduit c;
            ChapterScript sg;

            public S_quest(Conduit c, ChapterScript sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void complete()
            {
                Game.log($"S_quest.complete [{sg.id}]");
                c.pq.complete();
                c.dirty = true;
            }

            public void fail()
            {
                Game.log($"S_quest.fail [{sg.id}]");
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

                Game.log($"S_quest.sendMsg [{sg.id}]: {id}: {newMsg.subject ?? newMsg.body}");
            }

            public void tag(params string[] tags)
            {
                foreach (var tag in tags)
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

            public void trackLocation(string name, double lat, double @long, float size)
            {
                c.pq.bodyLocations[name] = new LatLong3(lat, @long, size);
                c.dirty = true;
            }

            public void clearLocation(string name)
            {
                c.dirty |= c.pq.bodyLocations.Remove(name);
            }

            public void clearAllLocations()
            {
                c.pq.bodyLocations.Clear();
                c.dirty = true;
            }

            public void keepLast(params string[] eventNames)
            {
                Game.log($"keepLast [{sg.id}]: {eventNames}");
                c.pq.keepLast(eventNames);
                c.dirty = true;
            }

            public T? getLast<T>() where T : JournalEntry
            {
                var name = typeof(T).Name;
                var last = c.pq.keptLasts.GetValueOrDefault(name);
                return last as T;
            }
        }

        public class S_chapter : SChapter
        {
            Conduit c;
            ChapterScript sg;

            public S_chapter(Conduit c, ChapterScript sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void start(string name)
            {
                Game.log($"S_chapter.start [{sg.id}]: {name}");
                c.pq.startChapter(name);
            }

            public void stop(string? name = null)
            {
                name ??= sg.id;
                Game.log($"S_chapter.stop [{sg.id}]: {name}");
                c.pq.stopChapter(name);
            }

            public void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null)
            {
                // do we have a pre-published message?
                var msg = id == null ? null : c.pq.quest.msgs.Find(m => m.id == id);

                // create a delivered message from it, with chapterId, overriding strings as necessary
                var newMsg = PlayMsg.send(msg, from, subject, body, this.sg.id);
                c.pq.sendMsg(newMsg);

                Game.log($"S_chapter.sendMsg [{sg.id}]: {id}: {newMsg.subject ?? newMsg.body}");
            }
        }

        public class S_objective : SObjective
        {
            Conduit c;
            ChapterScript sg;

            public S_objective(Conduit c, ChapterScript sg)
            {
                this.c = c;
                this.sg = sg;
            }

            public void show(string name, int progress = -1, int total = -1)
            {
                Game.log($"S_objective.show [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.visible;
                c.dirty = true;

                if (progress != -1 && total != -1)
                    this.progress(name, progress, total);
            }

            public void hide(string name)
            {
                Game.log($"S_objective.hide [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.hidden;
                c.dirty = true;
            }

            public void complete(string name)
            {
                Game.log($"S_objective.complete [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.complete;
                c.dirty = true;
            }

            public void fail(string name)
            {
                Game.log($"S_objective.fail [{sg.id}]: {name}");
                c.pq.objectives.init(name).state = PlayObjective.State.failed;
                c.dirty = true;
            }

            public void progress(string name, int current, int total)
            {
                Game.log($"S_objective.progress [{sg.id}]: {name} => {current} of {total}");
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
            ChapterScript sg;

            public S_game(Conduit c, ChapterScript sg)
            {
                this.c = c;
                this.sg = sg;
            }

            // TODO: It would be much safer to expose an explicitly limited representation of state.json, rather than expose the raw object SrvSurvey already uses
            public Status status => Game.activeGame!.status;


        }
    }

    /// <summary>
    /// The conduit between ChapterScripts and persisted quest states. There is one instance per quest.
    /// TODO: Maybe merge with PlayQuest and remove the need for a separate Conduit class?
    /// </summary>
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
