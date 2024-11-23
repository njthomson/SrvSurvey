using BioCriterias;
using SrvSurvey.canonn;
using SrvSurvey.game;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace SrvSurvey
{
    internal partial class FormShowCodex : Form
    {
        public static FormShowCodex? activeForm;

        public static FormShowCodex show()
        {
            if (activeForm == null)
                FormShowCodex.activeForm = new FormShowCodex();

            Util.showForm(FormShowCodex.activeForm);
            FormShowCodex.activeForm.prepStuff();

            // pre-select the current body
            if (Game.activeGame?.systemBody != null)
            {
                var idx = FormShowCodex.activeForm.stuff.Keys.ToList().IndexOf(Game.activeGame.systemBody);
                if (idx >= 0)
                {
                    FormShowCodex.activeForm.setCurrants(idx);
                    FormShowCodex.activeForm.updateStuff();
                }
            }

            return FormShowCodex.activeForm;
        }

        private string entryId;
        private Image? img;
        private float scale = 0.25f;
        private float mx, my, w, h;
        public PointF dragOffset;
        private bool dragging = false;
        private Point mouseDownPoint;
        private bool menuVisible = false;
        private string? lastTempRangeVariant;
        private string? lastTempRange;

        private Dictionary<SystemBody, List<BioVariant>> stuff = new();
        private int idxBody;
        private int idxVariant;
        private SystemBody currentBody;
        private List<BioVariant> currentVariants;
        private BioVariant? currentVariant;

        private FormShowCodex()
        {
            this.ForeColor = GameColors.Orange;
            InitializeComponent();
            this.DoubleBuffered = true;
            this.toolMore.DropDownDirection = ToolStripDropDownDirection.AboveLeft;

            FlatButton.applyGameTheme(btnPrevBio, btnNextBio, btnPrevBody, btnNextBody, btnMenu);

            foreach (ToolStripItem item in this.statusStrip.Items)
                item.ForeColor = SystemColors.ControlText;

            // use our last location
            Util.useLastLocation(this, Game.settings.formShowCodex);
        }

        private void prepStuff()
        {
            var game = Game.activeGame;
            if (game?.systemData == null) return;
            stuff.Clear();

            foreach (var body in game.systemData.bodies.OrderBy(b => b.shortName))
            {
                if (body.bioSignalCount == 0) continue;
                if (!stuff.ContainsKey(body)) stuff[body] = new();

                // prep known organisms
                if (body.organisms?.Count > 0)
                    foreach (var org in body.organisms)
                        if (org.entryId > 0)
                            stuff[body].Add(Game.codexRef.matchFromEntryId(org.entryId).variant);

                // prep predictions
                if (body.genusPredictions?.Count > 0)
                    foreach (var genus in body.genusPredictions)
                        foreach (var variants in genus.species.Values)
                            foreach (var variant in variants)
                                stuff[body].Add(variant.variant);
            }

            setCurrants(0);
            updateStuff();
        }

        private void setCurrants(int? newIdxBody)
        {
            this.idxBody = newIdxBody ?? 0;
            this.idxVariant = 0;

            var pair = this.stuff.Skip(idxBody).First();
            this.currentBody = pair.Key;
            this.currentVariants = pair.Value;
            this.currentVariant = this.currentVariants[idxVariant];

            prepMenuItems();
        }

        private void updateStuff(bool forceCanonn = false)
        {
            if (currentVariants == null || currentVariants.Count == 0) return;
            currentVariant = currentVariants[idxVariant];

            // lookup temp range, if needed
            if (lastTempRangeVariant != currentVariant.name)
            {
                var clauses = BioPredictor.predictTarget(currentBody, currentVariant.englishName);
                var tempClause = clauses.FirstOrDefault(c => c?.property == "temp");
                if (tempClause?.min > 0 && tempClause?.max > 0)
                {
                    lastTempRange = $" | Temp range: {tempClause?.min}K ~ {tempClause?.max}K ";
                    Game.log($"{currentVariant.englishName} ({currentVariant.species.name}) => temperature range: {tempClause?.min} ~ {tempClause?.max}, default surface temperature: {currentBody.surfaceTemperature}, current: {Game.activeGame?.status?.Temperature}");
                }
                else
                {
                    lastTempRange = null;
                    Game.log($"{currentVariant.englishName} ({currentVariant.species.name}) => has no predictive temperature range :(");
                }
            }
            lastTempRangeVariant = currentVariant.name;

            var isPrediction = currentBody.organisms?.Any(o => o.entryId.ToString() == currentVariant.entryId) == false;

            lblBodyName.Text = currentBody.name + $" [{idxBody + 1} of {stuff.Count}]";
            lblTitle.Text = $"[{idxVariant + 1} of {currentVariants.Count}] " + currentVariant.englishName;
            lblDetails.Text = (isPrediction ? "Predicted" : "Confirmed")
                + $" | Min dist: {Util.metersToString((decimal)currentVariant.species.genus.dist)}"
                + $" | Reward: {Util.credits(currentVariant.reward)}"
                + lastTempRange;

            repositionBodyParts();
            var targetEntryId = currentVariant.entryId;

            Task.Run(() =>
            {
                var filepath = Path.Combine(CodexRef.codexImagesFolder, $"{currentVariant.entryId}.png");

                Image? nextImg = null;
                var nextCredits = "";

                // do we have a local image?
                var localFilepath = Game.settings.localFloraFolder == null ? null : Path.Combine(Game.settings.localFloraFolder, $"{currentVariant.localImgName}.png");
                if (localFilepath != null && File.Exists(localFilepath) && !forceCanonn)
                {
                    // load the cached image - quickly, so as not to lock the file
                    using (var imgTmp = Bitmap.FromFile(localFilepath))
                        nextImg = new Bitmap(imgTmp);

                    nextCredits = "(local image)";
                }
                // load image from disk
                else if (File.Exists(filepath))
                {
                    // load the cached image - quickly, so as not to lock the file
                    using (var imgTmp = Bitmap.FromFile(filepath))
                        nextImg = new Bitmap(imgTmp);

                    nextCredits = $"cmdr: {currentVariant.imageCmdr}";
                }

                if (nextImg != null)
                {
                    Program.defer(() =>
                    {
                        // exit if this is no longer what we're supposed to be showing
                        if (targetEntryId != currentVariant.entryId) return;

                        this.lblCmdr.Text = nextCredits;

                        // calculate scale to make the width fit
                        this.img = nextImg;
                        lblLoading.Visible = nextImg == null;
                        if (nextImg != null)
                        {
                            this.scale = (float)this.ClientRectangle.Width / (float)img.Width;
                            this.calcSizes(true, true);
                        }

                        this.Invalidate(true);
                    });
                }
            });
        }

        private void repositionBodyParts()
        {
            var left = this.ClientSize.Width - flowBodyParts.Width;

            if (lblTitle.Right < left)
                flowBodyParts.Top = lblTitle.Top;
            else if (lblDetails.Right < left)
                flowBodyParts.Top = lblDetails.Top;
            else
                flowBodyParts.Top = lblDetails.Bottom;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            repositionBodyParts();
        }

        private void FormShowCodex_Load(object sender, EventArgs e)
        {
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

        public void prepAllSpecies()
        {
            if (Game.activeGame?.systemBody?.organisms == null) return;

            if (Game.activeGame.systemBody.organisms.Any(o => o.variantLocalized != null))
                foreach (var org in Game.activeGame.systemBody.organisms)
                    if (string.IsNullOrEmpty(org.variantLocalized)) continue;

            if (Game.activeGame.systemBody.predictions.Count > 0)
                foreach (var bioVariant in Game.activeGame.systemBody.predictions.Values)
                    // skip if that entryId is already present
                    if (menuStrip.Items.ContainsKey(bioVariant.entryId)) continue;
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
            if (currentVariant == null) return;
            var url = $"https://docs.google.com/forms/d/e/1FAIpQLSdtS-78k6MDb_L2RodLnVGoB3r2958SA5ARnufAEZxLeoRbhA/viewform?entry.987977054={Uri.EscapeDataString(Game.settings.lastCommander!)}&entry.1282362439={Uri.EscapeDataString(currentVariant.englishName)}&entry.468337930={Uri.EscapeDataString(currentVariant.entryId.ToString())}";
            Util.openLink(url);
        }

        private void toolOpenCanonn_Click(object sender, EventArgs e)
        {
            if (currentVariant == null) return;
            var url = $"https://canonn-science.github.io/Codex-Regions/?entryid={Uri.EscapeDataString(currentVariant.entryId.ToString())}&hud_category=Biology";
            Util.openLink(url);
        }

        private void toolOpenBioforge_Click(object sender, EventArgs e)
        {
            if (currentVariant == null) return;
            var url = $"https://bioforge.canonn.tech/?entryid={Uri.EscapeDataString(currentVariant.englishName)}";
            Util.openLink(url);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (Game.activeGame?.systemData == null) return;
            Util.openLink("https://canonn-science.github.io/canonn-signals/?system=" + Uri.EscapeDataString(Game.activeGame.systemData.name));
        }

        private void viewOnSpanshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Game.activeGame?.systemData == null) return;
            var url = $"https://spansh.co.uk/system/{Game.activeGame.systemData.address}";
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

        #region static image loading

        public static void loadImages()
        {
            var game = Game.activeGame;
            if (game?.systemData?.bioSignalsTotal > 0)
                Task.Run(() => doLoadSystemImages(game.systemData).ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Game.log(t.Exception);
                        FormErrorSubmit.Show(t.Exception);
                    }
                }));
        }

        public static async Task doLoadSystemImages(SystemData systemData)
        {
            Game.log($"Loading codex images for: {systemData.name} ({systemData.address})");

            foreach (var body in systemData.bodies.ToList())
            {
                if (body.bioSignalCount == 0) continue;

                /*
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
                 */

                // get images for known organisms
                if (body.organisms?.Count > 0)
                {
                    foreach (var org in body.organisms.ToList())
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
                    foreach (var genus in body.genusPredictions.ToList())
                    {
                        foreach (var variants in genus.species.Values.ToList())
                        {
                            foreach (var variant in variants.ToList())
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
                    if (!File.Exists(filepath))
                        imgTmp.Save(filepath, ImageFormat.Png);
                }
            }
        }

        #endregion

        private void Item_MouseDown(object? sender, MouseEventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            var forceRemoteImage = e.Button == MouseButtons.Right;
            if (item.Tag is int)
            {
                idxVariant = (int)item.Tag;
                updateStuff(e.Button == MouseButtons.Right);
            }
        }

        private void prepMenuItems()
        {
            menuStrip.Items.Clear();

            // prep known organisms
            var knownOrganisms = currentBody.organisms?.Where(o => o.entryId > 0).ToList();
            if (knownOrganisms?.Count > 0)
            {
                // show confirmed variants
                menuStrip.Items.Add(new ToolStripMenuItem("Confirmed:") { Enabled = false });

                foreach (var org in knownOrganisms)
                {
                    if (org.entryId > 0)
                    {
                        var match = Game.codexRef.matchFromEntryId(org.entryId);
                        var item = new ToolStripMenuItem()
                        {
                            Name = entryId,
                            Text = org.variantLocalized,
                            Tag = currentVariants.IndexOf(match.variant),
                        };
                        item.MouseUp += this.Item_MouseDown;

                        // show a check if there's a picture available
                        item.Checked = match.variant.imageUrl != null;

                        menuStrip.Items.Add(item);
                    }
                }
            }

            // prep predictions
            if (currentBody.genusPredictions?.Count > 0)
            {
                // show predictions
                menuStrip.Items.Add(new ToolStripMenuItem("Predicted:") { Enabled = false });

                foreach (var genus in currentBody.genusPredictions)
                {
                    foreach (var variants in genus.species.Values)
                    {
                        foreach (var variant in variants)
                        {
                            var item = new ToolStripMenuItem()
                            {
                                Name = entryId,
                                Text = variant.variant.englishName,
                                Tag = currentVariants.IndexOf(variant.variant),
                            };
                            item.MouseUp += this.Item_MouseDown;

                            // show a check if there's a picture available from Canonn
                            item.Checked = variant.variant.imageUrl != null;

                            menuStrip.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void btnPrevBody_Click(object sender, EventArgs e)
        {
            idxBody--;
            if (idxBody < 0) idxBody = stuff.Count - 1;

            var pair = stuff.Skip(idxBody).First();
            currentBody = pair.Key;
            currentVariants = pair.Value;
            if (idxVariant >= currentVariants.Count) idxVariant = currentVariants.Count - 1;

            prepMenuItems();
            updateStuff();
        }

        private void btnNextBody_Click(object sender, EventArgs e)
        {
            idxBody++;
            if (idxBody >= stuff.Count) idxBody = 0;

            var pair = stuff.Skip(idxBody).First();
            currentBody = pair.Key;
            currentVariants = pair.Value;
            if (idxVariant >= currentVariants.Count) idxVariant = currentVariants.Count - 1;

            prepMenuItems();
            updateStuff();
        }

        private void btnPrevBio_Click(object sender, EventArgs e)
        {
            idxVariant--;
            if (idxVariant < 0) idxVariant = currentVariants.Count - 1;

            updateStuff();
        }

        private void btnNextBio_Click(object sender, EventArgs e)
        {
            idxVariant++;
            if (idxVariant >= currentVariants.Count) idxVariant = 0;

            updateStuff();
        }

        private void btnMenu_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentVariant == null) return;

            if (!menuVisible)
            {
                menuStrip.Show(btnMenu, new Point(0, btnMenu.Height));
                btnMenu.Text = "⏶";
                menuVisible = true;
            }
            //else if (Game.activeGame?.systemBody != null && Debugger.IsAttached)
            //{
            //    var match = Game.codexRef.matchFromEntryId(currentVariant.entryId);
            //    var clauses = BioPredictor.predictTarget(Game.activeGame.systemBody, currentVariant.englishName);
            //    var tempClause = clauses.FirstOrDefault(c => c?.property == "temp");
            //    Game.log($"{match.variant.englishName} ({match.species.name}) => range: {tempClause?.min} ~ {tempClause?.max}, default: {Game.activeGame!.systemBody!.surfaceTemperature}, current: {Game.activeGame.status.Temperature}");
            //}
        }

        private void btnMenu_MouseEnter(object sender, EventArgs e)
        {
            menuVisible = menuStrip.Visible;
        }

        private void menuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Program.defer(() =>
            {
                menuVisible = false;
                btnMenu.Text = "⏷";
            });
        }
    }
}
