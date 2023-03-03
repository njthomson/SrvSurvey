using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{

    /// <summary>
    /// Which vehicle are we currently in?
    /// </summary>
    enum ActiveVehicle
    {
        Unknown,
        MainShip,
        Fighter,
        SRV,
        Foot,
        Taxi,

        Docked, // meaning - not in any of the above
    }

    /// <summary>
    /// A union of various enums and states that should be mutually exclusive.
    /// </summary>
    enum GameMode
    {
        // These are an exact match to GuiFocus from Status
        NoFocus = 0, // meaning ... playing the game
        InternalPanel, //(right hand side)
        ExternalPanel, // (left hand side)
        CommsPanel, // (top)
        RolePanel, // (bottom)
        StationServices,
        GalaxyMap,
        SystemMap,
        Orrery,
        FSS,
        SAA,
        Codex,

        // These are extra

        // game is not running
        Offline,

        // Commander is in a vehicle
        InFighter,
        InSrv,
        InTaxi,
        OnFoot, // at this level the following are simply "docked": Hangars, SocialSpace or in space or on foot in ground or space stations

        // Commander is in main ship, that is ...
        SuperCruising,
        GlideMode,     // gliding to a planet surface
        Flying,   // in deep space
                  // TODO: FlyingSurface, // either at planet or deep space

        // in the MainShip but not flying
        Landed,

        // At a landing pad or on foot within some enclosed space
        Docked,

        MainMenu,
        FSDJumping,
    }

    delegate void GameModeChanged(GameMode newMode);

    public interface ILocation
    {
        double Longitude { get; set; }
        double Latitude { get; set; }

    }


}
