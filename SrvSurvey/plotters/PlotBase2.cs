using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Resources;
using Res = Loc.PlotGrounded;

namespace SrvSurvey.plotters
{

    internal class PlotDef
    {
        public string name;
        public Func<Game, PlotDef, PlotBase2> ctor;
        /// <summary> Returns true if the plotter is allowed to be shown </summary>
        public Func<Game, bool> allowed;
        public Size defaultSize;

        /// <summary> Factors that govern if this plotter is allowed </summary>
        public HashSet<string>? factors;
        /// <summary> Optional journal event names that should automatically invalidate the plotter </summary>
        public HashSet<string>? invalidationJournalEvents;

        public PlotBase2? instance;
        public PlotContainer? form;
        public VROverlay? vr;

        public override string ToString()
        {
            return $"{name}, instance: {instance != null}, form: {form != null} allowed: {Game.activeGame != null && allowed(Game.activeGame)}";
        }
    }

    internal abstract class PlotBase2
    {
        protected Game game;
        protected PlotDef plotDef;
        protected bool isClosed;
        public string name { get; private set; }
        public int left { get; protected set; }
        public int top { get; protected set; }
        public int width { get; private set; }
        public int right => left + width;
        public int height { get; private set; }
        public int bottom => top + height;
        public float fade;

        public bool stale { get; private set; }
        public Bitmap frame { get; private set; }
        protected Bitmap background;

        public Font font = GameColors.Fonts.gothic_10;
        public Color color = C.orange;

        protected PlotBase2(Game game, PlotDef def)
        {
            this.game = game;
            this.plotDef = def;
            this.name = def.name;

            this.width = def.defaultSize.Width;
            this.height = def.defaultSize.Height;

            this.setPosition();

            this.frame = new Bitmap(this.width, this.height);
            this.stale = true;

            this.rm = this.prepResources(this.GetType().FullName!);
        }

        public Size size => new Size(width, height);
        public Rectangle rect => new Rectangle(left, top, width, height);

        public virtual void setPosition(Rectangle? gameRect = null, string? name = null)
        {
            // get initial position
            var pt = PlotPos.getPlotterLocation(name ?? this.name, this.size, gameRect ?? Rectangle.Empty, true);
            this.left = pt.X;
            this.top = pt.Y;
        }

        public virtual void setSize(Size sz)
        {
            setSize(sz.Width, sz.Height);
        }

        public virtual void setSize(int width, int height)
        {
            if (width == 0 || height == 0) Debugger.Break();
            var changed = this.width != width || this.height != height;
            if (changed)
            {
                this.width = width;
                this.height = height;

                this.setPosition();
                this.invalidate();
            }
        }

        public void close()
        {
            if (plotDef.form != null)
            {
                Program.closePlotter(plotDef.name);
                plotDef.form = null;
            }

            if (plotDef.vr != null)
            {
                plotDef.vr.Dispose();
                plotDef.vr = null;
            }

            this.isClosed = true;
            this.onClose();
        }

        protected virtual void onClose() { /* override as necessary */ }

        #region Resource Managering

        private readonly ResourceManager rm;

        private static Dictionary<string, ResourceManager> resourceManagers = new();

        public ResourceManager prepResources(string name)
        {
            if (!resourceManagers.ContainsKey(name))
                resourceManagers[name] = new ResourceManager(name, typeof(PlotBase2).Assembly);

            return resourceManagers[name];
        }

        protected string RES(string name, ResourceManager? rm = null)
        {
            var txt = (rm ?? this.rm).GetString(name);
            if (txt == null) Debugger.Break();
            return txt ?? "";
        }

        protected string RES(string name, params object?[] args)
        {
            var txt = rm.GetString(name);
            if (txt == null) Debugger.Break();
            return string.Format(txt ?? "", args);
        }

        #endregion

        #region rendering

        public void invalidate()
        {
            //Game.log($"invalidate: {this.name}");
            this.stale = true;

            if (this.plotDef.form != null)
                Program.invalidate(this.name);
            else
                BigOverlay.invalidate();

            this.plotDef.vr?.project();
        }

        public bool hidden
        {
            get => this._hidden;
            set
            {
                if (value) this.hide();
                else this.show();
            }
        }
        private bool _hidden;

        /// <summary>
        /// Stop rendering this plotter without destroying it
        /// </summary>
        public void hide()
        {
            this._hidden = true;
            this.invalidate();

            this.plotDef.vr?.hide();
        }

        /// <summary>
        /// Start rendering if hidden, or invalidates if already visible
        /// </summary>
        public void show()
        {
            if (this._hidden)
            {
                this._hidden = false;
                this.fade = 0;
            }

            this.invalidate();

            this.plotDef.vr?.show();
        }

        // TODO: remove "Game game" reference here - they already have it
        protected abstract SizeF doRender(Game game, Graphics g, TextCursor tt);

        /// <summary> Generate a new frame image </summary>
        public Bitmap render()
        {
            if (game.isShutdown || game.status == null) return this.frame;

            //Game.log($"Render: {this.name}, stale: {this.stale}");
            var renderCount = 0;
            Bitmap? nextFrame = null;
            do
            {
                // re-create background when missing or size has changed
                if (this.background == null || this.background.Size != this.size)
                    this.background = GameGraphics.getBackgroundImage(this.size);

                nextFrame = new Bitmap(this.background);
                using (var g = Graphics.FromImage(nextFrame))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    var tt = new TextCursor(g, this);
                    var sz = this.doRender(game, g, tt);

                    // render a yellow box if we are being adjusted
                    if (FormAdjustOverlay.targetName == this.name)
                    {
                        var rf = new RectangleF(0, 0, sz.Width, sz.Height);
                        g.DrawRectangle(GameColors.penYellow4, rf);
                    }

                    this.stale = false;
                    this.setSize((int)Math.Ceiling(sz.Width), (int)Math.Ceiling(sz.Height));
                }
                ++renderCount;
                if (renderCount > 5)
                {
                    Debug.WriteLine($"Render: {name} #{renderCount} => {this.width}, {this.height}, stale: {stale}");
                    Application.DoEvents();
                    Debugger.Break();
                }
            } while (stale && renderCount < 10);

            //Game.log($"Render: {this.name}, renderCount: {renderCount}");
            this.frame = nextFrame;

            return this.frame;
        }

        #endregion

        #region static def and instance mgmt

        static PlotBase2()
        {
            // start by collecting all non-abstract classes derived from PlotBase2
            var allPlotterTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(_ => typeof(PlotBase2).IsAssignableFrom(_) && !_.IsAbstract)
                .ToList();

            Game.log($"Registering {allPlotterTypes.Count} plotters");

            foreach (var type in allPlotterTypes)
            {
                var def = type
                    .GetField("plotDef", BindingFlags.Public | BindingFlags.Static)
                    ?.GetValue(null) as plotters.PlotDef;

                if (def == null)
                    throw new Exception($"Def not found: {type.Name}");

                defs[def.name] = def;
                PlotPos.typicalSize[def.name] = def.defaultSize;
            }
        }

        private static Dictionary<string, PlotDef> defs = new Dictionary<string, PlotDef>();

        public static PlotBase2 add(Game game, PlotDef def)
        {
            if (def.instance != null)
            {
                Game.log($"Overlays.add: {def.name} - why already present?");
                Debugger.Break();
                remove(def);
            }

            Game.log($"Overlays.add: {def.name}");
            def.instance = def.ctor(game, def);

            renderAll(game);
            return def.instance;
        }

        public static PlotDef? get(string name)
        {
            var def = defs.GetValueOrDefault(name);
            return def;
        }

        public static T? getPlotter<T>() where T : PlotBase2
        {
            var name = typeof(T).Name;
            return defs.GetValueOrDefault(name)?.instance as T;
        }

        public static bool isPlotter<T>() where T : PlotBase2
        {
            var name = typeof(T).Name;
            return defs.GetValueOrDefault(name)?.instance != null;
        }

        public static void remove(PlotDef def)
        {
            if (def.instance != null)
            {
                Game.log($"Overlays.remove: {def.name}");

                def.instance.close();
                def.instance = null;

                BigOverlay.invalidate();
            }
        }

        public static void invalidate(params string[] names)
        {
            var notFound = false;

            foreach (var name in names)
            {
                var def = defs.GetValueOrDefault(name);
                if (def?.instance != null)
                    def.instance.invalidate();
                else
                    notFound = true;
            }

            if (notFound) BigOverlay.invalidate();
        }

        public static T? addOrRemove<T>(Game? game) where T : PlotBase2
        {
            var type = typeof(T);
            var plotDef = type.GetField("plotDef", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as PlotDef;
            if (plotDef == null) throw new Exception($"Why no plotDef for: {type.Name}");
            return addOrRemove(game, plotDef) as T;
        }

        public static PlotBase2? addOrRemove(Game? game, PlotDef def)
        {
            var shouldShow = game?.cmdr != null && def.allowed(game);

            if (shouldShow && def.instance == null && game != null)
            {
                add(game, def);
            }
            else if (shouldShow && def.instance != null)
            {
                def.instance.invalidate();
            }
            else if (!shouldShow && def.instance != null)
            {
                remove(def);
            }

            return def.instance;
        }

        public static void renderAll(Game? game, bool force = false)
        {
            if (Program.control.InvokeRequired)
            {
                Program.defer(() => renderAll(game, force));
                return;
            }

            if (game == null || game.isShutdown || game.status == null || game.cmdr == null) return;

            // check if we need to create or remove any
            var invalidateBig = false;
            foreach (var def in defs.Values)
            {
                // nothing should show at the main menu, or before the cmdr is known
                var shouldShow = game.cmdr != null
                    && game.status != null
                    && !game.atMainMenu
                    && !game.isShutdown
                    && !game.hidePlottersFromCombatSuits
                    && def.allowed(game);

                //Game.log($"relevant? {def.name} => {def.instance != null} / shouldShow: {shouldShow}");

                // only attempt to create or destroy if something relevant changed
                if (shouldShow && def.instance == null)
                {
                    invalidateBig = true;
                    add(game, def);
                }
                else if (!shouldShow && def.instance != null)
                {
                    invalidateBig = true;
                    remove(def);
                }

                if (shouldShow && def.instance != null)
                {
                    // render if stale or if a relevant factor changed
                    if (def.instance.stale || force)
                    {
                        invalidateBig = true;
                        def.instance.render();
                    }

                    // render onto separate form?
                    if (Game.settings.disableLargeOverlay || PlotPos.shouldBeSeparate(def.name))
                    {
                        if (def.form == null)
                        {
                            def.form = new PlotContainer(def);
                            Program.showPlotter<PlotContainer>(null, def.name);
                        }
                        def.form.updateFrame();
                    }

                    // render into VR?
                    if (VR.enabled && VR.app != null)
                    {
                        if (def.vr == null) def.vr = new VROverlay(def);
                        def.vr.project();
                    }
                }
            }

            if (invalidateBig)
                BigOverlay.current?.Invalidate();
        }

        public static void closeAll()
        {
            Game.log($"Overlays.closeAll");

            foreach (var def in defs.Values)
                remove(def);

            BigOverlay.invalidate();
        }

        /// <summary>
        /// The list of active plotters to be rendered
        /// </summary>
        public static List<PlotBase2> active
        {
            get => defs.Values
                .Where(def => def.instance != null && def.form == null)
                .Select(def => def.instance!)
                .ToList<PlotBase2>();
        }

        #endregion

        #region journal and status file processing

        /// <summary> Gives every active plotter a chance to process status changes </summary>
        public static void processstatusChanged()
        {
            foreach (var def in defs.Values)
                def.instance?.onStatusChange(def.instance.game.status);
        }

        public static void processJournalEntry(IJournalEntry entry)
        {
            foreach (var def in defs.Values)
            {
                if (def.instance == null) continue;

                // pass events to active plotter
                def.instance.onJournalEntry((dynamic)entry);

                // invalidate it if this is one of the declared journal entry names
                if (def.invalidationJournalEvents?.Contains(entry.@event) == true)
                    def.instance.invalidate();
            }
        }

        protected virtual void onStatusChange(Status status) { /* override as needed */ }

        protected virtual void onJournalEntry(JournalEntry entry) { /* override as needed */ }

        protected virtual void onJournalEntry(BackpackChange entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(CodexEntry entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DataScanned entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Disembark entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Docked entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingCancelled entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingDenied entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingGranted entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingRequested entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Embark entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FactionKillBond entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSDJump entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSDTarget entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSSBodySignals entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSSDiscoveryScan entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(MaterialCollected entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Music entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(NavRoute entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(NavRouteClear entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Scan entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(ScanOrganic entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Screenshot entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(SendText entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(SupercruiseEntry entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Touchdown entry) { /* overridden as necessary */ }

        #endregion
    }

    internal abstract class PlotBase2Surface : PlotBase2
    {
        /// <summary> The center point on this plotter. </summary>
        protected Size mid;

        /// <summary>
        /// A automatically set scaling factor to apply to plotter rendering
        /// </summary>
        public float scale = 1.0f;

        /// <summary>
        /// A manually set scaling factor given by users with the `z` command.
        /// </summary>
        public float customScale = -1.0f;

        protected decimal radius { get => game.systemBody?.radius ?? 0; }

        protected PlotBase2Surface(Game game, PlotDef def) : base(game, def) { }

        public override void setSize(int width, int height)
        {
            base.setSize(width, height);
            this.mid = this.size / 2;
        }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            this.invalidate();
        }

        protected void clipToMiddle(Graphics g)
        {
            this.clipToMiddle(g, N.four, N.twoSix, N.four, N.twoFour);
        }

        protected void clipToMiddle(Graphics g, float left, float top, float right, float bottom)
        {
            if (g == null) return;

            g.ResetClip();
            var r = new RectangleF(left, top, this.width - left - right, this.height - top - bottom);
            g.Clip = new Region(r);
        }

        /// <summary> center in middle and NOT scaled </summary>
        protected void resetMiddle(Graphics g)
        {
            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
        }

        /// <summary> center in middle, scaled and rotated by player's heading </summary>
        protected void resetMiddleRotated(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(-game.status.Heading);
        }

        protected void drawCommander(Graphics g)
        {
            float sz = N.five;
            g.DrawEllipse(GameColors.penLime2, -sz, -sz, sz * 2, sz * 2);
            g.DrawLine(GameColors.penLime2, 0, 0, 0, sz * -2);
        }

        /// <summary> Centered on current origin </summary>
        protected virtual void drawCompassLines(Graphics g)
        {
            // draw black background checks
            g.FillRectangle(GameColors.adjustGroundChecks(scale), -width, -height, width * 2, height * 2);

            g.RotateTransform(-game.status.Heading);

            // draw compass rose lines centered on the commander
            var w2 = this.width * 2;
            var h2 = this.height * 2;
            g.DrawLine(Pens.DarkRed, -w2, 0, +w2, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +h2);
            g.DrawLine(Pens.Red, 0, -h2, 0, 0);

            g.RotateTransform(+game.status.Heading);
        }

        /// <summary> May reset graphics transform </summary>
        protected virtual void drawShipAndSrvLocation(Graphics g, TextCursor tt)
        {
            if (g == null || game.systemBody == null) return;

            // draw SRV marker
            if (game.srvLocation != null)
            {
                var srvSize = 10f / this.scale;

                var srv = Util.getOffset(this.radius, game.srvLocation, 180);
                var rect = new RectangleF(
                    (float)srv.X - srvSize,
                    (float)-srv.Y - srvSize,
                    srvSize * 2,
                    srvSize * 2);

                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }

            // draw touchdown marker
            if (game.cmdr.lastTouchdownLocation != null)
            {
                RectangleF rect;
                var shipSize = 24f / this.scale;
                var shipLatLong = game.cmdr.lastTouchdownLocation;
                var ship = Util.getOffset(game.status.PlanetRadius, shipLatLong, 180);

                // take advantage of last heading and offset by ship's center
                var po = CanonnStation.getShipOffset(game.currentShip.type);
                var pd = po.rotate(game.cmdr.lastTouchdownHeading);
                ship += pd;

                rect = new RectangleF(
                    (float)ship.X - shipSize,
                    (float)-ship.Y - shipSize,
                    shipSize * 2,
                    shipSize * 2);

                // draw ship circle
                var shipDeparted = game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty;
                var brush = shipDeparted ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;
                g.FillEllipse(brush, rect);

                // draw 2km ship departure perimeter
                var shipDist = Util.getDistance(Status.here, shipLatLong, this.radius);
                if (!shipDeparted && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
                {
                    const float liftoffSize = 2000f;
                    rect = new RectangleF((float)ship.X - liftoffSize, -(float)ship.Y - liftoffSize, liftoffSize * 2, liftoffSize * 2);
                    var p = GameColors.penShipDepartFar;
                    if (shipDist > 1800)
                        p = GameColors.penShipDepartNear;

                    g.DrawEllipse(p, rect);

                    // TODO: fix bug where warning shown and ship already departed
                    // and the warning message box
                    if (shipDist > 1800)
                    {
                        g.ResetTransform();
                        var msg = Res.NearingShipDistance;
                        var font = GameColors.fontSmall;
                        var sz = g.MeasureString(msg, font);
                        var tx = Util.centerIn(this.width, sz.Width);
                        var ty = N.fourTwo;

                        int pad = 14;
                        rect = new RectangleF(tx - pad, ty - pad, sz.Width + pad * 2, sz.Height + pad * 2);
                        g.FillRectangle(GameColors.brushShipDismissWarning, rect);

                        rect.Inflate(-10, -10);
                        g.FillRectangle(Brushes.Black, rect);
                        g.DrawString(msg, font, Brushes.Red, tx + 1, ty + 1);
                    }
                }
            }
        }
    }

    internal abstract class PlotBase2Site : PlotBase2Surface
    {
        protected LatLong2 siteLocation;
        protected float siteHeading;
        /// <summary>The cmdr's distance from the site origin</summary>
        protected decimal distToSiteOrigin;
        /// <summary>The cmdr's offset against the site origin ignoring site.heading</summary>
        protected PointF offsetWithoutHeading;
        /// <summary>The cmdr's offset against the site origin including site.heading</summary>
        protected PointF cmdrOffset;
        /// <summary>The cmdr's heading relative to the site.heading</summary>
        protected float cmdrHeading;

        protected Image? mapImage;
        protected Point mapCenter;
        protected float mapScale;

        protected PlotBase2Site(Game game, PlotDef def) : base(game, def) { }

        protected override void onStatusChange(Status status)
        {
            base.onStatusChange(status);
            if (game?.status == null || game.systemBody == null) return;

            this.distToSiteOrigin = Util.getDistance(siteLocation, Status.here, this.radius);
            this.offsetWithoutHeading = (PointF)Util.getOffset(this.radius, this.siteLocation); // explicitly EXCLUDING site.heading
            this.cmdrOffset = (PointF)Util.getOffset(radius, siteLocation, siteHeading); // explicitly INCLUDING site.heading
            this.cmdrHeading = game.status.Heading - siteHeading;
        }

        protected void resetMiddleSiteOrigin(Graphics g)
        {
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height); // shift to center of window
            g.ScaleTransform(scale, scale); // apply display scale factor (zoom)
            g.RotateTransform(-game.status.Heading); // rotate by cmdr heading
            g.TranslateTransform(-offsetWithoutHeading.X, offsetWithoutHeading.Y); // shift relative to cmdr
            g.RotateTransform(this.siteHeading); // rotate by site heading
        }

        /// <summary>
        /// Adjust graphics transform, calls the lambda then reverses the adjustments.
        /// </summary>
        protected void adjust(Graphics g, PointF pf, float rot, Action func)
        {
            adjust(g, pf.X, pf.Y, rot, func);
        }

        protected void adjust(Graphics g, float x, float y, float rot, Action func)
        {
            // Y value only is inverted
            g.TranslateTransform(+x, -y);
            g.RotateTransform(+rot);

            func();

            g.RotateTransform(-rot);
            g.TranslateTransform(-x, +y);
        }

        protected void drawMapImage(Graphics g)
        {
            if (this.mapImage == null || this.mapScale == 0 || this.mapCenter == Point.Empty) return;

            var mx = this.mapCenter.X * this.mapScale;
            var my = this.mapCenter.Y * this.mapScale;

            var sx = this.mapImage.Width * this.mapScale;
            var sy = this.mapImage.Height * this.mapScale;

            g.DrawImage(this.mapImage, -mx, -my, sx, sy);
        }

        /// <summary>
        /// Centered on site origin
        /// </summary>
        protected void drawSiteCompassLines(Graphics g)
        {
            adjust(g, PointF.Empty, -siteHeading, () =>
            {
                // draw compass rose lines centered on the site origin
                g.DrawLine(GameColors.penDarkRed2Ish, -1500, 0, +1500, 0);
                g.DrawLine(GameColors.penDarkRed2Ish, 0, -1500, 0, +1500);
                //g.DrawLine(GameColors.penRed2Ish, 0, -500, 0, 0);
            });

            // and a line to represent "north" relative to the site - visualizing the site's rotation
            if (this.siteHeading >= 0)
                g.DrawLine(GameColors.penRed2DashedIshIsh, 0, -1500, 0, 0);
        }
    }

    internal class PlotContainer : PlotBase
    {
        private readonly PlotDef def;

        public PlotContainer(PlotDef def)
        {
            this.def = def;
            this.Name = def.name;

            this.Left = def.instance!.left;
            this.Top = def.instance.top;

            this.resetOpacity();
        }

        protected override void Dispose(bool disposing)
        {
            this.def.form = null;

            base.Dispose(disposing);
        }

        public override bool allow => Game.activeGame == null ? false : def.allowed(Game.activeGame);

        private void checkLocationAndSize()
        {
            if (def.instance == null) return;

            if (this.Width != def.instance.width || this.Height != def.instance.height)
            {
                Game.log($"resize: {def.name} => {def.instance.size}");
                var gameRect = Elite.getWindowRect();
                var pt = PlotPos.getPlotterLocation(this.Name, def.instance.size, gameRect, false);
                this.SetBounds(pt.X, pt.Y, def.instance.width, def.instance.height);
            }

            if (this.Visible == def.instance.hidden)
                this.Visible = !def.instance.hidden;
        }

        public void updateFrame()
        {
            checkLocationAndSize();
            this.Invalidate();
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || def.instance == null || Game.activeGame == null) return;

            if (def.instance.frame == null || def.instance.stale)
                def.instance.render();

            checkLocationAndSize();

            g.DrawImage(def.instance.frame!, 0, 0);

            // g.DrawRectangle(Pens.Blue, 0, 0, this.Width-1, this.Height-1); // diagnostic
        }

    }
}
