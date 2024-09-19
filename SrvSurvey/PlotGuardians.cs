using DecimalMath;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using static SrvSurvey.game.GuardianSiteData;

namespace SrvSurvey
{
    public enum Mode
    {
        /// <summary> ask about site type </summary>
        siteType,
        /// <summary> ask about site heading </summary>
        heading,
        /// <summary> show site map </summary>
        map,
        /// <summary> aerial assist, show alignment to site origin </summary>
        origin,
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
        private double nearestObeliskDist = 1000d;
        public string? targetObelisk;

        public Mode mode;
        private PointF commanderOffset;

        private GuardianSiteData siteData { get => game?.systemSite!; }

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
                case 0: this.Width = scaled(300); this.Height = scaled(400); break;
                case 1: this.Width = scaled(500); this.Height = scaled(500); break;
                case 2: this.Width = scaled(600); this.Height = scaled(700); break;
                case 3: this.Width = scaled(800); this.Height = scaled(1000); break;
                case 4: this.Width = scaled(1200); this.Height = scaled(1200); break;
            }

            this.setMapScale();
            this.nextMode();
        }

        public override bool allow { get => PlotGuardians.allowPlotter; }

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
            Program.closePlotter<PlotGuardianStatus>();
        }

        public static bool allowPlotter
        {
            get => Game.settings.enableGuardianSites
                && Game.activeGame?.systemBody != null
                && !Game.activeGame.hidePlottersFromCombatSuits
                && Game.activeGame.status?.hasLatLong == true
                && Game.activeGame.systemSite?.location != null
                && !Game.activeGame.status.FsdChargingJump
                && Game.activeGame.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed, GameMode.Flying, GameMode.InFighter, GameMode.CommsPanel, GameMode.RolePanel)
                ;
        }

        private void PlotGuardians_Load(object sender, EventArgs e)
        {
            // if these are open - close them
            Program.closePlotter<PlotBioStatus>();
            Program.closePlotter<PlotGrounded>();
            Program.closePlotter<PlotPriorScans>();
            Program.closePlotter<PlotTrackers>();

            this.initializeOnLoad();
            this.siteData.loadPub();

            this.nextMode();
            this.reposition(Elite.getWindowRect(true));
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
                    PlotVertialStripe.show(PlotVertialStripe.Mode.Buttress, 20);
                else
                    Program.closePlotter<PlotVertialStripe>();
            }

            // load heading guidance image if that is the next mode
            if (this.mode == Mode.heading)
                this.loadHeadingGuidance();

            if (this.mode == Mode.origin)
            {
                // show aiming assistance suitable for this site type
                showAimingBySiteType();
            }
            else if (this.mode != Mode.heading)
            {
                // close potential plotter
                Program.closePlotter<PlotVertialStripe>();
            }
        }

        private PlotVertialStripe? showAimingBySiteType()
        {
            // close existing
            Program.closePlotter<PlotVertialStripe>();
            if (Game.settings.disableAerialAlignmentGrid) return null;

            if (game.vehicle == ActiveVehicle.SRV)
            {
                Game.log("showAiming: not in SRV");
                return null;
            }

            // assume onFoot only means Relic Towers
            if (game.vehicle == ActiveVehicle.Foot)
            {
                // TODO: does this ever happen?
                Debugger.Break();
                return PlotVertialStripe.show(PlotVertialStripe.Mode.RelicTower, 0);
            }

            switch (this.siteData.type)
            {
                case SiteType.Alpha: return PlotVertialStripe.show(PlotVertialStripe.Mode.Alpha, Game.settings.aerialAltAlpha);
                case SiteType.Beta: return PlotVertialStripe.show(PlotVertialStripe.Mode.Beta, Game.settings.aerialAltBeta);
                case SiteType.Gamma: return PlotVertialStripe.show(PlotVertialStripe.Mode.Gamma, Game.settings.aerialAltGamma);

                case SiteType.Bear: return PlotVertialStripe.show(PlotVertialStripe.Mode.Bear, 650);
                case SiteType.Bowl: return PlotVertialStripe.show(PlotVertialStripe.Mode.Bowl, 650);
                case SiteType.Crossroads: return PlotVertialStripe.show(PlotVertialStripe.Mode.Crossroads, 650);
                case SiteType.Fistbump: return PlotVertialStripe.show(PlotVertialStripe.Mode.Fistbump, 450);
                case SiteType.Hammerbot: return PlotVertialStripe.show(PlotVertialStripe.Mode.Hammerbot, 650);
                case SiteType.Lacrosse: return PlotVertialStripe.show(PlotVertialStripe.Mode.Lacrosse, 650);
                case SiteType.Robolobster: return PlotVertialStripe.show(PlotVertialStripe.Mode.Robolobster, 1500);
                case SiteType.Squid: return PlotVertialStripe.show(PlotVertialStripe.Mode.Squid, 650);
                case SiteType.Stickyhand: return PlotVertialStripe.show(PlotVertialStripe.Mode.Stickyhand, 650);
                case SiteType.Turtle: return PlotVertialStripe.show(PlotVertialStripe.Mode.Turtle, 650);
            }

            // TODO: does this ever happen?
            Debugger.Break();
            Game.log($"showAiming: {PlotVertialStripe.mode}");
            return Program.showPlotter<PlotVertialStripe>();
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
            else if (msg == ".alphaflip")
            {
                newHeading = siteData.siteHeading + 180;
                if (newHeading > 360) newHeading -= 360;
                changeHeading = true;
                this.mode = Mode.map;
            }

            if (changeHeading)
            {
                this.setSiteHeading(newHeading);
                return;
            }

            // set Relic Tower heading
            if (msg == MsgCmd.tower && siteData.isRuins)
            {
                var newAngle = new Angle(game.status.Heading);
                Game.log($"Changing Relic Tower heading from: {siteData.relicTowerHeading}° to: {newAngle}");
                siteData.relicTowerHeading = newAngle;
                siteData.Save();
                Program.closePlotter<PlotVertialStripe>();
            }
            else if (msg.StartsWith(MsgCmd.tower) && int.TryParse(msg.Substring(MsgCmd.tower.Length), out newHeading) && this.nearestPoi?.type == POIType.relic)
            {
                var newAngle = new Angle(newHeading);

                // structures or ruins
                var oldHeading = siteData.getRelicHeading(this.nearestPoi.name) ?? -1;
                if (template!.relicTowerNames.Contains(this.nearestPoi.name))
                {
                    Game.log($"Relic Tower '{this.nearestPoi.name}' heading from: {oldHeading}° to: {newAngle}°");
                    siteData.relicHeadings[this.nearestPoi.name] = newAngle;
                    siteData.poiStatus[this.nearestPoi.name] = SitePoiStatus.present;
                    siteData.Save();
                }
                else
                {
                    // raw Poi's...
                    var match = siteData.rawPoi?.FirstOrDefault(_ => _.name == this.nearestPoi.name);
                    if (match != null)
                    {
                        Game.log($"Raw Relic Tower '{this.nearestPoi.name}' heading from: {oldHeading}° to: {newAngle}°");
                        match.rot = newAngle;
                        siteData.Save();
                    }

                }
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
            if (msg == MsgCmd.os && this.nearestPoi?.type == POIType.obelisk)
            {
                siteData.toggleObeliskScanned();
            }

            // target a specific obelisk
            if (msg.StartsWith(MsgCmd.to))
            {
                this.setTargetObelisk(msg.Substring(MsgCmd.to.Length).Trim().ToUpperInvariant());
            }

            // temporary stuff after here
            this.xtraCmds(msg);

            if (msg.StartsWith(MsgCmd.@new) && Debugger.IsAttached)
            {
                // eg: .new totem
                this.addNewPoi(msg.Substring(4).Trim().ToLowerInvariant(), true);
            }
            else if (msg.StartsWith(MsgCmd.add))
            {
                // eg: .new totem
                this.addNewPoi(msg.Substring(4).Trim().ToLowerInvariant(), false);
            }
            else if (msg.StartsWith(MsgCmd.remove) && siteData.rawPoi != null && this.nearestPoi?.name.StartsWith('x') == true)
            {
                // eg: .remove
                var name = this.nearestPoi?.name;
                var match = siteData.rawPoi?.FirstOrDefault(_ => _.name == name);
                if (match != null && siteData.rawPoi != null)
                {
                    siteData.rawPoi.Remove(match);
                    this.siteData.Save();
                    this.Invalidate();
                }
            }
        }

        private void addNewPoi(string msg, bool addToMaster)
        {
            POIType poiType;
            if (!Enum.TryParse<POIType>(msg, true, out poiType))
            {
                Game.log($"Bad new poi: '{msg}'");
                return;
            }

            var dist = Util.getDistance(Status.here, siteData.location, game.systemBody!.radius);
            var angle = ((decimal)new Angle((Util.getBearing(Status.here, siteData.location) - (decimal)siteData.siteHeading)));
            var rot = (decimal)new Angle(game.status.Heading - this.siteData.siteHeading);

            // do not allow new POI that are too close to existing ones
            var match = template!.findPoiAtLocation(angle, dist, poiType, true, siteData.rawPoi);
            if (match != null)
            {
                Game.log($"New POI is too close to existing POI: {match}");
                return;
            }

            var newPoi = new SitePOI()
            {
                // name is populated below
                dist = dist,
                angle = angle,
                type = poiType,
                rot = rot,
            };

            if (addToMaster)
            {
                // add to the master template and ListView
                newPoi.name = template.nextNameForNewPoi(poiType);
                Game.log($"Added new {poiType} named '{newPoi.name}' as present: {newPoi}");
                template.poi.Add(newPoi);
                siteData.poiStatus[newPoi.name] = SitePoiStatus.present;
                SiteTemplate.SaveEdits();
                this.siteData.Save();
                this.Invalidate();
            }
            else
            {
                if (siteData.rawPoi == null) siteData.rawPoi = new List<SitePOI>();

                // add to the master template and ListView
                newPoi.name = $"x{siteData.rawPoi.Count + 1}";
                if (newPoi.type == POIType.relic) newPoi.rot = -1;
                Game.log($"Adding new raw {poiType} named '{newPoi.name}' as present: {newPoi}");
                siteData.rawPoi.Add(newPoi);
                this.siteData.Save();
                this.Invalidate();
            }

            FormRuins.activeForm?.Invalidate(true);
        }

        private string getPoiPrefix(POIType poiType)
        {
            switch (poiType)
            {
                case POIType.relic:
                    return "t";
                case POIType.casket:
                case POIType.orb:
                case POIType.tablet:
                case POIType.totem:
                case POIType.urn:
                case POIType.unknown:
                    return "p";

                case POIType.pylon:
                    return "py";

                case POIType.component:
                    return "c";

                default:
                    throw new Exception($"Unexpected poiType: '{poiType}'");
            }
        }

        public void setTargetObelisk(string? target)
        {
            // stop if the target is not known
            var obelisk = siteData.getActiveObelisk(target);
            if (string.IsNullOrEmpty(target) || obelisk == null)
            {
                Game.log($"Clearing target obelisk");
                this.targetObelisk = null;
            }
            else
            {
                Game.log($"Set target obelisk: '{target}'");
                this.targetObelisk = target;
            }
            this.Invalidate();
            Program.getPlotter<PlotRamTah>()?.Invalidate();
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
                var dist = ((double)Util.getDistance(Status.here, siteData.location, (decimal)game.systemBody!.radius)).ToString("N1");
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

        protected override void onJournalEntry(CodexEntry entry)
        {
            // exit if we have no nearest POI
            if (this.nearestPoi == null) return;

            if (this.nearestPoi.type == POIType.relic)
            {
                // if it's a relic tower - add POI to confirmed
                // use combat/exploration mode to know if item is present or missing
                Game.log($"POI confirmed: '{this.nearestPoi.name}' ({this.nearestPoi.type})");
                siteData.poiStatus[this.nearestPoi.name] = SitePoiStatus.present;
                siteData.Save();
                this.Invalidate();
            }
        }

        protected override void onJournalEntry(MaterialCollected entry)
        {
            if (this.nearestPoi?.type != POIType.obelisk)
            {
                Game.log($"No obelisk is known by '{this.nearestPoi?.name}'!");
                return;
            }
            var obelisk = siteData.getActiveObelisk(this.nearestPoi.name, true)!;
            Game.log($"Marking active obelisk '{this.nearestPoi.name}' as scanned, yielding: {entry.Name_Localised} ({entry.Name})");

            siteData.setObeliskScanned(obelisk, true);

            siteData.Save();
            Program.getPlotter<PlotGuardianStatus>()?.Invalidate();
            this.Invalidate();
        }

        protected override void onJournalEntry(Embark entry)
        {
            if (entry.SRV && this.srvLocation0 != null)
            {
                this.srvLocation0 = null;
                this.Invalidate();
            }


            this.setMapScale();
        }

        protected override void onJournalEntry(Disembark entry)
        {
            if (entry.SRV && this.srvLocation0 == null)
            {
                this.srvLocation0 = new TrackingDelta(
                    game.systemBody!.radius,
                    Status.here.clone());
                this.Invalidate();
            }

            this.scale = 2f;
            this.Invalidate();
        }

        private void setMapScale()
        {
            var newScale = this.scale;
            if (this.customScale != -1f)
                newScale = this.customScale;
            else if (this.nearestObeliskDist < 30 && (game.vehicle == ActiveVehicle.SRV || game.vehicle == ActiveVehicle.Foot))
                newScale = 3f;
            else if (game.vehicle == ActiveVehicle.Foot)
                newScale = 2f;
            else
                newScale = this.siteData.isRuins ? 0.65f : 1.5f;

            if (newScale != this.scale)
            {
                this.scale = newScale;
                this.Invalidate();
            }
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

            var filepath = imagePath ?? Path.Combine(Application.StartupPath, "images", $"{siteData.type}-background.png".ToLowerInvariant());
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

            var filepath = Path.Combine(Application.StartupPath, "images", $"{siteData.type}-heading-guide.png");
            if (File.Exists(filepath))
            {
                using (var img = Bitmap.FromFile(filepath))
                    this.headingGuidance = new Bitmap(img);
            }
            else if (siteData.name.StartsWith("$Ancient_Medium") || siteData.name.StartsWith("$Ancient_Small"))
            {
                filepath = Path.Combine(Application.StartupPath, "images", $"data-port-heading-guide.png");
                using (var img = Bitmap.FromFile(filepath))
                    this.headingGuidance = new Bitmap(img);
            }
        }

        protected void confirmPOI(SitePoiStatus poiStatus)
        {
            // it must be from rawPoi if not known to the template - ignore it
            if (game.systemData == null) return;
            if (!template!.poi.Any(_ => _.name == this.nearestPoi.name)) return;
            // and ignore obelisks
            if (this.nearestPoi.type == POIType.obelisk || this.nearestPoi.type == POIType.brokeObelisk || this.nearestPoi.type == POIType.emptyPuddle) return;

            if (poiStatus == SitePoiStatus.unknown)
            {
                // interpret POI is missing/present/empty by fire groups
                poiStatus = (SitePoiStatus)(game.status.FireGroup % 3) + 1;
            }

            // (don't let Relic's get a status of Empty)
            if (nearestPoi.type == POIType.relic && poiStatus == SitePoiStatus.empty) return;

            Game.log($"Confirming POI {poiStatus}: '{this.nearestPoi.name}' ({this.nearestPoi.type})");
            siteData.poiStatus[this.nearestPoi.name] = poiStatus;
            siteData.Save();
            this.Invalidate();
            // update GuardianSystemStatus entry
            game.systemData.prepSettlements();
            /* TODO: test this when next at Guardian sites...
            for(var n=0; n < game.systemData.settlements.Count; n++)
            {
                var entry = game.systemData.settlements[n];
                if (entry.body == game.systemBody && entry.name == this.siteData.name)
                {
                    if (siteData.isRuins)
                        game.systemData.settlements[n] = SystemSettlementSummary.forRuins(game.systemData, game.systemBody, siteData.index);
                    else
                        game.systemData.settlements[n] = SystemSettlementSummary.forStructure(game.systemData, game.systemBody);

                    break;
                }
            }
            */
        }

        protected override void Status_StatusChanged(bool blink)
        {
            if (this.IsDisposed || game?.status == null || game.systemBody == null) return;

            base.Status_StatusChanged(blink);

            if (game.systemSite == null)
            {
                Game.log("Too far, closing PlotGuardians");
                game.fireUpdate(true);
                Program.closePlotter<PlotGuardians>();
                Program.closePlotter<PlotGuardianStatus>();
                return;
            }

            // show Relic Tower aiming assistance when on foot and using this tool
            if (this.nearestPoi?.type == POIType.relic)
            {
                var isVerticalVisible = Program.isPlotter<PlotVertialStripe>();
                if (game.status.SelectedWeapon == "$humanoid_companalyser_name;")
                {
                    if (!isVerticalVisible)
                        PlotVertialStripe.show(PlotVertialStripe.Mode.RelicTower, 0);
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
                else if (this.nearestPoi != null && this.mode == Mode.map && this.nearestPoi.type == POIType.obelisk)
                {
                    // toggle obelisk scanned if we're next to one
                    siteData.toggleObeliskScanned();
                }
                else if (this.nearestPoi != null && this.mode == Mode.map && this.nearestPoi.type != POIType.obelisk && this.nearestPoi.type != POIType.brokeObelisk)
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
                    // ruins only
                    Game.log($"Changing Relic Tower heading from: {siteData.relicTowerHeading}° to: {newAngle}°");
                    siteData.relicTowerHeading = newAngle;
                    siteData.Save();
                }
                if (this.nearestPoi != null)
                {
                    // structures or ruins
                    var oldHeading = siteData.getRelicHeading(this.nearestPoi.name) ?? -1;
                    if (template!.relicTowerNames.Contains(this.nearestPoi.name))
                    {
                        Game.log($"Relic Tower '{this.nearestPoi.name}' heading from: {oldHeading}° to: {newAngle}°");
                        siteData.relicHeadings[this.nearestPoi.name] = newAngle;
                        siteData.poiStatus[this.nearestPoi.name] = SitePoiStatus.present;
                        siteData.Save();
                    }
                    else
                    {
                        // raw Poi's...
                        var match = siteData.rawPoi?.FirstOrDefault(_ => _.name == this.nearestPoi.name);
                        if (match != null)
                        {
                            Game.log($"Raw Relic Tower '{this.nearestPoi.name}' heading from: {oldHeading}° to: {newAngle}°");
                            match.rot = newAngle;
                            siteData.Save();
                        }
                    }
                }
            }

            // adjust the zoom?
            if (this.formEditMap == null)
                this.setMapScale();

            // prepare other stuff
            if (template != null && siteData != null)
            {
                //var offset = game.nearBody!.guardianSiteLocation! - Status.here;
                //this.commanderOffset = new PointF(
                //    (float)(offset.Long * template.scaleFactor),
                //    (float)(offset.Lat * -template.scaleFactor));
                //Game.log($"commanderOffset old: {commanderOffset}");
                var td = new TrackingDelta(game.systemBody.radius, siteData.location);
                this.commanderOffset = new PointF(
                    (float)td.dx,
                    -(float)td.dy);
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
                var td = new TrackingDelta(game.systemBody!.radius, this.siteData.location);
                formEditMap.txtDeltaLat.Text = td.dy.ToString("N2") + "m";
                formEditMap.txtDeltaLong.Text = td.dx.ToString("N2") + "m";
            }

            this.Invalidate();
        }

        private FileSystemWatcher watcher;

        private void devFileWatcher()
        {
            string filepath = Path.Combine(Application.StartupPath, "settlementTemplates.json");

            if (Debugger.IsAttached)
                filepath = Path.Combine(Application.StartupPath, "..\\..\\..\\..\\", "settlementTemplates.json");

            Game.log($"Dev watching: {filepath}");


            this.watcher = new FileSystemWatcher(Path.GetFullPath(Path.GetDirectoryName(filepath)!), "settlementTemplates.json");
            this.watcher.Changed += Watcher_Changed;
            this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            this.watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.IsDisposed) return;
            Program.crashGuard(() =>
            {
                Game.log("Reloading watched site template");
                Application.DoEvents(); Application.DoEvents(); Application.DoEvents(); Application.DoEvents();
                SiteTemplate.Import(true);
                this.loadSiteTemplate();
                this.Invalidate();
            }, true);
        }

        protected override void onPaintPlotter(PaintEventArgs e)
        {
            if (this.IsDisposed || game.isShutdown || this.siteData == null) return;

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
            var td = new TrackingDelta(game.systemBody!.radius, siteData.location!);
            var x = -td.dx;
            var y = td.dy;

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
                    (float)(x - vr), (float)(y - vr),
                    +vr * 2, +vr * 2);
            g.DrawEllipse(shipPen, rect);
            var dHeading = new Angle(game.status.Heading);
            //var dHeading = new Angle(game.status.Heading) - siteData.siteHeading;
            //var dHeading = new Angle(siteData.siteHeading) - game.status.Heading;
            var dx = (decimal)DecimalEx.Sin(dHeading.radians) * 10;
            var dy = (decimal)DecimalEx.Cos(dHeading.radians) * 10;
            g.DrawLine(shipPen, (float)x, (float)y, (float)(x + dx), (float)(y - dy));
            this.drawCompassLines(siteData.siteHeading);

            Point[] pps = { new Point((int)x, (int)y) };
            g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pps);

            g.ResetTransform();
            this.clipToMiddle();
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
            if (adjustAngle < 0) adjustAngle += 360;

            var headerTxt = $"Site heading: {siteData.siteHeading}°";

            Brush brush = GameColors.brushGameOrange;
            if (Math.Abs(adjustAngle) > 2)
            {
                brush = GameColors.brushCyan;
                var ax = 30f;
                var ay = -mid.Height * 0.8f;
                var ad = 80;
                var ac = 10;
                if (adjustAngle < 0 || adjustAngle > 180)
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

            if (Math.Abs(adjustAngle) > 0)
            {
                headerTxt += $" | Rotate ship ";
                headerTxt += adjustAngle > 0 && adjustAngle < 180 ? "right" : "left";
                if (adjustAngle > 180) adjustAngle = 360 - adjustAngle;
                headerTxt += $" {Math.Abs(adjustAngle)}°";
            }

            this.drawHeaderText(headerTxt, brush);
        }

        private void drawSiteMap()
        {
            if (g == null || this.template == null || this.siteMap == null) return;

            if (this.underlay == null)
            {
                Game.log("Why no underlay?");
                return;
            }

            // get pixel location of site origin relative to overlay window --
            g.ResetTransform();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(-game.status.Heading);

            PointF[] pts = { new PointF(commanderOffset.X, commanderOffset.Y) };
            g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pts);
            var siteOrigin = pts[0];
            g.ResetTransform();

            // Render background bitmap, rotated for commander heading, than translated, then rotated for the site heading
            g.ResetTransform();
            this.clipToMiddle();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(this.scale, this.scale);

            var mx = template.imageOffset.X * template.scaleFactor;
            var my = template.imageOffset.Y * template.scaleFactor;

            var sx = siteMap.Width * template.scaleFactor;
            var sy = siteMap.Height * template.scaleFactor;

            var r1 = -game.status.Heading;
            var r2 = siteData.siteHeading;

            var rx = commanderOffset.X;
            var ry = commanderOffset.Y;

            g.RotateTransform(+r1); // rotate by commander heading
            //g.DrawEllipse(Pens.HotPink, -20, -20, 40, 40);
            g.TranslateTransform(rx, ry);
            g.RotateTransform(+r2); // rotate by site heading

            g.DrawImage(this.siteMap, -mx, -my, sx, sy); // <-- -- -- **
            g.DrawEllipse(Pens.DarkRed, -2, -2, 4, 4);
            //g.DrawEllipse(Pens.DarkRed, -5, -5, 10, 10);

            //g.DrawRectangle(Pens.Blue, -mx, -my, sx, sy);
            //g.DrawLine(Pens.Blue, -mx, -my, -mx + sx, -my + sy);
            //g.DrawLine(Pens.Blue, -mx, -my + sy, sx - mx, -my);


            g.ResetTransform();
            this.clipToMiddle();
            g.TranslateTransform(mid.Width, mid.Height);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(-game.status.Heading);

            // draw compass rose lines centered on the commander
            float x = commanderOffset.X;
            float y = commanderOffset.Y;
            g.DrawLine(Pens.DarkRed, -this.Width * 2, y, +this.Width * 2, y);
            g.DrawLine(Pens.DarkRed, x, y, x, +this.Height * 2);
            g.DrawLine(Pens.Red, x, -this.Height * 2, x, y);

            this.drawTouchdownAndSrvLocation0(true);

            this.drawArtifacts(siteOrigin);

            this.drawObeliskGroupNames(siteOrigin);

            this.drawCommander0();
            g.ResetClip();
        }

        private void drawObeliskGroupNames(PointF siteOrigin)
        {
            if (g == null || template == null || (this.touchdownLocation0 == null && this.srvLocation0 == null)) return;

            // reset transform with origin at that point, scaled and rotated to match the commander
            g.ResetTransform();
            this.clipToMiddle();
            g.TranslateTransform(siteOrigin.X, siteOrigin.Y);
            g.ScaleTransform(this.scale, this.scale);
            var rot = 360 - game.status.Heading;
            g.RotateTransform(rot);

            foreach (var foo in template.obeliskGroupNameLocations)
            {
                if (formEditMap?.tabs.SelectedIndex != 2 && !siteData.obeliskGroups.Contains(foo.Key[0])) continue;

                if (foo.Value != PointF.Empty)
                {
                    var angle = 180 - siteData.siteHeading - foo.Value.X;
                    var dist = foo.Value.Y;
                    var pt = (PointF)Util.rotateLine((decimal)angle, (decimal)dist);
                    // draw guide lines when map editor is active
                    if (this.formEditMap?.tabs.SelectedIndex == 2 && this.formEditMap?.listGroupNames.Text == foo.Key)
                    {
                        var r2 = new RectangleF(-dist, -dist, dist * 2, dist * 2);
                        g.DrawEllipse(GameColors.penMapEditGuideLineGreen, r2);
                        g.DrawLine(GameColors.penMapEditGuideLineGreen, 0, 0, -pt.X, -pt.Y);
                    }

                    // draw background smudge?
                    // var pad = 18;
                    // var rect = new Rectangle(-(int)pt.X - pad, -(int)pt.Y - pad, pad * 2, pad * 2);
                    //g.FillEllipse(Brushes.Navy, rect);

                    // draw group name character
                    var sz = g.MeasureString(foo.Key, GameColors.fontBigBold);
                    var brush = this.formEditMap?.tabs.SelectedIndex == 2 ? Brushes.Lime : Brushes.DarkCyan; // game.cmdr.ramTahActive ? Brushes.SlateGray : Brushes.DarkCyan;

                    // we must re-translate/rotate otherwise the text will be rotated too
                    g.TranslateTransform(-pt.X, -pt.Y);
                    g.RotateTransform(-rot);

                    g.DrawString(foo.Key, GameColors.fontBig, brush, -(sz.Width / 2) + 2, -(sz.Height / 2) + 2);

                    g.RotateTransform(rot);
                    g.TranslateTransform(+pt.X, +pt.Y);
                }

            }
        }

        private void drawArtifacts(PointF siteOrigin)
        {
            if (this.template == null || this.template.poi.Count == 0)
            {
                // there is no map for these
                this.drawFooterText($"(There is no map yet for: {siteData.type})");
                return;
            }

            // reset transform with origin at that point, scaled and rotated to match the commander
            g.ResetTransform();
            this.clipToMiddle();
            g.TranslateTransform(siteOrigin.X, siteOrigin.Y);
            g.ScaleTransform(this.scale, this.scale);
            g.RotateTransform(360 - game.status.Heading);

            var nearestDist = double.MaxValue;
            var nearestPt = PointF.Empty;

            var nearestUnknownDist = double.MaxValue;
            var nearestUnknownPt = PointF.Empty;
            SitePOI? nearestUnknownPoi = null;

            this.nearestObeliskDist = 1000d;
            int countRelics = 0, confirmedRelics = 0, countPuddles = 0, confirmedPuddles = 0;
            Angle aa;
            string tt;

            // and draw all the POIs
            var poiToRender = siteData.rawPoi == null
                ? this.template.poi
                : this.template.poi.Union(siteData.rawPoi);

            foreach (var poi in poiToRender)
            {
                var isObelisk = poi.type == POIType.obelisk || poi.type == POIType.brokeObelisk;
                // skip obelisks in groups not in this site
                if (formEditMap?.tabs.SelectedIndex != 2 && siteData.obeliskGroups.Count > 0 && isObelisk && !string.IsNullOrEmpty(poi.name) && !siteData.obeliskGroups.Contains(poi.name[0])) continue;

                var poiStatus = siteData.getPoiStatus(poi.name);

                if (poi.type == POIType.relic)
                {
                    countRelics++;
                    if (poiStatus != SitePoiStatus.unknown)
                        confirmedRelics++;
                }
                else if (poi.type != POIType.obelisk && poi.type != POIType.brokeObelisk)
                {
                    countPuddles++;
                    if (poiStatus != SitePoiStatus.unknown)
                        confirmedPuddles++;
                }

                // calculate render point for POI
                var deg = 180 - siteData.siteHeading - poi.angle;
                var dist = poi.dist; //  * 1.01m; // scaling for perspective? !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                var pt = (PointF)Util.rotateLine(
                    deg,
                    dist);

                // work in progress - only render if a RUINS poi
                if (this.isRuinsPoi(poi.type, true, true) || Game.settings.enableEarlyGuardianStructures)
                    // render it
                    this.drawSitePoi(poi, (PointF)pt);

                // is this the closest POI?
                var x = pt.X - commanderOffset.X;
                var y = pt.Y - commanderOffset.Y;
                var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                // status is unknown, it's closer and not an obelisk
                var validNearestUnknown = poiStatus == SitePoiStatus.unknown && d < nearestUnknownDist && !isObelisk;
                // && (isRuinsPoi(poi.type, false) || (Game.settings.enableEarlyGuardianStructures && !siteData.isRuins && poi.type != POIType.obelisk && poi.type != POIType.brokeObelisk));
                // only target Relic Towers when in SRV
                if (validNearestUnknown && poi.type == POIType.relic && game.vehicle != ActiveVehicle.SRV)
                    validNearestUnknown = false;
                // it's a present closer relic without a heading (structures only)
                if (poi.type == POIType.relic && poiStatus == SitePoiStatus.present && !siteData.isRuins && d < nearestUnknownDist && siteData.getRelicHeading(poi.name) == null)
                    validNearestUnknown = true;
                // target obelisks trump above conditions
                if (targetObelisk != null)
                    validNearestUnknown = poi.name.Equals(targetObelisk, StringComparison.OrdinalIgnoreCase);

                if (validNearestUnknown)
                {
                    nearestUnknownPoi = poi;
                    nearestUnknownDist = d;
                    nearestUnknownPt = pt;
                }

                if (isObelisk && d < this.nearestObeliskDist)
                    this.nearestObeliskDist = d;

                var selectPoi = false;
                if (game.status.SelectedWeapon != "$humanoid_companalyser_name;")
                    selectPoi = d < nearestDist && (/*isRuinsPoi(poi.type, true) &&*/ poi.type != POIType.brokeObelisk);
                else
                    selectPoi = d < nearestDist && (poi.type == POIType.relic);

                if (forcePoi != null)
                    selectPoi = forcePoi == poi; // force selection in map editor if present
                if (selectPoi && poi.type == POIType.obelisk && siteData.getActiveObelisk(poi.name) == null && formEditMap == null)
                    selectPoi = false;

                // (obelisks are not selectable unless on foot or SRV)
                if (selectPoi && isObelisk && (game.vehicle != ActiveVehicle.Foot && game.vehicle != ActiveVehicle.SRV))
                    selectPoi = false;

                // (relic towers are not selectable unless on foot or SRV)
                if (selectPoi && poi.type == POIType.relic && (game.vehicle != ActiveVehicle.Foot && game.vehicle != ActiveVehicle.SRV))
                    selectPoi = false;

                if (selectPoi) // && poi.type != POIType.pylon && poi.type != POIType.brokeObelisk && poi.type != POIType.component)
                {
                    aa = Util.ToAngle(x, y) - siteData.siteHeading;
                    if (y < 0) aa += 180;
                    tt = $"{aa} | {x}, {y}";
                    nearestDist = d;
                    this.nearestPoi = poi;
                    nearestPt = pt;
                }
            }

            // set/clear the current obelisk
            if (this.nearestPoi?.type == POIType.obelisk && nearestDist < 25)
                siteData.setCurrentObelisk(this.nearestPoi.name);
            else
                siteData.setCurrentObelisk(null);

            if (this.formEditMap?.tabs.SelectedIndex == 0 && this.forcePoi != null)
            {
                // draw guide lines when map editor is active
                g.DrawLine(GameColors.penMapEditGuideLineGreenYellow, -nearestPt.X * 0.96f, -nearestPt.Y * 0.96f, -nearestPt.X * 1.04f, -nearestPt.Y * 1.04f);
                g.DrawEllipse(GameColors.penMapEditGuideLineGreenYellow, (float)-this.forcePoi.dist, (float)-this.forcePoi.dist, (float)this.forcePoi.dist * 2, (float)this.forcePoi.dist * 2);
            }

            // draw an indicator to the nearest unknown POI or target obelisk
            if (nearestUnknownPoi != null)
            {
                g.DrawLine(GameColors.penNearestUnknownSitePOI, -nearestUnknownPt.X, -nearestUnknownPt.Y, -commanderOffset.X, -commanderOffset.Y);
            }

            // draw footer text
            var footerTxt = "";
            var footerBrush = GameColors.brushGameOrange;


            if (this.targetObelisk != null && nearestUnknownDist != double.MaxValue) // && this.targetObelisk == siteData.currentObelisk?.name)
            {
                footerTxt = $"Obelisk {this.targetObelisk} - dist: {Util.metersToString(nearestUnknownDist)}";
                footerBrush = GameColors.brushCyan;
                if (this.targetObelisk == this.nearestPoi?.name)
                    g.DrawEllipse(GameColors.penLime2Dot, -nearestPt.X - 8, -nearestPt.Y - 8, 16, 16);
            }
            else if (nearestDist > 75 && forcePoi == null && this.targetObelisk == null)
            {
                // make sure we're relatively close before selecting the item
                this.nearestPoi = null!;

                switch (siteData.type)
                {
                    case SiteType.Crossroads:
                    case SiteType.Lacrosse:
                    case SiteType.Squid:
                    case SiteType.Stickyhand:
                        // there is no map for these
                        footerTxt = $"(There is no map yet for: {siteData.type})";
                        break;

                    default:
                        if (siteData.isRuins)
                            footerTxt = $"{siteData.bodyName}, Ruins #{siteData.index} - {siteData.type}";
                        else
                            footerTxt = $"{siteData.bodyName}: {siteData.type}";
                        break;
                }
            }
            else if (nearestPoi != null)
            {
                // draw highlight over closest POI
                if ((nearestPoi.type == POIType.obelisk))
                {
                    // use a smaller circle for obelisks
                    g.DrawEllipse(GameColors.penLime2Dot, -nearestPt.X - 8, -nearestPt.Y - 8, 16, 16);
                }
                else if ((nearestPoi == forcePoi) || isRuinsPoi(nearestPoi.type, false) || siteData.activeObelisks.ContainsKey(nearestPoi.name) || (!siteData.isRuins && Game.settings.enableEarlyGuardianStructures))
                {
                    g.DrawEllipse(GameColors.penLime4Dot, -nearestPt.X - 13, -nearestPt.Y - 13, 26, 26);
                }

                var poiStatus = siteData.getPoiStatus(this.nearestPoi.name);

                var action = "";

                if (this.nearestPoi.type == POIType.obelisk || this.nearestPoi.type == POIType.brokeObelisk)
                {
                    var obelisk = siteData.getActiveObelisk(nearestPoi.name);
                    // draw footer text about obelisks
                    if (this.targetObelisk != null)
                    {
                        footerTxt = $"Obelisk {this.targetObelisk} dist: {Util.metersToString(nearestUnknownDist)}";
                        footerBrush = GameColors.brushCyan;
                    }
                    else
                    {
                        footerTxt = $"Obelisk {nearestPoi.name}: " + (obelisk == null ? "Inactive" : "Active") + (obelisk?.scanned == true ? " (scanned)" : "");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(action) && this.nearestPoi.type == POIType.relic && poiStatus != SitePoiStatus.absent)
                    {
                        // show the relic tower heading (individual or site general)
                        var relicHeading = siteData.getRelicHeading(this.nearestPoi.name);
                        if (relicHeading != null)
                            action = $" ({relicHeading}°)";
                        else if (siteData.isRuins)
                            action = $" (site: {siteData.relicTowerHeading}°)";
                        else if (relicHeading == null || siteData.relicTowerHeading == -1)
                            action = $" (unknown heading)";
                    }
                    footerBrush = poiStatus == SitePoiStatus.unknown ? GameColors.brushCyan : GameColors.brushGameOrange;
                    footerTxt = $"{this.nearestPoi.type} {this.nearestPoi.name}: {poiStatus} {action}";
                }
            }
            this.drawFooterText(footerTxt, footerBrush);

            var headerBrush = confirmedRelics + confirmedPuddles < countRelics + countPuddles
                ? GameColors.brushCyan
                : GameColors.brushGameOrange;

            // Draw header
            this.drawHeader(confirmedRelics, countRelics, confirmedPuddles, countPuddles);

            g.ResetTransform();
        }

        private void drawHeader(int confirmedRelics, int countRelics, int confirmedPuddles, int countPuddles)
        {
            var status = siteData.getCompletionStatus();

            if (confirmedRelics < countRelics || confirmedPuddles < countPuddles)
            {
                // how many relic and puddles remain
                this.drawHeaderText($"Survey: {status.percent} | {confirmedRelics}/{countRelics} relics, {confirmedPuddles}/{countPuddles} items", GameColors.brushCyan); // TODO: use this.drawAt for different colors?
            }
            else if (siteData.isRuins)
            {
                // Ruins ...
                if (siteData.relicTowerHeading == -1)
                    this.drawHeaderText($"Need Relic Tower heading", GameColors.brushCyan);
                else
                    this.drawHeaderText($"Ruins #{siteData.index}: survey complete", GameColors.brushGameOrange);
            }
            else
            {
                // Structures...
                var countRelicTowersPresent = siteData.poiStatus.Count(_ => _.Key.StartsWith('t') && _.Value == SitePoiStatus.present);
                if (status.countRelicsNeedingHeading > 0)
                    this.drawHeaderText($"Need {status.countRelicsNeedingHeading} Relic Tower heading", GameColors.brushCyan);
                else
                    this.drawHeaderText($"Structure {siteData.type}: survey complete", GameColors.brushGameOrange);
            }
        }

        private bool isRuinsPoi(POIType poiType, bool incObelisks, bool incBrokeObelisks = false)
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

                case POIType.obelisk:
                    return incObelisks;

                case POIType.brokeObelisk:
                    return incBrokeObelisks;

                default:
                    return false;
            }
        }

        private void drawSitePoi(SitePOI poi, PointF pt)
        {
            // render no POI if form editor says so
            if (formEditMap != null && formEditMap.checkHideAllPoi.Checked)
                return;

            var poiStatus = this.siteData.getPoiStatus(poi.name);

            // diameters: relics are bigger then puddles (and bigger still at ruins)
            var d = poi.type == POIType.relic ? 16f : 10f;
            if (siteData.isRuins && poiStatus != SitePoiStatus.unknown) d *= 1.6f;
            var dd = d / 2;

            if (formEditMap != null && formEditMap.checkHighlightAll.Checked)
            {
                // highlight everything if map editor check says so
                var b = new SolidBrush(Color.FromArgb(160, Color.DarkSlateBlue));
                g.DrawEllipse(Pens.AntiqueWhite, -pt.X - dd - 2, -pt.Y - dd - 2, d + 4, d + 4);
            }

            if (poiStatus == SitePoiStatus.unknown && poi.type != POIType.obelisk && poi.type != POIType.brokeObelisk)
            {
                // anything unknown gets a blue circle underneath with dots
                g.FillEllipse(GameColors.brushAroundPoiUnknown, -pt.X - dd - 5, -pt.Y - dd - 5, d + 10, d + 10);
                g.DrawEllipse(GameColors.penAroundPoiUnknown, -pt.X - dd - 4, -pt.Y - dd - 4, d + 8, d + 8);
            }

            switch (poi.type)
            {
                case POIType.obelisk:
                case POIType.brokeObelisk:
                    this.drawObelisk(poi, pt, poiStatus); break;
                case POIType.pylon:
                    this.drawPylon(poi, pt, poiStatus); break;
                case POIType.component:
                    this.drawComponent(poi, pt, poiStatus); break;
                case POIType.relic:
                    this.drawRelicTower(poi, pt, poiStatus); break;
                default:
                    this.drawPuddle(poi, pt, poiStatus); break;
            }
        }

        private static PointF[] obeliskPoints = {
            new PointF(1 - 1.5f, 3.5f - 1.5f),
            new PointF(0 - 1.5f, 0 - 1.5f),
            new PointF(4 - 1.5f, 1 - 1.5f),
            new PointF(1 - 1.5f, 3.5f - 1.5f),
        };
        private static PointF[] brokeObeliskPoints = {
            new PointF(1 - 1.5f, 4 - 1.5f),
            new PointF(0 - 1.5f, 0 - 1.5f),
            new PointF(4 - 1.5f, 1 - 1.5f),
            new PointF(1 - 1.5f, 4 - 1.5f),
        };

        private void drawObelisk(SitePOI poi, PointF pt, SitePoiStatus poiStatus)
        {
            var rot = poi.rot + this.siteData.siteHeading + 167.5m; // adjust for rotation of points by: 167.5m

            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)rot);

            // show dithered arc for active obelisks - changing the colour if scanned or is relevant for Ram Tah
            var obelisk = siteData.getActiveObelisk(poi.name);
            if (poi.type == POIType.obelisk && obelisk != null)
            {
                var ramTahNeeded = game.cmdr.ramTahActive && game.systemSite?.ramTahObelisks?.ContainsKey(obelisk.msg) == true;
                if (ramTahNeeded && game.cmdr.ramTahActive)
                    GameColors.shiningBrush.CenterColor = GameColors.Cyan;
                else if (obelisk.scanned)
                    GameColors.shiningBrush.CenterColor = GameColors.Orange;
                else
                    GameColors.shiningBrush.CenterColor = Color.LightGray;

                g.FillPath(GameColors.shiningBrush, GameColors.shiningPath);
            }

            var points = poi.type == POIType.obelisk ? obeliskPoints : brokeObeliskPoints;
            var pp = obelisk == null ? GameColors.penObelisk : GameColors.penObeliskActive;
            g.DrawLines(pp, points);

            if (poi.type == POIType.obelisk)
            {
                g.DrawLine(pp, 0.2f, 0, -0.5f, -1.2f);
                g.DrawLine(pp, 0.2f, 0, +1.5f, -0.8f);
            }

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);
        }

        private static PointF[] pylonPoints = {
            new PointF(0, -3),
            new PointF(+6, 0),
            new PointF(0, +3),
            new PointF(-6, 0),
            new PointF(0, -3),
        };

        private void drawPylon(SitePOI poi, PointF pt, SitePoiStatus poiStatus)
        {
            var rot = poi.rot + this.siteData.siteHeading;

            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)+rot);

            var pp = GameColors.Map.pens[POIType.pylon][poiStatus];
            g.DrawLines(pp, pylonPoints);
            g.DrawLine(pp, 0, 0, 0, 3);

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);
        }

        private static PointF[] componentPoints = {
            new PointF(0, +2),
            new PointF(-2, -1),
            new PointF(+2, -1),
            new PointF(0, +2),
            new PointF(0, +5),
            new PointF(-5, -3),
            new PointF(+5, -3),
            new PointF(0, +5),
        };

        private void drawComponent(SitePOI poi, PointF pt, SitePoiStatus poiStatus)
        {
            var rot = poi.rot + this.siteData.siteHeading - 45;

            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)+rot);

            var pp = GameColors.Map.pens[POIType.component][poiStatus];
            g.DrawLines(pp, componentPoints);

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);
        }

        private static PointF[] relicPoints = {
            new PointF(-5, -3),
            new PointF(+5, -3),
            new PointF(0, +6),
            new PointF(-5, -3),
        };

        private void drawRelicTower(SitePOI poi, PointF pt, SitePoiStatus poiStatus)
        {
            // draw dashed blue line at ruins only
            if (poiStatus == SitePoiStatus.unknown && siteData.isRuins)
                g.DrawEllipse(GameColors.penPoiRelicUnconfirmed, -pt.X - 8, -pt.Y - 8, 16, 16);

            var towerHeading = siteData.getRelicHeading(poi.name);
            var rot = towerHeading == null ? 0 : towerHeading - 180;

            // if at ruins, with site relic heading - use that over nothing
            if (towerHeading == null && siteData.isRuins && siteData.relicTowerHeading != -1)
                rot = siteData.relicTowerHeading - 180;

            g.TranslateTransform(-pt.X, -pt.Y);
            g.RotateTransform((float)+rot);

            var bb = poiStatus == SitePoiStatus.present
                ? GameColors.brushPoiPresent
                : GameColors.brushPoiMissing;
            g.FillPolygon(bb, relicPoints);

            var pp = poiStatus == SitePoiStatus.present
                ? GameColors.penPoiRelicPresent
                : GameColors.penPoiRelicMissing;
            g.DrawPolygon(pp, relicPoints);

            if (towerHeading != null)
                g.DrawLine(GameColors.Map.penRelicTowerHeading, 0, -2000, 0, 2000);

            g.RotateTransform((float)-rot);
            g.TranslateTransform(+pt.X, +pt.Y);
        }

        private void drawPuddle(SitePOI poi, PointF pt, SitePoiStatus poiStatus)
        {
            var d = 10f;
            if (siteData.isRuins && poiStatus != SitePoiStatus.unknown) d *= 1.5f;
            var dd = d / 2;

            if (poiStatus == SitePoiStatus.unknown)
            {
                // draw dotted blue line
                g.DrawEllipse(GameColors.penPoiPuddleUnconfirmed, -pt.X - dd, -pt.Y - dd, d, d);
            }
            else if (poiStatus == SitePoiStatus.absent)
            {
                if (!siteData.isRuins) { d -= 2; dd -= 1; } // make them a bit smaller at structures

                g.FillEllipse(GameColors.brushPoiMissing, -pt.X - dd, -pt.Y - dd, d, d);
                g.DrawEllipse(GameColors.penPoiMissing, -pt.X - dd, -pt.Y - dd, d, d);
            }
            else if (poiStatus == SitePoiStatus.absent)
            {
                if (!siteData.isRuins) { d -= 2; dd -= 1; } // make them a bit smaller at structures

                g.FillEllipse(GameColors.brushPoiMissing, -pt.X - dd, -pt.Y - dd, d, d);
                g.DrawEllipse(GameColors.penPoiMissing, -pt.X - dd, -pt.Y - dd, d, d);
            }
            else // puddles present or empty
            {
                g.FillEllipse(GameColors.Map.brushes[poi.type][poiStatus], -pt.X - dd, -pt.Y - dd, d, d);
                g.DrawEllipse(GameColors.Map.pens[poi.type][poiStatus], -pt.X - dd, -pt.Y - dd, d, d);
            }
        }

        private void drawSiteTypeHelper()
        {
            if (g == null) return;

            this.drawHeaderText($"{siteData.nameLocalised} | {siteData.type} | ???°");
            this.drawFooterText($"{game.systemBody?.name}");
            g.ResetTransform();
            this.clipToMiddle();

            string msg;
            SizeF sz;
            var tx = 10f;
            var ty = 20f;

            this.drawFooterText($"{game.systemBody?.name}");

            // if we don't know the site type yet ...
            msg = $"\r\nSelect site type with \r\nfire group or send\r\nmessage:\r\n\r\n 'a' for Alpha\r\n\r\n 'b' for Beta\r\n\r\n 'g' for Gamma";
            sz = g.MeasureString(msg, GameColors.font1, this.Width);
            g.DrawString(msg, GameColors.font1, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);
        }

        private void drawSiteHeadingHelper()
        {
            if (g == null) return;

            this.drawHeaderText($"{siteData.nameLocalised} | {siteData.type} | ???°");
            this.drawFooterText($"{game.systemBody?.name}");
            g.ResetTransform();
            this.clipToMiddle();

            string msg;
            var tx = ten;
            var ty = twoEight;

            var isRuins = siteData.isRuins;
            msg = $"Need site heading:\r\n\r\n■ To use current heading either:\r\n    - Toggle cockpit mode twice\r\n    - Send message:   .heading\r\n\r\n■ Or send message: <degrees>";
            if (isRuins)
                msg += $"\r\n\r\nAlign with this buttress:";
            else
            {
                switch (siteData.type)
                {
                    case SiteType.Crossroads:
                    case SiteType.Fistbump:
                    case SiteType.Lacrosse:
                    case SiteType.Squid:
                    case SiteType.Stickyhand:
                        // there is no map for these
                        msg += $"\r\n\r\n■ Note: there is no map yet for: {siteData.type}";
                        break;
                }

                if (siteData.name.StartsWith("$Ancient_Medium") || siteData.name.StartsWith("$Ancient_Small"))
                    msg += $"\r\n\r\nAlign with the data port:";
            }
            var sz = g.MeasureString(msg, GameColors.fontMiddle, this.Width);

            g.DrawString(msg, GameColors.fontMiddle, GameColors.brushCyan, tx, ty, StringFormat.GenericTypographic);

            // show location of buttress or other thing to align with
            if (this.headingGuidance != null)
                g.DrawImage(this.headingGuidance, forty, ty + sz.Height);
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
