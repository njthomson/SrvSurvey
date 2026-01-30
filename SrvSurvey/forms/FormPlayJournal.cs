using Newtonsoft.Json.Linq;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Text;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayJournal : SizableForm
    {
        private int maxCount = 120;

        private Game game => Game.activeGame!;
        private TreeNode? nodeLastParent;
        private HashSet<string> incAttrs = [];

        public FormPlayJournal()
        {
            InitializeComponent();

            Util.applyTheme(this, true, false);

            this.Status_StatusChanged(false);
            this.pullPriorEntries();
            txtNewCode.Text = "";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (game.status == null || game.journals == null)
            {
                this.Close();
                return;
            }

            game.status.StatusChanged += Status_StatusChanged;
            game.journals.onRawJournalEntry += Journals_onRawJournalEntry;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (Game.activeGame == null)
                this.Close();
        }

        private void pullPriorEntries()
        {
            if (game.journals == null) return;

            var count = Math.Min(maxCount, game.journals.Entries.Count);
            var entries = game.journals.Entries.Slice(game.journals.Entries.Count - count, count).ToList();
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
            if (treeJournals.Nodes.Count >= maxCount)
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
                child.Tag = prop.Name;

                if (prop.Value is JObject childObj)
                {
                    foreach (var childProp in childObj.Properties())
                    {
                        if (childProp.Value.Type == JTokenType.Null) continue;
                        if (childProp.Value.Type == JTokenType.String && string.IsNullOrWhiteSpace($"{childProp.Value}")) continue;
                        child.Nodes.Add($"{childProp.Name}:! {childProp.Value}")
                            .Tag = $"{prop.Name}.{childProp.Name}";
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
            if (treeJournals.SelectedNode == null) return;

            var parent = treeJournals.SelectedNode;
            while (parent.Parent != null)
                parent = parent.Parent;

            replayNode(parent.Tag as JObject);
        }

        private void replayNode(JObject? obj)
        {
            if (obj == null) return;

            var entry = JournalFile.hydrate(obj)!;
            if (entry != null)
                game.cmdrPlay?.processJournalEntry(entry).justDoIt();
        }

        private void treeJournals_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                generateCodeFragment(e.Node);
        }

        private void generateCodeFragment(TreeNode? node)
        {
            if (node?.Tag == null) return;

            // get parent node, with the journal entry
            var parent = node;
            while (parent.Parent != null)
                parent = parent.Parent;

            var obj = parent.Tag as JObject;
            if (obj == null) return;

            // reset attr's if parent changed
            if (parent != nodeLastParent) incAttrs.Clear();
            nodeLastParent = parent;

            var tagName = node.Tag as string;
            if (tagName != null && tagName != "event")
            {
                if (incAttrs.Contains(tagName))
                    incAttrs.Remove(tagName);
                else
                    incAttrs.Add(tagName);
            }

            // generate a suitably named function
            var txt = new StringBuilder();
            var eventName = obj["event"]?.ToString();
            txt.AppendLine($"function on_{eventName}(entry)");

            // inject an IF block for any selected attributes
            if (incAttrs.Any())
            {
                var clauses = incAttrs
                    .Select(x =>
                    {
                        var val = obj?.SelectToken("$." + x);
                        switch (val?.Type)
                        {
                            case JTokenType.String:
                                return $"entry.{x} == \"{val}\"";
                            case JTokenType.Boolean:
                            case JTokenType.Integer:
                            case JTokenType.Float:
                                return $"entry.{x} == {val}";
                        }

                        // ignore anything else
                        incAttrs.Remove(x);
                        return null;
                    })
                    .Where(x => x != null);

                txt.AppendLine($"  if {string.Join(" and ", clauses)} then");
                txt.AppendLine("    -- TODO: your code");
                txt.AppendLine("  end");
            }
            else
            {
                txt.AppendLine("  -- TODO: your code");
            }

            txt.AppendLine("end");
            txtNewCode.Text = txt.ToString();
        }

        private void btnCopyCode_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewCode.Text))
                Clipboard.SetText(txtNewCode.Text);
        }

        private void menuDev_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormPlayDev>();
        }

        private void menuComms_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormPlayComms>();
        }
    }
}
