using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SrvSurvey
{

    delegate void OnStatusChange();

    class Status : ILocation, IDisposable
    {

        public static readonly string Filename = "Status.json";
        public static string Filepath { get => Path.Combine(Game.settings.watchedJournalFolder, Status.Filename); }

        #region properties from file

        public DateTime timestamp { get; set; }
        public string @event { get; set; }
        public StatusFlags Flags { get; set; }
        public StatusFlags2 Flags2 { get; set; }
        public PipsStatus Pips { get; set; }
        public int FireGroup { get; set; }
        public GuiFocus GuiFocus { get; set; }
        public FuelStatus Fuel { get; set; }
        public float Cargo { get; set; }
        public string LegalState { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Heading { get; set; }
        public int Altitude { get; set; }

        public string BodyName { get; set; }
        public Destination Destination { get; set; }
        public decimal PlanetRadius { get; set; }
        public string SelectedWeapon { get; set; }
        public string SelectedWeapon_Localised { get; set; }

        #endregion

        #region deserializing + file watching

        public event StatusFileChanged StatusChanged;

        private FileSystemWatcher? fileWatcher;

        public Status(bool watch)
        {
            if (watch)
            {
                // start watching the status file
                this.fileWatcher = new FileSystemWatcher(Game.settings.watchedJournalFolder, Status.Filename);
                this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                this.fileWatcher.Changed += fileWatcher_Changed;
                this.fileWatcher.EnableRaisingEvents = true;

                this.parseStatusFile();
            }
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
                if (this.fileWatcher != null)
                {
                    this.fileWatcher.Changed -= fileWatcher_Changed;
                    this.fileWatcher = null;
                }
            }
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Program.crashGuard(() =>
            {
                PlotPulse.LastChanged = DateTime.Now;
                this.parseStatusFile();
            });
        }

        private void parseStatusFile()
        {
            if (!File.Exists(Status.Filepath))
            {
                Game.log($"Check watched journal folder setting!\r\nCannot find file: {Status.Filepath}");
                return;
            }

            // read the file contents ...
            using (var sr = new StreamReader(new FileStream(Status.Filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                try
                {
                    var json = sr.ReadToEnd();
                    if (json == null || json == "") return;

                    // ... parse into tmp object ...
                    var obj = JsonConvert.DeserializeObject<Status>(json);

                    // ... assign all property values from tmp object 
                    var allProps = typeof(Status).GetProperties(Program.InstanceProps);
                    foreach (var prop in allProps)
                    {
                        if (prop.CanWrite)
                        {
                            prop.SetValue(this, prop.GetValue(obj));
                        }
                    }

                    // update singleton location
                    Status.here.Lat = this.Latitude;
                    Status.here.Long = this.Longitude;

                    var blink = this.trackBlinks();

                    // fire the event for external code on the UI thread
                    Program.control!.Invoke((MethodInvoker)delegate
                    {
                        if (this.StatusChanged != null)
                        {
                            this.StatusChanged(blink);
                        }
                    });
                }
                catch (Exception)
                {
                    // ignore any errors
                }
            }
        }

        #endregion

        private bool trackBlinks()
        {
            var blinkSignal = Game.settings.blinkTigger; // StatusFlags.HudInAnalysisMode; // CargoScoopDeployed; // StatusFlags.LightsOn;

            // when on foot, use shield toggling instead
            if ((this.Flags2 & StatusFlags2.OnFootExterior) > 0)
                blinkSignal = StatusFlags.ShieldsUp;

            var newBlinkState = (this.Flags & blinkSignal) > 0;
            var duration = DateTime.Now - this.lastblinkChange;

            //Game.log($"newBlinkState: {newBlinkState}, this.blinkState: {this.blinkState}, this.lastblinkChange: {this.lastblinkChange}, duration: {DateTime.Now - this.lastblinkChange}");
            if (newBlinkState != this.blinkState)
            {
                this.blinkState = newBlinkState;
                this.lastblinkChange = DateTime.Now;

                if (duration.TotalMilliseconds < Game.settings.blinkDuration)
                {
                    Game.log($"Blink detected blinked!");
                    return true;
                }
            }

            return false;
        }

        private bool blinkState = false;
        public DateTime lastblinkChange = DateTime.Now;

        [JsonIgnore]
        public static readonly LatLong2 here = new LatLong2(0, 0);

        [JsonIgnore]
        public bool OnFoot { get => (this.Flags2 & StatusFlags2.OnFoot) > 0; }
        [JsonIgnore]
        public bool InSrv { get => (this.Flags & StatusFlags.InSRV) > 0; }
        [JsonIgnore]
        public bool InFighter { get => (this.Flags & StatusFlags.InFighter) > 0; }
        [JsonIgnore]
        public bool InMainShip { get => (this.Flags & StatusFlags.InMainShip) > 0; }
        [JsonIgnore]
        public bool hasLatLong { get => (this.Flags & StatusFlags.HasLatLong) > 0; }
        [JsonIgnore]
        public bool InTaxi { get => (this.Flags2 & StatusFlags2.InTaxi) > 0; }
    }

    class FuelStatus
    {
        public float FuelMain { get; set; }
        public float FuelReservoir { get; set; }
    }

    class PipsStatus : List<int>
    {
        public int sys { get => this[0]; }
        public int eng { get => this[1]; }
        public int wep { get => this[2]; }
    }

    class Destination
    {
        public long System { get; set; }
        public int Body { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }

    }


    [Flags]
    enum StatusFlags
    {
        // See https://elite-journal.readthedocs.io/en/latest/Status%20File/#status-file
        Docked = 0x_0000_0001, // (on a landing pad)
        Landed = 0x_0000_0002,  // (on planet surface)
        LandingGearDown = 0x_0000_0004,
        ShieldsUp = 0x_0000_0008,
        Supercruise = 0x_0000_0010,
        FlightAssistOff = 0x_0000_0020,
        HardpointsDeployed = 0x_0000_0040,
        InWing = 0x_0000_0080,
        LightsOn = 0x_0000_0100,
        CargoScoopDeployed = 0x_0000_0200,
        SilentRunning = 0x_0000_0400,
        ScoopingFuel = 0x_0000_0800,
        SrvHandbrake = 0x_0000_1000,
        SrvUsingTurretView = 0x_0000_2000,
        SrvTurretRetracted = 0x_0000_4000, //(close to ship)
        SrvDriveAssist = 0x_0000_8000,
        FsdMassLocked = 0x_0001_0000,
        FsdCharging = 0x_0002_0000,
        FsdCooldown = 0x_0004_0000,
        LowFuel = 0x_0008_0000, //( < 25% )
        OverHeating = 0x_0010_0000, //( > 100% )
        HasLatLong = 0x_0020_0000,
        IsInDanger = 0x_0040_0000,
        BeingInterdicted = 0x_0080_0000,
        InMainShip = 0x_0100_0000,
        InFighter = 0x_0200_0000,
        InSRV = 0x_0400_0000,
        HudInAnalysisMode = 0x0800_0000,
        NightVision = 0x_1000_0000,
        AltitudeFromAverageRadius = 0x_2000_0000,
        fsdJump = 0x_4000_0000,
        srvHighBeam = unchecked((int)0x_8000_0000),
    }

    [Flags]
    enum StatusFlags2
    {
        // See https://elite-journal.readthedocs.io/en/latest/Status%20File/#status-file
        OnFoot = 0x_0000_0001,
        InTaxi = 0x_0000_0002,   //(or dropship/shuttle)
        InMulticrew = 0x_0000_0004, //(ie in someone else's ship)
        OnFootInStation = 0x_0000_0008,
        OnFootOnPlanet = 0x_0000_0010,
        AimDownSight = 0x_0000_0020,
        LowOxygen = 0x_0000_0040,
        LowHealth = 0x_0000_0080,
        Cold = 0x_0000_0100,
        Hot = 0x_0000_0200,
        VeryCold = 0x_0000_0400,
        VeryHot = 0x_0000_0800,
        GlideMode = 0x_0000_1000,
        OnFootInHangar = 0x_0000_2000,
        OnFootSocialSpace = 0x_0000_4000,
        OnFootExterior = 0x_0000_8000,
        BreathableAtmosphere = 0x_0001_0000,
        TelepresenceMulticrew = 0x_0002_0000,
        PhysicalMulticrew = 0x_0004_0000,
    }

    enum GuiFocus
    {
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
    }

}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
