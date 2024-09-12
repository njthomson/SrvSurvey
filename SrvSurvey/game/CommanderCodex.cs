using Newtonsoft.Json;
using System.Globalization;

namespace SrvSurvey.game
{
    internal class CommanderCodex : Data
    {
        public static CommanderCodex Load(string fid, string commanderName)
        {
            var filepath = Path.Combine(Program.dataFolder, $"{fid}-codex.json");

            return Data.Load<CommanderCodex>(filepath)
                ?? new CommanderCodex()
                {
                    filepath = filepath,
                    commander = commanderName,
                    fid = fid,
                };
        }

        private static CommanderCodex LoadRegion(string fid, string commanderName, int regionId)
        {
            var filepath = Path.Combine(Program.dataFolder, $"{fid}-codex-{regionId}.json");

            return Data.Load<CommanderCodex>(filepath)
                ?? new CommanderCodex()
                {
                    filepath = filepath,
                    commander = commanderName,
                    fid = fid,
                    region = GalacticRegions.getDisplayNameFromIdx(regionId),
                };
        }

        public void reload()
        {
            // reload relevant data from file
            var data = Data.Load<CommanderCodex>(this.filepath)!;
            if (data?.codexFirsts == null) return;

            this.codexFirsts = data.codexFirsts;
            this.regionalFirsts.Clear();
        }

        public string fid;
        public string commander;
        public string? region;

        /// <summary>
        /// The timestamp, system address and body Id each entryId was first encountered
        /// </summary>
        public Dictionary<long, CodexFirst> codexFirsts = new Dictionary<long, CodexFirst>();

        /// <summary>
        /// The timestamp, system address and body Id each entryId was first encountered, per galactic region
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, CommanderCodex> regionalFirsts = new Dictionary<int, CommanderCodex>();

        /// <summary>
        /// EntryID's for Green Gas Giants present in CodexRef
        /// </summary>
        private static List<long> entryIdGGGs = new List<long>() { 1200102, 1200302, 1200402, 1200502, 1200602, 1200702, 1200802, 1200902 };

        public float completionProgress
        {
            get
            {
                if (Game.codexRef.codexRefCount == 0) return 0;

                // disregard discoveries for things not in CodexRef
                const long minBio = 1400102;
                var countValid = codexFirsts.Keys.Count(k => k >= minBio || entryIdGGGs.Contains(k));

                return 1f / Game.codexRef.codexRefCount * countValid;
            }
        }

        private CommanderCodex getRegionalTracker(int regionId)
        {
            // load if not in memory
            if (!this.regionalFirsts.ContainsKey(regionId))
                this.regionalFirsts[regionId] = LoadRegion(this.fid, this.commander, regionId);

            return this.regionalFirsts[regionId];
        }

        public void trackCodex(string displayName, long entryId, DateTime timestamp, long systemAddress, int? bodyId, int regionId)
        {
            // check galactic firsts
            this.trackCodex(displayName, entryId, timestamp, systemAddress, bodyId);

            // then check regional firsts
            var regionalTracker = this.getRegionalTracker(regionId);
            regionalTracker.trackCodex(displayName, entryId, timestamp, systemAddress, bodyId);
        }

        private void trackCodex(string displayName, long entryId, DateTime timestamp, long systemAddress, int? bodyId)
        {
            // exit early if this is not new
            if (this.codexFirsts.ContainsKey(entryId) && timestamp >= this.codexFirsts[entryId].time && this.codexFirsts[entryId].address != -1) return;

            // add/update list and save
            this.codexFirsts[entryId] = new CodexFirst(timestamp, systemAddress, bodyId ?? -1);
            var scope = this.region ?? "cmdr";
            Game.log($"New first CodexEntry for {scope}! {displayName} ({entryId}) at systemAddress: {systemAddress}, bodyId: {bodyId}");

            // sort by entryId before saving
            this.codexFirsts = this.codexFirsts.OrderBy(_ => _.Key).ToDictionary(_ => _.Key, _ => _.Value);
            this.Save();

            // If the Codex Bingo form is open - make it re-calculate
            FormCodexBingo.activeForm?.calcCompletions();
        }

        public bool isPersonalFirstDiscovery(long entryId, long systemAddress, int bodyId)
        {
            var match = this.codexFirsts.GetValueOrDefault(entryId);

            // return true if entryId, systemAddress AND bodyId all match
            return match?.address == systemAddress && match?.bodyId == bodyId;
        }

        public bool isDiscovered(string entryId)
        {
            var exists = this.codexFirsts.ContainsKey(long.Parse(entryId));
            return exists;
        }

        public bool isDiscoveredInRegion(string entryId, string region)
        {
            var regionId = GalacticRegions.getIdxFromName(region);
            return isDiscoveredInRegion(entryId, regionId);
        }

        public bool isDiscoveredInRegion(string entryId, int regionId)
        {
            var regionalTracker = regionId == 0
                ? this
                : this.getRegionalTracker(regionId);

            var exists = regionalTracker.codexFirsts.ContainsKey(long.Parse(entryId));
            return exists;
        }

        public CodexFirst? getEntry(string entryId, int regionId)
        {
            var regionalTracker = regionId == 0
                ? this
                : this.getRegionalTracker(regionId);

            var entry = regionalTracker.codexFirsts.GetValueOrDefault(long.Parse(entryId));
            return entry;
        }

    }

    [JsonConverter(typeof(CodexFirst.JsonConverter))]
    internal class CodexFirst
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTime time;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public long address;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int bodyId;

        public CodexFirst(DateTime timestamp, long address, int bodyId)
        {
            this.time = timestamp;
            this.address = address;
            this.bodyId = bodyId;
        }

        public override string ToString()
        {
            return time.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + $"_{address}_{bodyId}";
        }

        class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanConvert(Type objectType) { return false; }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var txt = serializer.Deserialize<string>(reader);
                if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

                // "{time}_{address}_{bodyId}"
                // eg: "2022-08-31T05:38:57_669611992529_11"
                var parts = txt.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var time = DateTime.Parse(parts[0], CultureInfo.InvariantCulture);
                var addr = long.Parse(parts[1], CultureInfo.InvariantCulture);
                var body = int.Parse(parts[2], CultureInfo.InvariantCulture);

                var data = new CodexFirst(time, addr, body);

                return data;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var data = value as CodexFirst;
                if (data == null) throw new Exception($"Unexpected value: {value?.GetType().Name}");

                var txt = data.ToString();
                writer.WriteValue(txt);
            }
        }

    }

}
