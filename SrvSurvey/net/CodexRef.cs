using Newtonsoft.Json;
using SrvSurvey.game;

namespace SrvSurvey.canonn
{
    internal class CodexRef
    {
        private static string codexRefPath = Path.Combine(Application.UserAppDataPath, "codexRef.json");
        private static string speciesRewardPath = Path.Combine(Application.UserAppDataPath, "speciesRewards.json");
        private static string entryIdRewardPath = Path.Combine(Application.UserAppDataPath, "entryIdRewards.json");

        private Dictionary<string, long>? rewards;
        private Dictionary<string, long>? rewardsByEntryId;

        public void init()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            loadOrganicRewards();
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
            if (!File.Exists(speciesRewardPath) || !File.Exists(entryIdRewardPath))
            {
                Game.log("Preparing organic rewards from codex/ref");
                var codexRef = await loadCodexRef();
                var foo = codexRef!.Values
                    .Where(_ => _.sub_category == "$Codex_SubCategory_Organic_Structures;" && _.reward > 0);

                rewards = new Dictionary<string, long>();
                rewardsByEntryId = new Dictionary<string, long>();
                foreach (var _ in foo)
                {
                    // extract the species prefix from the name, without the color variant part
                    var species = Util.getSpeciesPrefix(_.name);

                    if (!rewards.ContainsKey(species))
                    {
                        rewards.Add(species, (long)_.reward!);
                    }
                    else if (rewards[species] != (long)_.reward!)
                    {
                        Game.log($"BAD? {_.name} / {species} {rewards[species]} vs {(long)_.reward!}");
                    }

                    var entryIdKey = _.entryid.Substring(0, 5);
                    if (!rewardsByEntryId.ContainsKey(entryIdKey))
                    {
                        rewardsByEntryId.Add(entryIdKey, (long)_.reward!);
                    }
                    else if (rewardsByEntryId[entryIdKey] != (long)_.reward!)
                    {
                        Game.log($"BAD? {_.name} / {species} {rewardsByEntryId[entryIdKey]} vs {(long)_.reward!}");
                    }
                }

                File.WriteAllText(speciesRewardPath, JsonConvert.SerializeObject(rewards));
                File.WriteAllText(entryIdRewardPath, JsonConvert.SerializeObject(rewardsByEntryId));
            }
            else
            {
                Game.log("Reading organic rewards from disk");
                rewards = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(speciesRewardPath))!;
                rewardsByEntryId = JsonConvert.DeserializeObject<Dictionary<string, long>>(File.ReadAllText(entryIdRewardPath))!;
            }
        }

        public long getRewardForSpecies(string name)
        {
            if (rewards == null)
                loadOrganicRewards().Wait();

            var key = rewards!.Keys.FirstOrDefault(_ => name.StartsWith(_));
            if (key != null && rewards.ContainsKey(key))
                return rewards[key];
            else
                return -1;
        }

        public long getRewardForEntryId(string entryId)
        {
            if (rewardsByEntryId == null)
                loadOrganicRewards().Wait();
            entryId = entryId.Substring(0, 5);
            if (rewardsByEntryId.ContainsKey(entryId))
                return rewardsByEntryId[entryId];
            else
                return -1;
        }
    }
}
