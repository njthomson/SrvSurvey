using Newtonsoft.Json;
using SrvSurvey.game.canonn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    internal class CodexRef
    {
        private static string codexRefPath = Path.Combine(Application.UserAppDataPath, "codexRef.json");
        private static string speciesRewardPath = Path.Combine(Application.UserAppDataPath, "speciesRewards.json");

        private Dictionary<string, long>? rewards;

        public void init()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.loadOrganicRewards();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task<Dictionary<string, RefCodexEntry>> loadCodexRef()
        {
            string json;
            if (!File.Exists(codexRefPath))
            {
                Game.log("Requesting codex/ref from network");
                json = await new HttpClient().GetStringAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/query/codex/ref");
                File.WriteAllText(codexRefPath, json);
            }
            else
            {
                Game.log("Reading codex/ref from disk");
                json = File.ReadAllText(codexRefPath);
            }

            var codexRef = JsonConvert.DeserializeObject<Dictionary<string, RefCodexEntry>>(json)!;
            return codexRef;
        }

        public async Task loadOrganicRewards()
        {
            if (!File.Exists(speciesRewardPath))
            {
                Game.log("Preparing organic rewards from codex/ref");
                var codexRef = await loadCodexRef();
                var foo = codexRef!.Values
                    .Where(_ => _.sub_category == "$Codex_SubCategory_Organic_Structures;" && _.reward > 0);

                this.rewards = new Dictionary<string, long>();
                foreach (var _ in foo)
                {
                    // extract the species prefix from the name, without the color variant part
                    var species = _.name.Replace("$Codex_Ent_", "").Replace("_Name;", "");
                    var idx = species.LastIndexOf('_');
                    if (species.IndexOf('_') != idx)
                        species = species.Substring(0, idx);

                    species = $"$Codex_Ent_{species}_";

                    if (!this.rewards.ContainsKey(species))
                    {
                        this.rewards.Add(
                            species,
                            (long)_.reward!);
                    }
                    else if (this.rewards[species] != (long)_.reward!)
                    {
                        Game.log($"BAD? {_.name} / {species} {this.rewards[species]} vs {(long)_.reward!}");
                    }
                }

                var json = JsonConvert.SerializeObject(rewards);
                File.WriteAllText(speciesRewardPath, json);
            }
            else
            {
                Game.log("Reading organic rewards from disk");
                var json = File.ReadAllText(speciesRewardPath);
                this.rewards = JsonConvert.DeserializeObject<Dictionary<string, long>>(json)!;
            }
        }

        public long getRewardForSpecies(string name)
        {
            if (this.rewards == null)
                this.loadOrganicRewards().Wait();

            var key = this.rewards!.Keys.FirstOrDefault(_ => name.StartsWith(_));
            if (key != null && this.rewards.ContainsKey(key))
                return this.rewards[key];
            else
                return -1;
        }
    }
}
