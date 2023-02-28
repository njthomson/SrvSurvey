using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    delegate void OnStatusChange();

    class Status : ILocation
    {

        public static readonly string Filename = "Status.json";
        public static string Filepath { get => Path.Combine(SrvSurvey.journalFolder, Status.Filename); }

        #region properties from file
        // members from file

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
        public int Altitude{ get; set; }

        public string BodyName { get; set; }
        public Destination Destination { get; set; }
        public double PlanetRadius { get; set; }

        #endregion

        #region deserializing + file watching

        public event OnSurveyChange StatusChanged;

        private FileSystemWatcher fileWatcher;

        public Status(bool watch)
        {
            if (watch)
            {
                // start watching the status file
                this.fileWatcher = new FileSystemWatcher(SrvSurvey.journalFolder, Status.Filename);
                this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                this.fileWatcher.Changed += fileWatcher_Changed;
                this.fileWatcher.EnableRaisingEvents = true;

                this.parseStatusFile();
            }
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            PlotPulse.LastChanged = DateTime.Now;
            this.parseStatusFile();
        }

        private void parseStatusFile()
        {
            // read the file contents ...
            using (var sr = new StreamReader(new FileStream(Status.Filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
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
            }

            // fire the event for external code on the UI thread
            if (this.StatusChanged != null)
            {
                Program.control.Invoke((MethodInvoker)delegate
                {
                    this.StatusChanged();
                });
            }
        }

        #endregion

        [JsonIgnore]
        public bool OnFoot { get => (this.Flags2 & StatusFlags2.OnFoot) > 0; }
        [JsonIgnore]
        public bool InSrv{ get => (this.Flags & StatusFlags.InSRV) > 0; }
        [JsonIgnore]
        public bool InFighter { get => (this.Flags & StatusFlags.InFighter) > 0; }
        [JsonIgnore]
        public bool InMainShip { get => (this.Flags & StatusFlags.InMainShip) > 0; }
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
