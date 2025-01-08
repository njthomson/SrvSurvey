using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SrvSurvey.canonn
{
    internal class CodexRef
    {
        private static string codexRefPath = Path.Combine(Program.dataFolder, "codexRef.json");
        private static string bioRefPath = Path.Combine(Program.dataFolder, "bioRef.json");
        public static string defaultCodexImagesFolder = Path.Combine(Program.dataFolder, "codexImages");
        private static string nebulaePath = Path.Combine(Program.dataFolder, "nebulae.json");
        private static string codexNotFoundPath = Path.Combine(Program.dataFolder, "codexNotFound.json");

        public static Task? taskDownloadAllCodexImages;

        public List<BioGenus> genus;
        private List<double[]>? allNebula;
        private double[] lastNebulaPos;
        public int codexRefCount { get; private set; }
        public Dictionary<string, List<CodexNotFound>> codexNotFound;

        public async Task init(bool reset)
        {
            var duration = DateTime.Now - Game.settings.lastCodexRefDownload;
            Game.log($"CodexRef init (reset: {reset}, last downloaded: {duration.TotalDays.ToString("N3")} days ago) ...");

            // force a download and re-processing once a week
            if (duration.TotalDays > 7)
            {
                reset = true;
                if (File.Exists(nebulaePath)) File.Delete(nebulaePath);
            }

            // get CodexRef ready first, before running these in parallel
            var codexRef = await loadCodexRef(reset);
            prepBioRef(codexRef, reset);

            if (Game.settings.downloadCodexImageFolder != null && !Directory.Exists(Game.settings.downloadCodexImageFolder))
                Directory.CreateDirectory(Game.settings.downloadCodexImageFolder);

            await this.prepNebulae(reset);

            await this.prepCodexNotFounds(reset);

            if (Game.settings.preDownloadCodexImages && taskDownloadAllCodexImages == null)
            {
                taskDownloadAllCodexImages = Task.Run(() => Program.crashGuard(async () =>
                {
                    await this.downloadAllCodexImages(codexRef.Values.ToList());
                }));
            }

            Game.log("CodexRef init - complete");
        }

        private async Task downloadAllCodexImages(List<RefCodexEntry> codexEntries)
        {
            if (!Game.settings.preDownloadCodexImages) return;

            // get min/max bio entryId's (we don't need images of space based stuff)
            var min = 999_999_999_999D;
            var max = 0D;
            this.genus.ForEach(genus => genus.species.ForEach(species => species.variants.ForEach(variant =>
            {
                var entryId = long.Parse(variant.entryId);
                if (entryId < min) min = entryId;
                if (entryId > max) max = entryId;
            })));

            for (var n = 0; n < codexEntries.Count; n++)
            {
                // exit early if this gets cleared
                if (taskDownloadAllCodexImages == null) break;

                var entry = codexEntries[n];
                if (string.IsNullOrWhiteSpace(entry.image_url)) continue;

                var entryId = long.Parse(entry.entryid);
                if (entryId < min || entryId > max) continue;

                await downloadCodexImage(entry.entryid, entry.image_url);
                //await Task.Delay(1000);
            }

            Game.log("CodexRef pre-download images - complete");
            taskDownloadAllCodexImages = null;
        }

        public async Task downloadCodexImage(string entryId, string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            var folder = Game.settings.downloadCodexImageFolder;

            var filepath = Path.Combine(folder, $"{entryId}.jpg");
            if (!File.Exists(filepath))
            {
                Game.log($"Downloading {imageUrl} => {filepath}");

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);

                if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    //Debugger.Break();
                    return;
                }

                using (var stream = await client.GetStreamAsync(imageUrl))
                {
                    using (var imgTmp = Image.FromStream(stream))
                    {
                        if (!File.Exists(filepath))
                        {
                            var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                            var encParams = new EncoderParameters() { Param = new[] { new EncoderParameter(Encoder.Quality, 90L) } };
                            imgTmp.Save(filepath, encoder, encParams);
                        }
                    }
                }
            }

            // remove any old .png's
            filepath = Path.Combine(folder, $"{entryId}.png");
            if (File.Exists(filepath))
            {
                try { File.Delete(filepath); }
                catch (UnauthorizedAccessException) { /* ignore these */ }
            }
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
            this.codexRefCount = codexRef.Count;
            return codexRef;
        }

        public void prepBioRef(Dictionary<string, RefCodexEntry> codexRef, bool reset = false)
        {
            if (!File.Exists(bioRefPath) || reset)
            {
                Game.log("prepBioRef: (re)building from whole CodexRef ...");
                this.genus = new List<BioGenus>();
                var organicStuff = codexRef!.Values
                    .Where(_ => _.reward > 0);

                foreach (var thing in organicStuff)
                {
                    if (thing.entryid.Length != 7) throw new Exception("Bad EntryId length!");

                    // extract/create various names
                    string variantName, variantEnglishName, speciesName, speciesEnglishName, genusName = null!, genusEnglishName;

                    // scannable Thargoid things - treat like legacy?
                    if (thing.platform == "odyssey" && thing.hud_category != "Thargoid")
                    {
                        // regular odyssey things - extract/create various names
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

                        if (thing.sub_category == "$Codex_SubCategory_Thargoid;")
                        {
                            genusEnglishName = variantEnglishName;
                        }
                        else
                        {
                            var parts = thing.english_name.Split(' ', 2); // StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            genusEnglishName = parts[1]; // TODO: "Brain Tree" vs "Brain Trees" ?!
                        }

                        // special cases for legacy things
                        switch (thing.sub_class)
                        {
                            case "Anemone": genusName = "$Codex_Ent_Sphere_Name;"; break;
                            case "Amphora Plant": genusName = "$Codex_Ent_Vents_Name;"; break;
                            case "Bark Mounds": genusName = "$Codex_Ent_Cone_Name;"; break;
                            case "Brain Tree": genusName = "$Codex_Ent_Brancae_Name;"; break;
                            case "Shards": genusName = "$Codex_Ent_Ground_Struct_Ice_Name;"; break;
                            case "Tubers": genusName = "$Codex_Ent_Tube_Name;"; break;
                        }
                        // special cases for scannable Thargoid things
                        switch (thing.name)
                        {
                            case "$Codex_Ent_Thargoid_Coral_Root_Name;":
                            case "$Codex_Ent_Thargoid_Coral_Tree_Name;":
                            case "$Codex_Ent_Thargoid_Coral_Name;": // TODO: Remove this once Coral Tree is present?
                                genusName = "$Codex_Ent_Thargoid_Coral_Name;";
                                break;

                            case "$Codex_Ent_Thargoid_Barnacle_Matrix_Name;":
                                genusName = "$Codex_Ent_Barnacles_Name;";
                                break;

                            // all the Spires?
                            case "$Codex_Ent_Thargoid_Tower_Name;":
                            case "$Codex_Ent_Thargoid_Tower_Low_Name;":
                            case "$Codex_Ent_Thargoid_Tower_Med_Name;":
                            case "$Codex_Ent_Thargoid_Tower_High_Name;":
                            case "$Codex_Ent_Thargoid_Tower_ExtraHigh_Name;":
                                genusName = "$Codex_Ent_Thargoid_Tower_Name;";
                                break;
                        }

                        if (genusName == null) throw new Exception($"Oops: {thing.sub_class}?");
                    }

                    // match or create the Genus
                    var genusRef = this.genus.FirstOrDefault(_ => _.name == genusName);
                    if (genusRef == null)
                    {
                        genusRef = new BioGenus()
                        {
                            name = genusName,
                            englishName = genusEnglishName,
                            dist = BioScan.getRange(genusName),
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

                // special case for Legacy Genus names
                this.genus.First(_ => _.englishName == "Brain Tree").englishName = "Brain Trees";
                this.genus.First(_ => _.englishName == "Mounds").englishName = "Bark Mounds";
                this.genus.First(_ => _.englishName == "Anemone").englishName = "Luteolum Anemone";
                this.genus.First(_ => _.englishName == "Plant").englishName = "Amphora Plant";
                //this.genus.First(_ => _.englishName == "Sinuous Tubers").englishName = "Tubers";
                this.genus.First(_ => _.englishName == "Shards").englishName = "Crystalline Shards";

                File.WriteAllText(bioRefPath, JsonConvert.SerializeObject(this.genus, Formatting.Indented));
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

        public BioMatch matchFromEntryId(string entryId, bool allowNull = false)
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

            if (allowNull) return null!;
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

        public BioMatch? matchFromVariantDisplayName(string variantDisplayName)
        {
            if (this.genus == null || this.genus.Count == 0) throw new Exception($"BioRef is not loaded.");

            var parts = variantDisplayName.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 4)
            {
                // for Odyssey organisms, search more efficiently
                var genus = parts[0];
                var species = $"{parts[0]} {parts[1]}";

                foreach (var genusRef in this.genus)
                    if (genusRef.englishName == genus)
                        foreach (var speciesRef in genusRef.species)
                            if (speciesRef.englishName == species)
                                foreach (var variantRef in speciesRef.variants)
                                    if (variantRef.englishName == variantDisplayName)
                                        return new BioMatch(genusRef, speciesRef, variantRef);

                return null;
            }

            // for legacy organisms, search less efficiently
            foreach (var genusRef in this.genus)
                if (!genusRef.odyssey)
                    foreach (var speciesRef in genusRef.species)
                        foreach (var variantRef in speciesRef.variants)
                            if (variantRef.englishName == variantDisplayName)
                                return new BioMatch(genusRef, speciesRef, variantRef);

            return null;
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

            var genusRef = Game.codexRef.genus.FirstOrDefault(genusRef => genusRef.name == genusName);
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
                        summary.species.Add(new SummarySpecies(speciesRef, speciesRef.displayName));
                }
            }

            return codexRefSummary;
        }

        #region nebula

        private async Task<List<double[]>> prepNebulae(bool reset = false)
        {
            if (this.allNebula != null)
            {
                Game.log("prepNebulae: from memory ...");
                return this.allNebula;
            }

            // StellarPOIs
            if (!File.Exists(nebulaePath) || reset)
            {
                Game.log("prepNebulae: from network ...");

                var csv = await new HttpClient().GetStringAsync("https://edastro.b-cdn.net/mapcharts/files/nebulae-coordinates.csv");
                this.allNebula = csv.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Skip(1)
                    .Select(line =>
                    {
                        var parts = line.Split(',');
                        return new double[3]
                        {
                            // Columns are: Name, System, X, Y, Z, Type
                            double.Parse(parts[2], CultureInfo.InvariantCulture), // x
                            double.Parse(parts[3], CultureInfo.InvariantCulture), // y
                            double.Parse(parts[4], CultureInfo.InvariantCulture), // z
                        };
                    })
                    .ToList();
                File.WriteAllText(nebulaePath, JsonConvert.SerializeObject(this.allNebula));

                Game.log("prepNebulae: complete");
                return this.allNebula;
            }
            else
            {
                Game.log("prepNebulae: from disk ...");
                var json = File.ReadAllText(nebulaePath);
                this.allNebula = JsonConvert.DeserializeObject<List<double[]>>(json)!;
                return this.allNebula;
            }
        }

        private async Task<Tuple<string, string>> getPoiUrlDetails()
        {
            // fetch HTML and JS files to get the expected timestamp
            var html = await new HttpClient().GetStringAsync("https://edastro.com/galmap/");

            var r0 = new Regex(@"<script src=""/galmap/galmap.js\?ver=(.*?)""></script>", RegexOptions.Compiled);
            var m0 = r0.Match(html);
            var jsTimestamp = m0.Groups[1].Value;


            var jsCode = await new HttpClient().GetStringAsync($"https://edastro.com/galmap/galmap.js?ver={jsTimestamp}");
            var r1 = new Regex("var cdn = 'https://(.*?)';");
            var r2 = new Regex("var timestamp = '(.*?)';", RegexOptions.Compiled);

            var m1 = r1.Match(jsCode);
            var m2 = r2.Match(jsCode);

            return new Tuple<string, string>(
                m1.Groups[1].Value,
                m2.Groups[1].Value
            );
        }

        public async Task<double> getDistToClosestNebula(double[] systemPos, int maxDistance = 100)
        {
            // if we had a hit previously, use that first
            if (lastNebulaPos != null)
            {
                var d1 = Util.getSystemDistance(systemPos, lastNebulaPos);
                if (d1 < maxDistance)
                    return d1;
            }

            var vectors = await prepNebulae(false);
            var nebularDist = vectors.Min(v => Util.getSystemDistance(systemPos, v));
            return nebularDist;
        }

        private static List<double[]> smallGuardianBubbles = new List<double[]>()
        {
            new double[] { -9298.6875, -419.40625, 7911.15625 }, // Prai Hypoo OK-I b0
            new double[] { -5479.28125, -574.84375, 10468.96875 }, // Prua Phoe US-B d58 
            new double[] { 1228.1875, -694.5625, 12341.65625 }, // Blaa Hypai EK-C c14-1
            new double[] { 4961.1875, 158.09375, 20642.65625 }, // Eorl Auwsy SY-Z d13-3643
            new double[] { 14602.75, -237.90625, 3561.875 }, // NGC 3199 Sector JH-V c2-0
            new double[] { 8649.125, -154.71875, 2686.03125 }, // Eta Carina Sector EL-Y d1
        };

        public bool isWithinGuardianBubble(double[] systemPos)
        {
            // two big bubbles - 750ly
            var gammaVelorum = new StarPos(1099.21875, -146.6875, -133.59375);
            var dist = Util.getSystemDistance(systemPos, gammaVelorum);
            if (dist < 750) return true;

            var hen2333 = new StarPos(-840.65625, -561.15625, 13361.8125);
            dist = Util.getSystemDistance(systemPos, hen2333);
            if (dist < 750) return true;

            // six small bubbles - 100ly
            foreach (var bubblePos in smallGuardianBubbles)
            {
                dist = Util.getSystemDistance(systemPos, bubblePos);
                if (dist < 100) return true;
            }

            return false;
        }

        #endregion

        #region missing codex items

        public bool isRegionalNewDiscovery(string galacticRegion, string entryId)
        {
            return isRegionalNewDiscovery(galacticRegion, long.Parse(entryId));
        }

        public bool isRegionalNewDiscovery(string galacticRegion, long entryId)
        {
            var isNewDiscovery = this.codexNotFound[galacticRegion].Any(e => e.entryId == entryId);
            return isNewDiscovery;
        }

        private async Task<Dictionary<string, List<CodexNotFound>>> prepCodexNotFounds(bool reset)
        {
            if (!File.Exists(codexNotFoundPath) || reset)
            {
                Game.log("prepCodexNotFounds: preparing from network ...");
                var csv = await new HttpClient().GetStringAsync("https://docs.google.com/spreadsheets/d/1TpPZUFd61KUQWy1sV8VhScZiVbRWJ435wTN8xjN0Qv0/gviz/tq?tqx=out:csv&sheet=Individual+Items");

                this.codexNotFound = parseNotFountCsv(csv);
                var json = JsonConvert.SerializeObject(this.codexNotFound, Formatting.Indented);
                File.WriteAllText(codexNotFoundPath, json);

                Game.log("prepCodexNotFounds: complete");
                Game.settings.lastCodexNotFoundDownload = DateTime.Now;
                return this.codexNotFound;
            }
            else
            {
                Game.log("prepCodexNotFounds: reading from disk");
                var json = File.ReadAllText(codexNotFoundPath)!;
                this.codexNotFound = JsonConvert.DeserializeObject<Dictionary<string, List<CodexNotFound>>>(json)!;
                return this.codexNotFound;
            }
        }

        private Dictionary<string, List<CodexNotFound>> parseNotFountCsv(string csv)
        {
            var lines = csv.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var parsed = lines
                .Skip(1)
                .Select(line => CodexNotFoundRow.parse(line))
                // Found==0,NotExpectedToBeFound==0
                .Where(entry => !entry.Found && !entry.NotExpectedToBeFound);

            var data = new Dictionary<string, List<CodexNotFound>>();
            foreach (var entry in parsed)
            {
                var regionName = GalacticRegions.getNameFromIdx(entry.RegionID);
                if (!data.ContainsKey(regionName)) data[regionName] = new List<CodexNotFound>();
                data[regionName].Add(new CodexNotFound() { entryId = entry.EntryID, variant = entry.Varient });
            }

            // sort the lists of entryId's
            foreach (var regionName in data.Keys)
            {
                var sorted = data[regionName].OrderBy(entry => entry.entryId).ToList();
                data[regionName] = sorted;
            }

            // sort the dictionary
            data = data.OrderBy(_ => GalacticRegions.getIdxFromName(_.Key))
                .ToDictionary(_ => _.Key, _ => _.Value);

            return data;
        }

        [JsonConverter(typeof(CodexNotFound.JsonConverter))]
        public class CodexNotFound
        {
            public long entryId;
            public string variant;

            public override string ToString()
            {
                return $"{entryId}_{variant}";
            }

            class JsonConverter : Newtonsoft.Json.JsonConverter
            {
                public override bool CanConvert(Type objectType) { return false; }

                public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
                {
                    var txt = serializer.Deserialize<string>(reader);
                    if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                    // "{entryId}_{variant}"
                    // eg: "2310111_Wolf Rayet"
                    var parts = txt.Split('_');

                    var data = new CodexNotFound()
                    {
                        entryId = long.Parse(parts[0]),
                        variant = parts[1],
                    };

                    return data;
                }

                public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                {
                    var data = value as CodexNotFound;
                    if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                    // create a single string
                    var txt = data.ToString();
                    writer.WriteValue(txt);
                }
            }
        }

        public class CodexNotFoundRow
        {
            public int RegionID;
            public string RegionName;
            public string EnglishName;
            public bool Found;
            public bool NotExpectedToBeFound;
            public long EntryID;
            public string Name;
            public string Varient;

            public static CodexNotFoundRow parse(string line)
            {
                try
                {
                    var txt = line.Substring(1) + ",";
                    var parts = txt.Split("\",\"", StringSplitOptions.TrimEntries);

                    long entryId;
                    string name;
                    if (string.IsNullOrEmpty(parts[5]))
                    {
                        var match = Game.codexRef.matchFromVariantDisplayName(parts[2])!;
                        entryId = match.entryId;
                        name = match.variant.name;
                    }
                    else
                    {
                        entryId = long.Parse(parts[5]);
                        name = parts[6];
                    }

                    var entry = new CodexNotFoundRow()
                    {
                        RegionID = int.Parse(parts[0]),
                        RegionName = parts[1],
                        EnglishName = parts[2],
                        Found = parts[3] == "1",
                        NotExpectedToBeFound = parts[4] == "1",
                        EntryID = entryId,
                        Name = name,
                        Varient = parts[7],
                    };
                    return entry;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to parse: {ex.Message}\r\n{line}");
#pragma warning disable CA2200 // Rethrow to preserve stack details
                    throw ex;
#pragma warning restore CA2200 // Rethrow to preserve stack details
                }
            }

            public override string ToString()
            {
                return $"{EnglishName} / {RegionName} / found:{Found}, notExpected:{NotExpectedToBeFound}, variant: {Varient}";
            }
        }

        #endregion
    }
}
