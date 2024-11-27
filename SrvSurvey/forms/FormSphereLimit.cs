using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.net.EDSM;
using SrvSurvey.Properties;
using SrvSurvey.units;
using System.Data;

namespace SrvSurvey
{
    [Draggable]
    internal partial class FormSphereLimit : FixedForm
    {
        public static bool allow { get => Game.activeGame?.cmdr != null; }

        protected Game game = Game.activeGame!;

        private Dictionary<string, StarSystem> matchedSystems = new Dictionary<string, StarSystem>();
        private StarPos? targetStarPos;
        private string lastSearch = "";
        private int interruptCount;

        public FormSphereLimit()
        {
            InitializeComponent();
            this.Icon = Icons.sphere;

            txtCurrentSystem.Text = game.cmdr.currentSystem;

            // populate controls from settings
            numRadius.Value = (decimal)game.cmdr.sphereLimit.radius;
            comboSystemName.Text = game.cmdr.sphereLimit.centerSystemName;
            targetStarPos = game.cmdr.sphereLimit.centerStarPos ?? new StarPos();
            if (targetStarPos != null)
            {
                txtStarPos.Text = targetStarPos.ToString();
                var dist = Util.getSystemDistance(game.cmdr.starPos, targetStarPos).ToString("N2");
                txtCurrentDistance.Text = $"{dist}ly";
            }
            else
            {
                txtCurrentDistance.Text = $"-";
            }

            Util.applyTheme(this);
        }

        private async Task lookupSystem(string searchName)
        {
            try
            {
                Game.log($"lookupSystem begin: {this.interruptCount}");

                if (string.IsNullOrEmpty(searchName)) return;

                // avoid a networi=k request if we already have some hits in matched systems
                var hits = this.matchedSystems.Values.Where(_ => _.name.StartsWith(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (hits.Count > 0 && searchName.Length >= lastSearch.Length)
                {
                    Game.log($"use existing for '{searchName}'! {this.interruptCount}");

                    processResults(searchName, hits);
                    return;
                }

                // no hits - make a network request
                btnDisable.Enabled = btnAccept.Enabled = txtStarPos.Enabled = false;

                comboSystemName.Items.Clear();
                comboSystemName.Items.Add("Searching ...");
                comboSystemName.DroppedDown = true;
                comboSystemName.SelectionStart = searchName.Length;
                comboSystemName.SelectionLength = comboSystemName.Text.Length;

                var results = await Game.edsm.getSystems(searchName);
                Game.log($"Found {results.Length} systems from: {searchName}");

                // stop if we need to go round again
                if (this.interruptCount > 1) return;

                if (results == null || results.Length == 0)
                {
                    // no matches found
                    this.matchedSystems.Clear();
                    comboSystemName.Items.Clear();
                    comboSystemName.Items.Add("No match found");
                    comboSystemName.Text = searchName;
                    comboSystemName.SelectionStart = searchName.Length;
                    comboSystemName.SelectionLength = 0;
                }
                else
                {
                    // some systems found
                    processResults(searchName, results);
                }

                Program.control!.Invoke((MethodInvoker)delegate
                {
                    btnDisable.Enabled = btnAccept.Enabled = txtStarPos.Enabled = true;
                });
            }
            finally
            {
                var runAgain = this.interruptCount > 1;
                Game.log($"run again? {runAgain}");
                this.interruptCount = 0;
                if (runAgain)
                {
                    // lookpu again if text has changed?
                    this.interruptCount++;
                    _ = this.lookupSystem(comboSystemName.Text);
                }
            }
        }

        private void processResults(string searchName, IEnumerable<StarSystem> matches)
        {
            Game.log($"processResults for '{searchName}'! {this.interruptCount}");

            // ignore older results
            if (this.interruptCount == 0) return;

            this.matchedSystems.Clear();
            comboSystemName.Items.Clear();
            foreach (var system in matches)
            {
                // allow dupes to clobber?
                this.matchedSystems[system.name] = system;
                this.comboSystemName.Items.Add(system.name);
            }

            //comboSystemName.Text = searchName;
            comboSystemName.SelectionStart = searchName.Length;
            //comboSystemName.SelectionLength = 0; // comboSystemName.Text.Length;

            lastSearch = searchName;

        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            game.cmdr.sphereLimit.active = true;
            game.cmdr.sphereLimit.radius = (double)numRadius.Value;
            game.cmdr.sphereLimit.centerSystemName = comboSystemName.Text;
            game.cmdr.sphereLimit.centerStarPos = targetStarPos!;
            game.cmdr.Save();

            this.Close();
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            game.cmdr.sphereLimit.active = false;
            game.cmdr.Save();

            this.Close();
        }

        private void comboSystemName_TextUpdate(object sender, EventArgs e)
        {
            Game.log($"txt update: {this.interruptCount}");
            this.interruptCount++;
            if (this.interruptCount > 1) return;

            var targetSystem = this.matchedSystems.GetValueOrDefault(comboSystemName.Text);
            if (targetSystem != null)
            {
                // we have an exact match
                setTargetSystem(targetSystem);
            }
            else
            {
                // no match - do a lookup
                txtStarPos.Text = "";
                targetStarPos = null!;
                _ = lookupSystem(comboSystemName.Text);
            }
        }

        private void setTargetSystem(StarSystem? targetSystem)
        {
            if (targetSystem != null)
            {
                targetStarPos = targetSystem.coords;
                txtStarPos.Text = $"[ {targetSystem.coords.x} , {targetSystem.coords.y} , {targetSystem.coords.z} ]";

                var dist = Util.getSystemDistance(game.cmdr.starPos, targetStarPos).ToString("N2");
                txtCurrentDistance.Text = $"{dist}ly";
            }
            else
            {
                txtStarPos.Text = "";
                targetStarPos = null;
                txtCurrentDistance.Text = $"-";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboSystemName_SelectedIndexChanged(object sender, EventArgs e)
        {

            var targetSystem = this.matchedSystems.GetValueOrDefault(comboSystemName.Text);
            this.setTargetSystem(targetSystem);
        }
    }
}

// Col 173 Sector OX-S
// b20