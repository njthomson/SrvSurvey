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
