using SrvSurvey.widgets;
using System.ComponentModel;
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

        [DefaultValue(FlatStyle.Flat)]
        [Localizable(true)]
        public new FlatStyle FlatStyle
        {
            get => base.FlatStyle;
            set => base.FlatStyle = value;
        }

        public static void applyGameTheme(params FlatButton[] buttons)
        {
            foreach (var btn in buttons)
            {
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 64, 64, 64);
                btn.FlatAppearance.MouseDownBackColor = GameColors.OrangeDim;
            }
        }
    }

    public class TreeView2 : TreeView
    {
        public bool doNotPaint = false;

        public TreeView2()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //Debug.WriteLine($"OnPaintBackground: {Name} / {doNotPaint}");
            if (doNotPaint) return;

            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Debug.WriteLine($"OnPaint: {Name} / {doNotPaint}");
            if (doNotPaint) return;

            base.OnPaint(e);
        }
    }

    public class TextBox2 : Panel
    {
        private TextBox tb;
        private Button? bc;

        public TextBox2()
        {
            tb = new TextBox()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
                Font = this.Font,
                ForeColor = this.ForeColor,
                Width = this.ClientSize.Width,
            };
            this.Controls.Add(tb);

            // apply some defaults
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
        }

        #region TextBox overrides

        [AllowNull]
        [Localizable(true)]
        [Browsable(true)]
        public override string Text
        {
            get => tb.Text;
            set => tb.Text = value;
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set { base.BackColor = value; tb.BackColor = value; }
        }

        public override Color ForeColor
        {
            get => base.ForeColor;
            set { base.ForeColor = value; tb.ForeColor = value; }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            tb.Enabled = this.Enabled;
        }

        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set { base.Font = value; tb.Font = value; }
        }

        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public bool ReadOnly
        {
            get => tb.ReadOnly;
            set => tb.ReadOnly = value;
        }

        [Localizable(true)]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get => tb.TextAlign;
            set => tb.TextAlign = value;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            tb.Top = Util.centerIn(this.ClientSize.Height, tb.Height);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return tb.GetPreferredSize(proposedSize);
        }

        #endregion

        #region Clear text code

        [Browsable(true)]
        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public bool UseClearButton
        {
            get => this.bc != null;
            set
            {
                // remove if needed
                if (value == false && this.bc != null)
                {
                    this.Controls.Remove(this.bc);
                    this.bc = null;

                    tb.Width = this.ClientSize.Width;

                }
                else if (value == true && this.bc == null)
                {
                    createButtonClear();
                }

                //   tb.Visible = bc == null;

                tb.BackColor = Color.Cyan;
            }
        }

        private void createButtonClear()
        {
            this.bc = new FlatButton()
            {
                Anchor = AnchorStyles.Right,
                AutoSize = false,
            };

            bc.Size = new Size(16, 16);
            bc.Left = this.ClientSize.Width - bc.Width - 1;
            bc.Top = Util.centerIn(this.Height, bc.Height) - 1;
            bc.Paint += B_Paint;
            bc.Click += Bc_Click;

            tb.Width = bc.Left;

            this.Controls.Add(bc);
            this.Controls.SetChildIndex(bc, 0);
        }

        private void Bc_Click(object? sender, EventArgs e)
        {
            tb.Text = "";
            tb.Focus();
        }

        private void B_Paint(object? sender, PaintEventArgs e)
        {
            if (bc == null || this.IsDisposed) return;
            const int w = 3;
            e.Graphics.DrawLine(SystemPens.WindowText, w, w, bc.Width - 1 - w, bc.Height - 1 - w);
            e.Graphics.DrawLine(SystemPens.WindowText, w, bc.Width - 1 - w, bc.Height - 1 - w, w);
        }

        #endregion
    }

}
