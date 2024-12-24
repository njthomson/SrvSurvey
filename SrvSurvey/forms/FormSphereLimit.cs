using SrvSurvey.forms;
using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.Properties;
using SrvSurvey.units;

namespace SrvSurvey
{
    [Draggable]
    internal partial class FormSphereLimit : FixedForm
    {
        private CommanderSettings cmdr;
        private StarPos? targetStarPos;

        public FormSphereLimit()
        {
            InitializeComponent();
            this.Icon = Icons.sphere;

            this.cmdr = CommanderSettings.LoadCurrentOrLast();
            txtCurrentSystem.Text = cmdr.currentSystem;

            // populate controls from settings
            numRadius.Value = (decimal)cmdr.sphereLimit.radius;
            comboSystemName.Text = cmdr.sphereLimit.centerSystemName;
            targetStarPos = cmdr.sphereLimit.centerStarPos ?? new StarPos();
            if (targetStarPos != null)
            {
                txtStarPos.Text = targetStarPos.ToString();
                var dist = Util.getSystemDistance(cmdr.starPos, targetStarPos).ToString("N2");
                txtCurrentDistance.Text = $"{dist}ly";
            }
            else
            {
                txtCurrentDistance.Text = $"-";
            }

            Util.applyTheme(this);
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            cmdr.sphereLimit.active = true;
            cmdr.sphereLimit.radius = (double)numRadius.Value;
            cmdr.sphereLimit.centerSystemName = comboSystemName.Text;
            cmdr.sphereLimit.centerStarPos = targetStarPos!;
            cmdr.Save();

            this.Close();
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            cmdr.sphereLimit.active = false;
            cmdr.Save();

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboSystemName_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.setTargetSystem(comboSystemName.SelectedSystem);
        }

        private void setTargetSystem(StarRef? targetSystem)
        {
            if (targetSystem != null)
            {
                targetStarPos = targetSystem.toStarPos();
                txtStarPos.Text = $"[ {targetSystem.x} , {targetSystem.y} , {targetSystem.z} ]";

                var dist = Util.getSystemDistance(cmdr.starPos, targetStarPos).ToString("N2");
                txtCurrentDistance.Text = $"{dist}ly";
            }
            else
            {
                txtStarPos.Text = "";
                targetStarPos = null;
                txtCurrentDistance.Text = $"-";
            }
        }

    }
}
