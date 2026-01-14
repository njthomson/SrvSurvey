using SrvSurvey.game;
using SrvSurvey.quests;
using System.Data;
using System.Text;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayComms : SizableForm
    {
        public PlayState cmdrPlay;
        private string mode = "quests";

        public FormPlayComms()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (cmdrPlay != null)
            {
                // no op
                showQuests();
            }
            else if (Game.activeGame?.cmdrPlay == null)
            {
                this.setChildrenEnabled(false);
                PlayState.load(CommanderSettings.currentOrLastFid).continueOnMain(this, rslt =>
                {
                    this.cmdrPlay = rslt;
                    showQuests();
                    this.setChildrenEnabled(true);
                });
            }
            else
            {
                this.cmdrPlay = Game.activeGame.cmdrPlay;
                showQuests();
            }
        }

        public void onQuestChanged(PlayQuest pq)
        {
            if (mode == "quests")
                showQuest(pq);
            else
                showMsgs();
        }

        private void btnQuests_Click(object sender, EventArgs e)
        {
            this.mode = "quests";
            showQuests();
        }

        private void showQuests()
        {
            if (!(cmdrPlay.activeQuests.Count > 0)) return;

            list.Items.Clear();
            foreach (var pq in cmdrPlay!.activeQuests)
                list.Items.Add($"{pq.quest.title}", pq.id, pq);

            if (list.Items.Count > 0)
                list.Items[0].Selected = true;
        }

        private void btnMsgs_Click(object sender, EventArgs e)
        {
            this.mode = "msgs";
            showMsgs();
        }

        private void showMsgs()
        {
            if (!(cmdrPlay.activeQuests.Count > 0)) return;
            list.Items.Clear();

            foreach (var pq in cmdrPlay.activeQuests)
            {
                foreach (var msg in pq.msgs.OrderByDescending(m => m.received))
                {
                    var item = list.Items.Add($"[{pq.id}:{msg.id}]{msg.subject ?? msg.body}", msg.id, msg);
                    item.SubItems[0].Tag = pq;
                }

                if (list.Items.Count > 0)
                    list.Items[0].Selected = true;
            }
        }

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list.SelectedItems.Count == 0) return;

            if (mode == "quests")
                showQuest(list.SelectedItems[0].Tag as PlayQuest);
            else
                showMsg(list.SelectedItems[0].SubItems[0].Tag as PlayQuest, list.SelectedItems[0].Tag as PlayMsg);
        }

        private void showQuest(PlayQuest? pq)
        {
            if (pq == null) return;

            var txt = new StringBuilder();
            txt.AppendLine($"'{pq.quest.title}' ({pq.id})");
            txt.AppendLine();
            txt.AppendLine($"Objectives:");
            foreach (var key in pq.quest.objectives.Keys)
            {
                var po = pq.objectives.GetValueOrDefault(key);
                var state = po == null ? "?" : $"{po.state} {po.current} of {po.total}".ToUpper();
                txt.AppendLine($"> {key} : [{state}] {pq.quest.objectives[key]}");
            }

            txt.AppendLine();
            txt.AppendLine($"Description:");
            txt.AppendLine(pq.quest.desc);

            txtStuff.Text = txt.ToString();
        }

        private void showMsg(PlayQuest? pq, PlayMsg? pm)
        {
            if (pq == null || pm == null) return;

            var msg = pq.quest.msgs.Find(m => m.id == pm.id);

            var txt = new StringBuilder();
            txt.AppendLine($"From:       {pm.from ?? msg?.from}");
            var subject = pm.subject ?? msg?.subject;
            if (subject != null) txt.AppendLine($"Subject:   {subject}");
            txt.AppendLine("--------------------------------------------------------");
            txt.AppendLine(pm.body ?? msg?.body);
            txt.AppendLine("--------------------------------------------------------");

            if (pm.actions != null)
                txt.AppendLine($"actions: {pm.actions}");

            txtStuff.Text = txt.ToString();
        }
    }
}
