using Newtonsoft.Json;
using SrvSurvey.game;
using System.Diagnostics;
using System.IO.Compression;

namespace SrvSurvey.net
{
    internal class Git
    {
        public static string pubDataFolder = Path.Combine(Program.dataFolder, "pub");
        public static string pubGuardianFolder = Path.Combine(pubDataFolder, "guardian");

        private static HttpClient client;

        static Git()
        {
            Git.client = new HttpClient();
            Git.client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            Git.client.DefaultRequestHeaders.Add("user-agent", $"SrvSurvey-{Game.releaseVersion}");
        }

        public async Task refreshPublishedData()
        {
            Game.log($"updatePubData ...");
            try
            {
                var json = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data.json");
                var pubData = JsonConvert.DeserializeObject<GitDataIndex>(json)!;
                Game.log($"pubDataSettlementTemplate - local: {Game.settings.pubDataSettlementTemplate}, remote: {pubData.settlementTemplate}");

                Directory.CreateDirectory(Git.pubDataFolder);
                Directory.CreateDirectory(pubGuardianFolder);

                if (pubData.codexRef > Game.settings.pubCodexRef)
                {
                    Game.log($"Updating CodexRef ...");
                    await Game.codexRef.init(true);

                    // update settings to current level
                    Game.settings.pubCodexRef = pubData.codexRef;
                    Game.settings.Save();
                }

                if (pubData.settlementTemplate > Game.settings.pubDataSettlementTemplate)
                {
                    Game.log($"Downloading settlementTemplates.json");
                    var json2 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/settlementTemplates.json");
                    File.WriteAllText(Path.Combine(Git.pubDataFolder, "settlementTemplates.json"), json2);

                    // update settings to current level
                    Game.settings.pubDataSettlementTemplate = pubData.settlementTemplate;
                    Game.settings.Save();
                }

                Game.log($"pubDataGuardian - local: {Game.settings.pubDataGuardian}, remote: {pubData.guardian}");
                if (pubData.guardian > Game.settings.pubDataGuardian)
                {
                    Game.log($"Downloading allRuins.json");
                    var json3 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/allRuins.json");
                    File.WriteAllText(Path.Combine(Git.pubDataFolder, "allRuins.json"), json3);

                    Game.log($"Downloading allStructuresRuins.json");
                    var json4 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/SrvSurvey/allStructures.json");
                    File.WriteAllText(Path.Combine(Git.pubDataFolder, "allStructures.json"), json4);

                    Game.log($"Downloading guardian.zip ...");
                    var url = $"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data/guardian.zip";
                    var filepath = Path.Combine(Git.pubDataFolder, "guardian.zip");
                    Game.log($"{url} => {filepath}");
                    var bytes = await Git.client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(filepath, bytes);
                    ZipFile.ExtractToDirectory(filepath, pubGuardianFolder, true);

                    // update settings to current level
                    Game.settings.pubDataGuardian = pubData.guardian;
                    Game.settings.Save();

                    // finally, re-init canonn to get updated allRuins
                    Game.canonn.init();
                    Game.log($"Downloading guardian.zip - complete");
                }
            }
            catch (Exception ex)
            {
                Game.log($"Error in updatePubData:\r\n{ex}");
                if ((ex as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(ex))
                {
                    // ignore NotFound responses
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
        }

        public void publishLocalData()
        {
            Game.log($"publishLocalData ...");
            var modifiedPubData = new List<GuardianSitePub>();
            var sites = GuardianSiteData.loadAllSitesFromAllUsers();
            var diffCount = 0;

            foreach (var site in sites)
            {
                site.loadPub();
                if (site.pubData == null)
                {
                    Game.log($"Site without pubData? '{site.displayName}'");
                    continue;
                }

                var diff = false;
                // site heading
                if (site.siteHeading != -1 && !Util.isClose(site.pubData!.sh, site.siteHeading, 1))
                {
                    diff = true;
                    site.pubData.sh = site.siteHeading;
                }
                // relic tower heading
                if (site.relicTowerHeading != -1 && !Util.isClose(site.pubData!.rh, site.relicTowerHeading, 1))
                {
                    diff = true;
                    site.pubData.rh = site.relicTowerHeading;
                }
                // location
                if (site.location != null && (site.pubData.ll == null || !Util.isClose(site.pubData.ll.Lat, site.location.Lat, 0.0001) || !Util.isClose(site.pubData.ll.Long, site.location.Long, 0.0001)))
                {
                    diff = true;
                    site.pubData.ll = site.location;
                }

                // obelisk groups
                var siteOG = string.Join("", site.obeliskGroups);
                if (site.pubData.og != siteOG)
                {
                    diff = true;
                    site.pubData.og = siteOG;
                }

                // relic tower headings
                if (site.relicHeadings.Count != site.pubData.relicTowerHeadings.Count && site.relicHeadings.Count > 0)
                {
                    diff = true;
                    site.pubData.rth = string.Join(',', site.relicHeadings.Select(_ => $"{_.Key}:{_.Value}"));
                }

                // POI status
                var template = SiteTemplate.sites[site.type];

                var poiPresent = new List<string>();
                var poiAbsent = new List<string>();
                var poiEmpty = new List<string>();
                foreach (var _ in site.poiStatus)
                {
                    var poiType = template.poi.FirstOrDefault(poi => poi.name == _.Key)?.type;
                    if (poiType == null)
                    {
                        Game.log($"POI unknown to template? At '{site.displayName}' => {_.Key}");
                        Clipboard.SetText(site.bodyName);
                        Debugger.Break();
                    }
                    else if (poiType == POIType.obelisk || poiType == POIType.brokeObelisk)
                    {
                        Game.log($"Obelisk in poiStatus? At '{site.displayName}' => {_.Key}");
                        Clipboard.SetText(site.bodyName);
                        Debugger.Break();
                    }
                    else if (_.Key == _.Key.ToUpperInvariant())
                    {
                        Game.log($"Upper case non-Obelisk POI? At '{site.displayName}' => {_.Key}");
                        Clipboard.SetText(site.bodyName);
                        Debugger.Break();
                    }

                    // (use the func, so it reads into site.pubData intentionally)
                    if (site.getPoiStatus(_.Key) == SitePoiStatus.present) poiPresent.Add(_.Key);
                    if (site.getPoiStatus(_.Key) == SitePoiStatus.absent) poiAbsent.Add(_.Key);
                    if (site.getPoiStatus(_.Key) == SitePoiStatus.empty) poiEmpty.Add(_.Key);
                }
                poiPresent.Sort();
                poiAbsent.Sort();
                poiEmpty.Sort();
                var sitePP = string.Join(',', poiPresent);
                var sitePA = string.Join(',', poiAbsent);
                var sitePE = string.Join(',', poiEmpty);

                var sumSitePoi = poiPresent.Count + poiAbsent.Count + poiEmpty.Count;
                var sumPubDataPoi = (site.pubData.pp?.Split(",").Length ?? 0) + (site.pubData.pa?.Split(",").Length ?? 0) + (site.pubData.pe?.Split(",").Length ?? 0);
                if (sumSitePoi < sumPubDataPoi)
                    Game.log($"Skipping poiStatus due suspicious counts - sumSitePoi:{sumSitePoi} vs sumPubDataPoi:{sumPubDataPoi}");
                else
                {
                    if (site.pubData.pp != sitePP && !string.IsNullOrWhiteSpace(sitePP))
                    {
                        diff = true;
                        site.pubData.pp = sitePP;
                    }
                    if (site.pubData.pa != sitePA && !string.IsNullOrWhiteSpace(sitePA))
                    {
                        diff = true;
                        site.pubData.pa = sitePA;
                    }
                    if (site.pubData.pe != sitePE && !string.IsNullOrWhiteSpace(sitePE))
                    {
                        diff = true;
                        site.pubData.pe = sitePE;
                    }
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
                    site.pubData.ao = siteObelisks.ToHashSet();
                    foreach (var ao in site.pubData.ao) ao.scanned = false;
                }

                if (site.pubData.sh == 0)
                {
                    diff = true;
                    site.pubData.sh = -1;
                }
                if (site.pubData.rh == 0)
                {
                    diff = true;
                    site.pubData.rh = -1;
                }

                if (diff)
                {
                    diffCount++;
                    var roughType = site.isRuins ? "ruins" : "structure";
                    var filepath = Path.Combine(@"D:\code\SrvSurvey\data\guardian", $"{site.bodyName}-{roughType}-{site.index}.json");
                    Game.log($"Updating pubData for: '{site.displayName}' into: {filepath}");

                    var json = JsonConvert.SerializeObject(site.pubData, Formatting.Indented);
                    File.WriteAllText(filepath, json);
                }
            }
            Game.log($"publishLocalData - complete, diffCount: {diffCount}");
            if (diffCount > 0)
                this.prepNextZip();
        }

        public void prepNextZip()
        {
            Game.log($"Updating guardian.zip");
            var sourceFolder = @"D:\code\SrvSurvey\data\guardian";
            var targetZipFile = @"D:\code\SrvSurvey\data\guardian.zip";
            if (File.Exists(targetZipFile)) File.Delete(targetZipFile);

            ZipFile.CreateFromDirectory(sourceFolder, targetZipFile, CompressionLevel.SmallestSize, false);
        }
    }

    internal class GitDataIndex
    {
        public int codexRef;
        public int settlementTemplate;
        public int guardian;
    }
}
