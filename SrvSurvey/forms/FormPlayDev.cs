using Newtonsoft.Json;
using SrvSurvey.quests;
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
    [TrackPosition, Draggable]
    internal partial class FormPlayDev : SizableForm
    {
        public PlayQuest pq;

        public FormPlayDev()
        {
            InitializeComponent();
        }

        private void FormPlayDev_Load(object sender, EventArgs e)
        {
            txtTitle.Text = $"{pq.id}: {pq.quest.title}";
            txtStuff.Text = JsonConvert.SerializeObject(pq, Formatting.Indented);
        }

        private void btnTemp_Click(object sender, EventArgs e)
        {
            txtStuff.Text = JsonConvert.SerializeObject(pq, Formatting.Indented);
        }
    }
}
