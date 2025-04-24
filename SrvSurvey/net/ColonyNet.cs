using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SrvSurvey.game
{
    class ColonyNet
    {
        private static string colonizationCostsPath = Path.Combine(Application.StartupPath, "colonization-costs2.json");
        public static string uxUri = "https://ravencolonial.com";
        public static string svcUri
        {
            get
            {
                if (!string.IsNullOrEmpty(Game.settings.buildProjectsUrl_TEST))
                    return Game.settings.buildProjectsUrl_TEST;
                else if (Debugger.IsAttached && Process.GetProcessesByName("RavenColonial").Length > 0)
                    return "https://localhost:7007";
                else
                    return "https://ravencolonial100-awcbdvabgze4c5cq.canadacentral-01.azurewebsites.net";
            }
        }

        private static HttpClient client;

        static ColonyNet()
        {
            ColonyNet.client = new HttpClient();
            ColonyNet.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
        }

        public List<ColonyCost2> loadDefaultCosts()
        {
            var json = File.ReadAllText(colonizationCostsPath);
            var data = JsonConvert.DeserializeObject<List<ColonyCost2>>(json)!;
            return data;
        }

        public async Task<Project?> create(ProjectCreate row)
        {
            var json1 = JsonConvert.SerializeObject(row);
            Game.log($"Colony.create:\r\n{json1}");

            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await ColonyNet.client.PutAsync($"{svcUri}/api/project/", body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Project>(json2)!;
            }
            else
            {
                Game.log($"Colony.create: failed:\n\t{json2}");
                return null;
            }
        }

        public async Task<Project> update(ProjectUpdate row)
        {
            Game.log($"Colony.update: {row}");

            var json1 = JsonConvert.SerializeObject(row);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await ColonyNet.client.PostAsync($"{svcUri}/api/project/{Uri.EscapeDataString(row.buildId)}", body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<Project>(json2)!;
            return obj;
        }

        public async Task linkCmdr(string buildId, string cmdr)
        {
            Game.log($"Colony.link: {cmdr} => {buildId}");

            var response = await ColonyNet.client.PutAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/link/{Uri.EscapeDataString(cmdr)}", null);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            await response.Content.ReadAsStringAsync();
        }

        public async Task unlinkCmdr(string buildId, string cmdr)
        {
            Game.log($"Colony.link: {cmdr} => {buildId}");

            var response = await ColonyNet.client.DeleteAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/link/{Uri.EscapeDataString(cmdr)}");
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            await response.Content.ReadAsStringAsync();
        }

        public async Task assign(string buildId, string cmdr, string commodity)
        {
            Game.log($"Colony.link: {cmdr} => {commodity}=> {buildId}");

            var response = await ColonyNet.client.PutAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/assign/{Uri.EscapeDataString(cmdr)}/{Uri.EscapeDataString(commodity)}", null);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var txt = await response.Content.ReadAsStringAsync();
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}: {txt}");
        }

        public async Task unAssign(string buildId, string cmdr, string commodity)
        {
            Game.log($"Colony.link: {cmdr} => {commodity}=> {buildId}");

            var response = await ColonyNet.client.DeleteAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/assign/{Uri.EscapeDataString(cmdr)}/{Uri.EscapeDataString(commodity)}");
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var txt = await response.Content.ReadAsStringAsync();
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}: {txt}");
        }

        public async Task<Project?> load(long id64, long marketId)
        {
            Game.log($"Colony.load: {id64}/{marketId}");
            try
            {
                var url = $"{svcUri}/api/system/{Uri.EscapeDataString(id64.ToString())}/{Uri.EscapeDataString(marketId.ToString())}";
                Game.log($"Colony.load: {url}");

                var response = await ColonyNet.client.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json)) return null;

                var obj = JsonConvert.DeserializeObject<Project>(json)!;
                return obj;
            }
            catch (Exception ex)
            {
                FormErrorSubmit.Show(ex);
                return null;
            }
        }


        public async Task contribute(string buildId, string cmdr, Dictionary<string, int> diff)
        {
            Game.log(diff.formatWithHeader($"Colony.contribute: {buildId}"));

            var json1 = JsonConvert.SerializeObject(diff);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var url = $"{svcUri}/api/project/{buildId}/contribute/{Uri.EscapeDataString(cmdr)}";
            var response = await ColonyNet.client.PostAsync(url, body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");
        }

        public async Task<Dictionary<string, int>> supplyFC(long marketId, string cargo, int delta)
        {
            return await supplyFC(marketId, new Dictionary<string, int> { { cargo, delta } });
        }

        public async Task<Dictionary<string, int>> supplyFC(long marketId, Dictionary<string, int> diff)
        {
            Game.log(diff.formatWithHeader($"Colony.supplyFC: {marketId}"));

            var json1 = JsonConvert.SerializeObject(diff);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var url = $"{svcUri}/api/fc/{Uri.EscapeDataString(marketId.ToString())}/cargo";
            var response = await ColonyNet.client.PatchAsync(url, body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<Dictionary<string, int>>(json2)!;
            return obj;
        }

        public async Task<Dictionary<string, int>> updateCargoFC(long marketId, Dictionary<string, int> cargo)
        {
            Game.log(cargo.formatWithHeader($"Colony.updateCargoFC: {marketId}"));

            var json1 = JsonConvert.SerializeObject(cargo);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var url = $"{svcUri}/api/fc/{Uri.EscapeDataString(marketId.ToString())}/cargo";
            var response = await ColonyNet.client.PostAsync(url, body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<Dictionary<string, int>>(json2)!;
            return obj;
        }

        public async Task<FleetCarrier> getFC(long marketId)
        {
            Game.log($"Colony.getFC: {marketId}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/fc/{Uri.EscapeDataString(marketId.ToString())}");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<FleetCarrier>(json)!;
            return obj;
        }

        public async Task<FleetCarrier[]> getAllCmdrFCs(string cmdr)
        {
            Game.log($"Colony.getCmdrFCAll: {cmdr}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/fc/all");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<FleetCarrier[]>(json)!;
            return obj;
        }

        public async Task<Project?> getProject(string buildId)
        {
            Game.log($"Colony.getProject: {buildId}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Project>(json)!;
            else
            {
                Game.log($"Colony.getProject: failed:\n\t{json}");
                return null;
            }
        }

        public async Task<Project?> getProject(long id64, long marketId)
        {
            Game.log($"Colony.getProject: {id64}/{marketId}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/system/{Uri.EscapeDataString(id64.ToString())}/{Uri.EscapeDataString(marketId.ToString())}");
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Project>(json)!;
            else
            {
                Game.log($"Colony.getProject: failed:\n\t{json}");
                return null;
            }
        }

        public async Task<List<Project>> getCmdrProjects(string cmdr)
        {
            Game.log($"Colony.getCmdrProjects: {cmdr}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<Project>>(json)!;
            return obj;
        }

        public async Task<CmdrSummary> getCmdrSummary(string cmdr)
        {
            Game.log($"Colony.getCmdrProjects: {cmdr}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/summary");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<CmdrSummary>(json)!;
            return obj;
        }

        public async Task<List<Project>> getCmdrActive(string cmdr)
        {
            Game.log($"Colony.getCmdrActive: {cmdr}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/active");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<Project>>(json)!;
            return obj;
        }

        public async Task<string> getPrimary(string cmdr)
        {
            Game.log($"Colony.getPrimary: {cmdr}");

            var response = await ColonyNet.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/primary");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<string>(json)!;

            return obj;
        }

        public async Task setPrimary(string cmdr, string? buildId)
        {
            Game.log($"Colony.setPrimary: {cmdr} => {buildId}");

            var response = buildId == null
                    ? await ColonyNet.client.DeleteAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/primary/")
                    : await ColonyNet.client.PutAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/primary/{Uri.EscapeDataString(buildId ?? "")}", null);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");
        }

        /*
        public async Task<string> foo1()
        {
            //var url = $"{svcUri}/api/project/a6f8bb60-b061-43bb-90e9-8f885c1e521c";
            var url = $"{svcUri}/api/system/2868099032505/3951207938";
            //var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await Colony.client.GetAsync(url);

            var json2 = await response.Content.ReadAsStringAsync();
            Game.log($"{url} =>\r\n{json2}");
            return json2;
        }

        public async Task<string> foo0()
        {
            var url = $"{svcUri}/api/project/ab84b296-cfe2-4b8f-9575-17fde152a25e/supply";
            //var url = $"{svcUri}/api/system/2868099032505/3951207938/supply";
            var obj = new
            {
                titanium = 12,
                copper = 10,
            };
            var json1 = JsonConvert.SerializeObject(obj);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await Colony.client.PostAsync(url, body);

            var json2 = await response.Content.ReadAsStringAsync();
            Game.log($"{url} =>\r\n{json2}");
            return json2;
        }

        public async Task<string> foo4()
        {
            var url = $"{svcUri}/api/project/c0db67fe-ae0a-4105-b4ce-5af993eb9a10";
            var obj = new
            {
                ETag = "W/\"datetime'2025-03-01T06%3A26%3A30.3776594Z'\"",
                buildId = "c0db67fe-ae0a-4105-b4ce-5af993eb9a10",
                buildType = "coriolis2",
                buildName = "primary-port",
                factionName = "Raven XXX Colonial Corporation",
                notes = "Hello world again!",
                commodities = new
                {
                    aluminium = new { need = 444, total = 479 },
                    ceramiccomposites = new { need = 479, total = 600 },
                    junk = new { need = 479, total = 0 },
                    //cmmcomposites = new { need = 479, total = 479 },
                    //computercomponents = new { need = 479, total = 479 },
                    //copper = new { need = 479, total = 479 },
                    //foodcartridges = new { need = 479, total = 479 },
                    //fruitandvegetables = new { need = 479, total = 479 },
                    //insulatingmembrane = new { need = 479, total = 479 },
                    //liquidoxygen = new { need = 479, total = 479 },
                    //medicaldiagnosticequipment = new { need = 479, total = 479 },
                    //nonlethalweapons = new { need = 479, total = 479 },
                    //polymers = new { need = 479, total = 479 },
                    //powergenerators = new { need = 479, total = 479 },
                    //semicondunctors = new { need = 479, total = 479 },
                    //steel = new { need = 479, total = 479 },
                    //superconductors = new { need = 479, total = 479 },
                    //titanium = new { need = 479, total = 479 },
                    //water = new { need = 479, total = 479 },
                    //waterpurifiers = new { need = 479, total = 479 },
                },
            };
            var json1 = JsonConvert.SerializeObject(obj);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await Colony.client.PostAsync(url, body);

            var json2 = await response.Content.ReadAsStringAsync();
            Game.log($"{url} =>\r\n{json2}");
            return json2;
        }

        public async Task<string> foo()
        {
            //var key = "2868099032505/3951207938";
            Game.log($"Colony.foo:");
            var obj = new
            {
                buildType = "coriolis",
                buildName = "primary-port",
                marketId = 3951207938,
                systemAddress = 2868099032505,
                systemName = "52 Herculis",
                starPos = new double[] { -131.47, 115.09, 44.00 },
                factionName = "Raven Colonial Corporation",
                notes = "Hello world!",
                //commodities = new
                //{
                //    aluminium = 479,
                //    ceramiccomposites = 499,
                //    cmmcomposites = 4353,
                //    computercomponents = 59,
                //    copper = 236,
                //    foodcartridges = 90,
                //    fruitandvegetables = 51,
                //    insulatingmembrane = 360,
                //    liquidoxygen = 1823,
                //    medicaldiagnosticequipment = 13,
                //    nonlethalweapons = 12,
                //    polymers = 517,
                //    powergenerators = 19,
                //    semicondunctors = 70,
                //    steel = 6681,
                //    superconductors = 117,
                //    titanium = 6325,
                //    water = 750,
                //    waterpurifiers = 38
                //},
                commanders = new[] { "alpha", "bravo" },
            };
            var json1 = JsonConvert.SerializeObject(obj);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await Colony.client.PutAsync($"{svcUri}/api/project/", body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            Game.log(json2);
            return json2;
        }
        // */
    }

    class AllColonizationCosts : Dictionary<string, Dictionary<string, int>> { }

    public class ColonyCost2
    {
        public string buildType;
        public string category;
        public int tier;
        public string location;
        public string displayName;
        public List<string> layouts;
        public Dictionary<string, int> cargo;

        public override string ToString()
        {
            // Used by ComboBox's
            return $"Tier {tier}: {displayName} ({string.Join(", ", layouts)})";
        }
    }

    public class ProjectCore
    {
        // Schema.Project
        public string buildType;
        public string buildName;

        public long marketId;
        public long systemAddress;
        public string systemName;
        public double[] starPos;
        public int? bodyNum;
        public string? bodyName;

        public string? factionName;
        public string? architectName;
        public int maxNeed;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, HashSet<string>> commanders = new();

        // Schema.ProjectNotes
        public string? notes;
    }

    public class ProjectCreate : ProjectCore
    {
        // Schema.ProjectCommodity
        public Dictionary<string, int>? commodities;
    }

    public class Project : ProjectCore
    {
        public DateTimeOffset? Timestamp { get; set; }
        public string ETag { get; set; }

        public string buildId;

        public int sumNeed;
        public int sumTotal;
        public bool complete;

        // Schema.ProjectCommodity
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, int> commodities = new();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HashSet<string> ready;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<ProjectFC> linkedFC;

        public override string ToString()
        {
            // Used as display name by ComboBox
            return $"{systemName}: {buildName}";
        }
    }

    public class ProjectFC
    {
        public long marketId;
        public string name;
        public string displayName;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public HashSet<string> assign = new();
    }

    public class ProjectRef
    {
        // Schema.Project
        public required string buildId;
        public required string buildType;
        public required string buildName;

        public long marketId;
        public long systemAddress;
        public required string systemName;
        public required double[] starPos;
        public int? bodyNum;
        public string? bodyName;

        public string? factionName;
        public string? architectName;
    }

    public class CmdrSummary
    {
        public string? primaryBuildId;
        public List<Project> projects;
    }

    public class ProjectUpdate
    {
        [SetsRequiredMembers]
        public ProjectUpdate(string buildId)
        {
            this.buildId = buildId;
        }

        public DateTimeOffset? Timestamp { get; set; }
        public string ETag { get; set; }

        public required string buildId;

        public string? buildType;
        public string? buildName;

        public int? bodyNum;
        public string? bodyName;

        public string? factionName;
        public string? architectName;

        public string? notes;

        public int? maxNeed;
        public Dictionary<string, int>? commodities;
    }

    public class FleetCarrier
    {
        public long marketId;
        public string name;
        public string displayName;
        public Dictionary<string, int> cargo;
    }

}
