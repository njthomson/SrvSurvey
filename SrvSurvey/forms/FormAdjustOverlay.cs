using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.ComponentModel;

namespace SrvSurvey.forms
{
    [Draggable]
    internal partial class FormAdjustOverlay : FixedForm
    {
        /// <summary> The name of the current plotter getting adjusted, or null if none </summary>
        public static string? targetName;

        private Control[] enablementControls;

        private bool changing = false;

        public FormAdjustOverlay()
        {
            PlotPos.backup();
            this.renderOwnTitleBar = false;

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

            if (fake != null) fake.Close();

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

            if (fake != null)
            {
                fake.Close();
                fake = null;
            }
        }

        private void prepPlotters()
        {
            targetName = null;

            var txt = comboPlotter.Text;

            // all names, or only those visible?
            var names = checkShowAll.Checked
                ? PlotPos.getAllPlotterNames()
                : Program.getAllPlotterNames();

            for (var n = 0; n < names.Length; n++)
            {
                var item = comboPlotter.Items.Count > n + 1 ? comboPlotter.Items[n + 1]!.ToString() : "";
                if (item != names[n])
                    comboPlotter.Items.Insert(n + 1, names[n]);
            }

            while (comboPlotter.Items.Count > names.Length + 1)
                comboPlotter.Items.RemoveAt(names.Length + 1);

            if (comboPlotter.Items.Contains(txt) && comboPlotter.Text != txt)
                comboPlotter.Text = txt;

            if (comboPlotter.SelectedIndex == -1)
                comboPlotter.SelectedIndex = 0;

            if (comboPlotter.SelectedIndex == 0)
                resetForm();

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
            if (plotter == null)
            {
                createFakePlotter(targetName);
            }
            else if (fake != null)
            {
                fake.Close();
                fake = null;
            }

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

        private FakePlotter? fake;

        private void createFakePlotter(string name)
        {
            fake ??= new();

            fake.Name = name;
            fake.Size = PlotPos.getLastSize(name);
            fake.Location = PlotPos.getPlotterLocation(name, fake.Size, Rectangle.Empty);
            fake.BackgroundImage = GameGraphics.getBackgroundImage(fake.Size, true);

            fake.Show();
            Program.defer(() => this.Activate());
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

        private void repositionPlotters()
        {
            Program.repositionPlotters();
            if (fake != null)
                fake.Location = PlotPos.getPlotterLocation(fake.Name, fake.Size, Rectangle.Empty);
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

            this.repositionPlotters();
            if (fake != null)
                fake.Location = PlotPos.getPlotterLocation(fake.Name, fake.Size, Rectangle.Empty);
        }

        private void checkVertical_CheckedChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var box = (CheckBox)sender;
            checkIfMe((CheckBox)sender, checkTop, checkMiddle, checkBottom, checkVScreen);

            var pp = PlotPos.get(targetName);
            if (pp == null) return;

            pp.v = Enum.Parse<PlotPos.Vert>(box.Tag?.ToString()!);

            this.repositionPlotters();
            if (fake != null)
                fake.Location = PlotPos.getPlotterLocation(fake.Name, fake.Size, Rectangle.Empty);
        }

        private void num_ValueChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var pp = PlotPos.get(targetName);
            if (targetName == null || pp == null) return;

            pp.x = (int)numX.Value;
            pp.y = (int)numY.Value;

            this.repositionPlotters();
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

            this.repositionPlotters();

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

            this.repositionPlotters();
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

            this.repositionPlotters();
        }

        private void numOpacity_ValueChanged(object sender, EventArgs e)
        {
            if (changing || disabled(sender)) return;

            var pp = PlotPos.get(targetName);
            if (targetName == null || pp == null) return;

            pp.opacity = (float)numOpacity.Value / 100f;

            this.repositionPlotters();
        }

        private void checkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            this.prepPlotters();
        }

        class FakePlotter : Form
        {
            public FakePlotter()
            {
                this.BackColor = Color.Black;
                this.ShowIcon = false;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.FormBorderStyle = FormBorderStyle.None;
                this.TopMost = true;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ControlBox = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Opacity = 0.7f; // ok
                this.ResizeRedraw = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                var p = GameColors.penYellow4;
                // draw outer yellow box and the plotter's name
                e.Graphics.DrawRectangle(p, p.Width / 2, p.Width / 2, this.Width - p.Width, this.Height - p.Width);
                BaseWidget.renderText(e.Graphics, this.Name, 10, 10, GameColors.Fonts.gothic_10);
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x00000020 + 0x00080000 + 0x08000000; // WS_EX_TRANSPARENT + WS_EX_LAYERED + WS_EX_NOACTIVATE
                    return cp;
                }
            }
        }
    }
}
