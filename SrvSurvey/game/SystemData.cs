using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.canonn;
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
        private static SystemData? Load(string systemName, long systemAddress)
        {
            Game.log($"Loading SystemData for: '{systemName}' ({systemAddress})");
            // use cache entry if present
            if (cache.ContainsKey(systemAddress))
            {
                return cache[systemAddress];
            }

            // try finding files by systemAddress first, then system name
            var path = Path.Combine(Application.UserAppDataPath, "systems", Game.activeGame!.fid!);
            Directory.CreateDirectory(path);
            var files = Directory.GetFiles(path, $"*{systemAddress}.json");
            if (files.Length == 0)
            {
                files = Directory.GetFiles(path, $"{systemName}*.json");
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
        public static void Close(SystemData data)
        {
            Game.log($"Closing and saving SystemData for: '{data.name}' ({data.address})");
            lock (cache)
            {
                // ensure data is saved then remove from the cache
                data.Save();

                if (cache.ContainsValue(data))
                    cache.Remove(data.address);
            }
        }

        //public static SystemData From(FSDJump entry)
        //{
        //    lock (cache)
        //    {
        //        // load from file or cache
        //        var data = Load(entry.StarSystem, entry.SystemAddress);

        //        if (data == null)
        //        {
        //            // create a new data object with the main star populated
        //            data = new SystemData()
        //            {
        //                filepath = Path.Combine(Application.UserAppDataPath, "systems", Game.activeGame!.fid!, $"{entry.StarSystem}_{entry.SystemAddress}.json"),
        //                name = entry.StarSystem,
        //                address = entry.SystemAddress,
        //                starPos = entry.StarPos,
        //                commander = Game.activeGame.cmdr.commander,
        //                firstVisited = DateTimeOffset.UtcNow,
        //                lastVisited = DateTimeOffset.UtcNow,
        //            };

        //            var mainStar = new SystemBody()
        //            {
        //                id = entry.BodyID,
        //                name = entry.Body,
        //                type = SystemBodyType.Star,
        //                firstVisited = DateTimeOffset.UtcNow,
        //                lastVisited = DateTimeOffset.UtcNow,
        //            };
        //            data.bodies.Add(mainStar);

        //            cache[entry.SystemAddress] = data;
        //            data.Save();
        //        }

        //        return data;
        //    }
        //}

        public static SystemData From(ISystemDataStarter entry)
        {
            lock (cache)
            {
                // load from file or cache
                var data = Load(entry.StarSystem, entry.SystemAddress);

                if (data == null)
                {
                    // create a new data object with the main star populated
                    data = new SystemData()
                    {
                        filepath = Path.Combine(Application.UserAppDataPath, "systems", Game.activeGame!.fid!, $"{entry.StarSystem}_{entry.SystemAddress}.json"),
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

        //public static SystemData From(CarrierJump entry)
        //{
        //    lock (cache)
        //    {
        //        // load from file or cache
        //        var data = Load(entry.StarSystem, entry.SystemAddress);

        //        if (data == null)
        //        {
        //            // create a new data object with the main star populated
        //            data = new SystemData()
        //            {
        //                filepath = Path.Combine(Application.UserAppDataPath, "systems", Game.activeGame!.fid!, $"{entry.StarSystem}_{entry.SystemAddress}.json"),
        //                name = entry.StarSystem,
        //                address = entry.SystemAddress,
        //                starPos = entry.StarPos,
        //                commander = Game.activeGame.cmdr.commander,
        //                firstVisited = DateTimeOffset.UtcNow,
        //                lastVisited = DateTimeOffset.UtcNow,
        //            };

        //            var mainStar = new SystemBody()
        //            {
        //                id = entry.BodyID,
        //                name = entry.Body,
        //                type = SystemBodyType.Star,
        //                firstVisited = DateTimeOffset.UtcNow,
        //                lastVisited = DateTimeOffset.UtcNow,
        //            };
        //            data.bodies.Add(mainStar);

        //            cache[entry.SystemAddress] = data;
        //            data.Save();
        //        }

        //        return data;
        //    }
        //}

        //private static SystemBodyType mapBodyType(BodyType bodyType)
        //{
        //    switch(bodyType)
        //    {
        //        case BodyType.Star:
        //            return SystemBodyType.Star;

        //        case BodyType.PlanteryRing:
        //            return SystemBodyType.
        //    }

        //}

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

        public static List<string> journalEventTypes = new List<string>()
        {
            nameof(FSSDiscoveryScan),
            nameof(Scan),
            nameof(FSSBodySignals),
            nameof(FSSAllBodiesFound),
            nameof(SAAScanComplete),
            nameof(SAASignalsFound),
            nameof(ApproachBody),
            nameof(Touchdown),
            nameof(CodexEntry),
            nameof(ScanOrganic),
        };

        public void Journals_onJournalEntry(JournalEntry entry) { this.onJournalEntry((dynamic)entry); }
        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        public void onJournalEntry(FSSDiscoveryScan entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.name}`, got: {entry.SystemName}");

            // Discovery scan a.k.a. honk
            this.honked = true;
            this.bodyCount = entry.BodyCount;
        }

        public void onJournalEntry(FSSAllBodiesFound entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");

            // FSS completed in-game
            this.fssAllBodies = true;
        }

        public void onJournalEntry(Scan entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.name}`, got: {entry.StarSystem}");
            var body = this.findOrCreate(entry.Bodyname, entry.BodyID);

            // update fields
            body.type = SystemBody.typeFrom(entry.StarType, entry.PlanetClass, entry.Landable);
            body.planetClass = entry.PlanetClass;
            body.terraformable = entry.TerraformState == "Terraformable";
            body.mass = entry.MassEM > 0 ? entry.MassEM : entry.StellarMass; // mass
            body.distanceFromArrivalLS = entry.DistanceFromArrivalLS;
            body.radius = entry.Radius;
            body.wasDiscovered = entry.WasDiscovered;
            body.wasMapped = entry.WasMapped;

            // TODO: Parents ?

            if (entry.Rings != null)
            {
                body.rings = new List<SystemRing>();
                foreach (var ring in entry.Rings)
                {
                    Game.log($"add ring '{ring.Name}' to '{body.name}' ({body.id})");
                    body.rings.Add(new SystemRing()
                    {
                        name = ring.Name,
                        ringClass = ring.RingClass,
                    });
                }
            }
        }

        public void onJournalEntry(SAAScanComplete entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
            var body = this.findOrCreate(entry.BodyName, entry.BodyID);

            // update fields
            body.dssComplete = true;
            body.lastVisited = DateTimeOffset.Now;
        }

        public void onJournalEntry(FSSBodySignals entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
            var body = this.findOrCreate(entry.Bodyname, entry.BodyID);

            // update fields
            var bioSignals = entry.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");
            if (bioSignals != null && body.bioSignalCount < bioSignals.Count)
                body.bioSignalCount = bioSignals.Count;
        }

        public void onJournalEntry(SAASignalsFound entry)
        {
            // DSS complete
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
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
                    }
                }
            }
        }

        public void onJournalEntry(ApproachBody entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
            var body = this.findOrCreate(entry.Body, entry.BodyID);

            // update fields
            body.lastVisited = DateTimeOffset.Now;
        }

        public void onJournalEntry(Touchdown entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
            // ignore landing on stations
            if (!entry.OnPlanet) return;

            var body = this.findOrCreate(entry.Body, entry.BodyID);

            // update fields
            body.lastVisited = DateTimeOffset.Now;
            body.lastTouchdown = entry;
        }

        public void onJournalEntry(CodexEntry entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
            // ignore non bio entries
            if (entry.Category != "$Codex_Category_Biology;") return;

            var body = this.bodies.FirstOrDefault(_ => _.id == entry.BodyID);
            if (body!.organisms == null) body.organisms = new List<SystemOrganism>();

            var genusName = Util.getGenusNameFromVariant(entry.Name) + "Genus_Name;";
            var organism = body.organisms.FirstOrDefault(_ => _.genus == genusName);
            if (organism == null)
            {
                organism = new SystemOrganism()
                {
                    genus = genusName,
                };
                Game.log($"add organism '{genusName}' to '{body.name}' ({body.id})");
                body.organisms.Add(organism);
            }

            // update fields
            organism.entryId = entry.EntryID;
            organism.variant = entry.Name;
            organism.variantLocalized = entry.Name_Localised;
            organism.reward = Game.codexRef.getRewardForEntryId(entry.EntryID.ToString());
        }

        public void onJournalEntry(ScanOrganic entry)
        {
            if (entry.SystemAddress != this.address) throw new ArgumentOutOfRangeException($"Unmatched system! Expected: `{this.address}`, got: {entry.SystemAddress}");
            var body = this.bodies.FirstOrDefault(_ => _.id == entry.Body);
            if (body!.organisms == null) body.organisms = new List<SystemOrganism>();

            var organism = body.organisms.FirstOrDefault(_ => _.variant == entry.Variant);

            // we might not have variant yet? Try again with just Genus
            if (organism == null)
                organism = body.organisms.FirstOrDefault(_ => _.genus == entry.Genus);

            // some organisms have 2 species on the same planet, eg: Brain Tree's
            if (organism != null && organism.variant != entry.Variant)
            {
                // clear current organism, so we start another one
                organism = null;
                body.bioSignalCount++;
            }

            if (organism == null)
            {
                organism = new SystemOrganism()
                {
                    genus = entry.Genus,
                    genusLocalized = entry.Genus_Localized,
                };
                Game.log($"add organism '{entry.Genus_Localized ?? entry.Genus}' to '{body.name}' ({body.id})");
                body.organisms.Add(organism);
            }

            // TODO: handle multiple Brain Tree entries!

            // update fields
            organism.species = entry.Species;
            organism.speciesLocalized = entry.Species_Localised;
            organism.variant = entry.Variant;
            organism.variantLocalized = entry.Variant_Localised;

            if (organism.entryId == 0)
            {
                Game.codexRef.loadCodexRef().ContinueWith(response =>
                {
                    if (!response.IsCompletedSuccessfully) return;

                    Dictionary<string, RefCodexEntry> codexRef = response.Result;
                    var match = codexRef.Values.FirstOrDefault(_ => _.name == entry.Variant);

                    if (match != null)
                    {
                        // update fields
                        organism.entryId = long.Parse(match.entryid);
                        if (match.reward.HasValue) organism.reward = match.reward.Value;

                        Application.DoEvents();
                        Application.DoEvents();
                        this.Save();
                    }
                    else
                    {
                        Game.log($"No codexRef match found for organism '{entry.Variant_Localised ?? entry.Variant}' to '{body.name}' ({body.id})");
                    }
                });
            }

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
                if (organism.entryId > 0)
                    Game.activeGame!.cmdr.scannedBioEntryIds.Add($"{this.address}_{body.id}_{organism.entryId}");
                else
                    Game.log($"BAD! Why entryId for organism '{entry.Variant_Localised ?? entry.Variant}' to '{body.name}' ({body.id})");
            }

            // We cannot log locations here because we do not know if the event is tracked live or retrospectively during playback (where the status file cannot be trusted)
        }

        #endregion

        public override string ToString()
        {
            return $"'{this.name}' ({this.address}";
        }

        [JsonIgnore]
        public int fssBodyCount { get => this.bodies.Count(_ => _.type != SystemBodyType.Asteroid); }

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
        public List<SystemOrganism> organisms;

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
        public double rewardEstimate
        {
            get
            {
                var estimate = Util.GetBodyValue(
                    this.planetClass, // planetClass
                    this.terraformable, // isTerraformable
                    this.mass, // mass
                    !this.wasDiscovered, // isFirstDiscoverer
                    this.dssComplete, // isMapped
                    !this.wasMapped // isFirstMapped
                );

                return estimate;
            }
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
