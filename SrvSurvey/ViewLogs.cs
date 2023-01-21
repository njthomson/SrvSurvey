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
    public partial class ViewLogs : Form
    {
        public static void show(StringBuilder logs)
        {
            var form = new ViewLogs(logs);
            form.Show();
        }

        public StringBuilder logs;

        private ViewLogs(StringBuilder logs)
        {
            this.logs = logs;

            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.logs.Clear();

            var txt = DateTime.Now.ToString("HH:mm:ss") + ": Logs reset";
            this.logs.AppendLine(txt);

            txtLogs.Text = this.logs.ToString();
            txtLogs.SelectionStart = txtLogs.Text.Length;
        }

        private void ViewLogs_Load(object sender, EventArgs e)
        {
            txtLogs.Text = this.logs.ToString();
            txtLogs.SelectionStart = txtLogs.Text.Length;

        }
    }
}
