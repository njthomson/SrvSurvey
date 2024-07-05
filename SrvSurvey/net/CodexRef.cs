using Newtonsoft.Json;
using SrvSurvey.game;

namespace SrvSurvey.canonn
{
    internal class CodexRef
    {
        private static string codexRefPath = Path.Combine(Program.dataFolder, "codexRef.json");
        private static string bioRefPath = Path.Combine(Program.dataFolder, "bioRef.json");
        public static string codexImagesFolder = Path.Combine(Program.dataFolder, "codexImages");

        public List<BioGenus> genus;

        public async Task init(bool reset)
        {
            var duration = DateTime.Now - Game.settings.lastCodexRefDownload;
            Game.log($"CodexRef init (reset: {reset}, last downloaded: {duration.TotalDays.ToString("N3")} days ago) ...");

            // force a download and re-processing once a week
            if (duration.TotalDays > 7) reset = true;

            // get CodexRef ready first, before running these in parallel
            var codexRef = await loadCodexRef(reset);
            prepBioRef(codexRef, reset);

            if (!Directory.Exists(CodexRef.codexImagesFolder))
                Directory.CreateDirectory(CodexRef.codexImagesFolder);

            Game.log("CodexRef init - complete");
        }

        public async Task<Dictionary<string, RefCodexEntry>> loadCodexRef(bool reset = false)
        {
            string json;
            if (!File.Exists(codexRefPath) || reset)
            {
                Game.log("loadCodexRef: preparing from network ...");
                json = await new HttpClient().GetStringAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/ref");
                File.WriteAllText(codexRefPath, json);
                Game.log("loadCodexRef: complete");
                Game.settings.lastCodexRefDownload = DateTime.Now;
            }
            else
            {
                Game.log("loadCodexRef: reading codex/ref from disk");
                json = File.ReadAllText(codexRefPath);
            }

            var codexRef = JsonConvert.DeserializeObject<Dictionary<string, RefCodexEntry>>(json)!;
            return codexRef;
        }

        public void prepBioRef(Dictionary<string, RefCodexEntry> codexRef, bool reset = false)
        {
            if (!File.Exists(bioRefPath) || reset)
            {
                Game.log("prepBioRef: (re)building from whole CodexRef ...");
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
                            imageUrl = thing.image_url,
                            imageCmdr = thing.image_cmdr,
                        };
                        speciesRef.variants.Add(variantRef);
                    };
                }

                // special case for Brain Tree genus name
                var trouble = this.genus.FirstOrDefault(_ => _.englishName == "Brain Tree");
                if (trouble != null) trouble.englishName = "Brain Trees";

                File.WriteAllText(bioRefPath, JsonConvert.SerializeObject(this.genus));
                Game.log("prepBioRef: complete");
            }
            else
            {
                Game.log("prepBioRef: reading bioRef from disk");
                this.genus = JsonConvert.DeserializeObject<List<BioGenus>>(File.ReadAllText(bioRefPath))!;
            }

            // post-process to make each node aware of its parent
            foreach (var genusRef in this.genus)
            {
                foreach (var speciesRef in genusRef.species)
                {
                    speciesRef.genus = genusRef;
                    foreach (var variantRef in speciesRef.variants)
                        variantRef.species = speciesRef;
                }
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

            var speciesName = Util.getSpeciesPrefix(variantName);

            foreach (var genusRef in this.genus)
                foreach (var speciesRef in genusRef.species)
                    if (speciesRef.name.StartsWith(speciesName))
                        foreach (var variantRef in speciesRef.variants)
                            if (variantRef.name == variantName)
                                return new BioMatch(genusRef, speciesRef, variantRef);

            throw new Exception($"Unexpected variantName: '{variantName}' (speciesName: '{speciesName}')");
        }

        public BioMatch matchFromVariantDisplayName(string variantDisplayName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");

            var parts = variantDisplayName.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var genus = parts[0];
            var species = $"{parts[0]} {parts[1]}";
            var variant = parts[3];

            foreach (var genusRef in this.genus)
                if (genusRef.englishName == genus)
                    foreach (var speciesRef in genusRef.species)
                        if (speciesRef.englishName == species)
                            foreach (var variantRef in speciesRef.variants)
                                if (variantRef.englishName == variantDisplayName)
                                    return new BioMatch(genusRef, speciesRef, variantRef);

            throw new Exception($"Unexpected variantName: '{variantDisplayName}'");
        }

        public BioSpecies matchFromSpecies(string speciesName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");
            if (string.IsNullOrEmpty(speciesName)) throw new Exception($"Missing species name!");

            // we cannot pre-match by genus name as Brain Tree species name are not consistent with their genus names
            foreach (var genusRef in this.genus)
                foreach (var speciesRef in genusRef.species)
                    if (speciesRef.name == speciesName || speciesRef.englishName.Equals(speciesName, StringComparison.Ordinal))
                        return speciesRef;

            throw new Exception($"Unexpected speciesName: '{speciesName}'");
        }

        public BioMatch matchFromSpecies2(string speciesName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");
            if (string.IsNullOrEmpty(speciesName)) throw new Exception($"Missing species name!");

            // we cannot pre-match by genus name as Brain Tree species name are not consistent with their genus names
            foreach (var genusRef in this.genus)
                foreach (var speciesRef in genusRef.species)
                    if (speciesRef.name == speciesName || speciesRef.englishName.Equals(speciesName, StringComparison.Ordinal))
                        return new BioMatch(genusRef, speciesRef, null!);

            throw new Exception($"Unexpected speciesName: '{speciesName}'");
        }

        public BioGenus? matchFromGenus(string genusName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");
            if (string.IsNullOrEmpty(genusName)) throw new Exception($"Missing genus name!");

            var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.species.Any(_ => genusRef.name == genusName));
            return genusRef;
        }

        public bool isLegacyGenus(string genusName, string speciesName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");

            var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.species.Any(_ => _.name == speciesName) || genusRef.name == genusName);
            return genusRef?.odyssey == false;
        }

        private static List<SummaryGenus>? codexRefSummary = null;

        public List<SummaryGenus> summarizeEverything()
        {
            if (codexRefSummary == null)
            {
                codexRefSummary = new List<SummaryGenus>();
                foreach (var genusRef in this.genus)
                {
                    var summary = new SummaryGenus(genusRef, genusRef.englishName);
                    codexRefSummary.Add(summary);

                    foreach (var speciesRef in genusRef.species)
                    {
                        var shortSpeciesName = speciesRef.englishName.Replace(genusRef.englishName, "").Trim();
                        summary.species.Add(new SummarySpecies(speciesRef, shortSpeciesName));
                    }
                }
            }

            return codexRefSummary;
        }
    }
}
