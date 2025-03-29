namespace SrvSurvey.forms
{
    public partial class FormBuildAddCmdr : Form
    {
        public FormBuildAddCmdr(string? cmdr)
        {
            InitializeComponent();
            txtCmdr.Text = cmdr;
        }

        public string commander => txtCmdr.Text;

        private void btnAdd_Click(object sender, EventArgs e)
        {

        }
    }
}
