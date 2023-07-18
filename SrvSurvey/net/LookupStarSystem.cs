using SrvSurvey.game;
using SrvSurvey.net.EDSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.net
{
    delegate void StarSystemMatch(StarSystem? starSystem);


    internal class LookupStarSystem
    {
        public event StarSystemMatch? onSystemMatch;

        private ComboBox combo;
        private int interruptCount;
        private Dictionary<string, StarSystem> matchedSystems = new Dictionary<string, StarSystem>();
        private string lastSearch;

        public LookupStarSystem(ComboBox combo)
        {
            this.combo = combo;

            combo.TextUpdate += Combo_TextUpdate;
            combo.SelectedIndexChanged += Combo_SelectedIndexChanged;
            combo.KeyDown += Combo_KeyDown;
            combo.Items.Clear();
            combo.Items.Add("(type a system name begin)");
        }

        private void Combo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && onSystemMatch != null)
            {
                var targetSystem = this.matchedSystems.FirstOrDefault(_ => _.Key.Equals(combo.Text, StringComparison.OrdinalIgnoreCase)).Value;
                onSystemMatch(targetSystem);
            }
        }

        private void Combo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (onSystemMatch != null)
            {
                var targetSystem = this.matchedSystems.GetValueOrDefault(combo.Text);
                onSystemMatch(targetSystem);
            }
        }

        private void Combo_TextUpdate(object? sender, EventArgs e)
        {
            if (combo.Text == "(type a system name begin)") return;

            this.interruptCount++;
            if (this.interruptCount > 1) return;

            var targetSystem = this.matchedSystems.GetValueOrDefault(this.combo.Text);
            if (targetSystem == null)
            {
                // no match - do a lookup
                _ = lookupSystem(combo.Text);
            }
        }

        private async Task lookupSystem(string searchName)
        {
            try
            {
                Game.log($"lookupSystem begin: {this.interruptCount}");

                if (string.IsNullOrEmpty(searchName)) return;

                // avoid a network request if we already have some hits in matched systems
                var hits = this.matchedSystems.Values.Where(_ => _.name.StartsWith(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (hits.Count > 0 && searchName.Length >= lastSearch.Length)
                {
                    Game.log($"use existing for '{searchName}'! {this.interruptCount}");

                    processResults(searchName, hits);
                    return;
                }

                // otherwise - make a network request
                combo.Items.Clear();
                combo.Items.Add("Searching ...");
                combo.DroppedDown = true;
                combo.SelectionStart = searchName.Length;
                combo.SelectionLength = combo.Text.Length;

                var results = await Game.edsm.getSystems(searchName);
                Game.log($"Found {results.Length} systems from: {searchName}");

                // stop if we need to go round again
                if (this.interruptCount > 1) return;

                if (results == null || results.Length == 0)
                {
                    // no matches found
                    this.matchedSystems.Clear();
                    combo.Items.Clear();
                    combo.Items.Add("No match found");
                    combo.Text = searchName;
                    combo.SelectionStart = searchName.Length;
                    combo.SelectionLength = 0;
                }
                else
                {
                    // some systems found
                    processResults(searchName, results);
                }
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
                    _ = this.lookupSystem(combo.Text);
                }
            }
        }

        private void processResults(string searchName, IEnumerable<StarSystem> matches)
        {
            Game.log($"processResults for '{searchName}'! {this.interruptCount}");

            // ignore older results
            if (this.interruptCount == 0) return;

            this.matchedSystems.Clear();
            combo.Items.Clear();
            foreach (var system in matches)
            {
                // allow dupes to clobber?
                this.matchedSystems[system.name] = system;
                this.combo.Items.Add(system.name);
            }

            combo.SelectionStart = searchName.Length;
            lastSearch = searchName;
        }
    }
}
