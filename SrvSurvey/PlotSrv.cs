using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey
{
    class PlotSrv
    {
        private JournalWatcher watcher;

        public bool InSrv { get; private set; }

        public ApproachSettlement LastApproachSettlement { get; private set; }
        public Touchdown LastTouchdown { get; private set; }
        public CodexEntry LastCodexEntry { get; private set; }

        public PointF pointSettlement { get; private set; }
        public PointF pointTouchdown { get; private set; }
        public PointF offsetTouchdownToSettlement { get; private set; }

        private SizeF offset = new SizeF(50, 50);

        public Image plotSrv { get; private set; }

        public PlotSrv(JournalWatcher watcher)
        {
            this.watcher = watcher;
            watcher.onJournalEntry += Watcher_onJournalEntry;

            this.plotSrv = new Bitmap(1000, 1000);

            using (var g = Graphics.FromImage(this.plotSrv))
            {
                //    //g.RotateTransform(44);
                //    //g.ScaleTransform(2, 2);
                //    //g.TranslateTransform(50, 50);
                //    //g.ScaleTransform(20, 20);
                //    //g.PageScale = 20;
                //    //g.TranslateTransform(this.pointSettlement.X, this.pointSettlement.Y);
                //    g.DrawLine(Pens.Blue, 10, 10, 30, 10);
                g.FillRectangle(Brushes.LightGray, 0, 0, 1000, 1000);
                }


            }

        private void Watcher_onJournalEntry(JournalEntry entry, int index)
        {
            // update state machine
            if (entry is LaunchSRV) this.InSrv = true;
            else if (entry is DockSRV) this.InSrv = false;

            if (entry is ApproachSettlement) this.onApproachSettlement(entry as ApproachSettlement);
            if (entry is Touchdown) this.onTouchdown(entry as Touchdown);
            if (entry is CodexEntry) this.onCodexEntry(entry as CodexEntry);
        }

        private Graphics createGraphics()
        {
            var g = Graphics.FromImage(this.plotSrv);

            //g.RotateTransform(44);
            //g.ScaleTransform(2, 2);
            //g.TranslateTransform(-this.plotSrv.Width , -this.plotSrv.Height );
            //g.TranslateTransform(-this.plotSrv.Width - this.pointSettlement.X, -this.plotSrv.Height - this.pointSettlement.Y);
            //g.ScaleTransform(20, 20);

            return g;
        }

        private void onApproachSettlement(ApproachSettlement entry)
        {
            this.LastApproachSettlement = entry as ApproachSettlement;
            this.pointSettlement = LatLong.From(entry);

            using (var g = this.createGraphics())
            {
                var pp = this.pointSettlement - new SizeF(150, 150);
                g.DrawLine(Pens.Red, new PointF(10,10), pp);
                //g.DrawLine(Pens.Blue, pp, this.pointSettlement);
            }

        }

        private void onTouchdown(Touchdown entry)
        {
            this.LastTouchdown = entry as Touchdown;
            this.pointTouchdown = LatLong.From(entry);
            var offsetTouchdownToSettlement = this.pointSettlement - new SizeF(this.pointTouchdown);

            using (var g = Graphics.FromImage(this.plotSrv))
            {
                //g.DrawLine(Pens.Blue, (PointF)this.offset, offsetTouchdownToSettlement + this.offset);
            }

        }

        private void onCodexEntry(CodexEntry entry)
        {
            this.LastCodexEntry = entry as CodexEntry;
            var pointCodex = LatLong.From(entry);


            using (var g = Graphics.FromImage(this.plotSrv))
            {
                //g.DrawLine(Pens.Red, this.pointSettlement, pointCodex);
            }

        }
    }
}
