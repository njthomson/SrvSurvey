using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using static SrvSurvey.game.GuardianSiteData;

namespace SrvSurvey
{
    public enum Mode
    {
        siteType,   // ask about site type
        heading,    // ask about site heading
        map,        // show site map
        origin,     // show alignment to site origin
    }

    internal partial class PlotGuardians : PlotBase, IDisposable
    {
        public SiteTemplate? template;
        private Image? siteMap;
        //private Image? trails;
        private Image? underlay;
        public Image? headingGuidance;
        private string highlightPoi; // tmp
        public SitePOI nearestPoi;
        public SitePOI? forcePoi;
        public FormEditMap? formEditMap;

        /// <summary> Site map width </summary>
        private float ux;
        /// <summary> Site map height </summary>
        private float uy;

        public Mode mode;
        private PointF commanderOffset;

        private GuardianSiteData siteData { get => game?.nearBody?.siteData!; }

        private PlotGuardians() : base()
        {
            if (PlotGuardians.instance != null)
            {
                Game.log("Why are there multiple PlotGuardians?");
                Program.closePlotter<PlotGuardians>();
                Application.DoEvents();
                PlotGuardians.instance.Dispose();
            }

            PlotGuardians.instance = this;
            InitializeComponent();

            // set window size based on setting
            switch (Game.settings.idxGuardianPlotter)
            {
                case 0: this.Width = 300; this.Height = 400; break;
                case 1: this.Width = 500; this.Height = 500; break;
                case 2: this.Width = 600; this.Height = 700; break;
                case 3: this.Width = 800; this.Height = 1000; break;
                case 4: this.Width = 1200; this.Height = 1200; break;
            }

            this.scale = this.siteData.isRuins ? 0.65f : 1.5f;

            this.nextMode();
        }

        public void devRefreshBackground(string imagePath)
        {
            if (this.template == null) return;

            Game.log("devRefreshBackground");
            this.loadSiteTemplate(imagePath);
            this.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            PlotGuardians.instance = null;
        }

        public override void reposition(Rectangle gameRect)
        {
            if (gameRect == Rectangle.Empty)
            {
                this.Opacity = 0;
                return;
            }

            this.Opacity = Game.settings.Opacity;
            Elite.floatLeftMiddle(this, gameRect);

            this.Invalidate();
        }

        private void PlotGuardians_Load(object sender, EventArgs e)
        {
            this.initialize();
            this.reposition(Elite.getWindowRect(true));
        }

        protected override void initialize()
        {
            base.initialize();

            this.nextMode();
        }

        private void nextMode()
        {
            /*************************************************************
             * Sequence is:
             * A) Site type
             * B) Site heading
             * C) Show the map
             */

            if (siteData.type == GuardianSiteData.SiteType.Unknown)
            {
                // we need to know the site type before anything else
                this.setMode(Mode.siteType);
            }
            else if (siteData.siteHeading == -1 || siteData.siteHeading == 0)
            {
                // then we need to know the site heading
                this.setMode(Mode.heading);
            }
            else
            {
                // we can show the map after that
                this.loadSiteTemplate();
                this.setMode(Mode.map);
            }
        }

        private void setMode(Mode newMode)
        {
            Game.log($"* * *> PlotGuardians: changing mode from: {this.mode} to: {newMode}");

            // do not allow some modes before we know others
            if (siteData.type == GuardianSiteData.SiteType.Unknown && newMode != Mode.siteType)
            {
                Game.log($"PlotGuardians: site type must be known first");
                newMode = Mode.siteType;
            }

            if (siteData.siteHeading == -1 && (newMode != Mode.siteType && newMode != Mode.heading))
            {
                Game.log($"PlotGuardians: heading must be known first");
                newMode = Mode.heading;
            }

            this.mode = newMode;
            this.Invalidate();
            game.fireUpdate();

            // show or hide the heading vertical stripe helper
            if (!Game.settings.disableRuinsMeasurementGrid)
            {
                if (this.mode == Mode.heading)
                {
                    PlotVertialStripe.targetAltitude = 20;
                    PlotVertialStripe.mode = PlotVertialStripe.mode = PlotVertialStripe.Mode.Buttress;
                    Program.showPlotter<PlotVertialStripe>();
                }
                else
                    Program.closePlotter<PlotVertialStripe>();
            }

            // load heading guidance image if that is the next mode
            if (this.mode == Mode.heading)
                this.loadHeadingGuidance();

            if (this.mode == Mode.origin)
            {
                if (!Game.settings.disableAerialAlignmentGrid)
                {
                    PlotVertialStripe.targetAltitude = 20;
                    showAiming();
                }
            }
            else if (this.mode != Mode.heading)
            {
                // close potential plotter
                Program.closePlotter<PlotVertialStripe>();
            }
        }

        private void showAiming()
        {
            // close existing
            Program.closePlotter<PlotVertialStripe>();
            if (Game.settings.disableAerialAlignmentGrid) return;

            if (game.vehicle == ActiveVehicle.SRV)
            {
                Game.log("showAiming: not in SRV");
                return;
            }

            switch (this.siteData.type)
            {
                case SiteType.Alpha:
                    PlotVertialStripe.mode = PlotVertialStripe.Mode.Alpha;
                    PlotVertialStripe.targetAltitude = Game.settings.aerialAltAlpha;
                    break;
                case SiteType.Beta:
                    PlotVertialStripe.mode = PlotVertialStripe.Mode.Beta;
                    PlotVertialStripe.targetAltitude = Game.settings.aerialAltBeta;
                    break;
                case SiteType.Gamma:
                    PlotVertialStripe.mode = PlotVertialStripe.Mode.Gamma;
                    PlotVertialStripe.targetAltitude = Game.settings.aerialAltGamma;
                    break;

                case SiteType.Robolobster:
                    PlotVertialStripe.mode = PlotVertialStripe.Mode.Robolobster;
                    PlotVertialStripe.targetAltitude = 1500;// TODO: Game.settings.aerialAltGamma;
                    break;
            }

            // assume onFoot only means Relic Towers
            if (game.vehicle == ActiveVehicle.Foot)
            {
                PlotVertialStripe.mode = PlotVertialStripe.Mode.RelicTower;
                PlotVertialStripe.targetAltitude = 0;
            }

            Game.log($"showAiming: {PlotVertialStripe.mode}");
            Program.showPlotter<PlotVertialStripe>();
        }

        protected override void onJournalEntry(SendText entry)
        {
            base.onJournalEntry(entry);

            var msg = entry.Message.ToLowerInvariant();
            if (siteData == null)
            {
                Game.log($"Why no game.nearBody?.siteData?");
                return;
            }

            // switch to/from offset/screenshot mode
            if (msg == MsgCmd.aerial)
            {
                this.setMode(Mode.origin);
                return;
            }
            if (msg == MsgCmd.map)
            {
                this.setMode(Mode.map);
                return;
            }
            if (msg == MsgCmd.aim)
            {
                if (Program.isPlotter<PlotVertialStripe>())
                    Program.closePlotter<PlotVertialStripe>();
                else
                    this.showAiming();
                return;
            }
            if (msg.StartsWith(MsgCmd.note, StringComparison.InvariantCultureIgnoreCase))
            {
                var note = msg.Substring(MsgCmd.note.Length).Trim();
                this.siteData.notes += $"\r\n{note}\r\n";
                this.siteData.Save();
            }

            // try parsing the raw text as site type?
            GuardianSiteData.SiteType parsedType = GuardianSiteData.SiteType.Unknown;
            if (siteData.type == GuardianSiteData.SiteType.Unknown || this.mode == Mode.siteType)
            {
                // try matching to the enum?
                if (!Enum.TryParse<GuardianSiteData.SiteType>(msg, true, out parsedType))
                {
                    // try matching to other strings?
                    switch (msg)
                    {
                        case "a":
                            parsedType = GuardianSiteData.SiteType.Alpha;
                            break;
                        case "b":
                            parsedType = GuardianSiteData.SiteType.Beta;
                            break;
                        case "g":
                            parsedType = GuardianSiteData.SiteType.Gamma;
                            break;
                    }
                }
            }
            else if (msg.StartsWith(MsgCmd.site))
            {
                // '.site <alpha>'
                Enum.TryParse<GuardianSiteData.SiteType>(msg.Substring(MsgCmd.site.Length), true, out parsedType);
            }

            if (parsedType != GuardianSiteData.SiteType.Unknown)
            {
                Game.log($"Changing site type from: '{siteData.type}' to: '{parsedType}'");
                siteData.type = parsedType;
                siteData.Save();

                this.nextMode();
                return;
            }

            // heading stuff...

            var changeHeading = false;
            int newHeading = -1;
            // try parsing some number as the site heading
            if (this.mode == Mode.heading)
                changeHeading = int.TryParse(msg, out newHeading);

            // msg is just '.heading'
            if (msg == MsgCmd.heading)
            {
                if (this.mode == Mode.heading)
                {
                    // take the current cmdr's heading
                    newHeading = game.status.Heading;
                    changeHeading = true;
                }
                else
                {
                    // move into heading mode
                    this.setMode(Mode.heading);
                    return;
                }
            }
            else if (msg.StartsWith(MsgCmd.heading))
            {
                // try parsing a number after '.heading'
                changeHeading = int.TryParse(msg.Substring(MsgCmd.heading.Length), out newHeading);
            }

            if (changeHeading)
            {
                this.setSiteHeading(newHeading);
                return;
            }

            // set Relic Tower heading
            if (msg == MsgCmd.tower)
            {
                var newAngle = new Angle(game.status.Heading);
                Game.log($"Changing Relic Tower heading from: {siteData.relicTowerHeading}° to: {newAngle}");
                siteData.relicTowerHeading = newAngle;
                siteData.Save();
                Program.closePlotter<PlotVertialStripe>();
            }
            else if (msg.StartsWith(MsgCmd.tower) && int.TryParse(msg.Substring(MsgCmd.tower.Length), out newHeading))
            {
                var newAngle = new Angle(newHeading);
                Game.log($"Changing Relic Tower heading from: {siteData.relicTowerHeading}° to: {newAngle}");
                siteData.relicTowerHeading = newAngle;
                siteData.Save();
            }

            if (changeHeading)
            {
                this.setSiteHeading(newHeading);
                return;
            }

            // empty puddle?
            if (msg == MsgCmd.empty && this.nearestPoi != null)
            {
                siteData.poiStatus[this.nearestPoi.name] = SitePoiStatus.empty;
                siteData.Save();
            }

            // set obelisk items
            if (msg.StartsWith(MsgCmd.ao, StringComparison.OrdinalIgnoreCase))
            {
                this.parseActiveObelisk(msg.Substring(3).Trim().ToLowerInvariant());
            }
            else if (msg.StartsWith(MsgCmd.aod, StringComparison.OrdinalIgnoreCase) && this.nearestPoi?.type == POIType.obelisk)
            {
                if (!this.siteData.activeObelisks.ContainsKey(this.nearestPoi.name))
                    this.siteData.activeObelisks.Add(this.nearestPoi.name, new ActiveObelisk());
                var obelisk = this.siteData.activeObelisks[this.nearestPoi.name];

                Game.log($"TODO Marking Active Obelisk '{this.nearestPoi.name}' as yielding: {msg}");

                //var obelisk = this.siteData.activeObelisks[this.nearestPoi.name];
                //obelisk.data = entry.Name;
                //siteData.Save();
            }
            else if (msg.StartsWith(MsgCmd.aom, StringComparison.OrdinalIgnoreCase) && this.nearestPoi != null)
            {
                if (!this.siteData.activeObelisks.ContainsKey(this.nearestPoi.name))
                    this.siteData.activeObelisks.Add(this.nearestPoi.name, new ActiveObelisk());
                var obelisk = this.siteData.activeObelisks[this.nearestPoi.name];

                obelisk.msg = msg.Substring(4).Trim().ToUpperInvariant();
                siteData.Save();
            }

            // temporary stuff after here
            this.xtraCmds(msg);
        }

        private void xtraCmds(string msg)
        {
            if (this.template != null && msg.StartsWith("sf"))
            {
                // temporary
                float newScale;
                if (float.TryParse(msg.Substring(3), out newScale))
                {
                    Game.log($"scaleFactor: {this.template.scaleFactor} => {newScale}");
                    this.template.scaleFactor = newScale;
                    this.Status_StatusChanged(false);
                }
            }

            if (msg == "..")
            {
                var dist = ((double)Util.getDistance(Status.here, siteData.location, (decimal)game.nearBody!.radius)).ToString("N1");
                // get angle relative to North, then adjust by siteHeading
                var angle = ((float)new Angle((Util.getBearing(Status.here, siteData.location) - siteData.siteHeading))).ToString("N1");
                var rot = (int)new Angle(game.status.Heading - this.siteData.siteHeading);
                Game.log($"!! dist: {dist}, angle: {angle}, rot: {rot}");
                var json = $"{{ \"name\": \"xxx\", \"dist\": {dist}, \"angle\": {angle}, \"type\": \"unknown\", \"rot\": {rot} }},";
                Game.log(json);
                Clipboard.SetText(json + "\r\n");
            }

            if (msg == "ll")
            {
                Game.log("Reloading site template");
                SiteTemplate.Import(true);
                this.loadSiteTemplate();
                this.Invalidate();
            }

            if (msg == "watch")
                this.devFileWatcher();

            if (msg == ".editmap" && this.formEditMap == null)
            {
                new FormEditMap().Show();
            }
            else if (msg == "ee" && this.formEditMap != null)
            {
                formEditMap.setCurrentPoi(nearestPoi);
                formEditMap.Activate();
            }

            if (msg == ".p")
                confirmPOI(SitePoiStatus.present);
            else if (msg == ".m")
                confirmPOI(SitePoiStatus.absent);
            else if (msg == ".e")
                confirmPOI(SitePoiStatus.empty);

            if (msg.StartsWith(">>"))
            {
                this.highlightPoi = msg.Substring(2).Trim();
                Game.log($"Highlighting POI: '{this.highlightPoi}'");
                this.Invalidate();
            }
        }

        private void parseActiveObelisk(string msg)
        {
            var parts = msg.Split(' ');
            if (this.nearestPoi.type != POIType.obelisk || parts.Length == 0) return;

            // add or retreive obelisk
            if (!this.siteData.activeObelisks.ContainsKey(this.nearestPoi.name))
                this.siteData.activeObelisks.Add(this.nearestPoi.name, new ActiveObelisk());
            var obelisk = this.siteData.activeObelisks[this.nearestPoi.name];

            obelisk.items = new List<ObeliskItem>();

            var item1 = parseActiveObeliskItem(parts[0]);
            if (item1 != ObeliskItem.unknown) obelisk.items.Add(item1);

            if (parts.Length == 2)
            {
                var item2 = parseActiveObeliskItem(parts[1]);
                if (item2 != ObeliskItem.unknown) obelisk.items.Add(item2);
            }
            Game.log($"Updating active obelisk '{this.nearestPoi.name}' to need items: " + string.Join(',', obelisk.items));
            siteData.Save();
            this.Invalidate();
        }

        private ObeliskItem parseActiveObeliskItem(string txt)
        {
            switch (txt)
            {
                case "ca":
                case "casket":
                    return ObeliskItem.casket;
                case "or":
                case "orb":
                    return ObeliskItem.orb;
                case "re":
                case "relic":
                    return ObeliskItem.relic;
                case "ta":
                case "tablet":
                    return ObeliskItem.tablet;
                case "to":
                case "totem":
                    return ObeliskItem.totem;
                case "ur":
                case "urn":
                    return ObeliskItem.urn;

                case "se":
                case "sensor":
                    return ObeliskItem.sensor;
                case "pr":
                case "probe":
                    return ObeliskItem.probe;
                case "li":
                case "link":
                    return ObeliskItem.link;
                case "cy":
                case "cyclops":
                    return ObeliskItem.cyclops;
                case "ba":
                case "basilisk":
                    return ObeliskItem.basilisk;
                case "me":
                case "medusa":
                    return ObeliskItem.medusa;

                default:
                    Game.log($"Unspected item for obelisk: {txt}");
                    return ObeliskItem.unknown;
            }
        }

        protected override void onJournalEntry(CodexEntry entry)
        {
            // exit if we have no selected POI or it isn't a relic
            if (this.nearestPoi == null || this.nearestPoi.type != POIType.relic) return;

            // add POI to confirmed
            // use combat/exploration mode to know if item is present or missing
            Game.log($"POI confirmed: '{this.nearestPoi.name}' ({this.nearestPoi.type})");
            siteData.poiStatus[this.nearestPoi.name] = SitePoiStatus.present;
            siteData.Save();
            this.Invalidate();
        }

        protected override void onJournalEntry(MaterialCollected entry)
        {
            // TODO: Finish this feature
            if (!Debugger.IsAttached) return;

            if (this.nearestPoi.type != POIType.obelisk) return;
            Game.log($"Marking Active Obelisk '{this.nearestPoi.name}' as yielding: {entry.Name_Localised} ({entry.Name})");

            if (!this.siteData.activeObelisks.ContainsKey(this.nearestPoi.name))
                this.siteData.activeObelisks.Add(this.nearestPoi.name, new ActiveObelisk());
            var obelisk = this.siteData.activeObelisks[this.nearestPoi.name];

            if (obelisk.data == null)
                obelisk.data = new HashSet<ObeliskData>();

            switch (entry.Name)
            {
                case "ancientbiologicaldata":
                    obelisk.data.Add(ObeliskData.alpha);
                    break;
                case "ancientlanguagedata":
                    obelisk.data.Add(ObeliskData.delta);
                    break;
                case "ancienttechnologicaldata":
                    obelisk.data.Add(ObeliskData.epsilon);
                    break;
                case "ancienthistoricaldata":
                    obelisk.data.Add(ObeliskData.gamma);
                    break;
            }

            siteData.Save();
            this.Invalidate();
        }

        protected override void onJournalEntry(Embark entry)
        {
            base.onJournalEntry(entry);

            this.scale = this.siteData.isRuins ? 0.65f : 1.5f;
        }

        protected override void onJournalEntry(Disembark entry)
        {
            base.onJournalEntry(entry);

            this.scale = 2f;
        }


        private void setSiteHeading(int newHeading)
        {
            Game.log($"Changing site heading from: '{siteData.siteHeading}' to: '{newHeading}'");
            siteData.siteHeading = (int)newHeading;
            siteData.Save();

            this.nextMode();
            this.Invalidate();
            game.fireUpdate(true);
        }

        private void loadSiteTemplate(string? imagePath = null)
        {
            if (siteData == null || siteData.type == GuardianSiteData.SiteType.Unknown) return;

            if (!SiteTemplate.sites.ContainsKey(siteData.type))
            {
                // create an empty site if needed
                Game.log($"Creating empty site for: {siteData.type}");
                SiteTemplate.sites[siteData.type] = new SiteTemplate()
                {
                    name = siteData.name
                };
            }

            this.template = SiteTemplate.sites[siteData.type];

            var folder = Path.GetDirectoryName(Application.ExecutablePath)!;
            var filepath = imagePath ?? Path.Combine(folder, "images", $"{siteData.type}-background.png".ToLowerInvariant());
            Game.log($"Loading image: {filepath}");
            if (File.Exists(filepath))
                this.siteMap = Bitmap.FromFile(filepath);
            else
                this.siteMap = new Bitmap(100, 100);

            //this.trails = new Bitmap(this.siteMap.Width * 2, this.siteMap.Height * 2);

            // Temporary until trail tracking works
            //using (var gg = Graphics.FromImage(this.trails))
            //{
            //    gg.Clear(Color.FromArgb(128, Color.Navy));
            //    //gg.FillRectangle(Brushes.Blue, -400, -400, 800, 800);
            //    gg.DrawRectangle(GameColors.penGameOrange8, 0, 0, this.trails.Width, this.trails.Height);
            //    gg.DrawLine(GameColors.penCyan8, 0, 0, trails.Width, trails.Height);
            //    gg.DrawLine(GameColors.penCyan8, trails.Width, 0, 0, trails.Height);
            //    gg.DrawLine(GameColors.penYellow8, 0, 0, trails.Height, 0);
            //    //gg.FillRectangle(Brushes.Red, (this.trails.Width / 2), (this.trails.Height / 2), 40, 40);
            //}

            // prepare underlay
            this.underlay = new Bitmap(this.siteMap.Width * 3, this.siteMap.Height * 3);
            this.ux = this.underlay.Width / 2;
            this.uy = this.underlay.Height / 2;

            //// prepare other stuff
            //var offset = game.nearBody!.guardianSiteLocation! - touchdownLocation.Target;
            //Game.log($"Touchdown offset: {offset}");
            //this.siteTouchdownOffset = new PointF(
            //    (float)(offset.Long * template.scaleFactor),
            //    (float)(offset.Lat * -template.scaleFactor));
            //Game.log(siteTouchdownOffset);
            //Game.log($"siteTouchdownOffset: {siteTouchdownOffset}");

            this.Status_StatusChanged(false);
        }

        private void loadHeadingGuidance()
        {
            if (this.headingGuidance != null)
            {
                this.headingGuidance.Dispose();
                this.headingGuidance = null;
            }

            var filepath = Path.Combine("images", $"{siteData.type}-heading-guide.png");
            if (File.Exists(filepath))
            {
                using (var img = Bitmap.FromFile(filepath))
                    this.headingGuidance = new Bitmap(img);
            }
        }

        protected void confirmPOI(SitePoiStatus poiStatus)
        {
            //SitePoiStatus poiStatus;
            if (poiStatus == SitePoiStatus.unknown)
            //{
            //    // confirm POI is missing/present by param
            //    poiStatus = present ? SitePoiStatus.present : SitePoiStatus.absent;
            //}
            //else
            {
                // confirm POI is missing/present/empty by fire groups
                poiStatus = (SitePoiStatus)(game.status.FireGroup % 3) + 1;
            }

            // (don't let Relic's get a status of Empty)
            if (!(nearestPoi.type == POIType.relic && poiStatus == SitePoiStatus.empty))
            {
                Game.log($"Confirming POI {poiStatus}: '{this.nearestPoi.name}' ({this.nearestPoi.type})");
                siteData.poiStatus[this.nearestPoi.name] = poiStatus;
                siteData.Save();
                this.Invalidate();
            }
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || this.IsDisposed) return;

            base.Status_StatusChanged(blink);

            if (game.status.Altitude > 4000)
            {
                Game.log("Too high, closing PlotGuardians");
                game.fireUpdate(true);
                Program.closePlotter<PlotGuardians>();
                Program.closePlotter<PlotGuardianStatus>();
            }

            // show Relic Tower aiming assistance when on foot and using this tool
            if (this.nearestPoi?.type == POIType.relic)
            {
                var isVerticalVisible = Program.isPlotter<PlotVertialStripe>();
                if (game.status.SelectedWeapon == "$humanoid_companalyser_name;")
                {
                    if (!isVerticalVisible)
                    {
                        PlotVertialStripe.mode = PlotVertialStripe.Mode.RelicTower;
                        Program.showPlotter<PlotVertialStripe>();
                    }
                }
                else if (isVerticalVisible)
                {
                    Program.closePlotter<PlotVertialStripe>();
                }
            }

            // if blink is detected and we're in the right mode
            if (blink && game.isMode(GameMode.InSrv, GameMode.InFighter, GameMode.Flying))
            {
                if (this.mode == Mode.siteType)
                {
                    // select site type
                    var newType = (SiteType)(game.status.FireGroup % 3) + 1;
                    Game.log($"Selecting site type from: '{siteData.type}' to: '{newType}'");
                    siteData.type = newType;
                    siteData.Save();

                    this.nextMode();
                }
                else if (this.mode == Mode.heading)
                {
                    // take current heading
                    this.setSiteHeading(game.status.Heading);
                }
                else if (this.nearestPoi != null && this.mode == Mode.map)
                {
                    confirmPOI(SitePoiStatus.unknown);
                }
            }

            if (blink && game.isMode(GameMode.OnFoot) && game.status.SelectedWeapon == "$humanoid_companalyser_name;")
            {
                var newAngle = game.status.Heading;
                if (newAngle < 0) newAngle += 360;
                if (this.siteData.isRuins)
                {
                    // ruins
                    Game.log($"Changing Relic Tower heading from: {siteData.relicTowerHeading}° to: {newAngle}");
                    siteData.relicTowerHeading = newAngle;
                    siteData.Save();
                }
                else if (this.nearestPoi != null)
                {
                    // structures
                    var oldHeading = siteData.relicHeadings.ContainsKey(this.nearestPoi.name) ? siteData.relicHeadings[this.nearestPoi.name] : -1;
                    Game.log($"Relic Tower heading from: {oldHeading}° to: {newAngle}");
                    siteData.relicHeadings[this.nearestPoi.name] = newAngle;
                    siteData.poiStatus[this.nearestPoi.name] = SitePoiStatus.present;
                    siteData.Save();
                }
            }

            // prepare other stuff
            if (template != null && siteData != null)
            {
                //var offset = game.nearBody!.guardianSiteLocation! - Status.here;
                //this.commanderOffset = new PointF(
                //    (float)(offset.Long * template.scaleFactor),
                //    (float)(offset.Lat * -template.scaleFactor));
                //Game.log($"commanderOffset old: {commanderOffset}");
                var td = new TrackingDelta(game.nearBody!.radius, siteData.location);
                var ss = 1f;
                this.commanderOffset = new PointF(
                    (float)td.dx * ss,
                    -(float)td.dy * ss);
                //Game.log($"commanderOffset: {commanderOffset}");


                /* TODO: get trail tacking working
                using (var gg = Graphics.FromImage(this.trails!))
                {
                    gg.ResetTransform();
                    //gg.FillRectangle(Brushes.Yellow, -10, -10, 20, 20);

                    // shift by underlay size
                    //gg.TranslateTransform(ux, uy);
                    //// rotate by site heading only
                    //gg.RotateTransform(+this.siteHeading);
                    //gg.ScaleTransform(this.template!.scaleFactor, this.template!.scaleFactor);


                    // shift by underlay size and rotate by site heading
                    //gg.TranslateTransform(this.trails!.Width / 2, this.trails.Height / 2);
                    gg.TranslateTransform(this.template.imageOffset.X, this.template.imageOffset.Y);
                    gg.RotateTransform(-this.siteHeading);

                    // draw the site bitmap and trails(?)
                    const float rSize = 5;
                    float x = -commanderOffset.X;
                    float y = -commanderOffset.Y;

                    var rect = new RectangleF(
                        x, y, //x - rSize, y - rSize,
                        rSize * 2, rSize * 2
                    );
                    Game.log($"smudge: {rect}");
                    gg.FillEllipse(Brushes.Navy, rect);
                }
                // */

                /*
                using (var g = Graphics.FromImage(this.plotSmudge))
                {
                    g.TranslateTransform(this.siteTemplate.imageOffset.X, this.siteTemplate.imageOffset.Y);

                    float rotation = (float)numSiteHeading.Value;
                    rotation = rotation % 360;
                    g.RotateTransform(-rotation);

                    PointF dSrv = (PointF)(this.pointSrv - this.survey.pointSettlement);
                    g.FillEllipse(Stationary.TireTracks,
                        dSrv.X - 5, dSrv.Y - 5, //  + srvFix.Y, 
                        10, 10);
                    //g.DrawLine(Pens.Pink, dSrv.X, dSrv.Y, 0, 0);

                }
                */
            }

            // update cmdr location relative to origin if editor is running
            if (formEditMap != null && this.siteData != null)
            {
                var td = new TrackingDelta(game.nearBody!.radius, this.siteData.location);
                formEditMap.txtDeltaLat.Text = td.dy.ToString("N2") + "m";
                formEditMap.txtDeltaLong.Text = td.dx.ToString("N2") + "m";
            }

            this.Invalidate();
        }
        private FileSystemWatcher watcher;

        private void devFileWatcher()
        {
            string filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "settlementTemplates.json");

            if (Debugger.IsAttached)
                filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, "..\\..\\..\\..\\", "settlementTemplates.json");

            Game.log($"Dev watching: {filepath}");


            this.watcher = new FileSystemWatcher(Path.GetFullPath(Path.GetDirectoryName(filepath)!), "settlementTemplates.json");
            this.watcher.Changed += Watcher_Changed;
            this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            this.watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Game.log("Reloading watched site template");
            Application.DoEvents(); Application.DoEvents(); Application.DoEvents(); Application.DoEvents();
            SiteTemplate.Import(true);
            this.loadSiteTemplate();
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.IsDisposed || game.isShutdown) return;

            //Game.log($"-- -- --> PlotGuardians: OnPaintBackground {this.template?.imageOffset} / {this.template?.scaleFactor}");
            if (this.template != null)
            {
                // alpha
                //this.template.scaleFactor = 0.88f;
                //this.template.imageOffset.X = 737;
                //this.template.imageOffset.Y = 885;


                //this.template.scaleFactor = 0.75f;
                //this.template.imageOffset.X = 730;
                //this.template.imageOffset.Y = 885;

                // beta
                //this.template.scaleFactor = 1.15f;
                //this.template.scaleFactor = 1.3f;

                //this.template.imageOffset.X = 652;
                //this.template.imageOffset.Y = 714;

                //this.template.imageOffset.X = 638;
                //this.template.imageOffset.Y = 752;

                //this.template.imageOffset.X = 644;
                //this.template.imageOffset.Y = 795;

                //this.template.imageOffset.X = 652;
                //this.template.imageOffset.Y = 714;

                // gamma
                //this.template.scaleFactor = 1.15f;

                //this.template.imageOffset.X = 915;
                //this.template.imageOffset.Y = 685;

                /*
    "imageOffset": "638,752",
    "imageOffset": "652,714",
                */
            }

            base.OnPaintBackground(e);
            this.g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            switch (this.mode)
            {
                case Mode.siteType:
                    this.drawSiteTypeHelper();
                    return;

                case Mode.heading:
                    this.drawSiteHeadingHelper();
                    return;

                case Mode.origin:
                    this.drawTrackOrigin();
                    return;

                case Mode.map:
                    this.drawSiteMap();
                    return;
            }
        }

        private void drawTrackOrigin()
        {
            g.ResetTransform();
            this.clipToMiddle();
            g.TranslateTransform(mid.Width, mid.Height);
            g.RotateTransform(360 - siteData.siteHeading);
            var sI = 10;
            var sO = 20;
            var vr = 5;

            // calculate deltas from site origin to ship
            var td = new TrackingDelta(game.nearBody!.radius, siteData.location!);
            var x = -(float)td.dx;
            var y = (float)td.dy;

            // adjust scale depending on distance from target
            var sf = 1f;
            if (td.distance < 40)
                sf = 3f;
            else if (td.distance < 70)
                sf = 2f;
            else if (td.distance > 170)
                sf = 0.5f;
            g.ScaleTransform(sf, sf);

            var siteBrush = GameColors.brushOffTarget;
            var sitePenI = GameColors.penGameOrangeDim2;
            var sitePenO = GameColors.penGameOrangeDim2;
            var shipPen = GameColors.penGreen2;
            if (td.distance < 10)
            {
                //siteBrush = GameColors.brushOnTarget;
                sitePenI = GameColors.penGameOrange2;
                shipPen = GameColors.penCyan2;
            }
            if (td.distance < 20)
            {
                siteBrush = new HatchBrush(HatchStyle.Percent50, GameColors.OrangeDim, Color.Transparent); // GameColors.brushOnTarget;
                sitePenO = GameColors.penGameOrange2;
            }

            // draw origin marker
            g.FillEllipse(siteBrush, -sO, -sO, +sO * 2, +sO * 2);
            g.DrawEllipse(sitePenO, -sO, -sO, +sO * 2, +sO * 2);
            g.DrawEllipse(sitePenI, -sI, -sI, +sI + sI, +sI + sI);

            // draw moving ship circle marker, showing ship heading
            var rect = new RectangleF(
                    x - vr, y - vr,
                    +vr * 2, +vr * 2);
            g.DrawEllipse(shipPen, rect);
            var dHeading = new Angle(game.status.Heading);
            //var dHeading = new Angle(game.status.Heading) - siteData.siteHeading;
            //var dHeading = new Angle(siteData.siteHeading) - game.status.Heading;
            var dx = (float)Math.Sin(dHeading.radians) * 10F;
            var dy = (float)Math.Cos(dHeading.radians) * 10F;
            g.DrawLine(shipPen, x, y, x + dx, y - dy);
            this.drawCompassLines(siteData.siteHeading);

            Point[] pps = { new Point((int)x, (int)y) };
            g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pps);

            g.ResetTransform();
            this.clipToMiddle(4, 26, 4, 24);
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(sf, sf);

            var pp = pps[0]; // new Point((int)pps[0].X, (int)pps[0].Y); // new Point((int)sz.Width, -(int)sz.Height);
            pp.Offset(-mid.Width, -mid.Height);

            var cs = 6; // cap size

            // vertical arrow
            if (Math.Abs(pp.Y) > 25)
            {
                var nn = pp.Y < 0 ? -1 : +1; // toggle up/down
                var z1 = new Size(0, 15 * -nn);
                var z2 = new Size(0, -pp.Y);
                var p1 = Point.Add(pp, z1);
                var p2 = Point.Add(pp, z2);

                g.DrawLine(GameColors.penCyan2, p1, p2);
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X - cs, p2.Y + (cs * nn));
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X + cs, p2.Y + (cs * nn));
            }

            // horizontal arrow
            if (Math.Abs(pp.X) > 25)
            {
                var nn = pp.X < 0 ? +1 : -1; ; // toggle left/right
                var z1 = new Size(15 * nn, 0);
                var z2 = new Size(-pp.X, 0);
                var p1 = Point.Add(pp, z1);
                var p2 = Point.Add(pp, z2);

                g.DrawLine(GameColors.penCyan2, p1, p2);
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X - (cs * nn), p2.Y - cs);
                g.DrawLine(GameColors.penCyan2, p2.X, p2.Y, p2.X - (cs * nn), p2.Y + cs);
            }

            var alt = game.status.Altitude.ToString("N0");
            var targetAlt = Util.targetAltitudeForSite(siteData.type);
            var footerTxt = $"Altitude: {alt}m | Target altitude: {targetAlt}m";

            // header text and rotation arrows
            this.drawOriginRotationGuide();

            // choose a font color: too low: blue, too high: red, otherwise orange
            Brush footerBrush;
            var altDiff = (int)game.status.Altitude - targetAlt;
            if (altDiff < -50)
                footerBrush = GameColors.brushCyan;
            else if (altDiff > 50)
                footerBrush = Brushes.Red;
            else
                footerBrush = GameColors.brushGameOrange;

            this.drawFooterText(footerTxt, footerBrush);
        }

        private void drawOriginRotationGuide()
        {
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);

            var adjustAngle = siteData.siteHeading - game.status.Heading;

            Brush brush = GameColors.brushGameOrange;
            if (Math.Abs(adjustAngle) > 2)
            {
                brush = GameColors.brushCyan;
                var ax = 30f;
                var ay = -mid.Height * 0.8f;
                var ad = 80;
                var ac = 10;
                if (adjustAngle < 0)
                {
                    ax *= -1;
                    ac *= -1;
                    ad *= -1;
                }
                var pp = GameColors.penCyan2;
                g.DrawLine(pp, ax, ay, ax + ad, ay);
                g.DrawLine(pp, ax + ad, ay, ax + ad - ac, ay - 10);
                g.DrawLine(pp, ax + ad, ay, ax + ad - ac, ay + 10);
            }

            var headerTxt = $"Site heading: {siteData.siteHeading}° | Rotate ship ";
            headerTxt += adjustAngle > 0 ? "right" : "left";
            headerTxt += $" {Math.Abs(adjustAngle)}°";

            this.drawHeaderText(headerTxt, brush);
        }

        private void drawSiteMap()
        {
            if (g == null) return;
            //this.drawHeaderText($"{siteData.nameLocalised} | {siteData.type} | {siteData.siteHeading}°");
            //this.drawFooterText($"{game.nearBody!.bodyName}");

            if (this.underlay == null)
            {
                Game.log("WHy no underlay?");
                return;
            }

            g.ResetTransform();
            this.clipToMiddle(8, 26, 8, 24);
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(-game.status.Heading);

            // prepare underlay image
            using (var gg = Graphics.FromImage(this.underlay))
            {
                gg.Clear(Color.Transparent);
                gg.ResetTransform();
                //gg.Clear(Color.Black);
                //gg.RotateTransform(360 - game.status!.Heading);

                // shift by underlay size
                gg.TranslateTransform(ux, uy);
                // rotate by site heading only
                gg.RotateTransform(+siteData.siteHeading);
                gg.ScaleTransform(this.template!.scaleFactor, this.template!.scaleFactor);

                // draw the site bitmap and trails(?)
                gg.DrawImageUnscaled(this.siteMap!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);

                //var ix = -(double)this.template!.imageOffset.X * (double)this.template!.scaleFactor;
                //var iy = -(double)this.template.imageOffset.Y * (double)this.template!.scaleFactor;
                //gg.DrawImageUnscaled(this.siteMap!, (int)ix, (int)iy); // -this.template!.imageOffset.X * this.template!.scaleFactor, -this.template.imageOffset.Y * this.template!.scaleFactor);
                //int xx = 0; // -this.trails!.Width;
                //int yy = 0; // -this.trails!.Height / 2;
                //gg.DrawImageUnscaled(this.trails!, -this.template!.imageOffset.X, -this.template.imageOffset.Y);

                //gg.ResetTransform();
                //gg.DrawRectangle(Pens.Red, 0, 0, underlay.Width, underlay.Height);
            }

            float x = commanderOffset.X;
            float y = commanderOffset.Y;

            g.DrawImageUnscaled(this.underlay!, -(int)(ux - x), -(int)(uy - y));

            // draw compass rose lines centered on underlay
            g.DrawLine(Pens.DarkRed, -this.Width * 2, y, +this.Width * 2, y);
            g.DrawLine(Pens.DarkRed, x, y, x, +this.Height * 2);
            g.DrawLine(Pens.Red, x, -this.Height * 2, x, y);
            g.ResetClip();

            this.drawTouchdownAndSrvLocation(true);

            this.drawArtifacts();

            this.drawCommander();

            //if (siteData.siteHeading != -1)
            //{
            //    //this.drawSiteSummaryFooter($"Site heading: {siteHeading}°");
            //    //var m = Util.getDistance(Status.here, siteData.location, (decimal)game.nearBody.radius);
            //    //var a = Util.getBearing(Status.here, siteData.location);
            //    //this.drawSiteSummaryFooter($"{Math.Round(m)}m / {Math.Round(a)}°");

            //    var ttd = new TrackingDelta(game.nearBody.radius, siteData.location);
            //    this.drawSiteSummaryFooter($"{ttd}");
            //}
        }

        private void drawArtifacts()
        {
            if (this.template == null || this.template.poi.Count == 0) return;

            // get pixel location of site origin relative to overlay window
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(-game.status.Heading);

            PointF[] pts = { new PointF(commanderOffset.X, commanderOffset.Y) };
            g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pts);
            var siteOrigin = pts[0];

            // reset transform with origin at that point, scaled and rotated to match the commander
            g.ResetTransform();
            this.clipToMiddle();
            g.TranslateTransform(siteOrigin.X, siteOrigin.Y);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(360 - game.status.Heading);

            var nearestDist = double.MaxValue;
            var nearestPt = PointF.Empty;

            int countRelics = 0, confirmedRelics = 0, countPuddles = 0, confirmedPuddles = 0;
            Angle aa;
            string tt;
            // and draw all the POIs
            foreach (var poi in this.template.poi)
            {
                if (poi.type == POIType.relic)
                {
                    countRelics++;
                    if (siteData.poiStatus.ContainsKey(poi.name))
                        confirmedRelics++;
                }
                else if (poi.type != POIType.obelisk && poi.type != POIType.brokeObelisk)
                {
                    countPuddles++;
                    if (siteData.poiStatus.ContainsKey(poi.name))
                        confirmedPuddles++;
                }

                // calculate render point for POI
                var deg = 180 - siteData.siteHeading - poi.angle;
                var dist = poi.dist * 1.01f; // scaling for perspective?
                var pt = Util.rotateLine(
                    deg,
                    dist);

                // render it
                this.drawSitePoi(poi, pt);

                // is this the closest POI?
                var x = pt.X - commanderOffset.X;
                var y = pt.Y - commanderOffset.Y;
                var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                //var foo = siteData.poiStatus.GetValueOrDefault(poi.name);
                //if (foo == SitePoiStatus.unknown)
                var selectPoi = d < nearestDist && poi.type != POIType.brokeObelisk;
                if (forcePoi != null)
                    selectPoi = forcePoi == poi; // force selection in map editor if present
                if (selectPoi) // && poi.type != POIType.pylon && poi.type != POIType.brokeObelisk && poi.type != POIType.component)
                {
                    aa = Util.ToAngle(x, y) - siteData.siteHeading; //  - game.status.Heading;
                    if (y < 0) aa += 180;
                    tt = $"{aa} | {x}, {y}";
                    nearestDist = d;
                    this.nearestPoi = poi;
                    nearestPt = pt;
                }
            }

            if (nearestDist > 75 && forcePoi == null)
            {
                // make sure we're relatively close before selecting the item
                this.nearestPoi = null!;
            }
            else
            {
                // draw highlight over closest POI
                // TODO: revisit?
                if ((nearestPoi == forcePoi) || (isRuinsPoi(nearestPoi.type)) || (siteData.activeObelisks.ContainsKey(nearestPoi.name)))
                    g.DrawEllipse(GameColors.penDarkCyan4, -nearestPt.X - 14, -nearestPt.Y - 14, 28, 28);

                var poiStatus = siteData.poiStatus.GetValueOrDefault(this.nearestPoi.name);

                var nextStatus = (SitePoiStatus)(game.status.FireGroup % 3) + 1;

                var nextStatusDifferent = nextStatus != siteData.poiStatus.GetValueOrDefault(nearestPoi.name);
                var action = nextStatusDifferent ? $"(set {nextStatus})" : "";

                if (this.nearestPoi.type == POIType.obelisk || this.nearestPoi.type == POIType.brokeObelisk)
                {
                    if (this.siteData.activeObelisks.ContainsKey(nearestPoi.name))
                    {
                        var txt = $"Obelisk {nearestPoi.name}";

                        var obelisk = this.siteData.activeObelisks[nearestPoi.name];
                        var items = "??";
                        if (obelisk.items != null)
                            items = string.Join(", ", obelisk.items).ToUpperInvariant();
                        var data = "?";
                        if (obelisk.data != null && obelisk.data.Count > 0)
                            data = string.Join(", ", obelisk.data).ToUpperInvariant();

                        txt += $": {items} for {data}";
                        if (!string.IsNullOrWhiteSpace(obelisk.msg))
                        {
                            string msg = "";
                            switch (obelisk.msg[0])
                            {
                                case 'C': msg = "Culture"; break;
                                case 'H': msg = "History"; break;
                                case 'B': msg = "Biology"; break;
                                case 'T': msg = "Technology"; break;
                            }
                            msg += " #" + obelisk.msg.Substring(1);
                            txt += $" ({msg})";
                        }

                        this.drawFooterText(txt , GameColors.brushCyan);
                    }
                    else
                    {
                        this.drawFooterText($"Obelisk {nearestPoi.name}", GameColors.brushGameOrange);
                    }
                }
                else
                {
                    var footerBrush = poiStatus == SitePoiStatus.unknown || nextStatusDifferent ? GameColors.brushCyan : GameColors.brushGameOrange;
                    this.drawFooterText($"{this.nearestPoi.type} {this.nearestPoi.name}: {poiStatus} {action}", footerBrush);
                }
            }

            var headerBrush = confirmedRelics + confirmedPuddles < countRelics + countPuddles
                ? GameColors.brushCyan
                : GameColors.brushGameOrange;

            this.drawHeaderText($"Confirmed: {confirmedRelics}/{countRelics} relics, {confirmedPuddles}/{countPuddles} items", headerBrush);

            g.ResetTransform();
            //var aa = new Angle(180 - game.status.Heading - nearestDeg);
            //this.drawBearingTo(0, this.Height - 20, tt, nearestDist, aa);

            forcePoi = null;
        }

        private bool isRuinsPoi(POIType poiType)
        {
            switch (poiType)
            {
                case POIType.casket:
                case POIType.orb:
                case POIType.relic:
                case POIType.tablet:
                case POIType.totem:
                case POIType.urn:
                    return true;

                default:
                    return false;
            }
        }

        private void drawSitePoi(SitePOI poi, PointF pt)
        {
            // render no POI if form editor says so
            if (formEditMap != null && formEditMap.checkHideAllPoi.Checked)
                return;

            var rot = poi.rot + this.siteData.siteHeading;

            // diameters: relics are bigger then puddles
            var d = poi.type == POIType.relic ? 16f : 10f;
            var dd = d / 2;

            if (formEditMap != null && formEditMap.checkHighlightAll.Checked)
            {
                // highlight everything if map editor check says so
                var b = new SolidBrush(Color.FromArgb(160, Color.DarkSlateBlue));
                //g.FillEllipse(b, -pt.X - dd - 2, -pt.Y - dd - 2, d + 4, d + 4);
                g.DrawEllipse(Pens.DarkViolet, -pt.X - dd - 2, -pt.Y - dd - 2, d + 4, d + 4);
                // DarkOliveGreen / DarkSlateBlue
            }

            // at structures - do not render missing POIs
            if (!this.siteData.isRuins && this.siteData.poiStatus.ContainsKey(poi.name) && this.siteData.poiStatus[poi.name] == SitePoiStatus.absent)
                return;

            if (!this.siteData.isRuins && !this.siteData.poiStatus.ContainsKey(poi.name) && poi.type != POIType.obelisk && poi.type != POIType.brokeObelisk)
            {
                // anything unknown gets a blue circle underneath
                var b = new SolidBrush(Color.FromArgb(160, Color.DarkSlateBlue));
                g.FillEllipse(b, -pt.X - dd - 5, -pt.Y - dd - 5, d + 10, d + 10);
                // DarkOliveGreen / DarkSlateBlue
            }

            // choose pen color
            var pen = this.getPoiPen(poi);
            if (poi.type == POIType.relic && !this.siteData.isRuins && this.siteData.relicHeadings.ContainsKey(poi.name))
                pen = GameColors.penPoiRelicPresent;

            // temporary highlight a particular POI
            var highlight = poi.name.StartsWith("?") || string.Compare(poi.name, this.highlightPoi, true) == 0;
            if (highlight)
            {
                pen = GameColors.penCyan8;
                //g.DrawLine(GameColors.penCyan2Dotted, 0, 0, -sz.Width, -sz.Height);
            }

            if (poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk)
            {
                //PointF[] points = {
                //    new PointF(0, - 3),
                //    new PointF(+ 4, + 2),
                //    new PointF( - 4, + 2),
                //    new PointF(0, - 3),
                //};
                var l = 4f;
                float s32 = (float)Math.Sqrt(3 / 2);
                //PointF[] points = {
                //    new PointF(l, 0),
                //    new PointF(-l/2f, l * s32),
                //    new PointF(-l/2f, -l * s32),
                //    new PointF(l, 0),
                //};
                //PointF[] points = {
                //    //new PointF(1-1.5f, 4-1.5f),
                //    //new PointF(0-1.5f, 0-1.5f),
                //    //new PointF(4-1.5f, 1-1.5f),
                //    //new PointF(1-1.5f, 4-1.5f),
                //    //new PointF(.5f, -1.5f),
                //    new PointF(2-3f, 8-3f),
                //    new PointF(0-3f, 0-3f),
                //    new PointF(8-3f, 2-3f),
                //    new PointF(2-3f, 8-3f),
                //    new PointF(1f, -3f),

                //};
                var pp = new Pen(Color.SandyBrown)
                {
                    Width = 1,
                    LineJoin = LineJoin.Bevel,
                    StartCap = LineCap.Triangle,
                    EndCap = LineCap.Triangle,
                };
                if (this.siteData.activeObelisks.ContainsKey(poi.name))
                {
                    pp.Color = Color.LightCyan;
                    pp.Width = 1;
                }

                rot += 150;
                g.TranslateTransform(-pt.X, -pt.Y);
                g.RotateTransform(rot);
                //var points = new PointF[] {
                //    new PointF(2-3f, 8-3f),
                //    new PointF(0-3f, 0-3f),
                //    new PointF(8-3f, 2-3f),
                //    new PointF(2-3f, 8-3f),
                //};
                //g.FillPolygon(Brushes.DarkSlateBlue, points);

                var points = poi.type == POIType.obelisk
                    ? new PointF[] {
                        new PointF(1 - 1.5f, 3.5f - 1.5f),
                        new PointF(0 - 1.5f, 0 - 1.5f),
                        new PointF(4 - 1.5f, 1 - 1.5f),
                        new PointF(1 - 1.5f, 3.5f - 1.5f),
                        new PointF(.5f, -1.5f),
                    }
                    : new PointF[] {
                        new PointF(1 - 1.5f, 4 - 1.5f),
                        new PointF(0 - 1.5f, 0 - 1.5f),
                        new PointF(4 - 1.5f, 1 - 1.5f),
                        new PointF(1 - 1.5f, 4 - 1.5f),
                    };

                g.DrawLines(pp, points);

                if (poi.type == POIType.obelisk)
                {
                    //new PointF(0 - 1.5f, 0 - 1.5f),
                }

                g.RotateTransform(-rot);
                g.TranslateTransform(+pt.X, +pt.Y);
            }
            else if (poi.type == POIType.pylon)
            {
                PointF[] points = {
                    new PointF(0, -3),
                    new PointF(+6, 0),
                    new PointF(0, +3),
                    new PointF(-6, 0),
                    new PointF(0, -3),
                };
                var pp = new Pen(Color.DodgerBlue) // SkyBlue ?
                {
                    Width = 2,
                    LineJoin = LineJoin.Bevel,
                    StartCap = LineCap.Triangle,
                    EndCap = LineCap.Triangle,
                };

                if (!this.siteData.isRuins && !this.siteData.poiStatus.ContainsKey(poi.name))
                    pp.Color = GameColors.Cyan;

                g.TranslateTransform(-pt.X, -pt.Y);
                g.RotateTransform(+rot);

                g.DrawLines(pp, points);
                g.DrawLine(pp, 0, 0, 0, 3);

                g.RotateTransform(-rot);
                g.TranslateTransform(+pt.X, +pt.Y);
            }
            else if (poi.type == POIType.component)
            {
                PointF[] points = {
                    new PointF(0, +1),
                    new PointF(-2, -2),
                    new PointF(+2, -2),
                    new PointF(0, +1),
                    new PointF(0, +4),
                    new PointF(-5, -4),
                    new PointF(+5, -4),
                    new PointF(0, +4),
                };
                var pp = new Pen(Color.Lime)
                {
                    Width = 1,
                    DashStyle = DashStyle.Dash,
                    LineJoin = LineJoin.Bevel,
                    StartCap = LineCap.Triangle,
                    EndCap = LineCap.Triangle,
                };

                if (!this.siteData.isRuins && !this.siteData.poiStatus.ContainsKey(poi.name))
                    pp.Color = GameColors.Cyan;

                rot -= 45;
                g.TranslateTransform(-pt.X, -pt.Y);
                g.RotateTransform(+rot);

                g.DrawLines(pp, points);

                g.RotateTransform(-rot);
                g.TranslateTransform(+pt.X, +pt.Y);
            }
            else if (siteData.isRuins)
            {
                // relic towers and items at ruins
                g.DrawEllipse(pen, -pt.X - dd, -pt.Y - dd, d, d);
            }
            else if (poi.type == POIType.relic)
            {
                PointF[] points = {
                    new PointF(0, -4),
                    new PointF(-5, +4),
                    new PointF(+5, +4),
                    new PointF(0, -4),
                };
                var pp = new Pen(Color.DarkBlue)
                {
                    Width = 2,
                    LineJoin = LineJoin.Bevel,
                    StartCap = LineCap.Triangle,
                    EndCap = LineCap.Triangle,
                };

                if (!this.siteData.poiStatus.ContainsKey(poi.name))
                    pp.Color = GameColors.Cyan;

                rot = 0; // + game.status.Heading;
                g.TranslateTransform(-pt.X, -pt.Y);
                g.RotateTransform(+rot);

                g.FillEllipse(Brushes.DarkCyan, -7, -6, 14, 14);
                g.DrawLines(pp, points);
                //g.DrawLine(pp, 0, 0, 0, -30);

                g.RotateTransform(-rot);
                g.TranslateTransform(+pt.X, +pt.Y);
            }
            else
            {
                //if (!this.siteData.poiStatus.ContainsKey(poi.name))
                //    pen = GameColors.
                //if (poi.type == POIType.relic && this.siteData.poiStatus.ContainsKey(poi.name))
                //    pen = GameColors.penGameOrangeDim2;
                //else 
                if (poi.type != POIType.relic && this.siteData.poiStatus.ContainsKey(poi.name))
                    pen = this.siteData.poiStatus.ContainsKey(poi.name)
                    ? GameColors.penGameOrangeDim2
                    : Pens.LightCoral;

                g.DrawEllipse(pen, -pt.X - dd, -pt.Y - dd, d, d);
            }
        }

        private Pen getPoiPen(SitePOI poi)
        {
            SitePoiStatus status = SitePoiStatus.unknown;
            if (siteData.poiStatus.ContainsKey(poi.name))
                status = siteData.poiStatus[poi.name];

            switch (poi.type)
            {
                case POIType.relic:
                    return status == SitePoiStatus.present
                        ? GameColors.penPoiRelicPresent
                        : status == SitePoiStatus.absent
                            ? GameColors.penPoiRelicMissing
                            : GameColors.penPoiRelicUnconfirmed;

                case POIType.orb:
                case POIType.casket:
                case POIType.tablet:
                case POIType.totem:
                case POIType.urn:
                    switch (status)
                    {
                        case SitePoiStatus.present: return GameColors.penPoiPuddlePresent;
                        case SitePoiStatus.absent: return GameColors.penPoiPuddleMissing;
                        case SitePoiStatus.unknown: return GameColors.penPoiPuddleUnconfirmed;
                        case SitePoiStatus.empty: return GameColors.penYellow4;
                        default: return Pens.Azure;
                    }

                default:
                    return Pens.Red;
            }
        }

        private void drawArtifactsOLD()
        {
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(-game.status.Heading);
            float x = commanderOffset.X;
            float y = commanderOffset.Y;

            //var pp = new PointF(
            //    commanderOffset.X,// / this.scale,
            //    commanderOffset.Y// / this.scale
            //);
            PointF[] pps = { new PointF(commanderOffset.X, commanderOffset.Y) };
            g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pps);
            var pp = pps[0];

            //var td = new TrackingDelta(game.nearBody.radius, siteData.location);

            //pp.X *= this.scale;
            //pp.Y *= this.scale;

            //this.drawHeaderText($"?d? {pp}");

            // ** ** ** **
            g.ResetTransform();
            g.TranslateTransform(pp.X, pp.Y);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(360 - game.status.Heading);

            //* BETA

            //// Synuefe TP-F b44-0 CD 1-ruins-2:
            //this.drawSitePoiOLD(151, 120, "casket");
            //this.drawSitePoiOLD(134f, 196, "tablet");
            //this.drawSitePoiOLD(122, 534, "totem");
            //this.drawSitePoiOLD(125.6f, 606, "urn");
            //this.drawSitePoiOLD(104f, 421, "tablet");
            //this.drawSitePoiOLD(90.6f, 241, "casket");
            //this.drawSitePoiOLD(141.4f, 538, "urn");
            //this.drawSitePoiOLD(111.9f, 560, "urn");
            //this.drawSitePoiOLD(26.5f, 367, "casket");
            //this.drawSitePoiOLD(5.5f, 549, "orb");
            //this.drawSitePoiOLD(357.8f, 496, "urn");
            //this.drawSitePoiOLD(286.7f, 285, "urn");
            //this.drawSitePoiOLD(258.1f, 544, "totem");
            //this.drawSitePoiOLD(193.3f, 226, "casket");
            //this.drawSitePoiOLD(181.7f, 499, "tablet");
            //this.drawSitePoiOLD(298.7f, 625, "urn");

            //// Synuefe LY-I b42-2 C 2-ruins-3:
            //this.drawSitePoiOLD(215.4f, 565, "totem");
            //this.drawSitePoiOLD(200.7f, 485, "orb");
            //this.drawSitePoiOLD(208.1f, 415, "tablet");
            //this.drawSitePoiOLD(179.3f, 394, "casket");
            //this.drawSitePoiOLD(169.2f, 276, "orb");
            //this.drawSitePoiOLD(337f, 126, "orb");
            //this.drawSitePoiOLD(300f, 183, "totem");

            //// Col 173 Sector PF-E b28-3 B 1, Ruins #2
            //this.drawSitePoiOLD(214.7f, 258, "totem");
            //this.drawSitePoiOLD(230.8f, 506, "casket");
            //this.drawSitePoiOLD(193.7f, 617, "urn");
            //this.drawSitePoiOLD(276.8f, 436, "urn");


            // relics ...

            //// Synuefe TP-F b44-0 CD 1-ruins-2:
            //this.drawSitePoiOLD(80.8f, 408, "relic");
            //this.drawSitePoiOLD(236.8f, 612, "relic"); // leaning
            //this.drawSitePoiOLD(151, 358, "relic");
            //this.drawSitePoiOLD(35.5f, 515, "relic");
            //this.drawSitePoiOLD(292.3f, 413, "relic"); // leaning
            //// this.drawSitePoi(292, 378, "relic"); // ? ish

            //// Synuefe LY-I b42-2 C 2-ruins-3:
            //this.drawSitePoiOLD(196.3f, 378, "relic");
            //this.drawSitePoiOLD(328.8f, 358, "relic");

            //// Col 173 Sector PF-E b28-3 B 1, Ruins #2
            //this.drawSitePoiOLD(253.2f, 390, "relic");

            // */

            /* ALPHA 

            // relics ...
            this.scale = 2f;
            // 
            this.drawSitePoi(32f, 80, "relic");
            this.drawSitePoi(160f, 250, "relic");
            this.drawSitePoi(176f, 147, "relic");
            this.drawSitePoi(351.6f, 502, "relic");

            // */



            // -- -- -- --
            //this.drawFooterText($"?c? {(int)pp.X},{(int)pp.Y}");
            //this.drawHeaderText($"?d? {commanderOffset}");

        }

        private void drawSitePoiOLD(float angle, float dist, string type)
        {
            var d = type == "relic" ? 16f : 10f;
            var dd = d / 2;

            var p = type == "relic"
                ? new Pen(Color.CornflowerBlue, 4) { DashStyle = DashStyle.Dash, }
                : new Pen(Color.PeachPuff, 2) { DashStyle = DashStyle.Dot, };

            var sz = Util.rotateLine(
                180 - siteData.siteHeading - angle,
                dist);
            g.DrawEllipse(p, -sz.X - dd, -sz.Y - dd, d, d);
        }

        private void drawSiteSummaryFooter(string msg)
        {
            if (g == null) return;

            // draw heading text (center bottom)
            g.ResetTransform();
            g.ResetClip();
            var sz = g.MeasureString(msg, Game.settings.fontSmall);
            var tx = mid.Width - (sz.Width / 2);
            var ty = this.Height - sz.Height - 6;
            g.DrawString(msg, Game.settings.fontSmall, GameColors.brushGameOrange, tx, ty);
        }

        private void drawSiteTypeHelper()
        {
            if (g == null) return;

            this.drawHeaderText($"{siteData.nameLocalised} | {siteData.type} | ???°");
            this.drawFooterText($"{game.nearBody!.bodyName}");
            g.ResetTransform();
            this.clipToMiddle();

            string msg;
            SizeF sz;
            var tx = 10f;
            var ty = 20f;

            this.drawFooterText($"{game.nearBody!.bodyName}");

            // if we don't know the site type yet ...
            msg = $"\r\nSelect site type with \r\nfire group or send\r\nmessage:\r\n\r\n 'a' for Alpha\r\n\r\n 'b' for Beta\r\n\r\n 'g' for Gamma";
            sz = g.MeasureString(msg, Game.settings.font1, this.Width);
            g.DrawString(msg, Game.settings.font1, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);
        }

        private void drawSiteHeadingHelper()
        {
            if (g == null) return;

            this.drawHeaderText($"{siteData.nameLocalised} | {siteData.type} | ???°");
            this.drawFooterText($"{game.nearBody!.bodyName}");
            g.ResetTransform();
            this.clipToMiddle();

            string msg;
            var tx = 10f;
            var ty = 20f;

            var isRuins = siteData.isRuins;
            msg = $"Need site heading\r\n\r\n■ To use current heading either:\r\n    - Toggle Cargo scoop twice\r\n    - Send message:   .heading\r\n\r\n■ Or send message: <degrees>";
            if (isRuins)
                msg += $"\r\n\r\nAlign with this buttress:";
            var sz = g.MeasureString(msg, Game.settings.fontMiddle, this.Width);

            g.DrawString(msg, Game.settings.fontMiddle, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);

            // show location of helpful buttress
            if (isRuins && this.headingGuidance != null)
                g.DrawImage(this.headingGuidance, 40, 20 + sz.Height); //, 200, 200);
        }

        #region static accessing stuff

        public static PlotGuardians? instance;

        public static void switchMode(Mode newMode)
        {
            if (PlotGuardians.instance != null)
            {
                PlotGuardians.instance.setMode(newMode);
                Elite.setFocusED();
                if (Game.activeGame != null)
                    Game.activeGame.fireUpdate(false);
            }
        }

        #endregion
    }
}
