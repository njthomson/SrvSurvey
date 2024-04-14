using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    internal class HumanSiteData : Data
    {
        #region static loading code

        private static string rootFolder = Path.Combine(Program.dataFolder, "human");

        private static string getFilename(ApproachSettlement entry)
        {
            return $"{entry.SystemAddress}-{entry.MarketID}-{entry.Name}.json";
        }

        public static HumanSiteData? Load(long systemAddress, long marketId)
        {
            var fid = Game.activeGame?.fid ?? Game.settings.lastFid!;
            var folder = Path.Combine(rootFolder, fid!);
            if (!Util.isOdyssey) folder = Path.Combine(folder, "legacy");

            Directory.CreateDirectory(folder);
            var filepath = Directory.GetFiles(folder, $"{systemAddress}-{marketId}-*.json").FirstOrDefault();

            // do not create if not found
            if (filepath == null || !File.Exists(filepath)) return null;

            var data = Data.Load<HumanSiteData>(filepath);

            if (data != null)
                data.template = HumanSiteTemplate.get(data.economy, data.subType);

            return data;
        }

        #endregion

        public HumanSiteData(ApproachSettlement? entry)
        {
            if (entry != null)
            {
                this.name = entry.Name;
                this.marketId = entry.MarketID;
                this.location = entry;

                this.economy = Util.toEconomy(entry.StationEconomy);
                this.economyLocalized = entry.StationEconomy_Localised!;
            }
        }

        public override string ToString()
        {
            return $"{this.economy} #{this.subType} ({this.marketId})";
        }

        #region data members

        public string name;
        public long marketId;
        public StationType stationType;
        public int subType;
        public Economy economy;
        public string economyLocalized;
        public LatLong2 location;
        public float heading = -1;
        public LandingPads landingPads;

        #endregion

        [JsonIgnore]
        public int targetPad;

        [JsonIgnore]
        public HumanSiteTemplate? template;

        public void dockingRequested(DockingRequested entry)
        {
            this.landingPads = entry.LandingPads;

            // some subTypes can be inferred from just the economy and pad configuration...
            if (this.subType == 0)
            {
                this.subType = mapLandingPadConfigToSubType.GetValueOrDefault($"{this.economy}/{this.landingPads}");
                if (this.subType > 0)
                    this.template = HumanSiteTemplate.get(this.economy, this.subType);
                Game.log($"dockingRequested: {this.economy}/{this.landingPads} matched subType: #{this.subType}");
            }
        }

        public void dockingGranted(DockingGranted entry)
        {
            this.stationType = entry.StationType;
            this.targetPad = entry.LandingPad;
        }

        public void docked(Docked entry, int shipHeading)
        {
            this.landingPads = entry.LandingPads;
            if (this.targetPad == 0) return; // (when in a taxi)

            // infer the subType
            if (this.subType == 0) 
                this.inferSubtypeFromDocked();

            if (this.targetPad == 1)
            {
                // pad 1 is always the definitive source of the site's heading
                this.heading = shipHeading;
            }
            else if (this.template == null)
            {
                Game.log($"Why no template already?!");
            }
            else
            {
                // for other pads, we must apply their rotation to the ships heading
                var padRot = this.template.landingPads[this.targetPad - 1].rot;
                this.heading = shipHeading + padRot;
                if (this.heading >= 360) this.heading -= 360;
            }

            // --- tmp ---
            Game.log($"Docked at: pad #{this.targetPad} - {this.name} ({this.marketId})");
            Game.log($"Station location: {this.location}");
            Game.log($"Ship location: {Status.here}, heading: {shipHeading}°");

            var radius = Game.activeGame.status.PlanetRadius;
            var dist = Util.getDistance(this.location, Status.here, radius);
            var bearing = Util.getBearing(this.location, Status.here);

            Game.log($"dist: {(int)dist}, bearing: {(int)bearing}");
            var foo = (int)(bearing - shipHeading);
            if (foo < 0) foo += 360;
            Game.log($"pad angle? {foo}°");

            Game.log($"site heading? {this.heading}°");
            var td = new TrackingDelta(radius, this.location);
            Game.log($"td: {td}");
            // --- tmp ---

            // Only save if we inferred the subType and have a real heading
            if (this.subType > 0 && this.heading != -1)
            {
                if (this.filepath == null)
                {
                    var fid = Game.activeGame?.fid ?? Game.settings.lastFid!;
                    var folder = Path.Combine(rootFolder, fid!);
                    if (!Util.isOdyssey) folder = Path.Combine(folder, "legacy");

                    this.filepath = Path.Combine(folder, $"{entry.SystemAddress}-{this.marketId}-{this.name}.json");
                }
                this.Save();
            }
        }

        private void inferSubtypeFromDocked()
        {
            // infer the subType by matching the pad we landed in
            var shipDist = Util.getDistance(this.location, Status.here, Game.activeGame!.status.PlanetRadius);

            foreach (var template in HumanSiteTemplate.templates)
            {
                if (template.economy != this.economy) continue; // wrong economy
                if (this.targetPad > template.landingPads.Count) continue; // not enough pads

                // are we within ~5m of the expected pad? Is ~5m enough?
                var padDist = template.landingPads[this.targetPad - 1].dist;
                var match = Util.isClose(padDist, shipDist, 5);
                if (match)
                {
                    this.subType = template.subType;
                    this.template = HumanSiteTemplate.get(this.economy, this.subType);
                    Game.log($"inferSubtypeFromDocked: {this.economy}, pad #{this.targetPad} distance matched subType: #{this.subType} (pad:{padDist} vs ship:{(float)shipDist})");
                    return;
                }
                else
                {
                    // TODO: Remove once templates are populated
                    Game.log($"not a match - {this.economy}, pad #{this.targetPad}: pad:{padDist} vs ship:{(float)shipDist}");
                }
            }

            if (this.subType == 0)
                Game.log($"inferSubtypeFromDocked: Doh! We should have been able to infer the subType by this point :(");
        }

        #region static mappings

        private static Dictionary<string, int> mapLandingPadConfigToSubType = new Dictionary<string, int>()
        {
            { "Agriculture/Small:2, Medium:0, Large:1", 4 },

            { "Military /Small:0, Medium:1, Large:0", 1 },
            { "Military /Small:2, Medium:0, Large:1", 2 },
            { "Military /Small:1, Medium:0, Large:0", 4 },

            { "Extraction/Small:0, Medium:0, Large:1", 3 },
            { "Extraction/Small:0, Medium:1, Large:0", 4 },
            { "Extraction/Small:1, Medium:0, Large:0", 5 },

            { "Industrial/Small:1, Medium:0, Large:0", 1 },
            { "Industrial/Small:0, Medium:1, Large:0", 3 },
            { "Industrial/Small:1, Medium:0, Large:1", 4 },

            { "HighTech/Small:1, Medium:0, Large:1", 1 },
            { "HighTech/Small:3, Medium:0, Large:0", 4 },

            { "Tourist/Small:2, Medium:0, Large:2", 1 },
            { "Tourist/Small:0, Medium:2, Large:0", 2 },
            { "Tourist/Small:2, Medium:0, Large:1", 3 },
            { "Tourist/Small:1, Medium:0, Large:1", 4 },
        };

        #endregion
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum Economy
    {
        Unknown, // ?
        Agriculture, // $economy_Agri;
        Colony, // $economy_Colony;
        Damaged, // $economy_Damaged;
                 // ? Engineer, // 
        Extraction, // $economy_Extraction;
        HighTech, // $economy_HighTech;
        Industrial, // $economy_Industrial;
        Military, // $economy_Military;
        Prison, // $economy_Prison;
        PrivateEnterprise, // $economy_Carrier;
        Refinery, // $economy_Refinery;
        Repair, // $economy_Repair;
        Rescue, // $economy_Rescue;
        Service, // $economy_Service;
        Terraforming, // $economy_Terraforming;
        Tourist, // $economy_Tourism;
    }
}
