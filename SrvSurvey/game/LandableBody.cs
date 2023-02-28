using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    class LandableBody
    {
        public readonly string bodyName;
        public double radius;

        //public string starSystem;
        //public long systemAddress;
        //public int bodyID;

        public LandableBody(string bodyName)
        {
            this.bodyName = bodyName;

            // see if we can get signals for this body
            this.readSAASignalsFound();
        }

        /// <summary>
        /// Meters per 1° of Latitude or Longitude
        /// </summary>
        public double mpd
        {
            get
            {
                var bodyCircumferance = this.radius * Math.PI * 2F;
                return bodyCircumferance / 360D;

            }
        }

        public List<ScanSignal> Signals { get; set; }
        public List<ScanGenus> Genuses { get; set; }

        public void readSAASignalsFound()
        {
            var entry = Game.activeGame.journals.FindEntryByType<SAASignalsFound>(-1, true);
            if (entry != null)
            {
                this.Signals = new List<ScanSignal>(entry.Signals);
                this.Genuses = new List<ScanGenus>(entry.Genuses);
            }
        }
    }
}
