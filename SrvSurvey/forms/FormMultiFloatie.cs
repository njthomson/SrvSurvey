using SrvSurvey.game;
using System.ComponentModel;

namespace SrvSurvey.forms
{
    [System.ComponentModel.DesignerCategory("")]
    class FormMultiFloatie : Form
    {
        public static FormMultiFloatie? current;

        public static FormMultiFloatie create()
        {
            current?.Close();

            Game.log("FormMultiFloatie.create");
            var form = new FormMultiFloatie();

            if (Game.settings.disableWindowParentIsGame)
            {
                form.TopMost = true;
                form.Show();
            }
            else
                form.Show(new Win32Window() { Handle = Elite.getWindowHandle() });
            return form;
        }

        public string cmdr { get; private set; }

        public FormMultiFloatie()
        {
            current = this;
            Name = "MultiFloatie";
            Opacity = 0.7f;
            BackColor = Color.Lime;
            Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);

            Height = SystemInformation.CaptionHeight;

            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(20, 0, 20, 0);

            setCmdr(Game.activeGame?.Commander ?? Program.forceFid ?? "?");
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 // WS_EX_TRANSPARENT
                    + 0x00080000 // WS_EX_LAYERED
                    + 0x08000000 // WS_EX_NOACTIVATE
                    + 0x00000004 // WS_EX_NOPARENTNOTIFY
                    ;
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Height = SystemInformation.CaptionHeight;
            this.positionOverGame(Elite.getWindowRect());
            System.Windows.Forms.Cursor.Show();

            Program.defer(() => this.Activate());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            current = null;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var sz = TextRenderer.MeasureText(this.Text, this.Font);
            return new Size(
                Math.Max(100, sz.Width + this.Padding.Horizontal),
                SystemInformation.CaptionHeight + this.Padding.Vertical + SystemInformation.VerticalResizeBorderThickness
            );
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (Elite.focusElite)
                e.Graphics.Clear(Color.Lime);

            var r = new Rectangle(0, 2, this.Width, this.Height);
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, r, Color.Black, Color.Lime, TextFormatFlags.HorizontalCenter);
        }

        public void setCmdr(string cmdr)
        {
            this.cmdr = cmdr;
            this.Text = " ~ " + this.cmdr + " ~ ";
            this.Size = this.GetPreferredSize(Size.Empty);
        }

        public void positionOverGame(Rectangle rect)
        {
            this.Left = rect.Left + Util.centerIn(rect.Width, this.Width);

            // Windows => above in the title bar / Borderless => match the top
            if (Elite.graphicsMode == GraphicsMode.Windowed)
                this.Top = rect.Top - this.Height - 2;
            else
                this.Top = rect.Top;
        }

        public static void focusNextWindow()
        {
            if (!Elite.hadManyGameProcs) return;

            var edProcs = Elite.GetGameProcs();
            var nextIdx = Elite.procIdx + 1;
            if (nextIdx >= edProcs.Length) nextIdx = 0;
            var nextGameProc = edProcs[nextIdx];

            Game.log($"focusNextWindow: #{nextGameProc.MainWindowHandle} (vs #{edProcs[Elite.procIdx].MainWindowHandle})");

            Elite.setFocusED(nextGameProc.MainWindowHandle, true);
        }

    }
}
