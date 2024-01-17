using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.canonn;
using SrvSurvey.net;
using SrvSurvey.net.EDSM;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal class SystemData : Data
    {
        #region load, close and caching

        private static Dictionary<long, SystemData> cache = new Dictionary<long, SystemData>();

        /// <summary>
        /// Open or create a SystemData object for the a star system by name or address.
        /// </summary>
        private static SystemData? Load(string systemName, long systemAddress, string fid)
        {
            Game.log($"Loading SystemData for: '{systemName}' ({systemAddress})");
            // use cache entry if present
            if (cache.ContainsKey(systemAddress))
            {
                return cache[systemAddress];
            }

            // try finding files by systemAddress first, then system name
            var folder = Path.Combine(Program.dataFolder, "systems", fid);
            Directory.CreateDirectory(folder);
            var files = Directory.GetFiles(folder, $"*_{systemAddress}.json");
            if (files.Length == 0)
            {
                files = Directory.GetFiles(folder, $"{systemName}_*.json");
            }

            // create new if no matches found
            if (files.Length == 0)
            {
                Game.log($"Nothing found for: '{systemName}' ({systemAddress})");
                return null;
            }
            else if (files.Length > 1)
            {
                Game.log($"Why are there {files.Length} multiple files for '{systemName}' ({systemAddress})? Using the first one.");
            }

            var filepath = files[0];
            cache[systemAddress] = Data.Load<SystemData>(filepath)!;
            return cache[systemAddress];
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

        public static SystemData From(ISystemDataStarter entry)
        {
            lock (cache)
            {
                // load from file or cache
                var fid = Game.activeGame!.fid!;
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
                        commander = Game.activeGame.cmdr.commander,
                        firstVisited = DateTimeOffset.Now,
                        lastVisited = DateTimeOffset.Now,
                    };

                    // only add this body if we know it's a star
                    if (entry.BodyType == BodyType.Star)
                    {
                        var mainStar = new SystemBody()
                        {
                            id = entry.BodyID,
                            name = entry.Body,
                            type = SystemBodyType.Star,
                            firstVisited = DateTimeOffset.Now,
                            lastVisited = DateTimeOffset.Now,
                        };
                        data.bodies.Add(mainStar);
                    }

                    cache[entry.SystemAddress] = data;
                    data.Save();
                }

                return data;
            }
        }

        #endregion

        #region migrate data from individual body files

        private static SystemData From(BodyData bodyData, CommanderSettings cmdr)
        {
            lock (cache)
            {
                // load from file or cache
                var data = SystemData.Load(bodyData.systemName, bodyData.systemAddress, cmdr.fid);

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
                var bodyData = Data.Load<BodyData>(filepath)!;

                var systemData = SystemData.From(bodyData, cmdr);
                if (systemData.starPos == null)
                {
                    // we need a starPos before we can create the file...
                    // get starPos from EDSM or Spansh, or fail :(
                    var edsmResult = await Game.edsm.getSystems(systemData.name);
                    systemData.starPos = edsmResult?.FirstOrDefault()?.coords?.starPos!;
                }
                if (systemData.starPos == null)
                {
                    var spanshResult = await Game.spansh.getSystem(systemData.name);
                    var matchedSystem = spanshResult.min_max.FirstOrDefault(_ => _.name.Equals(systemData.name, StringComparison.OrdinalIgnoreCase));
                    if (matchedSystem != null)
                        systemData.starPos = new double[] { matchedSystem.x, matchedSystem.y, matchedSystem.z };
                }
                if (systemData.starPos == null)
                {
                    // TODO: search back through journal files?
                    throw new Exception($"Failed to find a starPos for: '{systemData.name}'");
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
                            if (systemBody.bioScans == null) systemBody.bioScans = new List<BioScan>();
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
                    if (systemBody.organisms == null) systemBody.organisms = new List<SystemOrganism>();

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
                                            genus = bodyOrg.genus,
                                            genusLocalized = bodyOrg.genusLocalized!,
                                        };
                                        systemBody.organisms.Add(systemOrg);
                                        systemBody.bioSignalCount = Math.Max(systemBody.bioSignalCount, systemBody.organisms.Count);
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                // missing data - add just genus data for this, as if we DSS'd the body
                                Game.log($"Bad datafor: '{bodyOrg.species ?? bodyOrg.genus ?? bodyOrg.speciesLocalized ?? bodyOrg.genusLocalized}' on '{bodyData.bodyName}' ({bodyData.bodyId})");
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
                                genus = bioMatch.genus.name,
                            };
                            Game.log($"add organism '{bioMatch.variant.name}' ({bioMatch.entryId}) to '{systemBody.name}' ({systemBody.id})");
                            systemBody.organisms.Add(systemOrg);
                            systemBody.bioSignalCount = Math.Max(systemBody.bioSignalCount, systemBody.organisms.Count);
                        }

                        // update fields
                        if (systemOrg.entryId == 0) systemOrg.entryId = bioMatch.entryId;
                        if (systemOrg.reward == 0) systemOrg.reward = bioMatch.species.reward;
                        if (!systemOrg.analyzed && bodyOrg.analyzed) systemOrg.analyzed = true;

                        if (systemOrg.genusLocalized == null && bodyOrg.genusLocalized != null) systemOrg.genusLocalized = bodyOrg.genusLocalized;
                        if (systemOrg.genusLocalized == null) systemOrg.genusLocalized = bioMatch.genus.englishName;
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
        public bool honked;
        public bool fssAllBodies;

        /// <summary> A list of all bodies detected in this system </summary>
        public List<SystemBody> bodies = new List<SystemBody>();

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
                    // create a sub if not
                    body = new SystemBody()
                    {
                        name = bodyName,
                        id = bodyId,
                    };

                    Game.log($"SystemData: add body: '{body.name}' ({body.id}, {body.type})");
                    this.bodies.Add(body);
                }
                return body;
            }
        }

        public static List<string> journalEventTypes = new List<string>()
        {
            nameof(FSSDiscoveryScan),
            nameof(Scan),
            nameof(FSSBodySignals),
            nameof(FSSAllBodiesFound),
            nameof(SAAScanComplete),
            nameof(SAASignalsFound),
            nameof(ApproachBody),
            nameof(Disembark),
            nameof(Touchdown),
            nameof(CodexEntry),
            nameof(ScanOrganic),
            nameof(ApproachSettlement),
        };

        public void Journals_onJournalEntry(JournalEntry entry) { this.onJournalEntry((dynamic)entry); }
        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        public void onJournalEntry(FSSDiscoveryScan entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {entry.SystemName}"); return; }

            // Discovery scan a.k.a. honk
            this.honked = true;
            this.bodyCount = entry.BodyCount;
        }

        public void onJournalEntry(FSSAllBodiesFound entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }

            // FSS completed in-game
            this.fssAllBodies = true;
        }

        public void onJournalEntry(Scan entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {entry.StarSystem}"); return; }
            var body = this.findOrCreate(entry.Bodyname, entry.BodyID);

            // update fields
            body.type = SystemBody.typeFrom(entry.StarType, entry.PlanetClass, entry.Landable);
            body.planetClass = entry.PlanetClass;
            if (!string.IsNullOrEmpty(entry.TerraformState))
                body.terraformable = entry.TerraformState == "Terraformable";
            body.mass = entry.MassEM > 0 ? entry.MassEM : entry.StellarMass; // mass
            body.distanceFromArrivalLS = entry.DistanceFromArrivalLS;
            body.radius = entry.Radius;
            body.wasDiscovered = entry.WasDiscovered;
            body.wasMapped = entry.WasMapped;
            body.surfaceGravity = entry.SurfaceGravity;

            // TODO: Parents ?

            if (entry.Rings != null)
            {
                foreach (var newRing in entry.Rings)
                {
                    var ring = body.rings?.FirstOrDefault(_ => _.name == newRing.Name);
                    if (ring == null)
                    {
                        Game.log($"add ring '{newRing.Name}' to '{body.name}' ({body.id})");
                        if (body.rings == null) body.rings = new List<SystemRing>();
                        body.rings.Add(new SystemRing()
                        {
                            name = newRing.Name,
                            ringClass = newRing.RingClass,
                        });
                    }
                }
            }
        }

        public void onJournalEntry(SAAScanComplete entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);

            // update fields
            body.dssComplete = true;
            body.lastVisited = DateTimeOffset.Now;
        }

        public void onJournalEntry(FSSBodySignals entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            var body = this.findOrCreate(entry.Bodyname, entry.BodyID);

            // update fields
            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null && body.bioSignalCount < bioSignals.Count)
                body.bioSignalCount = bioSignals.Count;
        }

        public void onJournalEntry(SAASignalsFound entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);
            if (body.organisms == null) body.organisms = new List<SystemOrganism>();

            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null)
            {
                if (body.bioSignalCount < bioSignals.Count)
                    body.bioSignalCount = bioSignals.Count;

                foreach (var genusEntry in entry.Genuses)
                {
                    var organism = body.organisms.FirstOrDefault(_ => _.genus == genusEntry.Genus);
                    if (organism == null)
                    {
                        Game.log($"add organism '{genusEntry.Genus_Localised ?? genusEntry.Genus}' to '{body.name}' ({body.id})");
                        body.organisms.Add(new SystemOrganism()
                        {
                            genus = genusEntry.Genus,
                            genusLocalized = genusEntry.Genus_Localised!,
                        });
                        body.bioSignalCount = Math.Max(body.bioSignalCount, body.organisms.Count);
                    }
                }
            }
        }

        public void onJournalEntry(ApproachBody entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            var body = this.findOrCreate(entry.Body, entry.BodyID);

            // update fields
            if (entry.timestamp > body.lastVisited)
                body.lastVisited = entry.timestamp;
        }

        public void onJournalEntry(Touchdown entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            // ignore landing on stations
            if (!entry.OnPlanet) return;

            var body = this.findOrCreate(entry.Body, entry.BodyID);

            // update fields
            body.lastVisited = DateTimeOffset.Now;
            body.lastTouchdown = entry;
        }

        public void onJournalEntry(Disembark entry)
        {
            // this doesn't work very well
            //if (entry.OnPlanet)
            //{
            //    // assume first footfall if body was not discovered previously
            //    var body = this.findOrCreate(entry.Body, entry.BodyID);
            //    if (!body.wasDiscovered)
            //    {
            //        Game.log($"Assuming first footfall when disembarking an undiscovered body: '{body.name}' ({body.id})");
            //        body.firstFootFall = true;
            //    }
            //}
        }

        public void onJournalEntry(CodexEntry entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            // ignore non bio entries
            if (entry.SubCategory != "$Codex_SubCategory_Organic_Structures;") return;

            var body = this.bodies.FirstOrDefault(_ => _.id == entry.BodyID);
            if (body!.organisms == null) body.organisms = new List<SystemOrganism>();


            // find by first variant or entryId, then genusName
            var match = Game.codexRef.matchFromEntryId(entry.EntryID);
            var organism = body.findOrganism(match);

            if (organism == null)
            {
                organism = new SystemOrganism()
                {
                    genus = match.genus.name,
                };
                Game.log($"add organism '{match.variant.name}' ({match.entryId}) to '{body.name}' ({body.id})");
                body.organisms.Add(organism);
                body.bioSignalCount = Math.Max(body.bioSignalCount, body.organisms.Count);
            }

            // update fields
            organism.entryId = entry.EntryID;
            organism.variant = entry.Name;
            organism.variantLocalized = entry.Name_Localised;
            organism.reward = match.species.reward;

            if (organism.species == null) organism.species = match.species.name;
            if (organism.speciesLocalized == null) organism.speciesLocalized = match.species.englishName;
            if (organism.genusLocalized == null) organism.genusLocalized = match.genus.englishName;
        }

        public void onJournalEntry(ScanOrganic entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            var body = this.bodies.FirstOrDefault(_ => _.id == entry.Body);
            if (body!.organisms == null) body.organisms = new List<SystemOrganism>();

            // find by first variant or entryId, then genusName
            var match = Game.codexRef.matchFromVariant(entry.Variant);
            var organism = body.findOrganism(match);

            if (organism == null)
            {
                organism = new SystemOrganism()
                {
                    genus = entry.Genus,
                    genusLocalized = entry.Genus_Localized,
                };
                Game.log($"add organism '{entry.Variant_Localised}' ({match.entryId}) to '{body.name}' ({body.id})");
                body.organisms.Add(organism);
                body.bioSignalCount = Math.Max(body.bioSignalCount, body.organisms.Count);
            }

            // update fields
            if (!string.IsNullOrWhiteSpace(entry.Genus_Localized))
                organism.genusLocalized = entry.Genus_Localized;
            if (organism.genusLocalized == null && entry.Variant_Localised != null)
                organism.genusLocalized = Util.getGenusDisplayNameFromVariant(entry.Variant_Localised);

            organism.species = entry.Species;
            organism.speciesLocalized = entry.Species_Localised;
            organism.variant = entry.Variant;
            organism.variantLocalized = entry.Variant_Localised;
            organism.entryId = match.entryId;
            organism.reward = match.species.reward;

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
                else
                    Game.log($"BAD! Why entryId for organism '{entry.Variant_Localised ?? entry.Variant}' to '{body.name}' ({body.id})");
            }

            // We cannot log locations here because we do not know if the event is tracked live or retrospectively during playback (where the status file cannot be trusted)
        }

        public void onCanonnData(SystemPoi canonnPoi)
        {
            if (canonnPoi.system != this.name) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {canonnPoi.system}"); return; }
            if (!Game.settings.useExternalData) return;

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
                                genus = match.genus.name,
                                genusLocalized = match.genus.englishName,
                                variantLocalized = poi.english_name,
                                entryId = poi.entryid.Value,
                                reward = match.species.reward,
                            };
                            Game.log($"add organism '{match.variant.name}' ({match.entryId}) from Canonn POI to '{poiBody.name}' ({poiBody.id})");
                            if (poiBody.organisms == null) poiBody.organisms = new List<SystemOrganism>();
                            poiBody.organisms.Add(organism);
                            poiBody.bioSignalCount = Math.Max(poiBody.bioSignalCount, poiBody.organisms.Count);
                        }

                        // update fields
                        organism.entryId = poi.entryid.Value;
                        organism.reward = match.species.reward;
                    }
                }
            }
        }

        public void onEdsmResponse(EdsmSystem edsmSystem)
        {
            if (edsmSystem.id64 != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {edsmSystem.name}"); return; }
            if (!Game.settings.useExternalData) return;

            // update bodies from response
            foreach (var entry in edsmSystem.bodies)
            {
                var body = this.findOrCreate(entry.name, entry.bodyId);

                // update fields
                if (entry.type == "Star")
                    body.type = SystemBodyType.Star;
                else
                    body.type = SystemBody.typeFrom(null!, entry.subType, entry.isLandable);
                if (body.distanceFromArrivalLS == 0) body.distanceFromArrivalLS = entry.distanceToArrival;
                if (body.radius == 0 && entry.radius > 0) body.radius = entry.radius * 1000; // why?
                if (body.planetClass == null) body.planetClass = entry.subType;
                if (!string.IsNullOrEmpty(entry.terraformingState))
                    body.terraformable = entry.terraformingState == "Candidate for terraforming";
                if (body.mass == 0) body.mass = entry.earthMasses > 0 ? entry.earthMasses : entry.solarMasses; // mass
                if (body.surfaceGravity == 0 && entry.gravity > 0) body.surfaceGravity = entry.gravity.Value; // gravity

                if (entry.rings != null)
                {
                    foreach (var newRing in entry.rings)
                    {
                        var ring = body.rings?.FirstOrDefault(_ => _.name == newRing.name);
                        if (ring == null)
                        {
                            Game.log($"add ring '{newRing.name}' from EDSM to '{body.name}' ({body.id})");
                            if (body.rings == null) body.rings = new List<SystemRing>();
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

        public void onSpanshResponse(ApiSystemDumpSystem spanshSystem)
        {
            if (spanshSystem.id64 != this.address) { Game.log($"Unmatched system! Expected: `{this.name}`, got: {spanshSystem.name}"); return; }
            if (!Game.settings.useExternalData) return;

            // update bodies from response
            foreach (var entry in spanshSystem.bodies)
            {
                var body = this.findOrCreate(entry.name, entry.bodyId);

                // update fields
                if (entry.type == "Star")
                    body.type = SystemBodyType.Star;
                else
                    body.type = SystemBody.typeFrom(null!, entry.subType, entry.isLandable ?? false);
                if (body.distanceFromArrivalLS == 0) body.distanceFromArrivalLS = entry.distanceToArrival ?? 0;
                if (body.radius == 0 && entry.radius != null) body.radius = entry.radius.Value * 1000; // why?
                if (body.planetClass == null) body.planetClass = entry.subType;
                if (!string.IsNullOrEmpty(entry.terraformingState))
                    body.terraformable = entry.terraformingState == "Candidate for terraforming";
                if (body.mass == 0) body.mass = (entry.earthMasses > 0 ? entry.earthMasses : entry.solarMasses) ?? 0; // mass
                if (body.surfaceGravity == 0 && entry.gravity > 0) body.surfaceGravity = entry.gravity.Value; // gravity

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
                if (entry.signals?.signals?.ContainsKey("$SAA_SignalType_Biological;") == true)
                {
                    var newCount = entry.signals.signals["$SAA_SignalType_Biological;"];
                    if (body.bioSignalCount < newCount)
                        body.bioSignalCount = newCount;
                }

                // update genus if not already known
                if (entry.signals?.genuses?.Count > 0)
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
                                genus = newGenus,
                                genusLocalized = BioScan.genusNames[newGenus],
                            });
                        }
                    }
                }

            }
        }

        public void onJournalEntry(ApproachSettlement entry)
        {
            if (entry.SystemAddress != this.address) { Game.log($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}"); return; }
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);

            if (entry.Name.StartsWith("$Ancient"))
            {
                // update settlements
                if (body.settlements == null) body.settlements = new Dictionary<string, LatLong2>();
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
        }

        #endregion

        public override string ToString()
        {
            return $"'{this.name}' ({this.address}";
        }

        [JsonIgnore]
        public int fssBodyCount { get => this.bodies.Count(_ => _.type != SystemBodyType.Asteroid && _.type != SystemBodyType.Unknown); }

        /// <summary> Returns True when all non-star/non-asteroid bodies have been found with FSS </summary>
        [JsonIgnore]
        public bool fssComplete { get => this.bodyCount == this.fssBodyCount; }

        [JsonIgnore]
        public int dssBodyCount { get => this.bodies.Count(_ => _.dssComplete); }

        [JsonIgnore]
        public int bioSignalsTotal { get => this.bodies.Sum(_ => _.bioSignalCount); }

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

                // skip stars and asteroids
                if (_.type == SystemBodyType.Star || _.type == SystemBodyType.Asteroid) continue;

                var reducedName = _.name
                    .Replace(this.name, "")
                    .Replace(" ", "");

                // inject rings
                if (!Game.settings.skipRingsDSS && _.hasRings)
                {
                    if (_.rings.Count > 0)
                        names.Add(reducedName + "rA");
                    if (_.rings.Count > 0)
                        names.Add(reducedName + "rB");
                }

                // optionally skip gas giants
                if (Game.settings.skipGasGiantDSS && _.type == SystemBodyType.Giant) continue;

                // optionally skip low value bodoes
                if (Game.settings.skipLowValueDSS && _.rewardEstimate < Game.settings.skipLowValueAmount) continue;


                names.Add(reducedName);
            }

            return names;
        }

        public List<string> getBioRemainingNames()
        {
            var names = this.bodies
                .OrderBy(_ => _.id)
                .Where(_ => _.countAnalyzedBioSignals < _.bioSignalCount)
                .Select(_ => _.name.Replace(this.name, "").Replace(" ", ""))
                .ToList();

            return names;
        }

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
            var sites = GuardianSitePub.Find(this.name);
            Game.log($"prepSettlements: for: '{this.name}' ({this.address}), sites.Count: {sites.Count}");
            this._settlements = new List<SystemSettlementSummary>();
            foreach (var site in sites)
            {
                if (site.t == GuardianSiteData.SiteType.Alpha || site.t == GuardianSiteData.SiteType.Beta || site.t == GuardianSiteData.SiteType.Gamma)
                {
                    var body = this.bodies.FirstOrDefault(_ => _.name == site.bodyName)!;
                    if (body != null)
                        this._settlements.Add(SystemSettlementSummary.forRuins(this, body, site.idx));
                }
                else
                {
                    var body = this.bodies.FirstOrDefault(_ => _.name == site.bodyName)!;
                    if (body != null)
                        this._settlements.Add(SystemSettlementSummary.forStructure(this, body));
                    else
                        Game.log($"Why no body yet for: '{site.bodyName}'");
                }
            }
        }
    }

    internal class SystemBody
    {
        public static SystemBodyType typeFrom(string starType, string planetClass, bool landable)
        {
            // choose type
            if (landable)
                return SystemBodyType.LandableBody;
            else if (!string.IsNullOrEmpty(starType))
                return SystemBodyType.Star;
            else if (string.IsNullOrEmpty(starType) && string.IsNullOrEmpty(planetClass))
                return SystemBodyType.Asteroid;
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

        /// <summary> Is this a star, gas-giant, or landable body, etc </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SystemBodyType type;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long distanceFromArrivalLS;

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
        public bool wasDiscovered;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool wasMapped;

        // Parents? // TODO: "Parents":[ {"Ring":3}, {"Star":1}, {"Null":0} ]

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

        /// <summary> Locations of all bio scans or Codex scans performed on this body </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<BioScan>? bioScans = new List<BioScan>();

        /// <summary> Locations of named bookmarks on this body </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, List<LatLong2>>? bookmarks = new Dictionary<string, List<LatLong2>>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int bioSignalCount;

        /// <summary> All the organisms for this body </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public List<SystemOrganism>? organisms;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool firstFootFall;

        /// <summary> A of settlements known on this body. /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Dictionary<string, LatLong2> settlements;

        #endregion

        public override string ToString()
        {
            return $"'{this.name}' ({this.id}, {this.type})";
        }

        [JsonIgnore]
        public bool hasRings { get => this.rings?.Count > 0; }

        [JsonIgnore]
        public int countAnalyzedBioSignals
        {
            get => this.organisms == null
                ? 0
                : this.organisms.Count(_ => _.analyzed);
        }

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
        public long sumAnalyzed { get => this.organisms?.Sum(_ => !_.analyzed ? 0 : this.firstFootFall ? _.reward * 5 : _.reward) ?? 0; }

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
            return this.findOrganism(match.variant.name, match.entryId, match.genus.name);
        }

        public SystemOrganism? findOrganism(string variant, long entryId, string genus)
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
        public bool analyzed;

        public override string ToString()
        {
            return $"{this.speciesLocalized ?? this.species ?? this.genusLocalized ?? this.genus} ({this.entryId})";
        }

        [JsonIgnore]
        public int range { get => BioScan.ranges[this.genus]; }
    }
}
