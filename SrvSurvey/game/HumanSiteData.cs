using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
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

        [JsonIgnore]
        public long systemAddress
        {
            get
            {
                if (this.filepath == null)
                    return Game.activeGame?.systemData?.address ?? 0;

                var filename = Path.GetFileName(this.filepath);
                var address = filename.Substring(0, filename.IndexOf("-"));
                return long.Parse(address);
            }
        }

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
            if (entry != null) // if is tmp
                this.landingPads = entry.LandingPads;
            if (this.targetPad == 0) return; // (happens when in a taxi)

            // infer the subType
            if (this.subType == 0 || entry == null)
                this.inferSubtypeFromDocked(shipHeading);

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
                this.heading = shipHeading - padRot;
                if (this.heading > 0) this.heading -= 360;
                if (this.heading < 0) this.heading += 360;
            }

            // Only save if we inferred the subType and have a real heading
            if (this.subType > 0 && this.heading != -1 && entry != null)
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

        /// <summary>
        /// Infer the subType by matching the pad we landed in
        /// </summary>
        private void inferSubtypeFromDocked(int shipHeading)
        {
            var game = Game.activeGame!;
            var radius = game.status.PlanetRadius;
            var shipLatLong = Util.adjustForCockpitOffset(radius, Status.here, game.currentShip.type, shipHeading);

            foreach (var template in HumanSiteTemplate.templates)
            {
                if (template.economy != this.economy) continue; // wrong economy
                if (this.targetPad > template.landingPads.Count) continue; // not enough pads
                var landPadSummary = template.landPadSummary(); // check pad configuration matches
                if (this.landingPads.Small != landPadSummary.Small || this.landingPads.Medium != landPadSummary.Medium || this.landingPads.Large != landPadSummary.Large) continue;

                // are we within ~5m of the expected pad? Is ~5m enough?
                var pad = template.landingPads[this.targetPad - 1];
                var shipOffset = Util.getOffset(radius, this.location, shipLatLong, shipHeading - pad.rot);

                //var dx = pad.offset.X - shipOffset.X;
                //var dy = pad.offset.Y - shipOffset.Y;
                //var shipDistFromPadCenter = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                var po = pad.offset - shipOffset;
                var shipDistFromPadCenter = po.dist;

                if (shipDistFromPadCenter < 50) // TODO: adjust by pad size?
                {
                    this.subType = template.subType;
                    this.template = HumanSiteTemplate.get(this.economy, this.subType);
                    Game.log($"inferSubtypeFromDocked: matched {this.economy} #{template.subType} from pad #{this.targetPad}, shipDistFromPadCenter:" + Util.metersToString(shipDistFromPadCenter));
                    return;
                }
                else
                {
                    // TODO: Remove once templates are populated
                    Game.log($"not a match - {template.economy} #{template.subType}, pad #{this.targetPad} ({pad.size}): shipDistFromPadCenter: " + shipDistFromPadCenter.ToString("N2"));
                }
            }

            if (this.subType == 0)
                Game.log($"inferSubtypeFromDocked: Doh! We should have been able to infer the subType by this point :(");

            if (this.subType != 0)
                this.template = HumanSiteTemplate.get(this.economy, this.subType);
        }

        /// <summary>
        /// Infer the subType by matching the pad we are standing in
        /// </summary>
        public void inferSubtypeFromFoot(int heading)
        {
            Game.log($"Try infer site from heading: {heading}");
            var game = Game.activeGame!;
            var radius = game.status.PlanetRadius;

            foreach (var template in HumanSiteTemplate.templates)
            {
                if (template.economy != this.economy) continue; // wrong economy
                var landPadSummary = template.landPadSummary(); // check pad configuration matches

                foreach (var pad in template.landingPads)
                {
                    // are we within ~25m of the expected pad? Is ~5m enough?
                    var cmdrOffset = Util.getOffset(radius, this.location, heading - pad.rot);
                    var delta = pad.offset - cmdrOffset;
                    if (delta.dist < 25) // TODO: adjust by pad size?
                    {
                        this.subType = template.subType;
                        this.template = HumanSiteTemplate.get(this.economy, this.subType);
                        Game.log($"inferSubtypeFromFoot: matched {this.economy} #{template.subType} from pad #{this.targetPad}, shipDistFromPadCenter:" + Util.metersToString(delta.dist));
                        this.template = HumanSiteTemplate.get(this.economy, this.subType);
                        this.landingPads = landPadSummary;

                        var padIdx = template.landingPads.IndexOf(pad);
                        if (padIdx == 0)
                        {
                            this.heading = heading - pad.rot;
                        }
                        else
                        {
                            // for other pads, we must apply their rotation to the ships heading
                            var padRot = this.template!.landingPads[padIdx].rot;
                            this.heading = heading - padRot;
                        }
                        if (this.heading > 360) this.heading -= 360;
                        if (this.heading < 0) this.heading += 360;

                        if (string.IsNullOrEmpty(this.filepath))
                            this.filepath = Path.Combine(rootFolder, game.cmdr.fid, $"{game.systemData!.address}-{this.marketId}-{this.name}.json");

                        this.Save();
                        return;
                    }
                    else
                    {
                        // TODO: Remove once templates are populated
                        Game.log($"inferSubtypeFromFoot: not a match - {template.economy} #{template.subType}, pad #{this.targetPad} ({pad.size}): shipDistFromPadCenter: " + delta.dist.ToString("N2"));
                    }
                }
            }

            if (this.subType == 0)
                Game.log($"inferSubtypeFromFoot: Doh! We should have been able to infer the subType by this point :(");
        }

        #region static mappings

        private static Dictionary<string, int> mapLandingPadConfigToSubType = new Dictionary<string, int>()
        {
            { "Agriculture/Small:2, Medium:0, Large:1", 4 },

            { "Military/Small:0, Medium:1, Large:0", 1 },
            { "Military/Small:2, Medium:0, Large:1", 2 },
            { "Military/Small:1, Medium:0, Large:0", 4 },

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
