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
    internal partial class FormGroundTarget : Form
    {
        public LatLong2 targetLatLong;

        public FormGroundTarget()
        {
            InitializeComponent();
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            var latLong = new LatLong2(
                double.Parse(txtLat.Text),
                double.Parse(txtLong.Text));

            try
            {
                this.targetLatLong = new LatLong2(
                    double.Parse(txtLat.Text),
                    double.Parse(txtLong.Text)
                    );
            }
            catch (Exception ex)
            {
                Game.log("Parse error: " + ex.Message);
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
