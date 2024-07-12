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

            foreach (ToolStripItem item in this.statusStrip.Items)
                item.ForeColor = SystemColors.ControlText;

            // use our last location
            Util.useLastLocation(this, Game.settings.formShowCodex);
        }

        private void FormShowCodex_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(() =>
            {
                this.showEntryId(this.entryId);
            });
        }

        private void calcSizes(bool middle, bool imgSize)
        {
            if (imgSize && this.img != null)
            {
                w = img.Width * scale;
                h = img.Height * scale;
            }

            if (middle)
            {
                this.mx = (this.ClientRectangle.Width / 2f) - (w / 2f);
                this.my = (this.ClientRectangle.Height / 2f) - (h / 2f);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0)
                this.scale *= 1.1f;
            else
                this.scale *= 0.9f;

            if (this.scale < 0.1) this.scale = 0.1f;
            if (this.scale > 10) this.scale = 10f;

            this.calcSizes(true, true);
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

            this.calcSizes(true, false);
            this.Invalidate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormShowCodex.activeForm = null;
        }

        public async void showEntryId(string entryId)
        {
            Game.log($"showEntryId: {entryId}");
            // also find any other things we can show images for
            this.prepAllSpecies();
            if (entryId == this.entryId) return;

            this.lblLoading.Show();
            this.panelSubmit.Hide();
            //this.panelSubmit.Show();
            this.img = null;
            this.entryId = entryId;
            this.match = Game.codexRef.matchFromEntryId(entryId);
            this.lblTitle.Text = match.variant.englishName;
            this.lblCmdr.Text = $"cmdr: {match.variant.imageCmdr}";

            var filepath = Path.Combine(CodexRef.codexImagesFolder, $"{match.entryId}.png");

            // do we have a local image?
            if (!string.IsNullOrEmpty(Game.settings.localFloraFolder))
            {
                var localFilepath = Path.Combine(Game.settings.localFloraFolder, $"{match.variant.localImgName}.png");
                if (File.Exists(localFilepath))
                {
                    // load the cached image - quickly, so as not to lock the file
                    using (var imgTmp = Bitmap.FromFile(localFilepath))
                        this.img = new Bitmap(imgTmp);

                    this.lblCmdr.Text = "(local image)";
                }
            }

            if (this.img == null && File.Exists(filepath))
            {
                // download images once a week
                var duration = DateTime.Now.Subtract(File.GetLastWriteTime(filepath));
                if (duration.TotalDays < 7)
                {
                    // load the cached image - quickly, so as not to lock the file
                    using (var imgTmp = Bitmap.FromFile(filepath))
                        this.img = new Bitmap(imgTmp);
                }
                else
                {
                    // too old, delete and we'll download again below
                    File.Delete(filepath);
                }
            }

            // download if we don't have a file already
            if (this.img == null || !File.Exists(filepath))
            {
                if (match.variant.imageUrl == null)
                {
                    // we have no url
                    this.panelSubmit.Show();
                    this.lblCmdr.Text = "";
                    this.Invalidate();
                    this.lblLoading.Hide();
                    return;
                }

                Game.log($"Downloading: {match.variant.imageUrl}");
                this.Invalidate();
                Application.DoEvents();
                using (var stream = await new HttpClient().GetStreamAsync(match.variant.imageUrl))
                {
                    using (var imgTmp = Image.FromStream(stream))
                    {
                        this.img = new Bitmap(imgTmp);
                        imgTmp.Save(filepath, ImageFormat.Png);
                    }
                }
            }

            // calc scale to make the width fit
            this.scale = (float)this.ClientRectangle.Width / (float)img.Width;
            this.calcSizes(true, true);
            this.Invalidate(true);
            this.lblLoading.Hide();
        }

        private void prepAllSpecies()
        {
            if (Game.activeGame?.systemBody?.organisms == null) return;
            toolChange.DropDownItems.Clear();

            if (Game.activeGame.systemBody.organisms.Any(o => o.variantLocalized != null))
            {
                // show confirmed variants
                toolChange.DropDownItems.Add(new ToolStripMenuItem("Confirmed:") { Enabled = false });
                foreach (var org in Game.activeGame.systemBody.organisms)
                {
                    if (string.IsNullOrEmpty(org.variantLocalized)) continue;

                    var entryId = org.entryId.ToString();
                    var item = new ToolStripMenuItem(org.variantLocalized, null, this.Item_Click, entryId);

                    // show a check if there's a picture available
                    var match = Game.codexRef.matchFromEntryId(entryId);
                    item.Checked = match.variant.imageUrl != null;

                    toolChange.DropDownItems.Add(item);
                }
            }

            if (Game.activeGame.systemBody.predictions.Count > 0)
            {
                // show predictions
                toolChange.DropDownItems.Add(new ToolStripMenuItem("Predicted:") { Enabled = false });

                foreach (var foo in Game.activeGame.systemBody.predictions.Values)
                {
                    // skip if that entryId is already present
                    if (toolChange.DropDownItems.ContainsKey(foo.entryId)) continue;

                    var item = new ToolStripMenuItem(foo.englishName, null, this.Item_Click, foo.entryId);

                    // show a check if there's a picture available
                    item.Checked = foo.imageUrl != null;

                    toolChange.DropDownItems.Add(item);
                }
            }
        }

        private void Item_Click(object? sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            this.showEntryId(item.Name);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.Clear(BackColor);
            if (this.img == null) return;

            var x = mx - dragOffset.X * this.scale;
            var y = my - dragOffset.Y * this.scale;

            e.Graphics.DrawImage(this.img, x, y, w, h);
        }

        private void linkSubmitImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var url = $"https://docs.google.com/forms/d/e/1FAIpQLSdtS-78k6MDb_L2RodLnVGoB3r2958SA5ARnufAEZxLeoRbhA/viewform?entry.987977054={Uri.EscapeDataString(Game.settings.lastCommander!)}&entry.1282362439={Uri.EscapeDataString(match.variant.englishName)}&entry.468337930={Uri.EscapeDataString(match.entryId.ToString())}";
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
