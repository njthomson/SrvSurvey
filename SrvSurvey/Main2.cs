using SrvSurvey.forms;
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
    [Draggable]
    internal partial class Main2 : FixedForm
    {
        public Main2()
        {
            InitializeComponent();
            Util.applyTheme(this);
        }

        private void flatButton9_Click(object sender, EventArgs e)
        {
            var mv = new MainView();
            mv.Dock = DockStyle.Fill;
            this.Controls.Add(mv);
        }
    }
}
