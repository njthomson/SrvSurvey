using SrvSurvey.Properties;
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

namespace SrvSurvey.forms
{
    internal partial class FormRoutePlotter : SizableForm
    {
        public FormRoutePlotter()
        {
            InitializeComponent();
            this.Icon = Icons.logo;
            this.textBox2.Text = "two two";
            this.btnClose.Text += "x";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            textBox2.UseClearButton = !textBox2.UseClearButton;

            btnPaste.FlatAppearance.MouseDownBackColor = Color.Purple;
            btnPaste.FlatAppearance.MouseOverBackColor = Color.Red;

        }
    }

    internal class ExRoute
    {
        public string name;
        public long address;
        public StarPos starPos;
        public string starClass;
    }
}
