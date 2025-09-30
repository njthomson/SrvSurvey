using SrvSurvey.units;
using System.Diagnostics;

namespace SrvSurvey.game
{
    partial class Boxel
    {
        /*
         * Most code here was ported from https://bitbucket.org/Esvandiary/edts/src with kind permission from Cmdr Alot.
         * I have kept Python's naming convention to make it easier to compare this code with the original.
         * 
         * Remaining/TODO:
         *  - Handle hand-authored sectors + include list of known sectors
         *  - Implement class Sector/HASector/PGSector - not sure it is needed within SrvSurvey?
         */

        static class Sectors
        {
            private static string[] cx_fragments;
            private static Dictionary<string, int[]> _prefix_offsets;
            private static Dictionary<string, int[]> _c1_infix_offsets;
            private static int cx_prefix_total_run_length;
            private static int c1_infix_s1_length_default;
            private static int c1_infix_s2_length_default;
            private static int c1_infix_s1_total_run_length;
            private static int c1_infix_s2_total_run_length;

            static Sectors()
            {
                // Sort fragments by length to ensure we check the longest ones first
                cx_fragments = cx_raw_fragments
                    .OrderByDescending(f => f.Length)
                    .ToArray();

                // Get the total length of one run over all prefixes
                cx_prefix_total_run_length = cx_prefixes.Select(p => _get_prefix_run_length(p)).Sum();

                // Default infix run lengths
                c1_infix_s1_length_default = c1_suffixes_s2.Length;
                c1_infix_s2_length_default = cx_suffixes_s1.Length;

                // Total lengths of runs over all infixes, for each sequence
                c1_infix_s1_total_run_length = c1_infixes_s1.Sum(p => c1_infix_length_overrides.Get(p, c1_infix_s1_length_default));
                c1_infix_s2_total_run_length = c1_infixes_s2.Sum(p => c1_infix_length_overrides.Get(p, c1_infix_s2_length_default));

                // Cache the run offsets of all prefixes and C1 infixes
                _prefix_offsets = new();
                _c1_infix_offsets = new();
                int cnt = 0;
                foreach (var p in cx_prefixes)
                {
                    int plen = _get_prefix_run_length(p);
                    _prefix_offsets[p] = new int[] { cnt, plen };
                    cnt += plen;
                }

                cnt = 0;
                foreach (var i in c1_infixes_s1)
                {
                    int ilen = _c1_get_infix_run_length(i);
                    _c1_infix_offsets[i] = new int[] { cnt, ilen };
                    cnt += ilen;
                }

                cnt = 0;
                foreach (var i in c1_infixes_s2)
                {
                    int ilen = _c1_get_infix_run_length(i);
                    _c1_infix_offsets[i] = new int[] { cnt, ilen };
                    cnt += ilen;
                }
            }

            /// <summary>
            /// Returns the Sector co-ordinates from a sector name
            /// </summary>
            public static Point3? getSectorCoords(string sectorName, char mcode)
            {
                /* Not yet working ...
                // is this a hand-authored sector?
                var ha = ha_regions.Find(r => r.name.like(sectorName));
                if (ha != null)
                    return ha.get_origin(mcode);
                // */

                var offset = get_offset_from_name(sectorName);
                if (offset == -1) return null;

                var x = (offset % galaxy_size[0]);
                var y = (offset / galaxy_size[0]) % galaxy_size[1];
                var z = (offset / (galaxy_size[0] * galaxy_size[1]));

                return new Point3((int)x, (int)y, (int)z);
            }

            /// <summary>
            /// Returns the Sector offset from a sector name
            /// </summary>
            private static long get_offset_from_name(string sectorName)
            {
                var frags = Sectors.get_sector_fragments(sectorName)!;
                if (frags == null) return -1;

                var sc = _get_sector_class(frags);
                if (sc == 2)
                    return Sectors._c2_get_offset_from_name(frags);
                else
                    return Sectors._c1_get_offset_from_name(frags);
            }

            #region pgdata.py
            // From: https://bitbucket.org/Esvandiary/edts/src/develop/edtslib/pgdata.py

            /// <summary> Hopefully-complete list of valid name fragments / phonemes </summary>
            private static string[] cx_raw_fragments = new string[]
            {
                "Th", "Eo", "Oo", "Eu", "Tr", "Sly", "Dry", "Ou",
                "Tz", "Phl", "Ae", "Sch", "Hyp", "Syst", "Ai", "Kyl",
                "Phr", "Eae", "Ph", "Fl", "Ao", "Scr", "Shr", "Fly",
                "Pl", "Fr", "Au", "Pry", "Pr", "Hyph", "Py", "Chr",
                "Phyl", "Tyr", "Bl", "Cry", "Gl", "Br", "Gr", "By",
                "Aae", "Myc", "Gyr", "Ly", "Myl", "Lych", "Myn", "Ch",
                "Myr", "Cl", "Rh", "Wh", "Pyr", "Cr", "Syn", "Str",
                "Syr", "Cy", "Wr", "Hy", "My", "Sty", "Sc", "Sph",
                "Spl", "A", "Sh", "B", "C", "D", "Sk", "Io",
                "Dr", "E", "Sl", "F", "Sm", "G", "H", "I",
                "Sp", "J", "Sq", "K", "L", "Pyth", "M", "St",
                "N", "O", "Ny", "Lyr", "P", "Sw", "Thr", "Lys",
                "Q", "R", "S", "T", "Ea", "U", "V", "W",
                "Schr", "X", "Ee", "Y", "Z", "Ei", "Oe",

                "ll", "ss", "b", "c", "d", "f", "dg", "g", "ng", "h", "j", "k", "l", "m", "n",
                "mb", "p", "q", "gn", "th", "r", "s", "t", "ch", "tch", "v", "w", "wh",
                "ck", "x", "y", "z", "ph", "sh", "ct", "wr", "o", "ai", "a", "oi", "ea",
                "ie", "u", "e", "ee", "oo", "ue", "i", "oa", "au", "ae", "oe", "scs",
                "wsy", "vsky", "sms", "dst", "rb", "nts", "rd", "rld", "lls", "rgh",
                "rg", "hm", "hn", "rk", "rl", "rm", "cs", "wyg", "rn", "hs", "rbs", "rp",
                "tts", "wn", "ms", "rr", "mt", "rs", "cy", "rt", "ws", "lch", "my", "ry",
                "nks", "nd", "sc", "nk", "sk", "nn", "ds", "sm", "sp", "ns", "nt", "dy",
                "st", "rrs", "xt", "nz", "sy", "xy", "rsch", "rphs", "sts", "sys", "sty",
                "tl", "tls", "rds", "nch", "rns", "ts", "wls", "rnt", "tt", "rdy", "rst",
                "pps", "tz", "sks", "ppy", "ff", "sps", "kh", "sky", "lts", "wnst", "rth",
                "ths", "fs", "pp", "ft", "ks", "pr", "ps", "pt", "fy", "rts", "ky",
                "rshch", "mly", "py", "bb", "nds", "wry", "zz", "nns", "ld", "lf",
                "gh", "lks", "sly", "lk", "rph", "ln", "bs", "rsts", "gs", "ls", "vvy",
                "lt", "rks", "qs", "rps", "gy", "wns", "lz", "nth", "phs", "io", "oea",
                "aa", "ua", "eia", "ooe", "iae", "oae", "ou", "uae", "ao", "eae", "aea",
                "ia", "eou", "aei", "uia", "aae", "eau"
            };

            /// <summary>  Order here is relevant, keep it </summary>
            private static string[] cx_prefixes = cx_raw_fragments.Take(111).ToArray();

            /// <summary> Vowel-ish infixes </summary>
            private static string[] c1_infixes_s1 = new string[]
            {
                "o", "ai", "a", "oi", "ea", "ie", "u", "e",
                "ee", "oo", "ue", "i", "oa", "au", "ae", "oe"
            };

            /// <summary> Consonant-ish infixes </summary>
            private static string[] c1_infixes_s2 = new string[]
            {
                "ll", "ss", "b", "c", "d", "f", "dg", "g",
                "ng", "h", "j", "k", "l", "m", "n", "mb",
                "p", "q", "gn", "th", "r", "s", "t", "ch",
                "tch", "v", "w", "wh", "ck", "x", "y", "z",
                "ph", "sh", "ct", "wr"
            };

            private static string[][] c1_infixes = new string[][]
            {
                new string[0],
                c1_infixes_s1,
                c1_infixes_s2,
            };

            /// <summary> Sequence 1 </summary>
            private static string[] cx_suffixes_s1 = new string[]
            {
                "oe",  "io",  "oea", "oi",  "aa",  "ua", "eia", "ae",
                "ooe", "oo",  "a",   "ue",  "ai",  "e",  "iae", "oae",
                "ou",  "uae", "i",   "ao",  "au",  "o",  "eae", "u",
                "aea", "ia",  "ie",  "eou", "aei", "ea", "uia", "oa",
                "aae", "eau", "ee"
            };

            /// <summary> Sequence 2 </summary>
            private static string[] c1_suffixes_s2 = new string[]
            {
                "b", "scs", "wsy", "c", "d", "vsky", "f", "sms",
                "dst", "g", "rb", "h", "nts", "ch", "rd", "rld",
                "k", "lls", "ck", "rgh", "l", "rg", "m", "n", 
                // Formerly sequence 4/5...
                "hm", "p", "hn", "rk", "q", "rl", "r", "rm",
                "s", "cs", "wyg", "rn", "ct", "t", "hs", "rbs",
                "rp", "tts", "v", "wn", "ms", "w", "rr", "mt",
                "x", "rs", "cy", "y", "rt", "z", "ws", "lch", // "y" is speculation
                "my", "ry", "nks", "nd", "sc", "ng", "sh", "nk",
                "sk", "nn", "ds", "sm", "sp", "ns", "nt", "dy",
                "ss", "st", "rrs", "xt", "nz", "sy", "xy", "rsch",
                "rphs", "sts", "sys", "sty", "th", "tl", "tls", "rds",
                "nch", "rns", "ts", "wls", "rnt", "tt", "rdy", "rst",
                "pps", "tz", "tch", "sks", "ppy", "ff", "sps", "kh",
                "sky", "ph", "lts", "wnst", "rth", "ths", "fs", "pp",
                "ft", "ks", "pr", "ps", "pt", "fy", "rts", "ky",
                "rshch", "mly", "py", "bb", "nds", "wry", "zz", "nns",
                "ld", "lf", "gh", "lks", "sly", "lk", "ll", "rph",
                "ln", "bs", "rsts", "gs", "ls", "vvy", "lt", "rks",
                "qs", "rps", "gy", "wns", "lz", "nth", "phs"
            };

            private static string[] c2_suffixes_s2 = c1_suffixes_s2
                .ToList()
                .GetRange(0, cx_suffixes_s1.Length)
                .ToArray();

            private static string[][] c1_suffixes = new string[][]
            {
                new string[0],
                cx_suffixes_s1,
                c1_suffixes_s2,
            };

            private static string[][] c2_suffixes = new string[][]
            {
                new string[0],
                cx_suffixes_s1,
                c2_suffixes_s2,
            };

            /// <summary> These prefixes use the specified index into the c2_suffixes list </summary>
            private static Dictionary<string, int> c2_prefix_suffix_override_map = new()
            {
              {"Eo",  2}, {"Oo", 2}, {"Eu", 2},
              {"Ou",  2}, {"Ae", 2}, {"Ai", 2},
              {"Eae", 2}, {"Ao", 2}, {"Au", 2},
              {"Aae", 2}
            };

            /// <summary> These prefixes use the specified index into the c1_infixes list </summary>
            private static Dictionary<string, int> c1_prefix_infix_override_map = new()
            {
                {"Eo", 2}, {"Oo",  2}, {"Eu",  2}, {"Ou", 2},
                {"Ae", 2}, {"Ai",  2}, {"Eae", 2}, {"Ao", 2},
                {"Au", 2}, {"Aae", 2}, {"A",   2}, {"Io", 2},
                {"E",  2}, {"I",   2}, {"O",   2}, {"Ea", 2},
                {"U",  2}, {"Ee",  2}, {"Ei",  2}, {"Oe", 2}
            };

            // The default run length for most prefixes
            private static int cx_prefix_length_default = 35;

            // Some prefixes use short run lengths; specify them here
            private static Dictionary<string, int> cx_prefix_length_overrides = new()
            {
              { "Eu",  31}, { "Sly",  4}, {  "Tz",  1}, { "Phl", 13},
              { "Ae",  12}, { "Hyp", 25}, { "Kyl", 30}, { "Phr", 10},
              { "Eae",  4}, {  "Ao",  5}, { "Scr", 24}, { "Shr", 11},
              { "Fly", 20}, { "Pry",  3}, {"Hyph", 14}, {  "Py", 12},
              { "Phyl", 8}, { "Tyr", 25}, { "Cry",  5}, { "Aae",  5},
              { "Myc",  2}, { "Gyr", 10}, { "Myl", 12}, {"Lych",  3},
              { "Myn", 10}, { "Myr",  4}, {  "Rh", 15}, {  "Wr", 31},
              { "Sty",  4}, { "Spl", 16}, {  "Sk", 27}, {  "Sq",  7},
              { "Pyth", 1}, { "Lyr", 10}, {  "Sw", 24}, { "Thr", 32},
              { "Lys", 10}, {"Schr",  3}, {   "Z", 34},
            };

            private static Dictionary<string, int> c1_infix_length_overrides = new()
            {
                // Sequence 1
                { "oi",  88 }, { "ue", 147 }, { "oa",  57 },
                { "au", 119 }, { "ae",  12 }, { "oe",  39 },
                // Sequence 2
                { "dg",  31 }, { "tch", 20 }, { "wr",  31 },
            };

            // TODO: This doesn't work properly yet
            public static List<HARegion> ha_regions = new()
            {
                new HARegion("Trianguli Sector", 50.0, new Sphere(60.85156, -47.94922, -81.32031, 50.0)),
                new HARegion("IC 2391 Sector", 100.0, new Sphere(565.85938, -68.47656, 3.95117, 100.0)),
                // TODO: add the others...
            };

            #endregion

            #region pgnames.py
            // From: https://bitbucket.org/Esvandiary/edts/src/develop/edtslib/pgnames.py

            /// <summary>
            /// Returns the name of a sector from a position
            /// </summary>
            public static string? get_sector_name(Point3 pos, bool allow_ha = true)
            {
                /*
                 * Get the name of a sector that a position falls within.
                 *
                 * Args:
                 *   pos: A position
                 *   format_output: Whether or not to format the output or return it as fragments
                 * Returns:
                 *   The name of the sector which contains the input position, either as a string or as a list of fragments
                 */

                if (allow_ha)
                {
                    throw new NotImplementedException();
                    //string ha_name = _ha_get_name(pos);
                    //if (ha_name != null)
                    //    return ha_name;
                }

                int offset = _c1_get_offset(pos);
                string? output;
                if (_get_c1_or_c2(offset) == 1)
                    output = _c1_get_name(pos);
                else
                    output = _c2_get_name(pos);

                // output will contain pre-formatted names
                return output;
            }

            /// <summary>
            ///   Get a list of fragments from an input sector name
            ///   e.g. "Dryau Aowsy" --> ["Dry", "au", "Ao", "wsy"]
            /// </summary>
            private static List<string>? get_sector_fragments(string sectorName)
            {
                var fragments = new List<string>();
                var name = Util.pascalAllWords(sectorName).Replace(" ", "");

                while (!string.IsNullOrWhiteSpace(name))
                {
                    var match = cx_fragments.FirstOrDefault(p => name.StartsWith(p));
                    if (match == null)
                    {
                        Game.log($"Sector fragment not matched: {name}");
                        return null;
                    }

                    fragments.Add(match);
                    name = name.Substring(match.Length);
                }

                return fragments;
            }

            /// <summary>
            ///   Checks whether or not the provided sector name is a valid PG name
            ///   
            ///   Mild weakness: due to the way get_sector_fragments works, this currently ignores all spaces
            ///   This means that names like "Synoo kio" are considered valid
            /// </summary>
            public static bool is_valid_sector_name(string? sectorName)
            {
                return !(string.IsNullOrEmpty(sectorName) || get_sector_fragments(sectorName) == null);
            }

            private static string? format_sector_name(List<string>? fragments)
            {
                /*  Format a given set of fragments into a full name
                 *  
                 *  Args:
                 *      input: A list of sector name fragments
                 *  Returns:
                 *      The sector name as a string
                 */
                if (fragments == null) return null!;

                if (fragments.Count == 4 && cx_prefixes.Contains(fragments[2]))
                    return $"{fragments[0]}{fragments[1]} {fragments[2]}{fragments[3]}";
                else
                    return string.Join("", fragments);
            }

            /// <summary>
            /// Get the class of the sector from its name
            /// e.g. Froawns = 1, Froadue = 1, Eos Aowsy = 2
            /// </summary>
            private static int _get_sector_class(List<string> frags)
            {
                // TODO: HA

                if (frags.Count == 4 && cx_prefixes.Contains(frags[0]) && cx_prefixes.Contains(frags[2]))
                    return 2;
                else if ((frags.Count == 3 || frags.Count == 4) && cx_prefixes.Contains(frags[0]))
                    return 1;
                else
                    return 0;
            }

            private static List<string> _get_suffixes(string sectorName, bool getAll = false)
            {
                return _get_suffixes(get_sector_fragments(sectorName)!, getAll);
            }

            private static List<string> _get_suffixes(List<string> frags, bool get_all = false)
            {
                string wordStart = frags[0];
                string[] result;

                if (cx_prefixes.Contains(frags[^1]))
                {
                    // Append suffix straight onto a prefix (probably C2)
                    int suffix_map_idx = c2_prefix_suffix_override_map.ContainsKey(frags[^1])
                        ? c2_prefix_suffix_override_map[frags[^1]]
                        : 1;
                    result = c2_suffixes[suffix_map_idx];
                    wordStart = frags[^1];
                }
                else
                {
                    // Likely C1
                    if (c1_infixes[2].Contains(frags[^1]))
                    {
                        // Last infix is consonant-ish, return the vowel-ish suffix list
                        result = c1_suffixes[1];
                    }
                    else
                    {
                        result = c1_suffixes[2];
                    }
                }

                if (get_all)
                {
                    return result.ToList();
                }
                else
                {
                    int prefix_run_length = _get_prefix_run_length(wordStart);
                    return result.ToList().GetRange(0, prefix_run_length);
                }
            }

            /// <summary>
            /// Get the specified prefix's run length (e.g. Th => 35, Tz => 1)
            /// </summary>
            private static int _get_prefix_run_length(string prefix)
            {
                var len = cx_prefix_length_overrides.ContainsKey(prefix) ? cx_prefix_length_overrides[prefix] : cx_prefix_length_default;
                return len;
            }

            private static string _get_entry_from_offset(int offset, IEnumerable<string> keys, Dictionary<string, int[]> data)
            {
                var txt = keys.FirstOrDefault(c => offset >= data[c][0] && offset < (data[c][0] + data[c][1]));
                return txt!;
            }

            /// <summary>
            /// Get the sector offset of a position
            /// </summary>
            private static int _get_offset_from_pos(Point3 pos, int[] galSize)
            {
                // Get the sector offset of a position
                var offset = pos.z * galSize[1] * galSize[0];
                offset += pos.y * galSize[0];
                offset += pos.x;
                return offset;
            }

            /* TODO: needed?
            private static Point3 _get_sector_pos_from_offset(int offset, int[] galSize)
            {
                var x = (offset % galSize[0]);
                var y = (offset / galSize[0]) % galSize[1];
                var z = (offset / (galSize[0] * galSize[1]));
                if (z >= galaxy_size[2])
                    Game.log($"Sector position for offset {offset} is outside expected galaxy size!");

                // Put it in "our" coordinate space
                x -= base_sector_index[0];
                y -= base_sector_index[1];
                z -= base_sector_index[2];

                return new Point3(x, y, z);
            }
            */

            private static int _get_c1_or_c2(int key)
            {
                // Use Jenkins hash
                int hash = Jenkins32((uint)key);
                // Key is now an even/odd number, depending on which scheme we use
                // Return 1 for a class 1 sector, 2 for a class 2
                return (hash % 2) + 1;
            }

            /* TODO: needed?
            private static PGSector? _get_sector_from_name(string sector_name, bool allow_ha = true)
            {
                //sector_name = get_canonical_name(sector_name, sector_only: true);
                //if (sector_name == null)                    return null;

                if (false && allow_ha && ha_regions.ContainsKey(sector_name.ToLower()))
                {
                    return null; //  ha_regions[sector_name.ToLower()];
                }
                else
                {
                    var frags = util.IsString(sector_name) ? get_sector_fragments(sector_name) : (List<string>)sector_name;
                    if (frags != null)
                    {
                        int sc = _get_sector_class(frags);
                        if (sc == 2)
                        {
                            // Class 2
                            return _c2_get_sector(frags);
                        }
                        else if (sc == 1)
                        {
                            // Class 1
                            return _c1_get_sector(frags);
                        }
                        else
                        {
                            return null!;
                        }
                    }
                    else
                    {
                        return null!;
                    }
                }
            }
            */

            /* TODO: needed?
            public static Point3? _get_coords_from_name(string raw_system_name, bool allow_ha = true)
            {
                var bx = Boxel.parse(raw_system_name)!;
                var sector_name = bx.sector;
                var sect = _get_sector_from_name(sector_name, allow_ha);
                if (sect == null)
                    return null;

                // Get the absolute position of the sector
                var abs_pos = sect.get_origin(get_mcode_cube_width(bx.massCode));

                // Get the relative position of the star within the sector
                // Also get the +/- error bounds
                var rel_pos = bx.getRelativeCoords();// _get_relpos_from_sysid(m["L1"], m["L2"], m["L3"], m["MCode"], m["N1"], m["N2"]);

                //// Check if the relpos is invalid
                //var leeway = (sect.sector_class == "ha") ? rel_pos_error : 0;
                //if (rel_pos.Any(s => s > (sector_size + leeway)))
                //{
                //    Game.log($"RelPos for input {bx.name} was invalid: {rel_pos}, uncertainty {rel_pos_error}");
                //    return (null, null);
                //}

                if (abs_pos != null && rel_pos != null)
                    return abs_pos + rel_pos;
                else
                    return null;
            }
            */

            /// <summary>
            /// # Get the full list of infixes for a given set of fragments missing an infix
            /// # e.g. "Ogai", "Wre", "P"
            /// </summary>
            private static string[] _c1_get_infixes(List<string> frags)
            {
                if (cx_prefixes.Contains(frags[^1]))
                {
                    if (c1_prefix_infix_override_map.ContainsKey(frags[^1]))
                        return c1_infixes[c1_prefix_infix_override_map[frags[^1]]];
                    else
                        return c1_infixes[1];
                }
                else if (c1_infixes[1].Contains(frags[^1]))
                    return c1_infixes[2];
                else if (c1_infixes[2].Contains(frags[^1]))
                    return c1_infixes[1];
                else
                    return new string[0];
            }

            /// <summary>
            ///  Get the specified infix's run length
            /// </summary>
            private static int _c1_get_infix_run_length(string frag)
            {
                var def_len = c1_infixes_s1.Contains(frag)
                    ? c1_infix_s1_length_default
                    : c1_infix_s2_length_default;

                var len = c1_infix_length_overrides.Get(frag, def_len);
                return len;
            }

            /// <summary>
            /// Get the total run length for the series of infixes the input is part of
            /// </summary>
            private static int _c1_get_infix_total_run_length(string frag)
            {
                if (c1_infixes_s1.Contains(frag))
                    return c1_infix_s1_total_run_length;
                else
                    return c1_infix_s2_total_run_length;
            }

            /// <summary>
            /// Get the zero-based offset (counting from bottom-left of the galaxy) of the input sector name/position
            /// </summary>
            private static int _c1_get_offset(Point3 pos)
            {
                return _get_offset_from_pos(pos, galaxy_size);
            }

            /* TODO: needed?
            /// <summary>
            /// Get the zero-based offset (counting from bottom-left of the galaxy) of the input sector name/position
            /// </summary>
            private static int _c1_get_offset(List<string> frags)
            {
                return _c1_get_offset_from_name(frags);
            }
            */

            /// <summary>
            /// Get the zero-based offset (counting from bottom-left of the galaxy) of the input sector name/position
            /// </summary>
            private static int _c1_get_offset_from_name(List<string> frags)
            {
                try
                {
                    var sufs = _get_suffixes(frags.GetRange(0, frags.Count - 1), true);

                    // STEP 1: Acquire the offset for suffix runs, and adjust it
                    int suf_offset = sufs.IndexOf(frags[^1]);
                    // Assume suffix is fragment 3 unless we override that
                    int f3_offset = suf_offset;

                    // Add the total length of all the infixes we've already passed over
                    if (frags.Count > 3)
                    {
                        // We have a 4-phoneme name, which means we have to handle adjusting our "coordinates"
                        // from individual suffix runs up to fragment3 runs and then to fragment2 runs

                        // Check which fragment3 run we're on, and jump us up by that many total run lengths if not the first
                        suf_offset += (sufs.IndexOf(frags[^1]) / _c1_get_infix_run_length(frags[2])) * _c1_get_infix_total_run_length(frags[2]);

                        // STEP 1.5: Take our current offset from "suffix space" to "fragment3 space"
                        // Divide by the current fragment3's run length
                        // Remember the offset that we're at on the current suffix-run
                        int f3_offset_mod;
                        (f3_offset, f3_offset_mod) = Divmod(suf_offset, _c1_get_infix_run_length(frags[2]));
                        // Multiply by the total run length for this series of fragment3s
                        f3_offset *= _c1_get_infix_total_run_length(frags[2]);
                        // Reapply the f3 offset from earlier
                        f3_offset += f3_offset_mod;
                        // Add the offset of the current fragment3, to give us our overall position in the f3-sequence
                        f3_offset += _c1_infix_offsets[frags[2]][0];
                    }

                    // STEP 2: Take our current offset from "fragment3 space" to "fragment2 space"
                    // Divide by the current fragment2's run length
                    // Remember the offset that we're at on the current f3-run
                    int f2_offset, f2_offset_mod;
                    (f2_offset, f2_offset_mod) = Divmod(f3_offset, _c1_get_infix_run_length(frags[1]));
                    // Multiply by the total run length for this series of fragment2s
                    f2_offset *= _c1_get_infix_total_run_length(frags[1]);
                    // Reapply the f2 offset from earlier
                    f2_offset += f2_offset_mod;
                    // Add the offset of the current fragment2, to give us our overall position in the f2-sequence
                    f2_offset += _c1_infix_offsets[frags[1]][0];

                    // Divide by the current prefix's run length, this is now how many iterations of the full 3037 we should have passed over
                    // Also remember the current offset's position within a prefix run
                    int offset, offset_mod;
                    (offset, offset_mod) = Divmod(f2_offset, _get_prefix_run_length(frags[0]));
                    // Now multiply by the total run length (3037) to get the actual offset of this run
                    offset *= cx_prefix_total_run_length;
                    // Add the infixes/suffix's position within this prefix's part of the overall prefix run
                    offset += offset_mod;
                    // Add the base position of this prefix within the run
                    offset += _prefix_offsets[frags[0]][0];

                    // Whew!
                    return offset;
                }
                catch
                {
                    // Either the prefix or suffix lookup failed, likely a dodgy name
                    Game.log("Failed to look up prefixes/suffixes in _c1_get_offset_from_name; bad sector name?");
                    Debugger.Break();
                    return 0;
                }
            }

            /* TODO: needed?
            private static PGSector _c1_get_sector(List<string> frags)
            {
                var offset = _c1_get_offset(frags);

                // Calculate the X/Y/Z positions from the offset
                var spos = _get_sector_pos_from_offset(offset, galaxy_size);
                var name = format_sector_name(frags);
                return new PGSector(spos, name, _get_sector_class(frags));
            }
            */

            private static string? _c1_get_name(Point3 pos)
            {
                int offset = _c1_get_offset(pos);

                // Get the current prefix run we're on, and keep the remaining offset
                int prefixCnt, curOffset;
                (prefixCnt, curOffset) = Divmod(offset, cx_prefix_total_run_length);

                // Work out which prefix we're currently within
                var prefix = _get_entry_from_offset(curOffset, _prefix_offsets.Keys, _prefix_offsets);

                // Put us in that prefix's space
                curOffset -= _prefix_offsets[prefix][0];

                // Work out which set of infix1s we should be using, and its total length
                var infix1s = _c1_get_infixes(new List<string> { prefix });
                int infix1TotalLen = _c1_get_infix_total_run_length(infix1s[0]);

                // Work out where we are in infix1 space, keep the remaining offset
                int infix1Cnt;
                (infix1Cnt, curOffset) = Divmod(prefixCnt * _get_prefix_run_length(prefix) + curOffset, infix1TotalLen);


                // Find which infix1 we're currently in
                var infix1 = _get_entry_from_offset(curOffset, infix1s, _c1_infix_offsets);

                // Put us in that infix1's space
                curOffset -= _c1_infix_offsets[infix1][0];

                // Work out which set of suffixes we're using
                int infix1RunLen = _c1_get_infix_run_length(infix1);
                var sufs = _get_suffixes(new List<string> { prefix, infix1 }, true);

                // Get the index of the next entry in that list, in infix1 space
                int nextIdx = (infix1RunLen * infix1Cnt) + curOffset;


                // Start creating our output
                var frags = new List<string> { prefix, infix1 };

                // If the index of the next entry is longer than the list of suffixes...
                if (nextIdx >= sufs.Count)
                {
                    // Work out which set of infix2s we should be using
                    var infix2s = _c1_get_infixes(frags);
                    int infix2TotalLen = _c1_get_infix_total_run_length(infix2s[0]);

                    // Work out where we are in infix2 space, still keep the remaining offset
                    int infix2Cnt;
                    (infix2Cnt, curOffset) = Divmod(infix1Cnt * _c1_get_infix_run_length(infix1) + curOffset, infix2TotalLen);

                    // Find which infix2 we're currently in
                    var infix2 = _get_entry_from_offset(curOffset, infix2s, _c1_infix_offsets);

                    // Put us in this infix2's space
                    curOffset -= _c1_infix_offsets[infix2][0];

                    // Recalculate the next system index based on the infix2 data
                    int infix2RunLen = _c1_get_infix_run_length(infix2);
                    sufs = _get_suffixes(new List<string> { prefix, infix1, infix2 }, true); nextIdx = (infix2RunLen * infix2Cnt) + curOffset;

                    // Add our infix2 to the output
                    frags.Add(infix2);
                }

                // Add our suffix to the output, and return it
                frags.Add(sufs[nextIdx]);
                return format_sector_name(frags);
            }

            private static string? _c2_get_name(Point3 pos)
            {
                var offset = _get_offset_from_pos(pos, galaxy_size);
                return _c2_get_name_from_offset(offset);
            }

            /* TODO: needed?
            private static PGSector _c2_get_sector(List<string> frags)
            {
                var offset = _c2_get_offset_from_name(frags);

                // Calculate the X / Y / Z positions from the offset
                var spos = _get_sector_pos_from_offset(offset, galaxy_size);
                var name = format_sector_name(frags);
                return new PGSector(spos, name, _get_sector_class(frags));
            }
            */

            private static string? _c2_get_name_from_offset(int offset)
            {
                var tt = Deinterleave(offset, 32);
                var cur_idx0 = tt.Item1;
                var cur_idx1 = tt.Item2;

                // Get prefixes/suffixes from the individual offsets
                var p0 = _get_entry_from_offset(cur_idx0, _prefix_offsets.Keys, _prefix_offsets);
                var p1 = _get_entry_from_offset(cur_idx1, _prefix_offsets.Keys, _prefix_offsets);
                var s0 = _get_suffixes(p0)[cur_idx0 - _prefix_offsets[p0][0]];
                var s1 = _get_suffixes(p1)[cur_idx1 - _prefix_offsets[p1][0]];

                // Done!
                var name = format_sector_name(new List<string> { p0, s0, p1, s1 });
                return name;
            }

            public static int _c2_get_offset_from_name(List<string> frags)
            {
                var i1 = _prefix_offsets[frags[0]][0] + _get_suffixes(frags[0]).IndexOf(frags[1]);
                var i2 = _prefix_offsets[frags[2]][0] + _get_suffixes(frags[2]).IndexOf(frags[3]);
                var offset = Interleave(i1, i2, 32);
                return offset;
            }

            #endregion

            #region sector.py
            // From: https://bitbucket.org/Esvandiary/edts/src/develop/edtslib/sector.py

            public static int sector_size = 1280;
            public static int[] galaxy_size = new int[] { 128, 128, 128 };
            public static Point3 internal_origin_offset = new Point3(-49985, -40985, -24105);
            private static int[] base_sector_index = new int[] { 39, 32, 18 };
            public static Point3 base_coords = internal_origin_offset + (new Point3(base_sector_index) * sector_size);

            public static int get_mcode_cube_width(char mcode)
            {
                int d = 'h' - mcode;
                return sector_size / (int)Math.Pow(2, d);
            }

            #endregion

            #region util.py
            // From: https://bitbucket.org/Esvandiary/edts/src/develop/edtslib/util.py

            private static int Jenkins32(uint key)
            {
                key += (key << 12);
                key &= 0xFFFFFFFF;
                key ^= (key >> 22);
                key += (key << 4);
                key &= 0xFFFFFFFF;
                key ^= (key >> 9);
                key += (key << 10);
                key &= 0xFFFFFFFF;
                key ^= (key >> 2);
                key += (key << 7);
                key &= 0xFFFFFFFF;
                key ^= (key >> 12);
                return (int)key;
            }

            /// <summary>
            /// Shifts existing data left by N bits and adds a new value into the "empty" space
            /// </summary>
            public static long pack_and_shift(long value, int newData, int bits)
            {
                var shifted = value << bits;
                var tail = (newData & ((1 << bits) - 1));
                return shifted + tail;
            }

            private static int Interleave(int val1, int val2, int maxbits)
            {
                //Game.log($"interleave:\r\n\t{toBin(val1)}\r\n\t{toBin(val2)}");
                int output = 0;

                for (int i = 0; i <= maxbits / 2; i++)
                    output |= ((val1 >> i) & 1) << (i * 2);

                for (int i = 0; i <= maxbits / 2; i++)
                    output |= ((val2 >> i) & 1) << (i * 2 + 1);

                //var rslt = output & ((1 << maxbits) - 1);

                //Game.log($"interleave:\r\n\t{toBin(output)}");
                return output;
            }

            private static (int, int) Deinterleave(int val, int maxbits)
            {
                //Game.log($"deinterleave:\r\n\t{toBin(val)}");
                int out1 = 0;
                int out2 = 0;

                for (int i = 0; i < maxbits; i += 2)
                    out1 |= ((val >> i) & 1) << (i / 2);

                for (int i = 1; i < maxbits; i += 2)
                    out2 |= ((val >> i) & 1) << (i / 2);


                //Game.log($"deinterleave:\r\n\t{toBin(out1)}\r\n\t{toBin(out2)}");
                return (out1, out2);
            }

            #endregion

            // Helper function for division and remainder
            private static (int, int) Divmod(int x, int y)
            {
                return (x / y, x % y);
            }
        }

        class HARegion
        {
            public string name;
            public double size;
            public Sphere[] spheres;

            private Sphere origin;

            public HARegion(string name, double size, params Sphere[] spheres)
            {
                this.name = name;
                this.size = size;
                this.spheres = spheres;

                if (spheres.Length == 1)
                {
                    this.origin = spheres[0];
                }
                else
                {
                    // origin is smallest x,y,z (independently)
                    Debugger.Break();
                }
            }

            public Point3 get_origin(char mcode)
            {
                // TODO: make this work                
                var cube_width = Boxel.Sectors.get_mcode_cube_width(mcode);
                var x = (int)Math.Floor(origin.x);
                var y = (int)Math.Floor(origin.y);
                var z = (int)Math.Floor(origin.z);

                var dx = (x - Boxel.Sectors.base_coords.x) % cube_width;
                var dy = (y - Boxel.Sectors.base_coords.y) % cube_width;
                var dz = (z - Boxel.Sectors.base_coords.z) % cube_width;

                x -= (int)dx;
                y -= (int)dy;
                z -= (int)dz;

                return new Point3(x, y, z);
            }
        }

        internal class Sphere
        {
            public double x;
            public double y;
            public double z;
            public double size;

            public Sphere(double x, double y, double z, double size)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.size = size;
            }

            public static implicit operator Point3(Sphere s)
            {
                return new Point3((int)s.x, (int)s.y, (int)s.z);
            }
        }
    }
}
