using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    /// <summary>
    /// Represents live state of the game Elite Dangerous
    /// </summary>
    class Game
    {
        /// <summary>
        /// The Commander actively playing the game
        /// </summary>
        public string Commander { get; private set; }
        public StringBuilder logs = new StringBuilder();

        private JournalWatcher journals;
        public Status status { get; private set; }

        public Game(string cmdr)
        {
            // track this instance as the active one
            Game.activeGame = this;

            log($"Game is running", this.isRunning);

            // initialize from a journal file
            var filepath = this.getLastJournal(cmdr);
            this.initializeFromJournal(filepath);

            // track status file changes and force an immediate read
            this.status = new Status(true);
            this.status.StatusChanged += Status_StatusChanged;
            Status_StatusChanged();
        }

        private void Status_StatusChanged()
        {
            log("status changed");

            // update the easy things
            this.location = new LatLong2(this.status);

            // check various things we actively need to know have changed
            if (this._mode != this.mode)
            {
                log($"mode old: {this._mode}, new: {this.mode}");
                this._mode = this.mode;

                if (modeChanged != null) modeChanged(this._mode);
                // fire event!
            }

            // are we near a body?
            if ((this.status.Flags & StatusFlags.HasLatLong) > 0)
            {
                if (this.nearBody == null)
                    this.nearBody = new LandableBody()
                    {
                        name = status.BodyName,
                        radius = status.PlanetRadius,
                    };
            }
            else if (this.nearBody != null)
            {
                // remove old one we are far away from
                this.nearBody = null;
            }


        }

        public event GameModeChanged modeChanged;
        private GameMode _mode;
        public GameMode mode
        {
            get
            {
                if (!this.isRunning)
                    return GameMode.Offline;
                
                // use GuiFocus if it is interesting
                if (this.status.GuiFocus != GuiFocus.NoFocus)
                    return (GameMode)(int)this.status.GuiFocus;

                // otherwise use the type of vehicle we are in
                if (this.vehicle == ActiveVehicle.Fighter)
                    return GameMode.InFighter;
                if (this.vehicle == ActiveVehicle.SRV)
                    return GameMode.InSrv;
                if (this.vehicle == ActiveVehicle.Taxi)
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

                // if all else fails, we must simply be...
                return GameMode.Flying;
            }
        }

        public ActiveVehicle vehicle
        {
            get
            {
                if ((this.status.Flags & StatusFlags.InMainShip) > 0)
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
                    return ActiveVehicle.Docked;

                // TODO: do we care about telepresence?
            }
        }

        public LandableBody nearBody;

        public LatLong2 location { get; private set; }

        #region logging

        public static Game activeGame { get; private set; }

        public static void log(object msg, object o1 = null)
        {
            var txt = DateTime.Now.ToString("HH:mm:ss") + ": " + msg.ToString();

            if (o1 != null)
            {
                txt += ": " + o1.ToString();
            }

            Game.activeGame.logs.AppendLine(txt);
            Debug.WriteLine(txt);
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
                return procED.Length > 0 && this.Commander != null && !this.atMainMenu;
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

        #region journal reading stuff

        public static string journalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous\");

        private string getLastJournal(string cmdr)
        {
            var journalFiles = new DirectoryInfo(SrvSurvey.journalFolder)
                .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(_ => _.LastWriteTimeUtc)
                .Select(_ => _.FullName);

            if (cmdr == null)
            {
                // use the most recent journal file
                return journalFiles.First();
            }

            var CMDR = cmdr.ToUpper();
            var filename = journalFiles.First((filepath) =>
            {
                // TODO: Use some streaming reader to save reading the whole file up front?
                using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.Contains("\"event\":\"Commander\""))
                            // no need to process further lines
                            return line.ToUpper().Contains($"\"NAME\":\"{CMDR}\"");
                    }
                    return false;
                }
            });

            // TODO: As we already loaded the journal into memory, it would be nice to use that rather than reload it again from JournalWatcher

            return filename;
        }

        private void initializeFromJournal(string filepath)
        {
            this.journals = new JournalWatcher(filepath, true);
            var cmdrEntry = this.journals.FindEntryByType<Commander>(0, false);

            this.journals.onJournalEntry += Journals_onJournalEntry;

            // exit early if this journal file has no Commander entry yet
            if (cmdrEntry == null) return;

            onJournalEntry(cmdrEntry, journals.Entries.IndexOf(cmdrEntry));
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry, index);
        }

        private void onJournalEntry(JournalEntry entry, int index)
        {
            // ignore
        }

        private void onJournalEntry(Commander entry, int index)
        {
            this.Commander = entry.Name;

            log($"Initialized Commander", this.Commander);

        }

        private bool atMainMenu = false;

        private void onJournalEntry(Music entry, int index)
        {
            Game.log(">>"+ entry.MusicTrack);

            var newMainMenu = entry.MusicTrack == "MainMenu";
            if (this.atMainMenu != newMainMenu)
            {
                // if atMainMenu has changed - force a status change event
                this.atMainMenu = newMainMenu;
                this.Status_StatusChanged();
            }
        }

        #endregion
    }
}
