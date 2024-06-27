using DecimalMath;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace SrvSurvey
{
    internal class PlotHumanSite : PlotBaseSite, PlotterForm
    {
        public static bool autoZoom = true;

        private FileSystemWatcher? mapImageWatcher;
        private FileSystemWatcher? templateWatcher;
        private DockingState dockingState = DockingState.none;
        private bool hasLanded = false;
        private PointF buildingCorner;
        private float buildingHeading;
        private FormBuilder? builder { get => FormBuilder.activeForm; }

        private HumanSiteData site { get => game.humanSite!; }

        private PlotHumanSite() : base()
        {
            this.Width = scaled(500);
            this.Height = scaled(600);
            this.Font = GameColors.fontSmall;

            // these we get from current data
            this.siteOrigin = site.location;
            this.siteHeading = site.heading;

            if (!autoZoom) this.scale = 6;

            if (game.isMode(GameMode.OnFoot, GameMode.Docked, GameMode.InSrv, GameMode.Landed))
                dockingState = DockingState.landed;
        }

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
            base.OnLoad(e);

            this.initialize();
            this.reposition(Elite.getWindowRect(true));

            this.loadMapImage();
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = PlotPos.getOpacity(this);
            PlotPos.reposition(this, gameRect);

            this.Invalidate();
        }

        public static bool keepPlotter
        {
            get => Game.activeGame?.status != null
                && Game.settings.autoShowHumanSitesTest
                && !Game.activeGame.atMainMenu
                && Game.activeGame.status.hasLatLong
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.humanSite != null;
        }

        public static bool allowPlotter
        {
            get => keepPlotter
                && Game.activeGame?.atMainMenu == false
                && Game.activeGame!.isMode(GameMode.OnFoot, GameMode.Flying, GameMode.Docked, GameMode.InSrv, GameMode.InTaxi, GameMode.Landed, GameMode.CommsPanel, GameMode.ExternalPanel, GameMode.GlideMode);
        }

        protected override void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !PlotHumanSite.keepPlotter)
                Program.closePlotter<PlotHumanSite>(true);
            else if (this.Opacity > 0 && !PlotHumanSite.allowPlotter)
                this.Opacity = 0;
            else if (this.Opacity == 0 && PlotHumanSite.keepPlotter)
                this.reposition(Elite.getWindowRect());

            if (this.siteHeading == -1 && site?.heading >= 0)
                this.siteHeading = site.heading;

            this.setZoom(newMode);

            if (this.mapImage == null && site?.template != null)
                this.loadMapImage();

            if (newMode == GameMode.MainMenu)
                this.Close();
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;
            base.Status_StatusChanged(blink);

            // so we close once exceeding a certain altitude
            if (this.Opacity > 0 && !PlotHumanSite.keepPlotter)
                Program.closePlotter<PlotHumanSite>(true);

            if ((game.status.SelectedWeapon == "$humanoid_companalyser_name;" || game.status.OnFootInside) && autoZoom)
                this.scale = 6;
            else if (game.status.OnFootOutside && autoZoom)
                this.scale = 2;

            this.Invalidate();
        }

        private void setZoom(GameMode newMode)
        {
            if (newMode == GameMode.OnFoot && autoZoom)
                this.scale = 2;
            if ((newMode == GameMode.InSrv || newMode == GameMode.Landed || newMode == GameMode.Docked || newMode == GameMode.Landed) && autoZoom)
                this.scale = 1;

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

            var offset = Util.getOffset(radius, siteOrigin, siteHeading);

            // find closest data terminal
            HumanSitePoi2? terminal = null;
            var closest = 1000m;
            foreach (var dt in this.site.template.dataTerminals)
            {
                var dist = (offset - new PointM(dt.offset)).dist;
                if (dist < closest) // TODO: make it first <5 m
                {
                    terminal = dt;
                    closest = dist;
                }
            }

            if (terminal == null) return;

            terminal.processed = true;
            this.Invalidate();
        }

        private void loadTemplate()
        {
            Game.log("Loading humanSiteTemplates.json");
            PlotHumanSite.autoZoom = false;
            HumanSiteTemplate.import(true);
            Application.DoEvents();
            site.template = HumanSiteTemplate.get(game.humanSite!.economy, game.humanSite.subType);
            this.Invalidate();

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
            if (msg.StartsWith("z ")) autoZoom = false;
            if (msg == ".az") autoZoom = !autoZoom;

            base.onJournalEntry(entry);

            if (msg == "ll")
            {
                this.loadTemplate();
            }

            if (msg.StartsWith(MsgCmd.threat, StringComparison.OrdinalIgnoreCase))
            {
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
            else if (msg == MsgCmd.settlement)
            {
                this.site.inferSubtypeFromFoot(game.status.Heading);
            }

            if (msg == ";;")
            {
                this.loadTemplate();
                if (builder == null)
                    FormBuilder.show(this.site);
            }

            if (msg == ".stop")
            {
                game.exitMats(true);
            }
            if (msg == ".start")
            {
                game.initMats(site);
            }
            this.Invalidate();
        }

        private void TemplateWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.loadTemplate();
        }

        private RectangleF getBldRectF(PointF bldCorner, PointF cmdrOffset, decimal bldHeading)
        {
            var xx = cmdrOffset.X - bldCorner.X;
            var yy = cmdrOffset.Y - bldCorner.Y;

            var c = (decimal)Math.Sqrt(
                Math.Pow(bldCorner.X - cmdrOffset.X, 2)
                + Math.Pow(bldCorner.Y - cmdrOffset.Y, 2)
                );

            var aaa = Util.ToAngle(xx, yy) - (decimal)bldHeading;
            if (aaa < 0) aaa += 360;

            var dx = (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)aaa)) * c);
            var dy = (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)aaa)) * c);


            // when dx+ dy- (and nothing else)
            var dxx = bldCorner.X;
            var dyy = bldCorner.Y;

            // when dx- dy-
            if (dx < 0 && dy < 0)
            {
                dxx += xx;
                dyy += yy;
                dxx -= (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
                dyy -= (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
            }
            // when: dx+ dy+
            else if (dx > 0 && dy > 0)
            {
                dxx += (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
                dyy += (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)bldHeading)) * (decimal)dy);
            }
            // when dx- dy+
            else if (dx < 0 && dy > 0)
            {
                dxx += xx;
                dyy += yy;
            }
            Game.log($"---");
            Game.log($"{dxx},{dyy} /{(int)aaa}°/ {dx},{dy}");

            if (dx < 0) dx *= -1;
            if (dy < 0) dy *= -1;
            var rf3 = new RectangleF(dxx, dyy, dx, dy);
            Game.log($"{rf3.Left},{rf3.Top} /{(int)aaa}°/ {rf3.Width},{rf3.Height}");

            return rf3;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed || this.site == null) return;

            try
            {
                resetPlotter(g);

                var zoomText = $"Zoom: {this.scale.ToString("N1")}";
                if (autoZoom) zoomText += " (auto)";
                drawTextAt(this.ClientSize.Width - 8, zoomText, GameColors.brushGameOrangeDim, GameColors.fontSmall, true);

                // header
                var headerTxt = $"{site.name} - {site.economy} ";
                headerTxt += site.subType == 0 ? "??" : $"#{site.subType}";

                var offset = (PointF)Util.getOffset(radius, siteOrigin, siteHeading);
                var currentBld = site.template?.getCurrentBld(offset, this.siteHeading);
                if (currentBld != null)
                    headerTxt += $" - Building: {currentBld}";

                drawTextAt(eight, headerTxt);
                newLine(+ten, true);

                var dg = Util.getDistance(site.location, Status.here, this.radius);

                if (game.isMode(GameMode.Flying, GameMode.GlideMode))
                {
                    this.drawOnApproach(dg);
                }
                else
                {
                    // footer
                    var aaa = game.status.Heading - siteHeading;
                    if (aaa < 0) aaa += 360;
                    var footerTxt = $"cmdr offset: x: {(int)offset.X}m, y: {(int)offset.Y}m, {aaa}°";
                    if (this.mapImage != null)
                    {
                        var pf = Util.getOffset(this.radius, site.location, Status.here.clone(), site.heading);
                        pf.x += this.mapCenter.X;
                        pf.y = this.mapImage.Height - pf.y - this.mapCenter.Y;
                        footerTxt += $" (☍{(int)pf.X}, {(int)pf.Y})";
                    }
                    //if (buildingCorner != null)
                    //{
                    //    var xx = offset.X - buildingCorner.X;
                    //    var yy = offset.Y - buildingCorner.Y;

                    //    var aaa = Util.ToAngle(xx, yy) - (decimal)buildingHeading;
                    //    if (aaa < 0) aaa += 360;

                    //    var c = (decimal)Math.Sqrt(
                    //        Math.Pow(buildingCorner.X - offset.X, 2)
                    //        + Math.Pow(buildingCorner.Y - offset.Y, 2)
                    //        );

                    //    var dx = (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)aaa)) * c);
                    //    var dy = (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)aaa)) * c);

                    //    footerTxt += $" [ {dx.ToString("n1")} , {dy.ToString("n1")} ]";
                    //}
                    if (game.matStatsTracker?.completed == false)
                        footerTxt += ".";

                    if (builder != null && builder.nextPath != null)
                    {
                        var dx = builder.lastPoint.X - builder.offset.X;
                        var dy = builder.lastPoint.Y - builder.offset.Y;
                        var foo = (float)DecimalEx.ToDeg((decimal)Math.Atan2(dx, dy)); // - siteHeading;
                        if (foo < 0) foo += 360;

                        footerTxt += $"! {foo.ToString("N1")}°";
                        //var pm = new PointM(builder.offset.X - builder.lastPoint.X, builder.offset.Y - builder.lastPoint.Y );
                        //footerTxt += $"! {pm.angle.ToString("N1")}°";
                    }

                    this.drawFooterText(footerTxt);
                }

                clipToMiddle();
                // relative to site origin
                this.resetMiddleSiteOrigin();

                if (this.site.heading >= 0)
                {
                    // draw map and POIs
                    this.drawMapImage();
                    this.drawBuildings(offset);
                    this.drawLandingPads();
                    this.drawPOI(offset);

                    // draw bounding box for pending building
                    //if (this.buildingCorner.X != 0)
                    if (this.builder != null)
                        this.drawBuildingBox(offset);
                }

                this.drawCompassLines();

                // draw limit - outside which ships/taxi's an be requested
                var zz = 500;
                g.DrawEllipse(GameColors.newPen(Color.FromArgb(64, Color.Gray), 8, DashStyle.DashDotDot), -zz, -zz, zz * 2, zz * 2);

                // relative to cmdr ...
                this.resetMiddleRotated();
                this.drawShipAndSrvLocation();

                this.resetMiddle();
                this.drawCommander();

                // draw large arrow pointing to settlement origin if >300m away
                if (dg > 300)
                {
                    var td = new TrackingDelta(this.radius, site.location);
                    g.RotateTransform(180f - game.status.Heading + (float)td.angle);
                    g.DrawLine(GameColors.penGameOrangeDim4, 0, 20, 0, 100);
                    g.DrawLine(GameColors.penGameOrangeDim4, 0, 100, 20, 80);
                    g.DrawLine(GameColors.penGameOrangeDim4, 0, 100, -20, 80);
                }

                if (this.site.heading == -1 && game.isMode(GameMode.Landed, GameMode.InSrv, GameMode.OnFoot))
                {
                    g.ResetTransform();
                    drawTextAt(eight, $"Settlement type unknown. To fix:");
                    newLine(+ten, true);
                    drawTextAt(eight, $"► Crouch in middle of any landing pad");
                    newLine(+ten, true);
                    drawTextAt(eight, $"► Face the right direction");
                    newLine(+ten, true);
                    drawTextAt(eight, $"► Send message '.settlement'");
                }

                // show mat collection points on top of everything else
                this.resetMiddleSiteOrigin();
                this.drawCollectedMats();
            }
            catch (Exception ex)
            {
                Game.log($"PlotHumanSite.OnPaintBackground error: {ex}");
            }
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

                    g.DrawRectangle(Pens.SlateGray, r);
                    g.DrawLine(Pens.SlateGray, r.Left, r.Bottom, 0, r.Top);
                    g.DrawLine(Pens.SlateGray, r.Right, r.Bottom, 0, r.Top);
                    //g.DrawEllipse(Pens.SlateGray, -5, -5, 10, 10);
                    g.DrawEllipse(Pens.SlateGray, -10, -10, 20, 20);
                    //g.DrawEllipse(Pens.SlateGray, -20, -20, 40, 40);

                    // show pad # in corner
                    var idx = game.humanSite.template.landingPads.IndexOf(pad) + 1;
                    g.DrawString($"{idx}", GameColors.fontSmall, Brushes.SlateGray, r.Left + four, r.Top + four);
                });
            }

        }

        private void drawBuildings(PointF offset)
        {
            if (game.humanSite?.template?.buildings == null) return;
            // draw commited buildings
            if (site.template?.buildings != null)
                foreach (var bld in site.template!.buildings)
                    foreach (var p in bld.paths)
                        g.FillPath(Brushes.SaddleBrown, p);

            //if (game.humanSite?.template?.buildings == null) return;
            //var bb = Brushes.YellowGreen;
            //foreach (var bld in game.humanSite.template.buildings)
            //{
            //    if (bld.name.StartsWith("HAB", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 0, 32, 0)); // green
            //    else if (bld.name.StartsWith("CMD", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 48, 0, 0)); // red
            //    else if (bld.name.StartsWith("POW", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 32, 32, 64)); // purple
            //    else if (bld.name.StartsWith("EXT", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 0, 0, 72)); // blue
            //    else if (bld.name.StartsWith("STO", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 50, 30, 20)); // brown
            //    else if (bld.name.StartsWith("IND", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 60, 40, 10)); // brown
            //    else if (bld.name.StartsWith("MED", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 32, 32, 32)); // grey
            //    else if (bld.name.StartsWith("LAB", StringComparison.OrdinalIgnoreCase))
            //        bb = new SolidBrush(Color.FromArgb(255, 0, 32, 32)); // dark green
            //    else
            //        bb = Brushes.YellowGreen;

            //    adjust(bld.rect.X, bld.rect.Y, bld.rot, () =>
            //    {
            //        g.FillRectangle(bb, 0, 0, bld.rect.Width, bld.rect.Height);
            //    });
            //}
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

            //var rr = new Region(gp);
            //g.FillRegion(Brushes.Navy, rr);

            //g.DrawPath(Pens.Yellow, this.gp);

            // --- OLDER ---

            //if (this.buildingCorner.X == 0) return;

            //g.DrawLine(Pens.Salmon, buildingCorner.X, -buildingCorner.Y, offset.X, -offset.Y);
            //g.DrawEllipse(Pens.Salmon, buildingCorner.X - 2, -buildingCorner.Y - 2, 4, 4);

            //var rf = getBldRectF(buildingCorner, offset, (decimal)buildingHeading);
            //adjust(rf.Left, rf.Top, buildingHeading, () =>
            //{
            //    g.DrawRectangle(Pens.Yellow, 0, 0, rf.Width, rf.Height);
            //});

            // --- OLDER ---

            //var o = (PointF)Util.getOffset(radius, siteOrigin, siteHeading);

            //var xx = o.X - buildingCorner1.X;
            ////xx = buildingCorner1.X - o.X;
            //var yy = o.Y - buildingCorner1.Y;
            ////yy = buildingCorner1.Y - o.Y;

            //var c = (decimal)Math.Sqrt(
            //    Math.Pow(buildingCorner1.X - o.X, 2)
            //    + Math.Pow(buildingCorner1.Y - o.Y, 2)
            //    );

            ////var aa = DecimalEx.ToDeg((decimal) Math.Atan2(buildingCorner1.Y - o.Y, buildingCorner1.X - o.X));
            //// buildingHeading - siteHeading
            //var hh = buildingHeading; // game.status.Heading; // + buildingHeading;
            //var hh2 = buildingHeading; // siteHeading + buildingHeading; // game.status.Heading; // + buildingHeading;
            //var aaa = Util.ToAngle(xx, yy) - (decimal)buildingHeading;
            //if (aaa < 0) aaa += 360;

            //var p2 = Util.rotateLine((decimal)aaa, c);
            //var dx = (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)aaa)) * c);
            //var dy = (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)aaa)) * c);


            ////g.DrawLine(Pens.Yellow, 0, 0, 0, -20);

            //var rf2 = Util.getRectF(
            //    this.buildingCorner1,
            //    new PointF(buildingCorner1.X + dx, buildingCorner1.Y + dy));
            ////rf2 = new RectangleF(buildingCorner1.X, buildingCorner1.Y, dx, dy);
            ////g.DrawLine(Pens.Lime, rf2.Left, -rf2.Top, rf2.Width, rf2.Height);
            //g.DrawLine(Pens.Salmon, buildingCorner1.X, -buildingCorner1.Y, o.X, -o.Y);
            //g.DrawEllipse(Pens.Salmon, buildingCorner1.X - 2, -buildingCorner1.Y - 2, 4, 4);

            //// when dx+ dy- (and nothing else)
            //var dxx = buildingCorner1.X;
            //var dyy = buildingCorner1.Y;

            //// when dx- dy-
            //if (dx < 0 && dy < 0)
            //{
            //    dxx += xx;
            //    dyy += yy;
            //    dxx -= (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)buildingHeading)) * (decimal)dy);
            //    dyy -= (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)buildingHeading)) * (decimal)dy);
            //}
            //// when: dx+ dy+
            //else if (dx > 0 && dy > 0)
            //{
            //    dxx += (float)(DecimalEx.Sin(DecimalEx.ToRad((decimal)buildingHeading)) * (decimal)dy);
            //    dyy += (float)(DecimalEx.Cos(DecimalEx.ToRad((decimal)buildingHeading)) * (decimal)dy);
            //}
            //// when dx- dy+
            //else if (dx < 0 && dy > 0)
            //{
            //    dxx += xx;
            //    dyy += yy;
            //}

            //Game.log($"{xx},{yy} /{(int)aaa}°/ {dx},{dy} \\ {p2}");
            //adjust(dxx, dyy, buildingHeading, () =>
            //{
            //    g.DrawRectangle(Pens.Brown, 0, 0, rf2.Width, rf2.Height);
            //    g.DrawLine(Pens.Blue, 0f, 0f, (float)dx, 0);
            //    g.DrawLine(Pens.Blue, 0f, 0f, 0, -(float)dy);

            //    //g.DrawLine(Pens.Black, 0f, 0f, (float)p2.X, -(float)p2.Y);
            //    //g.DrawLine(Pens.Cyan, 0f, 0f, 0, (float)p2.Y);
            //    g.DrawLine(Pens.Yellow, 0, 0, 0, -4);

            //});
        }

        private void drawPOI(PointF offset)
        {
            if (game?.humanSite?.template == null) return;
            var wd2b = new Font("Wingdings 2", 4F, FontStyle.Bold, GraphicsUnit.Point);
            var wd6 = new Font("Wingdings", 6F, FontStyle.Bold, GraphicsUnit.Point);
            var wd4 = new Font("Wingdings", 4F, FontStyle.Bold, GraphicsUnit.Point);
            var fs4 = new Font("Lucida Sans Typewriter", 4F, FontStyle.Regular, GraphicsUnit.Pixel);
            var fs6 = new Font("Lucida Sans Typewriter", 6F, FontStyle.Regular, GraphicsUnit.Pixel);
            var b0 = Brushes.Green;
            var b1 = Brushes.SkyBlue;
            var b2 = Brushes.DarkOrange;
            var b3 = (Brush)new SolidBrush(Color.FromArgb(200, Color.Red));

            var p0 = new Pen(Color.Green, 0.5f) { EndCap = LineCap.Triangle, StartCap = LineCap.Triangle };
            var p1 = new Pen(Color.SkyBlue, 0.5f) { EndCap = LineCap.Triangle, StartCap = LineCap.Triangle };
            var p2 = new Pen(Color.DarkOrange, 0.5f) { EndCap = LineCap.Triangle, StartCap = LineCap.Triangle };
            var p3 = new Pen(Color.Red, 0.5f) { EndCap = LineCap.Triangle, StartCap = LineCap.Triangle };


            if (game.humanSite.template.secureDoors != null)
            {
                foreach (var door in game.humanSite.template.secureDoors)
                {
                    adjust(door.offset, door.rot, () =>
                    {
                        // adjust the colour according to security level
                        var b = b0;
                        if (door.level == 1) b = b1;
                        if (door.level == 2) b = b2;
                        if (door.level == 3) b = b3;

                        g.FillRectangle(b, -3, -2, 6, 3);
                        g.DrawRectangle(Pens.Navy, -3, -2, 6, 3);
                    });
                }
            }

            if (game.humanSite.template.namedPoi != null)
            {
                foreach (var poi in game.humanSite.template.namedPoi)
                {
                    // site.heading - game.status.Heading
                    adjust(poi.offset, -site.heading + game.status.Heading, () =>
                    {
                        var x = 0f;
                        var y = 0f;

                        var b = b0;
                        if (poi.level == 1) b = b1;
                        if (poi.level == 2) b = b2;
                        if (poi.level == 3) b = b3;

                        var p = p0;
                        if (poi.level == 1) p = p1;
                        if (poi.level == 2) p = p2;
                        if (poi.level == 3) p = p3;

                        // ❗❕❉✪✿❤➊➀⟐𖺋𖺋⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳☢
                        //string txt = "ꊢ"; // ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼"; ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍

                        if (poi.name == "Atmos")
                        {
                            g.DrawString("⚴", fs6, b, -2, -3);
                            //
                            //g.DrawString("⚴", GameColors.fontSmall, b, -4, -6);
                            x = 0.65f;
                            y = -3.7f;
                        }
                        else if (poi.name == "Alarm")
                        {
                            g.DrawString("%", wd4, b, -4, -3);
                            x = -0.8f;
                            y = -4.4f;
                        }
                        else if (poi.name == "Auth")
                        {
                            g.DrawString("⧌", fs6, b, -3.3f, -4f);
                            x = 0.2f;
                            y = -4.5f;
                        }
                        else if (poi.name == "Medkit")
                        {
                            g.DrawString("♥", fs4, b, -2, -3);
                            x = 0.1f;
                            y = -5f;
                        }
                        else if (poi.name == "Battery")
                        {
                            g.DrawString("+", fs6, b, -3f, -4.4f);
                            x = 0.0f;
                            y = -4.5f;
                        }
                        else if (poi.name == "Power")
                        {
                            // power symbol is always blue
                            b = Brushes.Cyan;
                            p = new Pen(Color.Cyan, 0.5f) { EndCap = LineCap.Triangle, StartCap = LineCap.Triangle };

                            g.DrawString("⭍", GameColors.fontSmall, b, -4, -6);
                            x = 2.5f;
                            y = -6;
                        }
                        else
                        {
                            g.DrawString("▨", fs4 /*GameColors.fontSmall*/, b, -2, -2);
                            x = -0.3f;
                            y = -3f;

                            //
                            //string txt = "⛉⟑";
                            //switch (poi.name)
                            //{
                            //    case "Power": txt = "⭍"; break;
                            //    case "Alarm": txt = "♨"; break;
                            //    case "Auth": txt = "⨻"; break;
                            //    case "Atmos": txt = "⦚"; break;
                            //}

                            // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗  // -8 ??
                            //g.DrawString(txt, GameColors.fontSmall, Brushes.Yellow, -4, -6);
                            //g.DrawEllipse(Pens.Yellow, -5, -5, 10, 10);
                            //return;
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

            if (game.humanSite.template.dataTerminals != null)
            {
                foreach (var terminal in game.humanSite.template.dataTerminals)
                {
                    // site.heading - game.status.Heading
                    adjust(terminal.offset, -site.heading + game.status.Heading, () =>
                    {
                        var b = b0;
                        if (terminal.level == 1) b = b1;
                        if (terminal.level == 2) b = b2;
                        if (terminal.level == 3) b = b3;
                        if (terminal.processed) b = Brushes.Gray;

                        var p = p0;
                        if (terminal.level == 1) p = p1;
                        if (terminal.level == 2) p = p2;
                        if (terminal.level == 3) p = p3;
                        if (terminal.processed) p = Pens.Gray;

                        // ❗❕❉✪✿❤➊➀⟐𖺋𖺋⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳
                        //string txt = "ꊢ"; // ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼"; ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍

                        // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 
                        //g.DrawString(txt, GameColors.fontSmall, Brushes.Yellow, -4, -6);

                        // :;<=
                        g.DrawString("/", wd2b, b, -2.5f, -3);
                        var x = 0.25f;
                        var y = -5.4f;

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
                        //g.DrawString("^", GameColors.fontSmall, b, +2, -6);
                        //g.DrawEllipse(Pens.Yellow, -5, -5, 10, 10);
                    });
                }
            }
        }

        private void drawCollectedMats()
        {
            if (game.matStatsTracker?.matLocations != null) // TODO: hide with a setting?
            {
                //game.matStatsTracker.matLocations.Clear();
                //game.matStatsTracker.countBuildings.Clear();
                //game.matStatsTracker.countMats.Clear();
                //game.matStatsTracker.countTypes.Clear();
                //game.matStatsTracker.Save();
                var pp = new Pen(Color.Yellow, 0.2f);

                foreach (var entry in game.matStatsTracker.matLocations)
                {
                    g.FillEllipse(Brushes.Black, entry.x - 0.5f, -entry.y - 0.5f, 1f, 1f);
                    g.DrawEllipse(pp, entry.x - 0.5f, -entry.y - 0.5f, 1f, 1f);
                }
            }
        }

        private void drawOnApproach(decimal dg)
        {
            // distance to site
            var dh = game.status.Altitude;
            var d = new PointM(dg, dh).dist;
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
