using Newtonsoft.Json;
using SrvSurvey.game;

namespace SrvSurvey.canonn
{
    internal class CodexRef
    {
        private static string codexRefPath = Path.Combine(Program.dataFolder, "codexRef.json");
        private static string speciesRewardPath = Path.Combine(Program.dataFolder, "speciesRewards.json");
        private static string entryIdRewardPath = Path.Combine(Program.dataFolder, "entryIdRewards.json");
        private static string bioRefPath = Path.Combine(Program.dataFolder, "bioRef.json");

        private Dictionary<string, long>? rewards;
        private Dictionary<string, long>? rewardsByEntryId;
        public List<BioGenus> genus;

        public async Task init()
        {
            // get CodexRef ready first, before running these in parallel
            var codexRef = await loadCodexRef();
            prepBioRef(codexRef);
            loadOrganicRewards(codexRef); // todo: retire
        }

        public async Task<Dictionary<string, RefCodexEntry>> loadCodexRef()
        {
            string json;
            if (!File.Exists(codexRefPath))
            {
                Game.log("loadCodexRef: preparing from network ...");
                json = await new HttpClient().GetStringAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/ref");
                File.WriteAllText(codexRefPath, json);
                Game.log("loadCodexRef: complete");
            }
            else
            {
                Game.log("loadCodexRef: reading codex/ref from disk");
                json = File.ReadAllText(codexRefPath);
            }

            var codexRef = JsonConvert.DeserializeObject<Dictionary<string, RefCodexEntry>>(json)!;
            return codexRef;
        }

        public void loadOrganicRewards(Dictionary<string, RefCodexEntry> codexRef)
        {
            if (!File.Exists(speciesRewardPath) || !File.Exists(entryIdRewardPath))
            {
                Game.log("loadOrganicRewards: preparing ...");
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
                Game.log("loadOrganicRewards: complete");
            }
            else
            {
                Game.log("loadOrganicRewards: reading organic rewards from disk");
                this.rewards = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(speciesRewardPath))!;
                this.rewardsByEntryId = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(entryIdRewardPath))!;
            }
        }

        public void prepBioRef(Dictionary<string, RefCodexEntry> codexRef)
        {
            if (!File.Exists(bioRefPath))
            {
                Game.log("prepBioRef: preparing from network ...");
                this.genus = new List<BioGenus>();
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
                            odyssey = thing.platform == "odyssey",
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
                Game.log("prepBioRef: complete");
            }
            else
            {
                Game.log("prepBioRef: reading bioRef from disk");
                this.genus = JsonConvert.DeserializeObject<List<BioGenus>>(File.ReadAllText(bioRefPath))!;
            }
        }

        public BioMatch matchFromEntryId(long entryId)
        {
            return matchFromEntryId(entryId.ToString());
        }

        public BioMatch matchFromEntryId(string entryId)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");

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

            throw new Exception($"Unexpected variantName: '{speciesName}'");
        }

        public BioSpecies matchFromSpecies(string speciesName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");
            if (string.IsNullOrEmpty(speciesName)) throw new Exception($"Missing species name!");

            // we cannot pre-match by genus name as Brain Tree species name are not consistent with their genus names
            foreach (var genusRef in this.genus)
                foreach (var speciesRef in genusRef.species)
                    if (speciesRef.name == speciesName)
                        return speciesRef;

            throw new Exception($"Unexpected speciesName: '{speciesName}'");
        }

        public BioGenus? matchFromGenus(string genusName)
        {
            var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.species.Any(_ => genusRef.name == genusName));
            return genusRef;
        }

        public bool isLegacyGenus(string genusName, string speciesName)
        {
            var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.species.Any(_ => _.name == speciesName) || genusRef.name == genusName);
            return genusRef?.odyssey == false;
        }

        public long getRewardForSpecies(string name)
        {

            var key = rewards!.Keys.FirstOrDefault(_ => name.StartsWith(_));
            if (key != null && rewards.ContainsKey(key))
                return rewards[key];
            else
                return -1;
        }

        public long getRewardForEntryId(string entryId)
        {
            entryId = entryId.Substring(0, 5);
            if (rewardsByEntryId!.ContainsKey(entryId))
                return rewardsByEntryId[entryId];
            else
                return -1;
        }
    }
}
