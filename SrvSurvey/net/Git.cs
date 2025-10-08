using BioCriterias;
using Newtonsoft.Json;
using SrvSurvey.game;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;

namespace SrvSurvey.net
{
    internal class Git
    {
        public static string pubDataFolder = Path.Combine(Program.dataFolder, "pub");
        public static string pubGuardianFolder = Path.Combine(pubDataFolder, "guardian");
        public static string pubBioCriteriaFolder = Path.Combine(pubDataFolder, "bio-criteria");
        public static string pubSettlementsFolder = Path.Combine(pubDataFolder, "settlements");

        public static string srcRootFolder = Path.Combine(Application.StartupPath, "..\\..\\..\\..\\..");
        private static string devGitDataFilepath = Path.Combine(srcRootFolder, "data.json");
        public static string boxelNamesPath = Path.Combine(pubDataFolder, "Boxel.Names.txt");

        private static HttpClient client;

        static Git()
        {
            Git.client = new HttpClient(Util.getResilienceHandler());
            Git.client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            Git.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        public async Task<bool> refreshPublishedData()
        {
            var updateAvailable = false;

            Game.log($"updatePubData ...");
            try
            {
                var json = await Git.client.GetStringAsync($"https://njthomson.github.io/SrvSurvey/data.json");
                var pubData = JsonConvert.DeserializeObject<GitDataIndex>(json)!;

                var hadNoPubFolder = !Directory.Exists(pubDataFolder);
                Game.log($"pubDataSettlementTemplate - local: {Game.settings.pubDataSettlementTemplate}, remote: {pubData.settlementTemplate}, hadNoPubFolder: {hadNoPubFolder}");

                Directory.CreateDirectory(Git.pubDataFolder);
                Directory.CreateDirectory(Git.pubGuardianFolder);

                // decide if an update is available
                if (!Debugger.IsAttached)
                {
                    var currentVersion = Version.Parse(Program.releaseVersion);
                    if (Program.isAppStoreBuild && pubData.msVer > currentVersion)
                        updateAvailable = true;
                    else if (!Program.isAppStoreBuild && pubData.ghVer > currentVersion)
                        updateAvailable = true;
                }

                if (pubData.codexRef > Game.settings.pubCodexRef)
                {
                    Game.log($"Updating CodexRef ...");
                    await Game.codexRef.init(true);

                    // update settings to current level
                    Game.settings.pubCodexRef = pubData.codexRef;
                    Game.settings.Save();
                }

                if (hadNoPubFolder || (pubData.bioCriteria > Game.settings.pubBioCriteria && BioCriteria.engVer >= pubData.bioEngine))
                {
                    Game.log($"Updating bio-criteria ...");
                    await this.updateBioCriteria();

                    // update settings to current level
                    Game.settings.pubBioCriteria = pubData.bioCriteria;
                    Game.settings.Save();
                }

                if (hadNoPubFolder || pubData.settlements > Game.settings.pubSettlements)
                {
                    Game.log($"Updating settlements ...");
                    await this.updateHumanSettlements();

                    // then re-import
                    HumanSiteTemplate.import(Debugger.IsAttached);

                    // update settings to current level
                    Game.settings.pubSettlements = pubData.settlements;
                    Game.settings.Save();
                }

                if (hadNoPubFolder || (pubData.settlementTemplate > Game.settings.pubDataSettlementTemplate))
                {
                    Game.log($"Downloading {GuardianSiteTemplate.filename}");
                    var json2 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/{GuardianSiteTemplate.filename}");
                    File.WriteAllText(Path.Combine(Git.pubDataFolder, GuardianSiteTemplate.filename), json2);

                    this.updateRawPoiAfterRefresh();
                    // then re-import
                    GuardianSiteTemplate.Import();

                    // update settings to current level
                    Game.settings.pubDataSettlementTemplate = pubData.settlementTemplate;
                    Game.settings.Save();
                }

                if (!File.Exists(Git.boxelNamesPath))
                {
                    Game.log($"Downloading Boxel.Names.txt ...");
                    var txt = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/game/Boxel.Names.txt");
                    File.WriteAllText(Git.boxelNamesPath, txt);
                    Game.log($"Downloading Boxel.Names.txt - complete");
                }

                Game.log($"pubDataGuardian - local: {Game.settings.pubDataGuardian}, remote: {pubData.guardian}");
                if (hadNoPubFolder || (pubData.guardian > Game.settings.pubDataGuardian))
                {
                    Game.log($"Downloading allRuins.json");
                    var json3 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/allRuins.json");
                    File.WriteAllText(Path.Combine(Git.pubDataFolder, "allRuins.json"), json3);

                    Game.log($"Downloading allStructuresRuins.json");
                    var json4 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/allStructures.json");
                    File.WriteAllText(Path.Combine(Git.pubDataFolder, "allStructures.json"), json4);

                    // Remove folder, to remove any orphan files
                    if (Directory.Exists(Git.pubGuardianFolder))
                        Directory.Delete(Git.pubGuardianFolder, true);

                    Game.log($"Downloading guardian.zip ...");
                    var url = $"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data/guardian.zip";
                    var filepath = Path.Combine(Git.pubDataFolder, "guardian.zip");
                    Game.log($"{url} => {filepath}");
                    var bytes = await Git.client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(filepath, bytes);
                    ZipFile.ExtractToDirectory(filepath, Git.pubGuardianFolder, true);

                    // update settings to current level
                    Game.settings.pubDataGuardian = pubData.guardian;
                    Game.settings.Save();

                    // finally, re-init Canonn to get updated allRuins
                    Game.canonn.init();
                    Game.log($"Downloading guardian.zip - complete");

                }
            }
            catch (Exception ex)
            {
                Game.log($"Error in updatePubData:\r\n{ex}");
                var reqEx = ex as HttpRequestException;
                if (reqEx?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(ex) || reqEx?.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    // swallow NotFound/TooManyRequests responses or problems caused by firewalls
                }
                else
                {
                    FormErrorSubmit.Show(ex);
                }
            }
            finally
            {
                Game.log($"updatePubData - complete");
            }
            return updateAvailable;
        }

        public static void updateDevGitData(Action<GitDataIndex> func)
        {
            // read
            var pubJson = File.ReadAllText(devGitDataFilepath);
            var pubData = JsonConvert.DeserializeObject<GitDataIndex>(pubJson)!;

            // allow calling code to manipulate
            func(pubData);

            // write into both locations
            var newPubJson = JsonConvert.SerializeObject(pubData, Formatting.Indented);
            File.WriteAllText(devGitDataFilepath, newPubJson);
            var docsPath = Path.Combine(srcRootFolder, "docs", "data.json");
            File.WriteAllText(docsPath, newPubJson);
        }

        private void updateRawPoiAfterRefresh()
        {
            if (string.IsNullOrEmpty(Game.settings.lastFid)) return;

            var sites = GuardianSiteData.loadAllSites(false);

            foreach (var site in sites)
            {
                if (site.rawPoi == null || site.rawPoi.Count == 0) continue;
                var template = GuardianSiteTemplate.sites[site.type];

                var initialRawCount = site.rawPoi.Count;
                for (var n = 0; n < site.rawPoi.Count; n++)
                {
                    var raw = site.rawPoi[n];
                    var match = template.findPoiAtLocation(raw.angle, raw.dist, raw.type, true);
                    if (match != null)
                    {
                        // if a match is found - promote rawPoi into regular
                        site.poiStatus[match.name] = raw.type == POIType.unknown ? SitePoiStatus.empty : SitePoiStatus.present;
                        if (raw.type == POIType.relic && raw.rot > -1)
                            site.relicHeadings[match.name] = (int)raw.rot;

                        site.rawPoi.RemoveAt(n);
                        n--;
                    }
                }

                Game.log($"Removed {initialRawCount - site.rawPoi.Count} rawPoi, {site.rawPoi.Count} remaining for: {site.displayName}");
                if (initialRawCount != site.rawPoi.Count)
                    site.Save();
            }
        }

        public void publishLocalData()
        {
            Game.log($"publishLocalData ...");
            var modifiedPubData = new List<GuardianSitePub>();
            var sites = GuardianSiteData.loadAllSitesFromAllUsers(true);
            var diffCount = 0;
            var templateChanged = false;

            foreach (var site in sites)
            {
                site.loadPub();
                if (site.pubData == null)
                {
                    Game.log($"Site without pubData? '{site.displayName}'");
                    continue;
                }

                // POI status
                var template = GuardianSiteTemplate.sites[site.type];

                // first - handle any rawPOIs ...
                if (site.rawPoi != null)
                {
                    foreach (var raw in site.rawPoi)
                    {
                        // all obelisks should be mapped at this point
                        if (raw.type == POIType.obelisk || raw.type == POIType.brokeObelisk) { continue; }

                        // skip anything that needs a `rot` value and it's missing
                        if (raw.rot < 0 && (raw.type == POIType.pylon || raw.type == POIType.component)) { continue; }

                        // does this raw POI match something now known to the template?
                        var match = template.findPoiAtLocation(raw.angle, raw.dist, raw.type, false);
                        if (match == null)
                        {
                            // raw POI does NOT match something from the template, so let's add it with a corrected name
                            raw.name = template.nextNameForNewPoi(raw.type);
                            template.poi.Add(raw);
                            templateChanged = true;
                        }
                        else
                        {
                            // switch this name, so we add that matched name on the few lines below
                            raw.name = match.name;
                        }

                        // add now consider that present
                        site.poiStatus[raw.name] = raw.type == POIType.unknown ? SitePoiStatus.empty : SitePoiStatus.present;
                        if (raw.type == POIType.relic && raw.rot != -1)
                            site.relicHeadings[raw.name] = (int)raw.rot;
                    }
                }

                var diff = false;
                // site heading
                if (site.siteHeading != -1 && !Util.isClose(site.pubData!.sh, site.siteHeading, 1m))
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: site heading\r\n\t> {site.pubData.sh}\r\n\t> {site.siteHeading}");
                    site.pubData.sh = site.siteHeading;
                }
                // relic tower heading
                if (site.relicTowerHeading != -1 && !Util.isClose(site.pubData!.rh, site.relicTowerHeading, 1m))
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: relic heading\r\n\t> {site.pubData.rh}\r\n\t> {site.relicTowerHeading}");
                    site.pubData.rh = site.relicTowerHeading;
                }
                // location
                if (site.location != null && (site.pubData.ll == null || !Util.isClose(site.pubData.ll.Lat, site.location.Lat, 0.0001m) || !Util.isClose(site.pubData.ll.Long, site.location.Long, 0.0001m)))
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: location\r\n\t> {site.pubData.ll}\r\n\t> {site.location}");
                    site.pubData.ll = site.location;
                }

                // obelisk groups
                var siteOG = string.Join("", site.obeliskGroups);
                if (site.pubData.og != siteOG)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: obelisk groups\r\n\t> {site.pubData.og}\r\n\t> {siteOG}");
                    site.pubData.og = siteOG;
                }

                // relic tower headings - merging
                if (site.relicHeadings.Count > 0 && string.Join(',', site.relicHeadings.Keys.Order()) != string.Join(',', site.pubData.relicTowerHeadings.Keys.Order()))
                {
                    // add only towers that are missing
                    foreach (var _ in site.relicHeadings)
                    {
                        if (!site.pubData.relicTowerHeadings.ContainsKey(_.Key))
                        {
                            diff = true;
                            site.pubData.relicTowerHeadings[_.Key] = _.Value;
                        }
                    }

                    site.pubData.rth = string.Join(',', site.pubData.relicTowerHeadings.OrderBy(_ => int.Parse(_.Key.Substring(1), CultureInfo.InvariantCulture)).Select(_ => $"{_.Key}:{_.Value}"));
                }

                // trim any poi that appear to be obelisks - occasionally they are reported at present, absent or empty but we don't care about them in this regard
                var bogus = site.poiStatus.Keys.Where(k => k.Length == 3 && char.IsUpper(k[0]) && char.IsDigit(k[1]) && char.IsDigit(k[2]));
                Game.log(bogus.formatWithHeader($"Ignoring {bogus.Count()} bogus items:"));
                foreach (var key in bogus)
                    site.poiStatus.Remove(key);

                var poiPresent = new HashSet<string>(template.poi.Where(p => site.getPoiStatus(p.name) == SitePoiStatus.present).Select(p => p.name));
                var poiAbsent = new HashSet<string>(template.poi.Where(p => site.getPoiStatus(p.name) == SitePoiStatus.absent).Select(p => p.name));
                var poiEmpty = new HashSet<string>(template.poi.Where(p => site.getPoiStatus(p.name) == SitePoiStatus.empty).Select(p => p.name));
                foreach (var _ in site.poiStatus)
                {
                    var poi = template.poi.FirstOrDefault(pt => pt.name == _.Key);
                    if (poi == null)
                    {
                        Game.log($"POI unknown to template? At '{site.displayName}' => {_.Key}");
                        Clipboard.SetText(site.bodyName);
                        Debugger.Break();
                        continue;
                    }
                    else if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
                    {
                        Game.log($"Obelisk in poiStatus? At '{site.displayName}' => {_.Key}");
                        continue;
                        //Clipboard.SetText(site.bodyName);
                        //Debugger.Break();
                    }
                    else if (_.Key == _.Key.ToUpperInvariant())
                    {
                        Game.log($"Upper case non-Obelisk POI? At '{site.displayName}' => {_.Key}");
                        Clipboard.SetText(site.bodyName);
                        Debugger.Break();
                    }
                }

                var sitePP = string.Join(',', poiPresent.Order());
                var sitePA = string.Join(',', poiAbsent.Order());
                var sitePE = string.Join(',', poiEmpty.Order());
                //if (site.pubData.sid == "GR134") Debugger.Break();

                // recalc known surveyable POIs if the template changed
                if (templateChanged)
                    template.poiSurvey = template.poi.Where(_ => _.type != POIType.obelisk && _.type != POIType.brokeObelisk).ToList();

                var sumSitePoi = poiPresent.Count + poiAbsent.Count + poiEmpty.Count;
                if (sumSitePoi > template.poiSurvey.Count)
                {
                    Game.log($"Skipping poiStatus due suspicious counts\r\n\tsumSitePoi:{sumSitePoi} vs sumPubDataPoi:{template.poiSurvey.Count}, {template.name}, {Path.GetFileName(site.filepath)}");
                    Debugger.Break();
                }
                if ((site.pubData.pp ?? "") != sitePP)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: poiPresent\r\n\t> {site.pubData.pp}\r\n\t> {sitePP}");
                    site.pubData.pp = string.IsNullOrWhiteSpace(sitePP) ? null! : sitePP;
                }
                if ((site.pubData.pa ?? "") != sitePA)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: poiAbsent\r\n\t> {site.pubData.pa}\r\n\t> {sitePA}");
                    site.pubData.pa = string.IsNullOrWhiteSpace(sitePA) ? null! : sitePA;
                }
                if ((site.pubData.pe ?? "") != sitePE)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: poiEmpty\r\n\t> {site.pubData.pe}\r\n\t> {sitePE}");
                    site.pubData.pe = string.IsNullOrWhiteSpace(sitePE) ? null! : sitePE;
                }

                // active obelisks
                var siteObelisks = template.poi
                    .Where(_ => _.type == POIType.obelisk)
                    .Select(_ => site.getActiveObelisk(_.name)!)
                    .Where(_ => _ != null)
                    .OrderBy(_ => _.name)
                    .ToHashSet();

                var obNamesList = siteObelisks.Select(_ => _.name).ToList();
                var obNamesSet = obNamesList.ToHashSet();
                if (obNamesSet.Count != obNamesList.Count)
                {
                    // we need to remove and reparse obelisks for this
                    Game.log($"Need to rerun publish to correct obelisk data!");
                    Debugger.Break();
                    siteObelisks = new HashSet<ActiveObelisk>();
                }

                var siteObelisksJson = JsonConvert.SerializeObject(siteObelisks).Replace("!", "");
                var pubDataObelisksJson = JsonConvert.SerializeObject(site.pubData.ao.OrderBy(_ => _.name));
                if (siteObelisks != null && siteObelisksJson != pubDataObelisksJson && siteObelisks.Count >= site.pubData.ao.Count && siteObelisksJson.Length > pubDataObelisksJson.Length)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: obelisks\r\n\t> {pubDataObelisksJson}\r\n\t> {siteObelisksJson}");
                    site.pubData.ao = siteObelisks.ToHashSet();
                    foreach (var ao in site.pubData.ao) ao.scanned = false;
                }

                if (site.pubData.sh == 0)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: site heading\r\nwas zero?");
                    site.pubData.sh = -1;
                }
                if (site.pubData.rh == 0)
                {
                    diff = true;
                    Game.log($"DIFF: {site.displayName}: relic heading\r\nwas zero?");
                    site.pubData.rh = -1;
                }

                if (diff)
                {
                    diffCount++;
                    var roughType = site.isRuins ? "ruins" : "structure";
                    var filepath = Path.GetFullPath(Path.Combine(srcRootFolder, "data", "guardian", $"{site.bodyName}-{roughType}-{site.index}.json"));
                    Game.log($"Updating pubData for: '{site.displayName}' into: {filepath}");

                    var json = JsonConvert.SerializeObject(site.pubData, Formatting.Indented);
                    File.WriteAllText(filepath, json);
                }
            }

            if (templateChanged)
            {
                GuardianSiteTemplate.SaveEdits();
                GuardianSiteTemplate.publish();
            }
            Game.log($"publishLocalData - complete, diffCount: {diffCount}");
            if (diffCount > 0)
            {
                var sourceFolder = Path.GetFullPath(Path.Combine(srcRootFolder, "data", "guardian"));
                var targetZipFile = Path.GetFullPath(Path.Combine(srcRootFolder, "data", "guardian.zip"));

                if (File.Exists(targetZipFile)) File.Delete(targetZipFile);
                ZipFile.CreateFromDirectory(sourceFolder, targetZipFile, CompressionLevel.SmallestSize, false);

                // increment version in data.json
                Git.updateDevGitData(pubData => pubData.guardian++);
            }
        }

        public void publishBioCriteria()
        {
            Game.log($"publishBioCriteria ...");

            var criteriaFolder = Path.GetFullPath(Path.Combine(srcRootFolder, "SrvSurvey", "bio-criteria"));
            var targetZipFile = Path.GetFullPath(Path.Combine(srcRootFolder, "data", "bio-criteria.zip"));

            // update .zip file
            if (File.Exists(targetZipFile)) File.Delete(targetZipFile);
            ZipFile.CreateFromDirectory(criteriaFolder, targetZipFile, CompressionLevel.SmallestSize, false);

            // increment version in data.json
            Git.updateDevGitData(pubData => pubData.bioCriteria++);

            Game.log($"publishBioCriteria - complete");
        }

        public async Task updateBioCriteria()
        {
            Game.log($"Downloading bio-criteria.zip ...");
            var url = $"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data/bio-criteria.zip";
            var filepath = Path.Combine(Git.pubDataFolder, "bio-criteria.zip");

            Game.log($"{url} => {filepath}");
            var bytes = await Git.client.GetByteArrayAsync(url);

            // Make a backup of the prior criteria (in case there's a code bug with the new ones)
            if (File.Exists(filepath))
                File.Copy(filepath, Path.Combine(Git.pubDataFolder, "bio-criteria-prior.zip"), true);

            // Remove folder, to remove any orphan files
            if (Directory.Exists(Git.pubBioCriteriaFolder))
                Directory.Delete(Git.pubBioCriteriaFolder, true);

            await File.WriteAllBytesAsync(filepath, bytes);
            ZipFile.ExtractToDirectory(filepath, Git.pubBioCriteriaFolder, true);
        }

        public void publishHumanSettlements()
        {
            Game.log($"publishHumanSettlements ...");

            var settlementsFolder = Path.GetFullPath(Path.Combine(srcRootFolder, "SrvSurvey", "settlements"));
            var targetZipFile = Path.GetFullPath(Path.Combine(srcRootFolder, "data", "settlements.zip"));

            // update .zip file
            if (File.Exists(targetZipFile)) File.Delete(targetZipFile);
            ZipFile.CreateFromDirectory(settlementsFolder, targetZipFile, CompressionLevel.SmallestSize, false);

            // increment version in data.json
            Git.updateDevGitData(pubData => pubData.settlements++);

            Game.log($"publishHumanSettlements - complete");
        }

        public async Task updateHumanSettlements()
        {
            Game.log($"Downloading settlements.zip ...");
            var url = $"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data/settlements.zip";
            var filepath = Path.Combine(Git.pubDataFolder, "settlements.zip");

            Game.log($"{url} => {filepath}");
            var bytes = await Git.client.GetByteArrayAsync(url);

            // Make a backup of the prior criteria (in case there's a code bug with the new ones)
            if (File.Exists(filepath))
                File.Copy(filepath, Path.Combine(Git.pubDataFolder, "settlements-prior.zip"), true);

            // Remove folder, to remove any orphan files
            if (Directory.Exists(Git.pubSettlementsFolder))
                Directory.Delete(Git.pubSettlementsFolder, true);

            await File.WriteAllBytesAsync(filepath, bytes);
            ZipFile.ExtractToDirectory(filepath, Git.pubSettlementsFolder, true);
        }
    }

    internal class GitDataIndex
    {
        public Version ghVer;
        public Version msVer;
        public int bioCriteria;
        public int bioEngine;
        public int codexRef;
        public int settlementTemplate;
        public int guardian;
        public int settlements;
    }
}
