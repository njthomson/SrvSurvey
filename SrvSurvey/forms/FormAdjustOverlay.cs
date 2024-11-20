using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey.forms
{
    public partial class FormAdjustOverlay : Form
    {
        /// <summary> The name of the current plotter getting adjusted, or null if none </summary>
        public static string? targetName;

        private Control[] enablementControls;

        private bool changing = false;

        public FormAdjustOverlay()
        {
            PlotPos.backup();

            InitializeComponent();
            this.Icon = Icons.spanner;

            this.enablementControls = new Control[] {
                btnReset,
                checkBottom,
                checkCenter,
                checkHScreen,
                checkLeft,
                checkMiddle,
                checkRight,
                checkTop,
                checkVScreen,
                labelOffset,
                numX,
                numY,
                checkOpacity,
                numOpacity,
            };

            this.prepPlotters();
            comboPlotter.SelectedIndex = 0;

            resetForm();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Program.defer(() => this.Activate());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            targetName = null;
            PlotPos.restore();
            Program.invalidateActivePlotters();
        }

        private void resetForm()
        {
            // reset controls ...
            foreach (var ctrl in enablementControls)
            {
                ctrl.Enabled = false;
                if (ctrl is CheckBox) ((CheckBox)ctrl).Checked = false;
            }

            numX.Value = 0;
            numY.Value = 0;
        }

        private void prepPlotters()
        {
            targetName = null;

            var txt = comboPlotter.Text;

            while (comboPlotter.Items.Count > 1)
                comboPlotter.Items.RemoveAt(1);

            comboPlotter.Items.AddRange(Program.getAllPlotterNames());

            if (comboPlotter.Items.Contains(txt))
                comboPlotter.Text = txt;

            Program.invalidateActivePlotters();
        }

        private void selectPlotter()
        {
            if (!this.Created || this.IsDisposed || targetName == null) return;

            var pp = PlotPos.get(targetName);
            if (pp == null) return;

            checkLeft.Checked = pp.h == PlotPos.Horiz.Left;
            checkCenter.Checked = pp.h == PlotPos.Horiz.Center;
            checkRight.Checked = pp.h == PlotPos.Horiz.Right;
            checkHScreen.Checked = pp.h == PlotPos.Horiz.OS;

            checkTop.Checked = pp.v == PlotPos.Vert.Top;
            checkMiddle.Checked = pp.v == PlotPos.Vert.Middle;
            checkBottom.Checked = pp.v == PlotPos.Vert.Bottom;
            checkVScreen.Checked = pp.v == PlotPos.Vert.OS;

            var er = Elite.getWindowRect();

            numX.Maximum = 10_000; // er.Width;
            numY.Maximum = 10_000; // er.Height;

            var plotter = Program.getPlotter(targetName);
            if (plotter == null) return;

            var pt = PlotPos.getPlotterLocation(targetName, plotter.Size, er);

            numX.Value = pp.x;
            numY.Value = pp.y;

            checkOpacity.Checked = pp.opacity.HasValue;
            numOpacity.Enabled = checkOpacity.Checked;
            numOpacity.Value = (decimal)(pp.opacity.HasValue ? pp.opacity.Value : Game.settings.Opacity) * 100;

            // populate controls...
            foreach (var ctrl in enablementControls) ctrl.Enabled = true;
            numOpacity.Enabled = checkOpacity.Checked;

            Program.invalidateActivePlotters();
        }

        private void comboPlotter_DropDown(object sender, EventArgs e)
        {
            this.prepPlotters();
        }

        private void comboPlotter_SelectedIndexChanged(object sender, EventArgs e)
        {
            changing = true;

            if (comboPlotter.SelectedIndex == 0)
            {
                targetName = null;
                this.resetForm();
            }
            else
            {
                targetName = comboPlotter.Text;
                this.selectPlotter();
            }

            changing = false;
            Program.invalidateActivePlotters();
        }

        private void checkIfMe(CheckBox sender, params CheckBox[] boxes)
        {
            if (changing) return;
            changing = true;

            foreach (var box in boxes)
                box.Checked = box == sender;

            changing = false;
        }

        private bool disabled(Object obj)
        {
            var ctrl = obj as Control;
            return ctrl?.Enabled == false;
        }

        private void checkHorizontal_CheckedChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var box = (CheckBox)sender;
            checkIfMe(box, checkLeft, checkCenter, checkRight, checkHScreen);

            var pp = PlotPos.get(targetName);
            if (pp == null) return;

            pp.h = Enum.Parse<PlotPos.Horiz>(box.Tag?.ToString()!);

            Program.repositionPlotters();
        }

        private void checkVertical_CheckedChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var box = (CheckBox)sender;
            checkIfMe((CheckBox)sender, checkTop, checkMiddle, checkBottom, checkVScreen);

            var pp = PlotPos.get(targetName);
            if (pp == null) return;

            pp.v = Enum.Parse<PlotPos.Vert>(box.Tag?.ToString()!);

            Program.repositionPlotters();
        }

        private void num_ValueChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var pp = PlotPos.get(targetName);
            if (targetName == null || pp == null) return;

            pp.x = (int)numX.Value;
            pp.y = (int)numY.Value;

            Program.repositionPlotters();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            var pp = PlotPos.get(targetName);
            if (targetName == null || pp == null) return;

            groupBox1.Enabled = false;
            changing = true;

            Application.DoEvents();

            PlotPos.resetToDefault(targetName);
            selectPlotter();

            Program.repositionPlotters();

            groupBox1.Enabled = true;
            changing = false;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            PlotPos.saveCustomPositions();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            PlotPos.restore();

            Program.repositionPlotters();
            Program.invalidateActivePlotters();

            this.Close();
        }

        private void checkOpacity_CheckedChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;
            var pp = PlotPos.get(targetName);
            if (pp == null) return;

            if (checkOpacity.Checked)
            {
                numOpacity.Enabled = true;
                numOpacity.Value = (decimal)(pp.opacity.HasValue ? pp.opacity.Value : Game.settings.Opacity) * 100;
            }
            else
            {
                numOpacity.Enabled = false;
                numOpacity.Value = (decimal)Game.settings.Opacity * 100;
                pp.opacity = null;
            }

            Program.repositionPlotters();
        }

        private void numOpacity_ValueChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var pp = PlotPos.get(targetName);
            if (targetName == null || pp == null) return;

            pp.opacity = (float)numOpacity.Value / 100f;

            Program.repositionPlotters();
        }
    }
}
