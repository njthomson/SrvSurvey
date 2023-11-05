using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        CarrierMgmt,

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
        public static bool showPlotter
        {
            get
            {
                return Game.activeGame?.systemStatus.honked ?? false;
                /*
                return Game.settings.autoShowPlotSysStatus
                    && Game.activeGame != null
                    && Game.activeGame.isMode(GameMode.SuperCruising, GameMode.SAA, GameMode.FSS, GameMode.ExternalPanel, GameMode.Orrery, GameMode.SystemMap)
                    // show only after honking
                    && Game.activeGame.systemStatus.honked
                    // hide if FSS is complete but we scanned no systems - means FSS was done previously
                    ; //&& (!Game.activeGame.systemStatus.fssComplete || Game.activeGame.systemStatus.fssBodies.Count > 0);
                      //&& Game.activeGame.systemStatus.dssRemaining.Count > 0
                      //&& Game.activeGame.systemStatus.bioBodies.Count > 0;
                // */
            }
        }

        public string name;
        public long address;
        public int bodyCount;
        public bool honked;
        public bool fssComplete;
        public Dictionary<string, string> fssBodies = new Dictionary<string, string>();
        public Dictionary<string, double> valueBodies = new Dictionary<string, double>();
        public HashSet<string> dssBodies = new HashSet<string>();
        public HashSet<string> visitedBodies = new HashSet<string>();
        public Dictionary<int, string> bodyIds = new Dictionary<int, string>();
        public Dictionary<int, int> bioBodies = new Dictionary<int, int>();
        public Dictionary<int, int> bioScans = new Dictionary<int, int>();
        public int scannedOrganics;

        public SystemStatus(string name, long address)
        {
            this.name = name;
            this.address = address;
        }

        public void initFromJournal(Game game)
        {
            Game.log($"initFromJournal.walk: begin");

            var manualVisits = new List<SendText>();

            game.journals?.walkDeep(-1, true, (entry) =>
            {
                // stop at FSDJump's
                if (entry is FSDJump) return true;

                this.Journals_onJournalEntry(entry);

                if (entry is SendText)
                    manualVisits.Add((SendText)entry);

                return false;
            });

            // re-init for .visited systems
            if (manualVisits.Count > 0)
            {
                foreach(var entry in manualVisits)
                    this.onJournalEntry(entry);
            }
            Game.log($"initFromJournal.walk: found FSS: {this.fssComplete}, bodyCount: {this.bodyCount}, count FSS: {this.fssBodies.Count}, count DSS: {this.dssBodies.Count}, count bio bodies: {this.bioBodies.Count}");
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

            // ignore stars
            if (entry.StarType != null)
                return;

            var bodyType = entry.StarType != null ? "Star" : entry.PlanetClass;

            if (bodyType != null)
            {
                this.fssBodies[entry.Bodyname] = bodyType;
                this.valueBodies[entry.Bodyname] = Util.GetBodyValue(entry, true);
            }

            // add any rings as independent bodies with "rA"/"rB" suffic
            if (entry.ScanType == "Detailed" && entry.Rings?.Count > 0)
            {
                this.fssBodies[entry.Bodyname + " rA"] = "Ring";
                this.bodyIds[entry.BodyID] = entry.Bodyname + " rA";

                if (entry.Rings?.Count == 2)
                {
                    this.fssBodies[entry.Bodyname + " rB"] = "Ring";
                    this.bodyIds[entry.BodyID] = entry.Bodyname + " rB";
                }
            }
        }

        public void onJournalEntry(SAAScanComplete entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) return;

            var bodyName = entry.BodyName;
            if (bodyName.EndsWith("Ring"))
            {
                bodyName = entry.BodyName.Replace(" Ring", "");
                bodyName = bodyName.Substring(0, bodyName.Length - 2)
                    + " r"
                    + bodyName.Substring(bodyName.Length - 1);
            }

            this.bodyIds[entry.BodyID] = bodyName;
            this.dssBodies.Add(bodyName);
            this.visitedBodies.Add(bodyName);
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

        public void onJournalEntry(SendText entry)
        {
            if (Game.activeGame?.systemStatus == null) return;

            var msg = entry.Message.ToLowerInvariant();
            if (msg.StartsWith(MsgCmd.visited, StringComparison.OrdinalIgnoreCase))
            {
                var bodyName = entry.Message.Substring(MsgCmd.visited.Length).Trim();
                Game.activeGame.systemStatus.visitedTargetBody(bodyName);
            }
        }

        public List<string> dssRemaining
        {
            get
            {
                var bodies = this.fssBodies
                    .Where(_ => _.Value != "Star" && !this.visitedBodies.Contains(_.Key));

                if (Game.settings.skipGasGiantDSS)
                    bodies = bodies
                        .Where(_ => !_.Value.Contains("gas giant", StringComparison.OrdinalIgnoreCase) && !this.visitedBodies.Contains(_.Key));

                if (Game.settings.skipRingsDSS)
                    bodies = bodies
                        .Where(_ => _.Value != "Ring" && !this.visitedBodies.Contains(_.Key));

                if (Game.settings.skipLowValueDSS)
                    bodies = bodies
                        .Where(_ => !this.valueBodies.ContainsKey(_.Key) || this.valueBodies[_.Key] > Game.settings.skipLowValueAmount);

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
                    .Where(_ => !this.bioScans.ContainsKey(_.Key) || this.bioScans[_.Key] < _.Value)
                    .Select(_ => this.bodyIds[_.Key].Replace(this.name, "").Replace(" ", ""))
                    .Order()
                    .ToList();
            }
        }

        public int sumOrganicSignals { get => this.bioBodies.Values.Sum(); }


        /// <summary>
        /// Marks the current target as a visited body
        /// </summary>
        public void visitedTargetBody(string bodyName)
        {
            if (Game.activeGame == null || Game.activeGame?.status?.Destination?.Name == null || string.IsNullOrWhiteSpace(bodyName)) return;

            var matchedBody = this.fssBodies.Keys.FirstOrDefault(_ => _.Replace(this.name, "").Replace(" ", "").Equals(bodyName, StringComparison.OrdinalIgnoreCase));
            // find a match from the destinations
            Game.log($"Matched '{matchedBody}' from '{bodyName}'");
            if (!string.IsNullOrWhiteSpace(matchedBody))
                this.visitedBodies.Add(matchedBody);
        }

        /// <summary>
        /// Open Landscape signal survey page in a browser
        /// </summary>
        public void submitSurvey(string notes)
        {
            if (Game.activeGame == null) return;

            var url = $"https://docs.google.com/forms/d/e/1FAIpQLSem1JJuPaRdBReaqowPTUelptVmLkJ-XtOP_R8ug1EHFSQTCA/viewform?usp=pp_url&entry.1338642726={Game.activeGame?.cmdr.commander}&entry.2050947313={this.name}";

            // was FSS completed
            if (this.fssComplete)
                url += "&entry.1907158015=FSS+of+all+planets";

            // were all planets DSS'd?
            var planetsInSystem = this.fssBodies.Where(_ => _.Value != "Ring" && _.Value != "Star");
            var planetsNotDSS = planetsInSystem.Where(_ => !this.dssBodies.Contains(_.Key));
            if (!planetsNotDSS.Any())
                url += "&entry.1907158015=Detailed+Surface+Scan+of+all+planets";

            // were all rings DSS'd?
            var ringsInSystem = this.fssBodies.Where(_ => _.Value == "Ring").Select(_ => _.Key);
            var ringsNotDSS = ringsInSystem.Where(_ => !this.dssBodies.Contains(_));
            if (!ringsNotDSS.Any())
                url += "&entry.1907158015=Detailed+Scan+of+all+rings";

            // finally, generate any useful notes
            var gasGiantsInSystem = planetsNotDSS.Where(_ => _.Value.Contains("gas giant", StringComparison.OrdinalIgnoreCase));
            if (planetsInSystem.Count() > 0 && gasGiantsInSystem.Count() > 0 && planetsNotDSS.All(_ => _.Value.Contains("gas giant", StringComparison.OrdinalIgnoreCase)))
                notes += " DSS all planets except Gas Giants.";

            if (this.visitedBodies.Count < this.fssBodies.Count)
                notes += $" Visited {this.visitedBodies.Count} of {this.fssBodies.Count} non-Star bodies.";

            if (!string.IsNullOrWhiteSpace(notes))
                url += $"&entry.622407078={notes.Trim()}";


            Util.openLink(url);
        }

        ///// <summary>
        ///// Open Landscape signal survey reservation page in a browser
        ///// </summary>
        //public void reserveMoreSystems()
        //{
        //    if (Game.activeGame == null) return;

        //    // assume we want to reserve more systems of the same class, meaning match "UY-S b", "KM-W c" or "FG-Y d"
        //    string systemPrefix = null!;
        //    if (Game.activeGame.cmdr.currentSystem.Contains("UY-S b"))
        //        systemPrefix = "Stuemeae UY-S b";
        //    else if (Game.activeGame.cmdr.currentSystem.Contains("KM-W c"))
        //        systemPrefix = "Stuemeae KM-W c";
        //    else if (Game.activeGame.cmdr.currentSystem.Contains("FG-Y d"))
        //        systemPrefix = "Stuemeae FG-Y d";

        //    var url = $"https://docs.google.com/forms/d/e/1FAIpQLSd2w1jFxFv33gxI-OeRsgEs0QPaCMbzDddmjefpEGjAU3A4kA/viewform?entry.1547632870={Game.activeGame?.cmdr.commander}&entry.1229009577=Reserve+10+Systems";
        //    if (systemPrefix != null)
        //        url += $"&entry.2100543585={systemPrefix}";


        //    Util.openLink(url);
        //}

        /// <summary>
        /// Open Landscape signal survey reservation page in a browser
        /// </summary>
        public async Task nextSystem()
        {
            if (Game.activeGame == null) return;

            var cmdrName = Game.activeGame?.cmdr.commander;
            var url = $"https://docs.google.com/spreadsheets/d/1U00SXnU0fGn_97mTQTlN6JL99jcHR3ox962BFqnpHGA/gviz/tq?gid=1930322502&single=true&tq=select * where A contains \"{cmdrName}\"";
            Game.log($"Requesting systems in current reservation\r\n{url}");
            var response = await new HttpClient().GetStringAsync(url);

            const string marker = "setResponse(";
            var idx = response.IndexOf(marker);
            var json = response.Substring(idx + marker.Length);
            json = json.Substring(0, json.Length - 2);

            var obj = JsonConvert.DeserializeObject<JObject>(json)!;

            // reduce the data down to the set of 10 system names.
            var cells = obj["table"]?["rows"]?[0]?["c"]
                ?.Select(_ => _.HasValues ? _["v"]?.Value<string?>() : "")
                ?.Where(_ => _ != null && _ != cmdrName);

            // and find the first non-blank one, that does not match our current system
            var nextSystem = cells?.FirstOrDefault(_ => !string.IsNullOrWhiteSpace(_) && _ != this.name);
            var form = Program.getPlotter<PlotSysStatus>();

            if (string.IsNullOrWhiteSpace(nextSystem))
            {
                Game.log($"Cannot find next reserved system from:\r\n{json}");
                if (form != null)
                {
                    var m = 20 - DateTime.Now.Minute % 20;
                    form.nextSystem = $"Unknown. Try again in ~{m} minutes.";
                    form.Invalidate();
                }
            }
            else
            {
                Game.log($"Copying next reserved system to clipboard: {nextSystem}");
                Clipboard.SetText(nextSystem);

                if (form != null)
                {
                    form.nextSystem = nextSystem;
                    form.Invalidate();
                }
            }
        }

    }
}
