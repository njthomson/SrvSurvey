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
        public static GuardianSiteData Load(ApproachSettlement entry) // string bodyName, string siteName, LatLong2 location)
        {
            var idx = parseSettlementIdx(entry.Name);
            var namePart = "ruins"; // TODO: structures?
            string filepath = Path.Combine(Application.UserAppDataPath, "guardian", $"{entry.BodyName}-{namePart}-{idx}.json");

            Directory.CreateDirectory(Path.Combine(Application.UserAppDataPath, "guardian"));

            var data = Data.Load<GuardianSiteData>(filepath);
                if (data == null)
            {
                data = new GuardianSiteData()
                {
                    name = entry.Name_Localised,
                    type = SiteType.unknown,
                    index = idx,
                    filepath = filepath,
                    location = entry,
                    systemAddress = entry.SystemAddress,
                    bodyId = entry.BodyID,
                };
                data.Save();
            }
            return data;
        }

        #region data members

        public string name;

        public SiteType type;

        public int index;

        public LatLong2 location;

        public long systemAddress;

        public int bodyId;

        public int siteHeading;

        #endregion

        public static int parseSettlementIdx(string name)
        {
            const string ruinsPrefix = "$Ancient:#index=";
            // $Ancient:#index=2;
            if (name.StartsWith(ruinsPrefix))
            {
                return int.Parse(name.Substring(ruinsPrefix.Length, 1));
            }
            throw new Exception("Unkown site type");
            return -1;
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
