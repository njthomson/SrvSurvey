using SrvSurvey.game;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal class PlotHumanSite : PlotBaseSite, PlotterForm
    {
        private FileSystemWatcher? mapImageWatcher;

        private HumanSiteData site { get => game.humanSite!; }

        private PlotHumanSite() : base()
        {
            this.Width = scaled(500);
            this.Height = scaled(600);
            this.Font = GameColors.fontSmall;

            // these we get from current data
            this.siteOrigin = game.humanSite!.location;
            this.siteHeading = game.humanSite!.heading;
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
                && !Game.activeGame.atMainMenu
                && Game.activeGame.status.hasLatLong
                && Game.activeGame.status.Altitude < 2000
                && Game.settings.autoShowHumanSitesTest
                && Game.activeGame.humanSite != null
                && Game.activeGame.humanSite.subType > 0;
        }

        public static bool allowPlotter
        {
            get => keepPlotter
                && Game.activeGame!.isMode(GameMode.OnFoot, GameMode.Flying, GameMode.Docked, GameMode.InSrv, GameMode.InTaxi, GameMode.Landed, GameMode.CommsPanel);
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
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;
            base.Status_StatusChanged(blink);

            // so we close once exceeding a certain altitude
            if (this.Opacity > 0 && !PlotHumanSite.keepPlotter)
                Program.closePlotter<PlotHumanSite>(true);
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

        //private List<PointF> bld = new List<PointF>(); // tmp!

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed || this.site?.template == null) return;

            try
            {
                resetPlotter(g);

                // footer
                this.drawTextAt($"{site!.name} - {site.economy} #{site.subType}");
                var pf = Util.getOffset(this.radius, site.location, site.heading);
                // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 
                // ❗❕❉✪✿❤➊➀⟐⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳

                var footerTxt = $"cmdr offset: x: {(int)cmdrOffset.X}m, y: {(int)cmdrOffset.Y}m";
                if (this.mapImage != null) footerTxt += $" (☍{(int)pf.X}, {(int)pf.Y})";
                this.drawFooterText(footerTxt);
                clipToMiddle();

                // map

                // relative to site origin
                this.resetMiddleSiteOrigin();

                this.drawMapImage();

                this.drawLandingPads();
                this.drawCompassLines();

                // draw limit - outside which ships/taxi's an be requested
                var zz = 500;
                g.DrawEllipse(GameColors.newPen(Color.FromArgb(64, Color.Gray), 8, DashStyle.DashDotDot), -zz, -zz, zz * 2, zz * 2);

                // relative to cmdr ...
                this.resetMiddleRotated();
                this.drawShipAndSrvLocation();

                this.resetMiddle();
                this.drawCommander();
            }
            catch (Exception ex)
            {
                Game.log($"PlotHumanSite.OnPaintBackground error: {ex}");
            }
        }

        private PointF pad1 = new PointF(-58.979515f, 3.3044147f);

        private void drawLandingPads()
        {
            //var pad = this.game!.humanSite!.template!.landingPads[0];

            // new PointF(-58.98437f, -3.0885863f),
            // new PointF(-58.98437f, -3.0885863f),

            //g.DrawLine(Pens.Yellow, Point.Empty, pad.offset);

            foreach (var pad in game!.humanSite!.template!.landingPads)
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
                    var idx = game!.humanSite!.template!.landingPads.IndexOf(pad) + 1;
                    g.DrawString($"{idx}", GameColors.fontSmall, Brushes.SlateGray, r.Left + four, r.Top + four);
                });
            }

            if (game!.humanSite!.template!.secureDoors != null)
            {
                foreach (var door in game!.humanSite!.template!.secureDoors)
                {
                    adjust(door.offset, door.rot, () =>
                    {
                        var b = door.level == 0 ? Brushes.Green : Brushes.OrangeRed;
                        g.FillRectangle(b, -3, -2, 6, 3);
                        g.DrawRectangle(Pens.DeepSkyBlue, -3, -2, 6, 3);
                    });
                }
            }

            if (game!.humanSite!.template!.namedPoi != null)
            {
                foreach (var door in game!.humanSite!.template!.namedPoi)
                {
                    // site.heading - game.status.Heading
                    adjust(door.offset, -site.heading + game.status.Heading, () =>
                    {
                        string txt = "⟑";
                        switch (door.name)
                        {
                            case "Power": txt = "⭍"; break;
                            case "Alarm": txt = "♨"; break;
                            case "Auth": txt = "⨻"; break;
                            case "Atmos": txt = "⦚"; break;
                        }

                        // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗  // -8 ??
                        g.DrawString(txt, GameColors.fontSmall, Brushes.Yellow, -4, -6);
                        //g.DrawEllipse(Pens.Yellow, -5, -5, 10, 10);
                    });
                }
            }

            if (game!.humanSite!.template!.dataTerminals != null)
            {
                foreach (var terminal in game!.humanSite!.template!.dataTerminals)
                {
                    // site.heading - game.status.Heading
                    adjust(terminal.offset, -site.heading + game.status.Heading, () =>
                    {
                        // ❗❕❉✪✿❤➊➀⟐⟊➟✦✔⛶⛬⛭⛯⛣⛔⛌⛏⚴⚳⚱⚰⚚⚙⚗⚕⚑⚐⚜⚝⚛⚉⚇♥♦♖♜☸☗☯☍☉☄☁◬◊◈◍◉▣▢╳
                        string txt = "ꊢ"; // ⦖⥣ꇗꊢꉄꇥꇗꇩꆜꄨꀜꀤꀡꀍ䷏〶〷〓〼〿⸙⸋⯒⭻⬨⬖⬘⬮⬯⫡⩸⩃⨟⨨⨱⨲⦼⧌⚼⦡⧲⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨹⨺⨻⨷⨳⨯⬙⭕⭍✉⛽✇⛳⛿✆⛋⚼"; ⛗ ⛘ ⛅ ⛍ ⛉ ❎ ⮔⮹ ⮺⯑⯳⯿⑅Ⓓⓓ▚▚▨▒◀◩ ⦡⦺⦹⦿⧳⧲⨝⨹⨺⨻⨷⨳⨯⬙⭕⭍

                        // ☢ ♨ ⚡ ☎ ☏ ♫ ⚠ ⚽ ✋ ❕ ❗ 
                        g.DrawString(txt, GameColors.fontSmall, Brushes.Yellow, -4, -6);
                        //g.DrawEllipse(Pens.Yellow, -5, -5, 10, 10);
                    });
                }
            }

            //// containers
            //adjust(new PointF(-26.862776f, -28.68819f), 0, () =>
            //{
            //    g.DrawRectangle(Pens.Gray, -2, -5, 4, 10);
            //});
            //adjust(new PointF(-19.692627f, -34.287342f), 0, () =>
            //{
            //    g.DrawRectangle(Pens.Gray, -2, -5, 4, 10);
            //});
            //adjust(new PointF(-10.7577715f, -34.48904f), 0, () =>
            //{
            //    g.DrawRectangle(Pens.Gray, -2, -5, 4, 10);
            //});
            //adjust(new PointF(-14.873593f, -34.347885f), 120, () =>
            //{
            //    g.DrawRectangle(Pens.Gray, -2, -5, 4, 10);
            //});


            //// building?
            //g.DrawLines(Pens.DarkOliveGreen, new PointF[]
            //{
            //    new PointF(59.59616f, -16.591711f),
            //    new PointF(46.238243f, -16.495996f),
            //    new PointF(46.435684f, -23.39868f),
            //    new PointF(40.352264f, -23.323128f),
            //    new PointF(40.376453f, -28.846914f),
            //    new PointF(33.663334f, -28.8208f),
            //    new PointF(33.357445f, -37.04121f),
            //    new PointF(40.376453f, -37.04121f),
            //    //new PointF(35.333553f, -36.638054f),
            //    new PointF(40.376453f, -41.875633f),
            //    new PointF(46.449062f, -41.875633f),
            //    new PointF(46.449062f, -44.817448f),
            //    new PointF(52.708496f, -44.817448f),
            //    new PointF(53.005432f, -41.875633f),
            //    new PointF(66.884224f, -41.875633f),
            //    new PointF(68.024635f, -17.262543f),
            //    new PointF(59.5556f, -17.262543f),
            //});

            //if (bld.Count > 0)
            //    g.DrawLines(Pens.DarkOliveGreen, bld.ToArray());

            //// doors
            //adjust(new PointF(49.937435f, -17.801565f), 270, () =>
            //{
            //    g.FillRectangle(Brushes.SkyBlue, -2, -3, 3, 6);
            //    g.DrawRectangle(Pens.DeepSkyBlue, -2, -3, 3, 6);
            //});
            //adjust(new PointF(34.425014f, -32.70976f), 0, () =>
            //{
            //    g.FillRectangle(Brushes.SkyBlue, -2, -3, 3, 6);
            //    g.DrawRectangle(Pens.DeepSkyBlue, -2, -3, 3, 6);
            //});
            //adjust(new PointF(31.614084f, 25.721779f), 90, () =>
            //{
            //    g.FillRectangle(Brushes.SkyBlue, -2, -3, 3, 6);
            //    g.DrawRectangle(Pens.DeepSkyBlue, -2, -3, 3, 6);
            //});
            //adjust(new PointF(9.832186f, 70.44976f), 0, () =>
            //{
            //    g.FillRectangle(Brushes.SkyBlue, -2, -3, 3, 6);
            //    g.DrawRectangle(Pens.DeepSkyBlue, -2, -3, 3, 6);
            //});
            //adjust(new PointF(-5.9735804f, 36.91453f), 90, () =>
            //{
            //    g.FillRectangle(Brushes.SkyBlue, -2, -3, 3, 6);
            //    g.DrawRectangle(Pens.DeepSkyBlue, -2, -3, 3, 6);
            //});

        }
    }

}
