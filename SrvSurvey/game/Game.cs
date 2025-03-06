using Newtonsoft.Json;
using SrvSurvey.canonn;
using SrvSurvey.net;
using SrvSurvey.net.EDSM;
using SrvSurvey.plotters;
using SrvSurvey.units;
using System.Diagnostics;

namespace SrvSurvey.game
{
    /// <summary>
    /// Represents live state of the game Elite Dangerous
    /// </summary>
    class Game : IDisposable
    {
        static Game()
        {
#if DEBUG
            // This stops logging code from starting up when custom controls are created in Visual Studio designer
            if (Process.GetCurrentProcess().ProcessName != "SrvSurvey") return;
#endif
            Game.logs = new List<string>();
            Game.logPath = prepLogFile();
            Game.log($"SrvSurvey version: {Program.releaseVersion}, isAppStoreBuild: {Program.isAppStoreBuild}");
            Game.log($"New log file: {Game.logPath}");
            Game.log($"dataFolder: {Program.dataFolder}");
            if (Program.isLinux) Game.log("Running on Linux");

            Game.removeExcessLogFiles();

            settings = Settings.Load();
            codexRef = new CodexRef();
            canonn = new Canonn();
            spansh = new Spansh();
            edsm = new EDSM();
            git = new Git();
            colony = new Colony();
        }

        #region logging

        private static string prepLogFile()
        {
            // prepare filepath for log file
            Directory.CreateDirectory(Game.logFolder);
            var datepart = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filepath = Path.Combine(Game.logFolder, $"srvs-{datepart}.txt")!;
            File.WriteAllLines(filepath, Game.logs);

            return filepath;
        }

        public static void log(object? msg)
        {
            var txt = DateTime.Now.ToString("HH:mm:ss") + ": " + msg?.ToString();

            Debug.WriteLine(txt);

            Game.logs.Add(txt);

            ViewLogs.append(txt);

            try
            {
                File.AppendAllText(Game.logPath, txt + "\r\n");
            }
            catch
            {
                // try one more shortly afterwards
                Application.DoEvents();
                try
                {
                    File.AppendAllText(Game.logPath, txt + "\r\n");
                }
                catch
                {
                    // and give up if the 2nd attempt fails too
                }
            }
        }

        private static void removeExcessLogFiles()
        {
            var logFiles = new DirectoryInfo(Game.logFolder)
                .EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly)
                .OrderByDescending(_ => _.LastWriteTimeUtc)
                .ToList();

            for (var idx = 5; idx < logFiles.Count; idx++)
            {
                var filename = logFiles[idx].FullName;
                Game.log($"Removing old log file: {filename}");
                File.Delete(filename);
            }
        }

        public static readonly List<string> logs;
        private static readonly string logPath;
        public static string logFolder = Path.Combine(Program.dataFolder, "logs", "");

        #endregion

        public static Game? activeGame { get; private set; }
        public static Settings settings { get; private set; }
        public static CodexRef codexRef { get; private set; }
        public static Canonn canonn { get; private set; }
        public static Spansh spansh { get; private set; }
        public static EDSM edsm { get; private set; }
        public static Git git { get; private set; }
        public static Colony colony { get; private set; }

        public bool initialized { get; private set; }

        /// <summary>
        /// The Commander actively playing the game
        /// </summary>
        public string? Commander { get; private set; }
        public string? fid { get; private set; }

        /// <summary>
        /// If we're supposed to be loading journals for a specific cmdr only
        /// </summary>
        private string? targetCmdr;
        public string musicTrack { get; private set; }
        public SuitType currentSuitType { get; private set; }

        public JournalWatcher? journals;
        public Status status { get; private set; }
        public NavRouteFile navRoute { get; private set; }
        public CargoFile cargoFile { get; private set; }
        public bool guardianMatsFull;
        public bool processDockedOnNextStatusChange = false;
        public bool dockingInProgress = false;

        public SystemData? systemData;
        public SystemBody? systemBody;
        public GuardianSiteData? systemSite;
        public CanonnStation? systemStation;
        public FSDTarget? fsdTarget;
        public ShipData currentShip = new();
        public Docked? lastDocked;
        public SettlementMatCollectionData? matStatsTracker;
        private string? lastDestination;
        public bool destinationNextRouteHop = false;
        /// <summary> Current counts of materials for the cmdr </summary>
        private Materials materials;

        /// <summary>
        /// An arbitrary point on a planet surface. Doesn't really matter where, so long as it isn't too far away.
        /// </summary>
        public LatLong2 dropPoint;

        /// <summary>
        /// Distinct settings for the current commander
        /// </summary>
        public CommanderSettings cmdr;
        public CommanderCodex cmdrCodex;
        public CommanderJourney? journey;

        public SystemPoi? canonnPoi = null;
        public DockToDockTimer? dockTimer;

        /// <summary>
        /// Becomes True once the game has completed initializing
        /// </summary>
        public static bool ready { get; private set; } = false;

        public Game(string? cmdr)
        {
            log($"Game .ctor, targetCmdr: {cmdr}");
            this.targetCmdr = cmdr;

            // track this instance as the active one
            Game.activeGame = this;

            if (!Elite.isGameRunning && !Program.useLastIfShutdown) return;

            // track status file changes and force an immediate read
            this.status = new Status(true);
            this.navRoute = NavRouteFile.load(true);
            this.cargoFile = CargoFile.load(true);

            // initialize from a journal file
            var filepath = JournalFile.getSiblingCommanderJournal(cmdr, true, DateTime.MaxValue);
            if (filepath == null)
            {
                Game.log($"No journal files found for: {cmdr}");
                this.isShutdown = true;
                return;
            }

            this.journals = new JournalWatcher(filepath);
            this.initializeFromJournal();
            if (this.isShutdown) return;

            // initialize boxel search if there's one active
            if (this.cmdr?.boxelSearch?.active == true && this.cmdr.boxelSearch.boxel != null)
            {
                Task.Delay(5_000)
                    .ContinueWith(t => this.cmdr.boxelSearch.activate(this.cmdr.boxelSearch.boxel));
            }

            log($"Cmdr loaded: {this.Commander != null}");

            // now listen for changes
            this.journals.onJournalEntry += Journals_onJournalEntry;
            this.status.StatusChanged += Status_StatusChanged;

            Game.ready = true;
            this.predictSystemSpecies();

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

                if (this.navRoute != null)
                {
                    this.navRoute.Dispose();
                    this.navRoute = null!;
                }
            }
        }

        #region modes

        public void fireUpdate(bool force = false)
        {
            if (Program.control.InvokeRequired)
                Program.control.Invoke(() => this.fireUpdate(this.mode, force));
            else
                this.fireUpdate(this.mode, force);
        }

        private void fireUpdate(GameMode newMode, bool force)
        {
            if (Game.update != null) Game.update(newMode, force);
        }

        public static event GameModeChanged? update;

        public bool atMainMenu = false;
        public bool atCarrierMgmt = false;
        public bool fsdJumping = false;
        public bool isShutdown = false;
        /// <summary>
        /// The name of the system for which we're requesting data for
        /// </summary>
        private string? fetchedSystemData = null;

        /// <summary>
        /// Tracks if we status had Lat/Long last time we knew
        /// </summary>
        private bool statusHasLatLong = false;
        /// <summary>
        /// Tracks BodyName from status - attempting to detect when multiple running games clobber the same file in error
        /// </summary>
        private string? statusBodyName;

        private bool landingGearDown;

        private void checkModeChange()
        {
            var doUpdate = false;

            // check various things we actively need to know have changed
            var newMode = this.mode;
            if (this._mode != newMode)
            {
                log($"Mode change {this._mode} => {newMode}");
                this._mode = newMode;
                doUpdate = true;
            }

            if (newMode != GameMode.StationServices)
                this.marketEventSeen = false;

            // show reminder message if we bought something on an FC but did not revisit the commodity market
            if (Game.settings.allowNotifications.fcMarketPurchaseBugReminder && marketBuyOnFC && newMode != GameMode.StationServices)
                PlotFloatie.showMessage("Don't forget to revisit the market when making buying at Fleet Carriers");

            if (status != null && landingGearDown != status.landingGearDown)
            {
                // TODO: one day ... move this type of logic into status
                landingGearDown = status.landingGearDown;
                doUpdate = true;
            }

            // fire event!
            if (doUpdate)
                fireUpdate(newMode, false);
        }

        private void Status_StatusChanged(bool blink)
        {
            //log("status changed");
            if (this.statusBodyName != null && status!.BodyName != null && status!.BodyName != this.statusBodyName)
            {
                // exit early if the body suddenly switches to something different without being NULL first (this is an artifact of running the game twice at the same time)
                Game.log($"Multiple running games? this.statusBodyName: ${this.statusBodyName} vs status.BodyName: {status.BodyName}");
                // TODO: some planet/moons are close enough together to trigger this - maybe compare the first word from each?
                return;
            }
            this.statusBodyName = status!.BodyName;

            // track if status HasLatLong, toggling behaviours when it changes
            var newHasLatLong = status.hasLatLong;
            if (this.statusHasLatLong != newHasLatLong)
            {
                Game.log($"Changed hasLatLong: {newHasLatLong}, has nearBody: {this.systemBody != null}, nearBody matches: {status.BodyName == this.systemBody?.name}");
                this.statusHasLatLong = newHasLatLong;

                if (!newHasLatLong)
                {
                    // we are departing
                    if (this.systemBody != null && !SystemData.isWithinLastDssDuration())
                    {
                        Game.log($"EVENT:departingBody, from {this.systemBody.name}");

                        // change system locations to be just system (not the body)
                        cmdr.currentBody = null;
                        cmdr.currentBodyId = -1;
                        cmdr.currentBodyRadius = -1;
                        cmdr.Save();

                        this.systemBody = null;
                        this.fireUpdate(false);
                    }
                }
                else
                {
                    if (this.systemBody == null && status.BodyName != null)
                    {
                        this.setCurrentBody(status.BodyName);

                        // we are approaching - create and fire event
                    }
                }
            }

            if (!status.hasLatLong && this.systemBody != null && !SystemData.isWithinLastDssDuration())
            {
                log($"status change clearing systemBody from: '{this.systemBody.name}' ({this.systemBody.id})");
                this.systemBody = null;
                this.fireUpdate(false);
            }

            //Game.log($"Status\r\n flags: {status.Flags}\r\nflags2: {status.Flags2}\r\n\t?\t{status.Flags2 & (StatusFlags2.OnFootInHangar | StatusFlags2.OnFootInStation | StatusFlags2.OnFootSocialSpace)}");

            // set or clear 'systemSite' // TODO: Does this *really* need to be here?
            this.setCurrentSite();

            //if (FormGenus.activeForm != null && FormGenus.activeForm.targetBody != this.targetBody)
            //    FormGenus.activeForm.populateGenus();
            // TODO: we could avoid flicker by updating the colors on labels, rather than destroying and recreating them.

            if (this.processDockedOnNextStatusChange)
            {
                this.processDockedOnNextStatusChange = false;
                if (status.Docked)
                    onDockedWhenSafe(CalcMethod.ManualDock, false);
            }

            var destinationHash = status.Destination?.ToString();
            if (this.lastDestination != destinationHash)
            {
                this.statusDestinationChanged();
                this.lastDestination = destinationHash;
            }

            this.checkModeChange();
        }

        private void statusDestinationChanged()
        {
            FormPredictions.invalidate();

            // is destination the next hop on the route?
            destinationNextRouteHop = false;
            if (this.status.Destination?.System > 0)
            {
                var currentHop = this.navRoute.Route.FirstOrDefault(r => r.SystemAddress == this.systemData?.address);
                if (currentHop != null)
                {
                    var destinationHop = this.navRoute.Route.FirstOrDefault(r => r.SystemAddress == this.status.Destination.System);
                    if (destinationHop != null)
                    {
                        var idxCurrent = this.navRoute.Route.IndexOf(currentHop);
                        var idxDestination = this.navRoute.Route.IndexOf(destinationHop);

                        if (idxDestination == idxCurrent + 1)
                        {
                            // destination is next hop in route!
                            destinationNextRouteHop = true;
                        }
                    }
                }
            }
        }

        public GameMode mode
        {
            get
            {
                if (!Elite.isGameRunning || this.isShutdown || this.status == null)
                    return GameMode.Offline;

                if (this.atMainMenu || this.Commander == null)
                    return GameMode.MainMenu;

                if (this.atCarrierMgmt)
                    return GameMode.CarrierMgmt;

                // use GuiFocus if it is interesting
                if (this.status.GuiFocus != GuiFocus.NoFocus)
                    return (GameMode)(int)this.status.GuiFocus;

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
                if (this.status.OnFootInStation)
                    return GameMode.OnFootInStation;
                if ((this.status.Flags2 & StatusFlags2.OnFootOnPlanet) > 0)
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

                // otherwise we must be ...
                if (activeVehicle == ActiveVehicle.MainShip)
                    return GameMode.Flying;

                // if we're still here and these are zero ...
                if (status.Flags == 0 && status.Flags2 == 0)
                    return GameMode.Offline;

                Game.log($"Unknown game mode? status.Flags: {status.Flags}, status.Flags2: {status.Flags2}");
                Debugger.Break();
                return GameMode.Unknown;
            }
        }
        private GameMode _mode;

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
                else if (this.status.InMainShip)
                    return ActiveVehicle.MainShip;
                else if (this.status.InFighter)
                    return ActiveVehicle.Fighter;
                else if (this.status.InSrv)
                    return ActiveVehicle.SRV;
                else if (this.status.OnFoot)
                    return ActiveVehicle.Foot;
                else if (this.status.InTaxi)
                    return ActiveVehicle.Taxi;
                else
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
            get => this.status != null
                && (this.status.Flags2 & StatusFlags2.OnFoot) > 0
                && (
                    (this.currentSuitType == SuitType.dominator && Game.settings.hidePlottersFromCombatSuits)
                    || (this.currentSuitType == SuitType.maverick && Game.settings.hidePlottersFromMaverickSuits)
                );
        }

        public string targetBodyShortName { get => this.targetBody?.shortName ?? ""; }

        public SystemBody? targetBody
        {
            get
            {
                if (this.systemData == null || this.status == null) return null;

                if (status.Destination != null)
                {
                    // ignore if target is outside our system
                    if (status.Destination.System != this.systemData.address)
                        return null;

                    if (status.Destination.Body >= 0)
                    {
                        var body = systemData.bodies.Find(_ => _.id == status.Destination.Body);
                        // but ignore if the name doesn't match (eg: when targetting a station around a planet)
                        if (body?.name == status.Destination.Name)
                            return body;
                    }
                }

                // otherwise, use current body if we are close to one
                if (status.hasLatLong && this.systemBody != null)
                    return this.systemBody;

                return null;
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
                log($"Game.initializeFromJournal: USING {this.Commander} (FID:{this.fid}), journals.Count:{journals?.Count}");
            }

            // exit early if we are shutdown
            var lastShutdown = journals!.FindEntryByType<Shutdown>(-1, true);
            if (lastShutdown != null && !Program.useLastIfShutdown)
            {
                log($"Game.initializeFromJournal: EXIT isShutdown! ({Game.activeGame == this})");
                this.isShutdown = true;
                return;
            }

            if (loadEntry != null)
            {
                // How much does it matter that v4-live/Horizons acts just like Odyssey?
                this.cmdr = CommanderSettings.Load(loadEntry.FID, journals.isOdyssey, loadEntry.Commander);
                this.cmdrCodex = cmdr.loadCodex();
                this.journey = cmdr.loadActiveJourney();
                this.journey?.doCatchup(this.journals!);

                if (Game.settings.buildProjects_TEST)
                    this.cmdr.loadColonyData().ContinueWith(t =>
                    {
                        if (t.Exception != null || !t.IsCompletedSuccessfully)
                            Util.isFirewallProblem(t.Exception);
                    });
            }

            // if we have MainMenu music - we know we're not actively playing
            var lastMusic = journals.FindEntryByType<Music>(-1, true);
            if (lastMusic != null)
                onJournalEntry(lastMusic);

            // try to find a location from recent journal items
            if (this.cmdr != null)
            {
                this.initSystemData(cmdr.fid, cmdr.commander);

                if (status.hasLatLong && (cmdr.currentMarketId > 0 || status.Docked))
                {
                    this.initHumanSite();
                    if (this.systemStation != null)
                        log($"HumanSite matched: {systemStation.name} ({systemStation.marketId}) - {systemStation.economy} #{systemStation.subType}");
                }

                LeaveBody? leaveBodyEvent = null;
                journals.walk(-1, true, (entry) =>
                {
                    var locationEvent = entry as Location;
                    if (locationEvent != null)
                    {
                        this.setLocations(locationEvent, cmdr.fid, cmdr.commander);
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

                    if (entry is LeaveBody)
                        leaveBodyEvent = entry as LeaveBody;

                    var approachBodyEvent = entry as ApproachBody;
                    if (approachBodyEvent != null && leaveBodyEvent == null)
                    {
                        this.setLocations(approachBodyEvent);
                        return true;
                    }
                    return false;
                });

                // if status file says we're around a different body - use that
                if (this.status.BodyName != null && this.status.PlanetRadius > 0 && this.systemBody?.name != this.status.BodyName)
                {
                    this.setCurrentBody(this.status.BodyName);
                    this.cmdr.currentBody = this.systemBody?.name;
                    this.cmdr.currentBodyRadius = this.systemBody?.radius ?? -1;
                    this.cmdr.currentBodyId = this.systemBody?.id ?? -1;
                }

                if (this.status.PlanetRadius > 0 && this.status.PlanetRadius != cmdr.currentBodyRadius)
                    log($"Oops - bad systemBody ?!");

                log($"Game.initializeFromJournal: system: '{cmdr.currentSystem}' (id:{cmdr.currentSystemAddress}), body: '{this.systemBody?.name}' (id:{this.systemBody?.id}, r: {Util.metersToString(this.systemBody?.radius ?? -1)})");
            }

            // if we have landed, we need to find the last Touchdown location
            if (this.isLanded && this._touchdownLocation == null) // || (status.Flags & StatusFlags.HasLatLong) > 0)
            {
                if (this.systemData == null || this.systemBody == null)
                {
                    Game.log("Why is nearBody null?");
                    return;
                }

                if (this.mode == GameMode.Landed)
                {
                    var location = journals.FindEntryByType<Location>(-1, true);
                    if (location != null && location.SystemAddress == this.systemData.address && location.BodyID == this.systemBody.id)
                    {
                        Game.log($"LastTouchdown from Location: {location}");
                        this.touchdownLocation = location;
                    }
                }

                if (this.touchdownLocation == null)
                {
                    this.getLastTouchdownDeep();
                }
            }

            // if we have landed, we need to find the last Touchdown location
            var lastMaterials = journals.FindEntryByType<Materials>(-1, true);
            if (lastMaterials != null)
                onJournalEntry(lastMaterials);

            // what ship are we in?
            var lastLoadout = journals.FindEntryByType<Loadout>(-1, true);
            if (lastLoadout != null)
            {
                this.currentShip = new(lastLoadout);
            }

            // clear old touchdown location but we're no longer on a planet
            if (cmdr?.lastTouchdownLocation != null && !status.hasLatLong) cmdr.clearTouchdown();

            if (this.status.hasLatLong)
                this.dropPoint = Status.here.clone();

            // remove any stale massacre missions?
            journals.walk(0, false, (entry) =>
            {
                if (entry is MissionAbandoned || entry is MissionCompleted || entry is MissionFailed)
                    this.Journals_onJournalEntry(entry, 0);

                return false;
            });
            var lastMissions = journals.FindEntryByType<Missions>(-1, true);
            if (lastMissions != null)
                this.onJournalEntry(lastMissions);

            log($"Game.initializeFromJournal: END Commander:{this.Commander}, starSystem:{cmdr?.currentSystem}, systemLocation:{cmdr?.lastSystemLocation}, systemBody:{this.systemBody}, journals.Count:{journals.Count}");
            this.initialized = Game.activeGame == this && this.Commander != null;
            this.checkModeChange();

            // do this last and a bit delayed, once initialization finished
            if (Game.settings.useExternalData && this.systemData != null)
            {
                Program.control.BeginInvoke(new Action(() =>
                {
                    this.fetchSystemData(this.systemData.name, this.systemData.address);
                }));
            }
        }

        public void initHumanSite()
        {
            if (this.journals == null || this.systemData == null || !status.hasLatLong) return;
            Game.log($"initHumanSite: currentMarketId: {cmdr.currentMarketId}");

            // do we have an existing match, that has a heading already?
            if (cmdr.currentMarketId > 0)
            {
                this.systemStation = systemData?.stations?.Find(s => s.marketId == cmdr.currentMarketId);
                if (this.systemStation != null && this.systemStation.heading != -1)
                {
                    // get details about the station faction
                    this.journals.walkDeep(true, (entry) =>
                    {
                        var lastApproachSettlement = entry as ApproachSettlement;
                        if (lastApproachSettlement != null)
                        {
                            this.systemStation.applyNonSavedSettlementValues(lastApproachSettlement, this.journals);
                            return true;
                        }
                        return false;
                    });
                    return;
                }

                // TODO: Keep this?
                //if (Game.settings.collectMatsCollectionStatsTest) this.initMats(this.systemStation);

            }

            // Otherwise, walk journals backwards collecting relevant entries for us to replay forwards
            ApproachSettlement? lastApproachSettlement = null;
            DockingRequested? lastDockingRequested = null;
            DockingGranted? lastDockingGranted = null;
            Docked? lastDocked = null;
            Touchdown? lastTouchdown = null;
            var liftedOff = false;

            this.journals.walkDeep(true, (entry) =>
            {
                var loadout = entry as Loadout;
                if (loadout != null && this.currentShip == null)
                    this.currentShip = new(loadout);

                // keep the first Docked event we find, unless we touched down
                var docked = entry as Docked;
                if (docked != null && lastDocked == null && lastTouchdown == null)
                    lastDocked = docked;

                // keep the first hit, or the one that matches the Docked
                var dockingRequested = entry as DockingRequested;
                if (dockingRequested != null && lastDockingRequested == null)
                    if (lastDocked == null || lastDocked.MarketID == dockingRequested.MarketID)
                        lastDockingRequested = dockingRequested;

                // keep the first hit, or the one that matches the Docked
                var dockingGranted = entry as DockingGranted;
                if (dockingGranted != null && lastDockingGranted == null)
                    if (lastDocked == null || lastDocked.MarketID == dockingGranted.MarketID)
                        lastDockingGranted = dockingGranted;

                // (so we can ignore any prior TouchDown's)
                if (entry is Liftoff) liftedOff = true;

                // in case we landed near a settlement without docking (and we're not already docked and did not Liftoff after)
                var touchdown = entry as Touchdown;
                if (touchdown != null && lastTouchdown == null && lastDocked == null && !liftedOff)
                    lastTouchdown = touchdown;

                // we often have ApproachSettlement near the top of the file and nothing else - let's try going past that to get the other entries
                var approachSettlement = entry as ApproachSettlement;
                if (approachSettlement != null && lastApproachSettlement == null)
                    if (docked == null || docked.MarketID == approachSettlement.MarketID)
                        lastApproachSettlement = entry as ApproachSettlement;

                // stop if we have ApproachBody or StartJump, otherwise keep going
                return entry is ApproachBody || entry is StartJump;
            });

            // exit early if there was no ApproachSettlement or we're not near that settlement any more
            if (lastApproachSettlement == null || lastApproachSettlement.BodyName != status.BodyName) return;
            if (Util.getDistance(lastApproachSettlement, Status.here, status.PlanetRadius) > 7_500) return;

            // we'll need a shipType if not already found
            if (this.currentShip == null)
            {
                Game.log("Didn't find ship details yet?");
                journals.walk(-1, true, entry =>
                {
                    if (entry is Loadout)
                    {
                        this.currentShip = new((Loadout)entry);
                        return true;
                    }
                    if (entry is LoadGame)
                    {
                        this.currentShip = new((LoadGame)entry);
                        return true;
                    }
                    return false;
                });
            }

            onJournalEntry(lastApproachSettlement);
            if (lastDockingRequested != null) onJournalEntry(lastDockingRequested);
            if (lastDockingGranted != null) onJournalEntry(lastDockingGranted);

            // but we can only use the Docked event if we are still docked at that same pad
            if (lastDocked != null && status.Docked && systemStation?.heading == -1)
            {
                onJournalEntry(lastDocked);
                cmdr.setMarketId(lastDocked.MarketID);
            }

            // or if we touched down near a site
            if (lastDocked == null && lastTouchdown?.NearestDestination == lastApproachSettlement.Name)
            {
                // not sure if we really need this.
                //Debugger.Break();
            }

            //// old ...
            //// if we've been to this settlement before - load it and exit early
            //this.humanSite = HumanSiteData.Load(systemData.address, lastApproachSettlement.MarketID);

            //if (this.humanSite != null)
            //{
            //    cmdr.setMarketId(this.humanSite.marketId);
            //    // This is probably a bad idea
            //    if (Game.settings.collectMatsCollectionStatsTest) this.initMats(this.humanSite);
            //    return;
            //}

            //// replay entries forwards - starting with ApproachSettlement
            //this.humanSite = new HumanSiteData(lastApproachSettlement);
            //Game.log($"initHumanSite: create: '{lastApproachSettlement.Name}' ({lastApproachSettlement.MarketID})");

            //// docking requested/granted we can use regardless
            //if (lastDockingRequested != null) humanSite.dockingRequested(lastDockingRequested);
            //if (lastDockingGranted != null) humanSite.dockingGranted(lastDockingGranted);

            //// but we can only use the Docked event if we are still docked at that same pad
            //if (lastDocked != null && status.Docked)
            //{
            //    // we'll need a shipType first, which may be much earlier than we already walked
            //    if (this.shipType == null) this.shipType = journals.FindEntryByType<Loadout>(-1, true)?.Ship!;
            //    if (this.shipType == null) this.shipType = journals.FindEntryByType<LoadGame>(-1, true)?.Ship!;

            //    humanSite.docked(lastDocked, status.Heading);
            //    cmdr.setMarketId(lastDocked.MarketID);
            //}

            //// or if we touched down near a site
            //if (lastDocked == null && lastTouchdown?.NearestDestination == lastApproachSettlement.Name)
            //{
            //    // not sure if we really need this.
            //}
        }

        private void initSystemData(string fid, string cmdrName)
        {
            // try to find a location from recent journal items
            if (cmdr == null || this.journals == null) return;

            log($"Game.initSystemData: rewind to last FSDJump or CarrierJump");

            var playForwards = new List<IJournalEntry>();

            Location? lastLocation = null;
            this.journals.walkDeep(true, (entry) =>
            {
                // record last location event, in case ...
                var locationEvent = entry as Location;
                if (locationEvent != null && lastLocation == null)
                {
                    log($"Game.initSystemData: last location: '{locationEvent.StarSystem}' ({locationEvent.SystemAddress})");
                    lastLocation = locationEvent;
                    return false;
                }

                // .. we were on a carrier and it jumped when not playing, hence no FSD or Carrier jump event. We can init from the Location event, once we see that journal entries are coming from some other system
                var systemAddressEntry = entry as ISystemAddress;
                if (systemAddressEntry?.SystemAddress > 0 && lastLocation != null && systemAddressEntry.SystemAddress != lastLocation.SystemAddress)
                {
                    log($"Game.initSystemData: Carrier jump since last session? Some SystemAddress ({systemAddressEntry.SystemAddress}) does not match current location ({lastLocation.SystemAddress})");
                    this.systemData = SystemData.From(lastLocation);
                    return true;
                }

                // init from FSD or Carrier jump event
                var starterEvent = entry as ISystemDataStarter;
                if (starterEvent != null && starterEvent.@event != nameof(Location))
                {
                    log($"Game.initSystemData: found last {starterEvent.@event}, to '{starterEvent.StarSystem}' ({starterEvent.SystemAddress})");
                    this.systemData = SystemData.From(starterEvent, fid, cmdrName);
                    return true;
                }

                if (SystemData.journalEventTypes.Contains(entry.@event))
                    playForwards.Add(entry);

                var dockedEvent = entry as Docked;
                if (dockedEvent != null && this.lastDocked == null)
                    this.lastDocked = dockedEvent;

                return false;
            });

            if (this.systemData == null && lastLocation != null)
            {
                log($"Game.initSystemData: using {lastLocation.@event}, to '{lastLocation.StarSystem}' ({lastLocation.SystemAddress})");
                this.systemData = SystemData.From(lastLocation, fid, cmdrName);
            }

            if (this.systemData == null)
            {
                log($"Game.initSystemData: Why no systemData? Current journal has {this.journals.Count} items.");
                Debugger.Break(); // why/when does this really happen?
                return;
            }

            log($"Game.initSystemData: Processing {playForwards.Count} journal items forwards...");
            playForwards.Reverse();
            foreach (var entry in playForwards)
            {
                log($"Game.initSystemData: playForwards '{entry.@event}' ({entry.timestamp})");
                this.systemData.Journals_onJournalEntry(entry, false);
            }

            this.systemData.Save();
            log($"Game.initSystemData: complete '{this.systemData.name}' ({this.systemData.address}), bodyCount: {this.systemData.bodyCount}");
        }

        private void getLastTouchdownDeep()
        {
            Game.log($"getLastTouchdownDeep");

            if (this.touchdownLocation == null && journals != null)
            {
                journals.searchDeep(
                    (Touchdown lastTouchdown) =>
                    {
                        Game.log($"LastTouchdown from deep search: {lastTouchdown}");

                        if (lastTouchdown.Body == status!.BodyName)
                        {
                            this.touchdownLocation = lastTouchdown;
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

        #endregion

        #region journal tracking for game state and modes

        private void Journals_onJournalEntry(IJournalEntry entry, int index)
        {
            try
            {
                Game.log($"Game.event => {entry.@event}");
                // it's important that journey gets to process these first
                if (this.journey != null)
                    this.journey.processJournalEntry(entry, true);

                this.onJournalEntry((dynamic)entry); // TODO: <-- move after systemData processes things

                if (this.systemData != null)
                    this.systemData.Journals_onJournalEntry(entry, true);
            }
            catch (Exception ex)
            {
                Game.log($"Exception processing event '{entry.@event}':\r\n{ex}");
                FormErrorSubmit.Show(ex);
            }
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(Commander entry)
        {
            // the game loaded some cmdr - now we can check it's the right one
            if (this.targetCmdr != null && !entry.Name.Equals(this.targetCmdr, StringComparison.OrdinalIgnoreCase))
            {
                Game.log($"Wrong cmdr loaded: '{entry.Name}' vs targetCmdr: '{this.targetCmdr}' - shutting down!");
                this.isShutdown = true;
                this.fireUpdate(true);
            }
        }

        private void onJournalEntry(LoadGame entry)
        {
            this.currentShip = new(entry);

            if (this.Commander == null)
                this.initializeFromJournal(entry);
            else
                this.fireUpdate(true);
        }

        private void onJournalEntry(Location entry)
        {
            // Happens when game has loaded after being at the main menu
            // Or player resurrects at a station

            // rely on previously tracked locations?

            this.setLocations(entry);
        }

        private void onJournalEntry(Resurrect entry)
        {
            this.fsdJumping = false;
            SystemData.Close(this.systemData);
            this.canonnPoi = null;
            this.systemBody = null;
            this.systemData = null;
            this.systemStation = null;
            this.fetchedSystemData = null;
            this.Status_StatusChanged(false);
        }

        private void onJournalEntry(Loadout entry)
        {
            this.currentShip = new(entry);
        }

        private void onJournalEntry(Died entry)
        {
            Game.log($"You died. Clearing ${Util.credits(this.cmdr.organicRewards)} from {this.cmdr.scannedBioEntryIds.Count} organisms.");
            // revisit all active bio-scan entries per body and mark them as Died

            this.cmdr.scannedOrganics?.Clear(); // retire?
            this.cmdr.scanOne = null;
            this.cmdr.scanTwo = null;
            this.cmdr.lastOrganicScan = null;
            this.cmdr.currentMarketId = 0;
            this.cmdr.Save();

            // forget these things
            this.lastDocked = null;
            if (this.dockTimer != null)
            {
                this.dockTimer.writeToFile();
                this.dockTimer = null;
            }

            this.Status_StatusChanged(false);

            this.exitMats();
        }

        private void onJournalEntry(Music entry)
        {
            this.musicTrack = entry.MusicTrack;

            var newMainMenu = entry.MusicTrack == "MainMenu";
            if (this.atMainMenu != newMainMenu)
            {
                // if atMainMenu has changed - force a status change event
                this.atMainMenu = newMainMenu;
                this.fsdJumping = false;
                this.Status_StatusChanged(false);
                if (this.atMainMenu)
                    this.exitMats();
            }

            var newCarrierMgmt = entry.MusicTrack == "FleetCarrier_Managment";
            if (this.atCarrierMgmt != newCarrierMgmt)
            {
                // if atMainMenu has changed - force a status change event
                this.atCarrierMgmt = newCarrierMgmt;
                this.Status_StatusChanged(false);
            }

            if (entry.MusicTrack == "GuardianSites" && this.systemBody != null)
            {
                // Are we at a Guardian site without realizing?
                Program.control.BeginInvoke(new Action(this.setCurrentSite));
            }
        }

        private void onJournalEntry(Shutdown entry)
        {
            this.atMainMenu = false;
            this.isShutdown = true;
            this.fireUpdate(true);
        }

        private void onJournalEntry(StartJump entry)
        {
            // FSD charged - either for Jump or SuperCruise
            if (entry.JumpType == "Hyperspace")
            {
                cmdr.countJumps += 1;
                cmdr.setMarketId(0); // (saves)

                Program.defer(() => this.fsdJumping = true);
                SystemData.Close(this.systemData);
                this.canonnPoi = null;
                this.systemBody = null;
                this.systemData = null;
                this.systemStation = null;
                this.fetchedSystemData = null;
                this.Status_StatusChanged(false);

                // stop force showing these any time we change systems
                PlotFSSInfo.forceShow = false;
                PlotBodyInfo.forceShow = false;
                Program.closePlotter<PlotFSSInfo>();
                Program.closePlotter<PlotBodyInfo>();

                // clear the nextSystem displayed when jumping to some other system
                var form = Program.getPlotter<PlotSysStatus>();
                if (form != null)
                {
                    form.nextSystem = null;
                    form.Invalidate();
                }
            }
            else
            {
                // For SuperCruise ?
            }

            // for either FSD type ...
            dockTimer?.onJournalEntry(entry);

            // we are certainly no longer at any humanSite
            if (this.systemStation != null)
                this.systemStation = null;

            fireUpdate(true);
        }

        private void onJournalEntry(FSDTarget entry)
        {
            this.fsdTarget = entry;
        }

        private void onJournalEntry(NavRoute entry)
        {
            if (cmdr.boxelSearch?.active == true)
                Program.defer(() => cmdr.boxelSearch.updateFromRoute(navRoute.Route));

            Program.invalidate<PlotSphericalSearch>();
        }

        private void onJournalEntry(NavRouteClear entry)
        {
            this.fsdTarget = null;
            PlotJumpInfo.forceShow = false;

            Program.invalidate<PlotSphericalSearch>();
        }

        private void onJournalEntry(FSDJump entry)
        {
            // FSD Jump completed
            this.fsdJumping = false;
            this.statusBodyName = null;
            cmdr.distanceTravelled += entry.JumpDist;

            this.setLocations(entry);

            dockTimer?.onJournalEntry(entry);

            // update boxel search?
            if (cmdr.boxelSearch?.active == true)
                cmdr.boxelSearch.markComplete(entry.SystemAddress, entry.StarSystem, entry.StarPos);

            // update route progress?
            if (cmdr.route?.active == true)
                cmdr.route.setNextHop(StarRef.from(entry));
        }

        private void onJournalEntry(CarrierJump entry)
        {
            // Carrier Jump completed
            this.fsdJumping = false;
            this.statusBodyName = null;

            this.setLocations(entry);
        }

        private void onJournalEntry(SupercruiseExit entry)
        {
            this.setLocations(entry);
            this.dropPoint = Status.here.clone();

            dockTimer?.onJournalEntry(entry);
        }

        public void deferPredictSpecies(SystemBody? body)
        {
            if (body == null) return;

            Util.deferAfter(100, () =>
            {
                body.predictSpecies();

                FormShowCodex.update();
                FormPredictions.refresh();
                Program.invalidateActivePlotters();
            });
        }

        public void predictSystemSpecies()
        {
            Game.log("predictSystemSpecies");
            if (this.systemData == null) return;

            // re-predict everything in the current system
            foreach (var body in this.systemData.bodies.ToList())
            {
                if (body.bioSignalCount > 0)
                    body.predictSpecies();
            }

            FormShowCodex.update();
            FormPredictions.refresh();
            Program.invalidateActivePlotters();
        }

        private void onJournalEntry(FSSDiscoveryScan entry)
        {
        }

        private void onJournalEntry(FSSAllBodiesFound entry)
        {
            Program.defer(() =>
            {
                // update boxel search?
                if (cmdr.boxelSearch?.active == true)
                    cmdr.boxelSearch.markComplete(entry.SystemAddress, entry.SystemName, null);

                Program.invalidate<PlotFSSInfo>();
            });
        }

        private void onJournalEntry(Scan entry)
        {
        }

        private void onJournalEntry(SAAScanComplete entry)
        {
            this.setCurrentBody(entry.BodyID);
            this.fireUpdate();

            this.deferPredictSpecies(this.systemBody);
        }

        private void onJournalEntry(Missions entry)
        {
            if (entry.Active.Any(_ => _.Name == "Mission_TheDead" || _.Name == "Mission_TheDead_name"))
                this.cmdr.decodeTheRuinsMissionActive = TahMissionStatus.Active;

            if (entry.Active.Any(_ => _.Name == "Mission_TheDead_002" || _.Name == "Mission_TheDead_002_name"))
                this.cmdr.decodeTheLogsMissionActive = TahMissionStatus.Active;

            Game.log($"Missions: decodeTheRuinsMissionActive: {this.cmdr.decodeTheRuinsMissionActive}, decodeTheLogsMissionActive: {this.cmdr.decodeTheLogsMissionActive}");

            // TODO: prune very stale massacre missions
            if (this.cmdr.trackMassacres?.Count > 0)
            {
                var count = this.cmdr.trackMassacres.RemoveAll(m => !(entry.Active.Any(a => a.MissionID == m.missionId) || entry.Complete.Any(c => c.MissionID == m.missionId)));
                Game.log($"Removed {count} invalid massacre missions");
            }
        }

        private void onJournalEntry(MissionAccepted entry)
        {
            if (entry.Name == "Mission_TheDead" || entry.Name == "Mission_TheDead_name")
            {
                this.cmdr.decodeTheRuinsMissionActive = TahMissionStatus.Active;
                this.cmdr.Save();
                Game.log("MissionAccepted: Starting 'Decoding the Ancient Ruins' ...");
            }
            else if (entry.Name == "Mission_TheDead_002" || entry.Name == "Mission_TheDead_002_name")
            {
                this.cmdr.decodeTheLogsMissionActive = TahMissionStatus.Active;
                this.cmdr.Save();
                Game.log("MissionAccepted: Starting 'Decrypting the Guardian Logs' ...");
            }
            else if (Game.settings.autoShowPlotMassacre_TEST && TrackMassacre.validMissionNames.Contains(entry.Name))
            {
                // track new massacre mission
                cmdr.addMassacreMission(entry);
            }
        }

        private void onJournalEntry(MissionFailed entry)
        {
            if (entry.Name == "Mission_TheDead" || entry.Name == "Mission_TheDead_name")
            {
                this.cmdr.decodeTheRuinsMissionActive = TahMissionStatus.NotStarted;
                this.cmdr.Save();
                Game.log("MissionFailed: Failed 'Decoding the Ancient Ruins' ...");
            }
            else if (entry.Name == "Mission_TheDead_002" || entry.Name == "Mission_TheDead_002_name")
            {
                this.cmdr.decodeTheLogsMissionActive = TahMissionStatus.NotStarted;
                this.cmdr.Save();
                Game.log("MissionFailed: Failed 'Decrypting the Guardian Logs' ...");
            }

            // remove massacre mission
            if (Game.settings.autoShowPlotMassacre_TEST)
                cmdr.removeMassacreMission(entry.MissionID, "Failed");
        }

        private void onJournalEntry(MissionAbandoned entry)
        {
            if (entry.Name == "Mission_TheDead" || entry.Name == "Mission_TheDead_name")
            {
                this.cmdr.decodeTheRuinsMissionActive = TahMissionStatus.NotStarted;
                this.cmdr.Save();
                Game.log("MissionAbandoned: Failed 'Decoding the Ancient Ruins' ...");
            }
            else if (entry.Name == "Mission_TheDead_002" || entry.Name == "Mission_TheDead_002_name")
            {
                this.cmdr.decodeTheLogsMissionActive = TahMissionStatus.NotStarted;
                this.cmdr.Save();
                Game.log("MissionAbandoned: Failed 'Decrypting the Guardian Logs' ...");
            }

            // remove massacre mission
            if (Game.settings.autoShowPlotMassacre_TEST)
                cmdr.removeMassacreMission(entry.MissionID, "Abandoned");
        }

        private void onJournalEntry(MissionCompleted entry)
        {
            if (entry.Name == "Mission_TheDead" || entry.Name == "Mission_TheDead_name")
            {
                this.cmdr.decodeTheRuinsMissionActive = TahMissionStatus.Complete;
                this.cmdr.Save();
                Game.log("MissionCompleted: Completed 'Decoding the Ancient Ruins' ...");
            }
            else if (entry.Name == "Mission_TheDead_002" || entry.Name == "Mission_TheDead_002_name")
            {
                this.cmdr.decodeTheLogsMissionActive = TahMissionStatus.Complete;
                this.cmdr.Save();
                Game.log("MissionCompleted: Completed 'Decrypting the Guardian Logs' ...");
            }

            // remove massacre mission
            if (Game.settings.autoShowPlotMassacre_TEST)
                cmdr.removeMassacreMission(entry.MissionID, "Completed");
        }

        private void onJournalEntry(Bounty entry)
        {
            if (Game.settings.autoShowPlotMassacre_TEST)
                cmdr.trackBounty(entry);
        }

        #endregion

        #region Cargo handling

        private static readonly List<string> cargoJournalEventTypes = new List<string>()
        {
            nameof(Cargo),
            nameof(CollectCargo),
            nameof(EjectCargo),
            nameof(CargoTransfer),
        };

        private void onJournalEntry(DockSRV entry)
        {
            this.systemData?.prepSettlements();
        }

        private void onJournalEntry(CollectCargo entry)
        {
            Game.log($"CollectCargo: {entry.Type_Localised} ({entry.Type})");

            var inventoryItem = this.cargoFile.Inventory.FirstOrDefault(_ => _.Name.Equals(entry.Type, StringComparison.OrdinalIgnoreCase));
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem(entry.Type, entry.Type_Localised);
                this.cargoFile.Inventory.Add(inventoryItem);
            }

            inventoryItem.Count++;
            Program.invalidateActivePlotters();
        }

        private void onJournalEntry(EjectCargo entry)
        {
            var inventoryItem = this.cargoFile.Inventory.FirstOrDefault(_ => _.Name.Equals(entry.Type, StringComparison.OrdinalIgnoreCase));
            if (inventoryItem == null)
            {
                Game.log($"EjectCargo: How can we eject cargo we do not have? {entry.Type_Localised} ({entry.Type})");
            }
            else
            {
                inventoryItem.Count -= entry.Count;
                Game.log($"EjectCargo: {entry.Count} x {entry.Type_Localised} ({entry.Type}), new count: {inventoryItem.Count}");
                if (inventoryItem.Count == 0)
                    this.cargoFile.Inventory.Remove(inventoryItem);
            }
            Program.invalidateActivePlotters();
        }

        private void onJournalEntry(CargoTransfer entry)
        {
            Game.log($"Updating inventory from transfer");
            foreach (var transferItem in entry.Transfers)
            {
                // TODO: check to / from ship?
                var inventoryItem = this.cargoFile.Inventory.FirstOrDefault(_ => _.Name.Equals(transferItem.Type, StringComparison.OrdinalIgnoreCase));
                if (inventoryItem == null)
                {
                    inventoryItem = new InventoryItem(transferItem.Type, transferItem.Type_Localised);
                    this.cargoFile.Inventory.Add(inventoryItem);
                }

                var delta = transferItem.Count;
                if ((this.vehicle == ActiveVehicle.SRV && transferItem.Direction == "toship") || (this.vehicle == ActiveVehicle.MainShip && transferItem.Direction == "tocarrier"))
                {
                    inventoryItem.Count -= delta;
                }
                else if ((this.vehicle == ActiveVehicle.SRV && transferItem.Direction == "tosrv") || (this.vehicle == ActiveVehicle.MainShip && transferItem.Direction == "toship"))
                {
                    inventoryItem.Count += delta;
                }
            }
            Program.invalidateActivePlotters();
            log($"Game.CargoTransfer: Current cargo:\r\n  " + string.Join("\r\n  ", this.cargoFile.Inventory));
        }

        private void onJournalEntry(CargoDepot entry)
        {
            // if we just handed in some required cargo but have some remainder, show a message for what remains
            if (Game.settings.allowNotifications.cargoMissionRemaining && entry.UpdateType == "Deliver" && entry.ItemsDelivered < entry.TotalItemsToDeliver)
            {
                var remaining = entry.TotalItemsToDeliver - entry.ItemsDelivered;
                PlotFloatie.showMessage($"Deliver {entry.CargoType}: {remaining} units remaining");
            }
        }

        private void onJournalEntry(Cargo entry)
        {
            if (Game.settings.buildProjects_TEST)
            {
                var diff = this.cargoFile.getDiff();
                if (diff.Count > 0)
                    this.cmdr.colonyData.supplyNeeds(lastDocked, diff);
            }
        }

        private void onJournalEntry(MarketBuy entry)
        {
            var item = cargoFile.Inventory.Find(i => i.Name == entry.Type);
            if (item == null)
            {
                // add if missing
                item = new InventoryItem(entry.Type, entry.Type_Localised);
                cargoFile.Inventory.Add(item);
            }
            item.Count += entry.Count;

            Program.invalidate<PlotBuildCommodities>();

            if (Game.settings.allowNotifications.fcMarketPurchaseBugReminder && this.fcMarketIds.Contains(entry.MarketId))
                marketBuyOnFC = true;
        }

        public bool marketBuyOnFC = false;
        public bool marketEventSeen = false;
        public HashSet<long> fcMarketIds = new();

        private void onJournalEntry(Market entry)
        {
            marketEventSeen = true;

            if (Game.settings.allowNotifications.fcMarketPurchaseBugReminder && entry.StationType == "FleetCarrier")
            {
                // remember if this is a FleetCarrier market - so we can help people avoid annoying not-yet-purchased bug
                this.fcMarketIds.Add(entry.MarketId);
                this.marketBuyOnFC = false;
            }

            if (PlotBuildCommodities.allowPlotter)
                Program.showPlotter<PlotBuildCommodities>();
        }

        private static Dictionary<string, string> inventoryItemNameMap = new Dictionary<string, string>()
        {
            { "ca", "ancientcasket" },
            { "casket", "ancientcasket" },
            { "or", "ancientorb" },
            { "orb", "ancientorb" },
            { "re", "ancientrelic" },
            { "relic", "ancientrelic" },
            { "ta", "ancienttablet" },
            { "tablet", "ancienttablet" },
            { "to", "ancienttotem" },
            { "totem", "ancienttotem" },
            { "ur", "ancienturn" },
            { "urn", "ancienturn" },

            { "se", "unknownartifact" }, // Thargoid Sensor
            { "sensor", "unknownartifact" },
            { "pr", "unknownartifact2" }, // Thargoid Probe
            { "probe", "unknownartifact2" },
            { "li", "unknownartifact3" }, // Thargoid Link
            { "link", "unknownartifact3" },
            { "cy", "thargoidtissuesampletype1" }, // Thargoid Cyclops Tissue Sample
            { "cyclops", "thargoidtissuesampletype1" },
            { "ba", "thargoidtissuesampletype2" }, // Thargoid Basilisk Tissue Sample
            { "basilisk", "thargoidtissuesampletype2" },
            { "me", "thargoidtissuesampletype3" }, // Thargoid Medusa Tissue Sample
            { "medusa", "thargoidtissuesampletype3" },
        };

        public InventoryItem? getInventoryItem(string itemName)
        {
            // for convenience, allow itemName to be an alias of what the game really uses, eg: 'ta' becomes 'ancienttablet'
            if (inventoryItemNameMap.ContainsKey(itemName))
                itemName = inventoryItemNameMap[itemName];

            var inventoryItem = this.cargoFile.Inventory.FirstOrDefault(_ => _.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            return inventoryItem;
        }

        private void onJournalEntry(Materials entry)
        {
            // keep track of this, it will be maintained
            this.materials = entry;

            if (!guardianMatsFull)
                guardianMatsFull = entry.Encoded.Find(_ => _.Name == "ancientbiologicaldata")?.Count == 150;
            if (!guardianMatsFull)
                guardianMatsFull = entry.Encoded.Find(_ => _.Name == "ancientlanguagedata")?.Count == 150;
            if (!guardianMatsFull)
                guardianMatsFull = entry.Encoded.Find(_ => _.Name == "ancientculturaldata")?.Count == 150;
            if (!guardianMatsFull)
                guardianMatsFull = entry.Encoded.Find(_ => _.Name == "ancienttechnologicaldata")?.Count == 150;
            if (!guardianMatsFull)
                guardianMatsFull = entry.Encoded.Find(_ => _.Name == "ancienthistoricaldata")?.Count == 150;
        }

        private void onJournalEntry(MaterialCollected entry)
        {
            if (Game.settings.allowNotifications.materialCountAfterPickup)
            {
                // find the matching material
                var match = this.materials.getMaterialEntry(entry.Category, entry.Name);
                if (match == null)
                {
                    Game.log("Why no material match??");
                    Debugger.Break();
                    return;
                }

                // adjust the count and show a message
                match.Count += entry.Count;
                PlotFloatie.showMessage($"Collected: {entry.Count}x {match.displayName}, new total {match.Count}"); // TODO: localize
            }
        }

        private void onJournalEntry(MaterialTrade entry)
        {
            // track adjustments to Materials from Material Trader interactions

            var matchPaid = this.materials.getMaterialEntry(entry.Paid.Category, entry.Paid.Material);
            if (matchPaid == null)
            {
                Game.log($"MaterialTrade: Why no paid material '{entry.Paid.Category}'/'{entry.Paid.Material}' match??");
                Debugger.Break();
                return;
            }

            var matchReceived = this.materials.getMaterialEntry(entry.Received.Category, entry.Received.Material);
            if (matchReceived == null)
            {
                Game.log($"MaterialTrade: Why no received material '{entry.Received.Category}'/'{entry.Received.Material}' match??");
                Debugger.Break();
                return;
            }

            matchPaid.Count -= entry.Paid.Quantity;
            matchReceived.Count += entry.Received.Quantity;
            Game.log($"MaterialTrade: {matchPaid.Name} -{entry.Paid.Quantity} => {matchPaid.Count} | {matchReceived.Name} +{entry.Received.Quantity} => {matchReceived.Count}");
        }

        private void onJournalEntry(TechnologyBroker entry)
        {
            // track adjustments to Materials from Tech Broker interactions
            foreach (var mat in entry.Materials)
            {
                var match = this.materials.getMaterialEntry(mat.Category, mat.Name);
                if (match == null)
                {
                    Game.log($"TechnologyBroker: Why no material '{mat.Category}'/'{mat.Name}' match??");
                    Debugger.Break();
                    continue;
                }

                match.Count -= mat.Count;
                Game.log($"TechnologyBroker: {match.Name} -{mat.Count} => {match.Count}");
            }
        }

        #endregion

        #region location tracking

        public void toggleFirstFootfall(string? bodyName)
        {
            if (this.systemData == null) return;
            if (string.IsNullOrEmpty(bodyName) && this.systemBody == null) return;

            // try to match the given bodyName
            SystemBody? targetBody = null;
            if (!string.IsNullOrEmpty(bodyName))
            {
                // match on just the body name?
                bodyName = bodyName.Trim().ToLowerInvariant();
                targetBody = this.systemData.bodies.FirstOrDefault(body =>
                {
                    var bodyNotSystem = body.name.Replace(systemData.name, "").Trim();
                    if (bodyNotSystem.Equals(bodyName, StringComparison.OrdinalIgnoreCase)) return true;
                    if (bodyNotSystem.Replace(" ", "").Equals(bodyName, StringComparison.OrdinalIgnoreCase)) return true;
                    return false;
                });
                if (targetBody == null)
                    Game.log($"No body matched from: '{bodyName}'");
                else
                    Game.log($"Match body '{targetBody?.name}' from: '{bodyName}'");
            }
            if (targetBody == null) targetBody = this.systemBody;


            targetBody!.firstFootFall = !targetBody.firstFootFall;
            Game.log($"Recording first Footfall: {targetBody.firstFootFall} for '{targetBody.name}' ({targetBody.id})");
            this.systemData.Save();

            // apply update
            var totalReward = 0L;
            var list = cmdr.scannedBioEntryIds.ToList();
            for (var n = 0; n < list.Count; n++)
            {
                var parts = list[n].Split('_');

                // toggle if it's the body we're on
                var prefix = $"{this.systemData.address}_{targetBody.id}_";
                if (list[n].StartsWith(prefix))
                {
                    parts[4] = targetBody.firstFootFall.ToString();
                    list[n] = string.Join('_', parts);
                }

                // and sum the new total
                var reward = long.Parse(parts[3]);
                if (parts[4] == bool.TrueString)
                    reward *= 5;
                totalReward += reward;
            }
            this.cmdr.scannedBioEntryIds = new HashSet<string>(list);

            Game.log($"Updated rewards total: {Util.credits(totalReward)}, was {Util.credits(cmdr.organicRewards)}");
            cmdr.organicRewards = totalReward;
            this.cmdr.Save();
            this.fireUpdate(true);
        }

        public string? getNearestSettlement()
        {
            // must be near a body and under 4km
            if (this.systemBody == null || !(this.systemBody.settlements?.Count > 0) || this.status.Altitude > 4000)
                return null;

            decimal minDist = decimal.MaxValue;
            string? minSite = null;
            foreach (var site in this.systemBody.settlements.Keys)
            {
                var dist = Util.getDistance(this.systemBody.settlements[site], Status.here, this.systemBody.radius);
                // TODO: Keep 4000m rule?
                if (dist < minDist)
                {
                    minDist = dist;
                    minSite = site;
                }
            }
            return minSite;
        }

        private void setCurrentSite()
        {
            var fireEvent = false;
            var nearestSettlement = this.getNearestSettlement();

            if (this.systemBody != null)
            {
                // load 'systemSite' if needed
                if (nearestSettlement != null && nearestSettlement.StartsWith("$Ancient"))
                {
                    if (this.systemSite == null)
                    {
                        log($"Close enough, creating  systemSite: '{nearestSettlement}' on '{this.systemBody!.name}' ");
                        this.systemSite = GuardianSiteData.Load(this.systemBody.name, nearestSettlement);

                        // create entry if no match found
                        if (this.systemSite == null)
                            Game.log($"Why no site for: '{nearestSettlement}' on 'this.systemBody.name' ?");

                        fireEvent = true;
                    }
                    else if (this.systemSite.name != nearestSettlement)
                    {
                        // TODO: check this where there are 2 ruins next to each other.
                        // Checked: this is mostly okay
                        this.systemSite = GuardianSiteData.Load(this.systemBody.name, nearestSettlement);
                        Program.closeAllPlotters();
                        fireEvent = true;
                    }
                }
            }

            // remove 'systemSite' if needed
            if (nearestSettlement == null && this.systemSite != null)
            {
                log($"Too far, clearing systemSite: '{systemSite.name}' ");
                this.systemSite.Save();
                this.systemSite = null;
                fireEvent = true;
            }

            if (fireEvent)
                this.fireUpdate(true);
        }

        private void setCurrentBody(string bodyName)
        {
            log($"setCurrentBody by name: {bodyName}");
            if (this.systemData == null)
            {
                Game.log($"setCurrentBody: No systemData - ignoring bodyName: {bodyName}");
                return;
            }
            var wasNull = this.systemBody == null;
            this.systemBody = this.systemData.bodies.FirstOrDefault(_ => _.name == bodyName);
            if (this.systemBody != null && wasNull)
                this.fireUpdate(true);

            this.setCurrentSite();
            Program.invalidateActivePlotters();
        }

        private void setCurrentBody(int bodyId)
        {
            log($"setCurrentBody by id: {bodyId}");
            if (this.systemData == null)
            {
                Game.log($"Why no systemData for bodyId: {bodyId}?");
                return;
            }
            var wasNull = this.systemBody == null;
            this.systemBody = this.systemData.bodies.FirstOrDefault(_ => _.id == bodyId);
            if (this.systemBody != null && wasNull)
                this.fireUpdate(true);

            this.setCurrentSite();
            Program.invalidateActivePlotters();
        }

        public void setLocations(ISystemDataStarter entry)
        {
            Game.log($"setLocations: from FSDJump/CarrierJump: {entry.StarSystem} / {entry.Body} ({entry.BodyType})");

            cmdr.currentSystem = entry.StarSystem;
            cmdr.currentSystemAddress = entry.SystemAddress;
            cmdr.starPos = entry.StarPos;
            this.systemData = SystemData.From(entry);

            // update our region
            cmdr.setGalacticRegion(entry.StarPos);

            if (entry.BodyType == FSDJumpBodyType.Planet)
            {
                // would this ever happen?
                Game.log($"setLocations: FSDJump/Carrier is a planet?!");

                cmdr.currentBody = entry.Body;
                cmdr.currentBodyId = entry.BodyID;
                this.setCurrentBody(entry.BodyID);

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
                this.systemBody = null;
                this.setCurrentSite();
            }
            cmdr.lastSystemLocation = Util.getLocationString(entry.StarSystem, entry.Body);
            cmdr.Save();
            this.fireUpdate();

            if (Game.settings.useExternalData)
                this.fetchSystemData(entry.StarSystem, entry.SystemAddress);
        }

        public void setLocations(ApproachBody entry)
        {
            Game.log($"setLocations: from ApproachBody: {entry.StarSystem} / {entry.Body}");

            cmdr.currentSystem = entry.StarSystem;
            cmdr.currentSystemAddress = entry.SystemAddress;

            cmdr.currentBody = entry.Body;
            cmdr.currentBodyId = entry.BodyID;
            this.setCurrentBody(entry.BodyID);

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

            if (Game.settings.useExternalData)
                this.fetchSystemData(entry.StarSystem, entry.SystemAddress);
        }

        public void setLocations(SupercruiseExit entry)
        {
            Game.log($"setLocations: from SupercruiseExit: {entry.Starsystem} / {entry.Body} ({entry.BodyType})");

            cmdr.currentSystem = entry.Starsystem;
            cmdr.currentSystemAddress = entry.SystemAddress;

            if (entry.BodyType == FSDJumpBodyType.Planet)
            {
                cmdr.currentBody = entry.Body;
                cmdr.currentBodyId = entry.BodyID;
                this.setCurrentBody(entry.BodyID);

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
                this.systemBody = null;
                this.setCurrentSite();
            }

            cmdr.lastSystemLocation = Util.getLocationString(entry.Starsystem, entry.Body);
            cmdr.Save();

            if (Game.settings.useExternalData)
                this.fetchSystemData(entry.Starsystem, entry.SystemAddress);
        }

        public void setLocations(Location entry, string? fid = null, string? cmdrName = null)
        {
            Game.log($"setLocations: from Location: {entry.StarSystem} / {entry.Body} ({entry.BodyType})");

            cmdr.currentSystem = entry.StarSystem;
            cmdr.currentSystemAddress = entry.SystemAddress;
            cmdr.starPos = entry.StarPos;
            this.systemData = SystemData.From(entry, fid, cmdrName);

            // update our region
            cmdr.setGalacticRegion(entry.StarPos);

            if (entry.BodyType == FSDJumpBodyType.Planet)
            {
                cmdr.currentBody = entry.Body;
                cmdr.currentBodyId = entry.BodyID;
                this.setCurrentBody(entry.BodyID);

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
                this.systemBody = null;
                this.setCurrentSite();
            }

            cmdr.lastSystemLocation = Util.getLocationString(entry.StarSystem, entry.Body);
            cmdr.Save();

            if (Game.settings.useExternalData)
                this.fetchSystemData(entry.StarSystem, entry.SystemAddress);
        }

        private decimal findLastRadius(string bodyName)
        {
            decimal planetRadius = -1;

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

        /// <summary>
        /// Request data about this system from EDSM, Canonn and Spansh
        /// </summary>
        private void fetchSystemData(string systemName, long systemAddress)
        {
            // exit early if we're already processing this system
            if (this.fetchedSystemData == systemName) return;

            try
            {
                this.fetchedSystemData = systemName;
                Game.log($"this.fetchedSystemData = '{systemName}' - begin");

                if (!Game.settings.useExternalData || this.canonnPoi?.system == systemName || this.systemData == null)
                {
                    return;
                }
                else if (this.canonnPoi != null)
                {
                    Game.log($"why/when {systemName} vs {this.canonnPoi.system}");
                    this.canonnPoi = null;
                }

                var shouldRefreshGuardianSystemStatus = false;

                // lookup system from Spansh
                Game.log($"Searching Spansh for '{systemName}' ({systemAddress})...");
                var taskSpansh = Game.spansh.getSystemDump(systemAddress).ContinueWith(response =>
                {
                    if (this.systemData == null || this.fetchedSystemData != systemName) return;

                    if (response.IsCompletedSuccessfully)
                    {
                        var spanshSystem = response.Result;

                        Game.log($"Found {spanshSystem.bodyCount} bodies from Spansh for '{systemName}'");
                        if (this.systemData != null && spanshSystem.id64 == this.systemData?.address)
                        {
                            this.systemData.onSpanshResponse(spanshSystem);

                            // update Guardian system status if any body has a Guardian signal
                            var foo = spanshSystem.bodies?.Any(body => body.signals?.signals?.Any(_ => _.Key.Contains("Guardian", StringComparison.OrdinalIgnoreCase)) == true) == true;
                            if (foo)
                                shouldRefreshGuardianSystemStatus = true;
                        }
                    }
                    else if ((response.Exception?.InnerException as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(response.Exception))
                    {
                        // ignore NotFound responses
                    }
                    else
                    {
                        Game.log($"Spansh call failed? {response.Exception}");
                    }
                });

                // lookup system from EDSM
                Game.log($"Searching EDSM for '{systemName}'...");
                var taskEDSM = Game.edsm.getBodies(systemName).ContinueWith(response =>
                {
                    if (this.systemData == null || this.fetchedSystemData != systemName) return;

                    if (response.IsCompletedSuccessfully)
                    {
                        var edsmData = response.Result;
                        Game.log($"Found {edsmData.bodyCount} bodies from EDSM for '{systemName}'");
                        if (edsmData.id64 == this.systemData?.address)
                            this.systemData.onEdsmResponse(edsmData);
                    }
                    else if ((response.Exception?.InnerException as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(response.Exception))
                    {
                        // ignore NotFound responses
                    }
                    else
                    {
                        Game.log($"EDSM call failed? {response.Exception}");
                    }
                });

                // make a call for Canonn system POIs and pre-load trackers for known bio-signals
                Game.log($"Searching for system POI from Canonn...");
                var taskCanonnPoi = Game.canonn.getSystemPoi(systemName, this.cmdr.commander).ContinueWith(response =>
                {
                    if (this.systemData == null || this.fetchedSystemData != systemName) return;

                    if (response.IsCompletedSuccessfully)
                    {
                        this.canonnPoi = response.Result;
                        Game.log($"Found system POI from Canonn for: {systemName}");
                        this.systemData.onCanonnPoiData(this.canonnPoi);

                        // update Guardian system status if any there are some Guardian signals
                        var foo = this.canonnPoi.SAAsignals?.Any(_ => _.hud_category == "Guardian") == true;
                        if (foo)
                            shouldRefreshGuardianSystemStatus = true;

                    }
                    else if ((response.Exception?.InnerException as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(response.Exception))
                    {
                        // ignore NotFound responses
                    }
                    else
                    {
                        Game.log($"Canonn getSystemPoi failed? {response.Exception}");
                    }
                });

                // make a call for Canonn stations
                var taskCanonnStations = Game.canonn.getStations(systemAddress).ContinueWith(response =>
                {
                    if (this.systemData == null || this.fetchedSystemData != systemName) return;

                    if (response.IsCompletedSuccessfully)
                    {
                        var stations = response.Result;
                        Game.log($"Found {stations.Count} stations from Canonn in: {systemName}");
                        if (stations != null)
                            this.systemData.onCanonnStationData(stations);

                    }
                    else if ((response.Exception?.InnerException as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(response.Exception))
                    {
                        // ignore NotFound responses
                    }
                    else
                    {
                        Game.log($"Canonn getStations failed? {response.Exception}");
                    }
                });

                Task.WhenAll(taskSpansh, taskEDSM, taskCanonnPoi, taskCanonnStations).ContinueWith(response =>
                {
                    this.fetchSystemDataEnd(shouldRefreshGuardianSystemStatus);
                });

                // measure distance to closest nebula
                this.systemData.getNebulaDist().ContinueWith(response =>
                {
                    this.predictSystemSpecies();
                });
            }
            finally
            {
                Game.log($"this.fetchedSystemData = '{systemName}' - complete");
            }
        }

        private void fetchSystemDataEnd(bool shouldRefreshGuardianSystemStatus)
        {
            if (shouldRefreshGuardianSystemStatus)
                this.systemData?.prepSettlements();

            this.systemData?.Save();
            this.fireUpdate(true);
        }

        /// <summary>
        /// Returns True when there are plottable bio signals in canonnPoi for the current body
        /// </summary>
        public bool canonnPoiHasLocalBioSignals()
        {
            if (this.systemData == null || this.systemBody == null || this.canonnPoi?.codex == null) return false;

            var hasSignals = this.canonnPoi.codex.Any(_ => _.body?.Replace(" ", "") == this.systemBody.shortName && _.hud_category == "Biology" && _.latitude != null && _.longitude != null && (!Game.settings.hideMyOwnCanonnSignals || _.scanned == false));
            return hasSignals;
        }

        //public void showPriorScans()
        //{
        //    if (this.canonnPoi?.codex == null || this.systemData == null || this.systemBody == null) return;

        //    Game.log($"Filtering organic signals from Canonn...");
        //    var currentBody = this.systemBody.name.Replace(this.systemData.name, "").Trim();
        //    var bioPoi = this.canonnPoi.codex.Where(_ => _.body == currentBody && _.hud_category == "Biology" && _.latitude != null && _.longitude != null).ToList();
        //    Game.log($"Found {bioPoi.Count} organic signals from Canonn for: {this.systemBody.name}");

        //    if (Game.settings.autoLoadPriorScans && bioPoi.Count > 0 && this.mode != GameMode.SuperCruising && (this.isLanded || this.cmdr.scanOne != null))
        //    {
        //        // show prior scans overlay
        //        Program.control.Invoke(new Action(() =>
        //        {
        //            Program.showPlotter<PlotPriorScans>()?.setPriorScans();
        //        }));
        //    }

        //}

        /// <summary>
        /// Returns most recent data for faction Inf, current state and cmdr Rep from the current journal file.
        /// </summary>
        public (double rep, double inf, string? state) getFactionInfRep(string factionName)
        {
            double rep = 0;
            double inf = 0;
            string? state = null;
            // find cmdr's reputation with this faction
            this.journals?.walk(-1, true, entry =>
            {
                var factionsEntry = entry as IFactions;
                if (factionsEntry?.Factions != null)
                {
                    if (factionsEntry.SystemAddress == systemData?.address)
                    {
                        var faction = factionsEntry.Factions.Find(f => f.Name == factionName)!;
                        rep = faction?.MyReputation ?? -1;
                        inf = faction?.Influence ?? -1;
                        state = faction?.FactionState;
                    }
                    return true;
                }

                return false;
            });

            return (rep, inf, state);
        }

        #endregion

        #region journal tracking for ground ops

        private LatLong2? _touchdownLocation;
        public LatLong2? srvLocation;

        public LatLong2 touchdownLocation
        {
            get
            {
                if (this._touchdownLocation == null && status.Docked && status.hasLatLong)
                {
                    this._touchdownLocation = Status.here.clone();
                    Game.log($"Using current location as we're docked already: {_touchdownLocation}");
                }
                if (this._touchdownLocation == null && this.journals != null)
                {
                    Game.log($"Searching journals for last touchdown location... (mode: {this.mode})");
                    journals.searchDeep(
                        (Touchdown entry) =>
                        {
                            _touchdownLocation = new LatLong2(entry);
                            Game.log($"Found last touchdown location: {_touchdownLocation}");
                            return true;
                        },
                        // stop searching older journal files if we see we reached this system
                        (JournalFile journals) => journals.search((ApproachBody _) => true)
                    );

                    // stop endless repeats
                    if (this._touchdownLocation == null)
                    {
                        if (status.hasLatLong && systemBody != null && systemBody.name == status.BodyName && systemBody.lastTouchdown != null)
                        {
                            Game.log($"Last touchdown not found since ApproachBody - using last from systemBody");
                            this._touchdownLocation = systemBody.lastTouchdown;
                        }
                        else
                        {
                            Game.log($"Last touchdown not found since ApproachBody");
                            this._touchdownLocation = LatLong2.Empty;
                        }
                    }

                    /*
                    var lastTouchdown = journals!.FindEntryByType<Touchdown>(-1, true);
                    var lastLiftoff = journals.FindEntryByType<Liftoff>(-1, true);
                    if (lastTouchdown != null)
                    {
                        if (lastLiftoff == null || lastTouchdown.timestamp > lastLiftoff.timestamp)
                        {
                            _touchdownLocation = new LatLong2(lastTouchdown);
                            Game.log($"Found last touchdown location: {_touchdownLocation}");
                        }
                        else
                        {
                            // TODO: search deep for landing in prior journal file.
                            Game.log($"Last touchdown location: NOT FOUND");
                            //    _touchdownLocation = new LatLong2(lastTouchdown);
                        }
                    }
                    else if (this.mode == GameMode.Landed)
                    {
                        _touchdownLocation = Status.here.clone();
                        Game.log($"Last touchdown location: NOT FOUND but we're landed so using current location: {_touchdownLocation}");
                    }
                    */
                }

                if (_touchdownLocation == null && cmdr.lastTouchdownLocation != null)
                {
                    Game.log($"Last touchdown not found anywhere - cmdr.lastTouchdownLocation: {cmdr.lastTouchdownLocation}");
                    _touchdownLocation = cmdr.lastTouchdownLocation;
                }

                return _touchdownLocation!;
            }
            set
            {
                this._touchdownLocation = value;
                if (this.systemBody != null) this.systemBody.lastTouchdown = value;
            }
        }

        private void onJournalEntry(Touchdown entry)
        {
            // wait a bit for the status file to be written?
            Task.Delay(10).ContinueWith((x) =>
            {
                // store the heading at the time of touchdown, so we can adjust the cockpit location to the center of the ship
                this.cmdr.setTouchdown(entry, status.Heading);
                this.touchdownLocation = entry;

                var ago = Util.timeSpanToString(DateTime.UtcNow - entry.timestamp);
                log($"Touchdown {ago}, at: {touchdownLocation}");
                this.fireUpdate(true);
            });
        }

        private void onJournalEntry(Liftoff entry)
        {
            this._touchdownLocation = LatLong2.Empty;
            this.systemData?.prepSettlements();
        }

        private void onJournalEntry(ApproachBody entry)
        {
            this.setLocations(entry);
        }

        private void onJournalEntry(ApproachSettlement entry)
        {
            if (entry.Name.StartsWith("$Ancient"))
            {
                // Guardian site
                PlotGuardianStatus.glideSite = GuardianSiteData.Load(entry);
                PlotGuardianStatus.glideSite.loadPub();
                this.setCurrentSite();

                if (systemSite != null)
                {
                    if (this.systemSite.lastVisited < entry.timestamp)
                    {
                        this.systemSite.lastVisited = entry.timestamp;
                        this.systemSite.Save();
                    }
                }
                return;
            }

            // new ...
            if (this.systemStation != null && systemStation.marketId != entry.MarketID) Debugger.Break(); // Would this ever happen?
            if (this.systemData == null) return;

            if (entry.StationServices == null
                || entry.StationServices.Count == 0 // horizons old settlements are not compatible
                || entry.StationServices.Contains("socialspace") // bigger settlements (Planetary ports) are not compatible
                || entry.StationGovernment == "$government_Engineer;" // Engineer's stations (with no socialspace) are also not compatible
            )
                return;

            // use known station reference if this station is known, or start creating a new one
            this.systemStation = systemData.getStation(entry.MarketID);
            if (this.systemStation == null)
            {
                Game.log($"Creating new CanonnStation for '{entry.Name}' ({entry.MarketID}) ");
                this.systemStation = new CanonnStation()
                {
                    name = entry.Name,
                    marketId = entry.MarketID,
                    systemAddress = entry.SystemAddress,
                    bodyId = entry.BodyID,
                    bodyRadius = (double)(systemBody?.radius ?? 0),
                    stationEconomy = entry.StationEconomy!,
                    // stationType is from Docked
                    lat = entry.Latitude,
                    @long = entry.Longitude,
                };

                if (systemData.stations == null) systemData.stations = new List<CanonnStation>();
                systemData.stations.Add(systemStation);
                systemData.Save();
            }

            // apply non-persisted values
            this.systemStation.applyNonSavedSettlementValues(entry, this.journals);

            Game.log($"~~ApproachSettlement: " + JsonConvert.SerializeObject(this.systemStation, Formatting.Indented));

            // TODO: Thargoid sites?
        }

        private void onJournalEntry(DockingRequested entry)
        {
            // stop here if not an Odyssey settlement
            if (entry.StationType != StationType.OnFootSettlement) return;
            if (this.systemStation == null || this.systemStation.marketId != entry.MarketID)
            {
                this.initHumanSite();
                return;
            }

            // new ...
            this.systemStation.stationType = entry.StationType;
            this.systemStation.availablePads = entry.LandingPads;

            //// old ...
            //if (entry.StationType == StationType.OnFootSettlement && this.humanSite?.marketId == entry.MarketID && Game.settings.autoShowHumanSitesTest)
            //{
            //    this.humanSite.dockingRequested(entry); // old
            //}
        }

        private void onJournalEntry(DockingCancelled entry)
        {
            this.dockingInProgress = false;
        }

        private void onJournalEntry(DockingDenied entry)
        {
            this.dockingInProgress = false;
        }

        private void onJournalEntry(DockingGranted entry)
        {
            // stop here if not an Odyssey settlement
            this.dockingInProgress = true;
            if (entry.StationType != StationType.OnFootSettlement || systemData == null) return;
            if (this.systemStation == null || this.systemStation.marketId != entry.MarketID) { return; }

            // new ...
            this.systemStation.stationType = entry.StationType;
            this.systemStation.cmdrPad = entry.LandingPad;
            // some subTypes can be inferred from just the economy and pad configuration...
            var changed = CanonnStation.inferOdysseySettlementFromPads(this.systemStation);
            if (changed) systemData.Save();

            //// old ...
            //if (entry.StationType == StationType.OnFootSettlement && this.humanSite?.marketId == entry.MarketID && Game.settings.autoShowHumanSitesTest)
            //{
            //    this.humanSite.dockingGranted(entry); // old
            //}
        }

        private void onJournalEntry(Docked entry)
        {
            this.lastDocked = entry;

            // store that we've docked here
            this.cmdr.setMarketId(entry.MarketID);
            this.dockingInProgress = false;

            // end the dock-to-dock timer
            if (Game.settings.logDockToDockTimes && dockTimer != null)
                dockTimer.onJournalEntry(entry);

            if (entry.StationType != StationType.OnFootSettlement || this.systemData == null) return;

            // new ...
            if (this.systemStation == null || this.systemStation.marketId != entry.MarketID)
            {
                this.initHumanSite();
                return;
            }

            if (this.systemStation.availablePads == null)
                this.systemStation.availablePads = entry.LandingPads;

            if (this._mode == GameMode.NoFocus && status.Docked)
            {
                // when called from initHumanSite during initialization, delay not needed
                onDockedWhenSafe(CalcMethod.AutoDock, false);
            }
            else if (this.musicTrack != "DockingComputer" && !entry.Taxi)
            {
                // wait for a mode change - as Heading in status file does not update until something else happens
                Task.Delay(3000).ContinueWith(t =>
                {
                    Game.log($"~~enqueue onDockedWhenSafe: alt: {status.Altitude} / {status.Heading}");
                    this.processDockedOnNextStatusChange = true;
                });
            }
            else
            {
                // delay a little
                Task.Delay(500).ContinueWith(t => onDockedWhenSafe(CalcMethod.AutoDock, entry.Taxi));
            }

            //// old ...
            //// wait a bit for the status file to be written?
            //if (this.humanSite == null)
            //{
            //    this.initHumanSite();
            //    if (Game.settings.collectMatsCollectionStatsTest && this.humanSite != null) this.initMats(this.humanSite);
            //}
            //else if (this.humanSite.marketId == entry.MarketID)
            //{
            //    this.humanSite.docked(entry, this.status.Heading);
            //    if (Game.settings.collectMatsCollectionStatsTest) this.initMats(this.humanSite);
            //}
            //else
            //{
            //    Game.log($"Mismatched name or marketId?! {entry.StationName} ({entry.MarketID})");
            //}

        }

        private void onDockedWhenSafe(CalcMethod calcMethod, bool inTaxi)
        {
            Game.log($"~~onDockedWhenSafe: alt: {status.Altitude}, heading: {status.Heading}, docked: {status.Docked} / {calcMethod} / {inTaxi}\r\n{status.Flags}\r\n{status.Flags2}");
            if (systemStation == null || systemData == null || (calcMethod == CalcMethod.ManualDock && !status.Docked)) return;

            // use current location as touchdown
            // store the heading at the time of touchdown, so we can adjust the cockpit location to the center of the ship
            var here = Status.here.clone();
            this.cmdr.setTouchdown(here, this.status.Heading);
            this.touchdownLocation = here;

            var changed = systemStation.inferFromShip(
                status.Heading,
                this.status.Latitude,
                this.status.Longitude,
                inTaxi ? "taxi" : this.currentShip.type,
                (double)status.PlanetRadius,
                calcMethod);

            if (changed)
            {
                systemData.Save();

                // Restore?
                // if (Game.settings.collectMatsCollectionStatsTest && this.humanSite != null) this.initMats(this.humanSite);
            }

            this.fireUpdate(true);
        }

        private void onJournalEntry(Undocked entry)
        {
            this.lastDocked = null;
            this.marketBuyOnFC = false;
            this.touchdownLocation = LatLong2.Empty;

            if (Game.settings.collectMatsCollectionStatsTest && this.matStatsTracker != null)
            {
                // stop capturing stats once undocked
                this.exitMats(true);
            }

            // start the dock-to-dock timer
            if (Game.settings.logDockToDockTimes)
                dockTimer = new DockToDockTimer(this, entry, this.lastDocked);
        }

        public void initMats(CanonnStation station)
        {
            Game.log($"initMats: '{station.name}' ({station.marketId}) ");

            if (this.matStatsTracker != null && Game.settings.collectMatsCollectionStatsTest)
                this.matStatsTracker.Save();

            if (this.matStatsTracker == null || matStatsTracker.marketId != station.marketId)
                this.matStatsTracker = SettlementMatCollectionData.Load(station);
        }

        public void startMats()
        {
            if (this.matStatsTracker == null) return;

            // populate things from the last ApproachSettlement
            this.journals!.searchDeep((ApproachSettlement entry) =>
            {
                if (entry.BodyName != status.BodyName || entry.SystemAddress != this.systemData?.address) return true;

                this.matStatsTracker.systemAddress = entry.SystemAddress;
                this.matStatsTracker.bodyId = entry.BodyID;
                if (!string.IsNullOrEmpty(entry.StationFaction?.Name)) this.matStatsTracker.factionName = entry.StationFaction.Name;
                if (!string.IsNullOrEmpty(entry.StationGovernment)) this.matStatsTracker.stationGovernment = entry.StationGovernment;
                if (!string.IsNullOrEmpty(entry.StationAllegiance)) this.matStatsTracker.stationAllegiance = entry.StationAllegiance;
                if (!string.IsNullOrEmpty(entry.StationEconomy)) this.matStatsTracker.stationEconomy = entry.StationEconomy;

                return true;
            });

            // populate things from the last FSDJump
            this.journals!.searchDeep((FSDJump entry) =>
            {
                if (entry.SystemAddress != this.systemData?.address) return true;

                this.matStatsTracker.systemAllegiance = entry.SystemAllegiance;
                this.matStatsTracker.systemEconomy = entry.SystemEconomy;
                this.matStatsTracker.systemSecondEconomy = entry.SystemSecondEconomy;
                this.matStatsTracker.systemGovernment = entry.SystemGovernment;
                this.matStatsTracker.systemSecurity = entry.SystemSecurity;
                this.matStatsTracker.population = entry.Population;

                this.matStatsTracker.systemFactionName = entry.SystemFaction.Name;
                var systemFaction = entry.Factions?.FirstOrDefault(f => f.Name == entry.SystemFaction.Name);
                if (systemFaction != null)
                    this.matStatsTracker.systemFactionState = systemFaction.FactionState;

                return true;
            });

            // Don't save until we pick up some item
        }

        public void exitMats(bool complete = false)
        {
            if (this.matStatsTracker != null && Game.settings.collectMatsCollectionStatsTest)
            {
                Game.log($"Stopped mats collection at: '{this.matStatsTracker.name}'\r\n${this.matStatsTracker.filepath}");
                this.matStatsTracker.completed = complete;
                if (this.matStatsTracker.totalMatCount > 0)
                    this.matStatsTracker.Save();
                this.matStatsTracker = null;
            }
        }

        private void onJournalEntry(BackpackChange entry)
        {
            if (this.matStatsTracker == null || this.systemStation == null || systemStation.heading == -1 || entry.Added == null) return;
            Application.DoEvents();

            // TODO: Use BackpackChange only for Data, restore CollectItems for the others

            foreach (var item in entry.Added)
            {
                if (item.Type != "Data") continue;

                // Track location offset +1m by cmdr heading, to account for mats being in front of cmdr
                var cmdrOffset = Util.getOffset(status.PlanetRadius, systemStation.location, systemStation.heading);
                var a = status.Heading - (decimal)systemStation.heading;
                if (a < 0) a += 360;
                if (a > 360) a -= 360;
                var d = Util.rotateLine(a, 1);
                var location = cmdrOffset + d;

                var building = systemStation.template?.getCurrentBld((PointF)cmdrOffset, systemStation.heading);
                this.matStatsTracker.track(item, (PointF)location, building);
            }

            Program.getPlotter<PlotHumanSite>()?.Invalidate();
            if (Game.settings.collectMatsCollectionStatsTest)
                this.matStatsTracker.Save();
        }

        private void onJournalEntry(CollectItems entry)
        {
            if (this.matStatsTracker == null || this.systemStation == null || systemStation.heading == -1) return;
            if (entry.Type == "Data") return;
            Application.DoEvents();

            // Track location offset +1m by cmdr heading, to account for mats being in front of cmdr
            var cmdrOffset = Util.getOffset(status.PlanetRadius, systemStation.location, systemStation.heading);
            var a = status.Heading - (decimal)systemStation.heading;
            if (a < 0) a += 360;
            if (a > 360) a -= 360;
            var d = Util.rotateLine(a, 1);
            var location = cmdrOffset + d;

            var building = systemStation.template?.getCurrentBld((PointF)cmdrOffset, systemStation.heading);
            this.matStatsTracker.track(entry, (PointF)location, building);

            Program.getPlotter<PlotHumanSite>()?.Invalidate();

            if (Game.settings.collectMatsCollectionStatsTest)
                this.matStatsTracker.Save();
        }

        private void onJournalEntry(CodexEntry entry)
        {
            if (entry.Name == "$Codex_Ent_Guardian_Beacons_Name;")
            {
                // A Guardian Beacon
                Game.log($"Scanned Guardian Beacon in: {entry.System}");
                var data = GuardianBeaconData.Load(entry);

                // add the lat/long co-ordinates
                data.scannedLocations[entry.timestamp] = entry;
                data.lastVisited = entry.timestamp;
                data.Save();
            }
            else if (entry.SubCategory == "$Codex_SubCategory_Organic_Structures;" && entry.NearestDestination != "$Fixed_Event_Life_Cloud;" && entry.NearestDestination != "$Fixed_Event_Life_Ring;" && this.status?.hasLatLong == true)
            {
                var match = Game.codexRef.matchFromEntryId(entry.EntryID);

                if (Game.settings.autoTrackCompBioScans)
                {
                    // auto add CodexScans as a tracker location
                    if (match != null && this.systemBody?.organisms != null)
                    {
                        // wait a bit for the status file to update
                        Application.DoEvents();
                        //Game.log($"!! Comp scan organic: {entry.Name_Localised ?? entry.Name} ({entry.EntryID}) timestamps entry: {entry.timestamp} vs status: {this.status?.timestamp} | Locations: entry: {entry.Latitude}, {entry.Longitude} vs status: {this.status?.Latitude}, {this.status?.Longitude}");
                        var organism = systemBody.findOrganism(match);
                        if (organism?.analyzed == true && Game.settings.skipAnalyzedCompBioScans)
                        {
                            Game.log($"Already analyzed, NOT auto-adding tracker for: {entry.Name_Localised} ({entry.EntryID})");
                        }
                        else
                        {
                            this.addBookmark(match.genus.name, entry);
                            Program.showPlotter<PlotTrackers>()?.prepTrackers();
                            Game.log($"Auto-adding tracker from CodexEntry: {entry.Name_Localised} ({entry.EntryID})");
                            this.fireUpdate(true);
                        }
                    }
                    else
                    {
                        Game.log($"Organism '{entry.Name_Localised}' not found from: '{entry.Name_Localised}' ({entry.EntryID})");
                    }
                }

                // re-predict species, to clean-up any excess predictions (or the whole system if it's a new discovery
                if (entry.IsNewEntry)
                    Program.defer(() => this.predictSystemSpecies());
                else
                    this.deferPredictSpecies(this.systemBody);
            }

            Program.defer(() => Program.invalidateActivePlotters());
        }

        private void onJournalEntry(ScanOrganic entry)
        {
            if (this.systemBody == null) throw new Exception($"Why no this.systemBody?");

            // are we changing organism before the 3rd scan?
            var hash = $"{entry.SystemAddress}|{entry.Body}|{entry.Species}";
            if (this.cmdr.lastOrganicScan != null && hash != this.cmdr.lastOrganicScan)
            {
                if (this.systemBody.bioScans == null) this.systemBody.bioScans = new List<BioScan>();

                // yes - mark current scans as abandoned and start over
                if (this.cmdr.scanOne != null && cmdr.scanOne.body == systemBody.name)
                {
                    var genusMatch = Game.codexRef.matchFromGenus(cmdr.scanOne.genus)!;
                    this.addBookmark(genusMatch.name, cmdr.scanOne.location);
                }
                if (this.cmdr.scanTwo != null && cmdr.scanTwo.body == systemBody.name)
                {
                    var genusMatch = Game.codexRef.matchFromGenus(cmdr.scanTwo.genus)!;
                    this.addBookmark(genusMatch.name, cmdr.scanTwo.location);
                }

                this.cmdr.scanOne = null;
                this.cmdr.scanTwo = null;
            }
            this.cmdr.lastOrganicScan = hash;

            // add to bio scan locations. Skip for ScanType == ScanType.Analyse as a Sample event happens right before at the same location
            // add a new bio-scan - assuming we don't have one at this position already
            var match = Game.codexRef.matchFromVariant(entry.Variant);
            var bioScan = new BioScan
            {
                location = Status.here.clone(),
                genus = entry.Genus,
                species = entry.Species,
                radius = match.genus.dist,
                status = BioScan.Status.Active,
                entryId = match.entryId,
                body = systemBody.name,
            };

            Game.log($"new bio scan: {bioScan} ({entry.ScanType}) | current location: {Status.here} {this.systemBody.name}");

            if (entry.ScanType == ScanType.Log)
            {
                // replace 1st, clear 2nd
                this.cmdr.scanOne = bioScan;
                this.cmdr.scanTwo = null;
                this.cmdr.Save();

                // adjust predictions/rewards calculations for this body
                this.deferPredictSpecies(this.systemBody);
            }
            else if (this.cmdr.scanOne != null && this.cmdr.scanTwo == null)
            {
                // populate 2nd
                this.cmdr.scanTwo = bioScan;
                this.cmdr.Save();
            }
            else if (this.cmdr.scanOne == null && entry.ScanType == ScanType.Sample)
            {
                // we missed the first scan somehow?
                this.cmdr.scanOne = bioScan;
                this.cmdr.Save();

                // adjust predictions/rewards calculations for this body
                this.deferPredictSpecies(this.systemBody);
            }
            else if (entry.ScanType == ScanType.Analyse)
            {
                // the 3rd and final scan...
                if (this.systemData == null || this.systemBody == null) throw new Exception("Why no systemBody?");
                if (this.systemBody.bioScans == null) this.systemBody.bioScans = new List<BioScan>();

                // add a new BioScan for this 3rd and final entry
                bioScan.status = BioScan.Status.Complete;
                this.systemBody.bioScans.Add(bioScan);

                // and add the first two scans as completed
                if (this.cmdr.scanOne != null)
                {
                    // (this can happen if there are 2 copies of Elite running at the same time)
                    this.cmdr.scanOne!.status = BioScan.Status.Complete;
                    this.systemBody.bioScans.Add(this.cmdr.scanOne);
                    this.cmdr.scanTwo!.status = BioScan.Status.Complete;
                    this.systemBody.bioScans.Add(this.cmdr.scanTwo);
                }

                // and clear state
                this.cmdr.lastOrganicScan = null;
                this.cmdr.scanOne = null;
                this.cmdr.scanTwo = null;
                this.cmdr.Save();

                if (Game.settings.autoRemoveTrackerOnFinalSample)
                {
                    Game.log($"Auto removing trackers for: '{entry.Species_Localised}'/'{entry.Genus}'");
                    this.removeBookmarkName(entry.Genus);

                    // force a re-render
                    Program.getPlotter<PlotTrackers>()?.prepTrackers();
                }

                // adjust predictions/rewards calculations for this body
                this.deferPredictSpecies(this.systemBody);
            }

            FormPredictions.invalidate();
            FormShowCodex.update();

            // force a mode change to update ux
            fireUpdate(true);
        }

        private void onJournalEntry(SellOrganicData entry)
        {
            // match and remove sold items from running totals
            Game.log($"SellOrganicData: removing {entry.BioData.Count} scans, worth: {Util.credits(entry.BioData.Sum(_ => _.Value))} + bonus: {Util.credits(entry.BioData.Sum(_ => _.Bonus))} ({Util.credits(entry.BioData.Sum(_ => _.Value) + entry.BioData.Sum(_ => _.Bonus))})");
            Game.log($"SellOrganicData: pre-sale total: {this.cmdr.organicRewards} (from {cmdr.scannedBioEntryIds.Count} signals)\r\n{String.Join("\r\n", cmdr.scannedBioEntryIds)}");
            foreach (var data in entry.BioData)
            {
                try
                {
                    // find the species + confirm the reward matches
                    var speciesRef = Game.codexRef.matchFromSpecies(data.Species);
                    if (speciesRef.reward != data.Value) throw new Exception("Why do rewards not match?");

                    // find the entry with the entryId and reward value. Sadly, it's not possible to concretely tie it to the system or body.
                    var txtReward = data.Value.ToString();
                    var scannedEntryId = this.cmdr.scannedBioEntryIds.FirstOrDefault(_ => _.Contains(speciesRef.entryIdPrefix) && _.Contains(txtReward));
                    Game.log($"SellOrganicData: sell: '{data.Species_Localised}' ({data.Species}, prefix: {speciesRef.entryIdPrefix}, match: {scannedEntryId}) for {txtReward} Cr");
                    if (scannedEntryId != null)
                        cmdr.scannedBioEntryIds.Remove(scannedEntryId);
                    else
                    {
                        Game.log($"SellOrganicData: No scannedBioEntryIds? For: '{data.Species_Localised}' ({data.Species}, {scannedEntryId}) for {txtReward} Cr");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Game.log($"Error when selling: {data.Species_Localised} for {data.Value} Cr:\r\n{e}");
                    continue;
                }
            }

            cmdr.reCalcOrganicRewards();
            cmdr.Save();
            Game.log($"SellOrganicData: post-sale total: {this.cmdr.organicRewards} (from {cmdr.scannedBioEntryIds.Count} sigals)\r\n{String.Join("\r\n", cmdr.scannedBioEntryIds)}");
            // force a mode change to update ux
            fireUpdate(true);
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

        private void onJournalEntry(Interdicted entry)
        {
            dockTimer?.onJournalEntry(entry);
        }

        #endregion

        #region bookmarking

        public void toggleBookmark(string name, LatLong2 location)
        {
            if (this.systemData == null || this.systemBody == null) return;

            this.systemBody.bookmarks ??= new();
            var exists = systemBody.bookmarks.ContainsKey(name);
            var tooClose = exists && systemBody.bookmarks[name].Any(_ => Util.getDistance(_, location, this.systemBody.radius) < Game.settings.minimumKeyLocationTrackingDistance);

            Game.log($"Tracking location {name} at: {location}, exists: {exists}, tooClose: {tooClose}");

            // NO ~~remove if too close, replace if far away, otherwise add new.~~
            // remove if exists, otherwise add new
            /*if (tooClose)
            {
                removeBookmarkName(name);
            }
            else */
            if (exists)
            {
                removeBookmarkName(name);
                //Application.DoEvents();
                //addBookmark(name, location);
            }
            else
            {
                addBookmark(name, location);
            }
        }

        public void addBookmark(string name, LatLong2 location)
        {
            if (this.systemData == null || this.systemBody == null) return;

            // use full genus name if we are given a short name
            var genus = Game.codexRef.genus.Find(g => g.shortName == name);
            if (genus != null) name = genus.name;

            this.systemBody.bookmarks ??= new();
            if (!this.systemBody.bookmarks.ContainsKey(name)) this.systemBody.bookmarks[name] = new List<LatLong2>();

            var tooClose = this.systemBody.bookmarks[name].Any(_ => Util.getDistance(_, location, this.systemBody.radius) < 20);

            if (tooClose)
            {
                Game.log($"Too close to prior bookmark. Move 20m away");
                return;
            }

            Game.log($"Add bookmark '{name}' ({location}) on '{this.systemBody.name}' ({this.systemBody.id})");
            this.systemBody.bookmarks[name].Add(location);

            // TODO: limit to only 4?
            //Game.log($"Group '{name}' has too many entries. Ignoring location: {location}");
            this.systemData.Save();

            Program.showPlotter<PlotTrackers>()?.prepTrackers();
            Program.showPlotter<PlotMiniTrack>();
        }

        public void clearAllBookmarks()
        {
            if (this.systemData == null || this.systemBody == null) return;
            Game.log($"Clearing all bookmarks on '{this.systemBody.name}' ({this.systemBody.id}");

            this.systemBody.bookmarks = null;
            this.systemData.Save();

            Program.showPlotter<PlotTrackers>()?.prepTrackers();
            Program.invalidate<PlotMiniTrack>();
        }

        public void removeBookmarkName(string name)
        {
            if (name == null || this.systemData == null || this.systemBody?.bookmarks == null) return;
            Game.log($"Clearing all bookmarks for '{name}' on '{this.systemBody.name}' ({this.systemBody.id}");

            if (!this.systemBody.bookmarks.ContainsKey(name))
            {
                // use full genus name if we are given a short name
                var genus = Game.codexRef.genus.Find(g => g.shortName == name);
                if (genus != null) name = genus.name;

                // exit if still no match
                if (!this.systemBody.bookmarks.ContainsKey(name)) return;
            }

            this.systemBody.bookmarks.Remove(name);
            if (this.systemBody.bookmarks.Count == 0) this.systemBody.bookmarks = null;
            this.systemData.Save();

            Program.showPlotter<PlotTrackers>()?.prepTrackers();
            Program.invalidate<PlotMiniTrack>();
        }

        public void removeBookmark(string name, LatLong2 location, bool nearest)
        {
            if (this.systemData == null || this.systemBody == null) throw new Exception($"Why no systemData or systemBody?");
            if (this.systemBody.bookmarks == null || this.systemBody.bookmarks.Count == 0) return;

            if (!this.systemBody.bookmarks.ContainsKey(name))
            {
                // use full genus name if we are given a short name
                var genus = Game.codexRef.genus.Find(g => g.shortName == name);
                if (genus != null) name = genus.name;

                // exit if still no match
                if (!this.systemBody.bookmarks.ContainsKey(name)) return;
            }

            var list = this.systemBody.bookmarks[name]
                .Select(_ => new TrackingDelta(this.systemBody.radius, _, location))
                .ToList();
            list.Sort((a, b) => a.distance.CompareTo(b.distance));

            var victim = list[nearest ? 0 : list.Count - 1].Target;

            Game.log($"Removing nearest bookmark '{name}' to {location}, victim: {victim} on '{this.systemBody.name}' ({this.systemBody.id}");
            this.systemBody.bookmarks[name].Remove(victim);

            if (this.systemBody.bookmarks[name].Count == 0) this.systemBody.bookmarks.Remove(name);
            if (this.systemBody.bookmarks.Count == 0) this.systemBody.bookmarks = null;

            this.systemData.Save();

            Program.showPlotter<PlotTrackers>()?.prepTrackers();
        }

        #endregion

        #region screen stuff

        private bool onPlanet = false;

        private void onJournalEntry(Disembark entry)
        {
            log($"Disembark - entry.onPlanet: {entry.OnPlanet}");
            this.onPlanet = entry.OnPlanet;

            // remember where we left the SRV
            if (entry.SRV)
                this.srvLocation = Status.here.clone();
        }

        private void onJournalEntry(Embark entry)
        {
            log($"Embark - entry.onPlanet: {entry.OnPlanet}");
            this.onPlanet = false;

            this.systemData?.prepSettlements();

            // clear where we left the SRV
            if (entry.SRV)
                this.srvLocation = null;
        }

        private void onJournalEntry(Backpack entry)
        {
            log($"Backpack - this.onPlanet: {this.onPlanet}, firstFootfall: {this.systemBody?.firstFootFall}");
            if (this.onPlanet && this.systemBody?.firstFootFall == false)
            {
                Program.defer(() => this.inferFirstFootFall());
            }
        }

        public void inferFirstFootFall()
        {
            if (this.systemBody == null) return;
            if (this.systemSite != null)
            {
                Game.log($"inferFirstFootFall: skip when at Guardian sites");
                return;
            }

            var threshold = Game.settings.inferThreshold;

            var fps = 20;
            var duration = 15; // seconds
            var frames = duration * fps;
            Game.log($"Start counting - fps: {fps} for {duration} seconds ...");

            float maxBlueCount = 0;

            var tim = new System.Timers.Timer(1000f / fps);
            var count = 0;
            tim.Elapsed += (o, s) =>
            {
                if (count++ > frames)
                {
                    tim.Stop();
                    Game.log($"Stop counting - fps: {fps} for {duration} seconds. (Max observed: {maxBlueCount.ToString("N5")})");
                }

                var blueCount = this.getBlueCount();
                if (blueCount > threshold && this.systemBody != null)
                {
                    tim.Stop();
                    Game.log($"Frame #{count}: {blueCount} > {threshold}");
                    Game.log($"Setting first footfall on: '{systemBody.name}' ({systemBody.id})");
                    this.systemBody.firstFootFall = true;
                    this.systemData?.Save();
                    this.fireUpdate(true);
                }
                maxBlueCount = Math.Max(maxBlueCount, blueCount);
            };

            tim.Start();
        }

        public float getBlueCount()
        {
            var gameRect = Elite.getWindowRect();

            // don't bother if we have no rectangle to analyze
            if (gameRect.Width == 0 || gameRect.Height == 0) return 0;

            var cw = gameRect.Width / 8;
            var ch = gameRect.Height / 7;
            var watchRect = new Rectangle(
                gameRect.Left + (gameRect.Width / 2) - cw,
                gameRect.Top + (int)(gameRect.Height * 0.17f),
                cw * 2, ch);

            // var hits = new Dictionary<string, int>(); // dgb
            using (var b = new Bitmap(watchRect.Width, watchRect.Height))
            {
                using (var g = Graphics.FromImage(b))
                {
                    g.CopyFromScreen(watchRect.Left, watchRect.Top, 0, 0, b.Size);

                    var countBlue = 0;
                    for (var y = 0; y < b.Height; y++)
                    {
                        for (var x = 0; x < b.Width; x++)
                        {
                            var p = b.GetPixel(x, y);
                            if (Util.isCloseColor(p, Game.settings.inferColor, Game.settings.inferTolerance)) countBlue++;

                            /* if (p.G > 250 && p.B > 250) // dbg
                            {
                                var key = p.ToString();
                                if (!hits.ContainsKey(key)) hits[key] = 0;
                                hits[key] += 1;
                            } // */
                        }
                    }

                    var ratio = 1f / (watchRect.Width * watchRect.Height) * countBlue;

                    /* if (hits.Count > 0) // dbg
                    {
                        Game.log($"ratio: {ratio}, hits: {hits.Count}");
                        Game.log(string.Join(", ", hits.OrderBy(_ => _.Value).Reverse().Select(_ => $"{_.Key}: {_.Value}")));
                    } // */
                    return ratio;
                }
            }
        }

        #endregion
    }

    class ShipData
    {
        public int id;
        public string name;
        public string ident;
        public string type;
        public double maxJump;
        public int cargoCapacity;

        public ShipData()
        {
            this.id = -1; ;
            this.name = "?";
            this.ident = "?";
            this.type = "?";
            this.maxJump = -1;
        }

        public ShipData(Loadout entry)
        {
            this.id = entry.ShipID;
            this.name = entry.Ship;
            this.name = string.IsNullOrWhiteSpace(entry.ShipName) ? entry.Ship : entry.ShipName;
            this.ident = entry.ShipIDent;
            this.type = entry.Ship;
            this.maxJump = entry.MaxJumpRange;
            this.cargoCapacity = entry.CargoCapacity;
        }

        public ShipData(LoadGame entry)
        {
            this.id = entry.ShipID;
            this.name = (string.IsNullOrWhiteSpace(entry.ShipName) ? entry.Ship_Localised : entry.ShipName) ?? "?";
            this.ident = entry.ShipIdent;
            this.type = entry.Ship;
            this.maxJump = -1;
        }
    }
}
