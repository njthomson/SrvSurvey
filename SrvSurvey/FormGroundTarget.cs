using SrvSurvey.game;
using SrvSurvey.units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    public partial class FormGroundTarget : Form
    {
        public FormGroundTarget()
        {
            InitializeComponent();
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            var latLong = new LatLong2(
                double.Parse(txtLat.Text),
                double.Parse(txtLong.Text));

            if (Game.activeGame.nearBody != null)
            {
                // if near a body
                new PlotTrackTarget(Game.activeGame.nearBody, latLong).Show();
            }
            else
            {
                // not near a body
                Game.log("Cannot show GroundTarget plotter - not near any body.");
            }
        }
    }
}
