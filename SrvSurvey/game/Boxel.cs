using Newtonsoft.Json;
using SrvSurvey.units;
using System.Text;
using System.Text.RegularExpressions;

namespace SrvSurvey.game
{
    // See: https://web.archive.org/web/20240303181408/http://disc.thargoid.space/Sector_Naming (http://disc.thargoid.space/Sector_Naming)
    // See: https://web.archive.org/web/20220618134655/http://disc.thargoid.space/ID64 (http://disc.thargoid.space/ID64)
    // See https://forums.frontier.co.uk/threads/marxs-guide-to-boxels-subsectors.618286/

    /// <summary>
    /// Represents a Boxel
    /// </summary>
    [JsonConverter(typeof(BoxelJsonConverter))]
    internal partial class Boxel
    {
        #region parsing and other statics

        private static Regex nameParts = new Regex(@"(.+) (\w\w-\w) (\w)(\d+)-?(\d+)?$", RegexOptions.Singleline | RegexOptions.Compiled);

        public static bool isValid(string? name)
        {
            var bx = parse(name);
            return bx != null;
        }

        /// <summary>
        /// Parses a boxel from a generated system name, or returns null if name is not valid.
        /// </summary>
        public static Boxel? parse(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var parts = nameParts.Match(name);

            // not a match?
            if (parts.Groups.Count != 5 && parts.Groups.Count != 6) return null;

            // confirm mass code is valid
            var mc = parts.Groups[3].Value[0];
            if (!MassCode.valid(mc)) return null;

            // confirm sector is valid
            var sectorName = parts.Groups[1].Value;
            if (!Sectors.is_valid_sector_name(sectorName)) return null;

            var bx = new Boxel
            {
                sector = sectorName,
                letters = parts.Groups[2].Value,
                massCode = mc,
                n1 = int.Parse(parts.Groups[4].Value),
            };

            if (parts.Groups.Count == 6 && parts.Groups[5].Success)
                bx.n2 = int.Parse(parts.Groups[5].Value);

            return bx;
        }

        /// <summary>
        /// Parses a boxel from an system address / id64 value, or returns null if name is not valid.
        /// </summary>
        public static Boxel? parse(long id64)
        {
            var id = id64;

            // extract mass code, then shift 3 bits
            var mc = (int)takeBits(id, 3);
            id = id >> 3; // and shift

            // count of bits to take, dependent on the mass code
            var ss = 7 - mc;

            // z boxel
            var zb = takeBits(id, ss);
            id = id >> (int)ss; // and shift

            // z sector
            var zs = takeBits(id, 7);
            id = id >> 7; // and shift

            // y boxel
            var yb = takeBits(id, ss);
            id = id >> (int)ss; // and shift

            // y sector
            var ys = takeBits(id, 6);
            id = id >> 6; // and shift

            // y boxel
            var xb = takeBits(id, ss);
            id = id >> (int)ss; // and shift

            // y sector
            var xs = takeBits(id, 7);
            id = id >> 7; // and shift

            // n2 value
            var n2 = (int)takeBits(id, 10);
            id = id >> 10; // and shift

            var abs_pos = new Point3((int)xs, (int)ys, (int)zs);
            var sectorName = Sectors.get_sector_name(abs_pos, false);
            if (string.IsNullOrEmpty(sectorName))
                throw new Exception($"Cannot generate sectorName from: {id64}");

            var mc2 = (char)('a' + (int)(id64 & 7));
            var rel_pos = new Point3((int)xb, (int)yb, (int)zb);

            var bx = Boxel.from(sectorName, rel_pos, mc2, n2);
            return bx;
        }

        public static string toBin(long id)
        {
            var n = 63;
            var t = new StringBuilder("".PadLeft(64, '0'));

            while (id > 0)
            {
                var b = id & 0x1;
                if (b == 0)
                    t[n] = '0';
                else
                    t[n] = '1';

                id >>= 1;
                n--;
            }
            return t.ToString();
        }

        /// <summary>
        /// Take count number of bits from the right edge of the number
        /// </summary>
        private static long takeBits(long num, long count)
        {
            if (count == 0) return 0;

            long r = 0;
            int n = 0;

            while (n < count)
            {
                var b = num & 0x1;
                if (b == 1)
                {
                    var nn = 1 << n;
                    r += nn;
                }

                num >>= 1;
                n++;
            }

            return r;
        }

        /// <summary>
        /// Creates a boxel from the given sector, co-ordinates and mass code
        /// </summary>
        private static Boxel from(string sector, Point3 pt, MassCode mc, int n2 = 0)
        {
            var idParts = getIdParts(pt);
            return new Boxel
            {
                sector = sector,
                letters = idParts.Item1,
                massCode = mc,
                n1 = idParts.Item2, // remainder
                n2 = n2,
            };
        }

        /// <summary> Convert a 3d point into the 3 letter code </summary>
        private static Tuple<string, int> getIdParts(Point3 pt)
        {
            // get n from coords
            var q = pt.x + (pt.y * 128) + (pt.z * 16384);

            // decode n as a base-26 number
            var a = q % 26;
            q = (q - a) / 26;

            var b = q % 26;
            q = (q - b) / 26;

            var c = q % 26;
            q = (q - c) / 26;

            var id = $"{(char)(a + 'A')}{(char)(b + 'A')}-{(char)(c + 'A')}";

            return new Tuple<string, int>(id, q);
        }

        /// <summary> Auto cast to a basic string </summary>
        public static implicit operator string?(Boxel b)
        {
            return b?.name;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            var bx = obj as Boxel;
            return this.GetHashCode() == bx?.GetHashCode();
        }

        public static bool operator ==(Boxel? bx1, Boxel? bx2)
        {
            return bx1?.GetHashCode() == bx2?.GetHashCode();
        }

        public static bool operator !=(Boxel? bx1, Boxel? bx2)
        {
            return bx1?.GetHashCode() != bx2?.GetHashCode();
        }

        #endregion

        /// <summary> The initial sector, eg: 'Thuechu' from 'Thuechu YV-T d4-12' </summary>
        public string sector;
        /// <summary> The letters of the id portion, eg: 'YV-T' from 'Thuechu YV-T d4-12' </summary>
        public string letters;
        /// <summary> The mass code portion, eg: 'd' from 'Thuechu YV-T d4-12' </summary>
        public MassCode massCode;
        /// <summary> The initial number portion, eg: '4' from 'Thuechu YV-T d4-12' or '0' from 'Thuechu LM-T c12' </summary>
        public int n1;
        /// <summary> The final number portion, eg: '12' from 'Thuechu YV-T d4-12' or 'Thuechu LM-T c12' </summary>
        public int n2;

        // Boxel.parse should be used to construct instances
        private Boxel() { }

        /// <summary> The id part of the name - without the sector or trailing n2, eg: 'YV-T d4' from 'Thuechu YV-T d4-12'  </summary>
        public string id => n1 == 0 ? $"{letters} {massCode}" : $"{letters} {massCode}{n1}";

        public string name { get => this.ToString(); }

        /// <summary> The name without the trailing system number </summary>
        public string prefix
        {
            get => n1 == 0
                    ? $"{sector} {letters} {massCode}"
                    : $"{sector} {letters} {massCode}{n1}-";
        }

        public override string ToString()
        {
            return $"{prefix}{n2}";
        }

        /// <summary>
        /// Get the relative boxel co-ordinates within the sector
        /// </summary>
        public Point3 getRelativeCoords()
        {
            // calc single ID number for boxel
            var i1 = letters[0] - 'A';
            var i2 = letters[1] - 'A';
            var i3 = letters[3] - 'A';
            int q = i1 + (i2 * 26) + (i3 * 676) + (n1 * 17576);

            // 128 * 128 = 16384
            var pt = new Point3
            {
                x = q % 128,
                y = (q % 16384) / 128,
                z = q / 16384,
            };

            return pt;
        }

        public Boxel parent
        {
            // make mass code one larger, and halve the co-ordinates
            get => Boxel.from(this.sector, this.getRelativeCoords() / 2, massCode + 1);
        }

        private static Point3[] childDeltas = new Point3[]
        {
            new Point3(0, 0, 0),
            new Point3(1, 0, 0),
            new Point3(0, 1, 0),
            new Point3(1, 1, 0),

            new Point3(0, 0, 1),
            new Point3(1, 0, 1),
            new Point3(0, 1, 1),
            new Point3(1, 1, 1),
        };

        public List<Boxel> children
        {
            get
            {
                var list = new List<Boxel>();
                if (this.massCode > 'a')
                {
                    // make mass code one smaller + double the co-ordinates ...
                    var mc = this.massCode - 1;
                    var pt = this.getRelativeCoords() * 2;

                    // ... then make 8 of them - 2 sets of 4 quadrants
                    foreach (var d in childDeltas)
                        list.Add(Boxel.from(this.sector, pt + d, mc));
                }

                return list;
            }
        }

        /// <summary>
        /// Returns a count of boxels contained by this one, including itself.
        /// </summary>
        public static int getTotalChildCount(int diff)
        {
            // total count of boxels
            var n = diff;
            var total = 0;
            while (n >= 0)
            {
                total += (int)Math.Pow(8, n);
                n--;
            }

            return total;
        }

        /// <summary>
        /// Returns a new Boxel instance like this one, but using the new n2 value
        /// </summary>
        public Boxel to(int newN2)
        {
            return new Boxel
            {
                sector = sector,
                letters = letters,
                massCode = massCode,
                n1 = n1,
                n2 = newN2,
            };
        }

        /// <summary>
        /// Returns true if we contain the given boxel
        /// </summary>
        public bool containsChild(Boxel? child)
        {
            if (child == null) return false;

            // is this the exact same boxel?
            if (child.prefix == this.prefix) return true;

            // sibling or bigger cannot be contained by smaller
            if (child.massCode >= this.massCode) return false;

            // calc and adjust our coords to the same scale as the child
            var d = this.massCode - child.massCode;
            var span = (int)Math.Pow(2, d);
            var coords = this.getRelativeCoords() * span;

            // are child coords are within the space of this boxel?
            var pt = child.getRelativeCoords();
            return coords.contains(span, pt);
        }

        /// <summary>
        /// Generate the systemAddress for this boxel.
        /// </summary>
        public long getAddress()
        {
            var bodyId = 0;

            var rel_pos = this.getRelativeCoords();
            var abs_pos = Sectors.getSectorCoords(this.sector);

            var mc = (int)this.massCode - 'a';
            int rs = 'h' - this.massCode; // relative shift value, dependent on the mass code

            long address = 0;
            address = Sectors.pack_and_shift(address, bodyId, 9);
            address = Sectors.pack_and_shift(address, this.n2, 11 + mc * 3);

            address = Sectors.pack_and_shift(address, abs_pos.x, 7);
            address = Sectors.pack_and_shift(address, rel_pos.x, rs);

            address = Sectors.pack_and_shift(address, abs_pos.y, 6);
            address = Sectors.pack_and_shift(address, rel_pos.y, rs);

            address = Sectors.pack_and_shift(address, abs_pos.z, 7);
            address = Sectors.pack_and_shift(address, rel_pos.z, rs);

            address = Sectors.pack_and_shift(address, mc, 3);

            return address;
        }
    }

    /// <summary>
    /// Represents the mass code of a boxel - characters 'a' through 'h'
    /// </summary>
    internal struct MassCode
    {
        public static bool valid(char c)
        {
            return c >= 'a' && c <= 'h';
        }

        private char c;

        public MassCode(char c)
        {
            if (!valid(c)) throw new Exception($"Bad mass code: {c}");

            this.c = c;
        }

        public override string ToString()
        {
            return this.c.ToString();
        }

        public static MassCode operator +(MassCode mc, int n)
        {
            var next = mc.c + n;
            return new MassCode((char)next);
        }

        public static MassCode operator -(MassCode mc, int n)
        {
            var next = mc.c - n;
            return new MassCode((char)next);
        }

        public static int operator -(MassCode m1, MassCode m2)
        {
            return (int)m1.c - (int)m2.c;
        }

        public static implicit operator char(MassCode mc)
        {
            return mc.c;
        }

        public static implicit operator MassCode(char c)
        {
            return new MassCode(c);
        }
    }

    /// <summary>
    /// JSON serializes Boxel's as a basic string, parsing them back into a Boxel upon deserialization
    /// </summary>
    class BoxelJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var txt = serializer.Deserialize<string>(reader);
            if (string.IsNullOrEmpty(txt)) throw new Exception($"Unexpected value: {txt}");

            return Boxel.parse(txt);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var boxel = value as Boxel;
            if (boxel == null) throw new Exception($"Unexpected type: {value?.GetType().Name}");

            writer.WriteValue(boxel.name);
        }
    }

}
