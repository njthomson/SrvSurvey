using SrvSurvey.game;
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

namespace SrvSurvey
{
    public partial class FormSetKeyChord : Form
    {
        public string keyChord { get => textChord.Text; }

        public FormSetKeyChord(string keyChord)
        {
            InitializeComponent();
            this.Icon = Icons.spanner;
            textChord.Text = keyChord;
            textChord.SelectionStart = 0;
            textChord.SelectionLength = 0;

            //btnAccept.Text += "abcdefg xx !";
            //btnDisable.Text += "abcdefg xx !";
            //btnCancel.Text += "abcdefg xx !";

            // force form width as it doesn't happen correctly by itself
            this.Width = flowButtons.Right + (flowButtons.Left * 2) + 4;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void FormSetKeyChord_KeyDown(object sender, KeyEventArgs e)
        {
            processKeyDown(e.KeyData, e.KeyCode, e.Alt, e.Control, e.Shift);
            e.SuppressKeyPress = true;
        }

        private void processKeyDown(Keys keyData, Keys keyCode, bool alt, bool ctrl, bool shift)
        { 
            // require a non ALT/CTRL/SHIFT key to be pressed
            btnAccept.Enabled = isValidChord(keyCode);
            //lblInvalid.Visible = !btnAccept.Enabled;

            // Pressing Enter or Escape should close the form and NOT be accepted as a chord
            if (keyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                return;
            }
            else if (keyCode == Keys.Enter && btnAccept.Enabled)
            {
                this.DialogResult = DialogResult.OK;
                return;
            }

            //Game.log($"{e.KeyData} / {e.KeyCode} / {e.SuppressKeyPress}");
            textChord.Text = KeyChords.getKeyChordString(keyCode, alt, ctrl, shift);
        }

        private bool isValidChord(Keys key)
        {
            if (key == Keys.Escape)
                return false;

            // Should we disallow duplicate settings?

            return KeyChords.keyToString(key) != null;
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            textChord.Text = null;
            btnAccept.Enabled = true;
            btnAccept.PerformClick();
        }

        private void textChord_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            processKeyDown(e.KeyData, e.KeyCode, e.Alt, e.Control, e.Shift);
        }
    }
}
