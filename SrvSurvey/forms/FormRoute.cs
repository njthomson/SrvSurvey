using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Data;
using System.Text.RegularExpressions;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormRoute : SizableForm
    {
        private Route route = new();
        private CommanderSettings cmdr;

        public FormRoute()
        {
            InitializeComponent();
            this.cmdr = CommanderSettings.LoadCurrentOrLast();

            // clone the route
            this.route = JObject.FromObject(cmdr.route1 ?? new()).ToObject<Route>()!;
            checkActive.Checked = this.route.active;

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


        private async Task doImportNames(string[] names)
        {
            var count = 0;
            try
            {
                route.hops ??= new();
                foreach (var name in names)
                {
                    var response = await Game.spansh.getSystemRef(name);
                    if (response == null)
                    {
                        Game.log($"Unknown star system: {name}");
                        continue;
                    }

                    route.hops.Add(response);
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

        private Regex regTouristUrl = new Regex(@"\/(.*?)\?", RegexOptions.Singleline | RegexOptions.Compiled);

        private void menuSpanshTourist_Click(object sender, EventArgs e)
        {
            var rslt = MessageBox.Show(this, "To proceed: copy the Spansh url of the tourist route to the clipboard", "SrvSurvey", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
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
            //var rr = new Regex(@"/(.+?)\?.+$", RegexOptions.Singleline | RegexOptions.Compiled);
            //var match = rr.Match(text);           //regTouristUrl
            //if (!Guid.TryParse(match.Groups[1].Value, out var routeId)) return;

            var msg = $"Importing route: '{routeId}' ...";
            Game.log(msg);
            lblStatus.Text = msg;
            this.setChildrenEnabled(false);
            Task.Run(() => doImportTouristRoute(routeId));
        }

        private async Task doImportTouristRoute(Guid routeId)
        {
            var status = "";
            var count = 0;
            try
            {
                var response = await Game.spansh.getTouristRoute(routeId);
                if (response?.result?.system_jumps == null)
                {
                    status = $"Route not found: {routeId}";
                    return;
                }

                this.route.clear();
                foreach (var jump in response.result.system_jumps)
                {
                    route.hops.Add(jump.toStarRef());
                    count++;
                    this.Invoke(() => prepList());
                }
                status = $"Successfully imported {count} hops";
            }
            catch (Exception ex)
            {
                Game.log(ex);
            }
            finally
            {
                Program.defer(() =>
                {
                    Game.log($"doImportNames: imported {count} hops");
                    lblStatus.Text = status;
                    this.setChildrenEnabled(true);
                });
            }

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

        #endregion

        private void prepList()
        {
            list.Items.Clear();
            if (this.route.hops == null || this.route.hops.Count == 0) return;

            StarRef lastStar = cmdr.getCurrentStarRef();
            foreach (var entry in this.route.hops)
            {
                var item = list.Items.Add(entry.name);
                item.Tag = entry;

                var dist = entry.getDistanceFrom(lastStar);
                var subDist = item.SubItems.Add($"{dist:N1} ly");
                subDist.Tag = dist;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // replace and save
            this.route.active = checkActive.Checked;
            cmdr.route1 = this.route;
            if (cmdr.route1.hops.Count == 0)
                cmdr.route1 = null;

            if (route.lastHop == null)
                route.nextHop = route.hops.FirstOrDefault();

            cmdr.Save();
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            route.clear();
            prepList();
        }


    }
}
