using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Text;

namespace SrvSurvey.net
{
    partial class Spansh
    {
        public async Task<SystemResponse> querySystems(SystemQuery query)
        {
            var queryJson = JsonConvert.SerializeObject(query, Formatting.None);
            Game.log($"Spansh.querySystems:\r\n{queryJson}");

            var body = new StringContent(queryJson, Encoding.ASCII, "application/json");
            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/systems/search", body);
            var json = await response.Content.ReadAsStringAsync();

            if (json == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + json);
            }

            var obj = JsonConvert.DeserializeObject<SystemResponse>(json)!;
            return obj;
        }

        #region Query

        internal class Query
        {
            public int size = 20;
            public int page = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public StarRef? reference_coords;

            public Dictionary<string, Filter> filters;

            internal abstract class Filter { }

            /// <summary> Filter by a single value property </summary>
            internal class Value : Filter
            {
                public string value;

                public Value() { }
                public Value(string value) { this.value = value; }
            }

            /// <summary> Filter by a multi value property </summary>
            internal class Values : Filter
            {
                public List<string> value;
                public Values() { value = new(); }
                public Values(params string[] values) { this.value = values.ToList(); }
            }
        }

        /*
        internal class BodyQuery : Query
        {
            public Sort sort = new();

            internal class Sort : List<Dictionary<string, List<SortCriteria>>>
            {
            }

            internal class SortCriteria
            {
                public string name;
                public Direction direction;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Direction
            {
                asc,
                desc,
            }

            internal class Filters
            {
                public Values atmosphere;
                public List<LandMark> landmarks;

                internal class Values
                {
                    public List<string> values;
                }

                internal class LandMark
                {
                    public string type;
                    public List<string> subtype;
                }
            }
        }
        */

        internal class SystemQuery : Query
        {
            public List<Sort> sort;

            internal class Sort : Dictionary<string, Direction>
            {
                public Sort(string name, SortOrder dir)
                {
                    this.Add(name, dir);
                }
            }

            internal class Direction
            {
                public SortOrder direction;

                public Direction(SortOrder dir)
                {
                    this.direction = dir;
                }

                public static implicit operator Direction(SortOrder dir)
                {
                    return new Direction(dir);
                }
            }
        }

        #endregion

        #region Respose

        internal class SystemResponse
        {
            /// <summary> Total row count </summary>
            public int count;
            /// <summary> Page starts from this row </summary>
            public int from;
            public JObject /* SystemQuery */ search; // TODO: write custom JSON deserializer that can make this be strongly typed
            public string search_reference;
            /// <summary> Size of this page </summary>
            public int size;
            public List<Result> results;

            public class Result : StarRef
            {
                public string id;
                public double distance;

                // public int body_count; // really?
                public long population;
                public string government;
                public string region;
                public string primary_economy;
                public string secondary_economy;
                public string security;
                public DateTimeOffset updated_at;

                // TODO: more properties?
                public long estimated_mapping_value;
                public long estimated_scan_value;
                public long landmark_value;

                public List<Body>? bodies;

                public List<MinorFactionPresence> minor_faction_presences;

                public override string ToString()
                {
                    return $"{name} [{x}, {y}, {z}]";
                }

                public class Body
                {
                    public double distance_to_arrival;
                    public long estimated_mapping_value;
                    public long estimated_scan_value;
                    public long id;
                    public long id64;
                    public bool? is_main_star;
                    public string name;
                    public string subtype;
                    public string type;
                }
            }

            public override string ToString()
            {
                return $"(count:{results.Count}, size:{size}, from:{from})";
            }
        }

        #endregion
    }
}
