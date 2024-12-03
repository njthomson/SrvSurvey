using BioCriterias;
using SrvSurvey.canonn;
using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.widgets;
using System.Drawing.Imaging;

namespace SrvSurvey
{
    [Draggable, TrackPosition]
    internal partial class FormShowCodex : SizableForm
    {
        public static bool allow { get => Game.activeGame?.systemData?.bioSignalsTotal > 0; }

        public static void update()
        {
            //Game.log("FormShowCodex.update ************************************");

            // update form if it exists
            var form = BaseForm.get<FormShowCodex>();
            form?.prepMenuItems();
            form?.updateStuff();

            // check/download images in the background
            loadImages();
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

        public FormShowCodex()
        {
            this.ForeColor = GameColors.Orange;
            InitializeComponent();
            this.DoubleBuffered = true;
            this.toolMore.DropDownDirection = ToolStripDropDownDirection.AboveLeft;

            FlatButton.applyGameTheme(btnPrevBio, btnNextBio, btnPrevBody, btnNextBody, btnMenu);

            foreach (ToolStripItem item in this.statusStrip.Items)
                item.ForeColor = SystemColors.ControlText;

            Game.update += Game_update;
        }

        private void Game_update(GameMode newMode, bool force)
        {
            //var currentTemp = Game.activeGame?.status.Temperature ?? 0;
            //if (currentTemp > 0 && newMode == GameMode.OnFoot)
            //{
            lastTempRangeVariant = null;
            this.updateStuff();
            //}
        }

        protected override void beforeShowing()
        {
            this.prepStuff();

            // pre-select the current body
            if (Game.activeGame?.systemBody != null)
            {
                var idx = this.stuff.Keys.ToList().IndexOf(Game.activeGame.systemBody);
                if (idx >= 0)
                {
                    this.setCurrants(idx);
                    this.updateStuff();
                }
            }

            repositionBodyParts();
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
            this.currentVariant = this.currentVariants.Count > 0 ? this.currentVariants[idxVariant] : null;

            prepMenuItems();
        }

        /// <summary> Call when changing the thing we're looking at </summary>
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
                    lastTempRange = $" | Temp range: {tempClause.min}K ~ {tempClause.max}K ";
                    if (Game.activeGame?.mode == GameMode.OnFoot)
                    {
                        var currentTemp = Game.activeGame?.status.Temperature ?? 0;
                        if (currentTemp > tempClause.max)
                            lastTempRange += " (too hot)";
                        if (currentTemp > 0 && currentTemp < tempClause.min)
                            lastTempRange += " (too cold)";
                    }

                    Game.log($"{currentVariant.englishName} ({currentVariant.species.name}) => temperature range: {tempClause?.min} ~ {tempClause?.max}, default surface temperature: {currentBody.surfaceTemperature}, current: {Game.activeGame?.status?.Temperature}");
                }
                else
                {
                    lastTempRange = null;
                    Game.log($"{currentVariant.englishName} ({currentVariant.species.name}) => has no predictive temperature range :(");
                }
            }
            lastTempRangeVariant = currentVariant.name;

            lblBodyName.Text = currentBody.name + $" [{idxBody + 1} of {stuff.Count}]";
            lblTitle.Text = $"[{idxVariant + 1} of {currentVariants.Count}] " + currentVariant.englishName;

            var org = currentBody.organisms?.FirstOrDefault(o => o.entryId.ToString() == currentVariant.entryId);
            var prefix = "Predicted";
            if (org?.analyzed == true)
                prefix = "Analyzed";
            else if (org?.scanned == true)
                prefix = "Confirmed";
            else if (org != null)
                prefix = "Reported";

            lblDetails.Text = prefix
                + $" | Min dist: {Util.metersToString((decimal)currentVariant.species.genus.dist)}"
                + $" | Reward: {Util.credits(currentVariant.reward)}"
                + lastTempRange;

            repositionBodyParts();
            var targetEntryId = currentVariant.entryId;

            // skip loading the image if nothing changed
            if (currentVariant.name == this.img?.Tag as string && !forceCanonn) return;

            // now load the image
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
                        this.img.Tag = currentVariant.name;

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
            if (flowBodyParts == null) return;

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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            this.calcSizes(true, false);
            this.Invalidate();
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
            var variant = item.Tag as BioVariant;
            var body = item.OwnerItem.Tag as SystemBody;
            if (variant != null && body != null)
            {
                if (body == currentBody)
                {
                    idxVariant = stuff[body].IndexOf(variant);
                    updateStuff(e.Button == MouseButtons.Right);
                }
                else
                {
                    currentBody = body;
                    currentVariants = stuff[body];
                    idxVariant = stuff[body].IndexOf(variant);
                    updateStuff(e.Button == MouseButtons.Right);
                }
            }

        }

        private void prepMenuItems()
        {
            // add menus for the other bodies
            var menuTree = new List<ToolStripMenuItem>();

            var bodies = currentBody.system.bodies;
            foreach (var body in bodies)
            {
                if (body.bioSignalCount == 0) continue;

                // show confirmed variants
                var bodyMenu = new ToolStripMenuItem(body.name) { Tag = body, Name = body.name };
                menuTree.Add(bodyMenu);

                // known organisms
                foreach (var org in body.organisms ?? new())
                {
                    if (org.entryId == 0) continue;
                    var match = Game.codexRef.matchFromEntryId(org.entryId);
                    var item = new ToolStripMenuItem()
                    {
                        Name = entryId,
                        Text = org.variantLocalized,
                        Tag = match.variant,
                        CheckState = org.scanned ? CheckState.Indeterminate : CheckState.Unchecked,
                    };
                    if (org.analyzed) item.CheckState = CheckState.Checked;

                    item.MouseUp += this.Item_MouseDown;

                    bodyMenu.DropDownItems.Add(item);
                }
                // predictions
                foreach (var genus in body.genusPredictions)
                {
                    foreach (var variants in genus.species.Values)
                    {
                        foreach (var variant in variants)
                        {
                            var item = new ToolStripMenuItem()
                            {
                                Name = entryId,
                                Text = "? " + variant.variant.englishName,
                                Tag = variant.variant,
                            };
                            item.MouseUp += this.Item_MouseDown;

                            bodyMenu.DropDownItems.Add(item);
                        }
                    }
                }
            }

            menuStrip.Items.Clear();
            menuStrip.Items.AddRange(menuTree.ToArray());
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

                // pre-expand the current body
                var idxCurrentBody = menuStrip.Items.IndexOfKey(currentBody.name);
                var menuCurrentBody = (ToolStripMenuItem)menuStrip.Items[idxCurrentBody];
                menuCurrentBody.Select();
                menuCurrentBody.ShowDropDown();
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
