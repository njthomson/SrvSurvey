using SrvSurvey.game;
using SrvSurvey.net.EDSM;
using System.Collections.Generic;
using System.Data;

namespace SrvSurvey
{
    internal partial class FormSphereLimit : Form
    {
        protected Game game = Game.activeGame!;

        private Dictionary<string, StarSystem> matchedSystems = new Dictionary<string, StarSystem>();
        private double[] targetStarPos;
        private string lastSearch = "";
        private bool requesting = false;
        private Task? pending;
        private int interruptCount;

        public FormSphereLimit()
        {
            InitializeComponent();

            txtCurrentSystem.Text = game.cmdr.currentSystem;

            // populate controls from settings
            numRadius.Value = (decimal)game.cmdr.sphereLimit.radius;
            comboSystemName.Text = game.cmdr.sphereLimit.centerSystemName;
            targetStarPos = game.cmdr.sphereLimit.centerStarPos;
            if (targetStarPos != null)
            {
                txtStarPos.Text = $"[ {targetStarPos[0]} , {targetStarPos[1]} , {targetStarPos[2]} ]";
                var dist = Util.getSystemDistance(game.cmdr.starPos, targetStarPos).ToString("N2");
                txtCurrentDistance.Text = $"{dist}ly";
            }
            else
            {
                txtCurrentDistance.Text = $"-";
            }
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
                this.requesting = true;

                comboSystemName.Items.Clear();
                comboSystemName.Items.Add("Searching ...");
                comboSystemName.DroppedDown = true;
                comboSystemName.SelectionStart = searchName.Length;
                comboSystemName.SelectionLength = comboSystemName.Text.Length;

                this.requesting = true;
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
                this.requesting = false;

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
                this.pending = null;
                if (runAgain)
                {
                    // lookpu again if text has changed?
                    this.interruptCount++;
                    _ = this.lookupSystem(comboSystemName.Text);
                }
            }
            /*
                        Game.spansh.getSystem(systemName).ContinueWith(t =>
                        {
                            GetSystemResponse rslt = null;
                            if (t.IsCompletedSuccessfully)
                            {
                                Game.log($"Got {t.Result.min_max.Count} systems");

                                rslt = t.Result;
                            }

                            Program.control!.Invoke((MethodInvoker)delegate
                            {
                                if (rslt != null && rslt.min_max.Count > 0)
                                {
                                    var targets = rslt.min_max.Where(_ => _.name.StartsWith(systemName, StringComparison.OrdinalIgnoreCase)).ToList();

                                    //var target = rslt.min_max.FirstOrDefault(_ => string.Equals(_.name, systemName, StringComparison.OrdinalIgnoreCase));
                                    if (targets.Count > 0)
                                    {
                                        // we have an exact match
                                        targetStarPos = new double[] { targets[0].x, targets[0].y, targets[0].z };
                                        txtStarPos.Text = $"[ {targets[0].x} , {targets[0].y} , {targets[0].z} ]";
                                    }

                                    if (targets.Count < 2)
                                        comboSystemName.DroppedDown = false;
                                    else
                                    {
                                        var top = rslt.min_max.Take(4).Select(_ => _.name).ToArray();
                                        comboSystemName.Items.Clear();
                                        comboSystemName.Items.AddRange(top);
                                        comboSystemName.DroppedDown = true;
                                        //comboSystemName.SelectionLength = 0;
                                        comboSystemName.SelectionStart = systemName.Length;
                                    }
                                }


                                btnDisable.Enabled = btnAccept.Enabled = txtStarPos.Enabled = true;
                            });
                        });
            */
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
            game.cmdr.sphereLimit.centerStarPos = targetStarPos;
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
                targetStarPos = null;
                this.pending = lookupSystem(comboSystemName.Text);
            }
        }

        private void setTargetSystem(StarSystem? targetSystem)
        {
            if (targetSystem != null)
            {
                targetStarPos = targetSystem.coords.starPos;
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