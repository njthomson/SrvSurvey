using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SrvSurvey
{
    public partial class FormSettings : Form
    {
        private Game game = Game.activeGame;
        private readonly Dictionary<string, FieldInfo> map = new Dictionary<string, FieldInfo>();


        public FormSettings()
        {
            InitializeComponent();

            // build a map of fields on the setting objects by name
            foreach (var fieldInfo in typeof(Settings).GetRuntimeFields())
                this.map.Add(fieldInfo.Name, fieldInfo);

        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            updateFormFromSettings(this);
        }

        private void updateFormFromSettings(Control parentControl)
        {
            foreach (Control ctrl in parentControl.Controls)
            {
                if (!string.IsNullOrWhiteSpace(ctrl.Tag?.ToString()))
                {
                    if (!map.ContainsKey(ctrl.Tag.ToString()))
                    {
                        throw new Exception($"Missing setting: {ctrl.Tag}");
                    }

                    var name = ctrl.Tag.ToString();
                    // Game.log($"Read setting: {name} => {map[name].GetValue(Game.settings)}");

                    switch (ctrl.GetType().Name)
                    {
                        case nameof(CheckBox):
                            ((CheckBox)ctrl).Checked = (bool)map[name].GetValue(Game.settings);
                            break;

                        case nameof(TextBox):
                            ((TextBox)ctrl).Text = (string)map[name].GetValue(Game.settings);
                            break;

                    }
                }

                // recurse if there are children
                if (ctrl.HasChildren)
                    updateFormFromSettings(ctrl);
            }
        }

        private void updateSettingsFromForm(Control parentControl)
        {
            foreach (Control ctrl in parentControl.Controls)
            {
                if (ctrl.Tag != null && map.ContainsKey(ctrl.Tag.ToString()))
                {
                    var name = ctrl.Tag.ToString();

                    object val = null;
                    switch (ctrl.GetType().Name)
                    {
                        case nameof(CheckBox):
                            val = ((CheckBox)ctrl).Checked;
                            break;

                        case nameof(TextBox):
                            val = ((TextBox)ctrl).Text;
                            break;

                    }

                    // Game.log($"Write setting: {name} => {val}");
                    map[name].SetValue(Game.settings, val);
                }

                // recurse if there are children
                if (ctrl.HasChildren)
                    updateSettingsFromForm(ctrl);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            updateSettingsFromForm(this);

            Game.settings.Save();

            this.DialogResult = DialogResult.OK;
        }

    }
}
