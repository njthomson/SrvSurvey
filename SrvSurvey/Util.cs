using DecimalMath;
using SharpDX.DirectInput;
using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using SrvSurvey.units;
using System.Diagnostics;
using static SrvSurvey.game.GuardianSiteData;
using static System.Windows.Forms.ListView;

namespace SrvSurvey
{
    static class Util
    {
        public static double[] sol = new double[] { 0, 0, 0 };

        public static double degToRad(double angle)
        {
            return angle / Angle.ratioDegreesToRadians;
        }
        public static double degToRad(int angle)
        {
            return angle / Angle.ratioDegreesToRadians;
        }

        public const decimal ratioDegreesToRadians = 180 / (decimal)Math.PI;

        public static decimal degToRad(decimal angle)
        {
            return angle / ratioDegreesToRadians;
        }

        public static string lsToString(double m)
        {
            if (m > 1000)
                return (m / 1000).ToString("N1") + "k LS";
            else
                return m.ToString("N0") + " LS";
        }

        public static double lsToM(double ls)
        {
            // 149597870691 M per LS
            return 149_597_870_691d * ls;
        }

        public static double mToLS(double ls)
        {
            // 149597870691 M per LS
            return ls / 149_597_870_691d;
        }

        public static double auToM(double ls)
        {
            // 299792458 M per AU
            return 299_792_458d * ls;
        }

        public static string metersToString(decimal m, bool asDelta = false)
        {
            return metersToString((double)m, asDelta);
        }

        /// <summary>
        /// Returns a count of meters to a string with 4 significant digits, adjusting the units accordinly between: m, km, mm
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string metersToString(double m, bool asDelta = false)
        {
            var prefix = "";
            if (asDelta)
            {
                prefix = m < 0 ? "-" : "+";
            }

            // make number positive
            if (m < 0) m = -m;

            if (m < 1)
            {
                // less than 1 thousand
                return "0m";
            }

            if (m < 1000)
            {
                // less than 1 thousand
                return prefix + m.ToString("#") + "m";
            }

            m = m / 1000;
            if (m < 10)
            {
                // less than 1 ten
                return prefix + m.ToString("##.##") + "km";
            }
            else if (m < 1000)
            {
                // less than 1 thousand
                return prefix + m.ToString("###.#") + "km";
            }

            m = m / 1000;
            // over one 1 million
            return prefix + m.ToString("#.##") + "mm";
        }

        public static string timeSpanToString(TimeSpan ago)
        {
            if (ago.TotalSeconds < 60)
                return "just now";
            if (ago.TotalMinutes < 120)
                return ago.TotalMinutes.ToString("#") + " minutes ago";
            if (ago.TotalHours < 24)
                return ago.TotalHours.ToString("#") + " hours ago";
            return ago.TotalDays.ToString("#") + " days ago";
        }

        public static string getLocationString(string starSystem, string body)
        {
            if (string.IsNullOrEmpty(body))
                return starSystem;


            // for cases like: "StarSystem":"Adenets" .. "Body":"Adenets 8 c"
            if (body.Contains(starSystem))
                return body;

            // for cases like: "StarSystem":"Enki" .. "Body":"Ponce de Leon Vision",
            return $"{starSystem}, {body}";

        }

        public static void openLink(string link)
        {
            var trimmedLink = link.Substring(0, Math.Min(200, link.Length));
            Game.log($"Opening link: (length: {link.Length})\r\n{trimmedLink}\r\n");

            var info = new ProcessStartInfo(link);
            info.UseShellExecute = true;
            Process.Start(info);
        }

        public static string credits(long credits, bool hideUnits = false)
        {
            string txt;

            if (credits < 1_000)
                txt = credits.ToString("N0");
            else if (credits < 100_000)
                txt = (credits / 1_000d).ToString("#.##") + " K";
            else if (credits < 1_000_000)
                txt = (credits / 1_000d).ToString("#") + " K";
            else if (credits < 100_000_000)
                txt = (credits / 1_000_000d).ToString("#.##") + " M";
            else if (credits < 1_000_000_000)
                txt = (credits / 1_000_000d).ToString("#") + " M";
            else
                txt = (credits / 1_000_000_000d).ToString("#.###") + " B";

            if (!hideUnits)
                txt += " CR";
            return txt;
        }

        public static string getMinMaxCredits(long min, long max)
        {
            if (min <= 0 && max <= 0) return "";

            return min == max
                ? Util.credits(min, true)
                : Util.credits(min, true) + " ~ " + Util.credits(max, true);
        }

        public static LatLong2 adjustForCockpitOffset(decimal radius, LatLong2 location, string shipType, float shipHeading)
        {
            if (shipType == null) return location;

            var po = CanonnStation.getShipOffset(shipType);
            if (po.X == 0 && po.Y == 0)
            {
                Game.log($"No offset for {shipType} yet :/");
                return location;
            }

            var pd = po.rotate((decimal)shipHeading);

            var dpm = 1 / (DecimalEx.TwoPi * radius / 360m); // meters per degree
            var dx = pd.X * dpm;
            var dy = pd.Y * dpm;

            var newLocation = new LatLong2(
                location.Lat + dy,
                location.Long + dx
            );

            var dist = po.dist;
            var angle = po.angle + (decimal)shipHeading;
            Game.log($"Adjusting landing location for: {shipType}, dist: {dist}, angle: {angle}\r\npd:{pd}, po:{po} (alt: {Game.activeGame?.status?.Altitude})\r\n{location} =>\r\n{newLocation}");
            return newLocation;
        }

        public static PointM getOffset(decimal r, LatLong2 p1, float siteHeading = -1)
        {
            return getOffset(r, p1, null, siteHeading);
        }

        /// <summary>
        /// Returns the relative offset (in meters) from one location to the other
        /// </summary>
        public static PointM getOffset(decimal r, LatLong2 p1, LatLong2? p2, float siteHeading = -1)
        {
            if (p2 == null) p2 = Status.here.clone();

            var dist = Util.getDistance(p1, p2, r);
            var radians = Util.getBearingRad(p1, p2);

            // apply siteHeading rotation, if given
            if (siteHeading != -1)
                radians = radians - DecimalEx.ToRad((decimal)siteHeading);

            var dx = DecimalEx.Sin(radians) * dist;
            var dy = DecimalEx.Cos(radians) * dist;

            // Positive latitude is to the north
            // Positive longitude is to the east

            var pf = new PointM(dx, dy);
            return pf;
        }

        /// <summary>
        /// Returns the distance between two lat/long points on body with radius r.
        /// </summary>
        public static decimal getDistance(LatLong2 p1, LatLong2 p2, decimal r)
        {
            // don't bother calculating if positions are identical
            if (p1.Lat == p2.Lat && p1.Long == p2.Long)
                return 0;

            var lat1 = DecimalEx.ToRad(p1.Lat);
            var lat2 = DecimalEx.ToRad(p2.Lat);

            var angleDelta = DecimalEx.ACos(
                DecimalEx.Sin(lat1) * DecimalEx.Sin(lat2) +
                DecimalEx.Cos(lat1) * DecimalEx.Cos(lat2) * DecimalEx.Cos(Util.degToRad(p2.Long - p1.Long))
            );
            var dist = angleDelta * r;

            //// ---
            //var dLat = DecimalEx.ToRad(p2.Lat - p1.Lat);
            //var dLon = DecimalEx.ToRad(p2.Long - p1.Long);

            //var aa = DecimalEx.Sin(dLat / 2) * DecimalEx.Sin(dLat / 2) +
            //    DecimalEx.Cos(DecimalEx.ToRad(p1.Lat)) * DecimalEx.Cos(DecimalEx.ToRad(p2.Lat)) * DecimalEx.Sin(dLon / 2) *
            //    DecimalEx.Sin(dLon / 2)
            //    ;
            //var cc = 2 * DecimalEx.ATan2(DecimalEx.Sqrt(aa), DecimalEx.Sqrt(1 - aa));
            //var dd = r * cc;
            //// ---

            return dist;
        }

        /// <summary>
        /// Returns the bearing between two lat/long points on a body.
        /// </summary>
        public static decimal getBearing(LatLong2 p1, LatLong2 p2)
        {
            var deg = DecimalEx.ToDeg(getBearingRad(p1, p2));
            if (deg < 0) deg += 360;

            return deg;
        }

        public static decimal getBearingRad(LatLong2 p1, LatLong2 p2)
        {
            var lat1 = Util.degToRad((decimal)p1.Lat);
            var lon1 = Util.degToRad((decimal)p1.Long);
            var lat2 = Util.degToRad((decimal)p2.Lat);
            var lon2 = Util.degToRad((decimal)p2.Long);

            var x = DecimalEx.Cos(lat2) * DecimalEx.Sin(lon2 - lon1);
            var y = DecimalEx.Cos(lat1) * DecimalEx.Sin(lat2) - DecimalEx.Sin(lat1) * DecimalEx.Cos(lat2) * DecimalEx.Cos(lon2 - lon1);

            // x is opposite / y is adjacent
            var rad = DecimalEx.ATan2(x, y);
            if (rad < 0) rad += DecimalEx.TwoPi;

            return rad;
        }

        public static PointM rotateLine(decimal angle, decimal length)
        {
            var dx = DecimalEx.Sin(DecimalEx.ToRad(angle)) * length;
            var dy = DecimalEx.Cos(DecimalEx.ToRad(angle)) * length;

            return new PointM(dx, dy);
        }

        public static PointF rotateLine(double angle, double length)
        {
            var dx = Math.Sin(Util.degToRad(angle)) * length;
            var dy = Math.Cos(Util.degToRad(angle)) * length;

            return new PointF((float)dx, (float)dy);
        }

        public static decimal ToAngle(double opp, double adj)
        {
            return ToAngle((decimal)opp, (decimal)adj);
        }

        public static decimal ToAngle(decimal opp, decimal adj)
        {
            if (opp == 0 && adj == 0) return 0;

            var flip = adj < 0;

            if (adj == 0) adj += 0.00001M;
            var angle = DecimalEx.ToDeg(DecimalEx.ATan(opp / adj));

            if (flip) angle = angle - 180;

            return angle;
        }

        /// <summary>
        /// Extract the species prefix from the name, without the color variant part
        /// </summary>
        public static string getSpeciesPrefix(string name)
        {
            // extract the species prefix from the name, without the color variant part
            var species = name.Replace("$Codex_Ent_", "").Replace("_Name;", "");
            var idx = species.LastIndexOf('_');
            if (species.IndexOf('_') != idx)
                species = species.Substring(0, idx);

            species = $"$Codex_Ent_{species}_";
            return species;
        }

        /// <summary>
        /// Extract the genus prefix from the name, without the color variant part
        /// </summary>
        public static string getGenusNameFromVariant(string name)
        {
            // extract the species prefix from the name, without the color variant part
            var genus = name.Replace("$Codex_Ent_", "").Replace("_Name;", "");
            var idx = genus.LastIndexOf('_');
            if (genus.IndexOf('_') != idx)
                genus = genus.Substring(0, idx);

            idx = genus.LastIndexOf('_');
            if (idx > 0)
                genus = genus.Substring(0, idx);

            genus = $"$Codex_Ent_{genus}_";
            return genus;
        }

        /// <summary>
        /// Extract the genus display name from variant localized
        /// </summary>
        public static string getGenusDisplayNameFromVariant(string variantLocalized)
        {
            var parts = variantLocalized.Split(" ", 2);
            if (variantLocalized.Contains("-"))
            {
                return parts[0];
            }
            else
            {
                return parts[1];
            }
        }

        public static double getSystemDistance(double[] here, double[]? there)
        {
            if (there == null) return -1;

            var dist = Math.Sqrt(
                Math.Pow(here[0] - there[0], 2)
                + Math.Pow(here[1] - there[1], 2)
                + Math.Pow(here[2] - there[2], 2)
            );
            return dist;
        }

        public static bool isCloseToScan(LatLong2 location, string? genusName)
        {
            if (Game.activeGame?.cmdr == null || Game.activeGame.systemBody == null || genusName == null) return false;
            var minDist = (decimal)Math.Min(PlotTrackers.highlightDistance, Game.activeGame.cmdr.scanOne?.radius ?? int.MaxValue);

            if (Game.activeGame.cmdr.scanOne != null)
            {
                if (Game.activeGame.cmdr.scanOne.genus != genusName) return false;

                var dist = Util.getDistance(location, Game.activeGame.cmdr.scanOne.location, Game.activeGame.systemBody!.radius);
                if (dist < minDist)
                    return true;
            }
            if (Game.activeGame.cmdr.scanTwo != null)
            {
                var dist = Util.getDistance(location, Game.activeGame.cmdr.scanTwo.location, Game.activeGame.systemBody!.radius);
                if (dist < minDist)
                    return true;
            }

            return false;
        }

        public static void useLastLocation(Form form, Rectangle rect, bool locationOnly = false)
        {
            useLastLocation(form, rect.Location, locationOnly ? Size.Empty : rect.Size);
        }

        public static void useLastLocation(Form form, Point pt)
        {
            useLastLocation(form, pt, Size.Empty);
        }

        public static void useLastLocation(Form form, Point pt, Size sz)
        {
            if (pt == Point.Empty)
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            if (sz != Size.Empty)
            {
                // not all forms are sizable, so don't change those
                form.Width = Math.Max(sz.Width, form.MinimumSize.Width);
                form.Height = Math.Max(sz.Height, form.MinimumSize.Height);
            }

            // position ourselves within the bound of which ever screen is chosen
            var r = Screen.GetBounds(pt);
            if (pt.X < r.Left) pt.X = r.Left;
            if (pt.X + form.Width > r.Right) pt.X = r.Right - form.Width;

            if (pt.Y < r.Top) pt.Y = r.Top;
            if (pt.Y + form.Height > r.Bottom) pt.Y = r.Bottom - form.Height;

            form.StartPosition = FormStartPosition.Manual;
            form.Location = pt;

            // Don't allow the form to be larger than the screen itself
            var screen = Screen.GetWorkingArea(form);
            if (form.Width > screen.Width && form.Height > screen.Height)
            {
                form.Width = screen.Width;
                form.Height = screen.Height;
            }
        }

        public static double targetAltitudeForSite(GuardianSiteData.SiteType siteType)
        {
            switch (siteType)
            {
                case GuardianSiteData.SiteType.Alpha: return Game.settings.aerialAltAlpha;
                case GuardianSiteData.SiteType.Beta: return Game.settings.aerialAltBeta;
                case GuardianSiteData.SiteType.Gamma: return Game.settings.aerialAltGamma;
                // TODO: add settings?
                case GuardianSiteData.SiteType.Robolobster: return 1000d;
                case GuardianSiteData.SiteType.Crossroads: return 500d;
                case GuardianSiteData.SiteType.Fistbump: return 450d;
                default: return 650d;
            }
        }

        public static string flattenStarType(string? starType)
        {
            if (starType == null) return null!;
            // Collapse these:
            //  D DA DAB DAO DAZ DAV DB DBZ DBV DO DOV DQ DC DCV DX
            //  W WN WNC WC WO
            //  CS C CN CJ CH CHd
            if (starType[0] == 'D' || starType[0] == 'W' || starType[0] == 'C')
                return starType[0].ToString();
            else
                return starType;
        }

        public static void showForm(Form form, Form? parent = null)
        {
            if (form.Visible == false)
            {
                //// NO! fade in if form doesn't exist yet
                //if (true && !form.IsHandleCreated)
                //{
                //    form.Opacity = 0;
                //    form.Load += new EventHandler((object? sender, EventArgs e) =>
                //    {
                //        // using the quicker fade-out duration
                //        Util.fadeOpacity(form, 1, Game.settings.fadeOutDuration); // 100ms
                //    });
                //}

                if (parent == null)
                    form.Show();
                else
                    form.Show(parent);
            }
            else
            {
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;

                form.Activate();
            }
        }

        public static void fadeOpacity(PlotterForm form, float targetOpacity, float durationMs = 0)
        {
            if (targetOpacity < 0 || targetOpacity > 1) throw new ArgumentOutOfRangeException(nameof(targetOpacity));
            //Debug.WriteLine($"!START! {form.Name} {form.Size} {durationMs}ms / {form.Opacity} => {targetOpacity}");

            // exit early if no-op
            if (targetOpacity == form.Opacity) return;

            // don't animate if duration is zero
            if (durationMs == 0)
                form.setOpacity(targetOpacity);
            else
            {
                var started = DateTime.Now.Ticks;
                var s1 = DateTime.Now;

                var delta = 1f / durationMs;
                fadeNext(form, targetOpacity, 0, delta);
                //Application.DoEvents();

                //var fadeTimer = new System.Windows.Forms.Timer()
                //{
                //    Interval = 50,
                //};
                //delta *= fadeTimer.Interval;
                //var nn = 0;

                //fadeTimer.Tick += new EventHandler((object? sender, EventArgs e) =>
                //{
                //    var dd = (DateTime.Now.Ticks - started) / 10_000;
                //    //Debug.WriteLine($"? {dd}");

                //    var wasOpacity = form.Opacity;
                //    var wasSmaller = form.Opacity < targetOpacity;

                //    var newOpacity = form.Opacity + (wasSmaller ? delta : -delta);
                //    var isSmaller = newOpacity < targetOpacity;
                //    ++nn;

                //    //Debug.WriteLine($"{dd} {form.Name} #{++nn} delta:{delta}, junk.Opacity:{wasOpacity}, this.targetOpacity:{targetOpacity}, wasSmaller:{wasSmaller}, isSmaller:{isSmaller}");

                //    // end animation when we reach target
                //    if (wasSmaller != isSmaller || form.Opacity == targetOpacity)
                //    {
                //        fadeTimer.Stop();
                //        Debug.WriteLine($"{dd} {form.Name} #{nn} delta:{delta}, junk.Opacity:{wasOpacity}, this.targetOpacity:{targetOpacity}, wasSmaller:{wasSmaller}, isSmaller:{isSmaller}");
                //        form.Opacity = targetOpacity;
                //        Debug.WriteLine($"!STOP! ({nn}) {form.Name} {DateTime.Now.Subtract(s1).TotalMilliseconds}");
                //    }
                //    else
                //    {
                //        form.Opacity = newOpacity;
                //    }
                //});

                //fadeTimer.Start();
            }
        }

        private static void fadeNext(PlotterForm form, float targetOpacity, long lastTick, float delta)
        {
            if (form.IsDisposed) return;
            if (!Elite.gameHasFocus || form.forceHide)
            {
                // stop early and hide form if the game loses focus, or the form is being forced hidden
                form.setOpacity(0);
                return;
            }

            var wasOpacity = form.Opacity;
            var wasSmaller = form.Opacity < targetOpacity;

            // (there are 10,000 ticks in a millisecond)
            var nextTick = lastTick + 10_000;

            if (nextTick < DateTime.Now.Ticks)
            {
                var newOpacity = form.Opacity + (wasSmaller ? delta : -delta);
                var isSmaller = newOpacity < targetOpacity;

                //Debug.WriteLine($"! delta:{delta}, junk.Opacity:{wasOpacity}, this.targetOpacity:{targetOpacity}, wasSmaller:{wasSmaller}, isSmaller:{isSmaller}");

                // end animation when we reach target
                if (wasSmaller != isSmaller || form.Opacity == targetOpacity)
                {
                    if (form.Opacity != newOpacity)
                        form.setOpacity(targetOpacity);
                    //Debug.WriteLine($"!STOP! {form.Name} {form.Size} {form.Opacity}");
                    form.Invalidate();
                    return;
                }

                form.setOpacity(newOpacity);
                lastTick = DateTime.Now.Ticks;
                // TODO: animate the location too, just a little?
            }
            else
            {
                //Debug.WriteLine($"~ delta:{delta}, junk.Opacity:{wasOpacity}, this.targetOpacity:{targetOpacity}, skip! {lastTick} // {nextTick} < {DateTime.Now.Ticks}");
            }

            Program.control.BeginInvoke(() => fadeNext(form, targetOpacity, lastTick, delta));
        }

        public static bool isOdyssey = Game.activeGame?.journals == null || Game.activeGame.journals.isOdyssey;

        public static int GetBodyValue(Scan scan, bool cmdrMapped)
        {
            return Util.GetBodyValue(
                scan.PlanetClass ?? scan.StarType, // planetClass
                scan.TerraformState == "Terraformable", // isTerraformable
                scan.MassEM > 0 ? scan.MassEM : scan.StellarMass, // mass
                !scan.WasDiscovered, // isFirstDiscoverer
                cmdrMapped, // isMapped
                !scan.WasMapped // isFirstMapped
            );
        }

        public static int GetBodyValue(SystemBody body, bool cmdrMapped, bool withEfficiencyBonus)
        {
            return Util.GetBodyValue(
                body.type == SystemBodyType.Star ? body.starType : body.planetClass, // planetClass
                body.terraformable, // isTerraformable
                body.mass,
                !body.wasDiscovered, // isFirstDiscoverer
                cmdrMapped,
                !body.wasMapped, // isFirstMapped
                withEfficiencyBonus
            );
        }

        public static double getStarKValue(string starClass)
        {
            var kk = 1200;
            if (starClass == "NS" || starClass == "BH" || starClass == "SupermassiveBlackHole")
                kk = 22628;
            else if (starClass?[0] == 'W')
                kk = 14057;

            return kk;
        }

        public static double getBodyKValue(string planetClass, bool isTerraformable)
        {
            int k = 0;

            if (planetClass == "Metal rich body") // MR - Metal Rich
                k = 21790;
            else if (planetClass == "Ammonia world") // AW
                k = 96932;
            else if (planetClass == "Sudarsky class I gas giant") // GG1
                k = 1656;
            else if (planetClass == "Sudarsky class II gas giant" // GG2
                || planetClass == "High metal content body") // HMC
            {
                k = 9654;
                if (isTerraformable) k += 100677;
            }
            else if (planetClass == "Water world") // WW
            {
                k = 64831;
                if (isTerraformable) k += 116295;
            }
            else if (planetClass?.StartsWith("Earth") == true) // ELW
            {
                k = 64831 + 116295;
            }
            else // RB - Rocky Body
            {
                k = 300;
                if (isTerraformable) k += 93328;
            }

            return k;
        }

        public static int GetBodyValue(string? planetClass, bool isTerraformable, double mass, bool isFirstDiscoverer, bool isMapped, bool isFirstMapped, bool withEfficiencyBonus = true, bool isFleetCarrierSale = false)
        {
            // Logic from https://forums.frontier.co.uk/threads/exploration-value-formulae.232000/

            var isOdyssey = Util.isOdyssey;
            var isStar = planetClass != null && (planetClass.Length < 8 || planetClass[1] == '_' || planetClass == "SupermassiveBlackHole" || planetClass == "Nebula" || planetClass == "StellarRemnantNebula");

            if (isStar)
            {
                var kk = getStarKValue(planetClass!);
                var starValue = kk + (mass * kk / 66.25);
                return (int)Math.Round(starValue);
            }

            // determine base value
            var k = getBodyKValue(planetClass!, isTerraformable);

            // public static int GetBodyValue(int k, double mass, bool isFirstDiscoverer, bool isMapped, bool isFirstMapped, bool withEfficiencyBonus, bool isOdyssey, bool isFleetCarrierSale)
            // based on code from https://forums.frontier.co.uk/threads/exploration-value-formulae.232000/
            const double q = 0.56591828;
            double mappingMultiplier = 1;
            if (isMapped)
            {
                if (isFirstDiscoverer && isFirstMapped)
                {
                    mappingMultiplier = 3.699622554;
                }
                else if (isFirstMapped)
                {
                    mappingMultiplier = 8.0956;
                }
                else
                {
                    mappingMultiplier = 3.3333333333;
                }
            }
            double value = (k + k * q * Math.Pow(mass, 0.2)) * mappingMultiplier;
            if (isMapped)
            {
                if (isOdyssey)
                {
                    value += ((value * 0.3) > 555) ? value * 0.3 : 555;
                }
                if (withEfficiencyBonus)
                {
                    value *= 1.25;
                }
            }
            value = Math.Max(500, value);
            value *= (isFirstDiscoverer) ? 2.6 : 1;
            value *= (isFleetCarrierSale) ? 0.75 : 1;
            return (int)Math.Round(value);
        }

        public static bool isCloseColor(Color actual, WatchColor expected)
        {
            return isCloseColor(actual, expected.color, expected.t);
        }

        public static bool isCloseColor(Color actual, Color expected, int tolerance = 5)
        {
            // confirm actual RGB values are within ~5 of expected
            return actual.R > expected.R - tolerance && actual.R < expected.R + tolerance
                && actual.G > expected.G - tolerance && actual.G < expected.G + tolerance
                && actual.B > expected.B - tolerance && actual.B < expected.B + tolerance;
        }

        public static bool isFirewallProblem(Exception? ex)
        {
            if (ex == null) return false;
            if (ex != null && !ex.Message.Contains("404")) Game.log($"isFirewallProblem?\r\n{ex}");
            if (ex?.Message.Contains("An attempt was made to access a socket in a way forbidden by its access permissions") == true
                || ex?.Message.Contains("A socket operation was attempted to an unreachable network") == true)
            {
                var rslt = MessageBox.Show(
                    Properties.Misc.FirewallBlocked.format(Application.ExecutablePath),
                    "SrvSurvey",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (rslt == DialogResult.Yes) Clipboard.SetText(Application.ExecutablePath);
                return true;
            }

            if ((ex as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.InternalServerError
                || ex?.Message.Contains("Response status code does not indicate success: 500") == true
                || ex?.Message.Contains("(Internal Server Error)") == true)
            {
                // strictly speaking this is not a firewall problem, it means some network call failed (hopefully) not due to us. Let's trace this and keep going.
                Game.log($"Ignoring HTTP:500 response");
                return true;
            }
            return false;
        }

        public static void handleError(Exception? ex)
        {
            if (Program.control.InvokeRequired)
            {
                Program.control.Invoke(new Action(() => handleError(ex)));
                return;
            }

            Game.log(ex);
            if ((ex as HttpRequestException)?.StatusCode == System.Net.HttpStatusCode.NotFound || Util.isFirewallProblem(ex))
            {
                // ignore NotFound or firewall responses
            }
            else if (ex != null)
            {
                FormErrorSubmit.Show(ex);
            }
            else
            {
                MessageBox.Show(
                    Main.ActiveForm,
                    Properties.Misc.UnexpectedWentWrong,
                    "SrvSurvey",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                BaseForm.show<ViewLogs>();
            }

        }

        /// <summary> Returns the localized name of the log category </summary>
        public static string getLogNameFromChar(char c)
        {
            switch (c)
            {
                case 'C': return Properties.Guardian.LogCulture;
                case 'H': return Properties.Guardian.LogHistory;
                case 'B': return Properties.Guardian.LogBiology;
                case 'T': return Properties.Guardian.LogTechnology;
                case 'L': return Properties.Guardian.LogLanguage;
                case '#': return "";
                default:
                    throw new Exception($"Unexpected character '{c}'");
            }
        }

        public static string getLoc(Enum enumValue)
        {
            var enumName = enumValue.GetType().Name + $".{enumValue}";
            return getLoc(enumName);
        }

        /// <summary> Returns the localized string for something</summary>
        public static string getLoc(string name)
        {
            switch (name)
            {
                case nameof(SiteType.Alpha): return Properties.Guardian.Alpha;
                case nameof(SiteType.Beta): return Properties.Guardian.Beta;
                case nameof(SiteType.Gamma): return Properties.Guardian.Gamma;
                // These are not localized - just use the given string
                case "SiteType." + nameof(SiteType.Lacrosse): // Tiny
                case "SiteType." + nameof(SiteType.Crossroads):
                case "SiteType." + nameof(SiteType.Fistbump):
                case "SiteType." + nameof(SiteType.Hammerbot): // Small
                case "SiteType." + nameof(SiteType.Bear):
                case "SiteType." + nameof(SiteType.Bowl):
                case "SiteType." + nameof(SiteType.Turtle):
                case "SiteType." + nameof(SiteType.Robolobster): // Medium
                case "SiteType." + nameof(SiteType.Squid):
                case "SiteType." + nameof(SiteType.Stickyhand):
                    return name;

                // Guardian items for Ram Tah missions
                case $"{nameof(POIType)}.{nameof(ObeliskItem.casket)}":
                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.casket)}":
                case nameof(ObeliskItem.casket):
                    return Properties.Guardian.ItemCasket;

                case $"{nameof(POIType)}.{nameof(ObeliskItem.orb)}":
                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.orb)}":
                case nameof(ObeliskItem.orb):
                    return Properties.Guardian.ItemOrb;

                case $"{nameof(POIType)}.{nameof(ObeliskItem.relic)}":
                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.relic)}":
                case nameof(ObeliskItem.relic):
                    return Properties.Guardian.ItemRelic;

                case $"{nameof(POIType)}.{nameof(ObeliskItem.tablet)}":
                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.tablet)}":
                case nameof(ObeliskItem.tablet):
                    return Properties.Guardian.ItemTablet;

                case $"{nameof(POIType)}.{nameof(ObeliskItem.totem)}":
                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.totem)}":
                case nameof(ObeliskItem.totem):
                    return Properties.Guardian.ItemTotem;

                case $"{nameof(POIType)}.{nameof(ObeliskItem.urn)}":
                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.urn)}":
                case nameof(ObeliskItem.urn):
                    return Properties.Guardian.ItemUrn;

                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.sensor)}":
                case nameof(ObeliskItem.sensor):
                    return Properties.Guardian.ItemSensor;

                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.probe)}":
                case nameof(ObeliskItem.probe):
                    return Properties.Guardian.ItemProbe;

                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.link)}":
                case nameof(ObeliskItem.link):
                    return Properties.Guardian.ItemLink;

                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.cyclops)}":
                case nameof(ObeliskItem.cyclops):
                    return Properties.Guardian.ItemCyclops;

                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.basilisk)}":
                case nameof(ObeliskItem.basilisk):
                    return Properties.Guardian.ItemBasilisk;

                case $"{nameof(ObeliskItem)}.{nameof(ObeliskItem.medusa)}":
                case nameof(ObeliskItem.medusa):
                    return Properties.Guardian.ItemMedusa;

                case nameof(SitePoiStatus) + ".unknown": return Properties.Guardian.SitePoiStatus_unknown;
                case nameof(SitePoiStatus) + ".present": return Properties.Guardian.SitePoiStatus_present;
                case nameof(SitePoiStatus) + ".absent": return Properties.Guardian.SitePoiStatus_absent;
                case nameof(SitePoiStatus) + ".empty":
                    return Properties.Guardian.SitePoiStatus_empty;

                case $"{nameof(POIType)}.{nameof(POIType.component)}":
                    return Properties.Guardian.POIType_Component;

                case $"{nameof(POIType)}.{nameof(POIType.pylon)}":
                    return Properties.Guardian.POIType_Pylon;

                case $"{nameof(POIType)}.{nameof(POIType.destructablePanel)}":
                    return "Destructable Panel";

                default:
                    Game.log($"Unexpected localizable name: '{name}'");
                    Debugger.Break();
                    return name + "?";
            }
        }

        /// <summary>
        /// Returns True if the two numbers are within tolerance of each other.
        /// </summary>
        public static bool isClose(double left, double right, double tolerance)
        {
            var delta = Math.Abs(left - right);
            return delta <= tolerance;
        }

        /// <summary>
        /// Returns True if the two numbers are within tolerance of each other.
        /// </summary>
        public static bool isClose(decimal left, decimal right, decimal tolerance)
        {
            var delta = Math.Abs(left - right);
            return delta <= tolerance;
        }

        /// <summary>
        /// Returns True is poiType is Casket, Orb, Tablet, Totem or an Urn.
        /// </summary>
        public static bool isBasicPoi(POIType poiType)
        {
            return poiType == POIType.casket || poiType == POIType.orb || poiType == POIType.tablet || poiType == POIType.totem || poiType == POIType.urn || poiType == POIType.unknown;
        }

        public static string pascal(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return "";
            else
                return char.ToUpperInvariant(txt[0]) + txt.Substring(1);
        }

        public static string pascalAllWords(string txt)
        {
            var words = txt
                .Split(' ')
                .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant());

            return string.Join(' ', words);
        }

        public static string compositionToCamel(string key)
        {
            // as needed, change "Sulphur dioxide" into "SulphurDioxide" or "Carbon dioxide-rich" into "CarbonDioxideRich"
            key = key.Replace("-rich", "Rich");
            var i = key.IndexOf(' ');
            if (i < 0)
                return key;
            else
                return key.Substring(0, i) + key.Substring(i + 1, 1).ToUpperInvariant() + key.Substring(i + 2);
        }

        public static bool isMatLevelThree(string matName)
        {
            switch (matName.ToLowerInvariant())
            {
                case "cadmium":
                case "mercury":
                case "molybdenum":
                case "niobium":
                case "tin":
                case "tungsten":
                    return true;

                default:
                    return false;
            }
        }
        public static bool isMatLevelFour(string matName)
        {
            switch (matName.ToLowerInvariant())
            {
                case "antimony":
                case "polonium":
                case "ruthenium":
                case "technetium":
                case "tellurium":
                case "yttrium":
                    return true;

                default:
                    return false;
            }
        }

        public static Image getBioImage(string genus, bool large = false)
        {
            //return ImageResources.tubus18;

            switch (genus)
            {
                case "$Codex_Ent_Aleoids_Genus_Name;": return ImageResources.aleoida_18;
                case "$Codex_Ent_Bacterial_Genus_Name;": return large ? ImageResources.bacterium38 : ImageResources.bacterium18;
                case "$Codex_Ent_Cactoid_Genus_Name;": return ImageResources.cactoida_18;
                case "$Codex_Ent_Clypeus_Genus_Name;": return ImageResources.clypeus_18;
                case "$Codex_Ent_Conchas_Genus_Name;": return ImageResources.concha_18;
                case "$Codex_Ent_Electricae_Genus_Name;": return large ? ImageResources.electricae38 : ImageResources.electricae_18;
                case "$Codex_Ent_Fonticulus_Genus_Name;": return ImageResources.fonticulua_18;
                case "$Codex_Ent_Shrubs_Genus_Name;": return large ? ImageResources.frutexa38 : ImageResources.frutexa18;
                case "$Codex_Ent_Fumerolas_Genus_Name;": return ImageResources.fumerola_18;
                case "$Codex_Ent_Fungoids_Genus_Name;": return large ? ImageResources.fungoida38 : ImageResources.fungoida18;
                case "$Codex_Ent_Osseus_Genus_Name;": return large ? ImageResources.osseus38 : ImageResources.osseus_18;
                case "$Codex_Ent_Recepta_Genus_Name;": return ImageResources.recepta_18;
                case "$Codex_Ent_Stratum_Genus_Name;": return large ? ImageResources.stratum38 : ImageResources.stratum18;
                case "$Codex_Ent_Tubus_Genus_Name;": return large ? ImageResources.tubus38 : ImageResources.tubus18;
                case "$Codex_Ent_Tussocks_Genus_Name;": return large ? ImageResources.tussock38 : ImageResources.tussock18;
                case "$Codex_Ent_Vents_Genus_Name;": return ImageResources.amphora_18;
                case "$Codex_Ent_Sphere_Name;": return ImageResources.anemone_18;
                case "$Codex_Ent_Cone_Genus_Name;": return ImageResources.barkmound_18;
                case "$Codex_Ent_Brancae_Genus_Name;": return ImageResources.braintree_18;
                case "$Codex_Ent_Ground_Struct_Ice_Genus_Name;": return ImageResources.shards_18;
                case "$Codex_Ent_Tube_Name;":
                case "$Codex_Ent_Tube_Genus_Name;":
                case "$Codex_Ent_TubeABCD_Genus_Name;":
                case "$Codex_Ent_TubeEFGH_Genus_Name;":
                    return ImageResources.tuber_18;

                default: throw new Exception($"Unexpected genus: '{genus}'");
            }
        }

        /// <summary>
        /// Returns an Economy enum from strings like "$economy_HighTech;"
        /// </summary>
        public static Economy toEconomy(string? txt)
        {
            switch (txt)
            {
                case "$economy_Agri;": return Economy.Agriculture;
                case "$economy_Colony;": return Economy.Colony;
                case "$economy_Damaged;": return Economy.Damaged;
                case "$economy_Extraction;": return Economy.Extraction;
                case "$economy_HighTech;": return Economy.HighTech;
                case "$economy_Industrial;": return Economy.Industrial;
                case "$economy_Military;": return Economy.Military;
                case "$economy_Prison;": return Economy.Prison;
                case "$economy_Carrier;": return Economy.PrivateEnterprise;
                case "$economy_Refinery;": return Economy.Refinery;
                case "$economy_Repair;": return Economy.Repair;
                case "$economy_Rescue;": return Economy.Rescue;
                case "$economy_Service;": return Economy.Service;
                case "$economy_Terraforming;": return Economy.Terraforming;
                case "$economy_Tourism;": return Economy.Tourist;

                default: return Economy.Unknown;
            }
        }

        public static string fromEconomy(Economy economy)
        {
            switch (economy)
            {
                case Economy.Agriculture: return "$economy_Agri;";
                case Economy.Colony: return "$economy_Colony;";
                case Economy.Damaged: return "$economy_Damaged;";
                case Economy.Extraction: return "$economy_Extraction;";
                case Economy.HighTech: return "$economy_HighTech;";
                case Economy.Industrial: return "$economy_Industrial;";
                case Economy.Military: return "$economy_Military;";
                case Economy.Prison: return "$economy_Prison;";
                case Economy.PrivateEnterprise: return "$economy_Carrier;";
                case Economy.Refinery: return "$economy_Refinery;";
                case Economy.Repair: return "$economy_Repair;";
                case Economy.Rescue: return "$economy_Rescue;";
                case Economy.Service: return "$economy_Service;";
                case Economy.Terraforming: return "$economy_Terraforming;";
                case Economy.Tourist: return "$economy_Tourism;";

                default: throw new Exception($"Unexpected: {economy}");
            }
        }

        public static void applyTheme(Control ctrl)
        {
            applyTheme(ctrl, Game.settings.darkTheme);
        }

        public static void applyTheme(Control ctrl, bool dark)
        {
            if (ctrl is Button)
            {
                ctrl.BackColor = dark ? SystemColors.ControlDark : SystemColors.ControlLight;
            }
            else if (ctrl is TextBox)
            {
                if (!ctrl.Enabled || ((TextBox)ctrl).ReadOnly)
                    ctrl.BackColor = dark ? SystemColors.AppWorkspace : SystemColors.Control;
                else
                    ctrl.BackColor = dark ? SystemColors.ScrollBar : SystemColors.Window;
            }
            else if (ctrl is ListView)
            {
                ctrl.BackColor = dark ? SystemColors.WindowFrame : SystemColors.Window;
                ctrl.ForeColor = dark ? SystemColors.Info : SystemColors.ControlText;
            }
            else if (ctrl is ListBox)
            {
                ctrl.BackColor = dark ? SystemColors.ScrollBar : SystemColors.Window;
            }
            else
            {
                ctrl.BackColor = dark ? SystemColors.AppWorkspace : SystemColors.Control;
            }

            foreach (Control child in ctrl.Controls)
            {
                if (child.Tag is string && child.Tag.ToString() == "NoTheme") continue;

                applyTheme(child, dark);
            }
        }

        public static string getReputationText(double reputation)
        {
            if (reputation <= -90)
                return Properties.Reputation.Hostile;
            else if (reputation <= -35)
                return Properties.Reputation.Unfriendly;
            else if (reputation <= +4)
                return Properties.Reputation.Neutral;
            else if (reputation <= +35)
                return Properties.Reputation.Cordial;
            else if (reputation <= +90)
                return Properties.Reputation.Friendly;
            else
                return Properties.Reputation.Allied;
        }

        public static int stringWidth(Font? font, string? text)
        {
            if (font == null || text == null)
                return 0;

            return TextRenderer.MeasureText(text, font, Size.Empty, textFlags).Width;
        }

        /// <summary>
        /// Returns the max width of any of the given strings
        /// </summary>
        public static float maxWidth(Font font, IEnumerable<string> texts)
        {
            return maxWidth(font, texts.ToArray());
        }

        /// <summary>
        /// Returns the max width of any of the given strings
        /// </summary>
        public static float maxWidth(Font font, params string[] texts)
        {
            if (texts.Length == 0) return 0;

            var max = texts.Max(t => TextRenderer.MeasureText(t, font, Size.Empty, textFlags).Width);
            return max;
        }

        public const TextFormatFlags textFlags = TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsTranslateTransform;

        /// <summary>
        /// Center the inner size within the outer size.
        /// </summary>
        public static int centerIn(int outer, int inner)
        {
            return (int)Math.Ceiling((outer / 2f) - (inner / 2f));
        }

        /// <summary>
        /// Center the inner size within the outer size.
        /// </summary>        
        public static float centerIn(float outer, float inner)
        {
            return (int)Math.Ceiling((outer / 2f) - (inner / 2f));
        }

        /// <summary>
        /// Waits to invoke the action, invoking only once should there be multiple requests during the delay time.
        /// </summary>
        public static void deferAfter(int delayMs, Action func)
        {
            var name = $"{func.Target}/{func.Method}";
            if (!pendingCounts.ContainsKey(name)) pendingCounts[name] = 0;

            pendingCounts[name]++;
            Game.log($"deferAfter {delayMs}ms => '{name}' (q:{pendingCounts[name]})");

            Task.Delay(delayMs).ContinueWith(t => Program.defer(() =>
            {
                if (--pendingCounts[name] <= 0)
                {
                    pendingCounts.Remove(name);
                    func();
                }
            }));
        }
        private static Dictionary<string, int> pendingCounts = new();

        public static bool hasIllegalFilenameCharacters(string filename)
        {
            return filename.Contains('\\')
                || filename.Contains('/')
                || filename.Contains(':')
                || filename.Contains('*')
                || filename.Contains('?')
                || filename.Contains('"')
                || filename.Contains('<')
                || filename.Contains('>')
                || filename.Contains('|')
                ;
        }
    }

    internal static class ExtensionMethods
    {
        /// <summary> Returns a string similar to ISO file format but valid for filenames </summary>
        public static string ToIsoFileString(this DateTimeOffset dateTime)
        {
            return dateTime.ToString("yyyyMMdd_HHmmss");
        }

        public static TreeNode Set(this TreeNodeCollection nodes, string key, string text)
        {
            if (!nodes.ContainsKey(key))
                return nodes.Add(key, text);
            else
                return nodes[key]!;
        }

        public static T? AddIfNotNull<T>(this List<T> list, T? item)
        {
            if (item != null)
                list.Add(item);

            return item;
        }

        /// <summary>
        /// Returns the value by key, or the given default value
        /// </summary>
        public static U? Get<T, U>(this Dictionary<T, U> dict, T key, U defaultValue) where T : notnull
        {
            if (dict.ContainsKey(key))
                return dict[key];
            else
                return defaultValue;
        }

        public static List<ListViewItem> ToList(this ListViewItemCollection items)
        {
            var list = new List<ListViewItem>();

            foreach (ListViewItem item in items)
                list.Add(item);

            return list;
        }

        public static List<TreeNode> ToList(this TreeNodeCollection nodes)
        {
            var list = new List<TreeNode>();

            foreach (TreeNode node in nodes)
                list.Add(node);

            return list;
        }

        public static int CountChecked(this TreeNodeCollection nodes)
        {
            int count = 0;

            foreach (TreeNode node in nodes)
                if (node.Checked) count++;

            return count;
        }

        public static bool HasChildren(this TreeNode node)
        {
            return node.Nodes.Count > 0;
        }

        public static bool HasGrandChildren(this TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
                if (child.HasChildren()) return true;

            return false;
        }

        /// <summary>
        /// The index of this node based on the visible nodes above it
        /// </summary>
        public static int VisibleIndex(this TreeNode node)
        {
            var idx = 0;
            var n = node;
            while (n.PrevVisibleNode != null)
            {
                idx++;
                //if (n.PrevVisibleNode?.Checked == true) idx--;

                n = n.PrevVisibleNode;
            }

            return idx;
        }

        public static void forEveryNode(this TreeNodeCollection nodes, Action<TreeNode> func)
        {
            foreach (TreeNode node in nodes)
            {
                // process any children
                if (node.Nodes.Count > 0)
                    forEveryNode(node.Nodes, func);

                // do the action
                func(node);
            }
        }

        public static void forEveryNode(this IEnumerable<TreeNode> nodes, Action<TreeNode> func)
        {
            foreach (TreeNode node in nodes)
            {
                // process any children
                if (node.Nodes.Count > 0)
                    forEveryNode(node.Nodes, func);

                // do the action
                func(node);
            }
        }

        /// <summary>
        /// Apply String.Format to the given string with the given parameters
        /// </summary>
        public static string format(this string txt, params object?[] args)
        {
            return string.Format(txt, args);
        }

        /// <summary>
        /// string.Join's the enumeration using separator prefixed by the header+separator
        /// </summary>
        public static string formatWithHeader<T>(this IEnumerable<T> values, string header, string separator)
        {
            return header + separator + string.Join(separator, values);
        }

        /// <summary>
        /// Supports basic string matching using wild-cards in the form of: "*foo", "foo*", "*foo*" or just "foo"
        /// </summary>
        public static bool matches(this string txt, string query)
        {
            if (query.StartsWith('*') && query.EndsWith('*'))
                return txt.Contains(query);
            else if (query.StartsWith('*') && !query.EndsWith('*'))
                return txt.EndsWith(query.Substring(1));
            else if (!query.StartsWith('*') && query.EndsWith('*'))
                return txt.StartsWith(query.Substring(0, query.Length - 1));
            else
                return txt == query;
        }

        /// <summary>
        /// Calls the action only if the task completes with a non-null result
        /// </summary>
        public static Task continueOnSuccess<T>(this Task<T> preTask, Action<T> func)
        {
            return continueOnSuccess(preTask, null, false, func);
        }

        /// <summary>
        /// Continues execution on the main thread only if the Task is successful and yields a non-null result, and the form has not been disposed
        /// </summary>
        public static Task continueOnMain<T>(this Task<T> preTask, Form? form, Action<T> func, bool allowNull = false)
        {
            return continueOnSuccess(preTask, form, true, func, allowNull);
        }

        private static Task continueOnSuccess<T>(this Task<T> preTask, Form? form, bool onMainThread, Action<T> func, bool allowNull = false)
        {
            return preTask.ContinueWith(postTask =>
            {
                // check for firewall problems?
                if (postTask.Exception != null || !postTask.IsCompletedSuccessfully)
                    Util.isFirewallProblem(postTask.Exception);

                // exit early if the call did not succeed or returns a null
                if (postTask.Exception != null || (!allowNull && postTask.Result == null)) return;

                // of if we have a form but it's disposed
                if (form?.IsDisposed == true) return;

                // invoke back on main UX thread?
                if (onMainThread)
                    Program.defer(() => func(postTask.Result));
                else
                    func(postTask.Result);
            });
        }

        /// <summary>
        /// Continues execution on the main thread only if the Task is successful and yields a non-null result, and the form has not been disposed
        /// </summary>
        public static Task continueOnMain(this Task preTask, Form? form, Action func)
        {
            return preTask.ContinueWith(postTask =>
            {
                // check for firewall problems?
                if (postTask.Exception != null || !postTask.IsCompletedSuccessfully)
                    Util.isFirewallProblem(postTask.Exception);

                // exit early if the call did not succeed or returns a null
                if (postTask.Exception != null) return;

                // of if we have a form but it's disposed
                if (form?.IsDisposed == true) return;

                // invoke back on main UX thread?
                Program.defer(() => func());
            });
        }

        /// <summary>
        /// Handle some task, logging errors but otherwise being silent
        /// </summary>
        public static bool justDoIt(this Task preTask)
        {
            preTask.ContinueWith(postTask =>
            {
                // check for firewall problems?
                if (postTask.Exception != null || !postTask.IsCompletedSuccessfully)
                    Util.isFirewallProblem(postTask.Exception);
            });

            return true;
        }

        /// <summary>
        /// Returns a string using local short date and 24 hour time
        /// </summary>
        public static string ToLocalShortDateTime24Hours(this DateTimeOffset dateTime)
        {
            return dateTime.LocalDateTime.ToShortDateString() + " " + dateTime.LocalDateTime.ToString("HH:mm");
        }

        /// <summary>
        /// Returns a string using Galactic short date and 24 hour time
        /// </summary>
        public static string ToGalacticShortDateTime24Hours(this DateTimeOffset dateTime)
        {
            return dateTime.AddYears(1286).UtcDateTime.ToShortDateString() + " " + dateTime.UtcDateTime.ToString("HH:mm");
        }

        public static TValue init<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
            // initialize dictionary entry if not found
            if (!dictionary.ContainsKey(key))
                dictionary[key] = Activator.CreateInstance<TValue>();

            return dictionary[key];
        }

        public static TValue? GetValueOrDefault<TValue>(this Dictionary<string, TValue> dictionary, string key, StringComparison comparison)
        {
            // match a key based on string comparison
            var matchKey = dictionary.Keys.FirstOrDefault(k => k.Equals(key, comparison));

            if (matchKey == null)
            {
                // initialize dictionary entry if not found
                return default;
            }
            else
            {
                return dictionary[matchKey];
            }
        }

        public static SizeF widestColumn(this SizeF sz, int idx, Dictionary<int, float> columns)
        {
            columns.init(idx);
            if (sz.Width > columns[idx]) columns[idx] = sz.Width;
            return sz;
        }
    }

    public class WatchColor
    {
        /// <summary> Tolerance </summary>
        public int t;

        public Color color;

        public WatchColor(int r, int g, int b, int t)
        {
            this.color = Color.FromArgb(255, r, g, b);
            this.t = t;
        }
    }
}
