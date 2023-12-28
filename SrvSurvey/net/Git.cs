using Newtonsoft.Json;
using SrvSurvey.game;
using System.IO.Compression;

namespace SrvSurvey.net
{
    internal class Git
    {
        public static string pubDataFolder = Path.Combine(Program.dataFolder, "pub");
        private static HttpClient client;

        static Git()
        {
            Git.client = new HttpClient();
            Git.client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            Git.client.DefaultRequestHeaders.Add("user-agent", $"SrvSurvey-{Game.releaseVersion}");
        }

        public async Task updatePubData()
        {
            Game.log($"updatePubData ...");
            try
            {
                var json = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data.json");
                var pubData = JsonConvert.DeserializeObject<GitDataIndex>(json)!;
                Game.log($"pubDataSettlementTemplate - local: {Game.settings.pubDataSettlementTemplate}, remote: {pubData.settlementTemplate}");

                Directory.CreateDirectory(Git.pubDataFolder);
                var pubDataGuardian = Path.Combine(Git.pubDataFolder, "guardian");
                Directory.CreateDirectory(pubDataGuardian);

                if (pubData.settlementTemplate > Game.settings.pubDataSettlementTemplate)
                {
                    Game.log($"Downloading settlementTemplates.json");
                    var json2 = await Git.client.GetStringAsync($"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data/settlementTemplates.json");
                    var filepath = Path.Combine(Git.pubDataFolder, "settlementTemplates.json");
                    File.WriteAllText(filepath, json2);

                    // update settings to current level
                    Game.settings.pubDataSettlementTemplate = pubData.settlementTemplate;
                    Game.settings.Save();
                }

                Game.log($"pubDataGuardian - local: {Game.settings.pubDataGuardian}, remote: {pubData.guardian}");
                if (pubData.guardian > Game.settings.pubDataGuardian)
                {
                    Game.log($"Downloading guardian.zip ...");

                    var url = $"https://raw.githubusercontent.com/njthomson/SrvSurvey/main/data/guardian.zip";
                    var filepath = Path.Combine(Git.pubDataFolder, "guardian.zip");
                    Game.log($"{url} => {filepath}");
                    var bytes = await Git.client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(filepath, bytes);
                    ZipFile.ExtractToDirectory(filepath, pubDataGuardian, true);

                    // update settings to current level
                    Game.settings.pubDataGuardian = pubData.guardian;
                    Game.settings.Save();
                    Game.log($"Downloading guardian.zip - complete");
                }
            }
            catch (Exception ex)
            {
                Game.log($"Error in updatePubData:\r\n{ex}");
                FormErrorSubmit.Show(ex);
            }
            finally
            {
                Game.log($"updatePubData - complete");
            }
        }
    }

    internal class GitDataIndex
    {
        public int settlementTemplate;
        public int guardian;
    }
}
