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

            new PlotGroundTarget(Game.activeGame.nearBody, latLong).Show();
        }
    }
}
