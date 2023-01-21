using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
//using System.Text.Json.Serialization;

namespace SrvSurvey
{
    public partial class SrvSurvey : Form
    {
        public static string journalFolder { get; private set; }

        static SrvSurvey()
        {
            SrvSurvey.journalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Saved Games\Frontier Developments\Elite Dangerous\");
            SiteTemplate.Import();
        }

        static string LastJournal
        {
            get
            {
                var entries = new DirectoryInfo(SrvSurvey.journalFolder).EnumerateFiles("*.log", SearchOption.TopDirectoryOnly);
                var lastOne = entries.OrderBy(_ => _.LastWriteTimeUtc).Last();
                //return @"C:\Users\grinn\Saved Games\Frontier Developments\Elite Dangerous\Journal.2023-01-11T185238.01.log";
                return lastOne.FullName;
            }
        }

        private Status status;

        private JournalWatcher watcher;
        private SurveyingSrv survey;
        private LatLong pointSrv;

        public PointF imgOffset;
        private SiteTemplate siteTemplate;
        private Bitmap siteBackground;
        private Image plotSmudge;
        private Image plotSrv;

        int cx, cy;

        public SrvSurvey()
        {
            this.status = new Status(true);
            this.status.StatusChanged += onStatusChange;
            InitializeComponent();

            //this.Opacity = 0.5;


            //statusWatcher = new FileSystemWatcher(SrvSurvey.journalFolder, "Status.json"); // !!!
            //statusWatcher.Changed += StatusWatcher_Changed;
            //statusWatcher.NotifyFilter = NotifyFilters.LastWrite;
            //statusWatcher.EnableRaisingEvents = true;


            //this.srv = new PlotSrv(this.watcher);

            // tmp!
            //this.pointSettlement = new LatLong(-57.499321, -100.514328);
            //txtSettlementLocation.Text = $"Lat: { this.pointSettlement.Lat}, Long: { this.pointSettlement.Long}";

            //this.pointTouchdown = new LatLong(-57.502190, -100.510368);
            //log($"Settlement: {this.pointSettlement}");
            //log($"Touchdown: {this.pointTouchdown}");
            //log($"SRV: {new LatLong(-57.498146, -100.505043)}");


            // reposition so the origin to be the settlement location, offset to be in the center of the image
            //var sF = this.pointSettlement.ToSF();
            //this.imgOffset = new PointF(
            //    (this.plotSrv.Width / 2) - sF.X,
            //    (this.plotSrv.Height / 2) - sF.Y);
        }

        private void log(object msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    txtMsg.Text += msg.ToString() + "\r\n";
                    lblStatus.Text = msg.ToString();
                });
            }
            else
            {
                txtMsg.Text += msg.ToString() + "\r\n";
                lblStatus.Text = msg.ToString();
            }
        }

        private void SrvBuddy_Load(object sender, EventArgs e)
        {
            this.initializeFromJournal(SrvSurvey.LastJournal);
            this.onStatusChange();
        }

        private bool atSettlement
        {
            get
            {
                return this.survey.settlementType != null && this.siteTemplate != null;
            }
        }

        private void initializeFromJournal(string filename)
        {
            if (this.watcher != null)
            {
                // clean-up old first
                this.watcher.onJournalEntry -= Watcher_onJournalEntry;
                this.survey.SurveyChange -= Survey_onSurveyChange;

                this.survey = null;
            }

            log($"reading: {Path.GetFileName(filename) }");

            this.watcher = new JournalWatcher(SrvSurvey.LastJournal);
            // read all entries BEFORE listening to new ones
            this.watcher.readEntries();
            this.survey = new SurveyingSrv(this.watcher);
            this.survey.SurveyChange += Survey_onSurveyChange;

            // show what we found
            log($"found : {survey.cmdrName}");
            if (!comboCmdr.Items.Contains(survey.cmdrName))
                comboCmdr.Items.Add(survey.cmdrName);
            comboCmdr.Text = survey.cmdrName;

            if (!survey.AtSettlement)
            {
                log($"not at Settlement: {survey.AtSettlement}");
            }
            else
            {
                log($"Landed at: {survey.bodyName}, {survey.settlementName}");
                log($"Settlement: {survey.pointSettlement}");
                log($"Touchdown: {survey.pointTouchdown}");
                log($"settlementType: {survey.settlementType}");
                log($"settlementHeading: {survey.settlementHeading}");
                log($"relicHeading: {survey.relicHeading}");
                log($"dist1Deg: {survey.dist1Deg}");

                txtSettlement.Text = $"{ this.survey.bodyName}, { this.survey.settlementName}";
                this.txtSettlementLocation.Text = this.survey.pointSettlement.ToString();
            }

            // if we parsed a settlement type - switch to that
            if (survey.settlementType != null)
            {
                comboType.Text = survey.siteTemplate.name;
            }

            if (survey.settlementHeading >= 0)
            {
                numSiteHeading.Value = survey.settlementHeading;
            }

            if (survey.siteTemplate != null)
            {
                numScale.Value = (decimal)survey.siteTemplate.scaleFactor;
            }
            this.loadOldTracksImage();

            // now listen to future events
            this.watcher.onJournalEntry += Watcher_onJournalEntry;

            log("Listening to journal events...");
        }

        private void loadOldTracksImage()
        {
            // load old tracks image if found
            string filename = $"{this.survey.bodyName} - {this.survey.settlementName}.tracks.png";
            string filepath = Path.Combine(SrvSurvey.journalFolder, "survey", filename);
            if (File.Exists(filepath))
            {
                using (var i = Bitmap.FromFile(filepath))
                {
                    this.plotSmudge = new Bitmap(i);

                }
            }
        }

        private void Survey_onSurveyChange()
        {
            if (survey == null) return;
            this.Invoke((MethodInvoker)delegate
            {
                if (survey?.settlementType != null)
                {
                    comboType.Text = "Ruins " + survey.settlementType;
                }

                if (this.plotSmudge != null && numSiteHeading.Value != survey.settlementHeading)
                {
                    // todo: maintain this?
                    this.plotSmudge = new Bitmap(this.plotSmudge.Width, this.plotSmudge.Height);
                }

                if (survey.settlementHeading >= 0)
                {
                    numSiteHeading.Value = survey.settlementHeading;
                }

                pictureBox1.Invalidate();
            });
        }

        private void showDistFromPoint(LatLong location1, LatLong location2)
        {
            // calculate distance to ship
            var dp = location1 - location2;
            var llDist = dp.AsDeltaDist(true);
            var meters = (int)Math.Round(llDist * survey.dist1Deg);
            //meters -= 10; // allowing for approx height from ship (or rather, that the count tends to be off by 10m always)
            lblMeters.Text = $"{meters}m";
        }

        private void onStatusChange()
        {
            if (status == null || survey == null || siteTemplate == null) return;

            // show which vehicle/mode we're in
            if (status.InSrv) txtMode.Text = "SRV";
            else if (status.InFighter) txtMode.Text = "Fighter";
            else if (status.InMainShip) txtMode.Text = "Ship";
            else if (status.OnFoot) txtMode.Text = "Foot";

            // show locational stuff
            this.txtLatLong.Text = new LatLong(status).ToString();
            lblStatus.Text = (new LatLong(status) - survey.pointSettlement).ToString();
            this.txtHeading.Text = $"{ status.Heading}°";

            // skip the rest if we're not in the SRV
            if (txtMode.Text != "SRV" && txtMode.Text != "Foot") return;

            var newPoint = new LatLong(status);

            // only if we have changed position
            if (this.pointSrv != newPoint)
            {

                this.pointSrv = newPoint;

                this.showDistFromPoint(this.pointSrv, survey.pointTouchdown);

                // match the closest thing to the SRV
                var match = this.siteTemplate.getNearestPoi2(this.pointSrv - this.survey.pointSettlement, -survey.settlementHeading, POIType.Unknown);
                this.highlightPoi = match;

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
            }
            this.pictureBox1.Invalidate();
        }


        private void setSiteTemplate(SiteTemplate template)
        {
            // exit early if not change
            if (this.siteTemplate == template) return;

            if (template == null)
            {
                this.siteTemplate = null;
                this.survey.setSettlementType(null);
                this.siteBackground = null;
                //this.plotSrv = null;
                this.pictureBox1.BackgroundImage = null;
                log("Site: unknown");
            }
            else if (survey != null)
            {
                this.siteTemplate = template;
                this.survey.siteTemplate = template;
                this.siteBackground = Bitmap.FromFile(template.backgroundImage) as Bitmap;
                this.plotSmudge = new Bitmap(siteBackground.Width, siteBackground.Height) as Bitmap;

                //this.plotSrv = new Bitmap(siteBackground.Width, siteBackground.Height) as Bitmap;
                //this.pictureBox1.BackgroundImage = this.plotSrv;

                this.cx = (int)(pictureBox1.Width / 2F);
                this.cy = (int)(pictureBox1.Height / 2F);

                log("Site: " + template.name);
                this.pictureBox1.Invalidate();
            }
        }

        private void Watcher_onJournalEntry(JournalEntry entry, int index)
        {
            // invoke the following overrides on the main thread with the correctly type of entry
            this.Invoke((MethodInvoker)delegate
            {
                this.onJournalEntry((dynamic)entry, index);
            });
        }


        private void onJournalEntry(CodexEntry entry, int index)
        {
            // ignore everything but Relic Towers
            if (entry.Name != "$Codex_Ent_Relic_Tower_Name;") return;

            var ll = new LatLong(entry);
            var d = ll - this.survey.pointSettlement;
            log($"Relic at: ${ll}");
            log($"Delta: {d.Lat.ToString("0.000000")}, {d.Long.ToString("0.000000")}");

            var dp = new LatLong(entry) - this.survey.pointSettlement;
            var match = this.siteTemplate.getNearestPoi2(dp, -survey.settlementHeading, POIType.RelicTower);
            if (match.distance > SitePOI.MaxMatchDistance)
            {
                log($"Unknown relic tower location? Please add:");

                //var newRelicTowerName = $"RT{siteTemplate.name.ToUpper()[0]}{siteTemplate.relicTowers.Count + 1}";
                //dp = dp.RotateBy(LatLong.degToRad(survey.settlementHeading));
                //log($"{{ \"{newRelicTowerName }\", new LatLong({dp.Lat.ToString("0.000000")}, {dp.Long.ToString("0.000000")}) }},");

                //siteTemplate.relicTowers.Add(newRelicTowerName, dp);
                //match = newRelicTowerName;
            };

            log($"Matched RelicTower: {match.poi.name}");
            this.highlightPoi = match;
            // ?
            //this.survey.relicTowers.Add(match);
            //this.survey.Export();
            this.pictureBox1.Invalidate();
        }

        private void onJournalEntry(DockSRV entry, int index)
        {
            this.exportTireTracks();
        }

        private void onJournalEntry(SupercruiseEntry entry, int index)
        {
            log("Leaving settlement...");
            this.setSiteTemplate(null);
            this.survey = null;
            pictureBox1.Invalidate();
        }

        private void onJournalEntry(SendText entry, int index)
        {
            var words = entry.Message.ToLower().Split(' ');
            switch (words[0])
            {
                case "site":
                    switch (words[1])
                    {
                        case "alpha":
                        case "beta":
                            comboType.Text = words[1][0].ToString().ToUpper() + words[1].Substring(1);
                            return;

                        default:
                            return;
                    }


                case "casket":
                case "orb":
                case "tablet":
                case "totem":
                case "urn":
                case "empty":
                    onScanPuddle(words[0]);
                    return;

                case "here":
                    this.showHere();
                    break;
            }

            //log(">>" + entry.Message);
            //log($"SendText At {new LatLong(this.status)}, {this.status.Heading}°");
        }

        private void showHere()
        {
            var here = new LatLong(this.status);
            var dm = (here - survey.pointSettlement).ToDeltaMeters(survey.dist1Deg);

            this.showDistFromPoint(here, survey.pointSettlement);
            log($"{{ X: {dm.X}, Y: {dm.Y} }}");
        }

        private void onScanPuddle(string item)
        {
            var dp = pointSrv - survey.pointSettlement;
            var match = this.siteTemplate.getNearestPoi2(dp, -survey.settlementHeading, POIType.Puddle);

            if (match.distance > SitePOI.MaxMatchDistance)
            {
                log($"Unknown puddle location. Please add?");

                //var newPuddleName = $"PD{siteTemplate.name.ToUpper()[0]}{siteTemplate.puddles.Count + 1}";
                //dp = dp.RotateBy(LatLong.degToRad((int)survey.settlementHeading));
                //log($"{{ \"{newPuddleName}\", new LatLong({dp.Lat.ToString("0.000000")}, {dp.Long.ToString("0.000000")}) }},");

                //siteTemplate.puddles.Add(newPuddleName, dp);

                //match = newPuddleName;
            }
            log($"Item: {item}, matched Puddle: {match}");
            this.highlightPoi = match;

            //this.survey.puddles[match] = item;
            //this.survey.Export();
            this.pictureBox1.Invalidate();
        }

        //private void onJournalEntry(Touchdown entry)
        //{
        //    this.pointTouchdown = new LatLong(entry);
        //    log($"TouchDown at {new LatLong(this.status)}, {this.status.Heading}°");

        //    for (int n = watcher.Entries.IndexOf(entry); n >= 0; n--)
        //    {
        //        var approachEntry = watcher.Entries[n] as ApproachSettlement;
        //        if (approachEntry != null 
        //            && entry.Body == approachEntry.BodyName 
        //            && approachEntry.Name == entry.NearestDestination)
        //        {
        //            this.pointSettlement = new LatLong(approachEntry);
        //        }
        //    }
        //}

        private void onJournalEntry(Commander entry, int index)
        {
            if (!comboCmdr.Items.Contains(entry.Name))
                comboCmdr.Items.Add(entry.Name);
            comboCmdr.Text = entry.Name;
        }

        private void onJournalEntry(Music entry, int index)
        {
            if (entry.MusicTrack == "MainMenu")
            {
                // save tracks image if switching to main menu
                this.exportTireTracks();
                log("Exited to main menu...");
            }
        }

        private void onJournalEntry(JournalEntry entry, int index)
        {
            // unexpected entry type...
            //log(entry.@event);
            //log($"~At {new LatLong(this.lastStatus)}, {this.lastStatus.Heading}°");
        }

        //private void StatusWatcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    //log("status changed!");
        //    this.status = this.parseStatus();

        //    this.Invoke((MethodInvoker)delegate { this.onStatusChange(); });
        //}

        private void exportTireTracks()
        {
            if (this.plotSmudge != null && this.survey != null)
            {
                // save the tracks
                string filename = $"{this.survey.bodyName} - {this.survey.settlementName}.tracks.png";
                string filepath = Path.Combine(SrvSurvey.journalFolder, "survey", filename);
                this.plotSmudge.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        private void SrvSurvey_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.exportTireTracks();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //log(File.GetLastWriteTime(SrvBuddy.StatusFile));
            //Status foo = this.parseStatus();
            //log(foo.ToString());
            //log("Landed " + ((foo.Flags & StatusFlags.Landed) > 0).ToString());
            //log("Docked " + ((foo.Flags & StatusFlags.Docked) > 0).ToString());
            //log("LandingGearDown " + ((foo.Flags & StatusFlags.LandingGearDown) > 0).ToString());
            //log("Supercruise " + ((foo.Flags & StatusFlags.Supercruise) > 0).ToString());
            //log("InMainShip " + ((foo.Flags & StatusFlags.InMainShip) > 0).ToString());
            //log("InFighter " + ((foo.Flags & StatusFlags.InFighter) > 0).ToString());
            //log("HasLatLong " + ((foo.Flags & StatusFlags.HasLatLong) > 0).ToString());
            //log("wep " + (foo.Pips.wep).ToString());

            //log(foo.Flags2 & StatusFlags.LandingGearDown);
            //log(foo.Flags2 & StatusFlags.Supercruise);

            //using (var g = Graphics.FromImage(this.plotSrv))
            //{
            //    g.Clear(Color.DarkBlue);

            //    //g.TranslateTransform(-50, -50);
            //    //g.Sc aleTransform(0.5F, 0.5F);
            //    //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //    //g.RotateTransform(44);

            //    g.DrawImageUnscaled(this.siteBackground, 0, 0);
            //    //                g.TranslateTransform(-50, -50);
            //}


            //log(this.pointSettlement);
            //log(this.pointSettlement.ToSF());
            //log(this.pointTouchdown);

            //var d = this.pointSettlement - this.pointTouchdown;
            //log(d);
            //log(d.To());

            //var codex11 = new LatLong(5.617179, -148.086594);
            //var codex12 = new LatLong(5.617392, -148.086700);
            //var codex13 = new LatLong(5.617364, -148.086456);

            //var codex2 = new LatLong(5.620966, -148.105118);
            //var codex3 = new LatLong(5.615134, -148.092316);
            //var codex4 = new LatLong(5.606137, -148.106308);

            //using (var g = this.createGraphics())
            //{
            //    //g.Clear(Color.DarkBlue);

            //    g.DrawLine(Pens.Red, this.pointSettlement.ToSF(), this.pointTouchdown.ToSF());

            //    g.DrawLine(Pens.Green, this.pointSettlement.ToSF(), codex11.ToSF());
            //    g.DrawLine(Pens.Green, this.pointSettlement.ToSF(), codex12.ToSF());
            //    g.DrawLine(Pens.Green, this.pointSettlement.ToSF(), codex13.ToSF());

            //    g.DrawLine(Pens.Green, this.pointSettlement.ToSF(), codex2.ToSF());
            //    g.DrawLine(Pens.Yellow, this.pointSettlement.ToSF(), codex3.ToSF());
            //    g.DrawLine(Pens.HotPink, this.pointSettlement.ToSF(), codex4.ToSF());


            //}
            //this.pictureBox1.Invalidate();


        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.atSettlement || survey.siteTemplate == null || survey.settlementHeading < 0) return;

            //var ll = new LatLong(e.X, e.Y);

            //var mLat = ((double)(e.X - ((float)this.pictureBox1.Width / 2)) / LatLong.Scale) + this.pointTouchdown.Lat;
            //var mLong = ((double)(e.Y - ((float)this.pictureBox1.Height / 2)) / LatLong.Scale) + this.pointTouchdown.Long;

            // offset for the size of the rectangle (should be window size not image!)
            var mousePos = this.GetMouseAsLatLong(e, false);
            mousePos.RotateBy(360 - survey.settlementHeading);

            //    new LatLong(
            //    -(e.Y - ((float)this.pictureBox1.Height / 2)),
            //    e.X - ((float)this.pictureBox1.Width / 2));


            //mousePos = (mousePos / LatLong.Scale) + this.survey.pointSettlement;
            //mousePos -= this.survey.pointSettlement;

            //var mLong = e.Y; // + (this.plotSrv.Height / 2);
            lblMousePos.Text = mousePos.ToString(); //$"Lat: {mLat}, Long: {mLong}";

            this.showDistFromPoint(mousePos, survey.pointTouchdown);
        }

        private LatLong GetMouseAsLatLong(MouseEventArgs e, bool delta)
        {
            var mousePos = new LatLong(
                -(e.Y - ((float)this.pictureBox1.Height / 2)),
                e.X - ((float)this.pictureBox1.Width / 2));

            mousePos = (mousePos / LatLong.Scale); // + this.survey.pointSettlement;
            if (delta)
                return mousePos;
            else
                return mousePos + this.survey.pointSettlement;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!this.atSettlement || survey.siteTemplate == null || survey.settlementHeading < 0) return;

            var dp = this.GetMouseAsLatLong(e, true);

            //var match = siteTemplate.getNearestPoi(dp, 0); // -LatLong.degToRad((int)survey.settlementHeading));
            var match = siteTemplate.getNearestPoi2(dp, 0, POIType.Unknown);
            //log($"! {match.poi.name}");

            // switch to highlight if different or toggle on/off if the same
            if (this.highlightPoi?.poi != match?.poi)
                this.highlightPoi = match;
            else
                this.highlightPoi = null;

            this.pictureBox1.Invalidate();
        }

        //private void pictureBox1_MouseDown_old(object sender, MouseEventArgs e)
        //{
        //    var mousePos = new LatLong(
        //        -(e.Y - ((float)this.plotSrv.Height / 2)),
        //        e.X - ((float)this.plotSrv.Width / 2));
        //    mousePos = (mousePos / LatLong.Scale) + this.survey.pointSettlement;
        //    lblMousePos.Text = mousePos.ToString(); //$"Lat: {mLat}, Long: {mLong}";

        //    var d = mousePos - this.survey.pointSettlement;
        //    //log($"Location: {mousePos}");
        //    //log($"Delta1: {d.Lat.ToString("0.000000")}, {d.Long.ToString("0.000000")}");

        //    var dist = d.AsDeltaDist();
        //    var rad = d.ToAngle() + LatLong.degToRad((int)numSiteHeading.Value);
        //    //log($"rad : {rad}");
        //    //if (rad < 0) rad += Math.PI * 2;
        //    //if (rad > Math.PI * 2) rad -= Math.PI * 2;


        //    //log($"rad2 : {rad}");

        //    double dx = Math.Cos(rad) * dist;
        //    double dy = Math.Sin(rad) * dist;

        //    log($"{{ \"xxx\", new LatLong({dy.ToString("0.000000")}, {dx.ToString("0.000000")}) }}");
        //    // this is good
        //    //var ll2 = d.RotateBy(degToRad((int)numSiteHeading.Value));
        //    //log($"Delta: {ll2}");


        //    //this.siteTemplate.relicTowers.Add(Guid.NewGuid().ToString(), ll2); // new LatLong(dy, dx));
        //    //this.survey.relicTowers.Add(ll2 + this.survey.pointSettlement);
        //    this.pictureBox1.Invalidate();

        //}

        private void comboType_TextChanged(object sender, EventArgs e)
        {
            var name = comboType.Text.ToLower();
            if (SiteTemplate.sites.ContainsKey(name))
            {
                this.setSiteTemplate(SiteTemplate.sites[name]);
            }
            else
            {
                this.setSiteTemplate(null);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // save the json data
            survey.Export();
            this.exportTireTracks();

            //SiteTemplate.Export();
            log("Saved");
        }

        private void drawBanner(Graphics g, string msg)
        {
            var sz = g.MeasureString(msg, Stationary.Need);

            g.FillRectangle(
                Stationary.OrangeFill,
                -pictureBox1.Width, 0 - (sz.Height),
                pictureBox1.Width * 2, 0 + (sz.Height)
                );

            g.DrawString(
                msg,
                Stationary.Need,
                Brushes.Black,
                0 - (sz.Width / 2), 0 - (sz.Height));

        }

        float plotScale = 0.75F;


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //var northIsUp = radioNorth.Checked;
            try
            {
                var g = e.Graphics;
                g.Clear(Color.Navy);

                // apply offset
                g.ResetTransform();
                g.TranslateTransform(cx, cy);
                g.ScaleTransform(plotScale, plotScale);

                if (this.siteTemplate == null)
                {
                    g.DrawString("Need site type", Stationary.Need, Brushes.Orange, 0, 0);
                    return;
                }
                if (this.survey.pointSettlement == LatLong.Empty)
                {
                    this.drawBanner(g, "Unknown location");
                    return;
                }

                // apply srvRotation?
                float srvRotation = status.Heading;

                // apply site rotation
                float siteRotation = (float)numSiteHeading.Value;

                if (radioSRV.Checked) siteRotation = siteRotation - srvRotation;
                siteRotation = siteRotation % 360;

                if (!radioSite.Checked) g.RotateTransform(+siteRotation);

                // draw the site bitmap, rotated by site heading
                var dda = -1;
                g.RotateTransform(-dda);
                g.DrawImageUnscaled(this.siteBackground, -this.siteTemplate.imageOffset.X, -this.siteTemplate.imageOffset.Y);
                g.RotateTransform(+dda);
                g.DrawImageUnscaled(this.plotSmudge, -this.siteTemplate.imageOffset.X, -this.siteTemplate.imageOffset.Y);

                if (this.survey.settlementHeading < 0)
                {
                    this.drawBanner(g, "Need site heading");
                    return;
                }

                // draw central lines, pointing N-S/E-W
                g.RotateTransform(-siteRotation);
                g.DrawLine(Pens.DarkRed, -pictureBox1.Width, 0, +pictureBox1.Width, 0);
                g.DrawLine(Pens.DarkRed, 0, -pictureBox1.Height, 0, +pictureBox1.Height);
                g.DrawLine(Pens.Red, 0, 0, 0, -pictureBox1.Height);
                //g.RotateTransform(+rotation);

                g.RotateTransform(+siteRotation);
                this.drawRelicTowersAndPuddles(g);
                g.RotateTransform(-siteRotation);

                // draw touchdown location
                if (this.survey.pointTouchdown != LatLong.Empty)
                {
                    var dt = this.survey.pointTouchdown - this.survey.pointSettlement;
                    PointF pt = (PointF)dt;
                    g.FillEllipse(Brushes.RoyalBlue, pt.X - 6, pt.Y - 6, 12, 12);
                }

                // draw SRV central lines
                if (status.InSrv || status.OnFoot)
                {
                    PointF dSrv = (PointF)(this.pointSrv - this.survey.pointSettlement);
                    //dSrv.X += srvFix.X;
                    //dSrv.Y += srvFix.Y;

                    var srvSize = 3 / this.plotScale;

                    //g.RotateTransform(+rotation);
                    //PointF dSrv2 = (this.pointSrv - this.survey.pointSettlement).RotateBy(0); // 360 - survey.settlementHeading); //LatLong.degToRad((int)rotation)); //
                    //g.DrawLine(Stationary.Green4,
                    //    -pictureBox1.Width, dSrv2.Y - (2 * srvSize),
                    //    +pictureBox1.Width, dSrv2.Y - (2 * srvSize));
                    //g.DrawLine(Stationary.Green4,
                    //    dSrv2.X + (2 * srvSize), -pictureBox1.Height,
                    //    dSrv2.X + (2 * srvSize), +pictureBox1.Height);
                    //g.RotateTransform(-rotation);

                    // draw SRV location
                    g.DrawEllipse(Stationary.Lime2, dSrv.X - srvSize, dSrv.Y - srvSize, srvSize * 2, srvSize * 2);
                    //g.DrawLine(Stationary.Lime2, dSrv.X, dSrv.Y, 0, 0);

                    var dx = (float)Math.Sin(LatLong.degToRad(this.status.Heading)) * 10F;
                    var dy = (float)Math.Cos(LatLong.degToRad(this.status.Heading)) * 10F;
                    g.DrawLine(Pens.Lime, dSrv.X, dSrv.Y, dSrv.X + dx, dSrv.Y - dy);
                }
            }
            catch (NullReferenceException)
            {
                // swallow and try again
                Application.DoEvents();
                pictureBox1.Invalidate();
            }
        }

        private PointF srvFix = new PointF(0, 5);

        private void drawRelicTowersAndPuddles(Graphics g)
        {
            // draw relic towers
            foreach (var rt in this.siteTemplate.poi)
            {
                var brush = Brushes.MediumBlue;
                //this.survey.relicTowers.Contains(rt.Key)
                //? Brushes.DodgerBlue
                //: Brushes.MediumBlue;

                PointF p = (PointF)rt.Value.location;
                g.FillRectangle(brush,
                    p.X - 6, p.Y - 6,
                    12, 12);
            }

            //// draw potential puddles
            //foreach (var pd in this.siteTemplate.puddles)
            //{
            //    var brush = this.survey.puddles.ContainsKey(pd.Key)
            //        ? Brushes.Magenta
            //        : Brushes.Indigo;

            //    PointF p = (PointF)pd.Value;
            //    g.FillRectangle(brush,
            //        p.X - 6, p.Y - 6,
            //        12, 12);
            //}

            //if (this.highlightPoi != null)
            //{
            //    //this.drawItemHighlight(g);
            //}

        }

        private void drawItemHighlight(Graphics g)
        {
            var txt = this.highlightPoi.poi.name;
            if (this.highlightPoi.poi.poiType == POIType.Puddle && survey.puddles.ContainsKey(this.highlightPoi.poi.name))
            {
                // for puddles - show the contained item
                txt += $": " + survey.puddles[this.highlightPoi.poi.name].ToUpper();
            }

            g.ResetTransform();
            g.TranslateTransform(cx, cy);
            g.ScaleTransform(plotScale, plotScale);

            PointF p = (PointF)this.highlightPoi.poi.location;

            if (highlightPoi.distance < SitePOI.MaxMatchDistance)
                g.DrawEllipse(Stationary.Orange6, p.X - 10, p.Y - 10, 20, 20);
            else
                g.DrawEllipse(Stationary.Orange2, p.X - 10, p.Y - 10, 20, 20);

            var sz = g.MeasureString(txt, this.Font); // ??  / plotScale;
            p.X -= sz.Width / 2;
            p.Y -= sz.Height + 10;

            g.FillRectangle(Brushes.Navy, p.X - 2, p.Y - 2, sz.Width, sz.Height);
            g.DrawString(txt, this.Font, Brushes.Orange, p);
        }

        //private Pen PP2 = new Pen(Brushes.Orange, 2);
        //private Pen PP4 = new Pen(Brushes.Orange, 4);
        //private Pen PP = Stationary.Orange2;
        private PoiVector highlightPoi;

        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            LatLong.Scale = (double)numScale.Value;
            pictureBox1.Invalidate();
        }


        private void numSiteHeading_ValueChanged(object sender, EventArgs e)
        {
            if (numSiteHeading.Value == -1)
            {
                numSiteHeading.Value = 359;
            }
            else if (numSiteHeading.Value == 360)
            {
                numSiteHeading.Value = 0;
            }
            this.pictureBox1.Invalidate();
        }
    }

    static class Stationary
    {
        public static Color GreenSmudge = Color.FromArgb(124, Color.Green);
        public static Brush TireTracks = new SolidBrush(GreenSmudge);

        public static Color OrangeIsh = Color.FromArgb(200, Color.Orange);
        public static Color OrangeIsh2 = Color.FromArgb(100, Color.Orange);
        public static Pen Orange2 = new Pen(OrangeIsh, 2);
        public static Pen Orange6 = new Pen(OrangeIsh, 6);
        public static Brush OrangeFill = new SolidBrush(OrangeIsh);

        public static Color LimeIsh = Color.FromArgb(200, Color.Lime);
        public static Pen Lime2 = new Pen(LimeIsh, 2);
        public static Pen Green4 = new Pen(GreenSmudge, 6);

        public static Font Need = new Font("Microsoft Sans Serif", 24, FontStyle.Regular);

    }
}
