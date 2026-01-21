using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.units;

namespace SrvSurvey
{
    delegate void OnStatusChange();

    public class Status : ILocation, IDisposable
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
        public double Temperature { get; set; }

        public string BodyName { get; set; }
        public long Balance { get; set; }
        public Destination Destination { get; set; }
        public decimal PlanetRadius { get; set; }
        public string SelectedWeapon { get; set; }
        public string SelectedWeapon_Localised { get; set; }

        #endregion

        #region deserializing + file watching

        public event StatusFileChanged StatusChanged;

        /// <summary> The names of properties that changed when last reading the file </summary>
        public HashSet<string> changed = new();

        private FileSystemWatcher? fileWatcher;
        //private bool haveRead;
        //private bool creditsMatched;
        //[JsonIgnore]
        //public long knownCredits = -1;

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
                PlotPulse.resetPulse();
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

            try
            {
                var oldFlags = this.Flags;
                var oldFlags2 = this.Flags2;

                // read the file contents ...
                using (var sr = new StreamReader(new FileStream(Status.Filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    var json = sr.ReadToEnd();
                    if (json == null || json == "") return;

                    // ... parse into tmp object ...
                    var obj = JsonConvert.DeserializeObject<Status>(json);

                    /* TODO: This is not ready ... need a way to safely initialize the credit balance
                    // to help multi-boxing ... ignore changes if credits AND GuiFocus or fire-group change at the same time
                    if (haveRead)
                    {
                        if (!creditsMatched)
                            creditsMatched = knownCredits == obj?.Balance;
                        if (creditsMatched)
                        {
                            var fireGroupChanged = obj?.FireGroup != this.FireGroup;
                            //var fuelChanged = obj?.Fuel?.FuelMain != this.Fuel?.FuelMain || obj?.Fuel?.FuelReservoir != this.Fuel?.FuelReservoir;
                            var balanceChanged = obj?.Balance != this.Balance;
                            var guiFocusChanged = obj?.GuiFocus != this.GuiFocus;
                            if (balanceChanged && (fireGroupChanged || guiFocusChanged))
                            {
                                Game.log($"Ignoring status.json change: {knownCredits}=={this.Balance} || {obj?.FireGroup} != {this.FireGroup} || {obj?.Balance} != {this.Balance} || {obj?.GuiFocus} vs {this.GuiFocus}");
                                return;
                            }

                            // ok to proceed
                            Game.log($"Accept Status.json change: {knownCredits}=={this.Balance} || {obj?.FireGroup} vs {this.FireGroup} || {obj?.Balance} vs {this.Balance} || {obj?.GuiFocus} vs {this.GuiFocus}");
                            knownCredits = obj!.Balance;
                        }
                    }
                    //*/

                    this.changed.Clear();

                    // ... assign all property values from tmp object
                    var allProps = typeof(Status).GetProperties(Program.InstanceProps);
                    foreach (var prop in allProps)
                    {
                        if (prop.CanWrite)
                        {
                            var currentValue = prop.GetValue(this);
                            var newValue = prop.GetValue(obj);
                            var didChange = this.getChanged(prop.Name, currentValue, newValue);
                            if (didChange)
                                changed.Add(prop.Name);

                            prop.SetValue(this, newValue);
                        }
                    }
                }

                // call out which individual flags have changed
                if (this.Flags != oldFlags)
                {
                    foreach (var f in Enum.GetValues<StatusFlags>())
                    {
                        var newValue = (this.Flags & f) > 0;
                        var oldValue = (oldFlags & f) > 0;
                        if (newValue != oldValue)
                            changed.Add(f.ToString());
                    }
                }
                if (this.Flags2 != oldFlags2)
                {
                    foreach (var f in Enum.GetValues<StatusFlags2>())
                    {
                        var newValue = (this.Flags2 & f) > 0;
                        var oldValue = (oldFlags2 & f) > 0;
                        if (newValue != oldValue)
                            changed.Add(f.ToString());
                    }
                }
                //Game.log("Status properties changed:" + string.Join(", ", changed));

                // update singleton location
                Status.here.Lat = this.Latitude;
                Status.here.Long = this.Longitude;

                // work-around negative headings that sometimes occur when on-foot
                if (this.Heading < 0)
                    this.Heading += 360;

                var blink = this.trackBlinks();

                // fire the event for external code on the UI thread
                if (this.StatusChanged != null)
                {
                    Program.control.Invoke(() =>
                    {
                        if (this.StatusChanged != null)
                            this.StatusChanged(blink);
                    });
                }

                //Game.log($"parseStatusFile:\n\t{Flags}\n\t{Flags2}");
                //haveRead = true;
            }
            catch (Exception ex)
            {
                Game.log($"parseStatusFile: {ex}");
            }
        }

        private bool getChanged(string propName, dynamic? currentValue, dynamic? newValue)
        {
            var nullChanged = (currentValue == null) != (newValue == null);
            if (nullChanged)
                return true;

            // some special cases for compounds objects
            if (propName == nameof(Pips))
                return Pips?[0] != newValue?[0]
                    || Pips?[1] != newValue?[1]
                    || Pips?[2] != newValue?[2];

            if (propName == nameof(Fuel))
                return Fuel?.FuelMain != newValue?.FuelMain
                    || Fuel?.FuelReservoir != newValue?.FuelReservoir;

            if (propName == nameof(Destination))
                return Destination?.System != newValue?.System
                    || Destination?.Body != newValue?.Body
                    || Destination?.Name != newValue?.Name
                    || Destination?.Name_Localised != newValue?.Name_Localised;

            // anything else will work with this
            return currentValue?.Equals(newValue) == false;
        }

        #endregion

        private bool trackBlinks()
        {
            var blinkSignal = Game.settings.blinkTigger; // StatusFlags.HudInAnalysisMode; // CargoScoopDeployed; // StatusFlags.LightsOn;

            // when on foot, use shield toggling instead
            if ((this.Flags2 & StatusFlags2.OnFootExterior) > 0)
                blinkSignal = StatusFlags.ShieldsUp;

            var newBlinkState = (this.Flags & blinkSignal) > 0;
            var duration = DateTime.Now - this.lastBlinkChange;

            //Game.log($"newBlinkState: {newBlinkState}, this.blinkState: {this.blinkState}, this.lastblinkChange: {this.lastBlinkChange}, duration: {DateTime.Now - this.lastBlinkChange}");
            if (newBlinkState != this.blinkState && !changed.Contains(nameof(Status.@event)))
            {
                this.blinkState = newBlinkState;
                this.lastBlinkChange = DateTime.Now;

                if (duration.TotalMilliseconds < Game.settings.blinkDuration)
                {
                    changed.Add("blink");
                    Game.log($"Blink detected blinked!");
                    return true;
                }
            }

            return false;
        }

        private bool blinkState = false;
        public DateTime lastBlinkChange = DateTime.Now;

        [JsonIgnore]
        public static readonly LatLong2 here = new LatLong2(0d, 0d);

        [JsonIgnore]
        public bool OnFoot { get => (this.Flags2 & StatusFlags2.OnFoot) > 0; }
        [JsonIgnore]
        public bool OnFootOnPlanet { get => (this.Flags2 & StatusFlags2.OnFootOnPlanet) > 0; }
        [JsonIgnore]
        public bool OnFootInside { get => (this.Flags2 & StatusFlags2.OnFoot) > 0 && (this.Flags2 & StatusFlags2.BreathableAtmosphere) > 0; }
        [JsonIgnore]
        public bool OnFootExterior { get => (this.Flags2 & StatusFlags2.OnFootExterior) > 0; }
        [JsonIgnore]
        public bool OnFootSocial { get => (this.Flags2 & StatusFlags2.OnFoot) > 0 && (this.Flags2 & StatusFlags2.OnFootSocialSpace) > 0; }
        [JsonIgnore]
        public bool OnFootInStation { get => (this.Flags2 & (StatusFlags2.OnFootInHangar | StatusFlags2.OnFootInStation | StatusFlags2.OnFootSocialSpace)) > 0; }
        [JsonIgnore]
        public bool InSrv { get => (this.Flags & StatusFlags.InSRV) > 0; }
        [JsonIgnore]
        public bool InFighter { get => (this.Flags & StatusFlags.InFighter) > 0; }
        [JsonIgnore]
        public bool InMainShip { get => (this.Flags & StatusFlags.InMainShip) > 0; }
        [JsonIgnore]
        public bool hasLatLong { get => (this.Flags & StatusFlags.HasLatLong) > 0; }
        [JsonIgnore]
        public bool UsingSrvTurret { get => (this.Flags & StatusFlags.SrvUsingTurretView) > 0; }
        [JsonIgnore]
        public bool InTaxi { get => (this.Flags2 & StatusFlags2.InTaxi) > 0; }
        [JsonIgnore]
        public bool Docked { get => (this.Flags & StatusFlags.Docked) > 0; }
        [JsonIgnore]
        public bool ShieldsUp { get => (this.Flags & StatusFlags.ShieldsUp) > 0; }
        [JsonIgnore]
        public bool FsdCharging { get => (this.Flags & StatusFlags.FsdCharging) > 0; }
        [JsonIgnore]
        public bool FsdChargingJump { get => (this.Flags2 & StatusFlags2.FsdChargingJump) > 0; }
        [JsonIgnore]
        public bool GlideMode { get => (this.Flags2 & StatusFlags2.GlideMode) > 0; }
        [JsonIgnore]
        public bool hudInAnalysisMode { get => (this.Flags & StatusFlags.HudInAnalysisMode) > 0; }

        [JsonIgnore]
        public bool landingGearDown { get => (this.Flags & StatusFlags.LandingGearDown) > 0; }
        [JsonIgnore]
        public bool cargoScoopDeployed { get => (this.Flags & StatusFlags.CargoScoopDeployed) > 0; }
        [JsonIgnore]
        public bool lightsOn { get => (this.Flags & StatusFlags.LightsOn) > 0; }

        public bool isWithin(double lat, double @long, double targetDist)
        {
            if (PlanetRadius == 0) return false;

            var actualDist = (double)Util.getDistance(new LatLong2(lat, @long), here, PlanetRadius);
            return actualDist <= targetDist;
        }
    }

    #region Types

    public class FuelStatus
    {
        public float FuelMain { get; set; }
        public float FuelReservoir { get; set; }
    }

    public class PipsStatus : List<int>
    {
        public int sys { get => this[0]; }
        public int eng { get => this[1]; }
        public int wep { get => this[2]; }
    }

    public class Destination
    {
        public long System { get; set; }
        public int Body { get; set; }
        public string Name { get; set; }
        public string Name_Localised { get; set; }

        public override string ToString()
        {
            return $"{Name} ({System})";
        }
    }

    [Flags]
    public enum StatusFlags
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
    public enum StatusFlags2
    {
        // See https://elite-journal.readthedocs.io/en/latest/Status%20File/#status-file
        OnFoot = 0x_0000_0001,
        InTaxi = 0x_0000_0002,   //(or dropship/shuttle)
        InMulticrew = 0x_0000_0004,
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
        FsdChargingJump = 0x_0008_0000,
        SCOverdrive= 0x_0010_0000,
        SCAssist = 0x_0020_0000,
        Unknown = 0x_0040_0000,
    }

    public enum GuiFocus
    {
        NoFocus = 0, // meaning ... playing the game
        InternalPanel, // (right hand side)
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

    #endregion
}
