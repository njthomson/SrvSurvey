namespace SrvSurvey.game
{
    /// <summary>
    /// Strings used in various places for dot commands
    /// </summary>
    public static class MsgCmd
    {
        /// <summary> Game: Close SrvSurvey outright </summary>
        public const string kill = ".kill";

        /// <summary> PlotGuardians : adjust plotter zoom level 'z number' </summary>
        public const string z = "z";

        /// <summary> PlotGuardians : invoke origin alignment mode </summary>
        public const string aerial = ".aerial";
        /// <summary> PlotGuardians : invoke map mode </summary>
        public const string map = ".map";
        /// <summary> PlotGuardians : invoke site heading mode </summary>
        public const string heading = ".heading";
        /// <summary> PlotGuardians : set site  type '.site alpha' </summary>
        public const string site = ".site";
        /// <summary> PlotGuardians : set relic tower heading </summary>
        public const string tower = ".tower";
        /// <summary> PlotGuardians : Signal an empty puddle </summary>
        public const string empty = ".empty";
        /// <summary> PlotGuardians : Append message to site notes </summary>
        public const string note = ".note";
        /// <summary> PlotGuardians : Target Obelisk - set a specific obelisk as the target to aim for </summary>
        public const string to = ".to";
        /// <summary> PlotGuardians : Obelisk Scanned - toggles that an obelisk has been scanned (mostly for Ram Tah mission) </summary>
        public const string os = ".os";
        /// <summary> PlotGuardians : add some POI to the map template directly</summary>
        public const string @new = ".new";
        /// <summary> PlotGuardians : add some POI to the map </summary>
        public const string add = ".add";
        /// <summary> PlotGuardians : remove some POI to the map </summary>
        public const string remove = ".remove";

        /// <summary> Main: Open the images folder </summary>
        public const string imgs = ".imgs";
        /// <summary> Main : set current location as tracking target </summary>
        public const string targetHere = ".target here";
        /// <summary> Main : hide tracking target plotter </summary>
        public const string targetOff = ".target off";
        /// <summary> Main : show tracking target plotter </summary>
        public const string targetOn = ".target on";
        /// <summary> Main : prefix for adding a bookmark by name </summary>
        public const string trackAdd = "+";
        /// <summary> Main : prefix for removing the nearest bookmark by name </summary>
        public const string trackRemove = "-";
        /// <summary> Main : prefix for removing the furthest bookmark by name </summary>
        public const string trackRemoveLast = "=";
        /// <summary> Main : prefix for removing all bookmarks by name </summary>
        public const string trackRemoveName = "--";
        /// <summary> Main : Remove all bookmarks on current body </summary>
        public const string trackRemoveAll = "---";
        /// <summary> PlotTrackers: show a picture of the current thing </summary>
        public const string show = ".show";

        /// <summary> Main : Record that a particular body has been visited </summary>
        public const string visited = ".visited";
        /// <summary> Main : submit a Landscape survey </summary>
        public const string submit = ".submit";
        /// <summary> Main : reserve another block of Landscape survey systems </summary>
        public const string nextSystem = ".nextSystem";
        /// <summary> Main : Record that first footfall on current body </summary>
        public const string firstFoot = ".firstFoot";
        /// <summary> Main : Record that first footfall on current body </summary>
        public const string ff = ".ff";

        /// <summary> PlotHumanSite : For manually initializing a settlement's rotation. Cmdr must be centered on a landing pad first. </summary>
        public const string settlement = ".settlement";
        /// <summary> PlotHumanSite : For manual entry of the threat level, eg: ".threat 1" </summary>
        public const string threat = ".threat";
        /// <summary> PlotHumanSite : Invoked the settlement map editor</summary>
        public const string edit = ".edit";
        /// <summary> PlotHumanSite : Begin survey, tracking settlement mats</summary>
        public const string start = ".start";
        /// <summary> PlotHumanSite : End survey, tracking settlement mats</summary>
        public const string stop = ".stop";
    }
}
