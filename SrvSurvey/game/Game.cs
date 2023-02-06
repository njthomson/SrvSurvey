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
        public static Game activeGame { get; private set; }
        public static Settings settings { get; private set; } = new Settings();

        /// <summary>
        /// The Commander actively playing the game
        /// </summary>
        public string Commander { get; private set; }
        public bool odyssey { get; private set; }
        public StringBuilder logs = new StringBuilder();

        private JournalWatcher journals;
        public Status status { get; private set; }

        public Game(string cmdr)
        {
            // track this instance as the active one
            Game.activeGame = this;

            log($"Game is running: {this.isRunning}, cmdr loaded: ${this.Commander != null}");

            // track status file changes and force an immediate read
            this.status = new Status(true);

            // initialize from a journal file
            var filepath = this.getLastJournalBefore(cmdr, DateTime.MaxValue);
            this.initializeFromJournal(filepath);

            // now listen for changes
            this.journals.onJournalEntry += Journals_onJournalEntry;
            this.status.StatusChanged += Status_StatusChanged;

            Status_StatusChanged();
        }

        #region modes

        private LatLong2 touchdownLocation;
        public LatLong2 location { get; private set; }
        private bool atMainMenu = false;
        private bool fsdEngaged = false;
        private bool fsdJumping = false;
        public LandableBody nearBody;

        private void checkModeChange()
        {
            // check various things we actively need to know have changed
            if (this._mode != this.mode)
            {
                log($"mode old: {this._mode}, new: {this.mode}");
                this._mode = this.mode;

                if (modeChanged != null) modeChanged(this._mode);
                // fire event!
            }

        }

        private void Status_StatusChanged()
        {
            log("status changed");

            // update the easy things
            this.location = new LatLong2(this.status);

            this.checkModeChange();

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

                if (this.atMainMenu || this.Commander == null)
                    return GameMode.MainMenu;

                // use GuiFocus if it is interesting
                if (this.status.GuiFocus != GuiFocus.NoFocus)
                    return (GameMode)(int)this.status.GuiFocus;

                if (this.fsdJumping)
                    return GameMode.FSDJumping;

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

        #endregion

        #region logging

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

        public static string journalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous\");

        private string getLastJournalBefore(string cmdr, DateTime timestamp)
        {
            var manyFiles = new DirectoryInfo(SrvSurvey.journalFolder)
                .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderByDescending(_ => _.LastWriteTimeUtc);

            var journalFiles = manyFiles
                .Where(_ => _.LastWriteTime < timestamp)
                .Select(_ => _.FullName);

            if (cmdr == null)
            {
                // use the most recent journal file
                return journalFiles.First();
            }

            var CMDR = cmdr.ToUpper();
            //var filename = journalFiles.First((filepath) => isJournalForCmdr(cmdr, filepath));

            var filename = journalFiles.FirstOrDefault((filepath) =>
            {
                // TODO: Use some streaming reader to save reading the whole file up front?
                using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        // TODO: allow for non-Odyssey
                        if (line.Contains("\"event\":\"Fileheader\"") && line.Contains("\"Odyssey\":false"))
                            return false;

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

        //private bool isJournalForCmdr(string cmdr, string filepath)
        //{
        //    var CMDR = cmdr.ToUpper();
        //    using (var reader = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        //    {
        //        while (!reader.EndOfStream)
        //        {
        //            var line = reader.ReadLine();
        //            if (line.Contains("\"event\":\"Commander\""))
        //                // no need to process further lines
        //                return line.ToUpper().Contains($"\"NAME\":\"{CMDR}\"");
        //        }
        //        return false;
        //    }
        //}

        //private string getJournalBefore(string cmdr, DateTime timestamp)
        //{
        //    var journalFiles = new DirectoryInfo(SrvSurvey.journalFolder)
        //        .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
        //        .OrderByDescending(_ => _.LastWriteTimeUtc)
        //        .Where(_ => _.LastWriteTime < timestamp)
        //        .Select(_ => _.FullName);

        //    return "";

        //}

        private void initializeFromJournal(string filepath)
        {
            this.journals = new JournalWatcher(filepath, true);
            var cmdrEntry = this.journals.FindEntryByType<Commander>(0, false);

            // exit early if this journal file has no Commander entry yet
            if (cmdrEntry == null) return;

            // read cmdr info
            this.Commander = cmdrEntry.Name;
            //onJournalEntry(cmdrEntry);

            // if we are not shutdown ...
            var lastShutdown = journals.FindEntryByType<Shutdown>(-1, true);
            if (lastShutdown == null)
            {

                // ... and we have MainMenu music - we know we're not actively playing
                var lastMusic = journals.FindEntryByType<Music>(-1, true);
                if (lastMusic != null)
                    onJournalEntry(lastMusic);
            }

            // if we are landed, we need to find the last Touchdown location
            if ((this.status.Flags & StatusFlags.Landed) > 0)
            {
                var lastTouchdown = this.findLastJournalOf<Touchdown>();
                if (lastTouchdown == null)
                    throw new Exception($"ERROR! Journal parsing failure! Failed to find any last Touchdown events.");

                if (lastTouchdown.Body != status.BodyName)
                {
                    throw new Exception($"ERROR! Journal parsing failure! Last found Touchdown was for body: {lastTouchdown.Body}, but we are currently on: {status.BodyName}");
                }

                onJournalEntry(lastTouchdown);
            }

            log($"Initialized Commander", this.Commander);

        }

        //private Touchdown findLastTouchdown()
        //{
        //    log($"Finding last Touchdown event... {journals.Count}");
        //    // do we have one recently in the active journal?
        //    var lastTouchdown = journals.FindEntryByType<Touchdown>(-1, true);

        //    if (lastTouchdown != null)
        //    {
        //        // yes, phew
        //        return lastTouchdown;
        //    }

        //    // otherwise, we need to dig into prior journal files
        //    do
        //    {
        //        var priorFilepath = this.getLastJournalBefore(this.Commander, journals.timestamp);
        //        if (priorFilepath == null) return null;
        //        var oldJournal = new JournalWatcher(priorFilepath, false);

        //        lastTouchdown = oldJournal.FindEntryByType<Touchdown>(-1, true);
        //    } while (lastTouchdown == null);

        //    if (lastTouchdown.Body != status.BodyName)
        //    {
        //        throw new Exception($"ERROR! Journal parsing failure! Last found Touchdown was for body: {lastTouchdown.Body}, but we are currently on: {status.BodyName}");
        //    }

        //    return lastTouchdown;
        //}

        private T findLastJournalOf<T>() where T : JournalEntry
        {
            log($"Finding last Touchdown event... {journals.Count}");
            // do we have one recently in the active journal?
            var lastEntry = journals.FindEntryByType<T>(-1, true);

            if (lastEntry != null)
            {
                // yes, phew
                return lastEntry;
            }

            // otherwise, we need to dig into prior journal files
            var timestamp = journals.timestamp;
            do
            {
                var priorFilepath = this.getLastJournalBefore(this.Commander, timestamp);
                if (priorFilepath == null) return null;
                var oldJournal = new JournalWatcher(priorFilepath, false);

                lastEntry = oldJournal.FindEntryByType<T>(-1, true);
                timestamp = oldJournal.timestamp;
            } while (lastEntry == null);

            return lastEntry;
        }


        #endregion

        #region journal tracking for game state and modes


        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            Game.log($"!--> {entry.@event}");
            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry)
        {
            // ignore
        }

        private void onJournalEntry(Commander entry)
        {
            if (this.Commander == null)
            {
                // this isn't right - needs reworking
                initializeFromJournal(this.journals.filepath);
            }
        }


        private void onJournalEntry(Music entry)
        {
            Game.log(">>" + entry.MusicTrack);

            var newMainMenu = entry.MusicTrack == "MainMenu";
            if (this.atMainMenu != newMainMenu)
            {
                // if atMainMenu has changed - force a status change event
                this.atMainMenu = newMainMenu;
                this.Status_StatusChanged();
            }
        }

        private void onJournalEntry(Shutdown entry)
        {
            this.atMainMenu = false;
            this.Status_StatusChanged();
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
            this.checkModeChange();
        }

        private void onJournalEntry(SupercruiseEntry entry)
        {
            // SuperCruising began
            this.fsdEngaged = false;
        }

        #endregion

        #region journal tracking for ground ops

        private void onJournalEntry(Touchdown entry)
        {
            this.touchdownLocation = new LatLong2(entry);
            var ago = Util.timeSpanToString(DateTime.UtcNow - entry.timestamp);
            log($"Touchdown {ago}, at: {touchdownLocation}");
        }

        private void onJournalEntry(Liftoff entry)
        {
            this.touchdownLocation = null;
            log($"Liftoff!");
        }


        #endregion
    }
}
