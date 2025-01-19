using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SrvSurvey.game;

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
        private Control ctrl;

        private SizeF lastTextSize;
        public SizeF frameSize;

        public TextCursor(Graphics g, Control ctrl)
        {
            this.g = g;
            this.ctrl = ctrl;

            reset();
        }

        public void reset()
        {
            this.dtx = N.eight;
            this.dty = N.ten;
            this.font = ctrl.Font;
            this.color = ctrl.ForeColor;
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

        #endregion 

        #region draw wrapped text (unscaled)

        public SizeF drawWrapped(float tx, string? txt, Font font)
        {
            return drawWrapped(tx, this.ctrl.Width, txt, null, font, false);
        }

        public SizeF drawWrapped(float tx, int w, string? txt, Font? font = null, bool rightAlign = false)
        {
            return drawWrapped(tx, w, txt, null, font, rightAlign);
        }

        public SizeF drawWrapped(float tx, string? txt, Color? col, Font? font = null, bool rightAlign = false)
        {
            return drawWrapped(tx, this.ctrl.Width, txt, col, font, rightAlign);
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

        private void formGrow(bool horiz = true, bool vert = false)
        {
            // grow width?
            if (horiz && this.dtx > this.frameSize.Width)
                this.frameSize.Width = this.dtx;

            // grow height?
            if (vert && this.dty > this.frameSize.Height)
                this.frameSize.Height = this.dty;
        }

        public bool resizeNeeded
        {
            get => this.ctrl.Width != this.frameSize.Width || this.ctrl.Height != this.frameSize.Height;
        }

        public bool resizeFrame(float dx = 0, float dy = 0, bool force = false)
        {
            this.frameSize.Width += dx;
            this.frameSize.Height += dy;

            if (this.resizeNeeded || force) // ?? && !forceRepaint)
            {
                this.ctrl.Size = new Size(
                    (int)Math.Ceiling(this.frameSize.Width),
                    (int)Math.Ceiling(this.frameSize.Height)
                );
                return true;
            }

            return false;
        }

        #endregion
    }
}
