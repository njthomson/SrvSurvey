using SrvSurvey.plotters;

namespace SrvSurvey.widgets
{
    internal class TextCursor : BaseWidget
    {
        public float dtx;
        public float dty;

        public float centerIn = 0;

        public Font font;
        public Color color;

        public TextFormatFlags flags = defaultTextFlags;

        private Graphics g;
        private Control? ctrl;
        private PlotBase2? plotter;

        public SizeF lastTextSize { get; private set; }
        public SizeF frameSize;

        public int padVertical;
        public int padHorizontal;

        public TextCursor(Graphics g, Control ctrl)
        {
            this.g = g;
            this.ctrl = ctrl;

            reset();
        }

        public TextCursor(Graphics g, PlotBase2 plotter)
        {
            this.g = g;
            this.plotter = plotter;

            reset();
        }

        public void reset()
        {
            this.dtx = N.eight;
            this.dty = N.ten;
            this.font = ctrl?.Font ?? plotter!.font;
            this.color = ctrl?.ForeColor ?? plotter!.color;
            this.flags = defaultTextFlags;
            this.frameSize = new SizeF();
        }

        #region draw text (unscaled)

        public SizeF draw(string? txt)
        {
            return draw(this.dtx, txt, null, null);
        }

        public SizeF draw(float tx, string? txt)
        {
            return draw(tx, txt, null, null);
        }

        public SizeF draw(string? txt, Font font)
        {
            return draw(this.dtx, txt, null, font);
        }

        public SizeF draw(float tx, string? txt, Font font)
        {
            return draw(tx, txt, null, font);
        }

        public SizeF draw(string? txt, Color? col)
        {
            return draw(this.dtx, txt, col, null);
        }

        public SizeF draw(string? txt, Color? col, Font? font)
        {
            return draw(this.dtx, txt, col, font);
        }

        public SizeF draw(float tx, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            this.dtx = tx;

            col = col ?? this.color;
            font = font ?? this.font;
            this.lastTextSize = TextRenderer.MeasureText(txt, font, Size.Empty, flags);

            var pt = new Point((int)this.dtx, (int)this.dty);

            if (centerIn > 0)
                pt.Y += Util.centerIn((int)centerIn, (int)lastTextSize.Height);

            if (rightAlign)
            {
                pt.X = (int)(dtx - this.lastTextSize.Width);
                renderText(g, txt, pt, font, col.Value, flags);
            }
            else
            {
                renderText(g, txt, pt, font, col.Value, flags);
                this.dtx += this.lastTextSize.Width;
            }

            return this.lastTextSize;
        }


        public SizeF drawRight(float tx, string txt, Color? col = null, Font? font = null)
        {
            return draw(tx, txt, col, font, true);
        }

        public SizeF drawCentered(string txt, Color? col = null, Font? font = null)
        {
            return drawCentered(this.dty, txt, col, font);
        }

        public SizeF drawCentered(float ty, string txt, Color? col = null, Font? font = null)
        {
            col = col ?? this.color;
            font = font ?? this.font;

            var centerFlags = flags | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

            this.lastTextSize = TextRenderer.MeasureText(txt, font, Size.Empty, centerFlags);

            var rect = new Rectangle(
                padHorizontal,
                this.containerHeight - padVertical - (int)this.lastTextSize.Height,
                this.containerWidth - (padHorizontal * 2),
                (int)this.lastTextSize.Height
            );

            TextRenderer.DrawText(g, txt, font, rect, col.Value, centerFlags);

            return this.lastTextSize;
        }

        /// <summary> Draw bottom/center aligned </summary>
        public SizeF drawFooter(string txt, Color? col = null, Font? font = null)
        {
            col = col ?? this.color;
            font = font ?? this.font;

            var footerFlags = flags | TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;

            this.lastTextSize = TextRenderer.MeasureText(txt, font, Size.Empty, footerFlags);

            var rect = new Rectangle(
                padHorizontal,
                this.containerHeight - padVertical - (int)this.lastTextSize.Height,
                this.containerWidth - (padHorizontal * 2),
                (int)this.lastTextSize.Height
            );

            TextRenderer.DrawText(g, txt, font, rect, col.Value, footerFlags);

            return this.lastTextSize;
        }

        #endregion

        /// <summary> Render strike-through over last rendered text </summary>
        public void strikeThroughLast(bool highlight = false)
        {
            // strike-through if already analyzed
            var x = (int)Math.Ceiling(dtx);
            var w = (int)Math.Ceiling(lastTextSize.Width) + N.one;
            var ly = dty + Util.centerIn(lastTextSize.Height, 2);
            g.DrawLine(highlight ? GameColors.penCyan1 : GameColors.penGameOrange1, x, ly, x - w, ly);
            g.DrawLine(highlight ? GameColors.penDarkCyan1 : GameColors.penGameOrangeDim1, x + 1, ly + 1, x - w + 1, ly + 1);
        }

        #region draw wrapped text (unscaled)

        private int containerWidth => ctrl?.Width ?? plotter!.width;
        private int containerHeight => ctrl?.Height ?? plotter!.height;

        public SizeF drawWrapped(float tx, string? txt, Font font)
        {
            return drawWrapped(tx, this.containerWidth, txt, null, font, false);
        }

        public SizeF drawWrapped(float tx, int w, string? txt, Font? font = null, bool rightAlign = false)
        {
            return drawWrapped(tx, w, txt, null, font, rightAlign);
        }

        public SizeF drawWrapped(float tx, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            return drawWrapped(tx, this.containerWidth, txt, col, font, rightAlign);
        }

        public SizeF drawWrapped(float tx, int w, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            this.dtx = tx;

            var wrappingFlags = this.flags | TextFormatFlags.WordBreak | TextFormatFlags.NoFullWidthCharacterBreak;

            col = col ?? this.color;
            font = font ?? this.font;

            var sz = new Size(w - (int)tx, 0);
            this.lastTextSize = TextRenderer.MeasureText(txt, font, sz, wrappingFlags);

            var rect = new Rectangle(
                (int)this.dtx, (int)this.dty,
                sz.Width, 2 + (int)this.lastTextSize.Height);

            if (rightAlign)
            {
                rect.X = (int)(dtx - this.lastTextSize.Width);
                TextRenderer.DrawText(g, txt, font, rect, col.Value, wrappingFlags);
            }
            else
            {
                TextRenderer.DrawText(g, txt, font, rect, col.Value, wrappingFlags);
                this.dtx += this.lastTextSize.Width;
            }

            return this.lastTextSize;
        }

        #endregion

        #region newlines and frame sizing

        public void newLine()
        {
            newLine(0, false);
        }

        public void newLine(bool grow)
        {
            newLine(0, grow);
        }

        public void newLine(float dy, bool grow = false)
        {
            this.dty += this.lastTextSize.Height + dy;

            if (grow)
                this.formGrow(true, true);
        }

        public void newLine(float dy, bool growHoriz, bool growVert)
        {
            this.dty += this.lastTextSize.Height + dy;

            this.formGrow(growHoriz, growVert);
        }

        public float setMinWidth(float width)
        {
            if (width > this.frameSize.Width)
                this.frameSize.Width = width;

            return this.frameSize.Width;
        }

        public float setMinHeight(float height)
        {
            if (height > this.frameSize.Height)
                this.frameSize.Height = height;

            return this.frameSize.Height;
        }

        private void formGrow(bool horiz = true, bool vert = false)
        {
            // grow width?
            if (horiz) this.setMinWidth(this.dtx);

            // grow height?
            if (vert) this.setMinHeight(this.dty);
        }

        public bool resizeNeeded
        {
            get => this.containerWidth != this.frameSize.Width || this.containerHeight != this.frameSize.Height;
        }

        /// <summary> Returns frameSize after increasing it by the given deltas </summary>
        public SizeF pad(float dx = 0, float dy = 0)
        {
            this.frameSize.Width = (float)Math.Ceiling(this.frameSize.Width + dx);
            this.frameSize.Height = (float)Math.Ceiling(this.frameSize.Height + dy);

            return this.frameSize;
        }

        /* Still needed? Maybe ...
        public bool resizeFrame(float dx = 0, float dy = 0, bool force = false)
        {
            this.frameSize.Width += dx;
            this.frameSize.Height += dy;

            if (this.resizeNeeded || force) // ?? && !forceRepaint)
            {
                if (this.ctrl != null)
                {
                    this.ctrl.Size = new Size(
                        (int)Math.Ceiling(this.frameSize.Width),
                        (int)Math.Ceiling(this.frameSize.Height)
                    );
                    return true;
                }
                else
                {
                    Debugger.Break();
                }
            }

            return false;
        }
        */

        #endregion
    }
}
