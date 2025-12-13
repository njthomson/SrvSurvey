using Newtonsoft.Json;
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
            "StationCoriolis", "Outpost", "StationBernalSphere", "StationONeilOrbis", "StationAsteroid", "StationDodec",
        };

        static Dictionary<string, string> mapSignalTypeBuildType = new()
        {
            { "Installation", "installation?" },
            { "Outpost", "outpost?" },
            { "StationCoriolis", "no_truss?" },
            { "StationBernalSphere", "ocellus" },
            { "StationONeilOrbis", "orbis?" },
            { "StationAsteroid", "asteroid" },
            { "StationDodec", "dodec?" },
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
            buildTypesOrbitalPorts.Add("orbis");
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
        private SystemSite? editSite;
        private Phase phase;
        private SystemSite? orbitalPendingBody;
        private FSSSignalDiscovered[]? lastSignalsDiscovered;
        private string? lastDestinationName;

        public FormRavenUpdater()
        {
            InitializeComponent();

            this.id64 = game.systemData!.address;
            panel1.Enabled = true;

            // prep combo items
            var sourceCosts = allCosts.ToDictionary(c => c.buildType, c => $"Tier {c.tier}: {c.displayName}");
            sourceCosts.Add("", "?");
            comboBuildType.DataSource = new BindingSource(sourceCosts, null);

            comboStatus.Items.AddRange(Enum.GetNames<SystemSite.Status>());
            game.status.StatusChanged += Status_StatusChanged;
            if (game.journals != null) game.journals.onJournalEntry += Journals_onJournalEntry;

            setPhase(Phase.preamble, lblTask.Text);

            this.loadSites().justDoIt();
        }

        #region raven data loading and merging 

        private async Task loadSites()
        {
            this.btnNext.Enabled = false;
            this.toolLinkRC.Text = id64.ToString();

            this.sysData = await Game.rcc.getSystem(id64.ToString());
            this.toolLinkRC.Text = sysData.name;

            // is a body import needed?
            if (this.sysData.bodies == null)
                this.sysData = await Game.rcc.importSystemBodies(id64.ToString());

            if (!string.IsNullOrEmpty(sysData.architect) && !sysData.architect.like(game.cmdr.commander) && !sysData.open)
            {
                Program.defer(() =>
                {
                    this.setChildrenEnabled(false);
                    this.btnRemove.Enabled = false;
                    MessageBox.Show("You are not the architect of this system or it has been secured. You are not allowed to make any changes.", "SrvSurvey");
                    this.Close();
                });
            }

            // reduce to short names + prep body combo
            setKnownBodies();

            // clone sites so as not to pollute the originals
            if (sysData.sites?.Count > 0)
                this.sites = sysData.sites.Select(s => JObject.FromObject(s).ToObject<SystemSite>()).ToList()!;

            if (phase != Phase.preamble && phase != Phase.allDone)
            {
                this.setFilter(Phase.none);
                nextPhase();
            }
            this.btnNext.Enabled = true;
        }

        private void setKnownBodies()
        {
            mapBodies.Clear();
            mapBodies.Add(-1, "?");
            if (sysData.bodies != null)
            {
                foreach (var body in sysData.bodies)
                {
                    if (body.name != sysData.name)
                        body.name = body.name.Replace(sysData.name + " ", "");

                    mapBodies[body.num] = body.name;
                }
                comboBody.DataSource = new BindingSource(mapBodies, null);
                comboBody.SelectedIndex = 0;
                comboBody.Enabled = true;
            }
        }

        private async Task saveSites()
        {
            Game.log($"Updating RavenColonial System '{sysData.name}' ({id64}) ...");

            // prep data to save
            var data = new SitesPut();

            var latestData = await Game.rcc.getSystem(id64.ToString());

            // take any sites where something changed
            foreach (var site in this.sites)
            {
                var match = latestData.sites.Find(s2 => site.id == s2.id || site.name.like(s2.name));
                if (match == null)
                {
                    data.update.Add(site);
                }
                else
                {
                    var json1 = JsonConvert.SerializeObject(match);
                    var json2 = JsonConvert.SerializeObject(site);
                    if (json1 != json2)
                        data.update.Add(site);
                }
            }

            // delete anything not previously known (from when we loaded before)
            foreach (var site in sysData.sites)
            {
                if (string.IsNullOrEmpty(site.id)) continue;

                var match = this.sites.Find(s2 => site.id == s2.id || site.name.like(s2.name));
                if (match == null)
                    data.delete.Add(site.id);
            }

            if (data.update.Any() || data.delete.Any())
            {
                Game.log($"saveSites: '{sysData.name}' ({id64}) update: {data.update.Count}, delete: {data.delete.Count}");
                // update, then reload everything
                var rslt = await Game.rcc.updateSystem(id64.ToString(), data);
                if (rslt == null)
                {
                    MessageBox.Show(this, $"Failed to save updates. Check logs for more details.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                await this.loadSites();
            }
        }

        private void setFilter(Phase newFilter)
        {
            phase = newFilter;

            switch (phase)
            {
                case Phase.none:
                case Phase.preamble:
                    lblPhase.Text = "Getting started...";
                    break;

                case Phase.scanning:
                    lblPhase.Text = "Scanning the system...";
                    break;

                case Phase.noBodyInstallation:
                    lblPhase.Text = "Identifying orbital installations...";
                    break;

                case Phase.noBodyOrbitalPorts:
                    lblPhase.Text = "Identifying orbital ports...";
                    break;

                case Phase.allSurfaceSites:
                    lblPhase.Text = "Identifying surface facilities...";
                    break;

                case Phase.allDone:
                    lblPhase.Text = "Process complete, ready to save. Dock at settlements and ports as needed";
                    break;

                default:
                    Debugger.Break();
                    break;
            }

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
            }).OrderBy(s => s.name);

            foreach (var site in filteredSites)
            {
                var item = list.Items.Add(site.name, site.id ?? site.name, site);
                var body = sysData.bodies?.Find(b => b.num == site.bodyNum);
                var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType?.StartsWith(l, StringComparison.OrdinalIgnoreCase) == true));
                var category = match == null ? null : $"Tier {match.tier}: {match.displayName}";

                item.SubItems.Add(body?.name ?? "?");
                item.SubItems.Add(site.buildType ?? "?");
                item.SubItems.Add(category ?? "?");
                item.SubItems.Add(site.status.ToString());

                if (list.Items.Count == 1)
                    item.Selected = true;
            }

            if (list.Items.Count > 0)
            {
                list.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.ColumnContent);
                list.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
                showSite(null);

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
            //if (item == null) Debugger.Break();
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
            if (site == null || item == null || comboBody.SelectedValue == null)
            {
                setFilter(phase);
                return;
            }

            var bodyNum = (int)comboBody.SelectedValue;
            var body = sysData.bodies.Find(b => b.num == bodyNum);
            if (body == null)
            {
                site.bodyNum = -1;
                item.SubItems[1].Text = mapBodies[-1];
            }
            else
            {
                site.bodyNum = body.num;
                item.SubItems[1].Text = mapBodies[body.num];
            }

            if (phase != Phase.allDone)
                toolFiller.Text = $"Remaining: {list.Items.Count}";

            // refresh view if it is filtered
            if (phase != Phase.none)
                setFilter(phase);
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
            item.SubItems[2].Text = buildType;

            var match = allCosts.FirstOrDefault(c => c.layouts.Any(l => site.buildType!.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
            item.SubItems[3].Text = match == null ? null : $"Tier {match.tier}: {match.displayName}";
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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (editSite == null) return;

            sites.Remove(editSite);

            if (list.Items.ContainsKey(editSite.id))
                list.Items.Remove(list.Items[editSite.id]!);
        }

        private void showSite(SystemSite? site)
        {
            panel1.setChildrenEnabled(false);
            btnRemove.Enabled = false;
            if (site == null)
            {
                txtName.Text = "";
                panel1.Enabled = false;
                return;
            }

            editSite = site;
            panel1.Enabled = true;

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
                comboBuildSubType.SelectedItem = site.buildType?.Trim('?');
            }
            else
            {
                comboBuildType.SelectedValue = "";
            }

            panel1.setChildrenEnabled(true);
            btnRemove.Enabled = true;
        }

        /* Not needed?

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
            panel1.setChildrenEnabled(ready);
            btnRemove.Enabled = ready;

            if (!ready)
            {
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
        */

        #endregion

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            this.saveSites().justDoIt();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            btnReload.Enabled = false;
            btnSubmit.Enabled = false;
            this.sites = new();
            this.phase = Phase.preamble;

            this.loadSites().continueOnMain(this, () => this.nextPhase());
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
            if (game?.journals == null || sysData == null) return;

            // Collect relevant journal items
            var (bodyCount, lastHonk, lastAllBodies, bodyScans, fssSignals, approaches, docks) = this.collectJournalItems();
            this.lastSignalsDiscovered = fssSignals;

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

            if (sysData.bodies.Count < totalBodyCount && game.systemData != null)
            {
                Task.Run(() => ColonyData.updateSysBodies(game.systemData).continueOnMain(this, bods =>
                {
                    if (bods == null)
                    {
                        MessageBox.Show(this, $"Updating system bodies was not successful. Check logs for more details.", "SrvSurvey", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        this.sysData.bodies = bods;
                        setKnownBodies();
                        setFilter(Phase.none);
                        nextPhase();
                    }
                }));
                return;
            }

            // ... we are good for journal events
            btnReload.Enabled = true;
            btnSubmit.Enabled = true;

            // create from FSS signals as needed
            this.createUnknownSignals(fssSignals);

            var pendingInstallations = sites.Where(s => s.bodyNum == -1 && buildTypesInstallation.Contains(s.buildType?.TrimEnd('?') ?? "")).ToList();
            if (pendingInstallations.Any() && phase < Phase.noBodyInstallation)
            {
                setPhase(Phase.noBodyInstallation, "On the left/external panel, set the filter to 'Points of interest' or use the system map.\r\nSelect the following installations:");
                return;
            }
            toolFiller.Text = "";

            // apply any Docked entries?
            foreach(var entry in docks)
            {
                var matchSite = sites.Find(s => s.name == entry.StationName);
                if (matchSite == null) continue;

                if (matchSite.marketId == null || matchSite.marketId == 0)
                    matchSite.marketId = entry.MarketID;

                if (entry.StationType == StationType.OnFootSettlement)
                    this.matchKnownSettlement(matchSite);
                else if (entry.StationType == StationType.Outpost)
                    identifyOutposts(entry, matchSite);
            }

            var pendingOrbitals = sites.Where(s => s.bodyNum == -1 && buildTypesOrbitalPorts.Contains(s.buildType?.TrimEnd('?') ?? "")).ToList();
            if (pendingOrbitals.Any())
            {
                setPhase(Phase.noBodyOrbitalPorts, "Open the system map in the game. In the list below: select each orbital port then set their parent body.");
                return;
            }

            // otherwise we are all done
            if (phase < Phase.allSurfaceSites)
            {
                lblTask.Text = $"In the left/external panel, set the filter to show settlements only, then select the first settlement.";
                setFilter(Phase.allSurfaceSites);
                return;
            }

            if (phase != Phase.none)
            {
                btnNext.Enabled = false;

                lblTask.Text = $"You may hit 'Submit' to update Raven Colonial or 'Reload' to start over.\r\nDock at Odyssey settlements to identify their types.\r\nSee the wiki for guidance of identifying other facility types.";
                panel1.setChildrenEnabled(true);
                setFilter(Phase.allDone);
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
                var fssDiscoveryScan = entry as FSSDiscoveryScan;
                if (lastHonk == null && fssDiscoveryScan != null && fssDiscoveryScan.SystemAddress == id64)
                    lastHonk = fssDiscoveryScan;

                var fssAllBodiesFound = entry as FSSAllBodiesFound;
                if (lastAllBodies == null && fssAllBodiesFound != null && fssAllBodiesFound.SystemAddress == id64)
                    lastAllBodies = fssAllBodiesFound;

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
                if (signal.SignalName.StartsWith("$") || signal.SignalName_Localised != null || signal.SignalType == "FleetCarrier" || signal.SignalType == "SquadronCarrier" || signal.SignalName.Contains("Construction Site")) continue;

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
                    id = $"y{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
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
            }

            lastDestinationName = name;
        }

        private void statusChanged_NoBodyInstallation(int bodyNum, string name)
        {
            // set bodyNum value
            var matchSite = sites.Find(s => s.name == name);
            if (matchSite?.bodyNum == -1)
            {
                var matchItem = list.Items.ToList().Find(i => i.Text == matchSite.name);
                if (matchItem == null) return;

                var body = sysData.bodies.Find(b => b.num == bodyNum);
                if (body == null)
                {
                    Game.log($"No matching body found for '{name}', bodyNum: {bodyNum} ?");
                    toolFiller.Text = $"'{name}' has an invalid body ID. Please set manually";

                    if (lastDestinationName != name)
                        selectOnlyItem(matchItem);
                }
                else
                {
                    // update item and list
                    matchSite.bodyNum = bodyNum;
                    matchItem.SubItems[1]!.Text = mapBodies[body.num];
                    toolFiller.Text = $"Found '{name}', remaining: {list.Items.Count}";

                    setFilter(phase);
                }
            }
            else if (matchSite != null)
            {
                toolFiller.Text = $"'{name}' already known, remaining: {list.Items.Count}";
            }

            if (list.Items.Count == 0)
                nextPhase();
        }

        private void selectOnlyItem(ListViewItem targetItem)
        {
            var selectedItems = list.SelectedItems.Cast<ListViewItem>();
            foreach (var item in selectedItems)
                if (item.Name != targetItem.Name)
                    item.Selected = false;

            if (!targetItem.Selected)
            {
                targetItem.EnsureVisible();
                targetItem.Selected = true;
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
                    // make this the only thing selected
                    var selectedItems = list.SelectedItems.Cast<ListViewItem>();
                    foreach (var item in selectedItems)
                        if (item.Name != matchItem.Name)
                            item.Selected = false;

                    if (!matchItem.Selected)
                    {
                        matchItem.EnsureVisible();
                        matchItem.Selected = true;
                    }
                    toolFiller.Text = $"Remaining: {list.Items.Count}";
                    lblTask.Text = $"Select parent body for: {orbitalPendingBody!.name}";
                }
            }
            else
            {
                toolFiller.Text = $"'{name}' already known, remaining: {list.Items.Count}";
                lblTask.Text = "Open the system map in the game. In the list below: select each orbital port then set their parent body.\r\nOptionally, if known, you may also set their build type.";
                orbitalPendingBody = null;
            }
        }

        private void statusChanged_AllSurfaceSites(int bodyNum, string name)
        {
            if (name.StartsWith(sysData.name) || name.Contains("Construction site", StringComparison.OrdinalIgnoreCase)) return;
            // skip anything known to be an orbital signal
            if (lastSignalsDiscovered?.Any(s => s.SignalName.like(name)) == true)
                return;

            var matchSite = sites.Find(s => s.name == name);
            if (matchSite == null)
            {
                // we may not know (yet) what this is but we know which body it is on
                matchSite = new SystemSite()
                {
                    bodyNum = bodyNum,
                    name = name,
                    buildType = "settlement?",
                    id = $"y{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    status = SystemSite.Status.complete,
                };

                toolFiller.Text = $"Matched '{name}'";
                Game.log($"Creating unknown site for: {name} ({matchSite.buildType}) on body #{bodyNum} ({mapBodies[bodyNum]})");
                sites.Insert(0, matchSite);
                setFilter(phase);
            }
            else if (matchSite.bodyNum == -1)
            {
                matchSite.bodyNum = bodyNum;
                matchSite.status = SystemSite.Status.complete;
                toolFiller.Text = $"Matched: '{name}'";
                Game.log($"Setting bodyNum: {bodyNum} for {matchSite.name} ({matchSite.id})");
            }
            else
            {
                matchSite.status = SystemSite.Status.complete;
                toolFiller.Text = $"'{name}' already known";
            }

            matchKnownSettlement(matchSite);

            lblTask.Text = $"Selected: {name}\r\nKeep selecting each and every settlement in the left/external panel.\r\nClick Next when all are in the list below.";
        }

        private void matchKnownSettlement(SystemSite matchSite)
        {
            // matches a known settlement?
            var matchSettlement = game.systemData?.stations?.Find(s => s.name == matchSite.name);
            if (matchSettlement != null)
            {
                matchSite.marketId = matchSettlement.marketId;

                if (matchSite.id == null)
                    matchSite.id = $"&{matchSettlement.marketId}";

                // always set it (or only if not known?)
                if (matchSettlement.subType > 0) // string.IsNullOrWhiteSpace(matchSite.buildType) || matchSite.buildType == "settlement?")
                {
                    var key = $"{matchSettlement.stationEconomy}{matchSettlement.subType}";
                    if (CanonnStation.mapSettlementTypes.TryGetValue(key, out var buildType))
                        matchSite.buildType = buildType;
                }
            }

            // update list item to match
            var matchItem = list.Items.ToList().Find(i => i.Text == matchSite.name!);
            if (matchItem != null)
            {
                matchItem.SubItems[1].Text = mapBodies[matchSite.bodyNum];
                matchItem.SubItems[2].Text = matchSite.buildType;

                var matchCost = allCosts.FirstOrDefault(c => c.layouts.Any(l => matchSite.buildType!.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
                matchItem.SubItems[3].Text = matchCost == null ? "?" : $"Tier {matchCost.tier}: {matchCost.displayName}";
                list.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.ColumnContent);

                if (lastDestinationName != matchSite.name)
                    selectOnlyItem(matchItem);
            }
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

        private void onJournalEntry(Docked entry)
        {
            var matchSite = sites.Find(s => s.name == entry.StationName);
            if (matchSite == null)
            {
                Game.log($"No match for site named: '{entry.StationName}' ({entry.MarketID})");
                return;
            }

            if (matchSite.marketId == null || matchSite.marketId == 0)
                matchSite.marketId = entry.MarketID;

            if (entry.StationType == StationType.OnFootSettlement)
            {
                // auto-detect a settlement?
                Util.deferAfter(250, () => matchKnownSettlement(matchSite));
            }
            else if (entry.StationType == StationType.CraterPort)
            {
                // assume this is a Large Planetary Port?
                if (string.IsNullOrWhiteSpace(matchSite.buildType))
                    matchSite.buildType = "aphrodite?";
            }
            else if (entry.StationType == StationType.Outpost)
            {
                identifyOutposts(entry, matchSite);
            }
            else if (entry.StationType == StationType.CraterOutpost)
            {
                identifySurfaceOutposts(entry, matchSite);
            }

            // update list item to match
            var matchItem = list.Items.ToList().Find(i => i.Text == matchSite.name!);
            if (matchItem != null)
            {
                matchItem.SubItems[2].Text = matchSite.buildType;

                var matchCost = allCosts.FirstOrDefault(c => c.layouts.Any(l => matchSite.buildType!.StartsWith(l, StringComparison.OrdinalIgnoreCase)));
                matchItem.SubItems[3].Text = matchCost == null ? "?" : $"Tier {matchCost.tier}: {matchCost.displayName}";
                list.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.ColumnContent);

                if (lastDestinationName != matchSite.name)
                    selectOnlyItem(matchItem);
            }
        }

        private void identifyOutposts(Docked entry, SystemSite site)
        {
            // Plutus sites have 3x landing pads but Vesta has 4
            if (entry.LandingPads.Small == 3 && entry.LandingPads.Medium == 1)
                site.buildType = "plutus";
            else if (entry.LandingPads.Small == 4 && entry.LandingPads.Medium == 1)
                site.buildType = "vesta";
            else
            {
                // If we have only one significant economy - we can use that to know the type
                var matches = entry.StationEconomies?.Where(x => x.Proportion >= 1).OrderBy(x => x.Proportion).ToList();
                if (matches?.Count == 1)
                {
                    switch (entry.StationEconomy)
                    {
                        case "$economy_HighTech;": site.buildType = "prometheus"; break;
                        case "$economy_Industrial;": site.buildType = "vulcan"; break;
                        case "$economy_Military;": site.buildType = "nemesis"; break;
                        case "$economy_Service;": site.buildType = "dysnomia"; break;
                    }
                }
            }
        }

        private void identifySurfaceOutposts(Docked entry, SystemSite site)
        {
            // If we have only one significant economy - we can use that to know the type
            var matches = entry.StationEconomies?.Where(x => x.Proportion >= 1).OrderBy(x => x.Proportion).ToList();
            if (matches?.Count == 1)
            {
                switch (entry.StationEconomy)
                {
                    // TODO: ...
                    //case "$economy_HighTech;": site.buildType = "prometheus"; break;
                    //case "$economy_Industrial;": site.buildType = "vulcan"; break;
                    //case "$economy_Military;": site.buildType = "nemesis"; break;
                    //case "$economy_Service;": site.buildType = "dysnomia"; break;
                }
            }
        }

        #endregion

        private void toolLinkRC_Click(object sender, EventArgs e)
        {
            Util.openLink($"https://ravencolonial.com/#sys={this.id64}");
        }

        private void toolLinkWiki_Click(object sender, EventArgs e)
        {
            Util.openLink($"https://github.com/njthomson/SrvSurvey/wiki/Colonisation-System-Update-Tool");
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            Util.openLink($"https://ravencolonial.com/vis");
        }

        private enum Phase
        {
            none,
            preamble,
            scanning,
            noBodyInstallation,
            noBodyOrbitalPorts,
            allSurfaceSites,
            allDone,
        }
    }
}
