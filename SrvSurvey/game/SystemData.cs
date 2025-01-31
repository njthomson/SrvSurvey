using BioCriterias;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.canonn;
using SrvSurvey.net;
using SrvSurvey.net.EDSM;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SrvSurvey.game
{
    internal class SystemData : Data
    {
        #region load, close and caching

        private static readonly Dictionary<long, SystemData> cache = new();

        /// <summary>
        /// Open or create a SystemData object for the a star system by name or address.
        /// </summary>
        public static SystemData? Load(string systemName, long systemAddress, string fid, bool skipPredictSpecies = false)
        {
            if (systemName != "") Game.log($"Loading SystemData for: '{systemName}' ({systemAddress})");

            // use cache entry if present
            if (systemAddress != 0 && cache.TryGetValue(systemAddress, out SystemData? value))
                return value;

            // try finding files by systemAddress first, then system name
            var folder = Path.Combine(Program.dataFolder, "systems", fid);
            Directory.CreateDirectory(folder);
            var files = Directory.GetFiles(folder, $"*_{systemAddress}.json");
            if (files.Length == 0 && !string.IsNullOrEmpty(systemName))
            {
                files = Directory.GetFiles(folder, $"{systemName}_*.json");
            }

            // create new if no matches found
            if (files.Length == 0)
            {
                if (systemName != "") Game.log($"Nothing found for: '{systemName}' ({systemAddress})");
                return null;
            }
            else if (files.Length > 1)
            {
                Game.log($"Why are there {files.Length} multiple files for '{systemName}' ({systemAddress})? Using the first one.");
            }

            var filepath = files[0];
            var systemData = Data.Load<SystemData>(filepath)!;
            cache[systemData.address] = systemData;

            // post-process after loading ...
            foreach (var body in systemData.bodies)
            {
                // make all bodies aware of their parent system
                body.system = systemData;
                // short names
                if (body.name == null && body.id == 0) body.name = systemData.name;
                body.shortName = body.name == systemData.name
                    ? "0"
                    : body.name!.Replace(systemData.name, "").Replace(" ", "") ?? "";
                // correct Barycenters from Asteroid
                if (body.type == SystemBodyType.Asteroid && body.name.Contains("barycentre", StringComparison.OrdinalIgnoreCase))
                    body.type = SystemBodyType.Barycentre;
                // null out unnecessarily populated members
                if (body.bookmarks?.Count == 0) body.bookmarks = null;
                if (body.bioScans?.Count == 0) body.bioScans = null;

                if (body.bioSignalCount > 0)
                {
                    // make all entries aware of their parent body
                    body.organisms?.ForEach(org => org.body = body);

                    // make predictions based on what we know
                    if (Game.ready && Game.activeGame?.fid == fid && !skipPredictSpecies)
                        body.predictSpecies();
                }
            }

            return systemData;
        }

        /// <summary>
        /// Close a SystemData object for the a star system by name or address.
        /// </summary>
        /// <param name="systemName"></param>
        /// <param name="systemAddress"></param>
        public static void Close(SystemData? data)
        {
            Game.log($"Closing and saving SystemData for: '{data?.name}' ({data?.address})");
            lock (cache)
            {
                // ensure data is saved then remove from the cache
                data?.Save();

                if (data != null && cache.ContainsValue(data))
                    cache.Remove(data.address);
            }
        }

        public static void CloseAll()
        {
            Game.log($"Closing and saving {cache.Count} entries");
            lock (cache)
            {
                // ensure data is saved then remove from the cache
                while (cache.Any())
                {
                    var data = cache.First().Value;
                    data.Save();
                    cache.Remove(data.address);
                }
            }
        }

        public static SystemData From(StarRef starRef, string fid)
        {
            return Load(starRef.name, starRef.id64, fid, true)!;
        }

        public static SystemData From(ISystemDataStarter entry, string? fid = null, string? cmdrName = null)
        {
            lock (cache)
            {
                // load from file or cache
                fid ??= Game.activeGame!.fid!;
                var data = Load(entry.StarSystem, entry.SystemAddress, fid);

                if (data == null)
                {
                    // create a new data object with the main star populated
                    data = new SystemData()
                    {
                        filepath = Path.Combine(Program.dataFolder, "systems", fid, $"{entry.StarSystem}_{entry.SystemAddress}.json"),
                        name = entry.StarSystem,
                        address = entry.SystemAddress,
                        starPos = entry.StarPos,
                        commander = cmdrName ?? Game.activeGame!.cmdr.commander,
                        firstVisited = entry.timestamp,
                        lastVisited = entry.timestamp,
                    };

                    // only add this body if we know it's a star
                    if (entry.BodyType == FSDJumpBodyType.Star)
                    {
                        var mainStar = new SystemBody()
                        {
                            system = data,
                            id = entry.BodyID,
                            name = entry.Body,
                            shortName = entry.Body.Replace(entry.StarSystem, "").Replace(" ", ""),
                            type = SystemBodyType.Star,
                            firstVisited = entry.timestamp,
                            lastVisited = entry.timestamp,
                        };
                        if (mainStar.shortName.Length == 0) mainStar.shortName = "0";
                        data.bodies.Add(mainStar);
                    }

                    cache[entry.SystemAddress] = data;
                    data.Save();
                }

                return data;
            }
        }

        public static SystemData From(EdsmSystem edsmBodies, double[] starPos, string fid, string cmdrName)
        {
            fid ??= Game.activeGame!.fid!;

            var data = new SystemData()
            {
                filepath = Path.Combine(Program.dataFolder, "systems", fid, $"{edsmBodies.name}_{edsmBodies.id64}.json"),
                name = edsmBodies.name,
                address = edsmBodies.id64,
                starPos = starPos,
                commander = cmdrName,
            };

            data.onEdsmResponse(edsmBodies);

            return data;
        }

        public static SystemData From(ApiSystemDump.System dump, string fid, string cmdrName)
        {
            fid ??= Game.activeGame!.fid!;

            var data = new SystemData()
            {
                filepath = Path.Combine(Program.dataFolder, "systems", fid, $"{dump.name}_{dump.id64}.json"),
                name = dump.name,
                address = dump.id64,
                starPos = new double[3] { dump.coords.x, dump.coords.y, dump.coords.z },
                commander = cmdrName,
            };

            data.onSpanshResponse(dump);

            return data;
        }

        #endregion

        #region migrate data from individual body files

        private static SystemData From(BodyDataOld bodyData, CommanderSettings cmdr)
        {
            lock (cache)
            {
                // load from file or cache
                var data = SystemData.Load(bodyData.systemName, bodyData.systemAddress, cmdr.fid, true);

                if (data == null)
                {
                    // create a new data object with the main star populated
                    data = new SystemData()
                    {
                        filepath = Path.Combine(Program.dataFolder, "systems", cmdr.fid, $"{bodyData.systemName}_{bodyData.systemAddress}.json"),
                        name = bodyData.systemName,
                        address = bodyData.systemAddress,
                        starPos = null!,
                        commander = cmdr.commander,
                        firstVisited = bodyData.firstVisited,
                        lastVisited = bodyData.lastVisited,
                    };

                    if (cache.ContainsKey(bodyData.systemAddress))
                        throw new Exception($"Cache problem with '{bodyData.systemName}' ?");

                    cache[bodyData.systemAddress] = data;
                    data.Save();
                }
                return data;
            }
        }

        public static async Task migrate_BodyData_Into_SystemData(CommanderSettings cmdr)
        {
            var folder = Path.Combine(Program.dataFolder, "organic", cmdr.fid);
            if (!Directory.Exists(folder)) return;

            var filenames = Directory.GetFiles(folder);
            Game.log($"Migrate cmdr 'BodyData' into 'SystemData', {filenames.Length} files to process ...");

            foreach (var filename in filenames)
            {
                var filepath = Path.Combine(folder, filename);
                Game.log($"Reading: {filepath}");
                var bodyData = Data.Load<BodyDataOld>(filepath)!;

                var systemData = SystemData.From(bodyData, cmdr);
                if (systemData.starPos == null)
                {
                    // we need a starPos before we can create the file...
                    // get starPos from EDSM or Spansh, or fail :(
                    var edsmResult = await Game.edsm.getSystems(systemData.name);
                    systemData.starPos = edsmResult?.FirstOrDefault()?.coords!;
                }
                if (systemData.starPos == null)
                {
                    var spanshResult = await Game.spansh.getSystems(systemData.name);
                    var matchedSystem = spanshResult.min_max.FirstOrDefault(_ => _.name.Equals(systemData.name, StringComparison.OrdinalIgnoreCase));
                    if (matchedSystem != null)
                        systemData.starPos = new double[] { matchedSystem.x, matchedSystem.y, matchedSystem.z };
                }
                if (systemData.starPos == null)
                {
                    // TODO: search back through journal files?
                    Game.log($"Failed to find a starPos for: '{systemData.name}' - NOT migrating this system :(");
                    continue;
                }

                // update fields
                if (systemData.firstVisited > bodyData.firstVisited) systemData.firstVisited = bodyData.firstVisited;
                if (systemData.lastVisited < bodyData.lastVisited) systemData.lastVisited = bodyData.lastVisited;

                // populate the body
                var systemBody = systemData.findOrCreate(bodyData.bodyName, bodyData.bodyId);
                if (systemBody.lastTouchdown == null && bodyData.lastTouchdown != null) systemBody.lastTouchdown = bodyData.lastTouchdown;
                if (systemBody.type == SystemBodyType.Unknown) systemBody.type = SystemBodyType.LandableBody; // assume it's landable as we have old data for it

                // migrate any bioScans
                if (bodyData.bioScans?.Count > 0)
                {
                    foreach (var bioScan in bodyData.bioScans)
                    {
                        // try using the variant name from 'bodyData.organisms'
                        if (bioScan.entryId == 0 || bioScan.entryId.ToString().EndsWith("00") || bioScan.entryId.ToString().Length < 7)
                        {
                            var matchSpecies = bodyData.organisms.Values.FirstOrDefault(_ => _.species == bioScan.species && _.variant != null);
                            if (matchSpecies?.variant != null)
                            {
                                bioScan.entryId = Game.codexRef.matchFromVariant(matchSpecies.variant).entryId;
                            }
                        }

                        // attempt to match entryId from cmdr's scannedBioEntryIds list
                        if (bioScan.entryId == 0 || bioScan.entryId.ToString().EndsWith("00") || bioScan.entryId.ToString().Length < 7)
                        {
                            var speciesRef = Game.codexRef.matchFromSpecies(bioScan.species!);
                            var prefix = $"{bodyData.systemAddress}_{bodyData.bodyId}_{speciesRef.entryIdPrefix}";
                            var matchForEntryId = cmdr.scannedBioEntryIds.FirstOrDefault(_ => _.StartsWith(prefix));
                            if (matchForEntryId != null)
                            {
                                var parts = matchForEntryId.Split('_');
                                bioScan.entryId = long.Parse(parts[2]);
                            }

                            if (bioScan.entryId == 0 || bioScan.entryId.ToString().EndsWith("00") || bioScan.entryId.ToString().Length < 7)
                            {
                                var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.species.Any(_ => _.name == bioScan.species) || genusRef.name == bioScan.genus);
                                if (genusRef?.odyssey == false)
                                {
                                    // try populate with some legacy entryId
                                    bioScan.entryId = long.Parse(speciesRef.entryIdPrefix + speciesRef.variants[0].entryIdSuffix);
                                }
                            }
                            if (bioScan.entryId == 0 || bioScan.entryId.ToString().EndsWith("00") || bioScan.entryId.ToString().Length < 7)
                            {
                                // otherwise populate with some weak entryId
                                bioScan.entryId = long.Parse(speciesRef.entryIdPrefix + "00");
                            }
                        }

                        // update equivalent in systemBody 
                        var match = systemBody.bioScans?.FirstOrDefault(_ => _.species == bioScan.species && _.location.Lat == bioScan.location.Lat && _.location.Long == bioScan.location.Long);
                        if (match == null)
                        {
                            systemBody.bioScans ??= new List<BioScan>();
                            systemBody.bioScans.Add(bioScan);
                        }
                        else if (match.entryId == 0 || match.entryId.ToString().EndsWith("00"))
                        {
                            match.entryId = bioScan.entryId;
                        }
                    }
                }


                // migrate any organisms
                if (bodyData.organisms?.Count > 0)
                {
                    systemBody.organisms ??= new List<SystemOrganism>();

                    foreach (var bodyOrg in bodyData.organisms.Values)
                    {
                        SystemOrganism? systemOrg;
                        if (string.IsNullOrEmpty(bodyOrg.variant))
                        {
                            var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.species.Any(_ => _.name == bodyOrg.species) || genusRef.name == bodyOrg.genus);
                            if (genusRef?.odyssey == false && bodyOrg.species != null)
                            {
                                // legacy organics never had a variant, but we double up the species info
                                Game.log($"Repair legacy missing variant: '{bodyOrg.species ?? bodyOrg.genus}' on '{bodyData.bodyName}' ({bodyData.bodyId})");
                                var matchedSpecies = Game.codexRef.matchFromSpecies(bodyOrg.species!);
                                bodyOrg.variant = matchedSpecies.variants[0].name;
                                bodyOrg.variantLocalized = matchedSpecies.variants[0].englishName;
                            }
                            else if (bodyOrg.genus != null)
                            {
                                var foo = systemBody.bioScans?.FirstOrDefault(_ => _.species == bodyOrg.species && _.entryId > 0 && !_.entryId.ToString().EndsWith("00") && _.entryId.ToString().Length == 7);
                                if (foo != null)
                                {
                                    // we can repair this one!
                                    Game.log($"Repairing missing variant: '{bodyOrg.species ?? bodyOrg.genus}' ({foo.entryId}) on '{bodyData.bodyName}' ({bodyData.bodyId})");
                                    var bioMatch2 = Game.codexRef.matchFromEntryId(foo.entryId);
                                    bodyOrg.variant = bioMatch2.variant.name;
                                    bodyOrg.variantLocalized = bioMatch2.variant.englishName;
                                }
                                else
                                {

                                    // missing data - add just genus data for this, as if we DSS'd the body
                                    // We did DSS but never scanned an organism - all we have is a genus
                                    systemOrg = systemBody.organisms.FirstOrDefault(_ => _.genus == bodyOrg.genus);
                                    if (systemOrg == null)
                                    {
                                        Game.log($"Variant missing, adding genus only for: '{bodyOrg.species ?? bodyOrg.genus}' on '{bodyData.bodyName}' ({bodyData.bodyId})");
                                        systemOrg = new SystemOrganism()
                                        {
                                            body = systemBody,
                                            genus = bodyOrg.genus,
                                            genusLocalized = bodyOrg.genusLocalized!,
                                        };
                                        systemBody.organisms.Add(systemOrg);
                                        systemBody.organisms = systemBody.organisms.OrderBy(_ => _.genusLocalized).ToList();
                                        systemBody.bioSignalCount = Math.Max(systemBody.bioSignalCount, systemBody.organisms.Count);
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                // missing data - add just genus data for this, as if we DSS'd the body
                                Game.log($"Bad data for: '{bodyOrg.species ?? bodyOrg.genus ?? bodyOrg.speciesLocalized ?? bodyOrg.genusLocalized}' on '{bodyData.bodyName}' ({bodyData.bodyId})");
                                continue;
                            }
                        }

                        if (bodyOrg.variant == null) throw new Exception($"Variant STILL missing, adding genus only for: '{{bodyOrg.species ?? bodyOrg.genus}}' on '{{bodyData.bodyName}}' ({{bodyData.bodyId}})\"");
                        var bioMatch = Game.codexRef.matchFromVariant(bodyOrg.variant);
                        systemOrg = systemBody.findOrganism(bioMatch);
                        if (systemOrg == null)
                        {
                            systemOrg = new SystemOrganism()
                            {
                                body = systemBody,
                                genus = bioMatch.genus.name,
                            };
                            Game.log($"add organism '{bioMatch.variant.name}' ({bioMatch.entryId}) to '{systemBody.name}' ({systemBody.id})");
                            systemBody.organisms.Add(systemOrg);
                            systemBody.organisms = systemBody.organisms.OrderBy(_ => _.genusLocalized).ToList();
                            systemBody.bioSignalCount = Math.Max(systemBody.bioSignalCount, systemBody.organisms.Count);
                        }

                        // update fields
                        if (systemOrg.entryId == 0) systemOrg.entryId = bioMatch.entryId;
                        if (systemOrg.reward == 0) systemOrg.reward = bioMatch.species.reward;
                        if (!systemOrg.analyzed && bodyOrg.analyzed) systemOrg.analyzed = true;

                        if (systemOrg.genusLocalized == null && bodyOrg.genusLocalized != null) systemOrg.genusLocalized = bodyOrg.genusLocalized;
                        systemOrg.genusLocalized ??= bioMatch.genus.englishName;
                        if (systemOrg.species == null && bodyOrg.species != null) systemOrg.species = bodyOrg.species;
                        if (systemOrg.speciesLocalized == null && bodyOrg.speciesLocalized != null) systemOrg.speciesLocalized = bodyOrg.speciesLocalized;
                        if (systemOrg.variant == null && bodyOrg.variant != null) systemOrg.variant = bodyOrg.variant;
                        if (systemOrg.variantLocalized == null && bodyOrg.variantLocalized != null) systemOrg.variantLocalized = bodyOrg.variantLocalized;
                    }

                    // finally - can we repair any 'scannedBioEntryIds' with better entryId's or firstFootfall's?
                    var list = cmdr.scannedBioEntryIds.ToList();
                    for (var n = 0; n < list.Count; n++)
                    {
                        var scannedEntryId = list[n];
                        var prefix = $"{bodyData.systemAddress}_{bodyData.bodyId}_";
                        if (!scannedEntryId.StartsWith(prefix)) continue;

                        var parts = scannedEntryId.Split('_');
                        if (parts[2].EndsWith("00") || parts[2].Length == 5)
                        {
                            var matchedOrganismWithEntryId = systemBody.organisms.FirstOrDefault(_ => !_.entryId.ToString().EndsWith("00") && _.entryId.ToString().StartsWith(parts[2].Substring(0, 5)));
                            if (matchedOrganismWithEntryId != null)
                                parts[2] = matchedOrganismWithEntryId.entryId.ToString();
                        }

                        if (systemBody.firstFootFall && parts[4] == bool.FalseString)
                            parts[4] = bool.TrueString;

                        scannedEntryId = string.Join('_', parts);
                        if (scannedEntryId != list[n])
                        {
                            Game.log($"Repairing: '{list[n]}' => '{scannedEntryId}'");
                            list[n] = scannedEntryId;
                        }
                    }
                    cmdr.scannedBioEntryIds = new HashSet<string>(list);
                }

                // done with this body
                systemData.Save();
                bodyData.Save();
            }

            //foreach (var scannedEntryId in cmdr.scannedBioEntryIds)
            //{
            //    var parts = scannedEntryId.Split('_');
            //    if (!parts[2].EndsWith("00")) continue;

            //    //var matchedOrganismWithEntryId = systemBody.organisms.FirstOrDefault(_ => !_.entryId.ToString().EndsWith("00") && _.entryId.ToString().StartsWith(parts[2].Substring(0, 5)));
            //    //if (matchedOrganismWithEntryId != null)

            //    Game.log($"TODO: Repair '{scannedEntryId}' ?");
            //}

            cmdr.migratedNonSystemDataOrganics = true;
            cmdr.reCalcOrganicRewards();
            cmdr.Save();
            Game.log($"Migrate cmdr 'BodyData' into 'SystemData', {filenames.Length} files to process - complete!");
        }

        #endregion

        #region data members

        /// <summary> SystemName </summary>
        public string name;

        /// <summary> SystemAddress - galactic system address </summary>
        public long address;

        /// <summary> StarPos - array of galactic [ x, y, z ] co-ordinates </summary>
        public double[] starPos;

        public string commander;
        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;

        /// <summary> Returns True when all bodies have been found with FSS </summary>
        public int bodyCount;

        /// <summary> True once a FSSDiscoveryScan is received for this system </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool honked;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool fssAllBodies;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool dssAllBodies;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string notes;

        /// <summary> A list of all bodies detected in this system </summary>
        public List<SystemBody> bodies = new();

        /// <summary> A list of known stations in this system. (Just Odyssey settlements for now) </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<CanonnStation>? stations;

        #endregion

        #region journal processing

        private SystemBody findOrCreate(string bodyName, int bodyId)
        {
            lock (this.bodies)
            {
                // do we know this body already?
                var body = this.bodies.FirstOrDefault(_ => _.id == bodyId);

                if (body == null)
                {
                    // create a stub if not
                    body = new SystemBody()
                    {
                        system = this,
                        name = bodyName,
                        shortName = bodyName.Replace(this.name, "").Replace(" ", ""),
                        id = bodyId,
                    };

                    Game.log($"SystemData: add body: '{body.name}' ({body.id}, {body.type})");
                    this.bodies.Add(body);
                }
                return body;
            }
        }

        public static List<string> journalEventTypes = new()
        {
            nameof(FSSDiscoveryScan),
            nameof(FSSAllBodiesFound),
            nameof(Scan),
            nameof(SAAScanComplete),
            nameof(FSSBodySignals),
            nameof(SAASignalsFound),
            nameof(ApproachBody),
            nameof(Touchdown),
            nameof(Disembark),
            nameof(CodexEntry),
            nameof(ScanOrganic),
            nameof(ApproachSettlement),
            nameof(ScanBaryCentre),
            nameof(FSSSignalDiscovered),
        };

        public void Journals_onJournalEntry(IJournalEntry entry, bool autoSave) { 
            var dirty = this.onJournalEntry((dynamic)entry);

            if (dirty && autoSave)
                this.Save();
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It is necessary")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "It is necessary")]
        private bool onJournalEntry(JournalEntry entry) { return false; }

        public bool onJournalEntry(FSSDiscoveryScan entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {entry.SystemName}"); return false; }

            // Discovery scan a.k.a. honk
            this.honked = true;
            this.bodyCount = entry.BodyCount;
            this.rawNonBodyCount = entry.NonBodyCount;

            return true;
        }

        public bool onJournalEntry(FSSAllBodiesFound entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }

            if (this.bodyCount < entry.Count) this.bodyCount = entry.Count;

            //// apply bonus to exploration rewards - if main star was not discovered?
            //var mainStar = bodies.Find(_ => _.isMainStar);
            //if (Game.activeGame?.systemData == this && !this.fssAllBodies && mainStar?.wasDiscovered == false)
            //{
            //    var bonus = this.bodyCount * 1000;
            //    Game.activeGame.cmdr.applyExplReward(bonus, $"FSS complete");
            //}
            // TODO: Confirm if this is the right thing to do

            // FSS completed in-game
            this.fssAllBodies = true;

            return true;
        }

        public bool onJournalEntry(Scan entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {entry.StarSystem}"); return false; }
            var body = this.findOrCreate(entry.Bodyname, entry.BodyID);

            // update fields
            body.scanned = true;
            body.type = SystemBody.typeFrom(entry.StarType, entry.PlanetClass, entry.Landable, entry.Bodyname);
            body.planetClass = entry.PlanetClass;
            if (!string.IsNullOrEmpty(entry.TerraformState))
                body.terraformable = entry.TerraformState == "Terraformable";
            body.mass = entry.MassEM > 0 ? entry.MassEM : entry.StellarMass; // mass
            body.distanceFromArrivalLS = entry.DistanceFromArrivalLS;
            body.semiMajorAxis = entry.SemiMajorAxis;
            body.absoluteMagnitude = entry.AbsoluteMagnitude;
            body.radius = entry.Radius;
            body.parents = entry.Parents;
            body.wasDiscovered = entry.WasDiscovered;
            body.wasMapped = entry.WasMapped;
            body.surfaceGravity = entry.SurfaceGravity;
            body.surfaceTemperature = entry.SurfaceTemperature;
            body.surfacePressure = entry.SurfacePressure;
            body.atmosphere = entry.Atmosphere;
            body.atmosphereType = entry.AtmosphereType;
            if (entry.AtmosphereComposition?.Count > 0) body.atmosphereComposition = entry.AtmosphereComposition.ToDictionary(_ => _.Name, _ => _.Percent);
            body.volcanism = entry.Volcanism;
            if (entry.StarType != null)
            {
                body.starType = entry.StarType;
                // body.starSubClass = entry.Subclass; // needed ?
            }
            if (entry.Materials?.Count > 0) body.materials = entry.Materials.ToDictionary(_ => _.Name, _ => _.Percent);

            if (entry.Rings != null)
            {
                foreach (var newRing in entry.Rings)
                {
                    var ring = body.rings?.FirstOrDefault(_ => _.name == newRing.Name);
                    if (ring == null)
                    {
                        Game.log($"add ring '{newRing.Name}' to '{body.name}' ({body.id})");
                        body.rings ??= new List<SystemRing>();
                        body.rings.Add(new SystemRing()
                        {
                            name = newRing.Name,
                            ringClass = newRing.RingClass,
                        });
                    }
                }
            }

            // add to system exploration rewards?
            if (Game.activeGame?.systemData == this && entry.ScanType != "NavBeaconDetail" && body.hasValue)
            {
                var reward = Util.GetBodyValue(entry, false);
                if (body.reward < reward)
                {
                    body.reward = reward;
                    Game.activeGame.cmdr.countScans += 1;
                    Game.activeGame.cmdr.applyExplReward(reward, $"Scan:{entry.ScanType} of {entry.Bodyname}");

                    // and adjust main star value too
                    if (this.honked)
                        this.applyMainStarHonkBonus();
                }
            }

            // track which body was FSS'd last
            if (entry.ScanType == "Detailed" && (body.type == SystemBodyType.LandableBody || body.type == SystemBodyType.SolidBody || body.type == SystemBodyType.Giant))
                this.lastFssBody = body;

            if (body.bioSignalCount > 0)
            {
                if (Game.activeGame?.systemData == this)
                {
                    Game.activeGame.deferPredictSpecies(body);
                    Program.invalidateActivePlotters();
                }
            }

            // redraw as well, in case visual state needs to change but predictions are no different
            Program.defer(() => FormPredictions.refresh());

            return true;
        }

        public bool onJournalEntry(ScanBaryCentre entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {entry.StarSystem}"); return false; }
            var body = this.findOrCreate($"{this.name} barycentre {entry.BodyID}", entry.BodyID);

            // update fields
            body.scanned = true;
            body.type = SystemBodyType.Barycentre;
            body.semiMajorAxis = entry.SemiMajorAxis;

            return true;
        }

        private void applyMainStarHonkBonus()
        {
            const double q = 0.56591828;

            double bonus = 0;
            int uncounted = this.bodyCount - 1;
            foreach (var body in this.bodies)
            {
                if (body.isMainStar || body.type == SystemBodyType.Asteroid) continue;
                uncounted -= 1;

                if (body.type == SystemBodyType.Star)
                {
                    var bodyValue = Util.GetBodyValue(body, false, false) * 0.3f;
                    if (!body.wasDiscovered) bodyValue *= 2.6f;
                    bonus += bodyValue;
                }
                else if (body.type == SystemBodyType.LandableBody || body.type == SystemBodyType.SolidBody || body.type == SystemBodyType.Giant)
                {
                    var k = Util.getBodyKValue(body.planetClass!, body.terraformable);
                    double bodyValue = Math.Max(500, (k + k * q * Math.Pow(body.mass, 0.2)) / 3);
                    if (!body.wasDiscovered) bodyValue *= 2.6f;

                    bonus += bodyValue;
                }
            }

            // assume "500" for any un-counted/unscanned bodies
            if (uncounted > 0)
                bonus += uncounted * 500;

            var mainStar = bodies.Find(_ => _.isMainStar);
            if (mainStar != null)
            {
                var baseValue = Util.GetBodyValue(mainStar, false, false);
                Game.log($"applyMainStarHonkBonus: baseValue: {baseValue}, bonus: {(int)bonus}");
                mainStar.reward = baseValue + (int)bonus;
            }
        }

        public bool onJournalEntry(SAAScanComplete entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);

            // update fields
            body.dssComplete = true;
            if (entry.timestamp > body.lastVisited) body.lastVisited = entry.timestamp;

            // store time of last DSS, so we can keep certain plotters visible within some duration of this time
            SystemData.lastDssCompleteAt = entry.timestamp;

            if (Game.activeGame?.systemData == this)
            {
                var reward = Util.GetBodyValue(body, true, entry.ProbesUsed <= entry.EfficiencyTarget);
                if (body.reward < reward)
                {
                    body.reward = reward;
                    Game.activeGame.cmdr.countDSS += 1;
                    Game.activeGame.cmdr.applyExplReward(reward, $"DSS of {body.name}");
                }
            }

            // have we mapped all mappable bodies in the system?
            var countMappableBodies = this.bodies.Count(_ => _.type == SystemBodyType.LandableBody || _.type == SystemBodyType.SolidBody || _.type == SystemBodyType.Giant);
            if (this.dssBodyCount == countMappableBodies && !this.dssAllBodies)
            {
                this.dssAllBodies = true;
                if (Game.activeGame?.systemData == this)
                {
                    var bonus = countMappableBodies * 10_000;
                    Game.activeGame.cmdr.applyExplReward(bonus, $"DSS mapped all valid bodies");
                }
            }

            return true;
        }

        public bool onJournalEntry(FSSSignalDiscovered entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }

            this.discoveredSignals[entry.SignalName] = entry;

            return true;
        }

        public bool onJournalEntry(FSSBodySignals entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            var body = this.findOrCreate(entry.Bodyname, entry.BodyID);

            // update fields
            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null && body.bioSignalCount < bioSignals.Count)
                body.bioSignalCount = bioSignals.Count;

            if (bioSignals != null && body.planetClass != null && Game.ready)
            {
                if (Game.activeGame?.systemData == this)
                    Game.activeGame.deferPredictSpecies(body);
            }

            var geoSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Geological;");
            if (geoSignals != null && body.geoSignalCount < geoSignals.Count)
                body.geoSignalCount = geoSignals.Count;

            return true;
        }

        public bool onJournalEntry(SAASignalsFound entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);
            body.organisms ??= new List<SystemOrganism>();

            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null)
            {
                if (body.bioSignalCount < bioSignals.Count)
                    body.bioSignalCount = bioSignals.Count;

                if (entry.Genuses != null)
                {
                    foreach (var genusEntry in entry.Genuses)
                    {
                        var organism = body.organisms.FirstOrDefault(_ => _.genus == genusEntry.Genus);
                        if (organism == null)
                        {
                            Game.log($"add organism '{genusEntry.Genus_Localised ?? genusEntry.Genus}' to '{body.name}' ({body.id})");
                            body.organisms.Add(new SystemOrganism()
                            {
                                body = body,
                                genus = genusEntry.Genus,
                                genusLocalized = genusEntry.Genus_Localised!,
                            });
                            body.organisms = body.organisms.OrderBy(_ => _.genusLocalized).ToList();
                            body.bioSignalCount = Math.Max(body.bioSignalCount, body.organisms.Count);
                        }
                    }
                }
            }

            var geoSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Geological;");
            if (geoSignals != null && body.geoSignalCount < geoSignals.Count)
                body.geoSignalCount = geoSignals.Count;

            return true;
        }

        public bool onJournalEntry(ApproachBody entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            var body = this.findOrCreate(entry.Body, entry.BodyID);

            // update fields
            if (entry.timestamp > body.lastVisited) body.lastVisited = entry.timestamp;

            return true;
        }

        public bool onJournalEntry(Touchdown entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            // ignore landing on stations
            if (!entry.OnPlanet) return false;

            var body = this.findOrCreate(entry.Body, entry.BodyID);

            // increment landing count if we have no prior touchdown location
            if (Game.activeGame?.systemData == this && body.lastTouchdown == null)
            {
                Game.activeGame.cmdr.countLanded += 1;
                Game.activeGame.cmdr.Save();
            }

            // update fields
            if (entry.timestamp > body.lastVisited) body.lastVisited = entry.timestamp;
            body.lastTouchdown = entry;

            return true;
        }

        public bool onJournalEntry(CodexEntry entry)
        {
            // track if this is a personal first discovery
            if (Game.activeGame != null)
            {
                var galacticRegion = EliteDangerousRegionMap.RegionMap.FindRegion(this.starPos);
                Game.activeGame.cmdrCodex.trackCodex(entry.Name_Localised, entry.EntryID, entry.timestamp, entry.SystemAddress, entry.BodyID, galacticRegion.Id);
            }

            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }

            var body = this.bodies.FirstOrDefault(_ => _.id == entry.BodyID);
            if (body == null) return false;

            // process geo signals
            if (entry.SubCategory == "$Codex_SubCategory_Geology_and_Anomalies;" && entry.Latitude != 0 && entry.Longitude != 0)
            {
                body.geoSignals ??= new List<SystemGeoSignal>();
                // add if not already present, and not within 25m
                var geoSignal = body.geoSignals.Find(s => s.entryId == entry.EntryID && Util.getDistance(s.location, entry, body.radius) < 25);
                if (geoSignal == null)
                {
                    geoSignal = new SystemGeoSignal()
                    {
                        entryId = entry.EntryID,
                        name = entry.Name,
                        nameLocalized = entry.Name_Localised,
                        location = entry,
                    };
                    body.geoSignals.Add(geoSignal);
                }
            }

            // otherwise, ignore non bio or Notable stellar phenomena entries
            if (entry.SubCategory != "$Codex_SubCategory_Organic_Structures;" || entry.NearestDestination == "$Fixed_Event_Life_Cloud;" || entry.NearestDestination == "$Fixed_Event_Life_Ring;" || entry.Latitude == 0 || entry.Longitude == 0) return false;

            if (entry.BodyID == null)
            {
                // alas, not much we can do if there's no bodyId, but we might have it on the post-processing form?
                if (FormPostProcess.activeForm == null || FormPostProcess.activeForm.lastSystemAddress != entry.SystemAddress) return false;
                entry.BodyID = FormPostProcess.activeForm.lastBodyId;
            }

            if (body!.organisms == null) body.organisms = new List<SystemOrganism>();

            // find by first variant or entryId, then genusName
            var match = Game.codexRef.matchFromEntryId(entry.EntryID);
            var organism = body.findOrganism(match);

            if (organism == null)
            {
                organism = new SystemOrganism()
                {
                    body = body,
                    genus = match.genus.name,
                };
                Game.log($"add organism '{match.variant.name}' ({match.entryId}) to '{body.name}' ({body.id})");
                body.organisms.Add(organism);
                body.organisms = body.organisms.OrderBy(_ => _.genusLocalized).ToList();
                body.bioSignalCount = Math.Max(body.bioSignalCount, body.organisms.Count);
            }

            // update fields
            organism.entryId = entry.EntryID;
            organism.variant = entry.Name;
            organism.variantLocalized = entry.Name_Localised;
            organism.reward = match.species.reward;
            organism.scanned = true;
            if (entry.IsNewEntry)
            {
                organism.isNewEntry = true;
                organism.resetCmdrFirst();
            }

            organism.species ??= match.species.name;
            organism.speciesLocalized ??= match.species.englishName;
            organism.genusLocalized ??= match.genus.englishName;

            Game.log($"CodexEntry: scanned organism: {organism}");

            return true;
        }

        public bool onJournalEntry(ScanOrganic entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            var body = this.bodies.FirstOrDefault(_ => _.id == entry.Body);
            if (body!.organisms == null) body.organisms = new List<SystemOrganism>();

            // find by first variant or entryId, then genusName
            var match = entry.Variant != null
                ? Game.codexRef.matchFromVariant(entry.Variant)
                : Game.codexRef.matchFromSpecies2(entry.Species);

            var organism = body.findOrganism(match);

            if (organism == null)
            {
                organism = new SystemOrganism()
                {
                    body = body,
                    genus = entry.Genus,
                    genusLocalized = entry.Genus_Localized,
                };
                Game.log($"add organism '{entry.Variant_Localised}' ({match.entryId}) to '{body.name}' ({body.id})");
                body.organisms.Add(organism);
                body.organisms = body.organisms.OrderBy(_ => _.genusLocalized).ToList();
                body.bioSignalCount = Math.Max(body.bioSignalCount, body.organisms.Count);
            }

            // update fields
            if (!string.IsNullOrWhiteSpace(entry.Genus_Localized))
                organism.genusLocalized = entry.Genus_Localized;
            if (organism.genusLocalized == null && entry.Variant_Localised != null)
                organism.genusLocalized = Util.getGenusDisplayNameFromVariant(entry.Variant_Localised);

            organism.species = entry.Species;
            organism.speciesLocalized = entry.Species_Localised;
            if (entry.Variant != null) organism.variant = entry.Variant;
            if (entry.Variant_Localised != null) organism.variantLocalized = entry.Variant_Localised;
            organism.entryId = match.entryId;
            organism.reward = match.species.reward;
            organism.scanned = true;

            // upon the 3rd and final scan ...
            if (entry.ScanType == ScanType.Analyse)
            {
                organism.analyzed = true;

                //if (organism.entryId == 0)
                //{
                //    Game.log($"Looking up missing missing entryId for organism '{entry.Variant_Localised ?? entry.Variant}' to '{body.name}' ({body.id})");
                //    // (run synchronously, assuming the network calls were done already)
                //    var pending = Game.codexRef.loadCodexRef();
                //    pending.Wait();
                //    var codexRef = pending.Result;
                //    var match = codexRef.Values.FirstOrDefault(_ => _.name == entry.Variant);

                //    if (match != null)
                //        organism.entryId = long.Parse(match.entryid);
                //    else
                //        Game.log($"No codexRef match found for organism '{entry.Variant_Localised ?? entry.Variant}' to '{body.name}' ({body.id})");
                //}

                // efficiently track which organisms were scanned where
                var cmdr = Game.activeGame?.cmdr;
                if (cmdr != null && organism.entryId > 0)
                {
                    var scannedEntryHash = $"{this.address}_{body.id}_{organism.entryId}_{organism.reward}";
                    var priorScan = cmdr.scannedBioEntryIds.FirstOrDefault(_ => _.StartsWith(scannedEntryHash));
                    if (priorScan != null) cmdr.scannedBioEntryIds.Remove(priorScan);

                    cmdr.scannedBioEntryIds.Add($"{scannedEntryHash}_{body.firstFootFall}");
                    cmdr.reCalcOrganicRewards();
                    cmdr.Save();
                }
                else if (cmdr != null)
                    Game.log($"BAD! Why entryId for organism '{entry.Variant_Localised ?? entry.Variant}' to '{body.name}' ({body.id})");
            }

            return true;
        }

        public bool onJournalEntry(ApproachSettlement entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return false; }
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);

            if (entry.Name.StartsWith("$Ancient") && Game.activeGame != null)
            {
                // update settlements
                body.settlements ??= new Dictionary<string, LatLong2>();
                body.settlements[entry.Name] = entry;

                var siteData = GuardianSiteData.Load(entry);
                // always update the location of the site based on latest journal data
                siteData.location = entry;
                if (siteData != null && siteData.lastVisited < entry.timestamp)
                {
                    siteData.lastVisited = entry.timestamp;
                    siteData.Save();
                }
            }

            return true;
        }

        public void onCanonnPoiData(SystemPoi canonnPoi)
        {
            if (canonnPoi.system != this.name) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {canonnPoi.system}"); return; }
            if (!Game.settings.useExternalData || !Game.settings.useExternalBioData) return;

            // update count of bio signals in bodies
            if (canonnPoi.SAAsignals != null)
            {
                foreach (var signal in canonnPoi.SAAsignals)
                {
                    if (signal.hud_category != "Biology") continue;

                    var signalBodyName = $"{canonnPoi.system} {signal.body}";
                    var signalBody = this.bodies.FirstOrDefault(_ => _.name == signalBodyName);

                    if (signalBody != null && signalBody.bioSignalCount < signal.count)
                    {
                        Game.log($"Updating bioSignalCount from {signalBody.bioSignalCount} to {signal.count} for '{signalBody.name}' ({signalBody.id})");
                        if (signalBody.bioSignalCount < signal.count)
                            signalBody.bioSignalCount = signal.count;
                    }
                }
            }

            // update known organisms
            if (canonnPoi.codex != null)
            {
                var shouldPredictBios = false;

                foreach (var poi in canonnPoi.codex)
                {
                    if (poi.hud_category != "Biology" || poi.latitude == null || poi.longitude == null || poi.entryid == null) continue;

                    var poiBodyName = $"{canonnPoi.system} {poi.body}";
                    var poiBody = this.bodies.FirstOrDefault(_ => _.name == poiBodyName);
                    if (poiBody != null && poi.entryid != null)
                    {
                        var match = Game.codexRef.matchFromEntryId(poi.entryid.Value);
                        var organism = poiBody.findOrganism(match);
                        if (organism == null)
                        {
                            organism = new SystemOrganism()
                            {
                                body = poiBody,
                                genus = match.genus.name,
                                genusLocalized = match.genus.englishName,
                                species = match.species.name,
                                speciesLocalized = match.species.englishName,
                                variant = match.variant.name,
                                variantLocalized = poi.english_name,
                                entryId = poi.entryid.Value,
                                reward = match.species.reward,
                            };
                            Game.log($"add organism '{match.variant.name}' ({match.entryId}) from Canonn POI to '{poiBody.name}' ({poiBody.id})");
                            poiBody.organisms ??= new List<SystemOrganism>();
                            poiBody.organisms.Add(organism);
                            poiBody.organisms = poiBody.organisms.OrderBy(_ => _.genusLocalized).ToList();
                            poiBody.bioSignalCount = Math.Max(poiBody.bioSignalCount, poiBody.organisms.Count);
                        }

                        // update fields
                        organism.entryId = poi.entryid.Value;
                        organism.reward = match.species.reward;

                        // populate these if not already known
                        if (organism.species == null && match.species.name != null) organism.species = match.species.name;
                        if (organism.speciesLocalized == null && match.species.englishName != null) organism.speciesLocalized = match.species.englishName;
                        if (organism.variant == null && match.variant.name != null) organism.variant = match.variant.name;
                        if (organism.variantLocalized == null && match.variant.englishName != null) organism.variantLocalized = match.variant.englishName;

                        shouldPredictBios = true;
                    }
                }

                if (shouldPredictBios)
                {
                    if (Game.activeGame?.systemData == this)
                    {
                        Game.activeGame.predictSystemSpecies();
                        Program.invalidateActivePlotters();
                    }
                }
            }
        }

        public void onCanonnStationData(List<CanonnStation> newStations)
        {
            if (newStations == null || newStations.Count == 0) return;

            // merge update stations with results
            this.stations ??= new List<CanonnStation>();

            foreach (var newStation in newStations)
            {
                var match = this.stations.Find(s => s.marketId == newStation.marketId);

                // add if not seen before
                if (match == null)
                    this.stations.Add(newStation);

                //TODO: merge or replace if CalcMethod is better?
            }

        }

        public void onEdsmResponse(EdsmSystem edsmSystem)
        {
            if (edsmSystem.id64 != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {edsmSystem.name}"); return; }
            if (!Game.settings.useExternalData) return;

            // update bodies from response
            foreach (var entry in edsmSystem.bodies)
            {
                var bodyId = entry.bodyId ?? 0;

                var body = this.findOrCreate(entry.name, bodyId);

                // update fields
                if (body.type == SystemBodyType.Unknown)
                {
                    if (entry.type == "Star")
                        body.type = SystemBodyType.Star;
                    else
                        body.type = SystemBody.typeFrom(null!, entry.subType!, entry.isLandable, entry.name);
                }
                if (body.distanceFromArrivalLS == 0) body.distanceFromArrivalLS = entry.distanceToArrival;
                if (body.semiMajorAxis == 0) body.semiMajorAxis = Util.lsToM(entry.semiMajorAxis ?? 0); // convert from LS to M
                if (body.absoluteMagnitude == 0) body.absoluteMagnitude = entry.absoluteMagnitude;
                if (body.radius == 0 && entry.radius > 0) body.radius = entry.radius * 1000;
                body.planetClass ??= getPlanetClassFromExternal(entry.subType);
                if (body.parents == null && entry.parents != null) body.parents = entry.parents;
                if (!string.IsNullOrEmpty(entry.terraformingState))
                    body.terraformable = entry.terraformingState == "Candidate for terraforming";
                if (body.mass == 0) body.mass = entry.earthMasses > 0 ? entry.earthMasses : entry.solarMasses; // mass
                if (body.surfaceGravity == 0 && entry.gravity > 0) body.surfaceGravity = entry.gravity.Value * 10; // gravity
                if (body.surfaceTemperature == 0 && entry.surfaceTemperature > 0) body.surfaceTemperature = entry.surfaceTemperature;
                if (body.surfacePressure == 0 && entry.surfacePressure > 0) body.surfacePressure = entry.surfacePressure.Value * 100_000f;
                body.atmosphereType ??= getAtmosphereTypeFromExternal(entry.atmosphereType);
                if (body.atmosphere == null && body.atmosphereType == "None") body.atmosphere = "";
                if (body.atmosphere == null && entry.atmosphereType != null) body.atmosphere = entry.atmosphereType;
                if (body.atmosphereComposition == null && entry.atmosphereComposition != null)
                    body.atmosphereComposition = entry.atmosphereComposition.ToDictionary(_ => Util.compositionToCamel(_.Key), _ => _.Value);
                if (body.materials == null && entry.materials != null) body.materials = entry.materials.ToDictionary(_ => _.Key.ToLowerInvariant(), _ => _.Value);
                if (body.volcanism == null && entry.volcanismType != null) body.volcanism = entry.volcanismType == "No volcanism" ? "" : entry.volcanismType;
                if (body.starType == null && entry.type == "Star" && entry.subType != null)
                {
                    body.starType = parseStarType(entry.subType);
                    //body.starSubClass = int.Parse(entry.spectralClass[1]);
                }

                // if EDSM knows about the body (and not because of you)... assume it should be marked as "discovered"
                // (this helps with bodies in the bubble that do not allow "wasDiscovered" to be true
                if (!string.IsNullOrEmpty(entry.discovery?.commander) && entry.discovery.commander.Equals(Game.activeGame?.Commander, StringComparison.OrdinalIgnoreCase) == false)
                    body.wasDiscovered = true;

                if (entry.rings != null)
                {
                    foreach (var newRing in entry.rings)
                    {
                        var ring = body.rings?.FirstOrDefault(_ => _.name == newRing.name);
                        if (ring == null)
                        {
                            Game.log($"add ring '{newRing.name}' from EDSM to '{body.name}' ({body.id})");
                            body.rings ??= new List<SystemRing>();
                            body.rings.Add(new SystemRing()
                            {
                                name = newRing.name,
                                ringClass = newRing.type,
                            });
                        }
                    }
                }
            }
        }

        private static string? getPlanetClassFromExternal(string? externalPlanetClass)
        {
            if (externalPlanetClass == "Metal-rich body")
                return "Metal rich body";
            else
                return externalPlanetClass;
        }

        private static string getAtmosphereTypeFromExternal(string? externalAtmosphereType)
        {
            if (externalAtmosphereType == "No atmosphere" || externalAtmosphereType == null)
            {
                return "None";
            }
            else
            {
                var parts = externalAtmosphereType
                    .Replace("Thin ", "")
                    .Replace("Thick ", "")
                    .Replace("-", " ")
                    .Replace(" atmosphere", "")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1));

                var atmosType = string.Join("", parts);
                return atmosType;
            }
        }

        private string parseStarType(string starType)
        {
            var r = new Regex(@"\((.*)\)", RegexOptions.Compiled);
            var m = r.Match(starType);
            if (m.Groups.Count == 2 && m.Groups[1].Value.Length <= 3)
                return m.Groups[1].Value;
            else if (starType == "T Tauri Star")
                return "TTS";
            else
                return starType[0].ToString();
        }

        public void onSpanshResponse(ApiSystemDump.System spanshSystem)
        {
            if (spanshSystem.id64 != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {spanshSystem.name}"); return; }
            if (!Game.settings.useExternalData) return;

            var shouldPredictBios = false;

            // update bodies from response
            foreach (var entry in spanshSystem.bodies)
            {
                var body = this.findOrCreate(entry.name, entry.bodyId);

                // update fields
                if (body.type == SystemBodyType.Unknown)
                {
                    if (entry.type == "Star")
                        body.type = SystemBodyType.Star;
                    else if (entry.type == "Barycentre")
                        body.type = SystemBodyType.Barycentre;
                    else
                        body.type = SystemBody.typeFrom(null!, entry.subType!, entry.isLandable ?? false, entry.name);
                }

                if (body.distanceFromArrivalLS == 0) body.distanceFromArrivalLS = entry.distanceToArrival ?? 0;
                if (body.semiMajorAxis == 0) body.semiMajorAxis = Util.lsToM(entry.semiMajorAxis ?? 0); // convert from LS to M
                if (body.absoluteMagnitude == 0) body.absoluteMagnitude = entry.absoluteMagnitude ?? 0;
                if (body.radius == 0 && entry.radius != null) body.radius = entry.radius.Value * 1000;
                if (body.parents == null && entry.parents != null) body.parents = entry.parents;
                if (body.planetClass == null) body.planetClass = getPlanetClassFromExternal(entry.subType);
                if (!string.IsNullOrEmpty(entry.terraformingState))
                    body.terraformable = entry.terraformingState == "Candidate for terraforming";
                if (body.mass == 0) body.mass = (entry.earthMasses > 0 ? entry.earthMasses : entry.solarMasses) ?? 0; // mass
                if (body.surfaceGravity == 0 && entry.gravity > 0) body.surfaceGravity = entry.gravity.Value * 10; // gravity
                if (body.surfaceTemperature == 0 && entry.surfaceTemperature > 0) body.surfaceTemperature = entry.surfaceTemperature.Value;
                if (body.surfacePressure == 0 && entry.surfacePressure > 0) body.surfacePressure = entry.surfacePressure.Value * 100_000f;
                if (body.atmosphereType == null) body.atmosphereType = getAtmosphereTypeFromExternal(entry.atmosphereType);
                if (body.atmosphere == null && body.atmosphereType == "None") body.atmosphere = "";
                if (body.atmosphere == null && entry.atmosphereType != null) body.atmosphere = entry.atmosphereType;
                if (body.atmosphereComposition == null && entry.atmosphereComposition != null)
                    body.atmosphereComposition = entry.atmosphereComposition.ToDictionary(_ => Util.compositionToCamel(_.Key), _ => _.Value);
                if (body.materials == null && entry.materials != null) body.materials = entry.materials;
                if (body.volcanism == null && entry.volcanismType != null) body.volcanism = entry.volcanismType == "No volcanism" ? "" : entry.volcanismType;
                if (body.starType == null && entry.subType != null && entry.spectralClass != null)
                {
                    body.starType = parseStarType(entry.subType);
                    //body.starSubClass = int.Parse(entry.spectralClass[1]);
                }

                // if there's a station that is not a Fleet Carrier ... assume it should be marked as "discovered"
                // (this helps with bodies in the bubble that do not allow "wasDiscovered" to be true
                if (entry.stations.Count > 0 && !entry.stations.Any(s => s.primaryEconomy != "Private Enterprise"))
                    body.wasDiscovered = true;

                // update rings
                if (entry.rings != null)
                {
                    foreach (var newRing in entry.rings)
                    {
                        var ring = body.rings?.FirstOrDefault(_ => _.name == newRing.name);
                        if (ring == null)
                        {
                            Game.log($"add ring '{newRing.name}' from Spansh to '{body.name}' ({body.id})");
                            if (body.rings == null) body.rings = new List<SystemRing>();
                            body.rings.Add(new SystemRing()
                            {
                                name = newRing.name,
                                ringClass = newRing.type,
                            });
                        }
                    }
                }

                // update bio counts
                DateTime liveLegacySplitDate = new DateTime(2022, 11, 29);
                if (entry.signals?.signals?.ContainsKey("$SAA_SignalType_Biological;") == true && entry.signals.updateTime > liveLegacySplitDate)
                {
                    var newCount = entry.signals.signals["$SAA_SignalType_Biological;"];
                    if (body.bioSignalCount < newCount)
                    {
                        body.bioSignalCount = newCount;
                        shouldPredictBios = true;
                    }
                }

                // update genus if not already known
                if (entry.signals?.genuses?.Count > 0 && Game.settings.useExternalBioData)
                {
                    foreach (var newGenus in entry.signals.genuses)
                    {
                        var organism = body.organisms?.FirstOrDefault(_ => _.genus == newGenus);
                        if (organism == null)
                        {
                            Game.log($"add organism '{newGenus}' from Spansh to '{body.name}' ({body.id})");
                            if (body.organisms == null) body.organisms = new List<SystemOrganism>();
                            body.organisms.Add(new SystemOrganism()
                            {
                                body = body,
                                genus = newGenus,
                                genusLocalized = BioScan.genusNames[newGenus],
                            });
                            body.organisms = body.organisms.OrderBy(_ => _.genusLocalized).ToList();
                            shouldPredictBios = true;
                        }
                    }
                }

                // and keep track of any space stations
                this.spanshStations = spanshSystem.getAllStations();
            }

            if (shouldPredictBios && Game.ready)
            {
                if (Game.activeGame?.systemData == this)
                {
                    Game.activeGame?.predictSystemSpecies();
                    Program.invalidateActivePlotters();
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return $"'{this.name}' ({this.address}";
        }

        /// <summary>
        /// Returns sum of current exploration rewards for this system
        /// </summary>
        public long sumRewards()
        {
            var sumReward = this.bodies.Sum(_ => _.reward);

            // Maybe this needs the system to be completely undiscovered? (Meaning primary star was not discovered)
            //// FSS completed?
            //if (this.fssAllBodies)
            //    sumReward += this.bodyCount * 1000;

            //// Mapped all valid bodies?
            //var countMappableBodies = this.bodies.Count(_ => _.type == SystemBodyType.LandableBody || _.type == SystemBodyType.SolidBody || _.type == SystemBodyType.Giant);
            //if (this.dssBodyCount == countMappableBodies)
            //    sumReward += countMappableBodies * 10_000;

            return sumReward;
        }

        [JsonIgnore]
        public int fssBodyCount { get => this.bodies.ToList().Count(_ => _.type != SystemBodyType.Asteroid && _.type != SystemBodyType.Unknown && _.type != SystemBodyType.Barycentre && _.type != SystemBodyType.PlanetaryRing); }

        /// <summary> Returns True when all non-star/non-asteroid bodies have been found with FSS </summary>
        [JsonIgnore]
        public bool fssComplete { get => this.fssBodyCount >= this.bodyCount; }

        [JsonIgnore]
        public int dssBodyCount { get => this.bodies.ToList().Count(_ => _.dssComplete); }

        [JsonIgnore]
        public int bioSignalsTotal { get => this.bodies.ToList().Sum(_ => _.bioSignalCount); }

        [JsonIgnore]
        public int bioSignalsRemaining { get => this.bodies.Sum(_ => _.bioSignalCount - _.countAnalyzedBioSignals); }

        public List<string> getDssRemainingNames()
        {
            var names = new List<string>();
            var ordered = this.bodies.OrderBy(_ => _.id);
            foreach (var _ in ordered)
            {
                // skip things already scanned
                if (_.dssComplete) continue;

                // skip anything except Scannable planets
                if (_.type != SystemBodyType.Giant && _.type != SystemBodyType.SolidBody && _.type != SystemBodyType.LandableBody) continue;

                // inject rings
                if (!Game.settings.skipRingsDSS && _.hasRings)
                {
                    if (_.rings.Count > 0)
                        names.Add(_.shortName + "rA");
                    if (_.rings.Count > 0)
                        names.Add(_.shortName + "rB");
                }

                // optionally skip gas giants
                if (Game.settings.skipGasGiantDSS && _.type == SystemBodyType.Giant) continue;

                // optionally skip low value bodies
                if (Game.settings.skipLowValueDSS && _.rewardEstimate < Game.settings.skipLowValueAmount) continue;

                // optionally skip anything too far away
                if (Game.settings.skipHighDistanceDSS && _.distanceFromArrivalLS > Game.settings.skipHighDistanceDSSValue)
                    continue;

                names.Add(_.shortName);
            }

            return names;
        }

        public List<string> getBioRemainingNames()
        {
            var names = this.bodies
                .OrderBy(_ => _.id)
                .Where(_ => _.countAnalyzedBioSignals < _.bioSignalCount)
                .Select(_ => _.shortName)
                .ToList();

            return names;
        }

        /// <summary>
        /// A raw count of non-body signals, includes asteroids, FCs and many other things
        /// </summary>
        [JsonIgnore]
        public int rawNonBodyCount;

        /// <summary>
        /// A count of non-body signals excluding just asteroids
        /// </summary>
        [JsonIgnore]
        public int nonBodySignalCount
        {
            get
            {
                var count = rawNonBodyCount;
                // remove the count of Asteroids
                count -= this.bodies.ToList().Count(b => b.type == SystemBodyType.Asteroid);
                // remove non-interesting signal types - no Stations/Outposts or NavBeacons
                count -= this.discoveredSignals.Values.Count(s => s.IsStation == true || s.SignalType == "Outpost" || s.SignalType == "NavBeacon");
                return count;
            }
        }

        /// <summary>
        /// A raw count of non-body signals, includes asteroids, FCs and many other things
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, FSSSignalDiscovered> discoveredSignals = new Dictionary<string, FSSSignalDiscovered>();

        [JsonIgnore]
        public SystemBody? lastFssBody;

        [JsonIgnore]
        private List<SystemSettlementSummary>? _settlements;

        [JsonIgnore]
        public List<SystemSettlementSummary> settlements
        {
            get
            {
                if (this._settlements == null)
                    this.prepSettlements();

                return this._settlements!;
            }
        }

        public void prepSettlements()
        {
            if (!Game.settings.enableGuardianSites) return;

            var sites = GuardianSitePub.Find(this.name);
            Game.log($"prepSettlements: for: '{this.name}' ({this.address}), sites.Count: {sites.Count}");
            this._settlements = new List<SystemSettlementSummary>();
            foreach (var site in sites)
            {
                if (site.isRuins)
                {
                    var body = this.bodies.FirstOrDefault(_ => _.name == site.bodyName)!;
                    if (body != null)
                    {
                        var ruins = SystemSettlementSummary.forRuins(this, body, site.idx);
                        if (ruins != null)
                            this._settlements.Add(ruins);
                    }
                }
                else
                {
                    var body = this.bodies.FirstOrDefault(_ => _.name == site.bodyName)!;
                    if (body != null)
                    {
                        var structure = SystemSettlementSummary.forStructure(this, body);
                        if (structure != null)
                            this._settlements.Add(structure);
                    }
                    else
                        Game.log($"Why no body yet for: '{site.bodyName}'");
                }
            }
        }

        public static DateTimeOffset lastDssCompleteAt;

        /// <summary>
        /// Returns True if we completed some DSS scan within the duration from setting keepBioPlottersVisibleDuration
        /// </summary>
        public static bool isWithinLastDssDuration()
        {
            var duration = DateTimeOffset.UtcNow - SystemData.lastDssCompleteAt;
            var withinDuration = duration.TotalSeconds < Game.settings.keepBioPlottersVisibleDuration;
            return withinDuration;
        }

        //public string? getParentStarType(SystemBody body, bool flatten)
        //{
        //    var parents2 = (BodyParents)body.parents;
        //    var parentStarBodyId = body.parents.Find(_ => _.ContainsKey(ParentBodyType.Star))?.Values.First();
        //    if (parentStarBodyId != null)
        //    {
        //        var parentStar = this.bodies.Find(_ => _.id == parentStarBodyId);
        //        var parentStarClass = parentStar?.starType;

        //        // or simply force the primary star?
        //        if (parentStarClass == null)
        //            parentStarClass = this.bodies.Find(_ => _.id == 0 || _.starType != null)?.starType; // ??

        //        // flatten any of the White Dwarf types down to just "D"
        //        if (flatten && parentStarClass?.StartsWith("D") == true) parentStarClass = "D";


        //        return parentStarClass;
        //    }
        //    //var ugh = body.parents[0].Values.First();
        //    var nameMatch = this.bodies.Find(_ => body.name.StartsWith(_.name));
        //    return nameMatch?.starType;

        //    //// or simply force the primary star?
        //    //var foo = this.bodies.Find(_ => _.id == 0 || _.starType != null)?.starType; // ??
        //    //return foo;
        //}

        //public List<string> getParentStarTypes(SystemBody body, bool flatten)
        //{
        //    var parents2 = (BodyParents)body.parents;
        //    var parentStars = new List<SystemBody>();

        //    // walk the list of parents for the given body
        //    foreach (var _ in body.parents)
        //    {
        //        if (_.First().Key == ParentBodyType.Star)
        //        {
        //            var parentBodyId = _.First().Value;
        //            var parentStar = this.bodies.Find(_ => _.id == parentBodyId);

        //            if (parentStar == null) throw new Exception("Why no parent star?");
        //            parentStars.Add(parentStar);

        //            //var starType = parentStar.starType!;
        //            // flatten any of the White Dwarf types down to just "D"
        //            //if (flatten && starType.StartsWith("D") == true) starType = "D";
        //            //parentStarTypes.Add(starType);
        //        }
        //        else if (_.First().Key == ParentBodyType.Null) // BaryCenter
        //        {
        //            var parentBodyId = _.First().Value;

        //            var hits = getBodiesByParent(parentBodyId)
        //                .Where(b => b.type == SystemBodyType.Star);

        //            if (hits?.Count() > 0)
        //            {
        //                parentStars.AddRange(hits);
        //                //parentStarTypes.AddRange(hits.Select(_ => _.starType));
        //            }
        //        }
        //    }

        //    var parentStarTypes = parentStars
        //        .ToHashSet()
        //        .Select(_ =>
        //        {
        //            if (_.starType == null) throw new Exception("Why no parent starType?");

        //            if (flatten && _.starType.StartsWith("D") == true)
        //                return "D";
        //            else
        //                return _.starType;
        //        })
        //        .ToList();

        //    return parentStarTypes;
        //}

        public HashSet<SystemBody> getParentStars(SystemBody body, bool onlyFirst)
        {
            var parentStars = new HashSet<SystemBody>();
            if (body.parents == null) return parentStars;

            // walk the list of parents for the given body
            foreach (var _ in body.parents)
            {
                if (_.type == ParentBodyType.Star)
                {
                    var parentBodyId = _.id;
                    var parentStar = this.bodies.Find(_ => _.id == parentBodyId);

                    if (parentStar == null) throw new Exception("Why no parent star?");
                    parentStars.Add(parentStar);
                    //break;

                    //var starType = parentStar.starType!;
                    // flatten any of the White Dwarf types down to just "D"
                    //if (flatten && starType.StartsWith("D") == true) starType = "D";
                    //parentStarTypes.Add(starType);
                }
                else if (_.type == ParentBodyType.Null) // BaryCenter
                {
                    var parentBodyId = _.id;

                    var hits = getStarsByParent(parentBodyId);
                    //.Where(b => b.type == SystemBodyType.Star);

                    if (hits?.Count > 0)
                        foreach (var hit in hits)
                            parentStars.Add(hit);
                }

                if (onlyFirst && parentStars.Count > 0)
                    break;
            }

            return parentStars;
        }

        public List<string> getParentStarTypes(SystemBody body, bool onlyFirst)
        {
            var parentStars = this.getParentStars(body, onlyFirst);

            var parentStarTypes = parentStars
                .Select(_ =>
                {
                    //if (_.starType == null) Debugger.Break(); //throw new Exception("Why no parent starType?");
                    return Util.flattenStarType(_.starType);
                })
                .ToList();

            return parentStarTypes;
        }

        public string getBrightestParentStarType(SystemBody body)
        {
            var parentStars = this.getParentStars(body, false);

            SystemBody chosenStar = null!;
            double maxValue = -1;
            foreach (var star in parentStars)
            {
                var dist = body.distanceFromArrivalLS - star.distanceFromArrivalLS;
                var dist2 = Math.Pow(dist, 2);
                var distMag = dist2.ToString().Length; // Really just using the magnitude not the actual number
                var relativeHeat = star.surfaceTemperature / distMag;

                if (relativeHeat > maxValue)
                {
                    maxValue = relativeHeat;
                    chosenStar = star;
                }
            }

            var starType = chosenStar.starType!;

            return Util.flattenStarType(starType);
        }

        public HashSet<SystemBody> getStarsByParent(int bodyId)
        {
            var stars = new HashSet<SystemBody>();

            // for every star in the system
            foreach (var body in this.bodies)
            {
                //if (body.parents == null) continue; // || body.type != SystemBodyType.Star) continue;
                if (body.parents == null || body.type != SystemBodyType.Star) continue;

                if (body.id == bodyId)
                    stars.Add(body);
                else if (body.hasParent(bodyId))
                    stars.Add(body);

                //var newValue = body.hasParent(bodyId);
                //var oldValue = body.parents.Any(_ => _.id == bodyId);
                //if (newValue != oldValue) Debugger.Break();

                //// walk the parents of that star
                //foreach (var parent in body.parents)
                //{
                //    var parentType = parent.type; // .Keys.First();
                //    var parentBodyId = parent.id; // .Values.First();

                //    if (parentBodyId == bodyId)
                //        stars.Add(body);
                //    else if (parentType == ParentBodyType.Null)
                //        continue;

                //    break;
                //}
            }

            // .Where(b => b.type == SystemBodyType.Star)
            return stars;
        }

        public long getMinBioRewards(bool applyFF) { return this.bodies.Sum(b => applyFF && b.firstFootFall ? b.minBioRewards * 5 : b.minBioRewards); }

        public long getMaxBioRewards(bool applyFF) { return this.bodies.Sum(b => applyFF && b.firstFootFall ? b.maxBioRewards * 5 : b.maxBioRewards); }

        [JsonIgnore]
        public double nebulaDist { get; private set; }

        public async Task<double> getNebulaDist()
        {
            this.nebulaDist = await Game.codexRef.getDistToClosestNebula(this.starPos);
            return nebulaDist;
        }

        public CanonnStation? getStation(long marketId)
        {
            return this.stations?.Find(s => s.marketId == marketId);
        }

        [JsonIgnore]
        public List<ApiSystemDump.System.Station>? spanshStations;

        [JsonIgnore]
        public string folderImages => Path.Combine(Game.settings.screenshotTargetFolder!, this.name);
    }

    internal class SummaryGenus
    {
        public BioGenus bioRef;
        public string displayName;

        public List<SummarySpecies> species = new List<SummarySpecies>();
        public List<SummarySpecies> predictions = new List<SummarySpecies>();

        public SummaryGenus(BioGenus bioRef, string displayName)
        {
            this.bioRef = bioRef;
            this.displayName = displayName;
        }

        public string name { get => bioRef.name; }

        public SummarySpecies findSpecies(string speciesName, string displayName, bool prediction = false)
        {
            var species = prediction
                ? this.predictions.Find(_ => _.name == speciesName)
                : this.species.Find(_ => _.name == speciesName);

            if (species == null)
            {
                var speciesRef = this.bioRef.matchSpecies(speciesName);
                //if (speciesRef == null) speciesRef = Game.codexRef.matchFromSpecies(speciesName); // TMP!!!
                if (speciesRef == null) throw new Exception($"Unknown species: '{speciesName}' ?!");

                species = new SummarySpecies(speciesRef, displayName) { predicted = prediction };

                if (prediction)
                    this.predictions.Add(species);
                else
                    this.species.Add(species);
            }

            return species;
        }

        public bool hasBody(SystemBody body)
        {
            return
                this.species.Any(_ => _.bodies.Contains(body))
                || this.predictions.Any(_ => _.bodies.Contains(body));
        }
    }

    internal class SummarySpecies
    {
        public BioSpecies bioRef;
        public string displayName;
        public HashSet<SystemBody> bodies = new HashSet<SystemBody>();
        public bool predicted;

        public SummarySpecies(BioSpecies bioRef, string displayName)
        {
            this.bioRef = bioRef;
            this.displayName = displayName;
        }

        public override string ToString()
        {
            return $"{this.bioRef.genus.englishName} {this.displayName} (guessed: {this.predicted}) {Util.credits(this.reward, true)}";
        }

        public string name { get => bioRef.name; }
        public long reward { get => bioRef.reward; }
    }

    internal class SummaryBody
    {
        public SystemBody body;
        public long minReward;
        public long maxReward;

        public List<SummarySpecies> species = new List<SummarySpecies>();

        public SummaryBody(SystemBody body)
        {
            this.body = body;
        }

        public long getMaxEstimatedRewardForGenus(string genusName, bool minNotMax)
        {
            var reward = minNotMax ? 50_000_000 : -1L;

            foreach (var foo in this.species)
            {
                if (foo.bioRef.genus.name != genusName) continue;
                if (minNotMax && foo.reward < reward)
                    reward = foo.reward;
                else if (!minNotMax && foo.reward > reward)
                    reward = foo.reward;
            }

            if (minNotMax && reward == 50_000_000)
                return 0;
            else
                return reward;
        }
    }

    internal class SystemBody
    {
        public static SystemBodyType typeFrom(string starType, string planetClass, bool landable, string bodyName)
        {
            // choose type
            if (landable)
                return SystemBodyType.LandableBody;
            else if (!string.IsNullOrEmpty(starType))
                return SystemBodyType.Star;
            else if (bodyName.Contains("cluster", StringComparison.OrdinalIgnoreCase))
                return SystemBodyType.Asteroid;
            else if (bodyName.EndsWith("Ring", StringComparison.OrdinalIgnoreCase))
                return SystemBodyType.PlanetaryRing;
            else if (string.IsNullOrEmpty(starType) && string.IsNullOrEmpty(planetClass))
                return SystemBodyType.Barycentre;
            else if (planetClass?.Contains("giant", StringComparison.OrdinalIgnoreCase) == true)
                return SystemBodyType.Giant;
            else
                return SystemBodyType.SolidBody;
        }

        #region data members

        /// <summary> BodyName - the full body name, typically with the system name embedded </summary>
        public string name;

        /// <summary> BodyId - id relative within the star system </summary>
        public int id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int reward;

        /// <summary> Is this a star, gas-giant, or landable body, etc </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SystemBodyType type;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double distanceFromArrivalLS;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double semiMajorAxis;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double absoluteMagnitude;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? starType;

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        //public int? starSubClass;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? planetClass;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool terraformable;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double mass;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public decimal radius;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double surfaceGravity;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double surfaceTemperature;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public double surfacePressure;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string atmosphere;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string atmosphereType;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, float> atmosphereComposition;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? volcanism;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, float> materials;

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        //public Dictionary<string, float> solidComposition;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool wasDiscovered;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool wasMapped;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool scanned;

        //  "Parents":[ {"Ring":3}, {"Star":1}, {"Null":0} ]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public BodyParents parents;

        /// <summary> Has a DSS been done on this body? </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool dssComplete;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset firstVisited;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset lastVisited;

        /// <summary> Location of last touchdown </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public LatLong2? lastTouchdown;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<SystemRing> rings;

        /// <summary> Locations of named bookmarks on this body </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, List<LatLong2>>? bookmarks;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int bioSignalCount;

        /// <summary> All the organisms for this body </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<SystemOrganism>? organisms;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int geoSignalCount;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<SystemGeoSignal> geoSignals;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool firstFootFall;

        /// <summary> A of settlements known on this body. </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, LatLong2>? settlements;

        /// <summary> Locations of all bio scans or Codex scans performed on this body </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<BioScan>? bioScans;

        #endregion

        public override string ToString()
        {
            return $"'{this.name}' ({this.id}, {this.type} {this.starType})";
        }

        [JsonIgnore]
        public SystemData system;

        [JsonIgnore]
        public string shortName;

        [JsonIgnore]
        public bool hasRings { get => this.rings?.Count > 0; }

        [JsonIgnore]
        public bool hasValue { get => valuables.Contains(this.type); }
        private readonly SystemBodyType[] valuables = { SystemBodyType.Star, SystemBodyType.LandableBody, SystemBodyType.Giant, SystemBodyType.SolidBody };

        [JsonIgnore]
        public bool isMainStar
        {
            get
            {
                if (this.type != SystemBodyType.Star) return false;
                if (this.id == 0 || this.name.EndsWith("A")) return true;
                return false;
            }
        }

        public bool hasParent(int targetBodyId)
        {
            // must be direct parent, or have only barycenters between it and the target
            foreach (var parent in parents)
            {
                if (parent.id == targetBodyId)
                    return true;
                if (parent.type != ParentBodyType.Null)
                    return false;
            }

            return false;
        }

        [JsonIgnore]
        public List<SystemBody> parentBodies
        {
            get => this.parents
                .Select(p => this.system.bodies.FirstOrDefault(b => b.id == p.id))
                .Where(b => b != null)
                .ToList()!;
        }

        public double getRelativeBrightness(double bodyDistanceFromArrivalLS)
        {
            if (this.starType == null) return 0;

            var dist = bodyDistanceFromArrivalLS - this.distanceFromArrivalLS;
            var dist2 = Math.Pow(dist, 2);
            var distMag = dist2.ToString().Length; // Really just using the magnitude not the actual number
            var relativeHeat = this.surfaceTemperature / distMag;

            return relativeHeat;
        }

        public double sumSemiMajorAxis(int targetBodyId)
        {
            // start with our own distance, then sum our parents until we reach (and include) the target
            var dist = this.semiMajorAxis;

            foreach (var parent in this.parentBodies)
            {
                dist += parent.semiMajorAxis;
                if (parent.id == targetBodyId) break;
            }

            return dist;
        }

        [JsonIgnore]
        public int countAnalyzedBioSignals
        {
            get => this.organisms == null
                ? 0
                : this.organisms.Count(_ => _.analyzed);
        }

        [JsonIgnore]
        public Dictionary<string, BioVariant> predictions = new Dictionary<string, BioVariant>();

        [JsonIgnore]
        public List<SystemGenusPrediction> genusPredictions = new List<SystemGenusPrediction>();

        [JsonIgnore]
        public long minBioRewards;

        [JsonIgnore]
        public long maxBioRewards;

        public string getMinMaxBioRewards(bool applyFF)
        {
            return Util.getMinMaxCredits(
                applyFF && this.firstFootFall ? this.minBioRewards * 5 : this.minBioRewards,
                applyFF && this.firstFootFall ? this.maxBioRewards * 5 : this.maxBioRewards);
        }

        /// <summary>
        /// TODO: retire!
        /// </summary>
        [JsonIgnore]
        public long sumPotentialEstimate
        {
            get
            {
                if (this.organisms == null) return 0;

                long estimate = 0;
                foreach (var organism in this.organisms)
                {
                    // use the reward if we know it, otherwise pick the lowest from the species
                    var reward = organism.reward;
                    if (reward == 0)
                    {
                        if (organism.species != null) throw new Exception($"Why no reward if we have a species?");

                        var genusRef = Game.codexRef.matchFromGenus(organism.genus);
                        reward = genusRef?.species.Min(_ => _.reward) ?? 0;
                    }
                    if (reward == 0) throw new Exception($"Why no reward?");

                    if (this.firstFootFall) reward *= 5;
                    estimate += reward;
                }

                return estimate;
            }
        }

        [JsonIgnore]
        public long sumAnalyzed { get => this.organisms?.Sum(o => !o.analyzed ? 0 : this.firstFootFall ? o.reward * 5 : o.reward) ?? 0; }

        /// <summary>
        /// Exploration reward estimate for body
        /// </summary>
        [JsonIgnore]
        public double rewardEstimate
        {
            get
            {
                var estimate = Util.GetBodyValue(
                    this.planetClass, // planetClass
                    this.terraformable, // isTerraformable
                    this.mass, // mass
                    !this.wasDiscovered, // isFirstDiscoverer
                    true, // assume yes - that's why this is an estimate :)
                          // this.dssComplete, // isMapped
                    !this.wasMapped // isFirstMapped
                );

                return estimate;
            }
        }

        public SystemOrganism? findOrganism(BioMatch match)
        {
            return this.findOrganism(match.variant?.name, match.entryId, match.genus.name);
        }

        public SystemOrganism? findOrganism(string? variant, long entryId, string genus)
        {
            var organism = this.organisms?.FirstOrDefault(_ => _.variant == variant || _.entryId == entryId);
            if (organism == null) organism = this.organisms?.FirstOrDefault(_ => _.genus == genus);

            // some organisms have 2+ species on the same planet, eg: Brain Tree's
            if (organism?.variant != null && organism.variant != variant)
            {
                // if we found something but the variant name is populated and different - it is not a valid match
                return null;
            }

            return organism;
        }

        public void predictSpecies()
        {
            if (this.bioSignalCount == 0 || Game.activeGame == null || !Game.ready) return;
            Game.log($"predictSpecies: '{this.name}'...");

            this.predictions.Clear();
            var knownRewards = 0L;

            // predict valid species within the genus ...
            if (this.organisms?.Count > 0)
            {
                foreach (var org in this.organisms)
                {
                    org.lookupMissingNamesIfNeeded();

                    // if we know the species, keep its reward value and move on
                    if (org.species != null)
                    {
                        knownRewards += /*this.firstFootFall ? o.reward * 5 :*/ org.reward;
                        continue;
                    }

                    // otherwise predict within the genus
                    var genusPredictions = BioPredictor.predict(this);
                    foreach (var speciesName in genusPredictions)
                    {
                        var match = Game.codexRef.matchFromVariantDisplayName(speciesName);
                        if (match != null)
                            this.predictions[speciesName] = match.variant;
                    }
                }
            }

            // predict the whole body if there are signals unaccounted for
            var delta = this.bioSignalCount;
            if (this.organisms?.Count > 0)
                delta -= this.organisms.Count(o => o.species != null);
            if (delta > 0)
            {
                var bodyPredictions = BioPredictor.predict(this);
                foreach (var speciesName in bodyPredictions)
                {
                    var match = Game.codexRef.matchFromVariantDisplayName(speciesName);
                    if (match == null) continue;
                    if (this.organisms != null)
                    {
                        // skip if a species is already known for this genus
                        if (this.organisms.Any(o => o.genus == match.genus.name && o.species != null)) continue;
                        // skip if genus are all known and this isn't one of them
                        if (this.organisms.Count == this.bioSignalCount && !this.organisms.Any(o => o.genus == match.genus.name)) continue;
                    }

                    this.predictions[speciesName] = match.variant;
                }
            }

            // after predictions, figure out what the min/max rewards are
            this.minBioRewards = knownRewards;
            this.maxBioRewards = knownRewards;
            if (this.predictions.Count > 0)
            {
                var sortedPredictions = this.predictions.OrderBy(_ => _.Value.reward).Select(_ => _.Value).ToList();
                this.minBioRewards += this.getShortListSum(delta, sortedPredictions, true);
                this.maxBioRewards += this.getShortListSum(delta, sortedPredictions, false);
            }

            Game.log($"predictSpecies: '{this.name}' predicted {predictions.Count} rewards: {this.getMinMaxBioRewards(false)}");
            if (predictions.Count > 0) Game.log("\r\n> " + string.Join("\r\n> ", this.predictions.Keys.Select(k => $"{k} ({predictions[k].entryId})")));

            // bucket by genus // here!
            var game = Game.activeGame;
            this.genusPredictions.Clear();
            foreach (var variant in this.predictions.Values)
            {
                var genusPrediction = genusPredictions.Find(g => g.genus == variant.species.genus);
                if (genusPrediction == null)
                {
                    genusPrediction = new SystemGenusPrediction(variant.species.genus);
                    genusPredictions.Add(genusPrediction);
                }

                if (!genusPrediction.species.ContainsKey(variant.species)) genusPrediction.species[variant.species] = new List<SystemVariantPrediction>();

                // is this something we've not seen yet?
                var isCmdrNew = !game.cmdrCodex.isDiscovered(variant.entryId);
                if (isCmdrNew) genusPrediction.hasCmdrNew = true;
                var isCmdrRegionalNew = !game.cmdrCodex.isDiscoveredInRegion(variant.entryId, game.cmdr.galacticRegion);
                if (isCmdrRegionalNew) genusPrediction.hasCmdrRegionalNew = true;

                // is this something no one has ever seen?
                var isRegionalNew = Game.codexRef.isRegionalNewDiscovery(game.cmdr.galacticRegion, variant.entryId);
                if (isRegionalNew) genusPrediction.hasRegionalNew = true;

                var variantPrediction = new SystemVariantPrediction(variant, isRegionalNew, isCmdrNew, isCmdrRegionalNew);
                genusPrediction.species[variant.species].Add(variantPrediction);

                if (variant.species.reward < genusPrediction.min) genusPrediction.min = variant.reward;
                if (variant.species.reward > genusPrediction.max) genusPrediction.max = variant.reward;
            }

            //Program.defer(() =>
            //{
            //    FormShowCodex.update();
            //    FormGenus.activeForm?.populateGenus();
            //    Program.invalidateActivePlotters();
            //    FormPredictions.refresh();
            //});
        }

        private long getShortListSum(int delta, List<BioVariant> sortedPredictions, bool minNotMax)
        {
            var idx = minNotMax ? 0 : sortedPredictions.Count - 1;

            var list = new List<BioVariant>();
            while (list.Count < delta && idx >= 0 && idx < sortedPredictions.Count)
            {
                var next = sortedPredictions[idx];

                // add if that genus is not already present
                var genusMatch = list.Find(_ => _.species.genus == next.species.genus);
                if (genusMatch == null)
                {
                    list.Add(next);
                }
                else if ((minNotMax && next.reward < genusMatch.reward) || (!minNotMax && next.reward > genusMatch.reward))
                {
                    list.Remove(genusMatch);
                    list.Add(next);
                }

                if (minNotMax)
                    idx++;
                else
                    idx--;
            }

            return list.Sum(_ => _.reward);
        }

        /// <summary>
        /// Return the min or max predicted reward value for the given genus on this body
        /// </summary>
        public long getBioRewardForGenus(SystemOrganism org, bool minNotMax)
        {
            // if we know the species reward already - return that number
            if (org.reward > 0) return org.reward;

            // otherwise figure it out from the predictions
            var reward = minNotMax ? 50_000_000 : -1L;

            foreach (var bioRef in this.predictions.Values)
            {
                if (bioRef.species.genus.name != org.genus) continue;

                if ((minNotMax && bioRef.reward < reward) || (!minNotMax && bioRef.reward > reward))
                    reward = bioRef.reward;
            }

            if (minNotMax && reward == 50_000_000)
                return 0;
            else
                return reward;
        }

        /// <summary> A distinct list of geo signal names </summary>
        public HashSet<string> geoSignalNames
        {
            get => this.geoSignals?.Select(_ => _.nameLocalized).ToHashSet() ?? new();
        }
    }

    internal class SystemRing
    {
        public string name;
        public string ringClass;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool dssComplete;

        public override string ToString()
        {
            return $"'{this.name}' ({this.ringClass})";
        }

        public static string decode(string ringClass)
        {
            switch (ringClass)
            {
                case "Rocky":
                case "eRingClass_Rocky":
                    return "Rocky";
                case "Metallic":
                case "eRingClass_Metalic":
                    return "Matalic";

                case "Metal Rich":
                case "eRingClass_MetalRich":
                    return "Metal rich";

                case "Icy":
                case "eRingClass_Icy":
                    return "Icy";

                default: throw new Exception($"Unexpected: {ringClass}");
            }
        }
    }

    internal class SystemOrganism
    {
        public string genus;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string genusLocalized;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? species;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? speciesLocalized;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? variant;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string? variantLocalized;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long entryId;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long reward;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        /// <summary> We have scanned this 3 times on foot </summary>
        public bool analyzed;
        /// <summary> We have scanned this using the composition scanner </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool scanned;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool isNewEntry;

        [JsonIgnore]
        public SystemBody body;

        [JsonIgnore]
        private bool? _cmdrFirst;

        public override string ToString()
        {
            return $"{this.speciesLocalized ?? this.species ?? this.genusLocalized ?? this.genus} ({this.entryId})";
        }

        /// <summary>
        /// Returns true if this is a cmdr first discovery
        /// </summary>
        [JsonIgnore]
        public bool isCmdrFirst
        {
            get
            {
                // look-up entryId if we don't already know it
                if (this.entryId == 0 && this.variant != null)
                {
                    var match = Game.codexRef.matchFromVariant(this.variant);
                    this.entryId = long.Parse(match.variant.entryId, CultureInfo.InvariantCulture);
                }

                if (this._cmdrFirst == null && this.entryId > 0)
                    this._cmdrFirst = Game.activeGame?.cmdrCodex?.isPersonalFirstDiscovery(this.entryId, this.body.system.address, this.body.id);

                return this._cmdrFirst ?? false;
            }
        }

        public void resetCmdrFirst()
        {
            // reset any instance of this same thing on any body in this system
            var similarOrgs = this.body.system.bodies
                .SelectMany(b => (b.organisms?.Where(o => o.entryId == this.entryId).ToList() ?? new()))
                .ToList();

            similarOrgs
                .ForEach(o => o._cmdrFirst = null);
        }

        /// <summary>
        /// Returns true if this is a regional or cmdr first discovery
        /// </summary>
        [JsonIgnore]
        public bool isFirst { get => (this.isNewEntry && Game.settings.highlightRegionalFirsts) || this.isCmdrFirst; }

        [JsonIgnore]
        public string prefix
        {
            get
            {
                //if (this.isRegionalNew) ?? 
                //    return "☀ ";
                if (this.isCmdrFirst)
                    return "⚑ ";
                else if (this.isNewEntry)
                    return "⚐ ";
                else
                    return "";
            }
        }

        [JsonIgnore]
        public VolColor volCol
        {
            get
            {
                if (this.isFirst)
                    return VolColor.Gold;
                else
                    return VolColor.Orange;
            }
        }

        [JsonIgnore]
        public int range { get => BioScan.getRange(this.genus); }

        public void lookupMissingNamesIfNeeded()
        {
            if (this.entryId == 0) return;

            // look-up species name if we don't already know it
            if (this.species == null)
            {
                var match = Game.codexRef.matchFromEntryId(this.entryId).species;
                this.species = match.name;
                this.speciesLocalized = match.englishName;

                if (this.reward == 0)
                    this.reward = match.reward;
            }

            // look-up variant name if we don't already know it
            if (this.variantLocalized == null)
            {
                var match = Game.codexRef.matchFromEntryId(this.entryId).variant;
                this.variantLocalized = match.englishName;
            }
        }
    }

    internal class SystemGeoSignal
    {
        public long entryId;
        public string name;
        public string nameLocalized;
        public LatLong2 location;
    }

    internal class SystemGenusPrediction
    {
        public BioGenus genus;
        public Dictionary<BioSpecies, List<SystemVariantPrediction>> species;
        public long min = 50_000_000;
        public long max = 0;
        public bool hasRegionalNew;
        public bool hasCmdrNew;
        public bool hasCmdrRegionalNew;

        public SystemGenusPrediction(BioGenus genus)
        {
            this.genus = genus;
            this.species = new Dictionary<BioSpecies, List<SystemVariantPrediction>>();
        }

        public bool isGold { get => hasCmdrNew || (Game.settings.highlightRegionalFirsts && hasCmdrRegionalNew); }

        [JsonIgnore]
        public VolColor volCol
        {
            get
            {
                if (this.isGold)
                    return VolColor.Gold;
                else
                    return VolColor.Orange;
            }
        }

        public string prefix
        {
            get
            {
                if (this.hasRegionalNew)
                    return "☀ ";
                else if (this.hasCmdrNew)
                    return "⚑ ";
                else if (this.hasCmdrRegionalNew)
                    return "⚐ ";
                else
                    return "";
            }
        }
    }

    internal class SystemVariantPrediction
    {
        public readonly BioVariant variant;
        public readonly bool isRegionalNew;
        public readonly bool isCmdrNew;
        public readonly bool isCmdrRegionalNew;

        public SystemVariantPrediction(BioVariant variant, bool isRegionalNew, bool isCmdrNew, bool isCmdrRegionalNew)
        {
            this.variant = variant;
            this.isRegionalNew = isRegionalNew;
            this.isCmdrNew = isCmdrNew;
            this.isCmdrRegionalNew = isCmdrRegionalNew;
        }

        public bool isGold { get => isCmdrNew || (Game.settings.highlightRegionalFirsts && isCmdrRegionalNew); }

        public string prefix
        {
            get
            {
                if (this.isRegionalNew)
                    return "☀ ";
                else if (this.isCmdrNew)
                    return "⚑ ";
                else if (this.isCmdrRegionalNew)
                    return "⚐ ";
                else
                    return "";
            }
        }
    }
}
