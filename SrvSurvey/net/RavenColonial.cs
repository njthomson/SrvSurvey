using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SrvSurvey.game.RavenColonial
{
    class RavenColonial
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

        static RavenColonial()
        {
            RavenColonial.client = new HttpClient(Util.getResilienceHandler());
            RavenColonial.client.DefaultRequestHeaders.Add("user-agent", Program.userAgent);
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
            Game.log($"RCC.create:\r\n{json1}");

            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await RavenColonial.client.PutAsync($"{svcUri}/api/project/", body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Project>(json2)!;
            }
            else
            {
                Game.log($"RCC.create: failed:\n\t{json2}");
                return null;
            }
        }

        public async Task<Project> updateProject(ProjectUpdate row)
        {
            Game.log($"RCC.update: {row}");

            var json1 = JsonConvert.SerializeObject(row);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await RavenColonial.client.PostAsync($"{svcUri}/api/project/{Uri.EscapeDataString(row.buildId)}", body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                Game.log($"RCC.updateProject'{row.buildId}' failed: HTTP:{(int)response.StatusCode}({response.StatusCode}): {json2}");
            var obj = JsonConvert.DeserializeObject<Project>(json2)!;
            return obj;
        }

        public async Task markComplete(string buildId)
        {
            Game.log($"RCC.markComplete: {buildId}");

            var response = await RavenColonial.client.PostAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/complete", null);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            await response.Content.ReadAsStringAsync();
        }

        public async Task linkCmdr(string buildId, string cmdr)
        {
            Game.log($"RCC.link: {cmdr} => {buildId}");

            var response = await RavenColonial.client.PutAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/link/{Uri.EscapeDataString(cmdr)}", null);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            await response.Content.ReadAsStringAsync();
        }

        public async Task unlinkCmdr(string buildId, string cmdr)
        {
            Game.log($"RCC.link: {cmdr} => {buildId}");

            var response = await RavenColonial.client.DeleteAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/link/{Uri.EscapeDataString(cmdr)}");
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            await response.Content.ReadAsStringAsync();
        }

        public async Task assign(string buildId, string cmdr, string commodity)
        {
            Game.log($"RCC.link: {cmdr} => {commodity}=> {buildId}");

            var response = await RavenColonial.client.PutAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/assign/{Uri.EscapeDataString(cmdr)}/{Uri.EscapeDataString(commodity)}", null);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var txt = await response.Content.ReadAsStringAsync();
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}: {txt}");
        }

        public async Task unAssign(string buildId, string cmdr, string commodity)
        {
            Game.log($"RCC.link: {cmdr} => {commodity}=> {buildId}");

            var response = await RavenColonial.client.DeleteAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}/assign/{Uri.EscapeDataString(cmdr)}/{Uri.EscapeDataString(commodity)}");
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var txt = await response.Content.ReadAsStringAsync();
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}: {txt}");
        }

        public async Task<Project?> load(long id64, long marketId)
        {
            Game.log($"RCC.load: {id64}/{marketId}");
            try
            {
                var url = $"{svcUri}/api/system/{Uri.EscapeDataString(id64.ToString())}/{Uri.EscapeDataString(marketId.ToString())}";
                Game.log($"RCC.load: {url}");

                var response = await RavenColonial.client.GetAsync(url);
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
            Game.log(diff.formatWithHeader($"RCC.contribute: {buildId}"));

            var json1 = JsonConvert.SerializeObject(diff);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var url = $"{svcUri}/api/project/{buildId}/contribute/{Uri.EscapeDataString(cmdr)}";
            var response = await RavenColonial.client.PostAsync(url, body);
            Game.log($"RCC.contribute: HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");
        }

        public async Task<Dictionary<string, int>> supplyFC(long marketId, string cargo, int delta)
        {
            return await supplyFC(marketId, new Dictionary<string, int> { { cargo, delta } });
        }

        public async Task<Dictionary<string, int>> supplyFC(long marketId, Dictionary<string, int> diff)
        {
            Game.log(diff.formatWithHeader($"RCC.supplyFC: {marketId}"));

            var json1 = JsonConvert.SerializeObject(diff);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            body.Headers.addIf("rcc-key", Game.activeGame?.cmdr?.rccApiKey);
            var url = $"{svcUri}/api/fc/{Uri.EscapeDataString(marketId.ToString())}/cargo";
            var response = await RavenColonial.client.PatchAsync(url, body);
            Game.log($"RCC.supplyFC: HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<Dictionary<string, int>>(json2)!;
            return obj;
        }

        public async Task<Dictionary<string, int>?> updateCargoFC(long marketId, Dictionary<string, int> cargo)
        {
            Game.log(cargo.formatWithHeader($"RCC.updateCargoFC: {marketId}"));

            var json1 = JsonConvert.SerializeObject(cargo);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            body.Headers.addIf("rcc-key", Game.activeGame?.cmdr?.rccApiKey);
            var url = $"{svcUri}/api/fc/{Uri.EscapeDataString(marketId.ToString())}/cargo";
            var response = await RavenColonial.client.PostAsync(url, body);
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Game.log($"RCC.updateCargoFC: '{marketId}' failed: HTTP:{(int)response.StatusCode}({response.StatusCode}): {json2}");
                return null;
            }
            var obj = JsonConvert.DeserializeObject<Dictionary<string, int>>(json2)!;
            return obj;
        }

        public async Task<FleetCarrier?> getFC(long marketId)
        {
            Game.log($"RCC.getFC: {marketId}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/fc/{Uri.EscapeDataString(marketId.ToString())}");
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Game.log($"RCC.getFC: HTTP:{(int)response.StatusCode}({response.StatusCode}): failed: {json}");
                return null;
            }

            var obj = JsonConvert.DeserializeObject<FleetCarrier>(json)!;
            return obj;
        }

        /// <summary>
        /// Replace server FC data with this. (Cargo untouched if it is null)
        /// </summary>
        public async Task<FleetCarrier?> publishFC(FleetCarrier fc)
        {
            Game.log($"RCC.updateFleetCarrier: {fc}");

            var json1 = JsonConvert.SerializeObject(fc);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            body.Headers.addIf("rcc-cmdr", Game.activeGame?.Commander);
            body.Headers.addIf("rcc-key", Game.activeGame?.cmdr?.rccApiKey);

            var response = await RavenColonial.client.PutAsync($"{svcUri}/api/fc/{fc.marketId}", body);
            var json2 = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Game.log($"RCC.updateFleetCarrier: HTTP:{(int)response.StatusCode}({response.StatusCode}): failed: {json2}");
                return null;
            }
            var obj = JsonConvert.DeserializeObject<FleetCarrier>(json2)!;
            return obj;
        }

        public async Task<FleetCarrier[]> getAllCmdrFCs(string cmdr)
        {
            Game.log($"RCC.getCmdrFCAll: {cmdr}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/fc/all");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<FleetCarrier[]>(json)!;
            return obj;
        }

        public async Task<Project?> getProject(string buildId)
        {
            Game.log($"RCC.getProject: {buildId}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/project/{Uri.EscapeDataString(buildId)}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Project>(json)!;
            else
            {
                Game.log($"RCC.getProject: failed:\n\t{json}");
                return null;
            }
        }

        public async Task<Project?> getProject(long id64, long marketId)
        {
            Game.log($"RCC.getProject: {id64}/{marketId}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/system/{Uri.EscapeDataString(id64.ToString())}/{Uri.EscapeDataString(marketId.ToString())}");
            Game.log($"HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<Project>(json)!;
            else
            {
                Game.log($"RCC.getProject: failed:\n\t{json}");
                return null;
            }
        }

        public async Task<List<Project>> getCmdrProjects(string cmdr)
        {
            Game.log($"RCC.getCmdrProjects: {cmdr}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<Project>>(json)!;
            return obj;
        }

        public async Task<CmdrSummary> getCmdrSummary(string cmdr)
        {
            Game.log($"RCC.getCmdrProjects: {cmdr}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/summary");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<CmdrSummary>(json)!;
            return obj;
        }

        public async Task<List<Project>> getCmdrActive(string cmdr)
        {
            Game.log($"RCC.getCmdrActive: {cmdr}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/active");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<Project>>(json)!;
            return obj;
        }

        public async Task<string> getPrimary(string cmdr)
        {
            Game.log($"RCC.getPrimary: {cmdr}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/primary");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<string>(json)!;

            return obj;
        }

        public async Task setPrimary(string cmdr, string? buildId)
        {
            Game.log($"RCC.setPrimary: {cmdr} => {buildId}");

            var response = buildId == null
                ? await RavenColonial.client.DeleteAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/primary/")
                : await RavenColonial.client.PutAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/primary/{Uri.EscapeDataString(buildId ?? "")}", null);
            Game.log($"RCC.setPrimary: HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");
        }

        public async Task<List<string>> getHiddenIDs(string cmdr)
        {
            Game.log($"RCC.getHiddenIDs: {cmdr}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/hiddenIDs");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<string>>(json)!;

            return obj;
        }

        public async Task<List<string>> setHiddenIDs(string cmdr, IEnumerable<string> buildIDs)
        {
            Game.log($"RCC.setHiddenIDs: {cmdr} => {string.Join(",", buildIDs)}");

            var json1 = JsonConvert.SerializeObject(buildIDs);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            var response = await RavenColonial.client.PostAsync($"{svcUri}/api/cmdr/{Uri.EscapeDataString(cmdr)}/hiddenIDs", body);
            Game.log($"RCC.setHiddenIDs: HTTP:{(int)response.StatusCode}({response.StatusCode}): {response.ReasonPhrase}");

            var json2 = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<string>>(json2)!;

            return obj;
        }

        public async Task<List<SystemSite>> getSystemSites(string nameOrNum)
        {
            Game.log($"RCC.getSystemSites: {nameOrNum}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/v2/system/{Uri.EscapeDataString(nameOrNum)}/sites");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<SystemSite>>(json)!;
            return obj;
        }

        public async Task<DataRCC?> updateSystem(string nameOrNum, SitesPut data)
        {
            Game.log($"RCC.updateSystem: {nameOrNum}");
            if (Game.activeGame?.cmdr?.rccApiKey == null)
            {
                // TODO: add some UI somewhere making this obvious
                Game.log($"Missing ApiKey! Cannot update system: {nameOrNum}");
                return null;
            }

            var json1 = JsonConvert.SerializeObject(data);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            body.Headers.addIf("rcc-key", Game.activeGame?.cmdr?.rccApiKey);

            var response = await RavenColonial.client.PutAsync($"{svcUri}/api/v2/system/{Uri.EscapeDataString(nameOrNum)}/sites", body);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Game.log($"updateSystem '{nameOrNum}' failed: HTTP:{(int)response.StatusCode}({response.StatusCode}): {json}");
                return null;
            }
            var obj = JsonConvert.DeserializeObject<DataRCC>(json);
            return obj;
        }

        public async Task<DataRCC> getSystem(string nameOrNum)
        {
            Game.log($"RCC.getSystemBodies: {nameOrNum}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/v2/system/{Uri.EscapeDataString(nameOrNum)}");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<DataRCC>(json)!;
            return obj;
        }

        public async Task<DataRCC> importSystemBodies(string nameOrNum)
        {
            Game.log($"RCC.importSystemBodies: {nameOrNum}");

            var response = await RavenColonial.client.PostAsync($"{svcUri}/api/v2/system/{Uri.EscapeDataString(nameOrNum)}/import/bodies", null);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<DataRCC>(json)!;
            return obj;
        }

        public async Task<string> getSystemArchitect(string nameOrNum)
        {
            Game.log($"RCC.getSystemArchitect: {nameOrNum}");

            var response = await RavenColonial.client.GetAsync($"{svcUri}/api/v2/system/{Uri.EscapeDataString(nameOrNum)}/architect");
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<string>(json)!;
            return obj;
        }

        /// <summary>
        /// Replace server body data with what we have
        /// </summary>
        public async Task<List<Bod>?> updateSysBodies(long address, List<Bod> bods)
        {
            Game.log($"RCC.updateSysBodies: {address} - bodies: {bods.Count}");

            var json1 = JsonConvert.SerializeObject(bods);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            body.Headers.addIf("rcc-key", Game.activeGame?.cmdr?.rccApiKey);
            var response = await RavenColonial.client.PutAsync($"{svcUri}/api/v2/system/{address}/bodies", body);
            var json2 = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Game.log($"updateSysBodies '{address}' failed: HTTP:{(int)response.StatusCode}({response.StatusCode}): {json2}");
                return null;
            }

            var newBods = JsonConvert.DeserializeObject<List<Bod>>(json2)!;
            return newBods;
        }

        public async Task<string?> getCmdrByApiKey(string apiKey)
        {
            Game.log($"RCC.getCmdrByApiKey");

            var req = new HttpRequestMessage(HttpMethod.Get, $"{svcUri}/api/cmdr/");
            req.Headers.addIf("rcc-key", apiKey);

            var response = await RavenColonial.client.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Game.log($"RCC.getCmdrByApiKey: failed:\n\t{json}");
                return null;
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
            return data.GetValueOrDefault("displayName");
        }

        public async Task publishCurrentShip(CmdrCurrentShip ship)
        {
            Game.log($"RCC.publishCurrentShip: {ship.name} ({ship.type})");

            var json1 = JsonConvert.SerializeObject(ship);
            var body = new StringContent(json1, Encoding.Default, "application/json");
            body.Headers.addIf("rcc-key", Game.activeGame?.cmdr?.rccApiKey);

            var response = await RavenColonial.client.PostAsync($"{svcUri}/api/cmdr/currentShip", body);
            var json2 = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                Game.log($"RCC.publishCurrentShip: response json:\r\n\t{json2}\r\n");
            else
                Game.log($"RCC.publishCurrentShip: HTTP:{(int)response.StatusCode}({response.StatusCode}): failed: {json2}");
        }

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
        public string? discordLink;
        //public DateTimeOffset? timeDue;
        public bool isPrimaryPort;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, HashSet<string>> commanders = new();

        // Schema.ProjectNotes
        public string? notes;
    }

    public class ProjectCreate : ProjectCore
    {
        /// <summary>
        /// The ID of the site from /api/v2/system that this project is replacing
        /// </summary>
        public string? systemSiteId;

        // Schema.ProjectCommodity
        public Dictionary<string, int> commodities;
        public ColonisationConstructionDepot? colonisationConstructionDepot;
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
        public ColonisationConstructionDepot? colonisationConstructionDepot;
    }

    public class FleetCarrier
    {
        public long marketId;
        public string name;
        public string displayName;
        public Dictionary<string, int> cargo;

        public override string ToString()
        {
            return $"{displayName} - {name} ({marketId})";
        }
    }

    public class DataRCC
    {
        public int v;
        public long id64;
        public string name;
        public string architect;
        public bool open;
        public int rev;
        public string reserveLevel;
        public List<SystemSite> sites;
        public List<Bod> bodies;
    }

    public class SystemSite
    {
        public string id;
        public string name;
        public int bodyNum;
        public string? buildType;
        public string? buildId;
        public long? marketId;
        public Status status;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Status
        {
            plan,
            build,
            complete
        }

        public override string ToString()
        {
            return $"{name} ({buildType}, {id}, {bodyNum})";
        }
    }

    /// <summary>Represents a body in a system</summary>
    public class Bod
    {
        public required string name;
        public required int num;
        public required double distLS;
        public required List<int> parents;
        public required BodyType type;
        /// <summary>Need to match Spansh</summary>
        public string? subType;
        public required HashSet<BodyFeature> features;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum BodyType
        {
            /// <summary>unknown</summary>
            un,
            /// <summary>Black Hole</summary>
            bh,
            /// <summary>Neutron Star</summary>
            ns,
            /// <summary>White Dwarf</summary>
            wd,
            /// <summary>some kind of star</summary>
            st,
            /// <summary>Ammonia World</summary>
            aw,
            /// <summary>Earth Like Body</summary>
            elw,
            /// <summary>Gas Giant</summary>
            gg,
            /// <summary>High Metal Content Body</summary>
            hmc,
            /// <summary>Icy Body</summary>
            ib,
            /// <summary>Metal Rich Body</summary>
            mrb,
            /// <summary>Rock Body</summary>
            rb,
            /// <summary>Rocky Ice Body</summary>
            ri,
            /// <summary>Water Giant</summary>
            wg,
            /// <summary>Water World</summary>
            ww,
            /// <summary>Asteroid cluster</summary>
            ac,
            /// <summary>barycenter</summary>
            bc,
        }

        public static Dictionary<string, BodyType> mapBodyTypeFromPlanetClass = new()
        {
            { "Ammonia world", BodyType.aw },
            { "Earthlike body", BodyType.elw },
            // Gas Giants - BodyType.gg - handled in code
            { "High metal content body", BodyType.hmc },
            { "Icy body", BodyType.ib },
            { "Metal rich body", BodyType.mrb },
            { "Rocky body", BodyType.rb },
            { "Rocky ice body", BodyType.ri },
            { "Water giant", BodyType.wg },
            { "Water world", BodyType.ww },
        };
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BodyFeature
    {
        bio,
        geo,
        rings,
        volcanism,
        terraformable,
        tidal,
        landable,
        atmosphere,
    }

    public class SitesPut
    {
        public List<SystemSite> update = new();
        public List<string> delete = new();

        // Intentionally missing: orderIDs
        public string? architect;
        public bool? open;
        public ReserveLevel? reserveLevel;
        // Intentionally missing: snapshot;
        // Intentionally missing: slots;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReserveLevel
    {
        depleted,
        low,
        common,
        major,
        pristine,
    }

    public class CmdrCurrentShip
    {
        public required string cmdr;
        public required string name; // or ident
        public required string type;
        public required int maxCargo;
        public required Dictionary<string, int> cargo;
    }
}
