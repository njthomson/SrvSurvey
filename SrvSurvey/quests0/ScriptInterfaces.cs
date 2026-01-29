using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.quests.scripting
{
    // --- start copy for interfaces.csx ---

    interface SQuest
    {
        /// <summary> Mark the quest as successfully complete </summary>
        void complete();
        /// <summary> Mark the quest as unsuccessfully complete </summary>
        void fail();
        /// <summary> Deliver a message to the player </summary>
        void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null);

        /// <summary> Add some tags </summary>
        void tag(params string[] tags);
        /// <summary> Remove some tags </summary>
        void untag(params string[] tags);
        /// <summary> Replace tags with what is given </summary>
        void setTags(params string[] tags);
        /// <summary> Remove all tags </summary>
        void clearTags();

        /// <summary> Start tracking a specific location (on any body). This will show in various overlays and will highlight when a player is within `size` meters </summary>
        void trackLocation(string id, double lat, double @long, float size);
        /// <summary> Removed a specific tracked location </summary>
        void clearLocation(string id);
        /// <summary> Clear all tracked locations </summary>
        void clearAllLocations();

        /// <summary> Tells the system to keep the last reference of named journal events </summary>
        void keepLast(params string[] eventNames);

        /// <summary> Returns the last seen instance of a journal event, or null if not yet seen </summary>
        T? getLast<T>() where T : JournalEntry;
    }

    interface SChapter
    {
        /// <summary> Start processing the named chapter. If it has a function `onStart`, it will be run </summary>
        void start(string name);
        /// <summary> Stops the named chapter, or current chapter if no name is given </summary>
        void stop(string? name = null);
        /// <summary> Deliver a message to the player </summary>
        void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null);
    }

    interface SObjective
    {
        /// <summary> Show the named objective as active. Optionally including progress and a total</summary>
        public void show(string name, int current = -1, int total = -1);
        /// <summary> Hide the named objective </summary>
        public void hide(string name);
        /// <summary> Show the named objective as completed</summary>
        public void complete(string name);
        /// <summary> Show the named objective as failed </summary>
        public void fail(string name);
        /// <summary> Update current and total values for the named objective </summary>
        public void progress(string name, int current, int total);
        /// <summary> Returns true if the named objective is active </summary>
        public bool isActive(string name);
    }

    // --- end copy for interfaces.csx ---
}
