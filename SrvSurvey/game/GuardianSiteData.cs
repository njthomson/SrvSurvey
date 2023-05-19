using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.canonn;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace SrvSurvey.game
{
    internal class GuardianSiteData : Data
    {
        public static string getFilename(ApproachSettlement entry)
        {
            var index = parseSettlementIdx(entry.Name);
            var namePart = "ruins"; // TODO: structures?
            return $"{entry.BodyName}-{namePart}-{index}.json";
        }

        public static GuardianSiteData Load(ApproachSettlement entry)
        {
            string filepath = Path.Combine(Application.UserAppDataPath, "guardian", Game.activeGame!.fid!, getFilename(entry));

            Directory.CreateDirectory(Path.Combine(Application.UserAppDataPath, "guardian"));

            var data = Data.Load<GuardianSiteData>(filepath);
            if (data == null)
            {
                data = new GuardianSiteData()
                {
                    name = entry.Name,
                    nameLocalised = entry.Name_Localised,
                    commander = Game.activeGame.Commander!,
                    type = SiteType.unknown,
                    index = parseSettlementIdx(entry.Name),
                    filepath = filepath,
                    location = entry,
                    systemAddress = entry.SystemAddress,
                    bodyId = entry.BodyID,
                    firstVisited = DateTimeOffset.UtcNow,
                };
                data.Save();
            }

            //if (data.type == SiteType.unknown)
            //{
            //    var grSite = Canonn.matchRuins(entry.BodyName);
            //    Game.log($"Matched grSite: #GR{grSite?.siteID}");

            //}

            return data;
        }

        #region data members

        public string name;
        public string nameLocalised;
        public string commander;
        public DateTimeOffset firstVisited;
        public DateTimeOffset lastVisited;
        [JsonConverter(typeof(StringEnumConverter))]
        public SiteType type;
        public int index;
        public LatLong2 location;
        public long systemAddress;
        public int bodyId;
        public int siteHeading = -1;
        public int relicTowerHeading = -1;

        #endregion

        [JsonIgnore]
        public bool isRuins { get => this.name.StartsWith("$Ancient:"); }

        public static int parseSettlementIdx(string name)
        {
            const string ruinsPrefix = "$Ancient:#index=";
            // $Ancient:#index=2;
            if (name.StartsWith(ruinsPrefix))
            {
                return int.Parse(name.Substring(ruinsPrefix.Length, 1));
            }
            throw new Exception("Unkown site type");
        }

        public enum SiteType
        {
            unknown,
            alpha,
            beta,
            gamma,
            // structures ... ?
        }
    }
}
