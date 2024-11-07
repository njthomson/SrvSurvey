using DecimalMath;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.units;
using System.Diagnostics;
using System.Globalization;

namespace SrvSurvey
{
    internal class PlotHumanSite : PlotBaseSite, PlotterForm
    {
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
                && Game.activeGame.systemStation != null;
        }

        /// <summary>
        /// Conditions when plotter can be visible
        /// </summary>
        public static bool allowPlotter
        {
            get => keepPlotter
                && Game.activeGame!.isMode(GameMode.OnFoot, GameMode.Flying, GameMode.Docked, GameMode.InSrv, GameMode.InTaxi, GameMode.Landed, GameMode.CommsPanel, GameMode.ExternalPanel, GameMode.GlideMode, GameMode.RolePanel);
        }

        /// <summary> If zoom levels should change automatically </summary>
        public static bool autoZoom = true;
        /// <summary> If the map should be huge and take over half the screen (game rect) </summary>
        public static bool beHuge = false;

        /// <summary>Distance of outer circle, outside which ships and shuttles can be ordered</summary>
        public static float limitDist = 500;
        /// <summary>Distance from settlement origin to begin showing arrow pointing back to settlement origin</summary>
        public static decimal limitWarnDist = 300;
        public static bool showMapPixelOffsets = false;

        private FileSystemWatcher? mapImageWatcher;
        private FileSystemWatcher? templateWatcher;
        private DockingState dockingState = DockingState.none;
        private int grantedPad;
        private bool hasLanded;
        private string deniedReason;

        private FormBuilder? builder { get => FormBuilder.activeForm; }

        private CanonnStation station;

        private PlotHumanSite() : base()
        {
            this.Size = Size.Empty;
            this.Font = GameColors.fontSmall;
            this.hasLanded = game.isMode(GameMode.Landed, GameMode.Docked, GameMode.InSrv, GameMode.OnFoot);

            // these we get from current data
            if (game?.systemStation == null) throw new Exception("Why no systemStation?");
            this.station = game.systemStation;
            this.siteLocation = this.station.location;
            this.siteHeading = this.station.heading;

            if (siteLocation.Lat == 0 || siteLocation.Long == 0) Debugger.Break(); // Does this ever happen?

            if (!PlotHumanSite.autoZoom) this.scale = 6;

            if (game.isMode(GameMode.OnFoot, GameMode.Docked, GameMode.InSrv, GameMode.Landed))
            {
                this.dockingState = DockingState.landed;
                this.hasLanded = true;
            }

            game.initMats(this.station);
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
            if (PlotHumanSite.beHuge)
            {
                // make plotter completely fill left half of the game window
                var er = Elite.getWindowRect();
                var maxWidth = er.Width * 0.4f;
                var maxHeight = er.Height * 0.9f;

                this.Width = (int)scaled(maxWidth);
                this.Height = (int)scaled(maxHeight);
            }
            else
            {
                this.Width = scaled(Game.settings.plotHumanSiteWidth);
                this.Height = scaled(Game.settings.plotHumanSiteHeight);
            }
            base.OnLoad(e);

            // close these, if they happen to be open
            Program.closePlotter<PlotBioStatus>();
            Program.closePlotter<PlotGrounded>();
            Program.closePlotter<PlotPriorScans>();

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            this.loadMapImage();
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
            if (this.siteHeading == -1 && station.heading != -1)
                this.siteHeading = station.heading;

            // load missing map image if the template was recently set
            if (this.mapImage == null && station.template != null)
                this.loadMapImage();

            // auto-adjust zoom?
            this.setZoom(newMode);
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;
            base.Status_StatusChanged(blink);

            // update if the site recently gained it
            if (this.siteHeading == -1 && station.heading != -1)
                this.siteHeading = station.heading;

            // auto-adjust zoom?
            this.setZoom(game.mode);
        }

        public void setZoom(GameMode mode)
        {
            if (PlotHumanSite.autoZoom)
            {
                var newZoom = getAutoZoomLevel(mode);
                if (newZoom != 0)
                    this.scale = newZoom;
            }

            this.Invalidate();
        }

        private float getAutoZoomLevel(GameMode mode)
        {
            if (game.status.SelectedWeapon == "$humanoid_companalyser_name;")
            {
                // zoom in LOTS if inside a building or using the Profile analyzer tool
                return Game.settings.humanSiteZoomTool;
            }
            else if (game.status.OnFootInside)
            {
                // zoom in LOTS if inside a building or using the Profile analyzer tool
                return Game.settings.humanSiteZoomInside;
            }
            else if (mode == GameMode.OnFoot || game.status.OnFootOutside)
            {
                // zoom in SOME if on foot outside
                return Game.settings.humanSiteZoomFoot;
            }
            else if (mode == GameMode.InSrv)
            {
                // zoom out if in some vehicle
                return Game.settings.humanSiteZoomSRV;
            }
            else if (mode == GameMode.Landed || mode == GameMode.Docked || mode == GameMode.Flying)
            {
                // zoom out if in some vehicle
                return Game.settings.humanSiteZoomShip;
            }

            return 0;
        }

        public void adjustZoom(bool zoomIn)
        {
            Game.log($"PlotHumanSite adjustZoom: zoomIn: {zoomIn}");
            const float delta = 0.5f;
            var newZoom = zoomIn ? this.scale + delta : this.scale - delta;

            if (newZoom < 0.5f || newZoom > 15) return;

            // enable/disable auto-zooming if the new zoom level matches
            var autoZoomLevel = this.getAutoZoomLevel(game.mode);
            if (autoZoomLevel != 0)
                PlotHumanSite.autoZoom = newZoom == autoZoomLevel;

            this.scale = newZoom;
            this.Invalidate();
        }

        public static void toggleHugeness()
        {
            Game.log($"Toggle PlotHumanSite hugeness => {!PlotHumanSite.beHuge}");
            PlotHumanSite.beHuge = !PlotHumanSite.beHuge;
            Program.closePlotter<PlotHumanSite>();
            Program.showPlotter<PlotHumanSite>();
        }

        private void loadMapImage()
        {
            // load a background image, if found
            try
            {
                if (this.station.template == null || this.station.subType == -1) return;

                if (this.mapImageWatcher != null)
                    this.mapImageWatcher.EnableRaisingEvents = false;
                if (this.mapImage != null)
                {
                    this.mapImage.Dispose();
                    this.mapImage = null;
                }

                // try publish folder
                var folder = Git.pubSettlementsFolder;
                var filepath = Directory.GetFiles(folder, $"{this.station.economy}~{this.station.subType}-*.png")?.FirstOrDefault();

                if (Debugger.IsAttached)
                {
                    // use source code folder
                    folder = Path.GetFullPath(Path.Combine(Git.srcRootFolder, "SrvSurvey", "settlements"));
                    filepath = Directory.GetFiles(folder, $"{this.station.economy}~{this.station.subType}-*.png")?.FirstOrDefault();
                }
                else if (filepath == null)
                {
                    // try install folder
                    folder = Path.Combine(Application.StartupPath, "settlements");
                    filepath = Directory.GetFiles(folder, $"{this.station.economy}~{this.station.subType}-*.png")?.FirstOrDefault();
                }


                // stop trying
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
            Task.Delay(1500).ContinueWith(t => this.BeginInvoke(() =>
            {
                this.dockingState = DockingState.approved;
                this.grantedPad = entry.LandingPad;
                this.Invalidate();
            }));
        }

        protected override void onJournalEntry(DockingDenied entry)
        {
            base.onJournalEntry(entry);
            this.deniedReason = entry.Reason;

            Task.Delay(1500).ContinueWith(t => this.BeginInvoke(() =>
            {
                this.dockingState = DockingState.denied;
                this.Invalidate();
            }));
        }

        protected override void onJournalEntry(DockingCancelled entry)
        {
            this.dockingState = DockingState.none;
            this.Invalidate();
        }

        protected override void onJournalEntry(Music entry)
        {
            this.Invalidate();
        }

        protected override void onJournalEntry(Docked entry)
        {
            base.onJournalEntry(entry);

            // TODO: show something if we are wanted?
            // if (entry.Wanted) { .. }

            this.hasLanded = true;
            this.dockingState = DockingState.landed;
            // invalidate from game.cs?
            //this.BeginInvoke(() =>
            //{
            //    this.siteHeading = site.heading;
            //    this.Invalidate();
            //});
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
            if (station.template?.dataTerminals == null) return;
            if (entry.Added == null || !entry.Added.Any(_ => _.Type == "Data")) return;

            // find closest data terminal, mark it as "processed" (so it renders differently)
            var offset = Util.getOffset(radius, siteLocation, siteHeading);
            var terminal = station.template.dataTerminals.FirstOrDefault(dt => (offset - new PointM(dt.offset)).dist < 5);
            if (terminal != null)
            {
                terminal.processed = true;
                this.Invalidate();
            }
        }

        private void reloadTemplate()
        {
            Game.log("Loading " + HumanSiteTemplate.humanSiteTemplates);
            PlotHumanSite.autoZoom = false;
            HumanSiteTemplate.import(true);
            Application.DoEvents();
            this.Invalidate();

            // start watching template file changes (if not already)
            if (this.templateWatcher == null)
            {
                var folder = Path.GetFullPath(Path.Combine(Git.srcRootFolder, "SrvSurvey", "settlements"));
                this.templateWatcher = new FileSystemWatcher(folder, HumanSiteTemplate.humanSiteTemplates);
                this.templateWatcher.Changed += TemplateWatcher_Changed;
                this.templateWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
                this.templateWatcher.EnableRaisingEvents = true;
            }
        }

        protected override void onJournalEntry(SendText entry)
        {
            if (this.IsDisposed || game?.systemStation == null) return;

            var msg = entry.Message.ToLowerInvariant().Trim();
            if (msg.StartsWith("z "))
            {
                PlotHumanSite.autoZoom = false;
                // (base class will handle setting the zoom level)
            }
            else if (msg == "z")
            {
                PlotHumanSite.autoZoom = true;
                this.setZoom(game.mode);
            }

            base.onJournalEntry(entry);

            if (msg == "ll")
            {
                // force reload the template
                this.reloadTemplate();
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
                this.reloadTemplate();
                if (builder == null)
                    FormBuilder.show(this.station);
            }
            if (msg == MsgCmd.start)
            {
                // begin settlement mats survey
                game.startMats();
            }
            if (msg == MsgCmd.stop)
            {
                // end settlement mats survey
                game.exitMats(true);
            }

            if (msg == ".flags")
            {
                Game.log($".flags: alt: {game.status.Altitude}, heading: {game.status.Heading}, docked: {game.status.Docked}\r\n{game.status.Flags}\r\n{game.status.Flags2}");
            }

            this.Invalidate();
        }

        private void TemplateWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Game.log("TemplateWatcher_Changed");
            this.reloadTemplate();
        }

        private static float zoomLevelMinWidth = 0;
        private static float zoomLevelAutoWidth = 0;

        private string? getLocalizedEconomy()
        {
            switch (station.economy)
            {
                case Economy.Agriculture:
                    return Properties.Economies.ResourceManager.GetString("Agri");
                case Economy.PrivateEnterprise:
                    return Properties.Economies.ResourceManager.GetString("Carrier");

                default:
                    return Properties.Economies.ResourceManager.GetString(station.economy.ToString());
            }
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            // first, draw headers and footers (before we start clipping)

            // header - left
            var economyTxt = this.getLocalizedEconomy();
            var headerTxt = $"{station.name} - {economyTxt} ";
            headerTxt += station.subType == 0 ? "?" : $"#{station.subType}";
            if (station.government == "$government_Anarchy;")
                headerTxt = "🏴‍☠️ " + headerTxt;

            var headerLeftSz = drawTextAt2(eight, headerTxt);

            // (one time) figure out how much space we need for the zoom headers
            if (zoomLevelAutoWidth == 0) zoomLevelAutoWidth = 8 + Util.maxWidth(GameColors.fontSmall, RES("ZoomHeaderAuto", (44.4f).ToString("N1")));
            if (zoomLevelMinWidth == 0) zoomLevelMinWidth = 8 + Util.maxWidth(GameColors.fontSmall, RES("ZoomHeaderMin", (44.4f).ToString("N1")));

            // header - right (if there's enough space)
            if (headerLeftSz.Width < this.Width - zoomLevelAutoWidth)
            {
                var zoomText = PlotHumanSite.autoZoom
                    ? RES("ZoomHeaderAuto", this.scale.ToString("N1"))
                    : RES("ZoomHeader", this.scale.ToString("N1"));
                drawTextAt2(this.ClientSize.Width - 8, zoomText, GameColors.OrangeDim, GameColors.fontSmall, true);
            }
            else if (headerLeftSz.Width < this.Width - zoomLevelMinWidth)
            {
                var zoomText = RES("ZoomHeaderMin", this.scale.ToString("N1"));
                drawTextAt2(this.ClientSize.Width - 8, zoomText, GameColors.OrangeDim, GameColors.fontSmall, true);
            }

            newLine(+ten, true);

            var distToSiteOrigin = Util.getDistance(siteLocation, Status.here, this.radius);

            clipToMiddle();

            if (this.siteHeading >= 0)
            {
                // when we know the settlement type and heading
                this.drawKnownSettlement(distToSiteOrigin);
            }

            g.ResetTransform();
            g.ResetClip();
            if (this.station.heading == -1 || game.status.GlideMode || (game.mode == GameMode.Flying && !this.hasLanded))
                this.drawOnApproach(distToSiteOrigin);
            else
                this.drawFooterAtSettlement();
        }

        private void drawFooterAtSettlement()
        {
            // TODO: improve this?

            // footer - cmdr's location heading RELATIVE TO SITE
            var aaa = game.status.Heading - siteHeading; // TODO: replace `aaa` with `cmdrHeading`
            if (aaa < 0) aaa += 360;
            var footerTxt = $"x: {Util.metersToString(cmdrOffset.X, true)}, y: {Util.metersToString(cmdrOffset.Y, true)}, {aaa}°";

            // are we inside a building?
            var currentBld = station.template?.getCurrentBld(cmdrOffset, this.siteHeading);
            if (currentBld != null)
            {
                var parts = currentBld?.Split(" ", 2);
                if (parts?.Length > 0)
                {
                    currentBld = RES(parts[0]);
                    if (parts.Length > 1)
                        currentBld += $" {parts[1]}";
                }

                if (currentBld != null) footerTxt += $"| {currentBld}"; // TODO: How to localize this?
            }

            // Pixel offset relative to background map image - an aide for creating map images
            if (showMapPixelOffsets && this.mapImage != null)
            {
                var pf = Util.getOffset(this.radius, siteLocation, Status.here.clone(), siteHeading);
                // TODO: confirm this is actually identical to `cmdrOffset`
                pf.x += this.mapCenter.X;
                pf.y = this.mapImage.Height - pf.y - this.mapCenter.Y;
                footerTxt += $" (☍{(int)pf.X}, {(int)pf.Y})";
                // TODO: remove this? Isn't it the same as we already render?
            }

            if (Game.settings.collectMatsCollectionStatsTest && game.matStatsTracker?.completed == false)
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

            dty = this.ClientSize.Height - twenty;
            drawTextAt2(eight, footerTxt);
        }

        private void drawKnownSettlement(decimal distToSiteOrigin)
        {
            // draw relative to site origin
            this.resetMiddleSiteOrigin();

            // For use with font based symbols, keeping them up-right no matter the site or cmdr heading
            var adjustedHeading = -siteHeading + game.status.Heading;

            // draw background map and POIs
            this.drawMapImage();
            if (this.mapImage == null)
                this.drawBuildings();
            this.drawLandingPads();
            this.drawSecureDoors();
            this.drawNamedPOI(adjustedHeading);
            if (Game.settings.humanSiteShow_DataTerminal)
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
                var td = new TrackingDelta(this.radius, siteLocation);
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
            if (station.template == null) return;

            foreach (var pad in station.template.landingPads)
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
                    g.DrawEllipse(GameColors.HumanSite.landingPad.pen, -2, -2, 4, 4);
                    g.DrawEllipse(GameColors.HumanSite.landingPad.pen, -10, -10, 20, 20);
                    //g.DrawEllipse(GameColors.HumanSite.landingPad.pen, -20, -20, 40, 40);

                    // show pad # in corner
                    var idx = station.template.landingPads.IndexOf(pad) + 1;
                    g.DrawString($"{idx}", GameColors.fontSmall, GameColors.HumanSite.landingPad.brush, r.Left + four, r.Top + four);
                });
            }

        }

        private void drawBuildings()
        {
            if (station.template?.buildings == null) return;

            // draw buildings from template
            foreach (var bld in station.template!.buildings)
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
                    bb = GameColors.Building.RES; // darker brown?
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

            // draw in-progress buildings
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
            if (station.template?.secureDoors == null) return;

            foreach (var door in station.template.secureDoors)
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
            if (station.template?.namedPoi == null) return;

            foreach (var poi in station.template.namedPoi)
            {
                if (poi.name == "Medkit" && !Game.settings.humanSiteShow_Medkit) continue;
                if (poi.name == "Battery" && !Game.settings.humanSiteShow_Battery) continue;

                adjust(poi.offset, adjustedHeading, () =>
                {
                    var x = 0f;
                    var y = 0f;

                    var b = GameColors.HumanSite.siteLevel[poi.level].brush;
                    var p = GameColors.HumanSite.siteLevel[poi.level].pen;

                    // ❗❕❉✪✿❤➊➀⟐𖺋𖺋⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛
                    // ⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳☢
                    // ꊢ ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅
                    // ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼
                    // ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍
                    // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 

                    // https://unicode-explorer.com/emoji/
                    //  🎂 🧁 🍥 ‡† ⁑ ⁂ ※ ⁜‼•🟎 🟂🟎🟒🟈⚑⚐⛿🏁🎌⛳🏴🏳🟎✩✭✪𓇽𓇼 🚕🛺🚐🚗🚜🚛🛬🚀🛩️☀️🌀☄️🔥⚡🌩️🌠☀️
                    // 💫 🧭🧭🌍🌐🌏🌎🗽♨️🌅🧶🎯🔮🧿🎈🏆🎖️🌌💫🚧
                    // 🍥🍪🧊⛩️🌋⛰️🗻❄️🎉🧨🎁🧿🎲🕹️📣🎨🧵
                    // 🔇🔕🎚️🎛️📻📱📺💻🖥️💾📕📖📦📍📎✂️📌📐📈💼🔰🛡️
                    // 🔨🗡️🔧🧪🚷🧴📵🧽➰🔻🔺🔔🔘🔳🔲🏁🚩🏴✔️✖️
                    // ❌➕➖➗ℹ️📛⭕☑️📶🔅🔆⚠️⛔🚫🧻↘️⚰️🧯🧰📡🧬⚗️
                    // 🔩⚙️🔓🗝️🗄️📩🧾📒📰🗞️🏷️📑🔖💡🔋🏮🕯🔌📞☎️💍👑 ☠️💀

                    // 🔔 alarm ?
                    // 🗝️ auth?
                    // 🔋 battery?
                    // 📩 data terminal?
                    // 🔩 sample container


                    if (poi.name == "Atmos")
                    {
                        g.DrawString("⚴", GameColors.Fonts.typewriter_p6, b, -2, -3);
                        x = 0.6f; y = -3.7f;
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

                        g.DrawString("⭍", GameColors.Fonts.typewriter_p6, b, -3, -4);
                        x = 0.6f; y = -5f;
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
            if (station.template?.dataTerminals == null) return;

            foreach (var terminal in station.template.dataTerminals)
            {
                adjust(terminal.offset, adjustedHeading, () =>
                {
                    var b = terminal.processed
                        ? GameColors.HumanSite.siteLevel[-1].brush
                        : GameColors.HumanSite.siteLevel[terminal.level].brush;

                    var p = terminal.processed
                        ? GameColors.HumanSite.siteLevel[-1].pen
                        : GameColors.HumanSite.siteLevel[terminal.level].pen;

                    g.DrawString("/", GameColors.Fonts.wingdings2_4, b, -2.5f, -3);
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
            if (game.matStatsTracker?.matLocations != null && Game.settings.humanSiteDotsOnCollection)
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
            dty += eight;
            // fade out the map a little
            g.FillRectangle(GameColors.HumanSite.brushTextFade, four, twoSix, this.Width - eight, this.Height - threeSix);

            if (station.subType == 0 && station.heading == -1)
                drawApproachText("❓", RES("UnknownTypeHeading"), GameColors.Cyan);
            else if (station.heading == -1)
                drawApproachText("❓", RES("UnknownHeading"), GameColors.Cyan);
            else if (!this.hasLanded)
                drawApproachText("►", RES("KnownSettlement"), GameColors.LimeIsh);

            if (!this.hasLanded)
            {
                // distance to site, triangulated with altitude 
                var d = new PointM(distToSiteOrigin, game.status.Altitude).dist;
                drawApproachText("►", $"{RES("OnApproach")}: " + Util.metersToString(d));

                // the controlling faction

                //var w = Util.maxWidth(GameColors.fontMiddle, prefixFaction, prefixInfluence);
                drawTextAt2(eight, "►", GameColors.fontMiddle);
                drawTextAt2(threeTwo, $"{RES("Faction")}: ", GameColors.fontMiddle);
                var x = dtx;
                drawTextAt2(station.factionName, GameColors.fontMiddleBold);
                newLine();
                drawTextAt2(x, $"{RES("Influence")}: ", GameColors.fontMiddle);
                drawTextAt2(station.influence?.ToString("p0"), GameColors.fontMiddleBold);

                // append faction state if we know it
                if (station.factionState != null)
                {
                    drawTextAt2(" | ", GameColors.fontMiddle);
                    var factionStateTxt = Properties.FactionStates.ResourceManager.GetString(station.factionState);
                    drawTextAt2(factionStateTxt, GameColors.fontMiddleBold);
                }
                newLine(+ten, true);

                // Your reputation with the controlling faction
                if (station.reputation.HasValue)
                {
                    var col = GameColors.Orange;
                    var prefix = "🔹";
                    if (station.reputation <= -35)
                    {
                        col = GameColors.red;
                        prefix = "✋";
                    }
                    else if (station.reputation > 35)
                    {
                        col = GameColors.LimeIsh;
                        prefix = "✔️";
                    }

                    var rep = Util.getReputationText(station.reputation.Value);
                    drawApproachText(prefix, RES("YourRep", rep), col);
                }

                if (station.government == "$government_Anarchy;")
                    drawApproachText("🏴‍☠️", $"{RES("Government")}: {station.governmentLocalized}");

                if (station.stationServices?.Contains("facilitator") == true)
                    drawApproachText("🙂", RES("HasInterstellar"));
            }

            if (game.dockingInProgress)
            {
                // docking status?
                if (this.dockingState == DockingState.requested || this.dockingState == DockingState.approved || this.dockingState == DockingState.denied)
                    drawApproachText("►", RES("DockingRequested"));
                if (this.dockingState == DockingState.approved)
                    drawApproachText("►", RES("DockingApproved", this.grantedPad));
            }
            else if (this.dockingState == DockingState.denied)
            {
                drawTextAt2(eight, $"⛔ {RES("DockingDenied")}", GameColors.fontMiddle);
                newLine(+ten, true);
                if (this.deniedReason == "Distance")
                    drawTextAt2(threeTwo, $"➟ {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "Hostile")
                    drawTextAt2(threeTwo, $"💀 {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "TooLarge")
                    drawTextAt2(threeTwo, $"🔷 {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "NoSpace")
                    drawTextAt2(threeTwo, $"❎ {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "Offenses")
                    drawTextAt2(threeTwo, $"🧻 {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "ActiveFighter")
                    drawTextAt2(threeTwo, $"🛩️ {RES(this.deniedReason)}", GameColors.fontMiddle);
                else
                    drawTextAt2(threeTwo, $"🚫 {RES("Unknown")}", GameColors.fontMiddle);
                newLine(+ten, true);
            }

            if (game.dockingInProgress && siteHeading == -1)
            {
                this.dty = this.Height - sevenTwo;

                // auto docking
                if (game.musicTrack == "DockingComputer")
                {
                    drawTextAt2(eight, $"⛳", GameColors.LimeIsh, GameColors.fontBig);
                    drawTextAt2b(fiveTwo, $"► {RES("AutoDock1")}", GameColors.LimeIsh, GameColors.fontMiddle);
                    newLine();
                    drawTextAt2b(fiveTwo, $"► {RES("AutoDock2")}", GameColors.LimeIsh, GameColors.fontMiddle);
                    newLine();
                    drawTextAt2b(fiveTwo, $"► {RES("AutoDock3")}", GameColors.LimeIsh, GameColors.fontMiddleBold);
                }
                else
                {
                    drawTextAt2(eight, $"✋", GameColors.fontBig);

                    // manual docking
                    drawTextAt2b(fiveTwo, $"► {RES("ManualDock1")}", GameColors.fontMiddle);
                    newLine();
                    drawTextAt2b(fiveTwo, $"⏳ {RES("ManualDock2")}", GameColors.fontMiddle);
                    newLine();
                    drawTextAt2b(fiveTwo, $"► {RES("ManualDock3")}", GameColors.fontMiddleBold);
                }
            }
            else if (this.hasLanded && siteHeading == -1)
            {
                if (game.status.Docked)
                {
                    drawTextAt2b(eight, $"► {RES("HelpShip1")}", GameColors.fontMiddle);
                    newLine(+six);
                    drawTextAt2b(eight, $"{RES("HelpShip2")}:", GameColors.fontMiddleBold);
                    newLine(+six);
                    drawTextAt2b(eight, $"► {RES("HelpShip3", ".settlement")}", GameColors.fontMiddle);
                }
                else if (game.vehicle == ActiveVehicle.Foot || game.vehicle == ActiveVehicle.SRV)
                {
                    if (this.siteHeading == -1 && game.isMode(GameMode.Landed, GameMode.InSrv, GameMode.OnFoot))
                    {
                        // otherwise, show guidance to manually set it
                        //g.ResetTransform();
                        drawTextAt2b(eight, RES("HelpFoot0"), GameColors.fontMiddleBold);
                        newLine(+six);
                        drawTextAt2b(eight, RES("HelpFoot1"), GameColors.fontMiddle);
                        newLine();
                        drawTextAt2b(eight, RES("HelpFoot2"), GameColors.fontMiddle);
                        newLine();
                        drawTextAt2b(eight, RES("HelpFoot3"), GameColors.fontMiddle);
                        newLine();
                        drawTextAt2b(eight, RES("HelpFoot4"), GameColors.fontMiddle);
                        newLine();
                        drawTextAt2b(eight, RES("HelpFoot5", ".settlement"), GameColors.fontMiddle);
                        newLine(+oneTwo);
                        drawTextAt2b(eight, RES("HelpFoot6"), GameColors.fontMiddle);
                    }
                }
            }
        }

        private void drawApproachText(string prefix, string txt, Color? col = null)
        {
            drawTextAt2(eight, prefix, col, GameColors.fontMiddle);
            drawTextAt2b(threeTwo, txt, col, GameColors.fontMiddle);
            newLine(+ten, true);
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
