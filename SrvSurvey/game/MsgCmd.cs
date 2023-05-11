﻿namespace SrvSurvey.game
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
        public const string align = ".align";
        /// <summary> PlotGuardians : invoke map mode </summary>
        public const string map = ".map";
        /// <summary> PlotGuardians : invoke site heading mode </summary>
        public const string heading = ".heading";
        /// <summary> PlotGuardians : set site  type '.site alpha' </summary>
        public const string site = ".site";

        /// <summary> Main : set current location as trackign target </summary>
        public const string targetHere = ".target here";
        /// <summary> Main : hide tracking target plotter </summary>
        public const string targetOff = ".target off";
        /// <summary> Main : show tracking target plotter </summary>
        public const string targetOn = ".target on";
    }
}