using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotHumanSite : PlotBaseSite, PlotterForm
    {
        private FileSystemWatcher? mapImageWatcher;
        private DockingState dockingState = DockingState.none;

        private HumanSiteData site { get => game.humanSite!; }

        private PlotHumanSite() : base()
        {
            this.Width = scaled(500);
            this.Height = scaled(600);
            this.Font = GameColors.fontSmall;

            // these we get from current data
            this.siteOrigin = site.location;
            this.siteHeading = site.heading;
            //this.scale = 9;

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
            }

            base.Dispose(disposing);
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
                //&& Game.activeGame.status.Altitude < 2000
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.humanSite != null;
            //&& Game.activeGame.humanSite.subType > 0;
        }

        public static bool allowPlotter
        {
            get => keepPlotter
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
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;
            base.Status_StatusChanged(blink);

            // so we close once exceeding a certain altitude
            if (this.Opacity > 0 && !PlotHumanSite.keepPlotter)
                Program.closePlotter<PlotHumanSite>(true);

            //if (game.status.OnFootInside)
            //    this.scale = 4;
            //if (game.status.OnFootOutside)
            //    this.scale = 2;

            this.Invalidate();
        }

        private void setZoom(GameMode newMode)
        {
            //if (newMode == GameMode.OnFoot)
            //    this.scale = 2;
            //if (newMode == GameMode.Docked || newMode == GameMode.Landed)
            //    this.scale = 1;

            this.Invalidate();
        }

        private void loadMapImage()
        {
            // load a background image, if found
            try
            {
                if (this.mapImageWatcher != null)
                    this.mapImageWatcher.EnableRaisingEvents = false;
                if (this.mapImage != null)
                    this.mapImage.Dispose();

                var folder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "images");
                var filepath = Directory.GetFiles(folder, $"{game!.humanSite!.economy}~{game.humanSite.subType}-*.png")?.FirstOrDefault();
                if (filepath == null) return;

                var nameParts = Path.GetFileNameWithoutExtension(filepath).Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length != 7 || nameParts[1] != "s" || nameParts[3] != "x" || nameParts[5] != "y")
                {
                    Game.log($"Bad filename format: {filepath}");
                    return;
                }
                this.mapScale = float.Parse(nameParts[2]);
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
            this.dockingState = DockingState.requested;
            this.Invalidate();
        }

        protected override void onJournalEntry(DockingGranted entry)
        {
            this.dockingState = DockingState.approved;
            this.Invalidate();
        }
        protected override void onJournalEntry(DockingDenied entry)
        {
            this.dockingState = DockingState.denied;
            this.Invalidate();
        }

        protected override void onJournalEntry(Docked entry)
        {
            // become full height
            this.dockingState = DockingState.landed;
            this.Height = scaled(600);
            this.BackgroundImage = GameGraphics.getBackgroundForForm(this);
            this.BeginInvoke(() =>
            {
                this.siteHeading = site.heading;
                this.Invalidate();
            });
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed) return;

            try
            {
                resetPlotter(g);

                var headerTxt = $"{site!.name} - {site.economy} ";
                headerTxt += site.subType == 0 ? "??" : $"#{site.subType}";
                drawTextAt(eight, headerTxt);
                newLine(+ten, true);

                var td = new TrackingDelta(this.radius, site.location);
                var dg = Util.getDistance(site.location, Status.here, this.radius);

                if (game.isMode(GameMode.Flying, GameMode.GlideMode))
                {
                    this.drawOnApproach(dg);
                    //return;
                }
                else
                {
                    // footer
                    var footerTxt = $"cmdr offset: x: {(int)cmdrOffset.X}m, y: {(int)cmdrOffset.Y}m";
                    if (this.mapImage != null)
                    {
                        var pf = Util.getOffset(this.radius, site.location, Status.here.clone(), site.heading);
                        pf.x += this.mapCenter.X;
                        pf.y = this.mapImage.Height - pf.y - this.mapCenter.Y;
                        footerTxt += $" (☍{(int)pf.X}, {(int)pf.Y})";
                    }
                    this.drawFooterText(footerTxt);
                }

                clipToMiddle();
                // relative to site origin
                this.resetMiddleSiteOrigin();

                if (this.site?.heading >= 0)
                {
                    // header
                    //this.drawTextAt($"{site!.name} - {site.economy} #{site.subType}");
                    // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 
                    // ❗❕❉✪✿❤➊➀⟐⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳


                    // map
                    this.drawMapImage();
                    this.drawLandingPads();
                    this.drawPOI();
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
                    float aa = td.angle;
                    g.RotateTransform(180 - game.status.Heading + aa);
                    g.DrawLine(GameColors.penGameOrangeDim4, 0, 20, 0, 100);
                    g.DrawLine(GameColors.penGameOrangeDim4, 0, 100, 20, 80);
                    g.DrawLine(GameColors.penGameOrangeDim4, 0, 100, -20, 80);
                }
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

        private void drawPOI()
        {
            if (game?.humanSite?.template == null) return;
            var wd2b = new Font("Wingdings 2", 6F, FontStyle.Bold, GraphicsUnit.Point);
            var b0 = Brushes.Lime;
            var b1 = Brushes.SkyBlue;
            var b2 = Brushes.DarkOrange;
            var b3 = Brushes.Red;

            var p0 = new Pen(Color.Lime, 0.5f) { EndCap = LineCap.Triangle, StartCap = LineCap.Triangle };
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

                        // ❗❕❉✪✿❤➊➀⟐𖺋𖺋⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳
                        //string txt = "ꊢ"; // ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼"; ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍

                        if (poi.name == "Atmos")
                        {
                            g.DrawString("⚴", GameColors.fontSmall, b, -4, -6);
                            x = 0.65f;
                            y = -5.7f;
                        }
                        else if (poi.name == "Alarm")
                        {
                            g.DrawString("%", GameColors.fontWingDings, b1, -4, -6);
                            x = 2.5f;
                            y = -3;
                        }
                        else if (poi.name == "Auth")
                        {
                            g.DrawString("⧌", GameColors.fontSmall, b, -4, -6);
                            x = 2.3f;
                            y = -5.5f;
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
                            string txt = "⛉⟑";
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

                        // draw cherons to indicate upstairs
                        if (poi.floor == 3) // 2
                        {
                            g.DrawLine(p, x, y, x + 1.5f, y + 1.5f);
                            g.DrawLine(p, x, y, x - 1.5f, y + 1.5f);
                        }
                        if (poi.floor == 2) // 1
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

                        var p = p0;
                        if (terminal.level == 1) p = p1;
                        if (terminal.level == 2) p = p2;
                        if (terminal.level == 3) p = p3;

                        // ❗❕❉✪✿❤➊➀⟐𖺋𖺋⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳
                        //string txt = "ꊢ"; // ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼"; ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍

                        // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 
                        //g.DrawString(txt, GameColors.fontSmall, Brushes.Yellow, -4, -6);

                        // :;<=
                        g.DrawString("/", wd2b, b, -4, -4);
                        var x = 0.25f;
                        var y = -1f;

                        if (terminal.floor == 3)
                        {
                            g.DrawLine(p, x, y, x + 1.5f, y + 1.5f);
                            g.DrawLine(p, x, y, x - 1.5f, y + 1.5f);
                        }
                        if (terminal.floor == 2)
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

        private void drawOnApproach(decimal dg)
        {
            // distance to site
            var dh = game.status.Altitude;
            var d = new PointM(dg, dh).dist;

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
