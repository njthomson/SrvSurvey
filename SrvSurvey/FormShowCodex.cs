using SrvSurvey.canonn;
using SrvSurvey.game;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace SrvSurvey
{
    public partial class FormShowCodex : Form
    {
        public static FormShowCodex? activeForm;

        public static FormShowCodex show(string entryId)
        {
            if (activeForm == null)
                FormShowCodex.activeForm = new FormShowCodex();

            FormShowCodex.activeForm.entryId = entryId;

            Util.showForm(FormShowCodex.activeForm);
            FormShowCodex.activeForm.showEntryId(entryId);
            return FormShowCodex.activeForm;
        }

        private string entryId;
        private BioMatch match;
        private Image? img;
        private float scale = 0.25f;
        private float mx, my, w, h;
        public PointF dragOffset;
        private bool dragging = false;
        private Point mouseDownPoint;


        private FormShowCodex()
        {
            this.ForeColor = GameColors.Orange;
            InitializeComponent();
            this.DoubleBuffered = true;
            this.toolImageCredit.ForeColor = SystemColors.ControlText;

            this.MouseWheel += FormShowCodex_MouseWheel;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formShowCodex);
        }

        private void FormShowCodex_Load(object sender, EventArgs e)
        {
            this.showEntryId(this.entryId);

            mx = (this.ClientRectangle.Width / 2f) - (w / 2f);
            my = (this.ClientRectangle.Height / 2f) - (h / 2);
        }

        private void FormShowCodex_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                this.scale *= 1.1f;
            else
                this.scale *= 0.9f;

            if (this.scale < 0.1) this.scale = 0.1f;
            if (this.scale > 10) this.scale = 10f;

            w = img.Width * scale;
            h = img.Height * scale;

            mx = (this.ClientRectangle.Width / 2f) - (w / 2f);
            my = (this.ClientRectangle.Height / 2f) - (h / 2);

            this.Invalidate();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formShowCodex != rect)
            {
                Game.settings.formShowCodex = rect;
                Game.settings.Save();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            mx = (this.ClientRectangle.Width / 2f) - (w / 2f);
            my = (this.ClientRectangle.Height / 2f) - (h / 2);

            this.Invalidate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormShowCodex.activeForm = null;
        }

        public async void showEntryId(string entryId)
        {
            this.panelSubmit.Hide();
            this.img = null;
            this.entryId = entryId;
            this.match = Game.codexRef.matchFromEntryId(entryId);
            this.lblTitle.Text = match.variant.englishName;

            // attempt to download if we don't have a file already
            var filepath = Path.Combine(CodexRef.codexImagesFolder, $"{match.entryId}.jpg");
            if (!File.Exists(filepath))
            {
                if (match.variant.imageUrl == null)
                {
                    // we have no url
                    this.panelSubmit.Show();
                    this.toolImageCredit.Text = "";
                    return;
                }

                Game.log($"Downloading: {match.variant.imageUrl}");
                using (var stream = await new HttpClient().GetStreamAsync(match.variant.imageUrl))
                {
                    using (var imgTmp = Image.FromStream(stream))
                    {
                        this.img = new Bitmap(imgTmp);
                        imgTmp.Save(filepath, ImageFormat.Png);
                    }
                }
            }
            else
            {
                // load the cached image - quickly, so as not to lock the file
                using (var imgTmp = Bitmap.FromFile(filepath))
                    this.img = new Bitmap(imgTmp);
            }

            w = img.Width * scale;
            h = img.Height * scale;

            mx = (this.ClientRectangle.Width / 2f) - (w / 2f);
            my = (this.ClientRectangle.Height / 2f) - (h / 2);

            this.toolImageCredit.Text = $"cmdr: {match.variant.imageCmdr}";
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.Clear(BackColor);
            if (this.img == null) return;

            //w = img.Width * scale;
            //h = img.Height * scale;

            //mx = (this.ClientRectangle.Width / 2f) - (w / 2f);
            //my = (this.ClientRectangle.Height / 2f) - (h / 2);

            //mx -= dragOffset.X;
            //my -= dragOffset.Y;

            var x = mx - dragOffset.X * this.scale;
            var y = my - dragOffset.Y * this.scale;

            e.Graphics.DrawImage(this.img, x, y, w, h);
        }

        private void linkSubmitImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = $"https://docs.google.com/forms/d/e/1FAIpQLSdtS-78k6MDb_L2RodLnVGoB3r2958SA5ARnufAEZxLeoRbhA/viewform?entry.987977054={Uri.EscapeDataString(Game.settings.lastCommander)}&entry.1282362439={Uri.EscapeDataString(match.variant.englishName)}&entry.468337930={Uri.EscapeDataString(match.entryId.ToString())}";
            Util.openLink(url);
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            var url = $"https://canonn-science.github.io/Codex-Regions/?entryid={Uri.EscapeDataString(match.entryId.ToString())}&hud_category=Biology";
            Util.openLink(url);
        }

        private void FormShowCodex_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPoint = e.Location;
            dragging = true;
            this.Cursor = Cursors.SizeAll;
        }

        private void FormShowCodex_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                this.dragOffset.X += (mouseDownPoint.X - e.Location.X) / this.scale;
                this.dragOffset.Y += (mouseDownPoint.Y - e.Location.Y) / this.scale;
                mouseDownPoint = e.Location;

                this.Invalidate();
            }
        }

        private void FormShowCodex_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            this.Cursor = Cursors.Hand;
        }
    }
}
