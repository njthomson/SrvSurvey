using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Text;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayJournal : SizableForm
    {
        private Game game => Game.activeGame!;

        public FormPlayJournal()
        {
            InitializeComponent();

            game.status.StatusChanged += Status_StatusChanged;
            game.journals!.onRawJournalEntry += Journals_onRawJournalEntry;

            Util.applyTheme(this, true, false);

            this.Status_StatusChanged(false);
            this.pullPriorEntries();
        }

        private void pullPriorEntries()
        {
            if (game.journals == null) return;

            var entries = game.journals.Entries.Slice(game.journals.Entries.Count - 40, 40).ToList();
            foreach (var entry in entries)
            {
                var obj = JObject.FromObject(entry);
                Journals_onRawJournalEntry(obj, 0);
            }
        }

        private void Status_StatusChanged(bool blink)
        {
            var str = new StringBuilder();

            if (game.status.Destination != null)
                str.AppendLine($"Destination: {game.status.Destination.Name} body:{game.status.Destination.Body} id64:{game.status.Destination.System}");

            if (game.status.Flags > 0)
                str.AppendLine($"Flags: {game.status.Flags}");
            if (game.status.Flags2 > 0)
                str.AppendLine($"Flags2: {game.status.Flags2}");

            str.AppendLine($"GuiFocus: {game.status.GuiFocus}, Pips: {game.status.Pips}, FireGroup: {game.status.FireGroup}");

            if (!string.IsNullOrWhiteSpace(game.status.BodyName))
                str.AppendLine($"BodyName: {game.status.BodyName}");

            if (game.status.hasLatLong)
                str.AppendLine($"Lat/Long: {new LatLong2(game.status)}, Heading: {game.status.Heading}°, Altitude: {game.status.Altitude}, Temp: {game.status.Temperature}");

            if (!string.IsNullOrWhiteSpace(game.status.SelectedWeapon))
                str.AppendLine($"SelectedWeapon: {game.status.SelectedWeapon} / {game.status.SelectedWeapon_Localised}");


            txtStatusFile.Text = str.ToString();
        }

        private void Journals_onRawJournalEntry(JObject obj, int index)
        {
            if (treeJournals.Nodes.Count >= 40)
                treeJournals.Nodes.RemoveAt(treeJournals.Nodes.Count - 1);

            if (treeJournals.Nodes.Count > 0)
                treeJournals.Nodes[0].Collapse();

            var timestamp = obj["timestamp"]?.ToObject<DateTime>();
            var node = new TreeNode($"{timestamp:yyyy-MM-dd HH:mm:ss} {obj["event"]}")
            {
                Tag = obj,
            };
            foreach (var prop in obj.Properties())
            {
                if (prop.Value.Type == JTokenType.Null) continue;
                if (prop.Value.Type == JTokenType.String && string.IsNullOrWhiteSpace($"{prop.Value}")) continue;
                var child = node.Nodes.Add($"{prop.Name}: {prop.Value}");

                if (prop.Value is JObject childObj)
                {
                    foreach (var childProp in childObj.Properties())
                    {
                        if (childProp.Value.Type == JTokenType.Null) continue;
                        if (childProp.Value.Type == JTokenType.String && string.IsNullOrWhiteSpace($"{childProp.Value}")) continue;
                        child.Nodes.Add($"{childProp.Name}: {childProp.Value}");
                    }
                }
                else if (prop.Value is JArray arr)
                {
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var arrItem = arr[i];
                        if (arrItem is JObject arrObj)
                        {
                            var arrNode = child.Nodes.Add($"[{i}]");
                            foreach (var arrProp in arrObj.Properties())
                            {
                                if (arrProp.Value.Type == JTokenType.Null) continue;
                                if (arrProp.Value.Type == JTokenType.String && string.IsNullOrWhiteSpace($"{arrProp.Value}")) continue;

                                arrNode.Nodes.Add($"{arrProp.Name}: {arrProp.Value}");
                            }
                        }
                        else
                        {
                            child.Nodes.Add($"[{i}] {arrItem}");
                        }
                    }
                }
            }

            node.Expand();
            treeJournals.Nodes.Insert(0, node);
        }

        private void btnReplay_Click(object sender, EventArgs e)
        {
            var node = treeJournals.SelectedNode;
            var obj = node?.Tag as JObject;
            if (obj == null) return;

            var entry = JournalFile.hydrate(obj)!;
            if (entry != null)
                game.cmdrPlay!.processJournalEntry(entry).justDoIt();
        }
    }
}
