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
            var form = new FormMultiFloatie()
            {
                Name = "MultiFloatie",
                Text = "MultiFloatie",
                Opacity = 0.7f,
                BackColor = Color.Lime,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),

                Width = 400,
                Height = SystemInformation.CaptionHeight,

                ShowIcon = false,
                ShowInTaskbar = false,
                MinimizeBox = false,
                MaximizeBox = false,
                ControlBox = false,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };

            form.Show(new Win32Window() { Handle = Elite.getWindowHandle() });
            return form;
        }

        private Label lbl;
        public string cmdr;

        public FormMultiFloatie()
        {
            current = this;
            this.cmdr = Game.activeGame?.Commander ?? Program.forceFid ?? "?";

            this.lbl = new Label()
            {
                Text = cmdr,
                AutoSize = true,
                Padding = new Padding(20, 0, 20, 0),
            };

            this.Controls.Add(lbl);
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

            this.Height = SystemInformation.CaptionHeight;
            this.positionOverGame(Elite.getWindowRect());
            System.Windows.Forms.Cursor.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            current = null;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            //if (Elite.focusElite)
            //    e.Graphics.Clear(C.cyanDark);

            //var cmdrName = CommanderSettings.currentOrLastCmdrName;
            //var x = Util.centerIn(this.Width, Util.stringWidth(this.Font, cmdrName));
            //TextRenderer.DrawText(e.Graphics, cmdrName, this.Font, new Point(x, 0), Color.Black);
        }

        public void setCmdr(string cmdr)
        {
            this.cmdr = cmdr;
            this.lbl.Text = " ~ " + this.cmdr + " ~ ";
        }

        public void positionOverGame(Rectangle rect)
        {
            this.lbl.Text = " ~ " + this.cmdr + " ~ ";

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
