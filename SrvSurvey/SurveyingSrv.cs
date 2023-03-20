using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SrvSurvey
{
    delegate void OnSurveyChange();

    /// <summary>
    /// Tracks the last settlement approached, Touchdown and Codex scans
    /// </summary>
    class SurveyingSrv
    {
        public event OnSurveyChange SurveyChange;
        private readonly JournalWatcher watcher;
        [JsonIgnore]
        public SiteTemplate siteTemplate;
        [JsonIgnore]
        public bool AtSettlement;

        public DateTime timeStamp { get; set; }

        // key fields that need to be known about a settlement
        public string starSystem { get; set; }
        public long systemAddress { get; set; }
        public string bodyName { get; set; }
        /// <summary>
        /// How many meters are spanned by 1°
        /// </summary>
        public double dist1Deg { get; set; }

        public string settlementName { get; set; }
        public string settlementType { get; set; }
        public int settlementHeading { get; set; } = -1;
        public int relicHeading { get; set; } = -1;

        public string cmdrName { get; set; }


        // key things need for rendering
        public LatLong pointSettlement { get; set; }
        [JsonIgnore]
        public LatLong pointTouchdown;

        public HashSet<string> relicTowers { get; set; } = new HashSet<string>();
        public SortedDictionary<string, string> puddles { get; set; } = new SortedDictionary<string, string>();

        /// <summary>
        /// do not consider any journal items ahead of this
        /// </summary>
        private int settlementStartIndex;
        /// <summary>
        /// do not consider any journal items after of this
        /// </summary>
        //private int settlementEndIndex;

        private bool initializing = true;

        private Status status;

        public SurveyingSrv(JournalWatcher watcher)
        {
            if (watcher == null) return;

            this.watcher = watcher;
            watcher.onJournalEntry += Watcher_onJournalEntry;

            this.status = new Status(true);
            this.status.StatusChanged += onStatusChange;


            // assume we already landed before this point, so we need to scan back in the current journal for the following...
            int lastEntry = this.watcher.Entries.Count - 1;

            // get Cmdr name
            var cmdr = watcher.FindEntryByType<Commander>(lastEntry, true);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            this.cmdrName = cmdr.Name;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            var lastTouchdown = watcher.FindEntryByType<Touchdown>(lastEntry, true);
            if (lastTouchdown != null && lastTouchdown.Latitude == 0 && lastTouchdown.Longitude == 0)
            {
                lastTouchdown = watcher.FindEntryByType<Touchdown>(watcher.Entries.IndexOf(lastTouchdown) - 1, true);
            }
            var lastLiftoff = watcher.FindEntryByType<Liftoff>(lastEntry, true);

            if (lastTouchdown == null) // && (status.InSrv || status.OnFoot))
            {
                // we are on a planet already, touchdown happened in a previous session
                this.initFromLocation();
            }
            // if there is no Touchdown, or the last Listoff is after Touchdown ... we're not landed
            else if (lastLiftoff != null && lastLiftoff.timestamp > lastTouchdown.timestamp)
            {
                // exit early
                return;
            }

            if (lastTouchdown != null)
            {
                // we are on the ground - find journalling start point
                var index = watcher.Entries.IndexOf(lastTouchdown);
                this.onJournalEntry(lastTouchdown, index);

                // find prior ApproachSettlement entry
                for (int n = index; n >= 0; n--)
                {
                    var approachEntry = watcher.Entries[n] as ApproachSettlement;
                    if (approachEntry != null)
                    {
                        if ((lastTouchdown.Body == null || lastTouchdown.Body == approachEntry.BodyName)
                        && approachEntry.Name == lastTouchdown.NearestDestination
                        && approachEntry.Latitude != 0 && approachEntry.Longitude != 0)
                        {
                            this.pointSettlement = new LatLong(approachEntry);
                            if (this.bodyName == null && approachEntry.BodyName != null)
                            {
                                this.bodyName = approachEntry.BodyName;
                            }
                            break;
                        }
                    }
                }

                // disregard journal entries after this point
                //this.settlementStartIndex = watcher.GetSettlementStartIndexBefore(index);
            }

            // TODO: get planetRadius from scan event
            var bodyCircumferance = status.PlanetRadius * Math.PI * 2F;
            this.dist1Deg = bodyCircumferance / 360D;

            // try importing an existing file
            this.Import();

            if (this.settlementStartIndex < 0) return;

            // process journal entries
            for (int n = this.settlementStartIndex; n < watcher.Entries.Count; n++)
            {
                this.onJournalEntry((dynamic)watcher.Entries[n], n);
            }

            this.initializing = false;
        }

        private void initFromLocation()
        {
            this.settlementStartIndex = 0;
            var location = watcher.FindEntryByType<Location>(-1, true);

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
            onJournalEntry(location, watcher.Entries.IndexOf(location));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8604 // Possible null reference argument.
            Debug.WriteLine("!");

            // sadly, we don't have location.NearestDestination_Localised;
            // but we do have ApproachSettlement entries - so we just need to decide which of those is closest.
        }

        private void onStatusChange()
        {
            // ??
        }

        public void setSettlementType(string newSettlementType)
        {
//            this.settlementType = newSettlementType;
//            if (this.settlementType != null && SiteTemplate.sites.ContainsKey(this.settlementType))
//            {
//                this.siteTemplate = SiteTemplate.sites[this.settlementType];
//                LatLong.Scale = this.siteTemplate.scaleFactor;
//            }
//            else
//            {
//#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
//                this.siteTemplate = null;
//#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
//            }
        }
        public void Export()
        {
            if (this.initializing) return;

            string filename = $"{this.bodyName} - {this.settlementName}.json";
            string filepath = Path.Combine(SrvSurvey.journalFolder, "survey", filename);

            // alpha sort these before saving
            this.relicTowers = this.relicTowers.OrderBy(_ => _).ToHashSet();

            //var foo = this.puddles.OrderBy(_ => _.Key); //.ToDictionary<string, string>(_ => _);
            //this.puddles = new SortedDictionary<string, string>(this.puddles);
            //Enumerable.ToDictionary(foo, )
            //var bar = foo.ToDictionary<string, string>(_ => _.Key);

            this.timeStamp = DateTime.UtcNow;
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);

            Directory.CreateDirectory(Path.Combine(SrvSurvey.journalFolder, "survey"));
            File.WriteAllText(filepath, json);
        }

        private void Import()
        {
            if (this.bodyName != null && this.settlementHeading >= 0) return;

            string filename = $"{this.bodyName} - {this.settlementName}.json";
            string filepath = Path.Combine(SrvSurvey.journalFolder, "survey", filename);
            if (!File.Exists(filepath)) return;

            // read and parse file contents into tmp object
            var json = File.ReadAllText(filepath);
            var obj = JsonConvert.DeserializeObject<SurveyingSrv>(json);

            // ... assign all property values from tmp object 
            var allProps = typeof(SurveyingSrv).GetProperties(Program.InstanceProps);
            foreach (var prop in allProps)
            {
                //prop.GetValue(obj) prop.PropertyType.GetDefaultMembers()
                prop.SetValue(this, prop.GetValue(obj));
            }

            this.setSettlementType(this.settlementType);
        }

        private void fireChange()
        {
            this.Export();
            if (this.SurveyChange != null) this.SurveyChange();
        }

        private void Watcher_onJournalEntry(JournalEntry entry, int index)
        {
            this.onJournalEntry((dynamic)entry, index);
        }

        private void onJournalEntry(JournalEntry entry, int index)
        {
            // ignore
        }

        private void onJournalEntry(Touchdown entry, int index)
        {
            if (entry == null) return;

            // if we land in the middle of nowhere - we're not at a settlement
            this.AtSettlement = entry.NearestDestination_Localised != null;

            if (entry.Latitude != 0 && entry.Longitude != 0)
            {
                this.pointTouchdown = new LatLong(entry);
            }
            this.starSystem = entry.StarSystem;
            this.systemAddress = entry.SystemAddress;
            if (entry.Body != null)
            {
                this.bodyName = entry.Body;
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            this.settlementName = entry.NearestDestination_Localised;
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        private void onJournalEntry(Location entry, int index)
        {
            if (this.pointSettlement != LatLong.Empty) return;

            this.pointTouchdown = new LatLong(entry);
            this.starSystem = entry.StarSystem;
            this.systemAddress = entry.SystemAddress;
            this.bodyName = entry.Body;

            var settlements = watcher.Entries.Where(_ => _ is ApproachSettlement).Cast<ApproachSettlement>();
            var nearestSettlements = settlements.OrderBy((ApproachSettlement _) =>
            {
                var dp = new LatLong(_) - this.pointTouchdown;
                return dp.AsDeltaDist();
            });
            var nearestSettlement = nearestSettlements.First();

            this.pointSettlement = new LatLong(nearestSettlement);
            this.settlementName = nearestSettlement.Name_Localised;
            this.AtSettlement = true;
        }


        private void onJournalEntry(Liftoff entry, int index)
        {
            this.AtSettlement = false;
            this.pointTouchdown = new LatLong();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            this.settlementName = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        private void onJournalEntry(CodexEntry entry, int index)
        {
            // ignore everything but Relic Towers
            if (entry.Name != "$Codex_Ent_Relic_Tower_Name;") return;
            // ignore until we know what what we're in
            if (siteTemplate == null) return;

            // get delta point relative to settlement origin and find the nearest tower
            var dp = new LatLong(entry) - this.pointSettlement;
            var towerName = siteTemplate.getNearestRelicTower(dp, -LatLong.degToRad(this.settlementHeading));
            if (towerName != null)
            {
                this.relicTowers.Add(towerName);
            }

            this.fireChange();
        }

        private void onJournalEntry(SendText entry, int index)
        {
            var words = entry.Message.ToLower().Split(' ');

            switch (words[0])
            {
                // alpha | beta | gamma ]
                case "alpha":
                case "beta":
                case "gamma":
                    this.settlementType = words[0];
                    break;

                // site [ alpha | beta | gamma ]
                case "site":
                    this.parseSiteStuff(words);
                    break;

                case "relic":
                    this.parseRelicStuff(words);
                    break;

                    // site [ alpha | beta | gamma ]
            }
        }

        private void parseSiteStuff(string[] words)
        {
            var wordTwo = words[1].ToLower();

            switch (wordTwo)
            {
                case "alpha":
                case "beta":
                    //case "gamma":
                    this.setSettlementType(wordTwo);
                    this.fireChange();
                    break;

                case "heading":
                    var heading = -1;
                    int.TryParse(words[2], out heading);
                    if (heading >= 0)
                    {
                        this.settlementHeading = heading;
                        this.fireChange();
                    }
                    break;
            }
        }

        private void parseRelicStuff(string[] words)
        {
            var wordTwo = words[1].ToLower();

            switch (wordTwo)
            {
                case "heading":
                    if (words.Length >= 3)
                    {
                        var heading = -1;
                        int.TryParse(words[2], out heading);
                        if (heading >= 0)
                        {
                            this.relicHeading = heading;
                            this.fireChange();
                        }
                    }
                    break;
            }
        }

    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
