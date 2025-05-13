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
        public async Task<SystemsResponse> querySystems(SearchQuery query)
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

            var obj = JsonConvert.DeserializeObject<SystemsResponse>(json)!;
            return obj;
        }

        public async Task<StationsResponse> queryStations(SearchQuery query)
        {
            var queryJson = JsonConvert.SerializeObject(query, Formatting.None);
            Game.log($"Spansh.querySystems:\r\n{queryJson}");

            var body = new StringContent(queryJson, Encoding.ASCII, "application/json");
            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/stations/search", body);
            var json = await response.Content.ReadAsStringAsync();

            if (json == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + json);
            }

            var obj = JsonConvert.DeserializeObject<StationsResponse>(json)!;
            return obj;
        }

        #region Query

        internal class Query
        {
            public int size = 20;
            public int page = 0;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public StarRef? reference_coords;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            public string? reference_system;

            public Dictionary<string, IFilter> filters;

            internal abstract class Filter: IFilter { }

            internal interface IFilter { }

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

            internal class MinMax : Filter
            {
                // "distance":{"min":"0","max":"10"},
                public string min;
                public string max;

                public MinMax() { }
                public MinMax(string min, string max) { this.min = min; this.max = max; }
            }

            internal class Comparison : Filter
            {
                // "population":{"comparison":"<=>","value":[0, 0]
                public string comparison = "<=>";
                [JsonIgnore]
                public int min;
                [JsonIgnore]
                public int max;
                public int[] value => new int[] { min, max };

                public Comparison() { }
                public Comparison(int min, int max) { this.min = min; this.max = max; }
            }

            internal class Markets : List<Market>, IFilter { }

            internal class Market : IFilter
            {
                public string name;
                public Clause buy_price;
                public Clause sell_price;
                public Clause supply;
                public Clause demand;

                public Market() { }

                public class Clause
                {
                    [JsonIgnore]
                    public long min = 0;
                    [JsonIgnore]
                    public long max = 0;
                    public long[] value => new long[2] { min, max };

                    public string comparison = "<=>";

                    public Clause(long min, long max)
                    {
                        this.min = min;
                        this.max = max;
                    }
                }
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

        internal class SearchQuery : Query
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

        internal class SearchResponse<T>
        {
            /// <summary> Total row count </summary>
            public int count;
            /// <summary> Page starts from this row </summary>
            public int from;
            public StarRef reference;
            public JObject /* SystemQuery */ search; // TODO: write custom JSON deserializer that can make this be strongly typed
            public string search_reference;
            /// <summary> Size of this page </summary>
            public int size;
            public List<T> results;

            public override string ToString()
            {
                return $"(count:{results.Count}, size:{size}, from:{from})";
            }
        }

        internal class SystemsResponse : SearchResponse<SystemsResponse.Result>
        {
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
        }

        internal class StationsResponse : SearchResponse<StationsResponse.Result>
        {
            public class Result
            {
                public string id;
                public string name;
                public string market_id;
                public string type;
                public bool is_planetary;

                public string? controlling_minor_faction;
                public double distance;
                public double distance_to_arrival;

                public string system_name;
                public long system_id64;
                public string system_primary_economy;
                public string system_secondary_economy;
                public double system_x;
                public double system_y;
                public double system_z;
                public bool system_is_colonised;
                // system_power ??

                public List<MarketEntry> market;

                public string primary_economy;
                public string secondary_economy;
                public List<Economy> economies;
                // export_commodities ??
                // import_commodities ??


                public List<Ship> ships;
                public List<Module> modules; 
                public List<Service> services; 

                public DateTimeOffset market_updated_at;
                public DateTimeOffset outfitting_updated_at;
                public DateTimeOffset shipyard_updated_at;
                public DateTimeOffset updated_at;

                public string government;

                public bool has_large_pad;
                public bool has_market;
                public bool has_outfitting;
                public bool has_shipyard;

                public int large_pads;
                public int medium_pads;
                public int small_pads;


                public override string ToString()
                {
                    return $"{name} ({market_id}/{type})";
                }

                public class Economy
                {
                    public string name;
                    public float share;
                }

                public class MarketEntry
                {
                    public string commodity;
                    public string category;
                    public int buy_price;
                    public int demand;
                    public int sell_price;
                    public int supply;
                }

                public class Module
                {
                    public string name;
                    public string category;
                    public int @class;
                    public string ed_symbol;
                    public string rating;
                    public string ship;
                }

                public class Service
                {
                    public string name;
                }

                public class Ship
                {
                    public string name;
                    public int price;
                    public string symbol;
                }


            }

        }

        #endregion
    }
}
