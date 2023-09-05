using SrvSurvey.canonn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        // At a landing pad or on foot within some enclosed space
        Social,

        MainMenu,
        FSDJumping,

        Unknown,
    }

    delegate void GameModeChanged(GameMode newMode, bool force);
    delegate void GameNearingBody(LandableBody nearBody);
    delegate void GameDepartingBody(LandableBody nearBody);
    delegate void StatusFileChanged(bool blink);

    public interface ILocation
    {
        double Longitude { get; set; }
        double Latitude { get; set; }
    }

    public enum SuitType
    {
        flightSuite,
        artiemis,
        maverick,
        dominator,
    }

    internal class SystemStatus
    {
        public string name;
        public long address;
        public int bodyCount;
        public bool honked;
        public bool fssComplete;
        public Dictionary<string, string> fssBodies = new Dictionary<string, string>();
        public HashSet<string> dssBodies = new HashSet<string>();
        public Dictionary<int, string> bodyIds = new Dictionary<int, string>();
        public Dictionary<int, int> bioBodies = new Dictionary<int, int>();
        public Dictionary<int, int> bioScans = new Dictionary<int, int>();
        public int scannedOrganics;

        public static SystemStatus from(FSDJump entry)
        {
            var systemStatus = new SystemStatus(entry.StarSystem, entry.SystemAddress);
            return systemStatus;
        }

        public static SystemStatus from(Location entry)
        {
            var systemStatus = new SystemStatus(entry.StarSystem, entry.SystemAddress);
            return systemStatus;
        }

        public SystemStatus(string name, long address)
        {
            this.name = name;
            this.address = address;
        }

        public void initFromJournal(Game game)
        {
            Game.log($"initFromJournal.walk: begin");

            game.journals?.walk(-1, true, (entry) =>
            {
                // stop at FSDJump's
                if (entry is FSDJump) return true;

                this.Journals_onJournalEntry(entry);
                return false;
            });

            // TODO: go deep?

            Game.log($"initFromJournal.walk: found FSS: {this.fssComplete}, bodyCount: {this.bodyCount}, count FSS: {this.fssBodies.Count}, count DSS: {this.dssBodies.Count}, count bio bodies: {this.bioBodies.Count}");
        }

        public static bool showPlotter
        {
            get
            {
                return Game.settings.autoShowPlotSysStatus
                    && Game.activeGame != null
                    && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                    // show only after honking
                    && Game.activeGame.systemStatus.honked
                    // hide if FSS is complete but we scanned no systems - means FSS was done previously
                    ; //&& (!Game.activeGame.systemStatus.fssComplete || Game.activeGame.systemStatus.fssBodies.Count > 0);
                      //&& Game.activeGame.systemStatus.dssRemaining.Count > 0
                      //&& Game.activeGame.systemStatus.bioBodies.Count > 0;
            }
        }

        private void Journals_onJournalEntry(JournalEntry entry) { this.onJournalEntry((dynamic)entry); }
        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        public void onJournalEntry(FSSDiscoveryScan entry)
        {
            // Discovery scan a.k.a. honk
            if (entry.SystemAddress != this.address) return;

            this.honked = true;
            this.bodyCount = entry.BodyCount;

            if (entry.Progress == 1)
                this.fssComplete = true;

            if (SystemStatus.showPlotter)
                Program.showPlotter<PlotSysStatus>();
        }

        public void onJournalEntry(FSSAllBodiesFound entry)
        {
            // FSS complete
            if (entry.SystemAddress != this.address) return;

            this.fssComplete = true;
        }

        public void onJournalEntry(Scan entry)
        {
            if (entry.SystemAddress != this.address) return;
            this.bodyIds[entry.BodyID] = entry.Bodyname;

            var bodyType = entry.StarType != null ? "star" : entry.PlanetClass;

            if (bodyType != null) // ? entry.ScanType == "Detailed"
                this.fssBodies[entry.Bodyname] = bodyType;
        }

        public void onJournalEntry(SAAScanComplete entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) return;
            this.bodyIds[entry.BodyID] = entry.BodyName;

            this.dssBodies.Add(entry.BodyName);
        }

        public void onJournalEntry(SAASignalsFound entry)
        {
            // DSS found body signals
            if (entry.SystemAddress != this.address) return;
            this.bodyIds[entry.BodyID] = entry.BodyName;

            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null)
                this.bioBodies[entry.BodyID] = bioSignals.Count;
        }

        public void onJournalEntry(FSSBodySignals entry)
        {
            // FSS found body signals
            if (entry.SystemAddress != this.address) return;
            this.bodyIds[entry.BodyID] = entry.Bodyname;

            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null)
                this.bioBodies[entry.BodyID] = bioSignals.Count;
        }

        public void onJournalEntry(ScanOrganic entry)
        {
            if (entry.SystemAddress != this.address) return;
            if (entry.ScanType != ScanType.Analyse) return;

            this.scannedOrganics += 1;

            if (!this.bioScans.ContainsKey(entry.Body))
                this.bioScans[entry.Body] = 0;

            this.bioScans[entry.Body]++;
        }

        public List<string> dssRemaining
        {
            get
            {
                var bodies = this.fssBodies
                    .Where(_ => _.Value != "star" && !this.dssBodies.Contains(_.Key));

                if (Game.settings.skipGasGiantDSS)
                    bodies = bodies
                        .Where(_ => !_.Value.Contains("giant", StringComparison.OrdinalIgnoreCase) && !this.dssBodies.Contains(_.Key));

                return bodies
                    .Select(_ => _.Key.Replace(this.name, "").Replace(" ", ""))
                    .Order()
                    .ToList();
            }
        }

        public List<string> bioRemaining
        {
            get
            {
                return this.bioBodies
                    .Where(_ => this.bioScans.ContainsKey(_.Key) && this.bioScans[_.Key] >= _.Value)
                    .Select(_ => this.bodyIds[_.Key].Replace(this.name, "").Replace(" ", ""))
                    .Order()
                    .ToList();
            }
        }

        public int sumOrganicSignals { get => this.bioBodies.Values.Sum(); }

    }
}
