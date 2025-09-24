using Newtonsoft.Json.Linq;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using System.Data;
using System.Diagnostics;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormRavenUpdater : SizableForm
    {
        #region statics

        static List<ColonyCost2> allCosts;
        static List<string> buildTypesInstallation = new();
        static List<string> buildTypesOrbitalPorts = new();
        static List<string> buildTypesSurfacePorts = new();
        static List<string> buildTypesSurfaceFacilities = new();

        static List<string> stationSignalTypes = new()
        {
            "StationCoriolis", "Outpost", "StationBernalSphere", "StationONeilOrbis","StationAsteroid"
        };

        static Dictionary<string, string> mapSignalTypeBuildType = new()
        {
            { "Installation", "installation?" },
            { "Outpost", "outpost?" },
            { "StationCoriolis", "no_truss?" },
            { "StationBernalSphere", "ocellus" },
            { "StationONeilOrbis", "orbis?" },
            { "StationAsteroid", "asteroid" },
        };

        static FormRavenUpdater()
        {
            // prep lookups and dictionaries
            allCosts = Game.rcc.loadDefaultCosts()
                .OrderBy(c => c.tier)
                .ToList();

            buildTypesInstallation.Add("installation");
            buildTypesOrbitalPorts.Add("outpost");
            buildTypesSurfacePorts.Add("settlement");
            buildTypesSurfaceFacilities.Add("facility");

            foreach (var c in allCosts)
            {
                var buildTypes = c.layouts.Select(t => t.ToLowerInvariant());
                if (c.location == "orbital")
                {
                    if (c.displayName.Contains("Installation") || c.category.Contains("Tourist") || c.category.Contains("Bar"))
                        buildTypesInstallation.AddRange(buildTypes);
                    else
                        buildTypesOrbitalPorts.AddRange(buildTypes);
                }
                else
                {
                    if (c.category == "Port" || c.displayName.Contains("Settlement"))
                        buildTypesSurfacePorts.AddRange(buildTypes);
                    else
                        buildTypesSurfaceFacilities.AddRange(buildTypes);
                }
            }
        }

        #endregion

        private Game game => Game.activeGame!;
        public long id64;
        private DataRCC sysData;
        private Dictionary<int, string> mapBodies = new();

        private List<SystemSite> sites = new();
        private List<ListViewItem> lastChecked = new();
        private SitesPut pending;
        private SystemSite? editSite;
        private Phase phase;
        private SystemSite? orbitalPendingBody;

        public FormRavenUpdater()
        {
            InitializeComponent();

            this.id64 = game.systemData!.address;
            this.pending = new();
            panel1.Enabled = true;

            // prep combo items
            var sourceCosts = allCosts.ToDictionary(c => c.buildType, c => $"Tier {c.tier}: {c.displayName}");
            comboBuildType.DataSource = new BindingSource(sourceCosts, null);

            comboStatus.Items.AddRange(Enum.GetNames<SystemSite.Status>());
            game.status.StatusChanged += Status_StatusChanged;
            if (game.journals != null) game.journals.onJournalEntry += Journals_onJournalEntry;

            this.loadSites().justDoIt();
        }

        #region raven data loading and merging 

        private async Task loadSites()
        {
            this.sysData = await Game.rcc.getSystem(id64.ToString());

            // TODO: trigger import if needed

            if (!string.IsNullOrEmpty(sysData.architect) && sysData.architect != game.cmdr.commander && !sysData.open)
            {
                Program.defer(() =>
                {
                    this.setChildrenEnabled(false);
                    MessageBox.Show("You are not the architect of this system or it has been secured. You are not allowed to make any changes.", "SrvSurvey");
                    this.Close();
                });
            }

            // reduce to short names + prep body combo
            mapBodies.Clear();
            if (sysData.bodies != null)
            {
                foreach (var body in sysData.bodies)
                {
                    if (body.name != sysData.name)
                        body.name = body.name.Replace(sysData.name + " ", "");

                    mapBodies.Add(body.num, body.name);
                }
                comboBody.DataSource = new BindingSource(mapBodies, null);
                comboBody.SelectedIndex = 0;
                comboBody.Enabled = true;
            }

            // clone sites so as not to pollute the originals
            if (sysData.sites?.Count > 0)
                this.sites = sysData.sites.Select(s => JObject.FromObject(s).ToObject<SystemSite>()).ToList()!;

            this.setFilter(Phase.none);
            nextPhase();
        }

        private void saveSites()
        {
            // prep data to save
            var newSites = sites.Where(s1 => !sysData.sites.Any(s2 => s1.id == s2.id || s1.name.Equals(s2.name, StringComparison.OrdinalIgnoreCase))).ToList();
            Game.log(newSites);
        }

        private void setFilter(Phase newFilter)
        {
            phase = newFilter;
            this.Text = $"Phase: {newFilter}";

            var priorCount = list.Items.Count;
            list.Items.Clear();
            lastChecked.Clear();
            orbitalPendingBody = null;

            var filteredSites = sites.Where(s =>
            {
                var buildType = s.buildType?.Trim('?') ?? ""; // s.buildType?.EndsWith("?") == true ? s.buildType.Substring(0, s.buildType.Length - 1) : s.buildType ?? "";

                if (phase == Phase.noBodyInstallation) return s.bodyNum < 0 && buildTypesInstallation.Contains(buildType);
                if (phase == Phase.noBodyOrbitalPorts) return s.bodyNum < 0 && buildTypesOrbitalPorts.Contains(buildType);
                if (phase == Phase.allSurfaceSites) return s.buildType == "" || (!buildTypesOrbitalPorts.Contains(buildType) && !buildTypesInstallation.Contains(buildType));

                return true;
            });

            foreach (var site in filteredSites)
            {
                var item = list.Items.Add(site.name, site.id ?? site.name, site);
                var body = sysData.bodies.Find(b => b.num == site.bodyNum);
                var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType?.StartsWith(l, StringComparison.OrdinalIgnoreCase) == true));
                var category = match == null ? null : $"Tier {match.tier}: {match.displayName}";

                item.SubItems.Add(body?.name ?? "?");
                item.SubItems.Add(site.buildType ?? "?");
                item.SubItems.Add(category ?? "?");
                item.SubItems.Add(site.status.ToString());
            }

            if (list.Items.Count > 0)
                list.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);

            // move to next phase if empty
            if (sites.Count > 0 && priorCount > 0 && list.Items.Count == 0 && phase != Phase.none)
            {
                nextPhase();
            }
            //else if (filter == Filter.noBodyOrbitalPorts && list.Items.Count > 0)
            //{
            //    orbitalPendingBody = (SystemSite)list.Items[0].Tag!;
            //}
        }

        #endregion

        #region editing existing and merging 

        private (SystemSite? site, ListViewItem? item) getListItem()
        {
            if (editSite == null) return (null, null);

            var item = list.Items.Find(editSite.id ?? editSite.name, false).FirstOrDefault();
            if (item == null) Debugger.Break();
            return (editSite, item);
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (!txtName.Enabled) return;

            var (site, item) = getListItem();
            if (site == null || item == null) return;

            site.name = txtName.Text;
            item.Text = site.name;
        }

        private void comboBody_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBody.Enabled) return;

            var (site, item) = getListItem();
            if (site == null || item == null || comboBody.SelectedValue == null) return;
            var bodyNum = (int)comboBody.SelectedValue;
            var body = sysData.bodies.Find(b => b.num == bodyNum);
            if (body == null) { Debugger.Break(); return; }

            site.bodyNum = body.num;
            item.SubItems[1].Text = mapBodies[body.num];

            // refresh view if it is filtered
            if (phase != Phase.none) setFilter(phase);
        }

        private void comboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboStatus.Enabled) return;

            var (site, item) = getListItem();
            if (site == null || item == null || comboStatus.SelectedItem == null) return;
            if (!Enum.TryParse<SystemSite.Status>(comboStatus.SelectedItem.ToString(), out var newStatus)) return;

            site.status = newStatus;
            item.SubItems[4].Text = newStatus.ToString();
        }

        private void comboBuildSubType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBuildSubType.Enabled) return;

            var (site, item) = getListItem();
            var buildType = comboBuildSubType.SelectedItem?.ToString()?.ToLowerInvariant();
            if (site == null || item == null || buildType == null) return;

            site.buildType = buildType;
            item.SubItems[3].Text = buildType;

            var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType!.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
            item.SubItems[2].Text = match == null ? null : $"Tier {match.tier}: {match.displayName}";
        }

        private void comboBuildType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBuildType.Enabled) return;

            comboBuildSubType.Items.Clear();

            var buildType = comboBuildType.SelectedValue?.ToString() ?? "";
            var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => buildType.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
            if (match != null)
            {
                comboBuildSubType.Items.AddRange(match.layouts.ToArray());
                comboBuildSubType.SelectedIndex = 0;
            }

            comboBuildSubType.Enabled = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list.SelectedItems.Count == 0)
                showSite(null);
            else if (list.SelectedItems.Count == 1)
                showSite(list.SelectedItems[0].Tag as SystemSite);
        }

        private void showSite(SystemSite? site)
        {
            panel1.setChildrenEnabled(false);
            if (site == null)
            {
                btnMerge.Enabled = false;
                txtName.Text = "";
                return;
            }

            editSite = site;

            var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType?.StartsWith(l, StringComparison.OrdinalIgnoreCase) == true));
            // set controls
            txtName.Text = site.name;
            comboBody.SelectedValue = site.bodyNum;
            comboStatus.SelectedItem = site.status.ToString();
            comboBuildSubType.Items.Clear();
            if (match != null)
            {
                comboBuildType.SelectedValue = match.buildType;
                comboBuildSubType.Items.AddRange(match.layouts.Select(l => l.ToLowerInvariant()).ToArray());
                comboBuildSubType.SelectedItem = site.buildType.Trim('?');
            }

            panel1.setChildrenEnabled(true);
        }

        private void list_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked)
                lastChecked.Insert(0, e.Item);
            else
                lastChecked.Remove(e.Item);

            // only allow 2 items to be checked
            if (list.CheckedItems.Count > 2 && lastChecked.Count > 0)
            {
                var oldest = lastChecked.Last();
                oldest.Checked = false;
                lastChecked.Remove(oldest);
            }

            prepMerge();
        }

        private void prepMerge()
        {
            var ready = lastChecked.Count == 2;
            btnMerge.Enabled = ready;
            panel1.setChildrenEnabled(ready);
            if (!ready)
            {
                btnMerge.Enabled = false;
                txtName.Text = "";
                return;
            }

            var a = (SystemSite)lastChecked[0].Tag!;
            var b = (SystemSite)lastChecked[1].Tag!;

            // TODO: start with A values, but replace with B if better
            editSite = new SystemSite()
            {
                id = a.id,
                name = a.name,
                bodyNum = a.bodyNum,
                buildType = a.buildType,
                buildId = a.buildId,
                status = a.status,
            };

            if (string.IsNullOrWhiteSpace(editSite.id)) { editSite.id = b.id; editSite.status = b.status; }
            if (string.IsNullOrWhiteSpace(editSite.name)) editSite.name = b.name;
            if (string.IsNullOrWhiteSpace(editSite.buildType)) editSite.buildType = b.buildType;
            if (editSite.bodyNum < 0 && b.bodyNum >= 0) editSite.bodyNum = b.bodyNum;
            //var category = getCategory(buildType);

            showSite(editSite);
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            // ...
        }

        #endregion

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            this.saveSites();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            nextPhase();
        }

        #region journal processing


        private void setPhase(Phase nextPhase, string msg)
        {
            this.phase = nextPhase;
            lblTask.Text = msg;

            if (nextPhase > Phase.scanning)
                setFilter(nextPhase);
        }

        /// <summary> Decides what instruction to show next </summary>
        private void nextPhase()
        {
            var game = Game.activeGame;
            if (game?.journals == null || sysData == null) return;

            // Collect relevant journal items
            var (bodyCount, lastHonk, lastAllBodies, bodyScans, fssSignals, approaches, docks) = this.collectJournalItems();

            // system scanned enough?
            if (lastAllBodies == null)
            {
                if (lastHonk == null)
                {
                    setPhase(Phase.scanning, "Discovery Scan needed ...");
                    return;
                }
                // FSS not complete?
                if (lastHonk?.Progress < 1)
                {
                    setPhase(Phase.scanning, "Complete system FSS ...");
                    return;
                }
            }

            var totalBodyCount = lastAllBodies?.Count ?? lastHonk?.BodyCount;

            // FSS complete but we don't have all the bodies
            if (bodyCount < totalBodyCount)
            {
                setPhase(Phase.scanning, "Scan the Nav Beacon ...");
                return;
            }

            // ... we are good!

            // create from FSS signals as needed

            this.createUnknownSignals(fssSignals);

            // does everything have a buildId
            var pendingInstallations = sites.Where(s => s.bodyNum == -1 && buildTypesInstallation.Contains(s.buildType.TrimEnd('?'))).ToList();
            if (pendingInstallations.Any())
            {
                setPhase(Phase.noBodyInstallation, "In the External panel, select the following installations:");
                return;
            }

            var pendingOrbitals = sites.Where(s => s.bodyNum == -1 && buildTypesOrbitalPorts.Contains(s.buildType.TrimEnd('?'))).ToList();
            if (pendingOrbitals.Any())
            {
                setPhase(Phase.noBodyOrbitalPorts, "Select orbital ports, then select their parent ...");
                return;
            }

            // otherwise we are all done
            if (phase < Phase.allSurfaceSites)
            {
                lblTask.Text = $"Set External panel filter to Settlements. Select all surface sites. Click Next when done ...";
                setFilter(Phase.allSurfaceSites);
                return;
            }

            if (phase != Phase.none)
            {
                lblTask.Text = $"All done?";
                panel1.setChildrenEnabled(true);
                setFilter(Phase.none);
            }
        }

        private (int bodyCount, FSSDiscoveryScan? lastHonk, FSSAllBodiesFound? lastAllBodies, Scan[] bodyScans, FSSSignalDiscovered[] fssSignals, ApproachSettlement[] approaches, Docked[] docks) collectJournalItems()
        {
            // Collect relevant journal items
            FSSDiscoveryScan? lastHonk = null;
            FSSAllBodiesFound? lastAllBodies = null;
            var bodyScans = new Dictionary<int, Scan>();
            var barycentreScans = new Dictionary<int, ScanBaryCentre>();
            var fssSignals = new Dictionary<string, FSSSignalDiscovered>();
            var approaches = new Dictionary<long, ApproachSettlement>();
            var docks = new Dictionary<long, Docked>();

            var jumpCount = 0;
            Game.activeGame!.journals!.walkDeep(true, entry =>
            {
                if (entry is FSSDiscoveryScan && lastHonk == null)
                    lastHonk = (FSSDiscoveryScan)entry;
                if (entry is FSSAllBodiesFound && lastAllBodies == null)
                    lastAllBodies = (FSSAllBodiesFound)entry;

                // collect the most recent of various journal entries
                var scan = entry as Scan;
                if (scan != null)
                    if (scan.SystemAddress == id64 && !bodyScans.ContainsKey(scan.BodyID))
                        bodyScans[scan.BodyID] = scan;

                var barycenterScan = entry as ScanBaryCentre;
                if (barycenterScan != null && barycenterScan.SystemAddress == id64 && !barycentreScans.ContainsKey(barycenterScan.BodyID))
                    barycentreScans[barycenterScan.BodyID] = barycenterScan;

                var fssSignal = entry as FSSSignalDiscovered;
                if (fssSignal != null && fssSignal.SystemAddress == id64 && !fssSignals.ContainsKey(fssSignal.SignalName)) fssSignals[fssSignal.SignalName] = fssSignal;

                var approach = entry as ApproachSettlement;
                if (approach != null && approach.SystemAddress == id64 && !approaches.ContainsKey(approach.MarketID)) approaches[approach.MarketID] = approach;

                var docked = entry as Docked;
                if (docked != null && docked.SystemAddress == id64 && !docks.ContainsKey(docked.MarketID)) docks[docked.MarketID] = docked;

                // search for the last 10 FSD jumps only
                if (entry is FSDJump) jumpCount++;

                return jumpCount > 10 || (jumpCount > 0 && bodyScans.Count == lastHonk?.BodyCount);
            }, null);

            var allBodyNums = new HashSet<int>(bodyScans.Keys.Concat(barycentreScans.Keys));
            var bodyCount = allBodyNums.Count;

            return (
                bodyCount,
                lastHonk,
                lastAllBodies,
                bodyScans.Values.ToArray(),
                fssSignals.Values.ToArray(),
                approaches.Values.ToArray(),
                docks.Values.ToArray()
                );
        }

        private void createUnknownSignals(FSSSignalDiscovered[] fssSignals)
        {
            foreach (var signal in fssSignals)
            {
                // skip FCs, RES sites, CZs, etc
                if (signal.SignalName.StartsWith("$") || signal.SignalName_Localised != null || signal.SignalType == "FleetCarrier" || signal.SignalName.Contains("Construction Site")) continue;

                // skip if already exists
                var match = sites.Find(s => s.name.Equals(signal.SignalName, StringComparison.OrdinalIgnoreCase));
                if (match != null) continue;

                // what should this be?
                var buildType = mapSignalTypeBuildType.GetValueOrDefault(signal.SignalType, "");

                Game.log($"Create unknown site from signal: {signal.SignalName} ({signal.SignalType} => {buildType})");

                sites.Insert(0, new SystemSite()
                {
                    bodyNum = -1,
                    name = signal.SignalName,
                    buildType = buildType,
                    id = null!, //$"y{DateTime.Now.Ticks}",
                    status = SystemSite.Status.complete,
                });
            }
        }

        private void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || sysData == null || game?.status?.Destination == null || game?.status?.Destination?.System != id64) return;
            var name = game.status.Destination.Name;
            var bodyNum = game.status.Destination.Body;
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith("$")) return;

            if (phase == Phase.noBodyInstallation && !name.StartsWith(sysData.name))
            {
                statusChanged_NoBodyInstallation(bodyNum, name);
            }
            else if (phase == Phase.noBodyOrbitalPorts)
            {
                statusChanged_NoBodyOrbital(bodyNum, name);
            }
            else if (phase == Phase.allSurfaceSites && !name.StartsWith(sysData.name))
            {
                statusChanged_AllSurfaceSites(bodyNum, name);
                return;
            }
        }

        private void statusChanged_NoBodyInstallation(int bodyNum, string name)
        {
            // set bodyNum value
            var matchSite = sites.Find(s => s.name == name);
            if (matchSite?.bodyNum == -1)
            {
                var body = sysData.bodies.Find(b => b.num == bodyNum);
                if (body == null) { Debugger.Break(); return; }

                var matchItem = list.Items.ToList().Find(i => i.Text == matchSite.name);
                if (matchItem == null) { Debugger.Break(); return; }

                // update item and list
                matchSite.bodyNum = bodyNum;
                matchItem.SubItems[1]!.Text = mapBodies[body.num];
                setFilter(phase);
            }
        }

        private void statusChanged_NoBodyOrbital(int bodyNum, string name)
        {
            if (orbitalPendingBody != null && name.StartsWith(sysData.name))
            {
                orbitalPendingBody.bodyNum = bodyNum;
                setFilter(phase);
                return;
            }

            // has a suitable item been selected?
            var matchItem = list.Items.ToList().Find(i => i.Text == name!);
            if (matchItem != null)
            {
                orbitalPendingBody = sites.Find(s => s.name == name);
                if (orbitalPendingBody != null)
                {
                    list.SelectedItems.Clear();
                    matchItem.EnsureVisible();
                    matchItem.Selected = true;
                    lblTask.Text = $"Select parent body for: {orbitalPendingBody!.name}";
                }
            }
            else
            {
                lblTask.Text = "Select orbital ports, then select their parent ...";
                orbitalPendingBody = null;
            }
        }

        private void statusChanged_AllSurfaceSites(int bodyNum, string name)
        {
            if (name.StartsWith(sysData.name) || name.Contains("Construction site", StringComparison.OrdinalIgnoreCase)) return;

            var matchSite = sites.Find(s => s.name == name);
            if (matchSite == null)
            {
                // we may not know what this is but we know which body it is on
                var newSite = new SystemSite()
                {
                    bodyNum = bodyNum,
                    name = name,
                    buildType = null!,
                    id = null!, //$"y{DateTime.Now.Ticks}",
                    status = SystemSite.Status.complete,
                };

                // matches a known settlement?
                var matchSettlement = game.systemData?.stations?.Find(s => s.name == name);
                if (matchSettlement != null)
                {
                    newSite.id = $"&{matchSettlement.marketId}";

                    if (sites.Any(s => s.id == newSite.id))
                    {
                        Debugger.Break();
                        Game.log($"Why the duplicate ID: {newSite.id}");
                        return;
                    }

                    var key = $"{matchSettlement.stationEconomy}{matchSettlement.subType}";

                    if (CanonnStation.mapSettlementTypes.TryGetValue(key, out var buildType))
                        newSite.buildType = buildType;
                }

                Game.log($"Creating unknown site for: {name} ({newSite.buildType}) on body #{bodyNum} ({mapBodies[bodyNum]})");
                sites.Insert(0, newSite);
                setFilter(phase);
            }
            else if (matchSite.bodyNum == -1)
            {
                Game.log($"Setting bodyNum: {bodyNum} for {matchSite.name} ({matchSite.id})");
                var matchItem = list.Items.ToList().Find(i => i.Text == name!);
                if (matchItem != null)
                {
                    matchSite.bodyNum = bodyNum;
                    matchSite.status = SystemSite.Status.complete;
                    matchItem.SubItems[1].Text = mapBodies[bodyNum];
                }
            }

            lblTask.Text = $"Selected: {name}\r\nKeep selecting each and every settlement ...";
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            if (this.IsDisposed) return;

            this.onJournalEntry((dynamic)entry);
        }

        private void onJournalEntry(JournalEntry entry) { /* ignore */ }

        private void onJournalEntry(FSSDiscoveryScan entry)
        {
            this.nextPhase();
        }

        private void onJournalEntry(FSSAllBodiesFound entry)
        {
            this.nextPhase();
        }
        private void onJournalEntry(NavBeaconScan entry)
        {
            this.nextPhase();
        }

        #endregion


        private enum Phase
        {
            none,
            scanning,
            noBodyInstallation,
            noBodyOrbitalPorts,
            //unknownOrbitalPorts,
            allSurfaceSites,
            allDone,
        }
    }
}
