using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SrvSurvey.forms
{
    [Draggable, TrackPosition]
    internal partial class FormBoxelSearch : SizableForm
    {

        public FormBoxelSearch()
        {
            InitializeComponent();
            txtSystemName.Text = "Thuechu YV-T d4-44";
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSystemName.Text)) return;
            var systemName = SystemName.parse(txtSystemName.Text);
            if (systemName.generatedName)
                searchSystems(systemName);
        }

        private void searchSystems(SystemName systemName)
        {
            btnSearch.Enabled = false;
            txtNext.Text = "";
            list.Items.Clear();

            var query = systemName.prefix + "*";
            Game.spansh.getBoxelSystems(query).continueOnMain(this, response =>
            {
                list.Items.Clear();

                if (response.results.Count > 0)
                {
                    foreach (var item in response.results)
                        list.Items.Add(item.name);

                    var last = SystemName.parse(response.results.Last().name);
                    lblMaxNum.Text = $"Last: {last.num}";
                }
                else
                {
                    lblMaxNum.Text = $"Last: -";
                }
                btnSearch.Enabled = true;
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < list.Items.Count; n++)
            {
                if (list.GetItemChecked(n)) continue;

                txtNext.Text = list.Items[n].ToString();
                break;
            }
        }
    }

    public class SystemName
    {
        // See https://forums.frontier.co.uk/threads/marxs-guide-to-boxels-subsectors.618286/ or http://disc.thargoid.space/Sector_Naming

        private static Regex nameParts = new Regex(@"(.+) (\w\w-\w) (\w)(\d+)-?(\d+)?$", RegexOptions.Singleline);

        public static SystemName parse(string systemName)
        {
            var parts = nameParts.Match(systemName);

            // not a match
            if (parts.Groups.Count != 5 && parts.Groups.Count != 6)
                return new SystemName { name = systemName, generatedName = false };

            var name = new SystemName
            {
                name = systemName,
                generatedName = true,
                sector = parts.Groups[1].Value,
                subSector = parts.Groups[2].Value,
                massCode = parts.Groups[3].Value,
                id = int.Parse(parts.Groups[4].Value),
                //num = int.Parse(parts.Groups[5].Value),
            };

            if (parts.Groups.Count == 6 && parts.Groups[5].Success)
                name.num = int.Parse(parts.Groups[5].Value);

            return name;
        }

        /// <summary> The whole name </summary>
        public string name;

        public bool generatedName;

        public string sector;
        public string subSector;
        public string massCode;
        public int id;
        public int num;

        public string prefix { get => $"{sector} {subSector} {massCode}{id}-"; }

        public override string ToString()
        {
            return name;
        }
    }
}
