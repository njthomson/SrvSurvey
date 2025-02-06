using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.forms;
using SrvSurvey.net;
using SrvSurvey.units;
using System.Diagnostics;

namespace SrvSurvey.game
{
    internal class FollowRoute : Data
    {
        #region static loading and house keeping/crud code

        private static readonly string dataFolder = Path.Combine(Program.dataFolder, $"routes");

        public static FollowRoute Load(string filename)
        {
            var folder = FollowRoute.dataFolder;
            Directory.CreateDirectory(folder);

            var filepath = Path.Combine(folder, $"{filename}.json");

            Game.log($"Loading journey: /routes/{filename}.json");
            var route = Data.Load<FollowRoute>(filepath)!;

            if (route == null)
            {
                // if not found, create with filepath populated but unsaved
                route = new FollowRoute()
                {
                    filepath = filepath,
                };
            }
            else
            {
                // prep certain properties after loading
                if (route.last + 1 < route.hops.Count)
                    route.nextHop = route.hops[route.last + 1];
            }

            return route;
        }

        #endregion

        #region data members

        /// <summary> If we are actively following this route </summary>
        public bool active = true;
        /// <summary> If we should auto copy the next hop to the clip board when entering the Gal-Map </summary>
        public bool autoCopy = true;
        /// <summary> The index of the last hop we reached </summary>
        public int last = -1;
        /// <summary> An ordered list of systems to be visited </summary>
        public List<Hop> hops = new();

        #endregion

        /// <summary> The next hop in the route </summary>
        [JsonIgnore]
        public Hop? nextHop;

        /// <summary> The route is complete when we have reached the final hop </summary>
        [JsonIgnore]
        public bool completed => this.last >= hops.Count;

        public void activate()
        {
            // do not allow the route to become active if it is completed
            if (this.completed)
            {
                Game.log($"FollowRoute.activate: cannot activate a completed route");
                this.active = false;
                return;
            }

            this.active = true;

            // set next hop based on last, or the first hop if none
            if (this.last < 0)
                this.nextHop = hops.FirstOrDefault();
            else
                this.nextHop = hops[last + 1];

            Game.log($"FollowRoute.activate: count hops: {hops.Count}, last: {last}, nextHop: {nextHop}");
        }

        public void disable()
        {
            Game.log($"FollowRoute.disable: count hops: {hops.Count}, last: {last}, nextHop was: {nextHop}");
            this.active = false;
            this.nextHop = null;
        }

        /// <summary>
        /// Returns the next route system or null if none or not valid
        /// </summary>
        public void setNextHop(StarRef star)
        {
            // exit early if we didn't start yet, or we jumped into a system that is not a known hop
            var idx = hops.FindIndex(h => h.id64 == star.id64);
            if (!active) return;

            var dirty = false;

            if (idx + 1 >= hops.Count)
            {
                // we reached the end of the route
                this.last = idx;
                this.disable();
                Game.log($"Route.setNextHop: reached end of route in: '{star}'");
                dirty = true;
            }
            else if (last + 1 == idx)
            {
                // we reached the next hop in the route - move to the system in the route
                this.last = idx;
                this.nextHop = hops[idx + 1];
                Game.log($"Route.setNextHop: #{idx + 1} in '{star}', set next: '{nextHop}'");
                dirty = true;
            }
            else if (idx >= 0)
            {
                Game.log($"Route.setNextHop: Following out of order? in '{star}' but expected: '{nextHop}'");
            }

            if (dirty)
                this.Save();

            BaseForm.get<FormRoute>()?.updateChecks(star);
        }

        /// <summary> If we should auto-copy the next hop </summary>
        [JsonIgnore]
        public bool useNextHop
        {
            get => this.active
                && this.autoCopy
                && this.nextHop != null;
        }

        /// <summary>
        /// An extended form of StarRef with extra details for routing
        /// </summary>
        [JsonConverter(typeof(Hop.JsonConverter))]
        public class Hop : StarRef
        {
            public string? notes;
            public bool refuel;
            public bool neutron;

            public Hop()
            { }

            public Hop(string name, long id64, double x, double y, double z, string? notes = null) : base(x, y, z, name, id64)
            {
                this.notes = notes;
            }

            public static Hop from(Spansh.Route.Result jump)
            {
                string? notes = null;
                if (jump.bodies != null)
                {
                    // Summarize body/landmark info as: "Stratum Tectonicas: [A5, B2]\r\nTussock Ignis: [A5]"
                    var mapLandmarkToBody = new Dictionary<string, HashSet<string>>();
                    foreach (var body in jump.bodies.OrderBy(b => b.id))
                    {
                        var bodyShortName = body.name.Replace($"{jump.name} ", "").Replace(" ", "");
                        if (body.landmarks == null)
                        {
                            // serialize body names to "Scan"
                            mapLandmarkToBody
                                .init("Scan")
                                .Add(bodyShortName);
                        }
                        else
                        {
                            // serialize landmarks on the body
                            foreach (var landmark in body.landmarks)
                            {
                                mapLandmarkToBody
                                    .init(landmark.subtype)
                                    .Add(bodyShortName);
                            }
                        }
                    }
                    if (mapLandmarkToBody.Count > 0)
                        notes = string.Join("\r\n", mapLandmarkToBody.Select(_ => $"{_.Key}: [{string.Join(", ", _.Value)}]"));
                }

                return new Hop(jump.name, jump.id64, jump.x, jump.y, jump.z, notes);
            }

            public static Hop from(Spansh.NeutronRoute.Result.Jump jump)
            {
                return new Hop(jump.system, jump.id64, jump.x, jump.y, jump.z);
            }

            public static Hop from(Spansh.TouristRoute.Result.Jump jump)
            {
                return new Hop(jump.system, jump.id64, jump.x, jump.y, jump.z);
            }

            public static Hop from(Spansh.GalaxyRoute.Result.Jump jump)
            {
                return new Hop(jump.name, jump.id64, jump.x, jump.y, jump.z)
                {
                    refuel = jump.must_refuel,
                    neutron = jump.has_neutron,
                };
            }

            class JsonConverter : Newtonsoft.Json.JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return false;
                }

                public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
                {
                    try
                    {
                        var obj = JToken.Load(reader);
                        if (obj == null) return null;

                        var hop = new Hop
                        {
                            name = obj["name"]?.Value<string>()!,
                            id64 = obj["id64"]?.Value<long>() ?? 0,
                            x = obj["x"]?.Value<double>() ?? 0,
                            y = obj["y"]?.Value<double>() ?? 0,
                            z = obj["z"]?.Value<double>() ?? 0,
                            // plus any optional fields
                            refuel = obj["refuel"]?.Value<bool>() ?? false,
                            neutron = obj["neutron"]?.Value<bool>() ?? false,
                            notes = obj["notes"]?.Value<string>(),
                        };

                        return hop;
                    }
                    catch (Exception ex)
                    {
                        Game.log($"Failed to parse StarRef: ${ex}");
                        Debugger.Break();
                        return null;
                    }
                }

                public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                {
                    var hop = value as Hop;
                    if (hop == null) throw new Exception($"Unexpected type: {value?.GetType().Name}");

                    var obj = new JObject();
                    obj["name"] = JToken.FromObject(hop.name);
                    obj["id64"] = JToken.FromObject(hop.id64);
                    obj["x"] = JToken.FromObject(hop.x);
                    obj["y"] = JToken.FromObject(hop.y);
                    obj["z"] = JToken.FromObject(hop.z);

                    // plus any optional fields
                    if (hop.refuel) obj["refuel"] = true;
                    if (hop.neutron) obj["neutron"] = true;
                    if (hop.notes != null) obj["notes"] = hop.notes;

                    // encode as json on a single line
                    var json = JsonConvert.SerializeObject(obj, Formatting.None);
                    writer.WriteRawValue(json);
                }
            }

        }
    }

}
