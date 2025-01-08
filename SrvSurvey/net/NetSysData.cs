using SrvSurvey.game;
using SrvSurvey.net.EDSM;
using SrvSurvey.Properties;
using SrvSurvey.units;
using System.Diagnostics;

namespace SrvSurvey.net
{
    /// <summary>
    /// General mechanism for cached look-ups of external system data
    /// </summary>
    internal class NetSysData
    {
        #region static caching / loading code

        internal enum Source
        {
            EdsmBodies,
            EdsmTraffic,
            SpanshDump,
            SpanshSystem,
            CanonnBio,
        }

        private static readonly Dictionary<string, NetSysData> cache = new();

        public static NetSysData get(string systemName, long systemAddress, Action<Source, NetSysData> func)
        {
            if (!cache.ContainsKey(systemName))
                cache[systemName] = new NetSysData(systemName, systemAddress, func);
            else
                cache[systemName].makeRequests();

            return cache[systemName];
        }

        #endregion

        public readonly string systemName;
        public long systemAddress { get; private set; }
        public string? starClass { get; set; }
        public StarPos? starPos { get; private set; }
        public bool? discovered { get; private set; }
        public string discoveredBy { get; private set; }
        public DateTimeOffset? discoveredDate { get; private set; }
        public int totalBodyCount { get; private set; }
        public int scanBodyCount { get; private set; }
        public int genusCount { get; private set; }
        public DateTimeOffset? lastUpdated { get; private set; }

        public Dictionary<string, int> countPOI = new Dictionary<string, int>()
        {
            // (these are rendered in this order)
            { "Bodies", 0 }, // Count of bodies in system
            { "Genus", 0 }, // Total count for the system
            { "StarPorts", 0 }, // Anything with a large pad
            { "Outposts", 0 },
            { "Settlements", 0 }, // Odyssey settlements
            { "FC", 0 }, // Fleet carriers
            { "Wars", 0 }, // Count of wars - (Count of factions in War or Civil-War state / 2)
        };

        /// <summary> A collection of special things worth rendering </summary>
        public Dictionary<string, List<string>>? special;

        private Action<Source, NetSysData> func;
        private Task<ApiSystemDump.System>? getSystemDump;
        //private Task<ApiSystem.Record>? getSystem;
        //private Task<CanonnBodyBioStats>? systemBioStats;

        // responses from various APIs
        public EDSM.EdsmSystem edsmBodies { get; private set; }
        public EDSM.EdsmSystemTraffic edsmTraffic { get; private set; }
        public ApiSystemDump.System spanshDump { get; private set; }
        //public ApiSystem.Record spanshSystem { get; private set; }
        //public canonn.CanonnBodyBioStats canonnBio { get; private set; }

        private NetSysData(string systemName, long systemAddress, Action<Source, NetSysData> func)
        {
            this.systemName = systemName;
            this.systemAddress = systemAddress;
            this.func = func;

            this.makeRequests();
        }

        private void makeRequests()
        {
            // get bodies from EDSM
            if (this.edsmBodies != null)
                this.func(Source.EdsmBodies, this);
            else
                Game.edsm.getBodies(systemName).ContinueWith(task => Program.crashGuard(() =>
                {
                    if (task.Exception != null || !task.IsCompletedSuccessfully)
                        Util.isFirewallProblem(task.Exception);
                    else if (this.edsmBodies == null)
                        this.processEdsmBodies(task.Result);
                }));

            // get traffic from EDSM
            if (this.edsmTraffic != null)
                this.func(Source.EdsmTraffic, this);
            else
                Game.edsm.getSystemTraffic(systemName).ContinueWith(task => Program.crashGuard(() =>
                {
                    if (task.Exception != null || !task.IsCompletedSuccessfully)
                        Util.isFirewallProblem(task.Exception);
                    else if (this.edsmTraffic == null)
                        this.processEdsmTraffic(task.Result);
                }));

            if (this.systemAddress > 0)
                this.makeRequestsByAddress(this.systemAddress);
        }

        private void makeRequestsByAddress(long address)
        {
            if (address == 0) return;

            if (this.spanshDump != null) this.func(Source.SpanshDump, this);
            //if (this.spanshSystem != null) this.func(Source.SpanshSystem, this);
            //if (this.canonnBio != null) this.func(Source.CanonnBio, this);

            lock (this.func)
            {
                this.systemAddress = address;

                // get general dump from Spansh
                if (this.spanshDump == null && getSystemDump == null)
                {
                    getSystemDump = Game.spansh.getSystemDump(address);
                    getSystemDump.ContinueWith(task => Program.crashGuard(() =>
                    {
                        if (task.Exception != null || !task.IsCompletedSuccessfully)
                            Util.isFirewallProblem(task.Exception);
                        else if (this.spanshDump == null && task.Result != null)
                            this.processSpanshDump(task.Result);
                    }));
                }

                /*
                // get system data from Spansh
                if (this.spanshSystem == null && getSystem == null)
                {
                    getSystem = Game.spansh.getSystem(address);
                    getSystem.ContinueWith(task => Program.crashGuard(() =>
                    {
                        if (task.Exception != null || !task.IsCompletedSuccessfully)
                            Util.isFirewallProblem(task.Exception);
                        else if (this.spanshSystem == null && task.Result != null)
                            this.processSpanshSystem(task.Result);
                    }));
                }
                */

                /*
                // get bio stats from Canonn
                if (this.canonnBio == null && systemBioStats == null)
                {
                    systemBioStats = Game.canonn.systemBioStats(address);
                    systemBioStats.ContinueWith(task => Program.crashGuard(() =>
                    {
                        if (task.Exception != null || !task.IsCompletedSuccessfully)
                            Util.isFirewallProblem(task.Exception);
                        else if (this.canonnBio == null && task.Result != null)
                            this.processCanonnBio(task.Result);
                    }));
                }
                */
            }
        }

        private void setDiscovered(long address)
        {
            if (address > 0) this.discovered = true;
            else if (this.discovered == null) this.discovered = false;
        }

        private void invokeFunc(Source source)
        {
            // check if all network requests are done and we're still not discovered

            // invoke on main thread if needed
            if (Program.control.InvokeRequired)
                Program.defer(() => this.func(source, this));
            else
                this.func(source, this);
        }

        public void processEdsmBodies(EDSM.EdsmSystem _edsmBodies)
        {
            this.edsmBodies = _edsmBodies;

            if (this.systemAddress == 0)
                this.makeRequestsByAddress(_edsmBodies.id64);

            // known system?
            this.setDiscovered(_edsmBodies.id64);

            if (_edsmBodies.bodies != null)
            {
                // starclass?
                if (this.starClass == null)
                    this.starClass = _edsmBodies.bodies.FirstOrDefault(b => b.isMainStar)?.spectralClass?[0].ToString();

                // use as lastUpdated?
                if (!Game.settings.useLastUpdatedFromSpanshNotEDSM && lastUpdated == null && _edsmBodies.bodies.Count > 0)
                    this.lastUpdated = _edsmBodies.bodies.Max(b => b.updateTime ?? DateTimeOffset.MinValue).ToLocalTime();

                // better body counts?
                if (this.scanBodyCount < _edsmBodies.bodies.Count) this.scanBodyCount = _edsmBodies.bodies.Count;
                if (this.totalBodyCount < _edsmBodies.bodyCount) this.totalBodyCount = _edsmBodies.bodyCount;
                this.countPOI["Bodies"] = this.totalBodyCount;

                // first discoverer - by which ever body first has that data. TODO: use MIN from all bodies?
                var firstBody = _edsmBodies.bodies.FirstOrDefault(b => b.discovery != null);
                if (firstBody?.discovery != null)
                {
                    this.discoveredBy = firstBody.discovery.commander;
                    this.discoveredDate = firstBody.discovery.date.ToLocalTime();
                }
            }

            // notify calling code
            this.invokeFunc(Source.EdsmBodies);
        }

        public void processEdsmTraffic(EDSM.EdsmSystemTraffic _edsmTraffic)
        {
            this.edsmTraffic = _edsmTraffic;
            if (this.systemAddress == 0)
                this.makeRequestsByAddress(_edsmTraffic.id64);

            // known system?
            this.setDiscovered(_edsmTraffic.id64);

            // use discovered by?
            if (_edsmTraffic.discovery?.commander != null && _edsmTraffic.discovery?.date != null)
            {
                this.discoveredBy = _edsmTraffic.discovery.commander;
                this.discoveredDate = _edsmTraffic.discovery.date.ToLocalTime();
            }

            // notify calling code
            this.invokeFunc(Source.EdsmTraffic);
        }

        public void processSpanshDump(ApiSystemDump.System _spanshDump)
        {
            this.spanshDump = _spanshDump;

            // known system?
            this.setDiscovered(_spanshDump.id64);

            // starPos?
            if (this.starPos == null)
                this.starPos = new StarPos(_spanshDump.coords.x, _spanshDump.coords.y, _spanshDump.coords.z);

            // starClass?
            if (this.starClass == null)
                this.starClass = _spanshDump.bodies.FirstOrDefault(b => b.mainStar == true)?.spectralClass?[0].ToString();

            // better body count?
            var scanCount = _spanshDump.bodies.Count(b => b.type != "Barycentre");
            if (this.scanBodyCount < scanCount) this.scanBodyCount = scanCount;
            if (this.totalBodyCount < _spanshDump.bodyCount) this.totalBodyCount = _spanshDump.bodyCount;
            this.countPOI["Bodies"] = this.totalBodyCount;

            // sum count of bio signals?
            this.genusCount = _spanshDump.bodies.Sum(b => b.signals?.signals?.GetValueOrDefault("$SAA_SignalType_Biological;")) ?? 0;
            this.countPOI["Genus"] = this.genusCount;

            // get stations from all bodies into a single list
            var allStations = new List<ApiSystemDump.System.Station>(_spanshDump.stations);
            _spanshDump.bodies.ForEach(b =>
            {
                if (b.stations.Count > 0)
                    allStations.AddRange(b.stations);
            });

            if (allStations.Count > 0)
            {
                var countFC = 0;
                var countSettlements = 0;
                var countStarports = 0;
                var countOutposts = 0;
                foreach (var station in allStations)
                {
                    if (station.type == "Drake-Class Carrier") countFC++;
                    if (station.type == "Settlement") countSettlements++;
                    if (station.type == "Outpost") countOutposts++;
                    if (EdsmSystemStations.Starports.Contains(station.type)) countStarports++;
                    // Include dockable mega ships with shipyards
                    if (station.type == "Mega ship" && station.landingPads != null) countStarports++;

                    // any traders or brokers?
                    if (station.services?.Contains("Material Trader") == true)
                    {
                        this.special ??= new ();
                        if (!this.special.ContainsKey(station.name)) this.special[station.name] = new List<string>();
                        var matTrader = Misc.NetSysData_MaterialTrader + " " + getMatTraderWithType(station);
                        this.special[station.name].Add(matTrader);
                    }
                    if (station.services?.Contains("Technology Broker") == true)
                    {
                        this.special ??= new();
                        if (!this.special.ContainsKey(station.name)) this.special[station.name] = new List<string>();
                        var techBroker = Misc.NetSysData_TechBroker + " " + getTechBrokerType(station);
                        this.special[station.name].Add(techBroker);
                    }

                    // or Engineers?
                    if (station.government == "Engineer")
                    {
                        this.special ??= new();
                        if (!this.special.ContainsKey(station.name)) this.special[station.name] = new List<string>();
                        this.special[station.name].Add($"{station.controllingFaction} {Misc.NetSysData_Engineer}");
                    }
                }
                if (countFC > 0) this.countPOI["FC"] = countFC;
                if (countSettlements > 0) this.countPOI["Settlements"] = countSettlements;
                if (countOutposts > 0) this.countPOI["Outposts"] = countOutposts;
                if (countStarports > 0) this.countPOI["StarPorts"] = countStarports;
            }

            // any factions at war?
            var countWars = _spanshDump.factions?.Count(f => f.state == "War" || f.state == "Civil War") ?? 0;
            if (countWars > 0)
                this.countPOI["Wars"] = countWars / 2;

            // notify calling code
            this.invokeFunc(Source.SpanshDump);
        }

        private string? getMatTraderWithType(ApiSystemDump.System.Station station)
        {
            var primary_economy = station.primaryEconomy.ToLowerInvariant();
            var secondary_economy = station.economies?.OrderBy(e => e.Value).Skip(1).FirstOrDefault().Key.ToLowerInvariant();

            // See: https://github.com/EDCD/FDevIDs/blob/master/How%20to%20determine%20MatTrader%20and%20Broker%20type
            if (primary_economy == "high tech" || primary_economy == "military") return Misc.NetSysData_Encoded;
            if (primary_economy == "extraction" || primary_economy == "refinery") return Misc.NetSysData_Raw;
            if (primary_economy == "industrial") return Misc.NetSysData_Manufactured;
            if (secondary_economy == "high tech" || secondary_economy == "military") return Misc.NetSysData_Encoded;
            if (secondary_economy == "extraction" || secondary_economy == "refinery") return Misc.NetSysData_Raw;
            if (secondary_economy == "industrial") return Misc.NetSysData_Manufactured;

            Debugger.Break();
            return null;
        }

        private string? getTechBrokerType(ApiSystemDump.System.Station station)
        {
            var primary_economy = station.primaryEconomy.ToLowerInvariant();
            var secondary_economy = station.economies?.OrderBy(e => e.Value).Skip(1).FirstOrDefault().Key.ToLowerInvariant();

            // See: https://github.com/EDCD/FDevIDs/blob/master/How%20to%20determine%20MatTrader%20and%20Broker%20type
            if (primary_economy == "high tech") return Misc.NetSysData_Guardian;
            if (primary_economy == "industrial") return Misc.NetSysData_Human; // human may be set as a default and it is not needed
            if (secondary_economy == "high tech") return Misc.NetSysData_Guardian;
            if (secondary_economy != null && secondary_economy != "high tech") return Misc.NetSysData_Human; // needs a confirmation

            Debugger.Break();
            return null;
        }

        /*
        public void processSpanshSystem(ApiSystem.Record _spanshSystem)
        {
            this.spanshSystem = _spanshSystem;

            // known system?
            this.setDiscovered(_spanshSystem.id64);

            // starPos?
            if (this.starPos == null)
                this.starPos = new StarPos(_spanshSystem.x, _spanshSystem.y, _spanshSystem.z);

            // starClass?
            if (this.starClass == null)
                this.starClass = _spanshSystem.bodies.FirstOrDefault(b => b.is_main_star == true)?.subtype?[0].ToString(); // TODO: confirm this is always the correct letter

            // use as lastUpdated?
            if (Game.settings.useLastUpdatedFromSpanshNotEDSM && this.lastUpdated == null)
                this.lastUpdated = _spanshSystem.updated_at?.ToLocalTime();

            // better body count?
            if (this.totalBodyCount < _spanshSystem.body_count) this.totalBodyCount = _spanshSystem.body_count;
            this.countPOI["Bodies"] = this.totalBodyCount;

            // how many stations are there?
            if (_spanshSystem.stations?.Count > 0)
            {
                var countFC = 0;
                var countSettlements = 0;
                var countStarports = 0;
                var countOutposts = 0;
                foreach (var sta in _spanshSystem.stations)
                {
                    if (sta.type == "Drake-Class Carrier") countFC++;
                    if (sta.type == "Settlement") countSettlements++;
                    if (sta.type == "Outpost") countOutposts++;
                    if (EdsmSystemStations.Starports.Contains(sta.type)) countStarports++;
                    // Include dockable mega ships with shipyards
                    if (sta.type == "Mega ship" && sta.has_shipyard) countStarports++;
                }
                if (countFC > 0) this.countPOI["FC"] = countFC;
                if (countSettlements > 0) this.countPOI["Settlements"] = countSettlements;
                if (countOutposts > 0) this.countPOI["Outposts"] = countOutposts;
                if (countStarports > 0) this.countPOI["StarPorts"] = countStarports;
            }

            // any factions at war?
            var countWars = _spanshSystem.minor_faction_presences?.Count(f => f.state == "War" || f.state == "Civil War") ?? 0;
            if (countWars > 0)
                this.countPOI["Wars"] = countWars / 2;

            // notify calling code
            this.invokeFunc(Source.SpanshSystem);
        }
        */

        /*
        public void processCanonnBio(canonn.CanonnBodyBioStats _canonnBio)
        {
            this.canonnBio = _canonnBio;

            //// known system?
            //this.setDiscovered(_canonnBio.id64);

            // starPos?
            if (this.starPos == null)
                this.starPos = new StarPos(_canonnBio.coords.x, _canonnBio.coords.y, _canonnBio.coords.z);

            // system genus count
            this.genusCount = _canonnBio.bodies.Sum(_ => _.signals?.genuses?.Count ?? 0);
            this.countPOI["Genus"] = this.genusCount;
            // OR ...?
            //foreach (var body in canonnBio.bodies)
            //{
            //    var bioSignals = body.signals?.signals?.GetValueOrDefault("$SAA_SignalType_Biological;") ?? 0;
            //    if (bioSignals > 0) this.info.countPOI["Genus"] += bioSignals;
            //}

            // notify calling code
            this.invokeFunc(Source.CanonnBio);
        }
        */

        public string? discoveryStatus
        {
            get
            {
                if (this.discovered == null)
                    return null;
                if (this.discovered == false || (this.lastUpdated == null && this.totalBodyCount == 0))
                    return Misc.NetSysData_UndiscoveredSystem;
                else if (this.totalBodyCount == 0)
                    return Misc.NetSysData_UnscannedSystem;
                else if (this.totalBodyCount == this.scanBodyCount)
                    return Misc.NetSysData_DiscoveredAll.format(this.totalBodyCount);
                else
                    return Misc.NetSysData_DiscoveredPartial.format(this.scanBodyCount, this.totalBodyCount);
            }
        }
    }

}
