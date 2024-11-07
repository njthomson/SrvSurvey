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
    public partial class FormRoutePlotter : SizableForm
    {
        public FormRoutePlotter()
        {
            InitializeComponent();
            this.Icon = DraggableForm.logo2;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
