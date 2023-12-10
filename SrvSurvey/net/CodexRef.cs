using Newtonsoft.Json;
using SrvSurvey.game;

namespace SrvSurvey.canonn
{
    internal class CodexRef
    {
        private static string codexRefPath = Path.Combine(Application.UserAppDataPath, "codexRef.json");
        private static string speciesRewardPath = Path.Combine(Application.UserAppDataPath, "speciesRewards.json");
        private static string entryIdRewardPath = Path.Combine(Application.UserAppDataPath, "entryIdRewards.json");
        private static string bioRefPath = Path.Combine(Application.UserAppDataPath, "bioRef.json");

        private Dictionary<string, long>? rewards;
        private Dictionary<string, long>? rewardsByEntryId;
        public List<BioGenus> genus;

        public void init()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            prepBioRef();
            loadOrganicRewards();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task<Dictionary<string, RefCodexEntry>> loadCodexRef()
        {
            string json;
            if (!File.Exists(codexRefPath))
            {
                Game.log("Requesting codex/ref from network");
                json = await new HttpClient().GetStringAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/ref");
                File.WriteAllText(codexRefPath, json);
            }
            else
            {
                Game.log("Reading codex/ref from disk");
                json = File.ReadAllText(codexRefPath);
            }

            var codexRef = JsonConvert.DeserializeObject<Dictionary<string, RefCodexEntry>>(json)!;
            return codexRef;
        }

        public async Task loadOrganicRewards()
        {
            if (!File.Exists(speciesRewardPath) || !File.Exists(entryIdRewardPath))
            {
                Game.log("Preparing organic rewards from codex/ref");
                var codexRef = await loadCodexRef();
                var organicStuff = codexRef!.Values
                    .Where(_ => _.sub_category == "$Codex_SubCategory_Organic_Structures;" && _.reward > 0);

                rewards = new Dictionary<string, long>();
                rewardsByEntryId = new Dictionary<string, long>();
                foreach (var _ in organicStuff)
                {
                    // extract the species prefix from the name, without the color variant part
                    var species = Util.getSpeciesPrefix(_.name);

                    if (!rewards.ContainsKey(species))
                    {
                        rewards.Add(species, (long)_.reward!);
                    }
                    else if (rewards[species] != (long)_.reward!)
                    {
                        Game.log($"BAD? {_.name} / {species} {rewards[species]} vs {(long)_.reward!}");
                    }

                    var entryIdKey = _.entryid.Substring(0, 5);
                    if (!rewardsByEntryId.ContainsKey(entryIdKey))
                    {
                        rewardsByEntryId.Add(entryIdKey, (long)_.reward!);
                    }
                    else if (rewardsByEntryId[entryIdKey] != (long)_.reward!)
                    {
                        Game.log($"BAD? {_.name} / {species} {rewardsByEntryId[entryIdKey]} vs {(long)_.reward!}");
                    }
                }

                File.WriteAllText(speciesRewardPath, JsonConvert.SerializeObject(rewards));
                File.WriteAllText(entryIdRewardPath, JsonConvert.SerializeObject(rewardsByEntryId));
            }
            else
            {
                Game.log("Reading organic rewards from disk");
                this.rewards = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(speciesRewardPath))!;
                this.rewardsByEntryId = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(entryIdRewardPath))!;
            }
        }

        public async Task prepBioRef()
        {
            if (!File.Exists(bioRefPath))
            {
                Game.log("Preparing organic rewards from codex/ref");
                var codexRef = await loadCodexRef();
                var organicStuff = codexRef!.Values
                    .Where(_ => _.sub_category == "$Codex_SubCategory_Organic_Structures;" && _.reward > 0);

                foreach (var thing in organicStuff)
                {
                    if (thing.entryid.Length != 7) throw new Exception("Bad EntryId length!");

                    // extract/create various names
                    string variantName, variantEnglishName, speciesName, speciesEnglishName, genusName, genusEnglishName;

                    if (thing.platform == "odyssey")
                    {
                        // extract/create various names
                        variantName = thing.name;
                        variantEnglishName = thing.english_name;

                        genusName = Util.getGenusNameFromVariant(thing.name) + "Genus_Name;";
                        speciesName = Util.getSpeciesPrefix(thing.name) + "Name;";

                        var parts = thing.english_name.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        genusEnglishName = parts[0];
                        speciesEnglishName = $"{parts[0]} {parts[1]}";
                    }
                    else
                    {

                        // extract/create various names
                        variantName = thing.name;
                        variantEnglishName = thing.english_name;

                        speciesName = variantName;
                        speciesEnglishName = variantEnglishName;

                        var parts = thing.english_name.Split(' ', 2); // StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        genusEnglishName = parts[1]; // TODO: "Brain Tree" vs "Brain Trees" ?!

                        switch (thing.sub_class)
                        {
                            case "Anemone": genusName = "$Codex_Ent_Sphere_Name;"; break;
                            case "Amphora Plant": genusName = "$Codex_Ent_Vents_Name;"; break;
                            case "Bark Mounds": genusName = "$Codex_Ent_Cone_Name;"; break;
                            case "Brain Tree": genusName = "$Codex_Ent_Brancae_Name;"; break;
                            case "Shards": genusName = "$Codex_Ent_Ground_Struct_Ice_Name;"; break;
                            case "Tubers": genusName = "$Codex_Ent_Tube_Name;"; break;
                            default:
                                throw new Exception($"Oops: {thing.sub_class}?");
                        }

                        Game.log("!");
                        // { "timestamp":"2023-06-11T22:50:27Z", "event":"ScanOrganic", "ScanType":"Sample", "Genus":"$Codex_Ent_Brancae_Name;", "Genus_Localised":"Brain Trees", "Species":"$Codex_Ent_SeedABCD_02_Name;", "Species_Localised":"Ostrinum Brain Tree", "Variant":"$Codex_Ent_SeedABCD_02_Name;", "Variant_Localised":"Ostrinum Brain Tree", "SystemAddress":546399072737, "Body":12 }
                        // { "timestamp":"2023-03-12T06:57:16Z", "event":"CodexEntry", "EntryID":2100203, "Name":"$Codex_Ent_SeedABCD_02_Name;", "Name_Localised":"Ostrinum Brain Tree", "SubCategory":"$Codex_SubCategory_Organic_Structures;", "SubCategory_Localised":"Organic structures", "Category":"$Codex_Category_Biology;", "Category_Localised":"Biological and Geological", "Region":"$Codex_RegionName_18;", "Region_Localised":"Inner Orion Spur", "System":"Synuefe EN-H d11-96", "SystemAddress":3309179996515, "BodyID":9, "Latitude":-23.696585, "Longitude":178.804047, "IsNewEntry":true }
                    }

                    // match or create the Genus
                    var genusRef = this.genus.FirstOrDefault(_ => _.name == genusName);
                    if (genusRef == null)
                    {
                        genusRef = new BioGenus()
                        {
                            name = genusName,
                            englishName = genusEnglishName,
                            dist = BioScan.ranges[genusName],
                            species = new List<BioSpecies>(),
                        };
                        this.genus.Add(genusRef);
                    };

                    // match or create the Species
                    var speciesRef = genusRef.species.FirstOrDefault(_ => _.name == speciesName);
                    if (speciesRef == null)
                    {
                        speciesRef = new BioSpecies()
                        {
                            name = speciesName,
                            englishName = speciesEnglishName,
                            reward = thing.reward!.Value,
                            entryIdPrefix = thing.entryid.Substring(0, 5),
                            variants = new List<BioVariant>(),
                        };
                        genusRef.species.Add(speciesRef);
                    };

                    // match or create the variant
                    var variantRef = speciesRef.variants.FirstOrDefault(_ => _.name == variantName);
                    if (variantRef == null)
                    {
                        variantRef = new BioVariant()
                        {
                            name = variantName,
                            englishName = variantEnglishName,
                            entryIdSuffix = thing.entryid.Substring(5),
                        };
                        speciesRef.variants.Add(variantRef);
                    };
                }

                File.WriteAllText(bioRefPath, JsonConvert.SerializeObject(this.genus));
            }
            else
            {
                Game.log("Reading bioRef from disk");
                this.genus = JsonConvert.DeserializeObject<List<BioGenus>>(File.ReadAllText(bioRefPath))!;
            }
        }

        public BioMatch matchFromEntryId(long entryId)
        {
            return genusFromEntryId(entryId.ToString());
        }

        public BioMatch genusFromEntryId(string entryId)
        {
            if (this.genus == null || this.genus.Count == 0)
                throw new Exception($"BioRef is not loaded.");

            // search all species for the shorter entryId
            var entryIdPrefix = entryId.Substring(0, 5);
            var entryIdSuffix = entryId.Substring(5);

            foreach (var genusRef in this.genus)
                foreach (var speciesRef in genusRef.species)
                    if (speciesRef.entryIdPrefix == entryIdPrefix)
                        foreach (var variantRef in speciesRef.variants)
                            if (variantRef.entryIdSuffix == entryIdSuffix)
                                return new BioMatch(genusRef, speciesRef, variantRef);

            throw new Exception($"Unexpected entryId: '{entryId}'");
        }

        public BioMatch matchFromVariant(string variantName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");

            var genusName = Util.getGenusNameFromVariant(variantName) + "Genus_Name;";
            var speciesName = Util.getSpeciesPrefix(variantName) + "Name;";

            foreach (var genusRef in this.genus)
                foreach (var speciesRef in genusRef.species)
                    if (speciesRef.name == speciesName)
                        foreach (var variantRef in speciesRef.variants)
                            if (variantRef.name == variantName)
                                return new BioMatch(genusRef, speciesRef, variantRef);

            throw new Exception($"Unexpected speciesName: '{speciesName}'");
        }

        public long getRewardForSpecies(string name)
        {
            if (rewards == null)
                loadOrganicRewards().Wait();

            var key = rewards!.Keys.FirstOrDefault(_ => name.StartsWith(_));
            if (key != null && rewards.ContainsKey(key))
                return rewards[key];
            else
                return -1;
        }

        public long getRewardForEntryId(string entryId)
        {
            if (rewardsByEntryId == null)
                loadOrganicRewards().Wait();
            entryId = entryId.Substring(0, 5);
            if (rewardsByEntryId!.ContainsKey(entryId))
                return rewardsByEntryId[entryId];
            else
                return -1;
        }
    }
}
