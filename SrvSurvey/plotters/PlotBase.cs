using Newtonsoft.Json;
using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Resources;
using System.Text;

namespace SrvSurvey.plotters
{
    /// <summary>
    /// A base class for all plotters
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    internal abstract partial class PlotBase : Form, PlotterForm, IDisposable
    {
        protected Game game = Game.activeGame!;
        public TrackingDelta? touchdownLocation0; // TODO: move to PlotSurfaceBase // make protected again
        protected TrackingDelta? srvLocation0; // TODO: move to PlotSurfaceBase

        /// <summary> The center point on this plotter. </summary>
        protected Size mid;
        protected Graphics g;

        /// <summary>
        /// A automatically set scaling factor to apply to plotter rendering
        /// </summary>
        public float scale = 1.0f;
        /// <summary>
        /// A manually set scaling factor given by users with the `z` command.
        /// </summary>
        public float customScale = -1.0f;
        public bool didFirstPaint { get; set; }
        private bool forceRepaint;
        public bool showing { get; set; }
        public bool fading { get; set; }

        protected PlotBase()
        {
            this.Cursor = Cursors.Cross;
            this.BackColor = Color.Black;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0; // ok
            this.DoubleBuffered = true;
            this.Name = this.GetType().Name;
            this.ResizeRedraw = true;
            if (Game.settings.fadeInDuration == 0)
                this.Size = Size.Empty;
            else
                this.Size = new Size(640, 640);

            if (this.game == null) throw new Exception("Why no active game?");

            if (game.systemData != null && game.systemBody != null) // retire
            {
                this.touchdownLocation0 = new TrackingDelta(
                    game.systemBody.radius,
                    game.touchdownLocation ?? LatLong2.Empty);
            }

            rm = this.prepResources(this.GetType().FullName!);

            // Does this cause windows to become visible when alt-tabbing?
            this.Text = this.Name;
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 + 0x00080000 + 0x08000000; // WS_EX_TRANSPARENT + WS_EX_LAYERED + WS_EX_NOACTIVATE
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.FormBorderStyle = FormBorderStyle.None;
            System.Windows.Forms.Cursor.Show();
        }

        protected override void OnActivated(EventArgs e)
        {
            //Game.log($"!!!! OnActivated: {this.Name}. Mouse is:{Cursor.Position}");

            // plotters are not suppose to receive focus - force it back onto the game if we do
            base.OnActivated(e);

            if (!this.showing || Elite.focusElite)
            {
                Elite.setFocusED();
                this.Invalidate();
            }
            else if (Game.settings.forceRefocusOnPlotterActivate)
            {
                Program.defer(() =>
                {
                    Program.defer(() =>
                    {
                        Elite.setFocusED();
                    });
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (game != null)
                {
                    Game.update -= Game_modeChanged;
                    if (game.status != null)
                        game.status.StatusChanged -= Status_StatusChanged;
                    if (game.journals != null)
                        game.journals.onJournalEntry -= Journals_onJournalEntry;
                    game = null!;
                }
            }
        }

        #region mouse handlers

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (Game.settings.hideOverlaysFromMouseInFSS_TEST)
            {
                HideAndReturnWhenMouseMoves(this);
            }
            else if (Game.settings.hideOverlaysFromMouse)
            {
                // move the mouse outside the overlay
                Game.log($"OnMouseEnter: {this.Name}. Mouse is:{Cursor.Position}, moving to: {Elite.gameCenter}");
                Cursor.Position = Elite.gameCenter;
            }
            else
            {
                Game.log($"OnMouseEnter: {this.Name}. Mouse is:{Cursor.Position}, would have moved to: {Elite.gameCenter}");
            }
        }

        public static void HideAndReturnWhenMouseMoves(Form form)
        {
            Game.log($"OnMouseEnter: {form.Name}. Mouse was:{Cursor.Position}, hiding overlay ...");
            form.Visible = false;
            var rect = form.Bounds;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (form.IsDisposed) return;

                    // frequently check the mouse position ... become visible again if the mouse is outside
                    await Task.Delay(500);

                    if (rect.Contains(Cursor.Position))
                    {
                        Game.log($"OnMouseEnter: {form.Name}. Mouse is:{Cursor.Position} - still inside");
                    }
                    else
                    {
                        Game.log($"OnMouseEnter: {form.Name}. Mouse is:{Cursor.Position} - outside");
                        form.Invoke(() =>
                        {
                            form.Visible = true;
                        });
                        break;
                    }
                }
            });
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            //Game.log($"!!!! OnMouseDown: {this.Name}. Mouse is:{Cursor.Position}");
            base.OnMouseDown(e);

            this.Invalidate();
            Elite.setFocusED();
        }

        #endregion

        /// <summary> When true, forces zero Opacity </summary>
        public bool forceHide
        {
            get => this._forceHide;
            set
            {
                this._forceHide = value;

                if (this._forceHide)
                    this.setOpacity(0);
                else
                    this.resetOpacity();

                this.Invalidate();
            }
        }
        private bool _forceHide;

        /// <summary> Reset opacity to default it's value </summary>
        public virtual void resetOpacity()
        {
            setOpacity(PlotPos.getOpacity(this));
        }

        /// <summary> Set opacity to the given value. </summary>
        public virtual void setOpacity(double newOpacity)
        {
            // enforce zero opacity if we're force hiding
            if (this.forceHide)
                newOpacity = 0;

            if (this.Opacity != newOpacity || (newOpacity == 1 && this.Opacity != 0.9999f))
            {
                // for reasons unknown ... an opacity of `1` sometimes means the window is not visible, even though it thinks it is :/
                if (newOpacity == 1)
                    this.Opacity = 0.9999f; // ok
                else
                    this.Opacity = newOpacity; // ok
            }
        }

        public virtual void reposition(Rectangle gameRect)
        {
            // do not attempt to reposition anything if the game window has been minimized
            //Game.log($"reposition:{this.Name}: opacity:{Opacity}, bounds:{this.Bounds}, gameRect:{gameRect}");
            if (gameRect.X < -30_000 || gameRect.Y < -30_000 || gameRect.Width == 0 || gameRect.Height == 0)
                return;

            // restore opacity, reposition ourselves according to plotters.json rules, then re-render
            var newOpacity = PlotPos.getOpacity(this);
            if (this.Opacity == 0 && newOpacity > 0)
                Util.fadeOpacity(this, newOpacity, Game.settings.fadeInDuration);
            else
                setOpacity(newOpacity);

            if (gameRect != Rectangle.Empty)
            {
                PlotPos.reposition(this, gameRect, this.Name);
                this.Invalidate();
            }
        }

        protected virtual void initializeOnLoad()
        {
            if (this.IsDisposed) return;

            this.reposition(Elite.getWindowRect());
            this.mid = this.Size / 2;

            if (this.IsDisposed) return;

            this.BackgroundImage = GameGraphics.getBackgroundImage(this);

            if (game != null)
            {
                Game.update += Game_modeChanged;
                if (game.status != null)
                    game.status.StatusChanged += Status_StatusChanged;
                if (game.journals != null)
                    game.journals!.onJournalEntry += Journals_onJournalEntry;
            }

            this.Status_StatusChanged(false);
        }

        /// <summary>
        /// Returns True if this plotter is allowed to exist currently
        /// </summary>
        public abstract bool allow { get; }

        protected virtual void Game_modeChanged(GameMode newMode, bool force)
        {
            if (this.IsDisposed) return;

            if (this.Opacity > 0 && !this.allow)
                this.setOpacity(0);
            else if (this.Opacity == 0 && this.allow)
                this.reposition(Elite.getWindowRect());

            this.Invalidate();
        }

        protected virtual void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            // TODO: retire
            if (this.touchdownLocation0 != null)
                this.touchdownLocation0.Current = Status.here;

            // TODO: retire
            if (this.srvLocation0 != null)
                this.srvLocation0.Current = Status.here;

            this.Invalidate();
        }

        #region journal processing

        protected void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            if (this.IsDisposed) return;

            // We need a strongly typed stub in this base class for any journal event any derived class would like to receive
            this.onJournalEntry((dynamic)entry);
        }

        protected void onJournalEntry(JournalEntry entry) { /* no-op */ }

        protected virtual void onJournalEntry(Touchdown entry)
        {
            if (game.systemBody == null) return;

            // TODO: retire
            if (this.touchdownLocation0 == null)
            {
                this.touchdownLocation0 = new TrackingDelta(
                    game.systemBody!.radius,
                    entry);
            }
            else
            {
                this.touchdownLocation0.Target = entry;
            }

            this.Invalidate();
        }
        protected virtual void onJournalEntry(Disembark entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Embark entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(SendText entry)
        {
            var msg = entry.Message.ToLowerInvariant().Trim();

            // adjust the zoom factor 'z <number>'
            if (msg == MsgCmd.z)
            {
                Game.log($"Resetting custom zoom scale");
                this.customScale = -1f;
                this.Invalidate();
                return;
            }
            else if (msg.StartsWith(MsgCmd.z))
            {
                if (float.TryParse(entry.Message.Substring(1), out this.customScale))
                {
                    this.customScale = (float)Math.Max(customScale, 0.1);
                    this.customScale = (float)Math.Min(customScale, 20);
                    Game.log($"Changing custom zoom scale from: '{this.scale}' to: '{this.customScale}'");
                    this.scale = this.customScale;
                    this.Invalidate();
                    return;
                }
            }
        }
        protected virtual void onJournalEntry(CodexEntry entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSDTarget entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Screenshot entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DataScanned entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(BackpackChange entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(SupercruiseEntry entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSDJump entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(NavRoute entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(NavRouteClear entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSSDiscoveryScan entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Scan entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FSSBodySignals entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(ScanOrganic entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingRequested entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingGranted entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingDenied entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(DockingCancelled entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Docked entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(MaterialCollected entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(Music entry) { /* overridden as necessary */ }
        protected virtual void onJournalEntry(FactionKillBond entry) { /* overridden as necessary */ }

        #endregion

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.IsDisposed || game == null || game.isShutdown) return;

            try
            {
                this.g = e.Graphics;
                this.g.SmoothingMode = SmoothingMode.HighQuality;
                this.formSize = new SizeF(2, 0);
                this.dtx = eight;
                this.dty = ten;
                this.forceRepaint = false;

                //Game.log($"Paint {this.Name} {this.Size} // {this.BackgroundImage!.Size}");

                // force draw the background as there may be a visible delay when the form size changes
                if (this.BackgroundImage != null)
                    g.DrawImage(this.BackgroundImage, 0, 0);
                onPaintPlotter(e);

                //Game.log($"FirstPaint? {this.Name} {firstPaint} {this.Opacity} {this.Size} (doRepaint: {doRepaint}) // {this.BackgroundImage?.Size}");

                if (forceRepaint)
                {
                    // exit early if our `g` reference has changed - it means we painted already async before this one finished
                    if (this.g != e.Graphics) return;

                    if (this.formSize.Width < this.Size.Width || this.formSize.Height < this.Size.Height)
                        this.hideMyClone();

                    g.FillRectangle(C.Brushes.black, 0, 0, this.Width, this.Height);
                    this.formSize = new SizeF(2, 0);
                    this.dtx = eight;
                    this.dty = ten;
                    if (this.BackgroundImage != null)
                        g.DrawImage(this.BackgroundImage, 0, 0);
                    onPaintPlotter(e);
                }

                if (Game.settings.streamOneOverlay && !this.fading)
                    this.cloneMe();

                if (!didFirstPaint)
                {
                    didFirstPaint = true;
                    var targetOpacity = PlotPos.getOpacity(this);
                    if (targetOpacity != this.Opacity)
                    {
                        Program.control.BeginInvoke(() =>
                        {
                            Util.fadeOpacity(this, targetOpacity, Game.settings.fadeInDuration);
                        });
                    }
                }

                if (FormAdjustOverlay.targetName == this.Name && g != null)
                    ifAdjustmentTarget(g, this);
            }
            catch (Exception ex)
            {
                // ignore this entirely
                if (ex is ArgumentException && (ex.TargetSite?.Name == "MeasureString" || ex.TargetSite?.Name == "CheckErrorStatus"))
                {
                    Debugger.Break(); // Is this still happening?
                    return;
                }
                Game.log($"{this.GetType().Name}.OnPaintBackground error: {ex}");
            }
            finally
            {
                this.g = null!;
            }
        }

        private void cloneMe()
        {
            try
            {
                if (!Game.settings.streamOneOverlay || this.fading || windowOne == null || backOne == null) return;

                var er = Elite.getWindowRect();
                var x = this.Left - er.Left;
                var y = this.Top - er.Top;

                if (this.Opacity == 0 || !this.Visible || x < 0 || y < 0)
                {
                    using (var g2 = Graphics.FromImage(backOne))
                    {
                        g2.FillRectangle(brushOne, this.lastOne);
                    }
                }
                else
                {
                    this.lastOne = new Rectangle(x, y, this.Width, this.Height);
                    using (var g2 = Graphics.FromImage(backOne))
                    {
                        g2.FillRectangle(brushOne, this.lastOne);
                        this.DrawToBitmap(backOne, this.lastOne);
                    }
                }

                Program.defer(() => windowOne.Invalidate());
            }
            catch { }
        }

        /// <summary> Show an obvious border if this plotter is getting adjusted </summary>
        public static void ifAdjustmentTarget(Graphics g, Form f)
        {
            g.ResetClip();
            g.ResetTransform();
            g.SmoothingMode = SmoothingMode.None;

            var p = GameColors.penYellow4;
            g.DrawRectangle(p, p.Width / 2, p.Width / 2, f.Width - p.Width, f.Height - p.Width);
        }

        protected virtual void onPaintPlotter(PaintEventArgs e) { /* TODO: make abstract */ }

        protected void drawCommander0()
        {
            if (g == null) return;

            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.RotateTransform(360 - game.status!.Heading);

            var locSz = 5f;
            g.DrawEllipse(GameColors.penLime2, -locSz, -locSz, locSz * 2, locSz * 2);
            var dx = (float)Math.Sin(Util.degToRad(game.status.Heading)) * 10F;
            var dy = (float)Math.Cos(Util.degToRad(game.status.Heading)) * 10F;
            g.DrawLine(GameColors.penLime2, 0, 0, +dx, -dy);
        }

        protected void clipToMiddle()
        {
            this.clipToMiddle(four, twoSix, four, twoFour);
        }

        protected void clipToMiddle(float left, float top, float right, float bottom)
        {
            if (g == null) return;

            g.ResetClip();
            var r = new RectangleF(left, top, this.Width - left - right, this.Height - top - bottom);
            g.Clip = new Region(r);
        }

        protected void drawCompassLines(int heading = -1)
        {
            if (g == null) return;

            if (heading == -1) heading = game.status!.Heading;

            g.ResetTransform();
            this.clipToMiddle();

            g.TranslateTransform(mid.Width, mid.Height);

            // draw compass rose lines
            g.RotateTransform(360 - heading);
            g.DrawLine(Pens.DarkRed, -this.Width, 0, +this.Width, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height);
            g.DrawLine(Pens.Red, 0, -this.Height, 0, 0);
            g.ResetClip();
        }

        protected void drawTouchdownAndSrvLocation0(bool hideHeader = false)
        {
            if (g == null || (this.touchdownLocation0 == null && this.srvLocation0 == null)) return;

            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(360 - game.status!.Heading);

            // draw touchdown marker
            if (game.touchdownLocation != null && game.systemBody != null)
            {
                const float shipSize = 24f;
                var shipLatLong = game.touchdownLocation;
                var ship = Util.getOffset(game.systemBody.radius, shipLatLong, 180);

                // adjust location by ship cockpit offset
                var po = CanonnStation.getShipOffset(game.currentShip.type);
                var pd = po.rotate(game.cmdr.lastTouchdownHeading);
                ship += pd;

                var rect = new RectangleF(
                    (float)ship.X - shipSize,
                    (float)-ship.Y - shipSize,
                    shipSize * 2,
                    shipSize * 2);

                var shipDeparted = game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty;
                var brush = shipDeparted ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;

                g.FillEllipse(brush, rect);
            }

            // draw SRV marker
            if (game.srvLocation != null)
            {
                var offset = Util.getOffset(game.status.PlanetRadius, game.srvLocation, 180);
                const float srvSize = 10f;
                var rect = new RectangleF(
                    (float)offset.X - srvSize,
                    (float)-offset.Y - srvSize,
                    srvSize * 2,
                    srvSize * 2);

                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }

            g.ResetTransform();

            if (!hideHeader)
            {
                if (this.touchdownLocation0 != null)
                    this.drawBearingTo(4, 10, "Touchdown:", this.touchdownLocation0.Target);

                if (this.srvLocation0 != null)
                    this.drawBearingTo(4 + mid.Width, 10, "SRV:", this.srvLocation0.Target);
            }
        }

        protected void drawBearingTo(float x, float y, string txt, LatLong2 location)
        {
            var dd = new TrackingDelta(game.systemBody!.radius, location);
            Angle deg = dd.angle - game.status!.Heading;

            drawBearingTo(x, y, txt, (double)dd.distance, (double)deg);
        }

        protected void drawBearingTo(float x, float y, string txt, double dist, double deg, Brush? brush = null, Pen? pen = null)
        {
            if (g == null) return;
            if (brush == null) brush = GameColors.brushGameOrange;
            if (pen == null) pen = GameColors.penGameOrange2;

            if (!string.IsNullOrEmpty(txt))
            {
                //var txt = scan == game.nearBody.scanOne ? "Scan one:" : "Scan two:";
                g.DrawString(txt, GameColors.fontSmall, brush, x, y);
            }

            var txtSz = g.MeasureString(txt, GameColors.fontSmall);

            var sz = five;
            x += txtSz.Width + eight;
            //y += 4;
            var r = new RectangleF(x, y, sz * 2, sz * 2);
            g.DrawEllipse(pen, r);

            // always point up if the distance is zero
            if (dist == 0) deg = 0;
            var dx = (float)Math.Sin(Util.degToRad(deg)) * nine;
            var dy = (float)Math.Cos(Util.degToRad(deg)) * nine;
            g.DrawLine(pen, x + sz, y + sz, x + sz + dx, y + sz - dy);

            x += 2 + sz * 3;
            g.DrawString(Util.metersToString(dist), GameColors.fontSmall, brush, x, y);
        }

        protected SizeF drawHeaderText(string msg, Brush? brush = null)
        {
            if (g == null) return Size.Empty;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            var font = GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = Util.centerIn(this.Width, sz.Width);
            var ty = 5;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
            return sz;
        }

        protected SizeF drawFooterText(string msg, Brush? brush = null, Font? font = null)
        {
            if (g == null) return Size.Empty;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();

            font = font ?? GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = Util.centerIn(this.Width, sz.Width);
            var ty = this.Height - sz.Height - 6;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
            return sz;
        }

        protected void drawCenterMessage(string msg, Brush? brush = null)
        {
            var font = GameColors.fontMiddle;
            var sz = g.MeasureString(msg, font);
            var tx = Util.centerIn(this.Width, sz.Width);
            var ty = 34;

            g.DrawString(msg, font, brush ?? GameColors.brushGameOrange, tx, ty);
        }

        protected void strikeThrough(float x, float y, float w, bool highlight)
        {
            var c1 = highlight ? GameColors.penCyan1 : GameColors.penGameOrange1;
            var c2 = highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1;

            x = (int)x;
            y = (int)y;

            g.DrawLine(c1, x, y, x + w, y);
            g.DrawLine(c2, x + 1, y + 1, x + w + 1, y + 1);
        }

        #region drawing text and resizing form

        /// <summary> The x location to use in drawTextAt</summary>
        protected float dtx;
        /// <summary> The y location to use in drawTextAt</summary>
        protected float dty;

        /// <summary>
        /// Draws text at the location of ( dtx, dty ) incrementing dtx by the width of the rendered string.
        /// </summary>
        protected SizeF drawTextAt(string? txt)
        {
            return this.drawTextAt(txt, null, null);
        }

        protected SizeF drawTextAt(float tx, string? txt)
        {
            return this.drawTextAt(tx, txt, null, null);
        }

        /// <summary>
        /// Draws text at the location of ( dtx, dty ) incrementing dtx by the width of the rendered string.
        /// </summary>
        protected SizeF drawTextAt(string? txt, Font? font = null)
        {
            return this.drawTextAt(txt, null, font);
        }

        protected SizeF drawTextAt(float tx, string? txt, Font? font = null)
        {
            return this.drawTextAt(tx, txt, null, font);
        }

        protected SizeF drawTextAt(string? txt, Brush? brush = null, Font? font = null)
        {
            return drawTextAt(this.dtx, txt, brush, font);
        }

        /// <summary>
        /// Draws text at the location of ( dtx, dty ) incrementing dtx by the width of the rendered string.
        /// </summary>
        protected SizeF drawTextAt(float tx, string? txt, Brush? brush = null, Font? font = null, bool rightAlign = false)
        {
            this.dtx = tx;
            if (txt == null) return Size.Empty;

            brush = brush ?? GameColors.brushGameOrange;
            font = font ?? this.Font;
            this.lastTextSize = g.MeasureString(txt, font);

            var stringFormat = StringFormat.GenericDefault;

            if (rightAlign)
            {
                var x = dtx - this.lastTextSize.Width;
                g.DrawString(txt, font, brush, x, this.dty, stringFormat);
            }
            else
            {
                g.DrawString(txt, font, brush, this.dtx, this.dty, stringFormat);
                this.dtx += this.lastTextSize.Width;
            }

            return this.lastTextSize;
        }


        protected SizeF drawTextAt2(string? txt)
        {
            return drawTextAt2(dtx, txt, null, null);
        }
        protected SizeF drawTextAt2(string? txt, Font? font = null)
        {
            return drawTextAt2(dtx, txt, null, font);
        }
        protected SizeF drawTextAt2(float tx, string? txt, Font? font = null)
        {
            return drawTextAt2(tx, txt, null, font);
        }

        protected SizeF drawTextAt2(string? txt, Color? col = null, Font? font = null)
        {
            return drawTextAt2(this.dtx, txt, col, font);
        }

        protected SizeF drawTextAt2(float tx, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            this.dtx = tx;

            const TextFormatFlags flags = Util.textFlags;

            col = col ?? GameColors.Orange;
            font = font ?? this.Font;
            this.lastTextSize = TextRenderer.MeasureText(txt, font, Size.Empty, flags);

            var pt = new Point((int)this.dtx, (int)this.dty);
            if (rightAlign)
            {
                pt.X = (int)(dtx - this.lastTextSize.Width);
                TextRenderer.DrawText(g, txt, font, pt, col.Value, flags);
            }
            else
            {
                TextRenderer.DrawText(g, txt, font, pt, col.Value, flags);
                this.dtx += this.lastTextSize.Width;
            }

            return this.lastTextSize;
        }

        protected SizeF drawTextAt2b(float tx, string? txt, Font? font = null, bool rightAlign = false)
        {
            return drawTextAt2b(tx, this.Width, txt, null, font, rightAlign);
        }

        protected SizeF drawTextAt2b(float tx, int w, string? txt, Font? font = null, bool rightAlign = false)
        {
            return drawTextAt2b(tx, w, txt, null, font, rightAlign);
        }

        protected SizeF drawTextAt2b(float tx, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            return drawTextAt2b(tx, this.Width, txt, col, font, rightAlign);
        }

        protected SizeF drawTextAt2b(float tx, int w, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            this.dtx = tx;

            const TextFormatFlags flags = Util.textFlags | TextFormatFlags.WordBreak | TextFormatFlags.NoFullWidthCharacterBreak;

            col = col ?? GameColors.Orange;
            font = font ?? this.Font;

            var sz = new Size(w - (int)tx, 0);
            this.lastTextSize = TextRenderer.MeasureText(txt, font, sz, flags);

            var rect = new Rectangle(
                (int)this.dtx, (int)this.dty,
                sz.Width, 2 + (int)this.lastTextSize.Height);

            if (rightAlign)
            {
                rect.X = (int)(dtx - this.lastTextSize.Width);
                TextRenderer.DrawText(g, txt, font, rect, col.Value, flags);
            }
            else
            {
                TextRenderer.DrawText(g, txt, font, rect, col.Value, flags);
                //this.dtx += this.lastTextSize.Width;
            }

            return this.lastTextSize;
        }

        protected SizeF lastTextSize;
        protected SizeF formSize;

        protected void newLine()
        {
            newLine(0, false);
        }

        protected void newLine(bool grow)
        {
            newLine(0, grow);
        }

        protected void newLine(float dy, bool grow = false)
        {
            this.dty += this.lastTextSize.Height + dy;

            if (grow)
                this.formGrow(true, true);
        }

        protected void newLine(float dy, bool growHoriz, bool growVert)
        {
            this.dty += this.lastTextSize.Height + dy;

            this.formGrow(growHoriz, growVert);
        }

        protected void resetPlotter(Graphics g)
        {
            this.g = g;
            this.g.SmoothingMode = SmoothingMode.HighQuality;
            this.formSize = new SizeF(2, 0);
            this.dtx = eight;
            this.dty = ten;
        }

        protected void formGrow(bool horiz = true, bool vert = false)
        {
            // grow width?
            if (horiz && this.dtx > this.formSize.Width)
                this.formSize.Width = this.dtx;

            // grow height?
            if (vert && this.dty > this.formSize.Height)
                this.formSize.Height = this.dty;
        }

        protected void formAdjustSize(float dx = 0, float dy = 0)
        {
            this.formSize.Width = (int)Math.Ceiling(this.formSize.Width + dx);
            this.formSize.Height = (int)Math.Ceiling(this.formSize.Height + dy);

            if (this.Size != this.formSize.ToSize() && !forceRepaint)
            {
                if (this.formSize.Width < this.Size.Width || this.formSize.Height < this.Size.Height)
                    this.hideMyClone();

                //Game.log($"formAdjustSize: {this.Name} - {this.Size} => {this.formSize.ToSize()}");
                this.Size = this.formSize.ToSize();
                this.BackgroundImage = GameGraphics.getBackgroundImage(this);
                this.reposition(Elite.getWindowRect());
                forceRepaint = true;
            }
        }

        #endregion

        private static Dictionary<string, POIType> itemPoiTypeMap = new Dictionary<string, POIType>()
        {
            { ObeliskItem.casket.ToString(), POIType.casket },
            { ObeliskItem.orb.ToString(), POIType.orb },
            { ObeliskItem.relic.ToString(), POIType.relic },
            { ObeliskItem.tablet.ToString(), POIType.tablet},
            { ObeliskItem.totem.ToString(), POIType.totem },
            { ObeliskItem.urn.ToString(), POIType.urn },
        };

        private static PointF[] ramTahRelicPoints = {
            new PointF(-8, -4),
            new PointF(+8, -4),
            new PointF(0, +10),
            new PointF(-8, -4),
        };

        protected void drawRamTahDot(float dx, float dy, string item)
        {
            if (item == POIType.relic.ToString())
            {
                dx += this.dtx + four;
                dy += this.dty + four;
                this.dtx += oneTwo;

                g.TranslateTransform(dx, dy);
                g.FillPolygon(GameColors.brushPoiPresent, ramTahRelicPoints);
                g.DrawPolygon(GameColors.penPoiRelicPresent, ramTahRelicPoints);

                g.TranslateTransform(-dx, -dy);
            }
            else if (itemPoiTypeMap.ContainsKey(item))
            {
                var r = new RectangleF(this.dtx + dx, this.dty + dy, ten, ten);
                var poiType = itemPoiTypeMap[item];
                this.dtx += oneTwo;

                g.FillEllipse(GameColors.Map.brushes[poiType][SitePoiStatus.present], r);
                g.DrawEllipse(GameColors.Map.pens[poiType][SitePoiStatus.present], r);
            }

            // TODO: Thargoid items?
        }

        public static void drawBioRing(Graphics g, string? genus, float x, float y, long reward = -1, bool highlight = false, float sz = 18, long maxReward = -1)
        {
            var rr = 22;
            if (sz == 38) rr = 42;

            // TODO: handle scaling
            g.FillEllipse(Brushes.Black, x - 1, y, sz + 2, sz + 2);
            g.DrawEllipse(sz == 18 ? GameColors.penGameOrangeDim1 : GameColors.penGameOrangeDim2, x - 1.5f, y - 0.5f, rr, rr);

            if (genus == null)
            {
                //g.DrawEllipse(GameColors.penUnknownBioSignal, x, y + 2, sz, sz);
                if (sz == 38)
                    g.DrawString("?", GameColors.font1, GameColors.brushUnknownBioSignal, x + 10.5f, y + 8);
                else
                    g.DrawString("?", GameColors.fontSmall, GameColors.brushUnknownBioSignal, x + 5, y + 6);
                return;
            }


            var img = Util.getBioImage(genus, sz == 38);
            g.DrawImage(img, x, y + 2, sz, sz);

            if (reward < 0) return;

            var maxLevel = -1;
            if (maxReward > Game.settings.bioRingBucketThree * 1_000_000) maxLevel = 3;
            else if (maxReward > Game.settings.bioRingBucketTwo * 1_000_000) maxLevel = 2;
            else if (maxReward > Game.settings.bioRingBucketOne * 1_000_000) maxLevel = 1;
            else if (maxReward > 0) maxLevel = 0;

            var level = -1;
            if (reward > Game.settings.bioRingBucketThree * 1_000_000) level = 3;
            else if (reward > Game.settings.bioRingBucketTwo * 1_000_000) level = 2;
            else if (reward > Game.settings.bioRingBucketOne * 1_000_000) level = 1;
            else if (reward > 0) level = 0;

            // outer rings - max
            var op0 = highlight ? GameColors.penCyan2Dotted : GameColors.penGameOrange2Dotted;
            if (maxLevel == 3)
                g.DrawArc(op0, x - 1.5f, y - 0.5f, rr, rr, -90, 360);
            else if (maxLevel == 2)
                g.DrawArc(op0, x - 1.5f, y - 0.5f, rr, rr, -90, 240);
            else if (maxLevel == 1)
                g.DrawArc(op0, x - 1.5f, y - 0.5f, rr, rr, -90, 120);
            else if (maxLevel == 0)
                g.DrawArc(op0, x - 1.5f, y - 0.5f, rr, rr, -100, 30);

            // outer rings - min
            var op = highlight ? GameColors.penCyan2 : GameColors.penGameOrange2;
            if (level == 3)
                g.DrawArc(op, x - 1.5f, y - 0.5f, rr, rr, -90, 360);
            else if (level == 2)
                g.DrawArc(op, x - 1.5f, y - 0.5f, rr, rr, -90, 275);
            else if (level == 1)
                g.DrawArc(op, x - 1.5f, y - 0.5f, rr, rr, -90, 180);
            else if (level == 0)
                g.DrawArc(op, x - 1.5f, y - 0.5f, rr, rr, -90, 90);


            var sz2 = sz / 2.5f;
            var sf = 1f / 18f * sz;
            x += 4.2f * sf;
            y += scaled(5f) * sf;

            //var sf = 1f / 18f * sz;
            //x += 3.5f * sf;
            //y += scaled(4.5f) * sf;

            if (sz == 18) y += 1;

            //var b0 = new SolidBrush(Color.FromArgb(255, 45, 18, 3));
            //var sz3 = sz / 1.8f;
            //g.FillEllipse(b0, x + sf, y + sf, sz3, sz3);

            /*
            x += 1 * sf;
            y += 1 * sf;

            // inner ring - max
            var dotBrush0 = maxDotBrush ?? GameColors.brushGameOrangeDim;
            if (maxLevel == 3)
                g.FillPie(dotBrush0, x, y, sz2, sz2, -90, 360);
            else if (maxLevel == 2)
                g.FillPie(dotBrush0, x, y, sz2, sz2, -90, 240);
            else if (maxLevel == 1)
                g.FillPie(dotBrush0, x, y, sz2, sz2, -90, 120);
            else if (maxLevel == 0)
                g.FillPie(dotBrush0, x, y, sz2, sz2, -100, 30);

            // inner ring
            if (level == 3)
                g.FillPie(dotBrush, x, y, sz2, sz2, -90, 360);
            else if (level == 2)
                g.FillPie(dotBrush, x, y, sz2, sz2, -90, 240);
            else if (level == 1)
                g.FillPie(dotBrush, x, y, sz2, sz2, -90, 120);
            else if (level == 0)
                g.FillPie(dotBrush, x, y, sz2, sz2, -100, 30);
            */
        }

        #region Resource Managering

        private readonly ResourceManager rm;

        private static Dictionary<string, ResourceManager> resourceManagers = new();

        public ResourceManager prepResources(string name)
        {
            if (!resourceManagers.ContainsKey(name))
                resourceManagers[name] = new ResourceManager(name, typeof(PlotBase).Assembly);

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

        #region One Window to Rule Them All

        public static WindowOne? windowOne;
        public static Bitmap? backOne;
        private static Brush brushOne = new SolidBrush(Game.settings.bigOverlayMaskColor);
        protected Rectangle lastOne;

        public class WindowOne : Form
        {
            public WindowOne(Bitmap bm) : base()
            {
                this.Text = "SrvSurveyWindowOne";
                this.Name = "SrvSurveyWindowOne";
                this.BackgroundImage = bm;
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.Manual;
                this.ShowIcon = false;
                this.ShowInTaskbar = false;
                this.BackColor = Game.settings.bigOverlayMaskColor;
                this.TransparencyKey = Game.settings.bigOverlayMaskColor;
                this.AllowTransparency = true;
                this.Size = bm.Size;
                this.Opacity = 0;

                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ControlBox = false;
                this.DoubleBuffered = true;
            }
        }

        public static void startWindowOne()
        {
            if (!Game.settings.streamOneOverlay || windowOne != null) return;

            var sz = Elite.getWindowRect().Size;
            if (sz.Width <= 0 || sz.Height <= 0) return;

            backOne = new Bitmap(sz.Width, sz.Height);
            windowOne = new WindowOne(backOne);
            windowOne.Show();
            Game.log("startWindowOne");
        }

        public static void stopWindowOne()
        {
            if (windowOne != null)
            {
                Game.log("stopWindowOne");
                windowOne.Close();
                windowOne = null;
                backOne = null;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.hideMyClone();
        }

        public void hideMyClone()
        {
            if (!Game.settings.streamOneOverlay || windowOne == null || backOne == null) return;

            Debug.WriteLine($"hideMyClone: {this.Name} => {this.lastOne}");
            using (var g2 = Graphics.FromImage(backOne))
            {
                g2.FillRectangle(brushOne, this.lastOne);
            }
            windowOne.Invalidate();
        }

        #endregion
    }

    /// <summary>
    /// A base class for plotters using lat/long co-ordinates
    /// </summary>
    internal abstract class PlotBaseSurface : PlotBase
    {
        // TODO: Move these to here
        //protected TrackingDelta? touchdownLocation;
        //protected TrackingDelta? srvLocation;
        protected LatLong2 cmdr;

        protected decimal radius { get => Game.activeGame?.systemBody?.radius ?? 0; }

        protected void resetMiddle()
        {
            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
        }

        protected void resetMiddleRotated()
        {
            // draw current location pointer (always at center of plot + unscaled)
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(-game.status.Heading);
        }

        protected void drawCommander()
        {
            const float sz = 5f;
            g.DrawEllipse(GameColors.penLime2, -sz, -sz, sz * 2, sz * 2);
            g.DrawLine(GameColors.penLime2, 0, 0, 0, sz * -2);
        }

        /// <summary>
        /// Centered on cmdr
        /// </summary>
        protected virtual void drawCompassLines()
        {
            g.RotateTransform(-game.status.Heading);

            // draw compass rose lines centered on the commander
            g.DrawLine(Pens.DarkRed, -this.Width * 2, 0, +this.Width * 2, 0);
            g.DrawLine(Pens.DarkRed, 0, 0, 0, +this.Height * 2);
            g.DrawLine(Pens.Red, 0, -this.Height * 2, 0, 0);

            g.RotateTransform(+game.status.Heading);
        }
    }

    /// <summary>
    /// A base class for plotters around some site origin
    /// </summary>
    internal abstract class PlotBaseSite : PlotBaseSurface
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

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;
            base.Status_StatusChanged(blink);

            this.distToSiteOrigin = Util.getDistance(siteLocation, Status.here, this.radius);
            this.offsetWithoutHeading = (PointF)Util.getOffset(this.radius, this.siteLocation); // explicitly EXCLUDING site.heading
            this.cmdrOffset = (PointF)Util.getOffset(radius, siteLocation, siteHeading); // explicitly INCLUDING site.heading
            this.cmdrHeading = game.status.Heading - siteHeading;
        }

        //protected PointF getSiteOffset()
        //{
        //    // Still needed? I don't think so...

        //    // get pixel location of site origin relative to overlay window --
        //    g.ResetTransform();
        //    g.TranslateTransform(mid.Width, mid.Height);
        //    g.ScaleTransform(this.scale, this.scale);
        //    g.RotateTransform(-game.status.Heading);

        //    PointF[] pts = { new PointF(cmdrOffset.X, cmdrOffset.Y) };
        //    g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pts);
        //    var siteOffset = pts[0];
        //    g.ResetTransform();
        //    return siteOffset;
        //}

        protected void resetMiddleSiteOrigin()
        {
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height); // shift to center of window
            g.ScaleTransform(scale, scale); // apply display scale factor (zoom)
            g.RotateTransform(-game.status.Heading); // rotate by cmdr heading
            g.TranslateTransform(-offsetWithoutHeading.X, offsetWithoutHeading.Y); // shift relative to cmdr
            g.RotateTransform(this.siteHeading); // rotate by site heading

            // vertical rotation flips depending on north/south hemisphere?
            //if (this.siteOrigin.Lat > 0) g.RotateTransform(180);
            // Tkachenko Command / (+/+) "lat": 27.571095, / "long": 13.16864 / needs rotate 180° AND flip vertical ?!
        }

        /// <summary>
        /// Adjust graphics transform, calls the lambda then reverses the adjustments.
        /// </summary>
        /// <param name="pf"></param>
        /// <param name="rot"></param>
        /// <param name="func"></param>
        protected void adjust(PointF pf, float rot, Action func)
        {
            adjust(pf.X, pf.Y, rot, func);
        }

        protected void adjust(float x, float y, float rot, Action func)
        {
            // Y value only is inverted
            g.TranslateTransform(+x, -y);
            g.RotateTransform(+rot);

            func();

            g.RotateTransform(-rot);
            g.TranslateTransform(-x, +y);
        }

        protected void drawMapImage()
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
        protected override void drawCompassLines()
        {
            adjust(PointF.Empty, -siteHeading, () =>
            {
                // draw compass rose lines centered on the site origin
                g.DrawLine(GameColors.penDarkRed2Ish, -500, 0, +500, 0);
                g.DrawLine(GameColors.penDarkRed2Ish, 0, -500, 0, +500);
                //g.DrawLine(GameColors.penRed2Ish, 0, -500, 0, 0);
            });


            // and a line to represent "north" relative to the site - visualizing the site's rotation
            if (this.siteHeading >= 0)
                g.DrawLine(GameColors.penRed2DashedIshIsh, 0, -500, 0, 0);
        }

        protected void drawShipAndSrvLocation()
        {
            if (g == null || game.systemBody == null) return;

            // draw touchdown marker
            if (game.cmdr.lastTouchdownLocation != null && game.cmdr.lastTouchdownHeading != -1)
            {
                const float shipSize = 24f;
                var shipLatLong = game.cmdr.lastTouchdownLocation!;
                var ship = Util.getOffset(game.systemBody.radius, shipLatLong, 180);

                // adjust location by ship cockpit offset
                var po = CanonnStation.getShipOffset(game.currentShip.type);
                var pd = po.rotate(game.cmdr.lastTouchdownHeading);
                ship += pd;

                var rect = new RectangleF(
                    (float)ship.X - shipSize,
                    (float)-ship.Y - shipSize,
                    shipSize * 2,
                    shipSize * 2);

                var shipDeparted = game.touchdownLocation == null || game.touchdownLocation == LatLong2.Empty;
                var brush = shipDeparted ? GameColors.brushShipFormerLocation : GameColors.brushShipLocation;

                g.FillEllipse(brush, rect);
            }

            // draw SRV marker
            if (game.srvLocation != null)
            {
                const float srvSize = 10f;

                var srv = Util.getOffset(game.systemBody.radius, game.srvLocation, 180);
                var rect = new RectangleF(
                    (float)srv.X - srvSize,
                    (float)-srv.Y - srvSize,
                    srvSize * 2,
                    srvSize * 2);

                g.FillRectangle(GameColors.brushSrvLocation, rect);
            }
        }
    }

    internal abstract class PlotBaseSelectable : PlotBase, PlotterForm
    {
        protected int selectedIndex = 0;
        private Point[] ptMain;
        private Point[] ptLetter;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool highlightBlink = false;

        protected PlotBaseSelectable() : base()
        {
            this.Width = 500;
            this.Height = 108;

            timer.Tick += Timer_Tick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Tick -= Timer_Tick;
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.initializeOnLoad();
            this.reposition(Elite.getWindowRect(true));

            const int blockWidth = 100;
            const int blockTop = 45;
            const int letterOffset = 10;
            ptMain = new Point[]
            {
                new Point((int)(this.mid.Width - (blockWidth * 1.5f)) , blockTop ),
                new Point((int)(this.mid.Width - (blockWidth * 0.5f)), blockTop ),
                new Point((int)(this.mid.Width + (blockWidth * 0.5f)) , blockTop )
            };
            ptLetter = new Point[]
            {
                new Point((int)(this.mid.Width - (blockWidth * 1.5f) - letterOffset) , blockTop - letterOffset),
                new Point((int)(this.mid.Width - (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset),
                new Point((int)(this.mid.Width + (blockWidth * 0.5f)) - letterOffset, blockTop - letterOffset)
            };
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed) return;

            this.selectedIndex = game.status.FireGroup % 3;

            if (!blink && timer.Enabled == false)
            {
                // if we are within a blink detection window - highlight the footer
                var duration = DateTime.Now - game.status.lastBlinkChange;
                if (duration.TotalMilliseconds < Game.settings.blinkDuration)
                {
                    this.highlightBlink = true;
                    timer.Interval = Game.settings.blinkDuration;
                    timer.Start();
                }
            }

            base.Status_StatusChanged(blink);
        }

        protected void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            this.highlightBlink = false;
            this.Invalidate();
        }

        protected void drawOptions(string msg1, string msg2, string? msg3, int highlightIdx)
        {
            var c = highlightIdx == 0 ? GameColors.Cyan : GameColors.Orange;
            if (highlightIdx == -2) c = Color.Gray;
            TextRenderer.DrawText(g, "A:", GameColors.fontSmall, ptLetter[0], c);
            TextRenderer.DrawText(g, msg1, GameColors.fontMiddle, ptMain[0], c);

            c = highlightIdx == 1 ? GameColors.Cyan : GameColors.Orange;
            if (highlightIdx == -2) c = Color.Gray;
            TextRenderer.DrawText(g, "B:", GameColors.fontSmall, ptLetter[1], c);
            TextRenderer.DrawText(g, msg2, GameColors.fontMiddle, ptMain[1], c);

            if (msg3 != null)
            {
                c = highlightIdx == 2 ? GameColors.Cyan : GameColors.Orange;
                if (highlightIdx == -2) c = Color.Gray;
                TextRenderer.DrawText(g, "C:", GameColors.fontSmall, ptLetter[2], c);
                TextRenderer.DrawText(g, msg3, GameColors.fontMiddle, ptMain[2], c);
            }

            // show selection rectangle
            var rect = new Rectangle(0, 0, 86, 44);
            rect.Location = ptMain[selectedIndex];
            rect.Offset(-12, -12);
            var p = highlightIdx == selectedIndex ? Pens.Cyan : GameColors.penGameOrange1;
            if (highlightIdx == -2 || (msg3 == null && highlightIdx == 2)) p = Pens.Gray;
            g.DrawRectangle(p, rect);

            if (highlightIdx != -2)
                showSelectionCue();
        }

        protected void showSelectionCue()
        {
            // show cue to select
            var triggerTxt = "cockpit mode";
            var b = this.highlightBlink ? GameColors.brushCyan : GameColors.brushGameOrange;
            var times = this.highlightBlink ? "again" : "once";
            drawFooterText($"(toggle {triggerTxt} {times} to set)", b);
        }
    }

    internal class PlotPos
    {
        #region static loading

        private static string customPlotterPositionPath = Path.Combine(Program.dataFolder, "plotters.json");
        private static string defaultPlotterPositionPath = Path.Combine(Application.StartupPath, "plotters.json");
        /// <summary>
        /// The "typical" size of this plotter, used when adjusting positions and the plotter itself isn't visible
        /// </summary>
        public static readonly Dictionary<string, Size> typicalSize = new();

        private static Dictionary<string, PlotPos> plotterPositions = new Dictionary<string, PlotPos>();
        private static Dictionary<string, PlotPos>? backupPositions;

        private static FileSystemWatcher watcher;

        public static void prepPlotterPositions()
        {
            if (!File.Exists(customPlotterPositionPath))
                PlotPos.resetAll();

            // start watching the custom file
            watcher = new FileSystemWatcher(Program.dataFolder, "plotters.json");
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;

            try
            {
                setPlotterPositions(customPlotterPositionPath);
            }
            catch (Exception ex)
            {
                if (ex is IOException) return; // ignore these
                Game.log($"Error first reading overlay positions:\r\n\r\n{ex.Message}");

                // if we fail the first time, retry using the default file
                setPlotterPositions(defaultPlotterPositionPath);
            }

            // start by collecting all non-abstract classes derived from PlotBase
            var allPlotterTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(_ => typeof(PlotBase).IsAssignableFrom(_) && !_.IsAbstract)
                .ToList();

            // These plotters are not movable and are intentionally missing from plotters.json
            // PlotTrackers

            var dirty = false;
            Dictionary<string, PlotPos>? defaultPositions = null;
            typicalSize["PlotPulse"] = BigOverlay.plotPulseDefaultSize;
            foreach (var type in allPlotterTypes)
            {
                var name = type.Name;
                if (!plotterPositions.ContainsKey(name))
                {
                    if (defaultPositions == null) defaultPositions = readPlotterPositions(defaultPlotterPositionPath);
                    // ignore plotters not present in the master file
                    if (defaultPositions.ContainsKey(name))
                    {
                        plotterPositions[name] = defaultPositions[name];
                        dirty = true;
                    }
                }

                // use the size from the attribute
                var approxSize = type.GetCustomAttribute<ApproxSizeAttribute>();
                typicalSize[name] = approxSize?.size ?? new Size(200, 60);

                if (Game.settings.overlayTombs)
                    Program.createTomb(name);
            }

            if (dirty)
                saveCustomPositions();
        }

        public static Size getLastSize(string name)
        {
            return typicalSize.GetValueOrDefault(name, new Size(140, 80));
        }

        public static string[] getAllPlotterNames()
        {
            return plotterPositions.Keys.ToArray();
        }

        public static void resetToDefault(string name)
        {
            if (string.IsNullOrEmpty(name) || !plotterPositions.ContainsKey(name)) return;

            var defaults = readPlotterPositions(defaultPlotterPositionPath);
            var original = defaults.GetValueOrDefault(name);
            if (original == null) return;

            plotterPositions[name] = original;
        }

        public static void resetAll()
        {
            Game.log($"Resetting custom overlay positions");
            // read and write (a basic file copy has issues with BitLocker on Win11?)
            var txt = File.ReadAllText(defaultPlotterPositionPath);
            File.WriteAllText(customPlotterPositionPath, txt);
        }

        public static void backup()
        {
            if (backupPositions == null)
            {
                backupPositions = new();
                foreach (var key in plotterPositions.Keys)
                    backupPositions[key] = new PlotPos(plotterPositions[key].ToString());
            }
        }

        public static void restore()
        {
            if (backupPositions != null)
            {
                plotterPositions = new(backupPositions);
                backupPositions = null;
            }
        }

        public static void saveCustomPositions()
        {
            var json = new StringBuilder();

            json.AppendLine("{");
            json.Append(@"  /*
   * Overlays can be repositioned relative to the edge of the game window: ""<horizontal> , <vertical> , <opacity>""
   *  Horizontal: [ left | center | right | screen ] : < +/- pixels >
   *    Vertical: [ top | middle | bottom | screen ] : < +/- pixels >
   *
   *  Using 'screen' means relative to the top / left corner of your primary monitor.
   *
   * Opacity (optional): < decimal number between 0 and 1 >
   * (when not specified, standard opacity will be used)
   *
   * Edits to this file will take immediate effect.
   */
");

            var lines = plotterPositions.Keys
                .Order()
                .Select(key => $"  \"{key}\": \"{plotterPositions[key]}\"");

            json.Append(string.Join(",\r\n", lines));
            json.AppendLine("\r\n}");

            File.WriteAllText(customPlotterPositionPath, json.ToString());

            backupPositions = null;
        }

        /// <summary>
        /// Returns a dictionary of PlotPos objects by name, parsed from the given .json file
        /// </summary>
        private static Dictionary<string, PlotPos> readPlotterPositions(string filepath)
        {
            Game.log($"Reading PlotterPositions from: {filepath}");

            var json = File.ReadAllText(filepath);
            var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;

            // Remove any plotters that no longer exist
            obj.Remove("PlotRavenSystem");

            return obj.ToDictionary(_ => _.Key, _ => new PlotPos(_.Value));
        }

        /// <summary>
        /// Parses given file and applies it as current plotter positions
        /// </summary>
        private static void setPlotterPositions(string filepath)
        {
            plotterPositions = readPlotterPositions(filepath);

            Program.defer(() => Program.repositionPlotters(Elite.getWindowRect()));
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                setPlotterPositions(e.FullPath);
            }
            catch (Exception ex)
            {
                if (ex is IOException) return; // ignore these
                Game.log($"Error reading overlay positions:\r\n\r\n{ex.Message}");
            }
        }

        public static float getOpacity(PlotterForm form, float defaultValue = -1)
        {
            if (!form.didFirstPaint) return 0;

            return getOpacity(form.GetType().Name, defaultValue);
        }

        public static float getOpacity(string formTypeName, float defaultValue = -1)
        {
            if (Program.tempHideAllPlotters || (!Elite.gameHasFocus && !Debugger.IsAttached)) return 0;

            var pp = plotterPositions.GetValueOrDefault(formTypeName);

            if (pp?.opacity.HasValue == true)
                return pp.opacity.Value;
            else if (defaultValue >= 0)
                return defaultValue;
            else
                return Game.settings.Opacity;
        }

        /// <summary> Returns True is horizontal or vertical are using Screen based values </summary>
        public static bool usesOS(string name)
        {
            var pp = plotterPositions.GetValueOrDefault(name);
            return pp?.h == Horiz.OS || pp?.v == Vert.OS;
        }

        public static void reposition(PlotterForm form, Rectangle rect, string? formTypeName = null)
        {
            formTypeName = formTypeName ?? form.GetType().Name;
            var pt = getPlotterLocation(formTypeName, form.Size, rect);
            if (pt != Point.Empty && form.Location != pt)
                form.Location = pt;
        }

        public static PlotPos? get(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            else
                return plotterPositions.GetValueOrDefault(name);
        }

        /// <summary> Returns where the named plotter should be </summary>
        /// <param name="formName">The name of the plotter</param>
        /// <param name="sz">The size of the plotter</param>
        /// <param name="rect">The rectangle of the game window</param>
        /// <param name="plot2">If this is a refactored plotter, using relative locations</param>
        public static Point getPlotterLocation(string formName, Size sz, Rectangle rect, bool plot2 = false)
        {
            // skip plotters that are fixed
            if (!plotterPositions.ContainsKey(formName))
                return Point.Empty;

            var pp = plotterPositions[formName];

            if (rect == Rectangle.Empty)
                rect = Elite.getWindowRect();

            if (plot2 && !Game.settings.disableBigOverlay)
            {
                if (pp.h != Horiz.OS) rect.X = 0;
                if (pp.v != Vert.OS) rect.Y = 0;
            }

            var left = 0;
            if (pp.h == Horiz.Left)
                left = rect.Left + pp.x;
            else if (pp.h == Horiz.Center)
                left = rect.Left + (rect.Width / 2) - (sz.Width / 2) + pp.x;
            else if (pp.h == Horiz.Right)
                left = rect.Right - sz.Width - pp.x;
            else if (pp.h == Horiz.OS)
                left = pp.x;

            var top = 0;
            if (pp.v == Vert.Top)
                top = rect.Top + pp.y;
            else if (pp.v == Vert.Middle)
                top = rect.Top + (rect.Height / 2) - (sz.Height / 2) + pp.y;
            else if (pp.v == Vert.Bottom)
                top = rect.Bottom - sz.Height - pp.y;
            else if (pp.v == Vert.OS)
                top = pp.y;

            return new Point(left, top);
        }

        #endregion

        public Horiz h;
        public Vert v;
        public int x;
        public int y;
        public float? opacity;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VR? vr;

        public PlotPos(string txt)
        {
            // eg: "left:40,top:50",
            // eg: "left:40,top:50, 0.7",
            // eg: "left:40,top:50, 0.7 { s: 1, r: <1, 2, 3>, p: <4, 5, 6> }",

            var i = txt.IndexOf('{');
            if (i > 0)
            {
                this.vr = VR.parse(txt.Substring(i));
                txt = txt.Substring(0, i);
            }
            var parts = txt.Split(new char[] { ':', ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) throw new Exception($"Bad plotter position: '{txt}'");
            this.h = Enum.Parse<Horiz>(parts[0], true);
            this.x = int.Parse(parts[1]);
            this.v = Enum.Parse<Vert>(parts[2], true);
            this.y = int.Parse(parts[3]);
            if (parts.Length >= 5)
            {
                this.opacity = float.Parse(parts[4]); // match culture
                if (this.opacity < 0 || this.opacity > 1)
                    throw new ArgumentException("Opacity must be a decimal number between 0 and 1");
            }
        }

        public override string ToString()
        {
            this.vr = new() { s = 12, r = new(), p = new() };
            var root = $"{h}:{x}, {v}:{y}".ToLowerInvariant();
            var op = this.opacity > 0 ? $", {opacity}" : "";
            var vr = this.vr != null ? $" {this.vr}" : "";
            return root + op + vr;
        }

        public enum Horiz
        {
            Left,
            Center,
            Right,
            OS,
        };

        public enum Vert
        {
            Top,
            Middle,
            Bottom,
            OS,
        };

        public class VR
        {
            /// <summary> Scale </summary>
            public float s;
            /// <summary> Rotation</summary>
            public Vector3 r;
            /// <summary> Position </summary>
            public Vector3 p;

            public override string ToString()
            {
                return $"{{ s: {s.ToString(CultureInfo.InvariantCulture)}, r: {r}, p: {p}}}";
            }

            public static VR? parse(string txt)
            {
                // "{ s: 12, r: <0, 0, 0>, p: <0, 0, 0>}"
                var parts = txt.Split(new char[] { '{', '}', ',', ':', '<', '>' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 10) return null;

                var vr = new VR()
                {
                    s = float.Parse(parts[1], CultureInfo.InvariantCulture),
                    r = new(float.Parse(parts[3], CultureInfo.InvariantCulture), float.Parse(parts[4], CultureInfo.InvariantCulture), float.Parse(parts[5], CultureInfo.InvariantCulture)),
                    p = new(float.Parse(parts[7], CultureInfo.InvariantCulture), float.Parse(parts[8], CultureInfo.InvariantCulture), float.Parse(parts[9], CultureInfo.InvariantCulture)),
                };
                return vr;
            }
        }
    }

    /// <summary>
    /// An approximate size this plotter would be, used when adjusting plotter locations in settings.
    /// </summary>
    internal class ApproxSizeAttribute : Attribute
    {
        public Size size;
        public ApproxSizeAttribute(int x, int y)
        {
            this.size = new Size(x, y);
        }
    }
}
