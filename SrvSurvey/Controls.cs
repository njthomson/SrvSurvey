using SrvSurvey.game;
using SrvSurvey.Properties;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey
{
    class FlatButton : Button
    {
        public FlatButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            this.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            //var g = pevent.Graphics;

            //g.Clear(this.BackColor);

            //var sz = TextRenderer.MeasureText(g, this.Text, this.Font, this.ClientSize, TextFormatFlags.Default);
            //var h = (ClientSize.Height / 2f) - (sz.Height / 2f);
            //var pt = new Point(0, (int)h);

            ////var bb = new SolidBrush(Color.FromArgb(255, this.BackColor));
            ////g.FillRectangle(bb, 0, (int)h, sz.Width, sz.Height);

            //////g.SmoothingMode = SmoothingMode.HighQuality;
            //var r1 = new Rectangle(1, (int)h, sz.Width, sz.Height);
            ////ControlPaint.DrawMenuGlyph(g, r1, MenuGlyph.Checkmark, this.ForeColor, this.BackColor);

            //g.DrawRectangle(Pens.Black, 0, (int)h, sz.Width, sz.Height);
            ////ControlPaint.DrawCheckBox()

            //TextRenderer.DrawText(g, this.Text, this.Font, pt, this.ForeColor);

        }
    }

    public class DraggableForm : Form
    {
        const int titleHeight = 24;

        public bool renderOwnTitleBar = true;

        private Button? fakeClose;
        private Button? fakeMinimize;

        public DraggableForm()
        {
            this.Margin = new Padding(0, 40, 0, 0);
        }

        #region mouse dragging

        private bool mouseDown;
        private Size startPoint;


        private void startDragging(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            mouseDown = true;
            startPoint = new Size(e.Location);
        }

        private void stopDragging()
        {
            mouseDown = false;
        }

        private void onMouseDrag(MouseEventArgs e)
        {
            if (mouseDown)
            {
                var delta = e.Location - startPoint;
                this.Location = this.Location + (Size)delta;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            startDragging(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            stopDragging();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            onMouseDrag(e);
        }

        #endregion

        private void adjustChildren(Control ctrl)
        {
            // unless the control has editable text ...
            //if (!(ctrl is TextBox) && ctrl.Cursor != Cursors.IBeam)
            if (ctrl.Cursor != Cursors.IBeam && !(ctrl is Button) && !(ctrl is ComboBox) && !(ctrl is StatusStrip))
            {
                // ... hook into their mouse events
                ctrl.MouseDown += new MouseEventHandler((object? sender, MouseEventArgs e) => startDragging(e));
                ctrl.MouseUp += new MouseEventHandler((object? sender, MouseEventArgs e) => stopDragging());
                ctrl.MouseMove += new MouseEventHandler((object? sender, MouseEventArgs e) => onMouseDrag(e));
                ctrl.MouseLeave += new EventHandler((object? sender, EventArgs e) => stopDragging());
            }

            foreach (Control child in ctrl.Controls)
                adjustChildren(child);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
                renderOwnTitleBar = false;

            if (renderOwnTitleBar)
            {
                //var notDocked = false;
                //foreach (Control ctrl in this.Controls)
                //{
                //    if (ctrl.Dock == DockStyle.None)
                //    {
                //        notDocked = true;
                //        break;
                //    }
                //}

                // shift everything down
                foreach (Control ctrl in this.Controls)
                {
                    if ((ctrl.Anchor & AnchorStyles.Top) == AnchorStyles.Top)
                    {
                        ctrl.Top += titleHeight;

                        if ((ctrl.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                            ctrl.Height -= titleHeight;
                    }

                    adjustChildren(ctrl);
                }

                // Needed for things that use Dock.xxx
                //this.Padding = new Padding(
                //    this.Padding.Left,
                //    this.Padding.Top + titleHeight,
                //    this.Padding.Right,
                //    this.Padding.Bottom
                //);

                // do this after or Bottom anchored controls shift with it
                this.Height += titleHeight;

                // add fake close
                fakeClose = new Button()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Text = "X",
                    AutoSize = false,
                    Width = titleHeight,
                    Height = titleHeight,
                    Top = 0,
                    Left = this.ClientSize.Width - titleHeight,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = SystemColors.ControlDarkDark,
                };
                fakeClose.FlatAppearance.BorderSize = 0;
                fakeClose.FlatAppearance.MouseOverBackColor = Color.Firebrick;
                fakeClose.FlatAppearance.MouseDownBackColor = Color.DarkRed;
                fakeClose.Click += new EventHandler((object? sender, EventArgs args) => this.Close());
                this.Controls.Add(fakeClose);

                // add fake minimize
                if (this.MinimizeBox)
                {
                    fakeMinimize = new Button()
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        Text = "_",
                        AutoSize = false,
                        Width = titleHeight,
                        Height = titleHeight,
                        Top = 0,
                        Left = this.ClientSize.Width - titleHeight * 2,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = SystemColors.ControlDarkDark,
                    };
                    fakeMinimize.FlatAppearance.BorderSize = 0;
                    fakeMinimize.FlatAppearance.MouseOverBackColor = Color.CadetBlue;
                    fakeMinimize.FlatAppearance.MouseDownBackColor = Color.SteelBlue;
                    fakeMinimize.Click += new EventHandler((object? sender, EventArgs args) => this.WindowState = FormWindowState.Minimized);
                    this.Controls.Add(fakeMinimize);
                }

                if (this.FormBorderStyle == FormBorderStyle.Sizable)
                {
                    foreach (Control ctrl in this.Controls)
                    {
                        if (ctrl is StatusStrip)
                        {
                            //doHijack = (StatusStrip)ctrl;
                            //hijack((StatusStrip)ctrl);
                            //doHijack.MouseDown += new MouseEventHandler((object? sender, MouseEventArgs e) =>
                            //{
                            //    startDragging(e);
                            //});
                            //doHijack.MouseUp += new MouseEventHandler((object? sender, MouseEventArgs e) => stopDragging());
                            //doHijack.MouseMove += new MouseEventHandler((object? sender, MouseEventArgs e) => onMouseDrag(e));
                            //doHijack.MouseLeave += new EventHandler((object? sender, EventArgs e) => stopDragging());

                            this.ControlBox = false;
                            //_text = this.Text;
                            this.Text = "";
                            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                            break;
                        }
                    }
                }
                //else
                //{
                this.FormBorderStyle = FormBorderStyle.None;
                //}
            }
        }

        private void hijack(StatusStrip ctrl)
        {
            if (ctrl == null) return;

            bool hijackDown = false;
            Size hijackStart = Size.Empty;


            ctrl.MouseDown += new MouseEventHandler((object? sender, MouseEventArgs e) =>
            {
                if (e.Button != MouseButtons.Left) return;

                hijackDown = true;
                hijackStart = new Size(e.Location);
            });
            ctrl.MouseUp += new MouseEventHandler((object? sender, MouseEventArgs e) =>
            {
                hijackDown = false;
            });
            ctrl.MouseMove += new MouseEventHandler((object? sender, MouseEventArgs e) =>
            {
                if (hijackDown)
                {
                    var delta = e.Location - hijackStart;
                    Game.log($"delta: {delta} / Size: {this.Size}");
                    this.Size = this.Size + (Size)delta;
                    hijackStart = new Size(e.Location);
                }
            });
            ctrl.MouseLeave += new EventHandler((object? sender, EventArgs e) =>
            {
                hijackDown = false;
            });
        }

        //[AllowNull]
        //public override string Text
        //{
        //    get
        //    {
        //        if (this.renderOwnTitleBar && this.FormBorderStyle == FormBorderStyle.Sizable)
        //            return "";

        //        return base.Text;
        //    }
        //    set
        //    {
        //        if (this.renderOwnTitleBar && this.FormBorderStyle != FormBorderStyle.Sizable)
        //            base.Text = value;

        //        this._text = value!;
        //    }
        //}

        //private string _text;
        private static Icon logo2 = new Icon(new MemoryStream(Resources.logo2));


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            if (renderOwnTitleBar)
            {
                g.FillRectangle(SystemBrushes.ControlDarkDark, 0, 0, this.Width, titleHeight);

                // icon top left
                g.DrawIcon(logo2, new Rectangle(0, 0, titleHeight, titleHeight));

                // fake title
                var pt = new Point(28, 2);
                TextRenderer.DrawText(g, this.Text ?? this.Text, GameColors.fontMiddleBold, pt, SystemColors.ControlLight);
            }
        }
    }

    //class GroupBox2 : GroupBox
    //{
    //    [Browsable(true)]
    //    [EditorBrowsable(EditorBrowsableState.Always)]
    //    public Color BorderColor { get; set; }

    //    protected override void OnPaint(PaintEventArgs e)
    //    {
    //        //base.OnPaint(e);
    //        var g = e.Graphics;

    //        var pt = new Point(8, 0);

    //        var r = this.ClientRectangle;
    //        r.Y += pt.X;
    //        r.Height -= 9;


    //        var sz = TextRenderer.MeasureText(this.Text, this.Font);
    //        var r2 = new Rectangle(pt, sz);
    //        g.SetClip(r2, System.Drawing.Drawing2D.CombineMode.Exclude);
    //        ControlPaint.DrawBorder(g, r, this.BorderColor, ButtonBorderStyle.Solid);

    //        g.ResetClip();
    //        TextRenderer.DrawText(g, this.Text, this.Font, pt, this.ForeColor);
    //    }
    //}

    //class TextPanel : Panel
    //{
    //    private TextBox tb;

    //    [Browsable(true)]
    //    [EditorBrowsable(EditorBrowsableState.Always)]
    //    public override string Text
    //    {
    //        get => this.tb.Text;
    //        set
    //        {
    //            base.Text = value;
    //            tb.Text = value;
    //        }
    //    }

    //    public override Color BackColor { 
    //        get => base.BackColor;
    //        set
    //        {
    //            base.BackColor = value;
    //            tb.BackColor = value;
    //        }
    //    }

    //    public override Color ForeColor
    //    {
    //        get => base.ForeColor;
    //        set
    //        {
    //            base.ForeColor = value;
    //            tb.ForeColor = value;
    //        }
    //    }

    //    protected override Size DefaultSize
    //    {
    //        get
    //        {
    //            if (tb == null) return base.DefaultSize;

    //            var sz = tb.PreferredSize;
    //            sz.Width += 2;
    //            sz.Height += 2;
    //            return sz;
    //        }
    //    }

    //    public TextPanel()
    //    {
    //        this.Padding = new Padding(1);

    //        this.tb = new TextBox()
    //        {
    //            BackColor = this.BackColor,
    //            BorderStyle = BorderStyle.None,
    //            Dock = DockStyle.Fill,
    //            ReadOnly = true,
    //        };

    //        this.Controls.Add(this.tb);
    //    }

    //    public override Size GetPreferredSize(Size proposedSize)
    //    {
    //        var sz = tb.GetPreferredSize(proposedSize);
    //        sz.Width += 2;
    //        sz.Height += 2;
    //        return sz;
    //    }

    //    [Browsable(true)]
    //    [EditorBrowsable(EditorBrowsableState.Always)]
    //    public Color BorderColor { get; set; }

    //    private Pen pen = new Pen(Color.Black);

    //    //protected override void OnPaintBackground(PaintEventArgs pevent)
    //    //{
    //    //    base.OnPaintBackground(pevent);

    //    //    var g = pevent.Graphics;
    //    //    this.pen.Color = this.BorderColor;
    //    //    g.DrawRectangle(this.pen, this.ClientRectangle);
    //    //}

    //    protected override void OnPaint(PaintEventArgs pevent)
    //    {
    //        base.OnPaint(pevent);

    //        //= SystemColors.WindowFrame;

    //        var g = pevent.Graphics;
    //        this.pen.Color = this.BorderColor;
    //        //g.Clear(this.BackColor);
    //        //TextBoxRenderer.DrawTextBox(g, this.ClientRectangle, this.Text, this.Font, TextBoxState.Normal);
    //        g.DrawRectangle(this.pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

    //        //var pt = new Point(3, 3);
    //        //TextRenderer.DrawText(g, this.Text, this.Font, pt, this.ForeColor);
    //    }
    //}

    //class CheckBox2 : CheckBox
    //{
    //    protected override void OnPaint(PaintEventArgs pevent)
    //    {
    //        base.OnPaint(pevent);
    //        var g = pevent.Graphics;

    //        //g.Clear(this.BackColor);
    //        var state = this.Checked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
    //        //state = CheckBoxState.MixedPressed;

    //        var sz = CheckBoxRenderer.GetGlyphSize(g, state);
    //        var h = (ClientSize.Height / 2f) - (sz.Height / 2f);
    //        var pt = new Point(0, (int)h);

    //        CheckBoxRenderer.DrawCheckBox(g, pt, state);


    //        //var bb = new SolidBrush(Color.FromArgb(255, this.BackColor));
    //        //g.FillRectangle(bb, 0, (int)h, sz.Width, sz.Height);

    //        ////g.SmoothingMode = SmoothingMode.HighQuality;
    //        //var r1 = new Rectangle(1, (int)h, sz.Width, sz.Height);
    //        //ControlPaint.DrawMenuGlyph(g, r1, MenuGlyph.Checkmark, this.ForeColor, this.BackColor);

    //        //g.DrawRectangle(Pens.Black, 0, (int)h, sz.Width, sz.Height);
    //        //ControlPaint.DrawCheckBox()

    //        var aa = Assembly.LoadWithPartialName("System.Windows.Forms");
    //        var tt = aa.DefinedTypes.FirstOrDefault(t => t.Name == "ControlPaint");
    //        //var tt = Type.GetType("System.Int32");
    //    }
    //}


}
