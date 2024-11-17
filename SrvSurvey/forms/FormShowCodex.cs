using SrvSurvey.canonn;
using SrvSurvey.game;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace SrvSurvey
{
    internal partial class FormShowCodex : Form
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

        public async void showEntryId(string entryId, bool forceRemoteImage = false)
        {
            Game.log($"showEntryId: {entryId}");
            // also find any other things we can show images for
            this.prepAllSpecies();
            if (entryId == this.entryId && !forceRemoteImage) return;

            this.lblLoading.Show();
            this.panelSubmit.Hide();
            this.img = null;
            this.entryId = entryId;
            this.match = Game.codexRef.matchFromEntryId(entryId);
            this.lblTitle.Text = match.variant.englishName;
            this.lblCmdr.Text = $"cmdr: {match.variant.imageCmdr}";

            var filepath = Path.Combine(CodexRef.codexImagesFolder, $"{match.entryId}.png");

            // do we have a local image?
            if (!string.IsNullOrEmpty(Game.settings.localFloraFolder) && !forceRemoteImage)
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
                // download images once a month
                var duration = DateTime.Now.Subtract(File.GetLastWriteTime(filepath));
                if (duration.TotalDays < 30 || true)
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

            // download if we don't have an imagefile already
            if (this.img == null && match.variant.imageUrl != null)
            {
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

            if (match.variant.imageUrl == null)
            {
                // we have no url
                this.panelSubmit.Show();
                this.lblCmdr.Text = "";
            }

            if (this.img != null)
            {
                // calc scale to make the width fit
                this.scale = (float)this.ClientRectangle.Width / (float)img.Width;
                this.calcSizes(true, true);
            }
            this.Invalidate(true);
            this.lblLoading.Hide();
        }

        public void prepAllSpecies()
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
                    var item = new ToolStripMenuItem()
                    {
                        Name = entryId,
                        Text = org.variantLocalized,
                    };
                    item.MouseUp += this.Item_MouseDown;

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

                foreach (var bioVariant in Game.activeGame.systemBody.predictions.Values)
                {
                    // skip if that entryId is already present
                    if (toolChange.DropDownItems.ContainsKey(bioVariant.entryId)) continue;

                    var item = new ToolStripMenuItem()
                    {
                        Name = bioVariant.entryId,
                        Text = bioVariant.englishName,
                    };
                    item.MouseUp += this.Item_MouseDown;

                    // show a check if there's a picture available from Canonn
                    item.Checked = bioVariant.imageUrl != null;

                    toolChange.DropDownItems.Add(item);
                }
            }
        }

        private void Item_MouseDown(object? sender, MouseEventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            var forceRemoteImage = e.Button == MouseButtons.Right;
            this.showEntryId(item.Name, forceRemoteImage);
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

        private void lblTitle_DoubleClick(object sender, EventArgs e)
        {
            this.openImageSurveyPage();
        }

        private void linkSubmitImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.openImageSurveyPage();
        }

        private void openImageSurveyPage()
        {
            var url = $"https://docs.google.com/forms/d/e/1FAIpQLSdtS-78k6MDb_L2RodLnVGoB3r2958SA5ARnufAEZxLeoRbhA/viewform?entry.987977054={Uri.EscapeDataString(Game.settings.lastCommander!)}&entry.1282362439={Uri.EscapeDataString(match.variant.englishName)}&entry.468337930={Uri.EscapeDataString(match.entryId.ToString())}";
            Util.openLink(url);
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            var url = $"https://canonn-science.github.io/Codex-Regions/?entryid={Uri.EscapeDataString(match.entryId.ToString())}&hud_category=Biology";
            Util.openLink(url);
        }

        private void toolOpenBioforge_Click(object sender, EventArgs e)
        {
            var url = $"https://bioforge.canonn.tech/?entryid={Uri.EscapeDataString(match.variant.englishName)}";
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

        public static void loadImages()
        {
            var game = Game.activeGame;
            if (game?.systemData?.bioSignalsTotal > 0)
                doLoadSystemImages(game.systemData).ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Game.log(t.Exception);
                        FormErrorSubmit.Show(t.Exception);
                    }
                });
        }

        public static async Task doLoadSystemImages(SystemData systemData)
        {
            Game.log($"Loading codex images for: {systemData.name} ({systemData.address})");

            foreach (var body in systemData.bodies)
            {
                if (body.bioSignalCount == 0) continue;

                // get images for known organisms
                if (body.organisms?.Count > 0)
                {
                    foreach (var org in body.organisms)
                    {
                        if (org.entryId == 0) continue;

                        // skip if no url or we already have the file
                        var match = Game.codexRef.matchFromEntryId(org.entryId);
                        if (match.variant.imageUrl == null) continue;

                        var filepath = Path.Combine(CodexRef.codexImagesFolder, $"{org.entryId}.png");
                        if (File.Exists(filepath)) continue;

                        // otherwise download it
                        await downloadImage(match.variant.imageUrl, filepath);
                    }
                }

                // get images for predictions
                if (body.genusPredictions?.Count > 0)
                {
                    foreach (var genus in body.genusPredictions)
                    {
                        foreach (var variants in genus.species.Values)
                        {
                            foreach (var variant in variants)
                            {
                                // skip if no url or we already have the file
                                if (variant.variant.imageUrl == null) continue;

                                var filepath = Path.Combine(CodexRef.codexImagesFolder, $"{variant.variant.entryId}.png");
                                if (File.Exists(filepath)) continue;

                                // otherwise download it
                                await downloadImage(variant.variant.imageUrl, filepath);
                            }
                        }
                    }
                }
            }
        }

        private static async Task downloadImage(string imageUrl, string filepath)
        {
            Game.log($"Downloading: {imageUrl} => {filepath}");
            using (var stream = await new HttpClient().GetStreamAsync(imageUrl))
            {
                using (var imgTmp = Image.FromStream(stream))
                {
                    imgTmp.Save(filepath, ImageFormat.Png);
                }
            }
        }
    }
}
