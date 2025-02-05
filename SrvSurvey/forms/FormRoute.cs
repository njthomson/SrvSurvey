using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.units;
using System.Data;
using static SrvSurvey.net.Spansh;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormRoute : SizableForm
    {
        private CommanderSettings cmdr;
        private List<FollowRoute.Hop> hops;
        private int lastIdx;
        private bool updatingListChecks;

        public FormRoute()
        {
            InitializeComponent();
            this.cmdr = CommanderSettings.LoadCurrentOrLast();
            updatingListChecks = true;

            // update UX from route
            var route = cmdr.route;
            this.hops = route.hops.ToList(); // make a copy of the List
            this.lastIdx = route.last;
            this.checkActive.Checked = route.active;
            this.checkAutoCopy.Checked = route.autoCopy;

            prepList();
        }

        #region import data

        private void menuImportNames_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, "To proceed: copy a number of system names to the clipboard, one system per line.", "SrvSurvey", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (rslt != DialogResult.OK || !Clipboard.ContainsText(TextDataFormat.Text)) return;

            var text = Clipboard.GetText();
            var names = text.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var msg = $"Importing ~{names.Length} names...";
            Game.log(msg);
            lblStatus.Text = msg;
            this.setChildrenEnabled(false);
            Task.Run(() => doImportNames(names));
        }

        private void menuSystemNamesFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Choose import file",
                DefaultExt = "txt",
                Filter = "Text files|*.txt",
                Multiselect = false,
            };

            var rslt = dialog.ShowDialog(this);
            if (rslt != DialogResult.OK) return;
            if (string.IsNullOrEmpty(dialog.FileName) || !File.Exists(dialog.FileName)) return;

            var lines = File.ReadAllLines(dialog.FileName);

            var msg = $"Importing ~{lines.Length} names...";
            Game.log(msg);
            lblStatus.Text = msg;
            this.setChildrenEnabled(false);

            Task.Run(() => doImportNames(lines));
        }

        private void menuSpanshTouristFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Choose import file",
                DefaultExt = "csv",
                Filter = "CSV files|*.csv",
                Multiselect = false,
            };

            var rslt = dialog.ShowDialog(this);
            if (rslt != DialogResult.OK) return;
            if (string.IsNullOrEmpty(dialog.FileName) || !File.Exists(dialog.FileName)) return;

            var lines = File.ReadAllLines(dialog.FileName)
                .Skip(1)
                .Select(l => l.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).First().Trim('"'))
                .ToArray();

            var msg = $"Importing ~{lines.Length} rows...";
            Game.log(msg);
            lblStatus.Text = msg;
            this.setChildrenEnabled(false);

            Task.Run(() => doImportNames(lines));
        }

        private async Task doImportNames(string[] names)
        {
            var count = 0;
            try
            {
                this.lastIdx = -1;
                this.hops.Clear();
                foreach (var name in names)
                {
                    var star = await Game.spansh.getSystemRef(name);
                    if (star == null)
                    {
                        Game.log($"Unknown star system: {name}");
                        continue;
                    }

                    hops.Add(new FollowRoute.Hop(star.name, star.id64, star.x, star.y, star.z));
                    count++;
                    this.Invoke(() => prepList());
                }
            }
            catch (Exception ex)
            {
                Game.log(ex);
            }
            finally
            {
                Program.defer(() =>
                {
                    Game.log($"doImportNames: imported {count} systems");
                    lblStatus.Text = $"Imported {count} systems";
                    this.setChildrenEnabled(true);
                });
            }

        }

        private void menuSpanshTourist_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, "To proceed: copy the URL of any Spansh route to the clipboard", "SrvSurvey", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (rslt != DialogResult.OK || !Clipboard.ContainsText(TextDataFormat.Text)) return;

            var text = Clipboard.GetText();
            var parts = text.Split(new char[] { '/', '?' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            Guid routeId = Guid.Empty;
            parts.FirstOrDefault(p => Guid.TryParse(p, out routeId));

            if (routeId == Guid.Empty && false)
            {
                lblStatus.Text = "Failed to find a tourist route ID";
                return;
            }

            var routeType = "route";
            if (parts.Contains("tourist"))
                routeType = "tourist";
            else if (parts.Contains("plotter"))
                routeType = "neutron";
            else if (parts.Contains("exact-plotter"))
                routeType = "galaxy";

            var msg = $"Importing {routeType} route: '{routeId}' ...";
            Game.log(msg);
            lblStatus.Text = msg;
            this.setChildrenEnabled(false);
            Task.Run(() => doImportSpanshRoute(routeId, routeType));
        }

        private async Task doImportSpanshRoute(Guid routeId, string routeType)
        {
            var status = "";
            try
            {
                List<FollowRoute.Hop>? parsedHops = null;
                if (routeType == "tourist")
                {
                    var response = await Game.spansh.getRoute<TouristRoute>(routeId);
                    parsedHops = response?.result?.system_jumps.Select(j => FollowRoute.Hop.from(j)).ToList()!;
                }
                else if (routeType == "galaxy")
                {
                    var response = await Game.spansh.getRoute<GalaxyRoute>(routeId);
                    parsedHops = response?.result?.jumps.Select(j => FollowRoute.Hop.from(j)).ToList()!;
                }
                else if (routeType == "neutron")
                {
                    var response = await Game.spansh.getRoute<NeutronRoute>(routeId);
                    parsedHops = response?.result?.system_jumps.Select(j => FollowRoute.Hop.from(j)).ToList()!;
                }
                else
                {
                    var response = await Game.spansh.getRoute<Route>(routeId);
                    parsedHops = response?.result?.Select(j => FollowRoute.Hop.from(j)).ToList()!;
                }

                if (parsedHops == null)
                {
                    status = $"Route not found: {routeId}";
                    return;
                }

                this.lastIdx = -1;
                this.hops.Clear();
                this.hops.AddRange(parsedHops);
                status = $"Successfully imported {this.hops.Count} hops";
            }
            catch (Exception ex)
            {
                Game.log(ex);
            }
            finally
            {
                Program.defer(() =>
                {
                    Game.log($"doImportNames: imported {this.hops.Count} hops");
                    prepList();
                    lblStatus.Text = status;
                    this.setChildrenEnabled(true);
                });
            }
        }

        #endregion

        private void prepList()
        {
            list.Items.Clear();
            if (this.hops == null || this.hops.Count == 0) return;

            try
            {
                updatingListChecks = true;

                // hide the 3rd column at first
                list.Columns[2].Width = 0;
                var hasNotes = false;
                var lastStar = lastIdx == -1 ? cmdr.getCurrentStarRef() : hops[0];
                for (int n = 0; n < hops.Count; n++)
                {
                    var star = hops[n];
                    var item = list.Items.Add(star.name);
                    item.Name = star.name;
                    item.Tag = star;
                    item.Checked = n <= lastIdx;

                    var dist = star.getDistanceFrom(lastStar);
                    var subDist = item.SubItems.Add($"{dist:N2} ly");
                    subDist.Tag = dist;

                    var subNotes = item.SubItems.Add("");
                    if (star.refuel) subNotes.Text += "⛽ Refuel ";
                    if (star.neutron) subNotes.Text += "⚠️ Neutron ";
                    if (star.notes != null) subNotes.Text += star.notes;
                    if (!string.IsNullOrEmpty(subNotes.Text)) hasNotes = true;

                    lastStar = star;
                }
                if (hasNotes)
                    list.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);

                setStatusLabel();
            }
            finally
            {
                Program.defer(() => updatingListChecks = false);
            }
        }

        private void setStatusLabel()
        {
            if (lastIdx == -1)
                this.lblStatus.Text = $"Route not started";
            else if (lastIdx + 1 == hops.Count)
                this.lblStatus.Text = $"Route complete";
            else
                this.lblStatus.Text = $"Route progress: {lastIdx + 1} of {hops.Count}";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // replace and save
            var route = cmdr.route;
            route.autoCopy = checkAutoCopy.Checked;
            route.last = lastIdx;
            route.hops = this.hops;
            if (checkActive.Enabled && checkActive.Checked)
                route.activate();
            else
                route.disable();

            Game.log($"Saving route: {route.filepath}");
            route.Save();

            if (route.active && PlotSphericalSearch.allowPlotter)
                Program.showPlotter<PlotSphericalSearch>(); // needed? .Invalidate();
            else
                Program.invalidate<PlotSphericalSearch>();

            this.Close();
        }

        private void list_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (updatingListChecks) return;
            try
            {
                updatingListChecks = true;

                lastIdx = list.Items.IndexOf(e.Item);
                if (!e.Item.Checked) lastIdx--;

                // make everything prior the click item be checked
                for (int n = 0; n < list.Items.Count; n++)
                    list.Items[n].Checked = n <= lastIdx;

                // if the final item is checked, disable the activate checkbox
                checkActive.Enabled = !list.Items[^1].Checked;

                setStatusLabel();
            }
            finally
            {
                Program.defer(() => updatingListChecks = false);
            }
        }

        /// <summary> Called from external code to update which systems are checked, if this window is open and we are FSD jumping between systems </summary>
        public void updateChecks(StarRef star)
        {
            var idx = hops.FindIndex(h => h.id64 == star.id64);
            if (idx == -1) return;

            list.Items[idx].Checked = true;
        }
    }
}
