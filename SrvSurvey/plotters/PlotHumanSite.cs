using DecimalMath;
using Newtonsoft.Json;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Globalization;
using Res = Loc.PlotHumanSite;

namespace SrvSurvey.plotters
{
    internal class PlotHumanSite : PlotBase2Site
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotHumanSite),
            allowed = allowed,
            ctor = (game, def) => new PlotHumanSite(game, def),
            defaultSize = new Size(320, 440),
            invalidationJournalEvents = new() { nameof(BackpackChange), nameof(CollectItems) },
        };

        /// <summary>
        /// Conditions when plotter can exist (but potentially hidden)
        /// </summary>
        public static bool allowed(Game game)
        {
            return Game.settings.autoShowHumanSitesTest
                && Game.activeGame?.status != null
                && !Game.activeGame.atMainMenu
                && Game.activeGame.status.hasLatLong
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.systemStation != null
                // Explicitly no mode check - we don't want to destroy this plotter too easily
                ;
        }

        #endregion

        /// <summary> If zoom levels should change automatically </summary>
        public static bool autoZoom = true;
        /// <summary> If the map should be huge and take over half the screen (game rect) </summary>
        public static bool beHuge { get; private set; } = false;

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
        private string? deniedReason;
        private bool showCZPoints;

        private FormBuilder? builder { get => FormBuilder.activeForm; }

        private CanonnStation station;

        private PlotHumanSite(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall;

            // these we get from current data
            if (game?.systemStation == null) throw new Exception("Why no systemStation?");
            this.station = game.systemStation;
            this.siteLocation = this.station.location;
            this.siteHeading = this.station.heading;
            this.showCZPoints = this.station.factionState == "War";

            if (siteLocation.Lat == 0 || siteLocation.Long == 0) Debugger.Break(); // Does this ever happen?

            if (!PlotHumanSite.autoZoom) this.scale = 6;

            if (game.isMode(GameMode.OnFoot, GameMode.Docked, GameMode.InSrv, GameMode.Landed))
            {
                this.dockingState = DockingState.landed;
                this.hasLanded = true;
            }

            game.initMats(this.station);
            this.setSizeByHugeness();
            this.loadMapImage();
        }

        protected override void onClose()
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

            base.onClose();

            if (this.builder != null)
                this.builder.Close();
        }

        private void setSizeByHugeness()
        {
            if (PlotHumanSite.beHuge)
            {
                // make plotter completely fill left half of the game window
                var er = Elite.getWindowRect();
                var maxWidth = er.Width * 0.4f;
                var maxHeight = er.Height * 0.9f;

                this.setSize((int)N.s(maxWidth), (int)N.s(maxHeight));
            }
            else
            {
                this.setSize(Game.settings.plotHumanSiteWidth, Game.settings.plotHumanSiteHeight);
            }

            this.invalidate();
        }

        protected override void onStatusChange(Status status)
        {
            if (game?.status == null || game.systemBody == null) return;
            base.onStatusChange(status);

            // hide (but not destroy) if not in any of these modes
            this.hidden = !game.isMode(GameMode.OnFoot, GameMode.Flying, GameMode.Docked, GameMode.InSrv, GameMode.InTaxi, GameMode.Landed, GameMode.CommsPanel, GameMode.ExternalPanel, GameMode.GlideMode, GameMode.RolePanel)
                || PlotStationInfo.allowed(game);

            // update if the site recently gained it
            if (this.siteHeading == -1 && station.heading != -1)
                this.siteHeading = station.heading;

            // auto-adjust zoom?
            if (status.changed.has("mode", "SelectedWeapon", "Flags2"))
                this.setZoom(game.mode);

            // load missing map image if the template was recently set
            if (this.mapImage == null && station.template != null)
                this.loadMapImage();
        }

        public void setZoom(GameMode mode)
        {
            if (PlotHumanSite.autoZoom)
            {
                var newZoom = getAutoZoomLevel(mode);
                if (newZoom != 0)
                    this.scale = newZoom;
            }

            this.invalidate();
        }

        private float getAutoZoomLevel(GameMode mode)
        {
            if (game.status.SelectedWeapon == "$humanoid_companalyser_name;")
            {
                // zoom in LOTS if using the Profile analyzer tool
                return Game.settings.humanSiteZoomTool;
            }
            else if (game.status.OnFootOnPlanet && !game.status.OnFootExterior)
            {
                // zoom in LOTS if inside a building
                return Game.settings.humanSiteZoomInside;
            }
            else if (mode == GameMode.OnFoot || game.status.OnFootExterior)
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
            this.invalidate();
        }

        public static void toggleHugeness()
        {
            if (!PlotBase2.isPlotter<PlotHumanSite>()) return;

            Game.log($"Toggle PlotHumanSite hugeness => {!PlotHumanSite.beHuge}");
            PlotHumanSite.beHuge = !PlotHumanSite.beHuge;
            PlotBase2.getPlotter<PlotHumanSite>()?.setSizeByHugeness();
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
                this.invalidate();
            });
        }

        protected override void onJournalEntry(DockingRequested entry)
        {
            base.onJournalEntry(entry);
            this.dockingState = DockingState.requested;
            this.invalidate();
        }

        protected override void onJournalEntry(DockingGranted entry)
        {
            base.onJournalEntry(entry);
            Task.Delay(1500).ContinueWith(t => Program.defer(() =>
            {
                this.dockingState = DockingState.approved;
                this.grantedPad = entry.LandingPad;
                this.invalidate();
            }));
        }

        protected override void onJournalEntry(DockingDenied entry)
        {
            base.onJournalEntry(entry);
            this.deniedReason = entry.Reason;

            Task.Delay(1500).ContinueWith(t => Program.defer(() =>
            {
                this.dockingState = DockingState.denied;
                this.invalidate();
            }));
        }

        protected override void onJournalEntry(DockingCancelled entry)
        {
            this.dockingState = DockingState.none;
            this.invalidate();
        }

        protected override void onJournalEntry(Music entry)
        {
            this.invalidate();
        }

        protected override void onJournalEntry(Docked entry)
        {
            base.onJournalEntry(entry);

            // TODO: show something if we are wanted?
            // if (entry.Wanted) { .. }

            this.hasLanded = true;
            this.dockingState = DockingState.landed;
            this.invalidate();
            // invalidate from game.cs?
            //Program.defer(() =>
            //{
            //    this.siteHeading = site.heading;
            //    this.Invalidate();
            //});
        }

        protected override void onJournalEntry(Touchdown entry)
        {
            base.onJournalEntry(entry);
            this.hasLanded = true;
            this.invalidate();
        }

        protected override void onJournalEntry(SupercruiseEntry entry)
        {
            base.onJournalEntry(entry);
            PlotBase2.remove(def);
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
                this.invalidate();
            }
        }

        private void reloadTemplate()
        {
            Game.log("Loading " + HumanSiteTemplate.humanSiteTemplates);
            PlotHumanSite.autoZoom = false;
            HumanSiteTemplate.import(true);
            Application.DoEvents();
            this.invalidate();

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
            if (game?.systemStation == null) return;

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

            if (msg.StartsWith("@") && msg.Length == 2 && this.station?.template != null)
            {
                var name = msg[1].ToString().ToUpperInvariant()[0];
                if ((name >= 'A' && name <= 'F') || name == 'P')
                {
                    // measure dist/angle from site origin for a foot CZ market
                    var radius = game.status.PlanetRadius;
                    var pf = Util.getOffset(radius, game.systemStation.location, game.systemStation.heading);

                    station.template.czPoints ??= new();
                    if (name != 'P')
                        station.template.czPoints.RemoveAll(p => !string.IsNullOrEmpty(p.name) && p.name[0] == name);

                    station.template.czPoints.Add(new HumanSitePoi2()
                    {
                        name = name.ToString(),
                        offset = (PointF)pf,
                    });
                    this.showCZPoints = true;
                }

                var txt = JsonConvert.SerializeObject(station.template.czPoints, Formatting.Indented);
                Game.log($"Ground CZ Points:\r\n\r\n\t{txt}\r\n");
                Clipboard.SetText(txt);
            }

            this.invalidate();
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

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            // first, draw headers and footers (before we start clipping)

            // header - left
            var headerTxt = station.name;
            if (station.government == "$government_Anarchy;")
                headerTxt = "🏴‍☠️ " + headerTxt;

            var headerLeftSz = tt.draw(N.eight, headerTxt);

            // (one time) figure out how much space we need for the zoom headers
            if (zoomLevelAutoWidth == 0) zoomLevelAutoWidth = 8 + Util.maxWidth(GameColors.fontSmall, Res.ZoomHeaderAuto.format((44.4f).ToString("N1")));
            if (zoomLevelMinWidth == 0) zoomLevelMinWidth = 8 + Util.maxWidth(GameColors.fontSmall, Res.ZoomHeaderMin.format((44.4f).ToString("N1")));

            // header - right (if there's enough space)
            if (headerLeftSz.Width < this.width - zoomLevelAutoWidth)
            {
                var zoomText = PlotHumanSite.autoZoom
                    ? Res.ZoomHeaderAuto.format(this.scale.ToString("N1"))
                    : Res.ZoomHeader.format(this.scale.ToString("N1"));
                tt.draw(this.width - 8, zoomText, GameColors.OrangeDim, GameColors.fontSmall, true);
            }
            else if (headerLeftSz.Width < this.width - zoomLevelMinWidth)
            {
                var zoomText = Res.ZoomHeaderMin.format(this.scale.ToString("N1"));
                tt.draw(this.width - 8, zoomText, GameColors.OrangeDim, GameColors.fontSmall, true);
            }

            tt.newLine(+N.ten, true);

            var distToSiteOrigin = Util.getDistance(siteLocation, Status.here, this.radius);

            clipToMiddle(g);

            if (this.siteHeading >= 0)
            {
                // when we know the settlement type and heading
                this.drawKnownSettlement(g, tt, distToSiteOrigin);
            }

            g.ResetTransform();
            g.ResetClip();
            if (this.station.heading == -1 || game.status.GlideMode || (game.mode == GameMode.Flying && !this.hasLanded))
                this.drawOnApproach(g, tt, distToSiteOrigin);
            else
                this.drawFooterAtSettlement(tt);

            // size is pre-determined
            return this.size;
        }

        private void drawFooterAtSettlement(TextCursor tt)
        {
            // TODO: improve this?

            // footer - cmdr's location heading RELATIVE TO SITE
            var cmdrHeading = game.status.Heading - siteHeading;
            if (cmdrHeading < 0) cmdrHeading += 360;
            var footerTxt = $"{this.station.template?.name} x: {Util.metersToString(cmdrOffset.X, true)}, y: {Util.metersToString(cmdrOffset.Y, true)}, {cmdrHeading}°";

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

            tt.dty = this.height - N.twenty;
            tt.draw(N.eight, footerTxt);
        }

        private void drawKnownSettlement(Graphics g, TextCursor tt, decimal distToSiteOrigin)
        {
            /* NOT READY
            // draw black background checks
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height); // shift to center of window
            g.ScaleTransform(scale, scale); // apply display scale factor (zoom)
            g.RotateTransform(-game.status.Heading); // rotate by cmdr heading
            g.TranslateTransform(-offsetWithoutHeading.X, offsetWithoutHeading.Y); // shift relative to cmdr
            g.FillRectangle(GameColors.adjustGroundChecks(GameColors.fontScaleFactor), -width, -height, width * 2, height * 2);
            */

            // draw relative to site origin
            this.resetMiddleSiteOrigin(g);

            // For use with font based symbols, keeping them up-right no matter the site or cmdr heading
            var adjustedHeading = -siteHeading + game.status.Heading;

            // draw background map and POIs
            this.drawMapImage(g);
            if (this.mapImage == null)
                this.drawBuildings(g);
            this.drawLandingPads(g);
            this.drawSecureDoors(g);
            this.drawNamedPOI(g, adjustedHeading);
            if (Game.settings.humanSiteShow_DataTerminal)
                this.drawDataTerminals(g, adjustedHeading);

            // draw/fill polygon for building being assembled in the editor
            if (this.builder != null)
                this.drawBuildingBox(g, cmdrOffset);

            this.drawSiteCompassLines(g);

            if (this.showCZPoints)
                this.drawCombatZonePoints(g, adjustedHeading);

            // draw limit circle outside which ships/taxi's can be requested
            g.DrawEllipse(GameColors.HumanSite.penOuterLimit, -limitDist, -limitDist, limitDist * 2, limitDist * 2);

            // draw relative to cmdr's location ...
            this.resetMiddleRotated(g);
            this.drawShipAndSrvLocation(g, tt);

            // draw relative to center of window ...
            this.resetMiddle(g);
            this.drawCommander(g);

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
            this.resetMiddleSiteOrigin(g);
            this.drawCollectedMats(g);
        }

        private void drawLandingPads(Graphics g)
        {
            if (station.template == null) return;

            foreach (var pad in station.template.landingPads)
            {
                adjust(g, pad.offset, pad.rot, () =>
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
                    g.DrawString($"{idx}", GameColors.fontSmall, GameColors.HumanSite.landingPad.brush, r.Left + N.four, r.Top + N.four);
                });
            }

        }

        private void drawBuildings(Graphics g)
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

        private void drawBuildingBox(Graphics g, PointF offset)
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

        private void drawSecureDoors(Graphics g)
        {
            if (station.template?.secureDoors == null) return;

            foreach (var door in station.template.secureDoors)
            {
                adjust(g, door.offset, door.rot, () =>
                {
                    // adjust the colour according to security level
                    var b = GameColors.HumanSite.siteLevel[door.level].brush;

                    g.FillRectangle(b, -3, -2, 6, 3);
                    g.DrawRectangle(GameColors.HumanSite.penDoorEdge, -3, -2, 6, 3);
                });
            }
        }

        private void drawNamedPOI(Graphics g, float adjustedHeading)
        {
            if (station.template?.namedPoi == null) return;

            foreach (var poi in station.template.namedPoi)
            {
                if (poi.name == "Medkit" && !Game.settings.humanSiteShow_Medkit) continue;
                if (poi.name == "Battery" && !Game.settings.humanSiteShow_Battery) continue;

                adjust(g, poi.offset, adjustedHeading, () =>
                {
                    var x = 0f;
                    var y = 0f;

                    var b = GameColors.HumanSite.siteLevel[poi.level].brush;
                    var p = GameColors.HumanSite.siteLevel[poi.level].pen;

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

        private void drawDataTerminals(Graphics g, float adjustedHeading)
        {
            if (station.template?.dataTerminals == null) return;

            foreach (var terminal in station.template.dataTerminals)
            {
                adjust(g, terminal.offset, adjustedHeading, () =>
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

        private Point[] lightningBolt = new Point[]
        {
            new (2,-3),
            new (-2,0),
            new (2,0),
            new (-2,3),
        };

        private void drawCombatZonePoints(Graphics g, float adjustedHeading)
        {
            if (station.template?.czPoints == null) return;

            var offset = Util.getOffset(radius, siteLocation, siteHeading);

            foreach (var point in station.template.czPoints)
            {
                adjust(g, point.offset, adjustedHeading, () =>
                {
                    if (point.name == "P")
                    {
                        // CZ power post
                        g.DrawLines(C.FCZ.penPowerpost, lightningBolt);
                        g.DrawEllipse(C.FCZ.penPowerpost, -5, -5, 10, 10);
                    }
                    else
                    {
                        var dist = (offset - new PointM(point.offset)).dist;

                        var p = dist < 5 ? C.FCZ.penCheckpointLocal : C.FCZ.penCheckpoint;
                        g.DrawEllipse(p, -2, -2, 4, 4);
                        g.DrawEllipse(p, -5, -5, 10, 10);

                        var b = dist < 5 ? C.FCZ.brushCheckpointLocal : C.FCZ.brushCheckpoint;
                        g.DrawString(point.name, GameColors.Fonts.gothic_9B, b, -6, -20);
                    }
                });
            }
        }

        private void drawCollectedMats(Graphics g)
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

        private void drawOnApproach(Graphics g, TextCursor tt, decimal distToSiteOrigin)
        {
            tt.dty += N.eight;
            // fade out the map a little
            g.FillRectangle(GameColors.HumanSite.brushTextFade, N.four, N.twoSix, this.width - N.eight, this.height - N.threeSix);

            if (station.subType == 0 && station.heading == -1)
                drawApproachText(tt, "❓", Res.UnknownTypeHeading, GameColors.Cyan);
            else if (station.heading == -1)
                drawApproachText(tt, "❓", Res.UnkownHeading, GameColors.Cyan);
            else if (!this.hasLanded)
            {
                // Include extra lines with the economy/subType AND colonisation name (if known)
                var txt = $"\n{getLocalizedEconomy()} #{station.subType}";
                if (!string.IsNullOrEmpty(this.station.template?.name))
                    txt += $"\n{this.station.template.name}";
                drawApproachText(tt, "►", Res.KnownSettlement + txt, GameColors.LimeIsh);
            }

            if (!this.hasLanded)
            {
                // distance to site, triangulated with altitude 
                var d = new PointM(distToSiteOrigin, game.status.Altitude).dist;
                drawApproachText(tt, "►", $"{Res.OnApproach}: " + Util.metersToString(d));

                // the controlling faction

                //var w = Util.maxWidth(GameColors.fontMiddle, prefixFaction, prefixInfluence);
                tt.draw(N.eight, "►", GameColors.fontMiddle);
                tt.draw(N.threeTwo, $"{Res.Faction}: ", GameColors.fontMiddle);
                var x = tt.dtx;
                tt.draw(station.factionName, GameColors.fontMiddleBold);
                tt.newLine();
                tt.draw(x, $"{Res.Influence}: ", GameColors.fontMiddle);
                tt.draw(station.influence?.ToString("p0"), GameColors.fontMiddleBold);

                // append faction state if we know it
                if (station.factionState != null)
                {
                    tt.draw(" | ", GameColors.fontMiddle);
                    var factionStateTxt = Properties.FactionStates.ResourceManager.GetString(station.factionState);
                    tt.draw(factionStateTxt, GameColors.fontMiddleBold);
                }
                tt.newLine(+N.ten, true);

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
                    drawApproachText(tt, prefix, Res.YourRep.format(rep), col);
                }

                if (station.government == "$government_Anarchy;")
                    drawApproachText(tt, "🏴‍☠️", $"{Res.Government}: {station.governmentLocalized}");

                if (station.stationServices?.Contains("facilitator") == true)
                    drawApproachText(tt, "🙂", Res.HasInterstellar);
            }

            if (game.dockingInProgress)
            {
                // docking status?
                if (this.dockingState == DockingState.requested || this.dockingState == DockingState.approved || this.dockingState == DockingState.denied)
                    drawApproachText(tt, "►", Res.DockingRequested);
                if (this.dockingState == DockingState.approved)
                    drawApproachText(tt, "►", Res.DockingApproved.format(this.grantedPad));
            }
            else if (this.dockingState == DockingState.denied)
            {
                tt.draw(N.eight, $"⛔ {Res.DockingDenied}", GameColors.fontMiddle);
                tt.newLine(+N.ten, true);
                if (this.deniedReason == "Distance")
                    tt.draw(N.threeTwo, $"➟ {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "Hostile")
                    tt.draw(N.threeTwo, $"💀 {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "TooLarge")
                    tt.draw(N.threeTwo, $"🔷 {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "NoSpace")
                    tt.draw(N.threeTwo, $"❎ {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "Offenses")
                    tt.draw(N.threeTwo, $"🧻 {RES(this.deniedReason)}", GameColors.fontMiddle);
                else if (this.deniedReason == "ActiveFighter")
                    tt.draw(N.threeTwo, $"🛩️ {RES(this.deniedReason)}", GameColors.fontMiddle);
                else
                    tt.draw(N.threeTwo, $"🚫 {Res.Unknown}", GameColors.fontMiddle);
                tt.newLine(+N.ten, true);
            }

            if (game.dockingInProgress && siteHeading == -1)
            {
                tt.dty = this.height - N.sevenTwo;

                // auto docking
                if (game.musicTrack == "DockingComputer")
                {
                    tt.draw(N.eight, $"⛳", GameColors.LimeIsh, GameColors.fontBig);
                    tt.draw(N.fiveTwo, $"► {Res.AutoDock1}", GameColors.LimeIsh, GameColors.fontMiddle);
                    tt.newLine();
                    tt.draw(N.fiveTwo, $"► {Res.AutoDock2}", GameColors.LimeIsh, GameColors.fontMiddle);
                    tt.newLine();
                    tt.drawWrapped(N.fiveTwo, $"► {Res.AutoDock3}", GameColors.LimeIsh, GameColors.fontMiddleBold);
                }
                else
                {
                    tt.draw(N.eight, $"✋", GameColors.fontBig);

                    // manual docking
                    tt.drawWrapped(N.fiveTwo, $"► {Res.ManualDock1}", GameColors.fontMiddle);
                    tt.newLine();
                    tt.drawWrapped(N.fiveTwo, $"⏳ {Res.ManualDock2}", GameColors.fontMiddle);
                    tt.newLine();
                    tt.drawWrapped(N.fiveTwo, $"► {Res.ManualDock3}", GameColors.fontMiddleBold);
                }
            }
            else if (this.hasLanded && siteHeading == -1)
            {
                if (game.status.Docked)
                {
                    tt.drawWrapped(N.eight, $"► {Res.HelpShip1}", GameColors.fontMiddle);
                    tt.newLine(+N.six);
                    tt.drawWrapped(N.eight, $"{Res.HelpShip2}:", GameColors.fontMiddleBold);
                    tt.newLine(+N.six);
                    tt.drawWrapped(N.eight, $"► {Res.HelpShip3.format(".settlement")}", GameColors.fontMiddle);
                }
                else if (game.vehicle == ActiveVehicle.Foot || game.vehicle == ActiveVehicle.SRV)
                {
                    if (this.siteHeading == -1 && game.isMode(GameMode.Landed, GameMode.InSrv, GameMode.OnFoot))
                    {
                        // otherwise, show guidance to manually set it
                        //g.ResetTransform();
                        tt.drawWrapped(N.eight, Res.HelpFoot0, GameColors.fontMiddleBold);
                        tt.newLine(+N.six);
                        tt.drawWrapped(N.eight, Res.HelpFoot1, GameColors.fontMiddle);
                        tt.newLine();
                        tt.drawWrapped(N.eight, Res.HelpFoot2, GameColors.fontMiddle);
                        tt.newLine();
                        tt.drawWrapped(N.eight, Res.HelpFoot3, GameColors.fontMiddle);
                        tt.newLine();
                        tt.drawWrapped(N.eight, Res.HelpFoot4, GameColors.fontMiddle);
                        tt.newLine();
                        tt.drawWrapped(N.eight, Res.HelpFoot5.format(".settlement"), GameColors.fontMiddle);
                        tt.newLine(+N.oneTwo);
                        tt.drawWrapped(N.eight, Res.HelpFoot6, GameColors.fontMiddle);
                    }
                }
            }
        }

        private void drawApproachText(TextCursor tt, string prefix, string txt, Color? col = null)
        {
            tt.draw(N.eight, prefix, col, GameColors.fontMiddle);
            tt.drawWrapped(N.threeTwo, txt, col, GameColors.fontMiddle);
            tt.newLine(+N.ten, true);
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
