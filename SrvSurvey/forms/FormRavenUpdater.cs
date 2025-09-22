using Newtonsoft.Json.Linq;
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

        static FormRavenUpdater()
        {
            // prep lookups and dictionaries
            allCosts = Game.colony.loadDefaultCosts()
                .OrderBy(c => c.tier)
                .ToList();

            buildTypesInstallation.Add("installation?");

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
        private SystemSite? pending;
        private Filter filter;

        public FormRavenUpdater()
        {
            InitializeComponent();

            this.id64 = game.systemData!.address;

            // prep combo items
            var sourceCosts = allCosts.ToDictionary(c => c.buildType, c => $"Tier {c.tier}: {c.displayName}");
            comboBuildType.DataSource = new BindingSource(sourceCosts, null);

            comboStatus.Items.AddRange(Enum.GetNames<SystemSite.Status>());
            game.status.StatusChanged += Status_StatusChanged;

            this.loadSites().justDoIt();
        }

        #region raven data loading and merging 

        private async Task loadSites()
        {
            this.sysData = await Game.colony.getSystem(id64.ToString());
            if (sysData.architect != game.cmdr.commander && !sysData.open)
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
            foreach (var body in sysData.bodies)
            {
                if (body.name != sysData.name)
                    body.name = body.name.Replace(sysData.name + " ", "");

                mapBodies.Add(body.num, body.name);
            }
            comboBody.DataSource = new BindingSource(mapBodies, null);
            comboBody.SelectedIndex = 0;
            comboBody.Enabled = true;

            // clone sites so as not to pollute the originals
            this.sites = sysData.sites.Select(s => JObject.FromObject(s).ToObject<SystemSite>()).ToList()!;
            this.setFilter(Filter.none);
        }

        private void setFilter(Filter newFilter)
        {
            filter = newFilter;

            list.Items.Clear();
            lastChecked.Clear();

            var filteredSites = sites.Where(s =>
            {
                if (filter == Filter.noBodyInstallation) return s.bodyNum < 0 && buildTypesInstallation.Contains(s.buildType);
                if (filter == Filter.noBodyOrbitalPorts) return s.bodyNum < 0 && buildTypesOrbitalPorts.Contains(s.buildType);

                return true;
            });

            foreach (var site in filteredSites)
            {
                var item = list.Items.Add(site.name, site.id ?? site.name, site);
                var body = sysData.bodies.Find(b => b.num == site.bodyNum);
                var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType!.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
                var category = match == null ? null : $"Tier {match.tier}: {match.displayName}";

                item.SubItems.Add(body?.name ?? "?");
                item.SubItems.Add(site.buildType ?? "?");
                item.SubItems.Add(category ?? "?");
                item.SubItems.Add(site.status.ToString());
            }

            list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            // move to next phase if empty
            if (sites.Count > 0 && list.Items.Count == 0 && filter != Filter.none)
                nextPhase();
        }

        private (SystemSite? site, ListViewItem? item) getListItem()
        {
            if (pending == null) return (null, null);

            var item = list.Items.Find(pending.id ?? pending.name, false).FirstOrDefault();
            if (item == null) Debugger.Break();
            return (pending, item);
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
            if (filter != Filter.none) setFilter(filter);
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

            pending = site;

            var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType.StartsWith(l, StringComparison.OrdinalIgnoreCase)));

            // set controls
            txtName.Text = site.name;
            comboBody.SelectedValue = site.bodyNum;
            comboStatus.SelectedItem = site.status.ToString();
            if (match != null) comboBuildType.SelectedValue = match.buildType;
            comboBuildSubType.SelectedValue = site.buildType;

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
            pending = new SystemSite()
            {
                id = a.id,
                name = a.name,
                bodyNum = a.bodyNum,
                buildType = a.buildType,
                buildId = a.buildId,
                status = a.status,
            };

            if (string.IsNullOrWhiteSpace(pending.id)) { pending.id = b.id; pending.status = b.status; }
            if (string.IsNullOrWhiteSpace(pending.name)) pending.name = b.name;
            if (string.IsNullOrWhiteSpace(pending.buildType)) pending.buildType = b.buildType;
            if (pending.bodyNum < 0 && b.bodyNum >= 0) pending.bodyNum = b.bodyNum;
            //var category = getCategory(buildType);

            showSite(pending);
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            // ...
        }

        #endregion

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (filter != Filter.none)
                setFilter(Filter.none);
            else
                nextPhase();
        }

        #region journal processing

        /// <summary> Decides what instruction to show next </summary>
        private void nextPhase()
        {
            var game = Game.activeGame;
            if (game?.journals == null) return;

            // Collect relevant journal items
            var (lastHonk, bodyScans, fssSignals, approaches, docks) = this.collectJournalItems();

            // Are we good?

            // system honked?
            if (lastHonk == null)
            {
                lblTask.Text = "Discovery Scan needed ...";
                return;
            }
            // FSS not complete?
            if (lastHonk?.Progress < 1)
            {
                lblTask.Text = "Complete system FSS ...";
                return;
            }
            // FSS complete but we don't have all the bodies
            if (bodyScans.Length < lastHonk!.BodyCount)
            {
                lblTask.Text = "Scan the Nav Beacon ...";
                return;
            }

            // ... we are good!

            // create from FSS signals as needed

            this.createUnknownSignals(fssSignals);

            // does everything have a buildId
            var missingBody = sites.Where(s => s.bodyNum == -1).ToList();
            if (missingBody.Any())
            {
                // scan installations?
                var installations = missingBody.Where(s => s.buildType == "installation?" || buildTypesInstallation.Contains(s.buildType)).ToList();
                if (installations.Count > 0)
                {
                    this.setFilter(Filter.noBodyInstallation);
                    lblTask.Text = $"Select installations around unknown bodies";
                    return;
                }

                // scan orbital ports
                var orbitalPorts = missingBody.Where(s => buildTypesOrbitalPorts.Contains(s.buildType)).ToList();
                if (orbitalPorts.Count > 0)
                {
                    this.setFilter(Filter.noBodyInstallation);
                    lblTask.Text = $"Select orbital ports around unknown bodies";
                    return;
                }
            }

            // otherwise we are all done
            lblTask.Text = $"All good?";
            if (filter != Filter.none)
                setFilter(Filter.none);
        }

        private (FSSDiscoveryScan? lastHonk, Scan[] bodyScans, FSSSignalDiscovered[] fssSignals, ApproachSettlement[] approaches, Docked[] docks) collectJournalItems()
        {
            // Collect relevant journal items
            FSSDiscoveryScan? lastHonk = null;
            var bodyScans = new Dictionary<int, Scan>();
            var fssSignals = new Dictionary<string, FSSSignalDiscovered>();
            var approaches = new Dictionary<long, ApproachSettlement>();
            var docks = new Dictionary<long, Docked>();

            var jumpCount = 0;
            Game.activeGame!.journals!.walkDeep(true, entry =>
            {
                if (entry is FSSDiscoveryScan && lastHonk == null)
                    lastHonk = (FSSDiscoveryScan)entry;

                // collect the most recent of various journal entries
                var scan = entry as Scan;
                if (scan != null)
                {

                    if (scan.SystemAddress == sysData.id64 && !bodyScans.ContainsKey(scan.BodyID)) bodyScans[scan.BodyID] = scan;
                }

                var fssSignal = entry as FSSSignalDiscovered;
                if (fssSignal != null && fssSignal.SystemAddress == sysData.id64 && !fssSignals.ContainsKey(fssSignal.SignalName)) fssSignals[fssSignal.SignalName] = fssSignal;

                var approach = entry as ApproachSettlement;
                if (approach != null && approach.SystemAddress == sysData.id64 && !approaches.ContainsKey(approach.MarketID)) approaches[approach.MarketID] = approach;

                var docked = entry as Docked;
                if (docked != null && docked.SystemAddress == sysData.id64 && !docks.ContainsKey(docked.MarketID)) docks[docked.MarketID] = docked;

                // search for the last 10 FSD jumps only
                if (entry is FSDJump) jumpCount++;

                return jumpCount > 10 || bodyScans.Count == lastHonk?.BodyCount;
            }, null);

            return (
                lastHonk,
                bodyScans.Values.ToArray(),
                fssSignals.Values.ToArray(),
                approaches.Values.ToArray(),
                docks.Values.ToArray()
                );
        }

        private void createUnknownSignals(FSSSignalDiscovered[] fssSignals)
        {
            // TODO: Make statics...
            var stationSignalTypes = new List<string>()
            {
                "StationCoriolis", "Outpost"
            };
            var mapSignalTypeBuildType = new Dictionary<string, string>()
            {
                { "Installation", "installation?" },
                { "StationCoriolis", "no_truss?" },
                { "Outpost", "b" },
                { "a", "b" },
            };

            foreach (var signal in fssSignals)
            {
                // skip FCs, RES sites, CZs, etc
                if (signal.SignalName_Localised != null || signal.SignalType == "FleetCarrier" || signal.SignalName.Contains("Construction Site")) continue;

                // skip if already exists
                var match = sites.Find(s => s.name.Equals(signal.SignalName, StringComparison.OrdinalIgnoreCase));
                if (match != null) continue;

                // what should this be?
                var buildType = mapSignalTypeBuildType.GetValueOrDefault(signal.SignalType, "");

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
            if (game?.status?.Destination?.System != sysData.id64) return;

            var match = sites.Find(s => s.name == game.status.Destination.Name);
            if (match?.bodyNum == -1)
            {
                match.bodyNum = game.status.Destination.Body;
                setFilter(filter);
            }
        }

        // { "timestamp":"2025-09-17T05:34:35Z", "event":"FSSDiscoveryScan", "Progress":0.265935, "BodyCount":17, "NonBodyCount":7, "SystemName":"Synuefe KJ-X b47-1", "SystemAddress":2871318881689 }
        // { "timestamp":"2025-09-17T05:47:39Z", "event":"FSSDiscoveryScan", "Progress":1.000000, "BodyCount":17, "NonBodyCount":7, "SystemName":"Synuefe KJ-X b47-1", "SystemAddress":2871318881689 }
        // { "timestamp":"2025-09-17T05:47:39Z", "event":"FSSSignalDiscovered", "SystemAddress":2871318881689, "SignalName":"$MULTIPLAYER_SCENARIO42_TITLE;", "SignalName_Localised":"Nav Beacon", "SignalType":"NavBeacon" }
        // { "timestamp":"2025-09-17T05:47:39Z", "event":"FSSSignalDiscovered", "SystemAddress":2871318881689, "SignalName":"Wellesley Vision", "SignalType":"StationONeilCylinder", "IsStation":true }

        #endregion


        private enum Filter
        {
            none,
            noBodyInstallation,
            noBodyOrbitalPorts,
        }
    }
}
