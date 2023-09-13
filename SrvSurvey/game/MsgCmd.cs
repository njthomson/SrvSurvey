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
        /// <summary> Signal an empty puddle </summary>
        public const string empty = ".empty";
        /// <summary> Toggle aiming assistance overlay </summary>
        public const string aim = ".aim";
        /// <summary> Append message to site notes </summary>
        public const string note = ".note";

        /// <summary> Main : set current location as trackign target </summary>
        public const string targetHere = ".target here";
        /// <summary> Main : hide tracking target plotter </summary>
        public const string targetOff = ".target off";
        /// <summary> Main : show tracking target plotter </summary>
        public const string targetOn = ".target on";
        /// <summary> Main : prefix for various track additive commands </summary>
        public const string trackAdd = "+";
        /// <summary> Main : prefix for various track removal commands </summary>
        public const string trackRemove = "-";
        /// <summary> Main : prefix for various track removal commands </summary>
        public const string trackRemoveLast = "=";

        /// <summary> Main : Record that a particular body has been visited </summary>
        public const string visited = ".visited";
        /// <summary> Main : submit a Landscape survey </summary>
        public const string submit = ".submit";
        /// <summary> Main : reserve another block of Landscape survey systems </summary>
        public const string nextSystem = ".nextSystem";
    }
}
