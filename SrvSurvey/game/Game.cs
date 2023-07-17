using SrvSurvey.canonn;
using SrvSurvey.net;
using SrvSurvey.net.EDSM;
using SrvSurvey.units;
using System.Diagnostics;
using System.Reflection;

namespace SrvSurvey.game
{
    /// <summary>
    /// Represents live state of the game Elite Dangerous
    /// </summary>
    class Game : IDisposable
    {
        static Game()
        {
            Game.logs = new List<string>();
            var releaseVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            Game.log($"SrvSurvey version: {releaseVersion}");

            settings = Settings.Load();
            codexRef = new CodexRef();
            canonn = new Canonn();
            spansh = new Spansh();
            edsm = new EDSM();
        }

        #region logging

        public static void log(object? msg)
        {
            var txt = DateTime.Now.ToString("HH:mm:ss") + ": " + msg?.ToString();

            Debug.WriteLine(txt);

            Game.logs.Add(txt);

            ViewLogs.append(txt);
        }

        public static readonly List<string> logs;

        #endregion

        public static Game? activeGame { get; private set; }
        public static Settings settings { get; private set; }
        public static CodexRef codexRef { get; private set; }
        public static Canonn canonn { get; private set; }
        public static Spansh spansh { get; private set; }
        public static EDSM edsm{ get; private set; }

        public bool initialized { get; private set; }

        /// <summary>
        /// The Commander actively playing the game
        /// </summary>
        public string? Commander { get; private set; }
        public string? fid { get; private set; }
        public bool isOdyssey { get; private set; }
        public string musicTrack { get; private set; }
        public SuitType currentSuitType { get; private set; }

        public JournalWatcher? journals;
        public Status status { get; private set; }

        /// <summary>
        /// Distinct settings for the current commander
        /// </summary>
        public CommanderSettings cmdr;

        public Game(string? cmdr)
        {
            log($"Game .ctor");

            // track this instance as the active one
            Game.activeGame = this;

            if (!this.isRunning) return;

            // track status file changes and force an immediate read
            this.status = new Status(true);

            // initialize from a journal file
            var filepath = JournalFile.getCommanderJournalBefore(cmdr, true, DateTime.MaxValue);
            if (filepath == null)
            {
                Game.log($"No journal files found for: {cmdr}");
                this.isShutdown = true;
                return;
            }

            this.journals = new JournalWatcher(filepath);
            this.initializeFromJournal();
            if (this.isShutdown) return;

            log($"Cmdr loaded: {this.Commander != null}");

            // now listen for changes
            this.journals.onJournalEntry += Journals_onJournalEntry;
            this.status.StatusChanged += Status_StatusChanged;

            Status_StatusChanged(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.nearBody != null)
                {
                    this.nearBody.Dispose();
                    this.nearBody = null;

                }

                if (this.journals != null)
                {
                    this.journals.onJournalEntry -= Journals_onJournalEntry;
                    this.journals.Dispose();
                    this.journals = null;
                }

                if (this.status != null)
                {
                    this.status.StatusChanged -= Status_StatusChanged;
                    this.status.Dispose();
                    this.status = null!;
                }
            }
        }

        #region modes

        public void fireUpdate(bool force = false)
        {
            this.fireUpdate(this.mode, force);
        }

        public void fireUpdate(GameMode newMode, bool force)
        {
            if (Game.update != null) Game.update(newMode, force);
        }

        public event GameNearingBody? nearingBody;
        public event GameDepartingBody? departingBody;
        public static event GameModeChanged? update;

        public bool atMainMenu = false;
        private bool fsdJumping = false;
        public bool isShutdown = false;
        public LandableBody? nearBody;
        /// <summary>
        /// Tracks if we status had Lat/Long last time we knew
        /// </summary>
        private bool statusHasLatLong = false;
        /// <summary>
        /// Tracks BodyName from status - attempting to detect when multiple running games clobber the same file in error
        /// </summary>
        private string? statusBodyName;

        private void checkModeChange()
        {
            // check various things we actively need to know have changed
            if (this._mode != this.mode)
            {
                log($"Mode change {this._mode} => {this.mode}");
                this._mode = this.mode;

                // fire event!
                fireUpdate(this._mode, false);
            }
        }

        private void Status_StatusChanged(bool blink)
        {
            //log("status changed");
            if (this.statusBodyName != null && status!.BodyName != null && status!.BodyName != this.statusBodyName)
            {
                // exit early if the body suddenly switches to something different without being NULL first
                Game.log($"Multiple running games? this.statusBodyName: ${this.statusBodyName} vs status.BodyName: {status.BodyName}");
                // TODO: some planet/moons are close enough together to trigger this - maybe compare the first word from each?
                return;
            }
            this.statusBodyName = status!.BodyName;

            // track if status HasLatLong, toggling behaviours when it changes
            var newHasLatLong = status.hasLatLong;
            if (this.statusHasLatLong != newHasLatLong)
            {
                Game.log($"Changed hasLatLong: {newHasLatLong}, has nearBody: {this.nearBody != null}, nearBody matches: {status.BodyName == this.nearBody?.bodyName}");
                this.statusHasLatLong = newHasLatLong;

                if (!newHasLatLong)
                {
                    // we are departing
                    if (this.departingBody != null && this.nearBody != null)
                    {
                        Game.log($"EVENT:departingBody, from {this.nearBody.bodyName}");

                        // change system locations to be just system (not the body)
                        cmdr.currentBody = null;
                        cmdr.currentBodyId = -1;
                        cmdr.currentBodyRadius = -1;
                        cmdr.Save();

                        // fire event
                        var leftBody = this.nearBody;
                        this.nearBody.Dispose();
                        this.nearBody = null;
                        this.departingBody(leftBody);
                    }
                }
                else
                {
                    if (this.nearBody == null && status.BodyName != null)
                    {
                        // we are approaching - create and fire event
                        this.createNearBody(status.BodyName);
                    }
                }
            }

            this.checkModeChange();
        }

        private GameMode _mode;
        public GameMode mode
        {
            get
            {
                if (!this.isRunning || this.isShutdown)
                    return GameMode.Offline;

                if (this.atMainMenu || this.Commander == null)
                    return GameMode.MainMenu;

                // use GuiFocus if it is interesting
                if (this.status?.GuiFocus != GuiFocus.NoFocus)
                    return (GameMode)(int)this.status!.GuiFocus;

                if (this.fsdJumping)
                    return GameMode.FSDJumping;

                // otherwise use the type of vehicle we are in
                var activeVehicle = this.vehicle;
                if (activeVehicle == ActiveVehicle.Fighter)
                    return GameMode.InFighter;
                if (activeVehicle == ActiveVehicle.SRV)
                    return GameMode.InSrv;
                if (activeVehicle == ActiveVehicle.Taxi)
                    return GameMode.InTaxi;
                if ((this.status.Flags2 & StatusFlags2.OnFootExterior) > 0)
                    return GameMode.OnFoot;

                // otherwise we are in the main ship, travelling ...
                if ((this.status.Flags & StatusFlags.Supercruise) > 0)
                    return GameMode.SuperCruising;
                if ((this.status.Flags2 & StatusFlags2.GlideMode) > 0)
                    return GameMode.GlideMode;

                if ((this.status.Flags & StatusFlags.Landed) > 0)
                    return GameMode.Landed;

                // or we maybe ...
                if ((this.status.Flags & StatusFlags.Docked) > 0)
                    return GameMode.Docked;
                if ((this.status.Flags2 & StatusFlags2.OnFootInStation) > 0)
                    return GameMode.Docked;
                if ((this.status.Flags2 & StatusFlags2.OnFootSocialSpace) > 0)
                    return GameMode.Social;

                // otherwise we must be ...
                if (activeVehicle == ActiveVehicle.MainShip)
                    return GameMode.Flying;

                Game.log($"Unknown game mode? status.Flags: {status.Flags}, status.Flags2: {status.Flags2}");
                return GameMode.Unknown;
            }
        }

        public bool isMode(params GameMode[] modes)
        {
            return modes.Contains(this.mode);
        }

        public ActiveVehicle vehicle
        {
            get
            {
                if (this.status == null)
                    return ActiveVehicle.Unknown;
                else if ((this.status.Flags & StatusFlags.InMainShip) > 0)
                    return ActiveVehicle.MainShip;
                else if ((this.status.Flags & StatusFlags.InFighter) > 0)
                    return ActiveVehicle.Fighter;
                else if ((this.status.Flags & StatusFlags.InSRV) > 0)
                    return ActiveVehicle.SRV;
                else if ((this.status.Flags2 & StatusFlags2.OnFoot) > 0)
                    return ActiveVehicle.Foot;
                else if ((this.status.Flags2 & StatusFlags2.InTaxi) > 0)
                    return ActiveVehicle.Taxi;
                else
                    //return ActiveVehicle.Docked;
                    return ActiveVehicle.Unknown;

                // TODO: do we care about telepresence?
            }
        }

        public bool isLanded
        {
            get => this.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed);
        }

        public bool hidePlottersFromCombatSuits
        {
            get => Game.settings.hidePlottersFromCombatSuits
                && (this.status.Flags2 & StatusFlags2.OnFoot) > 0
                && (this.currentSuitType == SuitType.dominator || this.currentSuitType == SuitType.maverick);
        }

        public bool showBodyPlotters
        {
            get => !this.isShutdown
                && !this.atMainMenu
                && this.isMode(GameMode.SuperCruising, GameMode.Flying, GameMode.Landed, GameMode.InSrv, GameMode.OnFoot, GameMode.GlideMode, GameMode.InFighter)
                && !this.hidePlottersFromCombatSuits;
        }

        public bool showGuardianPlotters
        {
            get => Game.settings.enableGuardianSites
                && this.nearBody?.siteData != null
                && this.nearBody?.siteData.location != null // ??
                && this.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed, GameMode.Flying, GameMode.InFighter)
                && !this.hidePlottersFromCombatSuits
                && this.status.Altitude < 4000;
        }

        #endregion

        #region Process and window handle stuff

        private static IntPtr getGameWindowHandle()
        {
            //this.TopMost = !this.TopMost;
            var procED = Process.GetProcessesByName("EliteDangerous64");
            if (procED.Length > 0)
                return procED[0].MainWindowHandle;
            else
                return IntPtr.Zero;
        }

        /// <summary>
        /// Returns True if the game is actively running
        /// </summary>
        public bool isRunning
        {
            get
            {
                var procED = Process.GetProcessesByName("EliteDangerous64");
                return procED.Length > 0;
            }
        }

        /// <summary>
        /// Returns True if multiple versions of the game are running
        /// </summary>
        public bool isRunningMultiple
        {
            get
            {
                var procED = Process.GetProcessesByName("EliteDangerous64");
                return procED.Length > 1;
            }
        }

        #endregion

        #region start-up / initialization from journals

        private void initializeFromJournal(LoadGame? loadEntry = null)
        {
            log($"Game.initializeFromJournal: BEGIN {this.Commander} (FID:{this.fid}), journals.Count:{journals?.Count}");

            if (loadEntry == null)
                loadEntry = this.journals!.FindEntryByType<LoadGame>(-1, true);

            // read cmdr info
            if (loadEntry != null)
            {
                if (this.Commander == null) this.Commander = loadEntry.Commander;
                if (this.fid == null) this.fid = loadEntry.FID;

                Game.settings.lastCommander = this.Commander;
                Game.settings.lastFid = this.fid;
            }

            // exit early if we are shutdown
            var lastShutdown = journals!.FindEntryByType<Shutdown>(-1, true);
            if (lastShutdown != null)
            {
                log($"Game.initializeFromJournal: EXIT isShutdown! ({Game.activeGame == this})");
                this.isShutdown = true;
                return;
            }

            if (loadEntry != null)
                this.cmdr = CommanderSettings.Load(loadEntry.FID, loadEntry.Odyssey, loadEntry.Commander);

            // if we have MainMenu music - we know we're not actively playing
            var lastMusic = journals.FindEntryByType<Music>(-1, true);
            if (lastMusic != null)
                onJournalEntry(lastMusic);

            // try to find a location from recent journal items
            if (cmdr != null)
            {
                journals.walk(-1, true, (entry) =>
                {
                    var locationEvent = entry as Location;
                    if (locationEvent != null)
                    {
                        this.setLocations(locationEvent);
                        return true;
                    }

                    var jumpEvent = entry as FSDJump;
                    if (jumpEvent != null)
                    {
                        this.setLocations(jumpEvent);
                        return true;
                    }

                    var superCruiseExitEvent = entry as SupercruiseExit;
                    if (superCruiseExitEvent != null)
                    {
                        this.setLocations(superCruiseExitEvent);
                        return true;
                    }
                    return false;
                });
                log($"Game.initializeFromJournal: system: '{cmdr.currentSystem}' (id:{cmdr.currentSystemAddress}), body: '{cmdr.currentBody}' (id:{cmdr.currentBodyId}, r: {Util.metersToString(cmdr.currentBodyRadius)})");
            }

            // if we are near a planet
            if (this.nearBody == null && status.hasLatLong)
            {
                this.createNearBody(status.BodyName);
                //// do lat/long check
                //// are we near a guardian site?
                //journals.searchDeep<ApproachSettlement>((entry) =>
                //{
                //    if (entry != null && entry.SystemAddress == this.nearBody.systemAddress)
                //    {
                //        this.nearBody.onJournalEntry(entry);
                //        this.nearBody.settlements.Count;
                //        return true;
                //    }

                //    return false;
                //},
                //// search only as far as the FSDJump arrival
                //(JournalFile journals) => journals.search((FSDJump _) => true));

                //if (this.nearBody.siteData?.location != null && this.nearBody.siteData.location.Lat == 0)
                //{
                //    // no longer needed?
                //    var lastApproachSettlement = journals.FindEntryByType<ApproachSettlement>(-1, true);
                //    if (lastApproachSettlement != null && lastApproachSettlement.SystemAddress == this.nearBody.systemAddress)
                //    {
                //        this.nearBody.onJournalEntry(lastApproachSettlement);
                //        this.nearBody.guardianSiteCount++;
                //    }
                //}
            }

            // if we have landed, we need to find the last Touchdown location
            if (this.isLanded && this._touchdownLocation == null) // || (status.Flags & StatusFlags.HasLatLong) > 0)
            {
                if (this.nearBody == null)
                {
                    Game.log("Why is nearBody null?");
                    return;
                }

                if (this.mode == GameMode.Landed)
                {
                    var location = journals.FindEntryByType<Location>(-1, true);
                    if (location != null && location.SystemAddress == this.nearBody.systemAddress && location.BodyID == this.nearBody.bodyId)
                    {
                        Game.log($"LastTouchdown from Location: {location}");
                        this.touchdownLocation = location;
                    }
                }

                if (this.touchdownLocation == null)
                {
                    journals.searchDeep(
                        (Touchdown lastTouchdown) =>
                        {
                            Game.log($"LastTouchdown from deep search: {lastTouchdown}");

                            if (lastTouchdown.Body == status!.BodyName)
                            {
                                onJournalEntry(lastTouchdown);
                                return true;
                            }
                            return false;
                        },
                        (JournalFile journals) =>
                        {
                            // stop searching older journal files if we see FSDJump
                            return journals.search((FSDJump _) => true);
                        }
                    );
                }
            }

            log($"Game.initializeFromJournal: END Commander:{this.Commander}, starSystem:{cmdr?.currentSystem}, systemLocation:{cmdr?.lastSystemLocation}, nearBody:{this.nearBody}, journals.Count:{journals.Count}");
            this.initialized = Game.activeGame == this && this.Commander != null;
            this.checkModeChange();
        }

        #endregion

        private void createNearBody(string bodyName)
        {
            if (string.IsNullOrEmpty(this.fid)) return;
            Game.log($"in createNearBody: {bodyName}");

            // exit early if we already have that body
            if (this.nearBody != null && this.nearBody.bodyName == bodyName) return;

            // from Scan
            if (this.nearBody == null)
            {
                // look for a recent Scan event
                journals!.searchDeep(
                    (Scan scan) =>
                    {
                        if (scan.Bodyname == bodyName)
                        {
                            this.nearBody = new LandableBody(
                                this,
                                scan.StarSystem,
                                scan.Bodyname,
                                scan.BodyID,
                                scan.SystemAddress,
                                scan.Radius);
                            return true;
                        }
                        return false;
                    },
                    (JournalFile journals) =>
                    {
                        // stop searching older journal files if we see FSDJump
                        return journals.search((FSDJump _) =>
                        {
                            return true;
                        });
                    }
                );
            }

            // from ApproachBody
            if (this.nearBody == null)
            {
                // look for a recent ApproachBody event?
                journals!.searchDeep(
                    (ApproachBody approachBody) =>
                    {
                        if (approachBody.Body == bodyName && status!.BodyName == approachBody.Body && status.PlanetRadius > 0)
                        {
                            this.nearBody = new LandableBody(
                                this,
                                approachBody.StarSystem,
                                approachBody.Body,
                                approachBody.BodyID,
                                approachBody.SystemAddress,
                                status.PlanetRadius);
                            return true;
                        }
                        return false;
                    },
                    (JournalFile journals) =>
                    {
                        // stop searching older journal files if we see FSDJump
                        return journals.search((FSDJump _) =>
                        {
                            return true;
                        });
                    }
                    );
            }

            // look for recent Location event?
            if (this.nearBody == null)
            {
                journals!.search((Location locationEntry) =>
                {
                    if (locationEntry.Body == bodyName && status!.BodyName == locationEntry.Body && status.PlanetRadius > 0)
                    {
                        this.nearBody = new LandableBody(
                            this,
                            locationEntry.StarSystem,
                            locationEntry.Body,
                            locationEntry.BodyID,
                            locationEntry.SystemAddress,
                            status.PlanetRadius);
                        return true;
                    }
                    return false;
                });
            }

            // look for recent SupercruiseExit event?
            if (this.nearBody == null)
            {
                journals!.search((SupercruiseExit exitEvent) =>
                {
                    if (exitEvent.Body == bodyName && status!.BodyName == exitEvent.Body && status.PlanetRadius > 0)
                    {
                        this.nearBody = new LandableBody(
                            this,
                            exitEvent.Starsystem,
                            exitEvent.Body,
                            exitEvent.BodyID,
                            exitEvent.SystemAddress,
                            status.PlanetRadius);
                        return true;
                    }
                    return false;
                });
            }

            if (this.nearBody?.data.countOrganisms > 0)
            {
                log($"Genuses ({this.nearBody.data.countOrganisms}): " + string.Join(",", this.nearBody.data.organisms.Values.Select(_ => _.genusLocalized)));
            }

            //if (this.nearBody?.guardianSiteCount > 0)
            //{
            //    // do we have an ApproachSettlement for this site?
            //    this.journals!.searchDeep<ApproachSettlement>(
            //    (ApproachSettlement _) =>
            //    {
            //        if (_.BodyName == nearBody?.bodyName && _.Name.StartsWith("$Ancient:") && _.Latitude != 0)
            //        {
            //            this.nearBody.onJournalEntry(_);
            //            return true;
            //        }
            //        return false;
            //    },
            //    // search only as far as the FSDJump arrival
            //    (JournalFile journals) => journals.search((FSDJump _) => true));
            //}

            if (this.nearBody != null)
            {
                Game.log($"EVENT:nearingBody, at {this.nearBody?.bodyName}");

                if (this.nearingBody != null && this.nearBody != null)
                    this.nearingBody(this.nearBody);
            }
        }

        #region journal tracking for game state and modes

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            Game.log($"Game.event => {entry.@event}");
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(LoadGame entry)
        {
            if (this.Commander == null)
                this.initializeFromJournal();
        }

        private void onJournalEntry(Location entry)
        {
            // Happens when has loaded after being at the main menu
            // Or player resurrects at a station

            //this.starSystem = entry.StarSystem;
            //this.systemLocation = Util.getLocationString(entry.StarSystem, entry.Body);

            // rely on previously tracked locations?
            Game.log($"Create nearBody now? For: {entry.Body}, has nearBody: {this.nearBody != null}, cmdr.lastBody: {cmdr.currentBody}");
            if (this.nearBody == null && status.hasLatLong && cmdr.currentBody != null)
            {
                this.createNearBody(cmdr.currentBody);
            }

            this.setLocations(entry);
        }

        private void onJournalEntry(Music entry)
        {
            this.musicTrack = entry.MusicTrack;

            var newMainMenu = entry.MusicTrack == "MainMenu";
            if (this.atMainMenu != newMainMenu)
            {
                // if atMainMenu has changed - force a status change event
                this.atMainMenu = newMainMenu;
                this.Status_StatusChanged(false);
            }

            if (entry.MusicTrack == "GuardianSites" && this.nearBody != null)
            {
                // Are we at a Guardian site without realizing?
                if (this.nearBody.siteData == null)
                    nearBody.findGuardianSites();

                if (this.showGuardianPlotters)
                    this.fireUpdate(true);
            }
        }

        private void onJournalEntry(Shutdown entry)
        {
            this.atMainMenu = false;
            this.isShutdown = true;
            this.checkModeChange();
            this.Status_StatusChanged(false);
        }

        private void onJournalEntry(StartJump entry)
        {
            // FSD charged - either for Jump or SuperCruise
            if (entry.JumpType == "Hyperspace")
            {
                this.fsdJumping = true;
                this.checkModeChange();
            }
        }

        private void onJournalEntry(FSDJump entry)
        {
            // FSD Jump completed
            this.fsdJumping = false;
            this.statusBodyName = null;

            this.setLocations(entry);
            this.checkModeChange();
        }

        private void onJournalEntry(SupercruiseEntry entry)
        {
            Program.closePlotter<PlotGuardians>();
        }

        private void onJournalEntry(SupercruiseExit entry)
        {
            this.setLocations(entry);

            // check there are any Guardian sites nearby
            if (this.status.hasLatLong && this.nearBody != null)
                this.nearBody.findGuardianSites();
        }

        #endregion

        #region location tracking

        public void setLocations(FSDJump entry)
        {
            Game.log($"setLocations: from FSDJump: {entry.StarSystem} / {entry.Body} ({entry.BodyType})");

            cmdr.currentSystem = entry.StarSystem;
            cmdr.currentSystemAddress = entry.SystemAddress;
            cmdr.starPos = entry.StarPos;

            if (entry.BodyType == "Planet")
            {
                // would this ever happen?
                Game.log($"setLocations: FSDJump is a planet?!");

                cmdr.currentBody = entry.Body;
                cmdr.currentBodyId = entry.BodyID;

                // steal radius from status?
                if (Game.activeGame!.status.BodyName == entry.Body)
                    cmdr.currentBodyRadius = Game.activeGame.status.PlanetRadius;
                else
                {
                    Game.log($"Cannot find PlanetRadius from status file! Searching for last Scan event.");
                    cmdr.currentBodyRadius = this.findLastRadius(entry.Body);
                }
            }
            else
            {
                cmdr.currentBody = null;
                cmdr.currentBodyId = -1;
                cmdr.currentBodyRadius = -1;
            }

            cmdr.lastSystemLocation = Util.getLocationString(entry.StarSystem, entry.Body);
            cmdr.Save();
        }

        public void setLocations(ApproachBody entry)
        {
            Game.log($"setLocations: from ApproachBody: {entry.StarSystem} / {entry.Body}");

            cmdr.currentSystem = entry.StarSystem;
            cmdr.currentSystemAddress = entry.SystemAddress;

            cmdr.currentBody = entry.Body;
            cmdr.currentBodyId = entry.BodyID;

            // steal radius from status?
            if (Game.activeGame!.status.BodyName == entry.Body)
                cmdr.currentBodyRadius = Game.activeGame.status.PlanetRadius;
            else
            {
                Game.log($"Cannot find PlanetRadius from status file! Searching for last Scan event.");
                cmdr.currentBodyRadius = this.findLastRadius(entry.Body);
            }

            cmdr.lastSystemLocation = Util.getLocationString(entry.StarSystem, entry.Body);
            cmdr.Save();
        }

        public void setLocations(SupercruiseExit entry)
        {
            Game.log($"setLocations: from SupercruiseExit: {entry.Starsystem} / {entry.Body} ({entry.BodyType})");

            cmdr.currentSystem = entry.Starsystem;
            cmdr.currentSystemAddress = entry.SystemAddress;

            if (entry.BodyType == "Planet")
            {
                cmdr.currentBody = entry.Body;
                cmdr.currentBodyId = entry.BodyID;

                // steal radius from status?
                if (Game.activeGame!.status.BodyName == entry.Body)
                    cmdr.currentBodyRadius = Game.activeGame.status.PlanetRadius;
                else
                {
                    Game.log($"Cannot find PlanetRadius from status file! Searching for last Scan event.");
                    cmdr.currentBodyRadius = this.findLastRadius(entry.Body);
                }
            }
            else
            {
                cmdr.currentBody = null;
                cmdr.currentBodyId = -1;
                cmdr.currentBodyRadius = -1;
            }

            cmdr.lastSystemLocation = Util.getLocationString(entry.Starsystem, entry.Body);
            cmdr.Save();
        }

        public void setLocations(Location entry)
        {
            Game.log($"setLocations: from Location: {entry.StarSystem} / {entry.Body} ({entry.BodyType})");

            cmdr.currentSystem = entry.StarSystem;
            cmdr.currentSystemAddress = entry.SystemAddress;
            cmdr.starPos = entry.StarPos;

            if (entry.BodyType == "Planet")
            {
                cmdr.currentBody = entry.Body;
                cmdr.currentBodyId = entry.BodyID;

                // steal radius from status?
                if (Game.activeGame!.status.BodyName == entry.Body)
                    cmdr.currentBodyRadius = Game.activeGame.status.PlanetRadius;
                else
                {
                    Game.log($"Cannot find PlanetRadius from status file! Searching for last Scan event.");
                    cmdr.currentBodyRadius = this.findLastRadius(entry.Body);
                }
            }
            else
            {
                cmdr.currentBody = null;
                cmdr.currentBodyId = -1;
                cmdr.currentBodyRadius = -1;
            }

            cmdr.lastSystemLocation = Util.getLocationString(entry.StarSystem, entry.Body);
            cmdr.Save();
        }

        private double findLastRadius(string bodyName)
        {
            double planetRadius = -1;

            journals!.searchDeep(
                (Scan scan) =>
                {
                    if (scan.Bodyname == bodyName)
                    {
                        planetRadius = scan.Radius;
                        return true;
                    }
                    return false;
                },
                // search only as far as the FSDJump arrival
                (JournalFile journals) => journals.search((FSDJump _) => true)
            );

            return planetRadius;
        }

        #endregion

        #region journal tracking for ground ops

        private LatLong2? _touchdownLocation;

        public LatLong2 touchdownLocation
        {
            get
            {
                if (this._touchdownLocation == null)
                {
                    Game.log($"Searching journals for last touchdown location...");
                    var lastTouchdown = journals!.FindEntryByType<Touchdown>(-1, true);
                    var lastLiftoff = journals.FindEntryByType<Liftoff>(-1, true);
                    if (lastTouchdown != null)
                    {
                        if (lastLiftoff == null || lastTouchdown.timestamp > lastLiftoff.timestamp)
                        {
                            _touchdownLocation = new LatLong2(lastTouchdown);
                        }
                        //else
                        //{
                        //    _touchdownLocation = new LatLong2(lastTouchdown);
                        //}
                    }
                }
                return _touchdownLocation!;
            }
            set
            {
                this._touchdownLocation = value;
                if (this.nearBody != null)
                    this.nearBody.data.lastTouchdown = value;

            }
        }

        private void onJournalEntry(Touchdown entry)
        {
            this.touchdownLocation = entry;

            var ago = Util.timeSpanToString(DateTime.UtcNow - entry.timestamp);
            log($"Touchdown {ago}, at: {touchdownLocation}");
        }

        private void onJournalEntry(Liftoff entry)
        {
            this._touchdownLocation = LatLong2.Empty;
            log($"Liftoff!");
        }

        private void onJournalEntry(ApproachBody entry)
        {
            this.setLocations(entry);

            if (this.nearBody == null)
                this.createNearBody(entry.Body);
        }

        private void onJournalEntry(ApproachSettlement entry)
        {
            if (this.nearBody == null)
                this.createNearBody(entry.BodyName);

            if (this.nearBody != null)
                this.nearBody.onJournalEntry(entry);
        }

        private void onJournalEntry(SAASignalsFound entry)
        {
            if (this.nearBody == null)
                this.createNearBody(entry.BodyName);
            else
                this.nearBody.readSAASignalsFound(entry);

            // force a mode change to update ux
            fireUpdate(this._mode, true);
        }

        private void onJournalEntry(SellOrganicData entry)
        {
            // match and remove sold items from running totals
            Game.log($"SellOrganicData: removing {entry.BioData.Count} scans, worth: {Util.credits(entry.BioData.Sum(_ => _.Value))} + bonus: {Util.credits(entry.BioData.Sum(_ => _.Bonus))}");
            foreach (var data in entry.BioData)
            {
                var match = cmdr.scannedOrganics.Find(_ => _.species == data.Species && _.reward == data.Value);
                if (match == null)
                {
                    Game.log($"No match found when selling: {data.Species_Localised} for {data.Value} Cr");
                    continue;
                }

                cmdr.organicRewards -= data.Value;
                cmdr.scannedOrganics.Remove(match);
            }

            cmdr.Save();
            // force a mode change to update ux
            fireUpdate(this._mode, true);
        }

        private void setSuitType(SuitLoadout entry)
        {

            if (entry.SuitName.StartsWith("flightsuit"))
                this.currentSuitType = SuitType.flightSuite;
            else if (entry.SuitName.StartsWith("explorationsuit"))
                this.currentSuitType = SuitType.artiemis;
            else if (entry.SuitName.StartsWith("utilitysuit"))
                this.currentSuitType = SuitType.maverick;
            else if (entry.SuitName.StartsWith("tacticalsuit"))
                this.currentSuitType = SuitType.dominator;
            else
                Game.log($"Unexpected suit type: {entry.SuitName}");

            Game.log($"setSuitType: '{entry.SuitName}' => {this.currentSuitType}");
        }

        private void onJournalEntry(SuitLoadout entry)
        {
            this.setSuitType(entry);
        }

        private void onJournalEntry(SwitchSuitLoadout entry)
        {
            this.setSuitType(entry);
        }

        #endregion
    }
}
