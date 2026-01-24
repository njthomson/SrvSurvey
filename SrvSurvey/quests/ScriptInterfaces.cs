using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.quests.scripting
{
    // --- start copy ---

    interface SQuest
    {
        /// <summary> Mark the quest as successfully complete </summary>
        void complete();
        void fail();
        /// <summary> Deliver a message to the player </summary>
        void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null);

        void tag(params string[] tags);
        void untag(params string[] tags);
        void setTags(params string[] tags);
        void clearTags();

        void trackLocation(string name, double lat, double @long, float size);
        void clearLocation(string name);
        void clearAllLocations();
    }

    interface SChapter
    {
        void start(string name);
        void stop(string? name = null);
        /// <summary> Deliver a message to the player </summary>
        void sendMsg(string? id = null, string? from = null, string? subject = null, string? body = null);
    }

    interface SObjective
    {
        public void show(string name, int progress = -1, int total = -1);
        public void hide(string name);
        public void complete(string name);
        public void fail(string name);
        public void progress(string name, int current, int total);
        public bool isActive(string name);
    }

    // --- end copy ---
}
