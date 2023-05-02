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
        public FormGroundTarget()
        {
            InitializeComponent();
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            try
            {
                Game.settings.targetLatLong = new LatLong2(
                    double.Parse(txtLat.Text),
                    double.Parse(txtLong.Text)
                    );
                Game.settings.targetLatLongActive = true;
                Game.settings.Save();
            }
            catch (Exception ex)
            {
                Game.log("Parse error: " + ex.Message);
            }

            this.DialogResult = DialogResult.OK;
        }

        private void FormGroundTarget_Load(object sender, EventArgs e)
        {
            txtLat.Text = Game.settings.targetLatLong.Lat.ToString();
            txtLong.Text = Game.settings.targetLatLong.Long.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game.settings.targetLatLongActive = false;
            Game.settings.Save();
            this.DialogResult = DialogResult.OK;
        }

        private void btnTargetCurrent_Click(object sender, EventArgs e)
        {
            txtLat.Text = Status.here.Lat.ToString();
            txtLong.Text = Status.here.Long.ToString();
        }
    }
}
