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
        /// <summary> PlotGuardians : Toggle aiming assistance overlay </summary>
        public const string aim = ".aim";
        /// <summary> PlotGuardians : Append message to site notes </summary>
        public const string note = ".note";
        /// <summary> PlotGuardians : Active Obelisk Groups - set which obelisk groups exist at this site, expecting a comma separated list of letters </summary>
        public const string aog = ".aog";
        /// <summary> PlotGuardians : Active Obelisk - set which items an obelisk is asking for, eg: '.ao ca or' </summary>
        public const string ao = ".ao "; // (the trailing space is necessary)
        /// <summary> PlotGuardians : Active Obelisk Data - set what data an obelisk provides. eg: '.aog a b t g' </summary>
        public const string aod = ".aod "; // (the trailing space is necessary)
        /// <summary> PlotGuardians : Active Obelisk Message - set what Ram Tah mission message an obelisk provides, eg: History $12 '.aom h12' or Biology #9 '.aom 'B9 </summary>
        public const string aom = ".aom "; // (the trailing space is necessary)
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
        /// <summary> PlotTrackers: write debug diagnostics to traces to help solve distance calculations </summary>
        public const string dbgDump = ".dbgdump";

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
    }
}
