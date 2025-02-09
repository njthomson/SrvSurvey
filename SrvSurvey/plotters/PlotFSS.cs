using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey.plotters
{
    [ApproxSize(420, 100)]
    internal class PlotFSS : PlotBase, PlotterForm
    {
        public static bool allowPlotter
        {
            get => Game.activeGame?.cmdr != null
                && Game.settings.autoShowPlotFSS
                && Game.activeGame.isMode(GameMode.FSS);
        }

        private FSSBodySignals? lastFSSBodySignals;

        private static long lastSystemAddress;
        private static string lastBodyName;
        private static string? lastInitialValue;
        private static string? lastMappedValue;
        private static string? lastNotes;
        private static bool lastWasDiscovered;

        private bool watching = false;
        private State watchState;
        private DateTime lastScanTime;

        private WatchFssPixelSettings? settings { get => Game.settings.watchFssSettings_TEST; }

        private PlotFSS() : base()
        {
            this.Font = GameColors.fontMiddle;

            if (game.systemData == null) throw new Exception("Why no SystemData when creating PlotFSS?");
            // game.systemData.lastFssBody = game.systemData.bodies.Find(b => b.shortName == "1a"); // tmp
            var lastFssBody = game.systemData.lastFssBody;

            if (lastFssBody != null)
            {
                lastBodyName = lastFssBody.name;
                if (!lastFssBody.wasDiscovered) lastBodyName = $"⚑ {lastBodyName}";

                var suffixes = new List<string>();
                if (lastFssBody.terraformable || lastFssBody.planetClass?.StartsWith("Earth") == true) suffixes.Add("🌎");
                if (lastFssBody.type == SystemBodyType.LandableBody) suffixes.Add("🚀");
                if (suffixes.Count > 0) lastBodyName += $"  {string.Join(',', suffixes)}";

                lastInitialValue = Util.GetBodyValue(lastFssBody, false, false).ToString("N0");
                lastMappedValue = Util.GetBodyValue(lastFssBody, true, true).ToString("N0");
                if (lastFssBody.bioSignalCount > 0)
                    lastNotes = RES("CountBioSignals", lastFssBody.bioSignalCount);
                else
                    lastNotes = "";
            }
            else if (lastSystemAddress > 0 && lastSystemAddress != game.systemData!.address)
            {
                lastBodyName = null!;
                lastInitialValue = null;
                lastMappedValue = null;
                lastNotes = null;
            }

            lastSystemAddress = game.systemData.address;
        }

        public override bool allow { get => PlotFSS.allowPlotter; }

        protected override void OnLoad(EventArgs e)
        {
            this.Width = scaled(420);
            this.Height = scaled(102);

            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            // start watching pixels - if we have bodies to FSS
            if (Game.settings.watchFssSettings_TEST != null && (game.systemData?.honked == true || game.systemData?.fssAllBodies == false))
                this.startWatching();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            this.stopWatching();
        }

        protected override void onJournalEntry(FSSBodySignals entry)
        {
            Game.log($"PlotFSS: FSSBodySignals event: {entry.Bodyname}");
            this.lastFSSBodySignals = entry;
        }

        protected override void onJournalEntry(Scan entry)
        {
            // skip asteroid clusters around a star
            var firstParentKey = entry.Parents?.FirstOrDefault()?.Keys?.FirstOrDefault();
            if (firstParentKey == ParentBodyType.Ring) return;

            Game.log($"PlotFSS: Scan event: {entry.Bodyname}");

            // remember time of last scan + change to waiting or skipped state if already waiting
            this.lastScanTime = DateTime.Now;
            if (this.watchState == State.Waiting) this.watchState = State.Skipped;
            else if (this.watchState != State.Skipped) this.watchState = State.Waiting;

            lastInitialValue = "";
            lastMappedValue = "";
            lastNotes = "";

            // ignore Belt Clusters
            if (entry.Bodyname.Contains("Belt Cluster") || !string.IsNullOrEmpty(entry.StarType))
                return;

            lastBodyName = entry.Bodyname;
            lastWasDiscovered = entry.WasDiscovered;
            if (!entry.WasDiscovered) lastBodyName = $"⚑ {lastBodyName}";

            var suffixes = new List<string>();
            if (entry.TerraformState == "Terraformable" || entry.PlanetClass?.StartsWith("Earth") == true) suffixes.Add("🌎");
            if (entry.Landable) suffixes.Add("🚀");
            if (suffixes.Count > 0) lastBodyName += $"  {string.Join(',', suffixes)}";

            lastInitialValue = Util.GetBodyValue(entry, false).ToString("N0");
            lastMappedValue = Util.GetBodyValue(entry, true).ToString("N0");
            lastNotes = "";

            if (this.lastFSSBodySignals?.BodyID == entry.BodyID)
            {
                var bioSignal = this.lastFSSBodySignals?.Signals.FirstOrDefault(_ => _.Type == "$SAA_SignalType_Biological;");

                if (bioSignal != null)
                {
                    lastNotes = RES("CountBioSignals", bioSignal.Count);

                    //var hasVulcanism = !string.IsNullOrEmpty(entry.Volcanism);
                    //// TODO: consider check for zero or low atmosphere?
                    //// var lowAtmosphere = this.lastScan?.AtmosphereType == "None";
                    //if (hasVulcanism) // && lowAtmosphere)
                    //{
                    //    lastNotes += " | Candidate for Brain Trees?";
                    //}
                }
            }

            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            var col = lastWasDiscovered ? GameColors.Orange : GameColors.Cyan;

            dty = eight;
            drawTextAt2(four, RES("LastScan", lastBodyName), col, GameColors.fontSmaller);

            if (game?.systemData?.lastFssBody != null)
                drawTextAt2(ClientSize.Width - eight, game.systemData.lastFssBody.distanceFromArrivalLS.ToString("N0") + " LS", col, GameColors.fontSmaller, true);
            newLine(true);

            if (string.IsNullOrEmpty(lastBodyName)) return;

            drawTextAt2(oneEight, "► " + RES("EstimatedValue"));
            drawTextAt2(oneEightFour, $"{lastInitialValue} cr");
            newLine(true);
            drawTextAt2(oneEight, "► " + RES("WithSurfaceScan"));
            drawTextAt2(oneEightFour, $"{lastMappedValue} cr");
            newLine(true);

            if (!string.IsNullOrEmpty(lastNotes))
            {
                drawTextAt2(oneEight, "► " + lastNotes, GameColors.Cyan);

                // draw volume bars from predictions
                if (game?.systemData?.lastFssBody?.genusPredictions != null)
                {
                    dtx = (float)Math.Round(dtx) + six;
                    dtx += PlotBioSystem.drawBodyBars(g, game.systemData.lastFssBody, dtx, dty + two, true);

                    var txt = " " + game.systemData.lastFssBody.getMinMaxBioRewards(false);
                    drawTextAt2(txt, GameColors.Cyan);
                }
            }

            // show reminder to icons, if we're watching pixels
            if (Game.settings.watchFssSettings_TEST != null)
            {
                dty = this.Height - fourFour;
                var duration = DateTime.Now - this.lastScanTime;
                //Debug.WriteLine($"!! {this.watchState} / {duration.TotalMilliseconds}");
                if (this.watchState == State.Waiting || duration.TotalMilliseconds < 250)
                    drawTextAt2(this.Width - two, "⏳", null, GameColors.fontBig, true);
                else if (this.watchState == State.Skipped)
                    drawTextAt2(this.Width - two, "✋", null, GameColors.fontBig, true);
                else if (this.watchState == State.Yellow)
                    drawTextAt2(this.Width - two, "📡", Color.Gold, GameColors.fontBig, true);
            }
        }

        #region pixel watching

        private void startWatching()
        {
            if (Game.settings.watchFssSettings_TEST == null) return;

            Game.log("Start watching FSS pixels");
            this.watching = true;
            this.watchState = State.None;

            Task.Run(this.onWatchPixels);
        }

        private void stopWatching()
        {
            Game.log("Stop watching FSS pixels");
            this.watching = false;
            this.watchState = State.None;

            Task.Delay(300).ContinueWith(t =>
            {
                Program.control.BeginInvoke(() => this.Invalidate());
            });
        }

        private void onWatchPixels()
        {
            try
            {
                // exit early if ...
                if (this.IsDisposed || game == null) return;

                // only scan pixels if needed
                if (this.watchState == State.Waiting || this.watchState == State.Skipped)
                    this.analyzeGrab(false);

                Program.control.BeginInvoke(() => this.Invalidate());

                // stop repeating?
                if (game?.systemData?.fssAllBodies == true && this.watchState != State.Waiting && this.watchState != State.Skipped)
                {
                    this.stopWatching();
                    return;
                }
            }
            catch (Exception ex)
            {
                Game.log($"onWatchPixels - error: {ex}");

                // ignore "A generic error occurred in GDI+."
                if (!(ex is System.Runtime.InteropServices.ExternalException))
                {
                    // kill timer
                    stopWatching();

                    // show error
                    Program.control.BeginInvoke(() =>
                    {
                        FormErrorSubmit.Show(ex);
                    });
                    return;
                }
            }

            // go round again?
            if (watching)
                Task.Delay(200).ContinueWith(t => this.onWatchPixels());
        }

        private Bitmap screenGrab(Rectangle rect)
        {
            using (var b = new Bitmap(rect.Width, rect.Height))
            {
                using (var g = Graphics.FromImage(b))
                {
                    g.CopyFromScreen(rect.Left, rect.Top, 0, 0, b.Size);
                    return new Bitmap(b);
                }
            }
        }

        public void analyzeGrab(bool saveDiagnosticImage)
        {
            Rectangle gameRect = Rectangle.Empty;

            // we need to do this on the UX thread, but the rest can be in the background
            Program.control.Invoke(() =>
            {
                gameRect = Elite.getWindowRect();
            });

            // exit early if 
            if (!Elite.focusElite || gameRect == Rectangle.Empty || settings == null) return;

            // grab just the top right quadrant
            gameRect.Width = gameRect.Width / 2;
            gameRect.Height = gameRect.Height / 2;
            gameRect.X += gameRect.Width;

            var grab = this.screenGrab(gameRect);

            // find area to analyze
            var watchArea = this.getWatchArea(gameRect, grab);

            if (watchArea != Rectangle.Empty)
            {
                // analyze it...
                var countWhite = 0;
                var countYellow = 0;

                for (var y = watchArea.Top; y < watchArea.Bottom; y++)
                {
                    for (var x = watchArea.Left; x < watchArea.Right; x++)
                    {
                        var p = grab.GetPixel(x, y);
                        if (Util.isCloseColor(p, settings.whiteText)) countWhite++;
                        if (Util.isCloseColor(p, settings.yellowText)) countYellow++;
                    }
                }

                // save a diagnostic image?
                if (saveDiagnosticImage)
                    dbgSaveGrab(grab, watchArea, $"countWhite: {countWhite} / countYellow: {countYellow}");

                // set state if we found enough coloured pixels
                var newState = this.watchState;
                if (countYellow > 100)
                    newState = State.Yellow;
                else if (countWhite > 100)
                    newState = State.White;

                //Debug.WriteLine($"countWhite: {countWhite} / countYellow: {countYellow} | {this.watchState} => {newState}");
                this.watchState = newState;
            }
        }

        private Rectangle getWatchArea(Rectangle gameRect, Bitmap grab)
        {
            if (settings == null) return Rectangle.Empty;

            var saveGrab = this.settings.saveDebugImages;

            var watchArea = new Rectangle();

            // start at the bottom middle
            var x = gameRect.Width / 2;
            var y = gameRect.Height - 1;

            // move up to find the yellow bar - height
            while (y > 0)
            {
                if (Util.isCloseColor(grab.GetPixel(x, y), settings.yellowBar))
                {
                    // set top as the yellow bar height for now
                    watchArea.Y = y;
                    break;
                }
                y--;
            }
            if (watchArea.Y == 0)
            {
                if (saveGrab) dbgSaveGrab(grab, watchArea, "No yellowHeight");
                return Rectangle.Empty;
            }

            // find the yellow bar - left
            while (x > 0)
            {
                if (!Util.isCloseColor(grab.GetPixel(x, y), settings.yellowBar.color, settings.yellowHorizontalTolerance))
                {
                    watchArea.X = x;
                    break;
                }
                x--;
            }
            if (watchArea.X == 0)
            {
                if (saveGrab) dbgSaveGrab(grab, watchArea, "No yellowLeft");
                return Rectangle.Empty;
            }

            // find the yellow bar - right
            x = gameRect.Width / 2;
            while (x < grab.Width)
            {
                if (!Util.isCloseColor(grab.GetPixel(x, y), settings.yellowBar.color, settings.yellowHorizontalTolerance))
                {
                    watchArea.Width = (x - watchArea.X) / 2;
                    watchArea.X += watchArea.Width;
                    break;
                }
                x++;
            }
            if (watchArea.Width == 0)
            {
                if (saveGrab) dbgSaveGrab(grab, watchArea, "No yellowRight");
                return Rectangle.Empty;
            }

            // find black below yellow - use right edge to avoid body ring pixels
            x -= 30;
            while (y < grab.Height)
            {
                if (Util.isCloseColor(grab.GetPixel(x, y), settings.blackArea))
                {
                    // shift top down to blackness + 1/3 the delta from yellow bar
                    var heightDelta = (y - watchArea.Y) / 3;
                    watchArea.Y = y + heightDelta;
                    watchArea.Height = (gameRect.Height - watchArea.Y - 1) / 2;
                    break;
                }
                y++;
            }
            if (watchArea.Height == 0)
            {
                if (saveGrab) dbgSaveGrab(grab, watchArea, "No blackHeight");
                return Rectangle.Empty;

                // Or ... as bodies with rings may render light pixels where we expect darkness ... use a fixed amount? 
                //watchArea.Y = watchArea.Y * 2;
                //Game.log($"{width} / yellowLeft:{yellowLeft}, yellowRight:{yellowRight}, yellowHeight:{yellowHeight}, blackHeight:{blackHeight}");
            }

            return watchArea;
        }

        private void dbgSaveGrab(Bitmap grab, Rectangle watchArea, string? msg = null)
        {
            var now = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"WatchFSS-{now}.png";
            var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            Game.log($"Saving: {filepath}, with:{msg}");

            using (var g = Graphics.FromImage(grab))
            {
                if (watchArea != Rectangle.Empty)
                {
                    msg = watchArea.ToString() + "\r\n" + msg;

                    if (watchArea.Height == 0) watchArea.Height = 1;
                    if (watchArea.Width == 0) watchArea.Width = 1;

                    g.DrawRectangle(Pens.Red, watchArea);
                }

                if (msg != null)
                    g.DrawString(msg, this.Font, Brushes.Lime, grab.Width / 2, 0);
            }
            grab.Save(filepath);
        }

        internal enum State
        {
            /// <summary>
            /// We are not waiting or watching
            /// </summary>
            None,
            /// <summary>
            /// We're skipped if we wanted to wait but are already waiting
            /// </summary>
            Skipped,
            /// <summary>
            /// We start waiting upon each scan
            /// </summary>
            Waiting,
            /// <summary>
            /// We found white pixels
            /// </summary>
            White,
            /// <summary>
            /// We found yellow pixels
            /// </summary>
            Yellow,
        }

        public class WatchFssPixelSettings
        {
            public bool saveDebugImages = false;
            public WatchColor yellowBar = new WatchColor(193, 156, 65, 60);
            public int yellowHorizontalTolerance = 100;
            public WatchColor blackArea = new WatchColor(0, 0, 0, 30);
            public WatchColor whiteText = new WatchColor(255, 255, 255, 50);
            public WatchColor yellowText = new WatchColor(233, 197, 24, 50);
        }

        #endregion
    }
}

