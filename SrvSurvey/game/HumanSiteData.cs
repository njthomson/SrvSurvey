using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SrvSurvey.units;

namespace SrvSurvey.game
{
    //[JsonConverter(typeof(HumanSiteData.JsonConverter))]
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
            var shipLatLong = Util.adjustForCockpitOffset(radius, Status.here, game.shipType, shipHeading);

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
                    // are we within ~5m of the expected pad? Is ~5m enough?
                    var offset = pad.offset - Util.getOffset(radius, this.location, heading - pad.rot);
                    if (offset.dist < 5) // TODO: adjust by pad size?
                    {
                        this.subType = template.subType;
                        this.template = HumanSiteTemplate.get(this.economy, this.subType);
                        Game.log($"inferSubtypeFromFoot: matched {this.economy} #{template.subType} from pad #{this.targetPad}, shipDistFromPadCenter:" + Util.metersToString(offset.dist));
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
                        Game.log($"not a match - {template.economy} #{template.subType}, pad #{this.targetPad} ({pad.size}): shipDistFromPadCenter: " + offset.dist.ToString("N2"));
                    }
                }
            }

            if (this.subType == 0)
                Game.log($"inferSubtypeFromDocked: Doh! We should have been able to infer the subType by this point :(");
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

        // Not needed after all?
        //class JsonConverter : Newtonsoft.Json.JsonConverter
        //{
        //    public override bool CanConvert(Type objectType)
        //    {
        //        return false;
        //    }

        //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        //    {
        //        var obj = serializer.Deserialize<JToken>(reader);
        //        if (obj == null || !obj.HasValues)
        //            return null;

        //        // read the simple fields
        //        var data = new HumanSiteData(null)
        //        {
        //            name = obj[nameof(HumanSiteData.name)]!.Value<string>()!,
        //            marketId = obj[nameof(HumanSiteData.marketId)]!.Value<long>()!,
        //            stationType = Enum.Parse<StationType>(obj[nameof(HumanSiteData.stationType)]!.Value<string>()!),
        //            subType = obj[nameof(HumanSiteData.subType)]!.Value<int>()!,
        //            economy = Enum.Parse<Economy>(obj[nameof(HumanSiteData.economy)]!.Value<string>()!),
        //            economyLocalized = obj[nameof(HumanSiteData.economyLocalized)]!.Value<string>()!,
        //            location = obj[nameof(HumanSiteData.location)]!.ToObject<LatLong2>()!,
        //            heading = obj[nameof(HumanSiteData.heading)]?.Value<float>() ?? -1,
        //            landingPads = obj[nameof(HumanSiteData.landingPads)]?.ToObject<LandingPads>()!,
        //            //buildings = new Dictionary<RectangleF, string>(obj[nameof(HumanSiteData.buildings)]?.ToObject<Dictionary<RectangleF, string>>()!),
        //        };

        //        //Game.log($"Reading: {data.bodyName} #{data.index}   ** ** ** ** {data.poiStatus.Count}");
        //        return data;
        //    }

        //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        //    {
        //        var data = value as HumanSiteData;

        //        if (data == null)
        //            throw new Exception($"Unexpected value: {value?.GetType().Name}");

        //        var obj = new JObject
        //        {
        //            { nameof(HumanSiteData.name), data.name },
        //            { nameof(HumanSiteData.marketId), data.marketId },
        //            { nameof(HumanSiteData.stationType), data.stationType.ToString() },
        //            { nameof(HumanSiteData.subType), data.subType },
        //            { nameof(HumanSiteData.heading), JToken.FromObject(data.heading) },
        //            { nameof(HumanSiteData.economy), data.economy.ToString() },
        //            { nameof(HumanSiteData.economyLocalized), data.economyLocalized },
        //            { nameof(HumanSiteData.location), JToken.FromObject(data.location) },
        //            { nameof(HumanSiteData.landingPads), JToken.FromObject(data.landingPads) },
        //            //{ nameof(HumanSiteData.buildings), new JObject(data.buildings) },
        //        };

        //        //Game.log($"Writing: {data.bodyName} #{data.index}   ** ** ** ** {data.poiStatus.Count}");
        //        obj.WriteTo(writer);
        //    }
        //}
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
