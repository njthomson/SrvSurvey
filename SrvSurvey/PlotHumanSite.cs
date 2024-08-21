using DecimalMath;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Globalization;

namespace SrvSurvey
{
    internal class PlotHumanSite : PlotBaseSite, PlotterForm
    {
        public static bool autoZoom = true;
        /// <summary>Distance of outer circle, outside which ships and shuttles can be ordered</summary>
        public static float limitDist = 500;
        /// <summary>Distance from settlement origin to begin showing arrow pointing back to settlement origin</summary>
        public static decimal limitWarnDist = 300;
        public static bool showMapPixelOffsets = false;

        private FileSystemWatcher? mapImageWatcher;
        private FileSystemWatcher? templateWatcher;
        private DockingState dockingState = DockingState.none;
        private bool hasLanded = false;

        private FormBuilder? builder { get => FormBuilder.activeForm; }

        private HumanSiteData site { get => game.humanSite!; }

        private PlotHumanSite() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall;

            // these we get from current data
            this.siteOrigin = site.location;
            this.siteHeading = site.heading;

            if (site.location.Lat == 0 || site.location.Long == 0) Debugger.Break(); // Does this ever happen?

            if (!autoZoom) this.scale = 6;

            if (game.isMode(GameMode.OnFoot, GameMode.Docked, GameMode.InSrv, GameMode.Landed))
            {
                this.dockingState = DockingState.landed;
                this.hasLanded = true;
            }
        }

        public override bool allow { get => PlotHumanSite.allowPlotter; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.mapImageWatcher != null)
                {
                    this.mapImageWatcher.Changed -= MapImageWatcher_Changed;
                    this.mapImageWatcher = null;
                }
                if (this.templateWatcher != null)
                {
                    this.templateWatcher.Changed -= TemplateWatcher_Changed;
                    this.templateWatcher = null;
                }

            }

            base.Dispose(disposing);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.builder != null)
                this.builder.Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(500);
            this.Height = scaled(600);
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            this.loadMapImage();
        }

        /// <summary>
        /// Conditions when plotter can exist (but potentially hidden)
        /// </summary>
        public static bool keepPlotter
        {
            get => Game.settings.autoShowHumanSitesTest
                && Game.activeGame?.status != null
                && !Game.activeGame.atMainMenu
                && Game.activeGame.status.hasLatLong
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.humanSite != null;
        }

        /// <summary>
        /// Conditions when plotter can be visible
        /// </summary>
        public static bool allowPlotter
        {
            get => keepPlotter
                && Game.activeGame!.isMode(GameMode.OnFoot, GameMode.Flying, GameMode.Docked, GameMode.InSrv, GameMode.InTaxi, GameMode.Landed, GameMode.CommsPanel, GameMode.ExternalPanel, GameMode.GlideMode);
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            // revisit this (why does opacity matter?)
            if (this.Opacity > 0 && !PlotHumanSite.keepPlotter)
                Program.closePlotter<PlotHumanSite>(true);
            else if (this.Opacity > 0 && !PlotHumanSite.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotHumanSite.keepPlotter)
                this.reposition(Elite.getWindowRect());

            // update if the site recently gained it
            if (this.siteHeading == -1 && site?.heading >= 0)
                this.siteHeading = site.heading;

            // load missing map image if the template was recently set
            if (this.mapImage == null && site?.template != null)
                this.loadMapImage();

            // auto-adjust zoom?
            this.setZoom(newMode);
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;
            base.Status_StatusChanged(blink);

            // auto-adjust zoom?
            this.setZoom(game.mode);
        }

        private void setZoom(GameMode mode)
        {
            if (autoZoom)
            {
                if (game.status.OnFootInside || game.status.SelectedWeapon == "$humanoid_companalyser_name;")
                {
                    // zoom in LOTS if inside a building or using the Profile analyzer tool
                    this.scale = 6;
                }
                else if (mode == GameMode.OnFoot || game.status.OnFootOutside)
                {
                    // zoom in SOME if on foot outside
                    this.scale = 2;
                }
                else if (mode == GameMode.InSrv || mode == GameMode.Landed || mode == GameMode.Docked || mode == GameMode.Flying)
                {
                    // zoom out if in some vehicle
                    this.scale = 1;
                }
            }

            this.Invalidate();
        }

        private void loadMapImage()
        {
            // load a background image, if found
            try
            {
                if (game.humanSite?.template == null || game.humanSite.subType == -1) return;

                if (this.mapImageWatcher != null)
                    this.mapImageWatcher.EnableRaisingEvents = false;
                if (this.mapImage != null)
                    this.mapImage.Dispose();

                var folder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "images");
                var filepath = Directory.GetFiles(folder, $"{game.humanSite.economy}~{game.humanSite.subType}-*.png")?.FirstOrDefault();
                if (filepath == null) return;

                var nameParts = Path.GetFileNameWithoutExtension(filepath).Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length != 7 || nameParts[1] != "s" || nameParts[3] != "x" || nameParts[5] != "y")
                {
                    Game.log($"Bad filename format: {filepath}");
                    return;
                }
                this.mapScale = float.Parse(nameParts[2], CultureInfo.InvariantCulture);
                this.mapCenter = new Point(int.Parse(nameParts[4]), int.Parse(nameParts[6]));

                using (var fileImage = Image.FromFile(filepath))
                    this.mapImage = new Bitmap(fileImage);

                Game.log($"Loaded mapImage: {filepath}");

                if (this.mapImageWatcher == null)
                {
                    this.mapImageWatcher = new FileSystemWatcher(folder, Path.GetFileName(filepath));
                    this.mapImageWatcher.Changed += MapImageWatcher_Changed;
                    this.mapImageWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                }
                this.mapImageWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Game.log($"Failed to load/parse human settlement background map:\r\n{ex}");
                this.mapScale = 0;
                this.mapCenter = Point.Empty;
                this.mapImage = null;
            }
        }

        private void MapImageWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Game.log($"Map image changed, reloading...");

            Program.crashGuard(() =>
            {
                this.loadMapImage();
                this.Invalidate();
            });
        }

        protected override void onJournalEntry(DockingRequested entry)
        {
            base.onJournalEntry(entry);
            this.dockingState = DockingState.requested;
            this.Invalidate();
        }

        protected override void onJournalEntry(DockingGranted entry)
        {
            base.onJournalEntry(entry);
            this.dockingState = DockingState.approved;
            this.Invalidate();
        }
        protected override void onJournalEntry(DockingDenied entry)
        {
            base.onJournalEntry(entry);
            this.dockingState = DockingState.denied;
            this.Invalidate();
        }

        protected override void onJournalEntry(Docked entry)
        {
            base.onJournalEntry(entry);

            // TODO: show something if we are wanted?
            // if (entry.Wanted) { .. }

            this.hasLanded = true;
            this.dockingState = DockingState.landed;
            this.BeginInvoke(() =>
            {
                this.siteHeading = site.heading;
                this.Invalidate();
            });
        }

        protected override void onJournalEntry(Touchdown entry)
        {
            base.onJournalEntry(entry);
            this.hasLanded = true;
        }

        protected override void onJournalEntry(SupercruiseEntry entry)
        {
            base.onJournalEntry(entry);

            Program.closePlotter<PlotHumanSite>();
        }

        protected override void onJournalEntry(BackpackChange entry)
        {
            base.onJournalEntry(entry);
            if (this.site.template?.dataTerminals == null) return;
            if (entry.Added == null || !entry.Added.Any(_ => _.Type == "Data")) return;

            // find closest data terminal, mark it as "processed" (so it renders differently)
            var offset = Util.getOffset(radius, siteOrigin, siteHeading);
            var terminal = this.site.template.dataTerminals.FirstOrDefault(dt => (offset - new PointM(dt.offset)).dist < 5);
            if (terminal != null)
            {
                terminal.processed = true;
                this.Invalidate();
            }
        }

        private void loadTemplate()
        {
            Game.log("Loading humanSiteTemplates.json");
            PlotHumanSite.autoZoom = false;
            HumanSiteTemplate.import(true);
            Application.DoEvents();
            site.template = HumanSiteTemplate.get(game.humanSite!.economy, game.humanSite.subType);
            this.Invalidate();

            // start watching template file changes (if not already)
            if (this.templateWatcher == null)
            {
                var folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\.."));
                this.templateWatcher = new FileSystemWatcher(folder, "humanSiteTemplates.json");
                this.templateWatcher.Changed += TemplateWatcher_Changed;
                this.templateWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                this.templateWatcher.EnableRaisingEvents = true;
            }
        }

        protected override void onJournalEntry(SendText entry)
        {
            if (this.IsDisposed || game?.humanSite == null) return;

            var msg = entry.Message.ToLowerInvariant().Trim();
            if (msg.StartsWith("z "))
            {
                autoZoom = false;
                // (base class will handle setting the zoom level)
            }
            else if (msg == "z")
            {
                autoZoom = true;
                this.setZoom(game.mode);
            }

            base.onJournalEntry(entry);

            if (msg == MsgCmd.settlement && game.status.OnFootOutside)
            {
                // reset settlement sub-type and heading calculations
                this.site.inferSubtypeFromFoot(game.status.Heading);
                this.siteHeading = this.site.heading;
            }

            if (msg == "ll")
            {
                // force reload the template
                this.loadTemplate();
            }

            if (msg.StartsWith(MsgCmd.threat, StringComparison.OrdinalIgnoreCase))
            {
                // set the settlement threat level for surveys
                int threatLevel;
                if (int.TryParse(entry.Message.Substring(8), out threatLevel))
                {
                    Game.log($"threatLevel: {threatLevel}");
                    if (game.matStatsTracker != null)
                    {
                        game.matStatsTracker.threatLevel = threatLevel;
                        game.matStatsTracker.Save();
                    }
                }
            }

            if (msg == MsgCmd.edit)
            {
                // open settlement map editor
                this.loadTemplate();
                if (builder == null)
                    FormBuilder.show(this.site);
            }
            if (msg == MsgCmd.start)
            {
                // begin settlement mats survey
                game.initMats(site);
            }
            if (msg == MsgCmd.stop)
            {
                // end settlement mats survey
                game.exitMats(true);
            }

            this.Invalidate();
        }

        private void TemplateWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Game.log("TemplateWatcher_Changed");
            this.loadTemplate();
        }

        //private RectangleF getBldRectF(PointF bldCorner, PointF cmdrOffset, decimal bldHeading)
        //{
        //    var xx = cmdrOffset.X - bldCorner.X;
        //    var yy = cmdrOffset.Y - bldCorner.Y;

        //    var c = (decimal)Math.Sqrt(
        //        Math.Pow(bldCorner.X - cmdrOffset.X, 2)
        //        + Math.Pow(bldCorner.Y - cmdrOffset.Y, 2)
        //        );

        //    var aaa = Util.ToAngle(xx, yy) - (decimal)bldHeading;
        //    if (aaa < 0) aaa += 360;

        //    var dx = (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)aaa)) * c);
        //    var dy = (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)aaa)) * c);


        //    // when dx+ dy- (and nothing else)
        //    var dxx = bldCorner.X;
        //    var dyy = bldCorner.Y;

        //    // when dx- dy-
        //    if (dx < 0 && dy < 0)
        //    {
        //        dxx += xx;
        //        dyy += yy;
        //        dxx -= (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
        //        dyy -= (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
        //    }
        //    // when: dx+ dy+
        //    else if (dx > 0 && dy > 0)
        //    {
        //        dxx += (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
        //        dyy += (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
        //    }
        //    // when dx- dy+
        //    else if (dx < 0 && dy > 0)
        //    {
        //        dxx += xx;
        //        dyy += yy;
        //    }
        //    Game.log($"---");
        //    Game.log($"{dxx},{dyy} /{(int)aaa}°/ {dx},{dy}");

        //    if (dx < 0) dx *= -1;
        //    if (dy < 0) dy *= -1;
        //    var rf3 = new RectangleF(dxx, dyy, dx, dy);
        //    Game.log($"{rf3.Left},{rf3.Top} /{(int)aaa}°/ {rf3.Width},{rf3.Height}");

        //    return rf3;
        //}

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.site == null) return;

            // first, draw headers and footers (before we start clipping)

            // header - right
            var zoomText = $"Zoom: {this.scale.ToString("N1")}";
            if (autoZoom) zoomText += " (auto)";
            drawTextAt(this.ClientSize.Width - 8, zoomText, GameColors.brushGameOrangeDim, GameColors.fontSmall, true);

            // header - left
            var headerTxt = $"{site.name} - {site.economy} ";
            headerTxt += site.subType == 0 ? "??" : $"#{site.subType}";

            //var currentBld = site.template?.getCurrentBld(cmdrOffset, this.siteHeading);
            //if (currentBld != null) headerTxt += $", inside: {currentBld}";
            drawTextAt(eight, headerTxt);
            newLine(+ten, true);

            var distToSiteOrigin = Util.getDistance(site.location, Status.here, this.radius);
            if (game.isMode(GameMode.Flying, GameMode.GlideMode))
                this.drawOnApproach(distToSiteOrigin);
            else
                this.drawFooterAtSettlement();

            clipToMiddle();

            if (this.siteHeading >= 0)
            {
                // when we know the settlement type and heading
                this.drawKnownSettlement(distToSiteOrigin);
            }
            else if (this.site.heading == -1 && game.isMode(GameMode.Landed, GameMode.InSrv, GameMode.OnFoot))
            {
                // otherwise, show guidance to manually set it
                g.ResetTransform();
                drawTextAt(eight, $"Settlement type unknown. To identify:");
                newLine(+ten, true);
                drawTextAt(eight, $"► Crouch in middle of any landing pad");
                newLine(+ten, true);
                drawTextAt(eight, $"► Face the EXACT pad direction");
                newLine(+ten, true);
                drawTextAt(eight, $"► Send message '.settlement'");
            }
        }

        private void drawFooterAtSettlement()
        {
            // TODO: improve this?

            // footer - cmdr's location heading RELATIVE TO SITE
            var aaa = game.status.Heading - siteHeading; // TODO: replace `aaa` with `cmdrHeading`
            if (aaa < 0) aaa += 360;
            var footerTxt = $"x: {(int)cmdrOffset.X}m, y: {(int)cmdrOffset.Y}m, {aaa}°";

            // are we inside a building?
            var currentBld = site.template?.getCurrentBld(cmdrOffset, this.siteHeading);
            if (currentBld != null) footerTxt += $", inside: {currentBld}";

            // Pixel offset relative to background map image - an aide for creating map images
            if (showMapPixelOffsets && this.mapImage != null)
            {
                var pf = Util.getOffset(this.radius, site.location, Status.here.clone(), site.heading);
                // TODO: confirm this is actually identical to `cmdrOffset`
                pf.x += this.mapCenter.X;
                pf.y = this.mapImage.Height - pf.y - this.mapCenter.Y;
                footerTxt += $" (☍{(int)pf.X}, {(int)pf.Y})";
                // TODO: remove this? Isn't it the same as we already render?
            }

            if (game.matStatsTracker?.completed == false)
                footerTxt += ".";

            if (builder != null && builder.nextPath != null)
            {
                // if creating a new building in the editor ...
                var lastPoint = builder.lastPoint;
                var dx = lastPoint.X - builder.offset.X;
                var dy = lastPoint.Y - builder.offset.Y;
                var foo = (float)DecimalEx.ToDeg((decimal)Math.Atan2(dx, dy));
                if (foo < 0) foo += 360;
                footerTxt += $"! {foo.ToString("N1")}°";
            }

            g.DrawString(footerTxt, this.Font, GameColors.brushGameOrange, eight, this.ClientSize.Height - twenty);
        }

        private void drawKnownSettlement(decimal distToSiteOrigin)
        {
            // draw relative to site origin
            this.resetMiddleSiteOrigin();

            // For use with font based symbols, keeping them up-right no matter the site or cmdr heading
            var adjustedHeading = -site.heading + game.status.Heading;

            // draw background map and POIs
            this.drawMapImage();
            this.drawBuildings();
            this.drawLandingPads();
            this.drawSecureDoors();
            this.drawNamedPOI(adjustedHeading);
            this.drawDataTerminals(adjustedHeading);

            // draw/fill polygon for building being assembled in the editor
            if (this.builder != null)
                this.drawBuildingBox(cmdrOffset);

            this.drawCompassLines();

            // draw limit circle outside which ships/taxi's can be requested
            g.DrawEllipse(GameColors.HumanSite.penOuterLimit, -limitDist, -limitDist, limitDist * 2, limitDist * 2);

            // draw relative to cmdr's location ...
            this.resetMiddleRotated();
            this.drawShipAndSrvLocation();

            // draw relative to center of window ...
            this.resetMiddle();
            this.drawCommander();

            // draw large arrow pointing to settlement origin if >300m away
            if (distToSiteOrigin > limitWarnDist)
            {
                var td = new TrackingDelta(this.radius, site.location);
                g.RotateTransform(180f - game.status.Heading + (float)td.angle);
                g.DrawLine(GameColors.HumanSite.penOuterLimitWarningArrow, 0, 20, 0, 100);
                g.DrawLine(GameColors.HumanSite.penOuterLimitWarningArrow, 0, 100, 20, 80);
                g.DrawLine(GameColors.HumanSite.penOuterLimitWarningArrow, 0, 100, -20, 80);
            }

            // show mat collection points on top of everything else
            this.resetMiddleSiteOrigin();
            this.drawCollectedMats();
        }

        private void drawLandingPads()
        {
            if (game?.humanSite?.template == null) return;

            foreach (var pad in game.humanSite.template.landingPads)
            {
                adjust(pad.offset, pad.rot, () =>
                {
                    RectangleF r;
                    if (pad.size == LandingPadSize.Small) // 50 x 70
                        r = new RectangleF(-25, -35, 50, 70);
                    else if (pad.size == LandingPadSize.Medium) // 70 x 135
                        r = new RectangleF(-35, -67.5f, 70, 135);
                    else // if (pad.size == LandingPadSize.Large) // 90 x 170
                        r = new RectangleF(-45, -85, 90, 170);

                    g.DrawRectangle(GameColors.HumanSite.landingPad.pen, r);
                    g.DrawLine(GameColors.HumanSite.landingPad.pen, r.Left, r.Bottom, 0, r.Top);
                    g.DrawLine(GameColors.HumanSite.landingPad.pen, r.Right, r.Bottom, 0, r.Top);
                    //g.DrawEllipse(GameColors.HumanSite.penLandingPad, -5, -5, 10, 10);
                    g.DrawEllipse(GameColors.HumanSite.landingPad.pen, -10, -10, 20, 20);
                    //g.DrawEllipse(GameColors.HumanSite.penLandingPad, -20, -20, 40, 40);

                    // show pad # in corner
                    var idx = game.humanSite.template.landingPads.IndexOf(pad) + 1;
                    g.DrawString($"{idx}", GameColors.fontSmall, GameColors.HumanSite.landingPad.brush, r.Left + four, r.Top + four);
                });
            }

        }

        private void drawBuildings()
        {
            if (game.humanSite?.template?.buildings == null) return;

            // draw buildings from template
            foreach (var bld in site.template!.buildings)
            {
                var bb = Brushes.SaddleBrown;

                if (bld.name.StartsWith("HAB", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.HAB; // green
                else if (bld.name.StartsWith("CMD", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.CMD; // red
                else if (bld.name.StartsWith("POW", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.POW; // purple
                else if (bld.name.StartsWith("EXT", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.EXT; // blue
                else if (bld.name.StartsWith("STO", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.STO; // brown
                else if (bld.name.StartsWith("RES", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.STO; // darker brown?
                else if (bld.name.StartsWith("IND", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.IND; // lighter brown
                else if (bld.name.StartsWith("MED", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.MED; // grey
                else if (bld.name.StartsWith("LAB", StringComparison.OrdinalIgnoreCase))
                    bb = GameColors.Building.LAB; // dark green

                foreach (var p in bld.paths)
                    g.FillPath(bb, p);
            }
        }

        private void drawBuildingBox(PointF offset)
        {
            var of2 = offset;
            of2.Y *= -1;

            if (this.builder == null) return;

            // draw pending buildings
            foreach (var p in builder.building.paths)
                g.FillPath(Brushes.Navy, p);

            // draw in-progress buildigs
            if (builder.circle)
            {
                var rf = new RectangleF(of2, new SizeF(builder.circleRadius * 2, builder.circleRadius * 2));
                rf.Offset(-builder.circleRadius, -builder.circleRadius);
                //rf.Inflate(builder.circleRadius * 2, builder.circleRadius * 2);

                g.FillEllipse(Brushes.SlateBlue, rf);
            }
            else if (builder.nextPath != null)
            {
                g.FillPath(Brushes.SlateBlue, builder.nextPath);
                g.DrawPath(Pens.LightBlue, builder.nextPath);

                if (builder.lastPoint != PointF.Empty)
                    g.DrawLine(Pens.Yellow, builder.lastPoint, builder.offset);
            }
        }

        private void drawSecureDoors()
        {
            if (game.humanSite?.template?.secureDoors == null) return;

            foreach (var door in game.humanSite.template.secureDoors)
            {
                adjust(door.offset, door.rot, () =>
                {
                    // adjust the colour according to security level
                    var b = GameColors.HumanSite.siteLevel[door.level].brush;

                    g.FillRectangle(b, -3, -2, 6, 3);
                    g.DrawRectangle(GameColors.HumanSite.penDoorEdge, -3, -2, 6, 3);
                });
            }
        }

        private void drawNamedPOI(float adjustedHeading)
        {
            if (game.humanSite?.template?.namedPoi == null) return;

            foreach (var poi in game.humanSite.template.namedPoi)
            {
                adjust(poi.offset, adjustedHeading, () =>
                {
                    var x = 0f;
                    var y = 0f;

                    var b = GameColors.HumanSite.siteLevel[poi.level].brush;
                    var p = GameColors.HumanSite.siteLevel[poi.level].pen;

                    // ❗❕❉✪✿❤➊➀⟐𖺋𖺋⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳☢
                    // ꊢ ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼
                    // ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍
                    // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 

                    if (poi.name == "Atmos")
                    {
                        g.DrawString("⚴", GameColors.Fonts.typewriter_p6, b, -2, -3);
                        x = 0.65f; y = -3.7f;
                    }
                    else if (poi.name == "Alarm")
                    {
                        g.DrawString("%", GameColors.Fonts.wingdings_4B, b, -4, -3);
                        x = -0.8f; y = -4.4f;
                    }
                    else if (poi.name == "Auth")
                    {
                        g.DrawString("⧌", GameColors.Fonts.typewriter_p6, b, -3.3f, -4f);
                        x = 0.2f; y = -4.5f;
                    }
                    else if (poi.name == "Medkit")
                    {
                        g.DrawString("♥", GameColors.Fonts.typewriter_p4, b, -2, -3);
                        x = 0.1f; y = -5f;
                    }
                    else if (poi.name == "Battery")
                    {
                        g.DrawString("+", GameColors.Fonts.typewriter_p6, b, -3f, -4.4f);
                        x = 0.0f; y = -4.5f;
                    }
                    else if (poi.name == "Power")
                    {
                        // power symbol is always blue
                        b = GameColors.HumanSite.powerIcon.brush;
                        p = GameColors.HumanSite.powerIcon.pen;

                        g.DrawString("⭍", GameColors.fontSmall, b, -4, -6);
                        x = 2.5f; y = -6f;
                    }
                    else
                    {
                        // we're not sure what POI this is
                        g.DrawString("▨", GameColors.Fonts.typewriter_p4, b, -2, -2);
                        x = -0.3f; y = -3f;
                    }

                    // draw chevrons to indicate upstairs
                    if (poi.floor == 3)
                    {
                        g.DrawLine(p, x, y, x + 1.5f, y + 1.5f);
                        g.DrawLine(p, x, y, x - 1.5f, y + 1.5f);
                    }
                    if (poi.floor >= 2)
                    {
                        g.DrawLine(p, x, y + 1, x + 1.5f, y + 2.5f);
                        g.DrawLine(p, x, y + 1, x - 1.5f, y + 2.5f);
                    }
                });
            }
        }

        private void drawDataTerminals(float adjustedHeading)
        {
            if (game.humanSite?.template?.dataTerminals == null) return;

            foreach (var terminal in game.humanSite.template.dataTerminals)
            {
                adjust(terminal.offset, adjustedHeading, () =>
                {
                    var b = terminal.processed
                        ? GameColors.HumanSite.siteLevel[-1].brush
                        : GameColors.HumanSite.siteLevel[terminal.level].brush;

                    var p = terminal.processed
                        ? GameColors.HumanSite.siteLevel[-1].pen
                        : GameColors.HumanSite.siteLevel[terminal.level].pen;

                    g.DrawString("/", GameColors.Fonts.wingdings2_2B, b, -2.5f, -3);
                    var x = 0.25f; var y = -5.4f;

                    // draw chevrons to indicate upstairs
                    if (terminal.floor == 3)
                    {
                        g.DrawLine(p, x, y, x + 1.5f, y + 1.5f);
                        g.DrawLine(p, x, y, x - 1.5f, y + 1.5f);
                    }
                    if (terminal.floor >= 2)
                    {
                        g.DrawLine(p, x, y + 1, x + 1.5f, y + 2.5f);
                        g.DrawLine(p, x, y + 1, x - 1.5f, y + 2.5f);
                    }
                });
            }
        }

        private void drawCollectedMats()
        {
            if (game.matStatsTracker?.matLocations != null && Game.settings.showMatsCollectionDots)
            {
                foreach (var entry in game.matStatsTracker.matLocations)
                {
                    g.FillEllipse(Brushes.Black, entry.x - 0.5f, -entry.y - 0.5f, 1f, 1f);
                    g.DrawEllipse(GameColors.penCollectedMatDot, entry.x - 0.5f, -entry.y - 0.5f, 1f, 1f);
                }
            }
        }

        private void drawOnApproach(decimal distToSiteOrigin)
        {
            // distance to site, triangulated with altitude
            var d = new PointM(distToSiteOrigin, game.status.Altitude).dist;
            if (this.hasLanded)
                drawTextAt(eight, $"► departing: {Util.metersToString(d)} from settlement ...", GameColors.fontMiddle);
            else
                drawTextAt(eight, $"► on approach: {Util.metersToString(d)} to settlement ...", GameColors.fontMiddle);
            newLine(+ten, true);

            // docking status?
            if (this.dockingState == DockingState.requested || this.dockingState == DockingState.approved || this.dockingState == DockingState.denied)
            {
                drawTextAt(eight, $"► docking requested ...", GameColors.fontMiddle);
                newLine(+ten, true);
            }
            if (this.dockingState == DockingState.denied)
            {
                drawTextAt(eight, $"► docking denied ...", GameColors.fontMiddle);
                newLine(+ten, true);
            }
            if (this.dockingState == DockingState.approved)
            {
                drawTextAt(eight, $"► docking approved: pad #{site.targetPad} ...", GameColors.fontMiddle);
                newLine(+ten, true);
            }
        }
    }

    enum DockingState
    {
        none,
        requested,
        denied,
        approved,
        landed,
    }
}
