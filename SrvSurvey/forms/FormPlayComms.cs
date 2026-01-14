using SrvSurvey.game;
using SrvSurvey.quests;
using SrvSurvey.widgets;
using System.Data;
using System.Drawing.Drawing2D;
using System.Text;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayComms : SizableForm
    {
        public PlayState cmdrPlay;
        public PlayQuest? selectedQuest;
        private bool expandQuest;
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

            expandQuest = false;
            list0.Items.Clear();
            tlist.Controls.Clear();
            foreach (var pq in cmdrPlay!.activeQuests)
            {
                list0.Items.Add($"{pq.quest.title}", pq.id, pq);
                var p = new Panel()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    AutoSize = false,
                    ForeColor = C.orange,
                    BackColor = Color.Black,
                    Padding = Padding.Empty,
                    Margin = Padding.Empty,
                };

                var btnP = new FlatButton()
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                    Text = "Pause",
                    Left = 10,
                    Visible = false,
                };
                btnP.Top = p.Height - btnP.Height - 2;
                p.Controls.Add(btnP);

                p.Click += (s, e) =>
                {
                    selectedQuest = pq;
                    expandQuest = !expandQuest;
                    tlist.Invalidate(true);
                    btnP.Visible = expandQuest;
                };
                p.Paint += (s, e) =>
                {
                    var tt = new TextCursor(e.Graphics, p);
                    P_Paint(e.Graphics, tt, pq);
                    p.Height = (int)(tt.dty + N.six) + (expandQuest ? btnP.Height : 0);
                };


                tlist.Controls.Add(p);
            }

            if (list0.Items.Count > 0)
                list0.Items[0].Selected = true;
        }

        private void P_Paint(Graphics g, TextCursor tt, PlayQuest pq)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            var selected = pq == selectedQuest;
            if (!selected && expandQuest) return;

            var c = selected && !expandQuest ? Color.Black : C.orange;
            g.Clear(selected && !expandQuest ? C.orange : Color.Black);
            tt.dty = N.four;

            if (expandQuest)
                tt.drawRight(tt.containerWidth - N.six, $"ID: {pq.quest.id}", C.orangeDark, GameColors.Fonts.gothic_7);

            tt.draw(N.eight, pq.quest.title, c, expandQuest ? GameColors.Fonts.gothic_16B : GameColors.Fonts.gothic_12B);
            tt.newLine(N.six);

            if (expandQuest)
            {
                g.SmoothingMode = SmoothingMode.None;
                g.DrawLine(C.Pens.orangeDark2, N.four, tt.dty - N.two, tt.containerWidth - N.six, tt.dty - N.two);
                g.SmoothingMode = SmoothingMode.HighQuality;
                tt.dty += N.two;
            }

            if (!pq.objectives.Any())
            {
                // no active objectives?
                tt.draw(N.eight, "► No active objectives", c, GameColors.Fonts.gothic_9);
                tt.newLine(N.four);
            }
            else
            {
                if (expandQuest)
                {
                    tt.draw(N.eight, "Objectives:", C.orangeDark, GameColors.Fonts.gothic_9);
                    tt.newLine(N.four);
                }

                // active objectives
                var p1 = /* highlight ? C.Pens.black1 : */ C.Pens.orangeDark1;
                var b = /* highlight ? C.Brushes.black : */C.Brushes.orangeDark;
                foreach (var (key, obj) in pq.objectives)
                {
                    var cc = c;
                    if (!expandQuest && obj.state != PlayObjective.State.visible) continue;
                    var prefix = " ▶";
                    if (obj.state == PlayObjective.State.complete) { prefix = "✔️"; } //🔹";
                    else if (obj.state == PlayObjective.State.failed) { prefix = "❌"; cc = C.orangeDark; }
                    tt.draw(N.eight, prefix, cc, GameColors.Fonts.gothic_9);
                    tt.draw(N.twoSix, pq.quest.strings.GetValueOrDefault(key, key), cc, GameColors.Fonts.gothic_9);
                    tt.newLine();

                    if (obj.total > 0)
                    {
                        tt.dty -= N.three;
                        var sz = tt.draw(N.fourForty, $"{obj.current} / {obj.total}", c, GameColors.Fonts.gothic_9);
                        tt.dty += N.three;
                        var w = N.fourHundred;
                        g.DrawRectangle(p1, N.twoSix, tt.dty, w + 5, N.ten);
                        w = w / obj.total * obj.current;
                        g.SmoothingMode = SmoothingMode.None;
                        g.FillRectangle(b, N.twoSix + 3, tt.dty + 3, w, N.ten - 5);
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        tt.dty += N.oneFour;
                    }
                }
            }

            if (!expandQuest) return;
            tt.dty += N.ten;

            tt.draw(N.eight, "Description:", C.orangeDark, GameColors.Fonts.gothic_9);
            tt.newLine(N.four);

            tt.drawWrapped(N.eight, pq.quest.desc, c, GameColors.Fonts.gothic_9);
            tt.newLine(N.ten);

            g.SmoothingMode = SmoothingMode.None;
            g.DrawLine(C.Pens.orangeDark2, N.four, tt.dty, tt.containerWidth - N.six, tt.dty);
            g.SmoothingMode = SmoothingMode.HighQuality;
            tt.dty += N.ten;
        }

        private void btnMsgs_Click(object sender, EventArgs e)
        {
            this.mode = "msgs";
            showMsgs();
        }

        private void showMsgs()
        {
            if (!(cmdrPlay.activeQuests.Count > 0)) return;
            list0.Items.Clear();

            foreach (var pq in cmdrPlay.activeQuests)
            {
                foreach (var msg in pq.msgs.OrderByDescending(m => m.received))
                {
                    var item = list0.Items.Add($"[{pq.id}:{msg.id}]{msg.subject ?? msg.body}", msg.id, msg);
                    item.SubItems[0].Tag = pq;
                }

                if (list0.Items.Count > 0)
                    list0.Items[0].Selected = true;
            }
        }

        private void list0_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list0.SelectedItems.Count == 0) return;

            if (mode == "quests")
            {
                selectedQuest = list0.SelectedItems[0].Tag as PlayQuest;
                showQuest(selectedQuest);
            }
            else
            {
                selectedQuest = list0.SelectedItems[0].SubItems[0].Tag as PlayQuest;
                showMsg(selectedQuest, list0.SelectedItems[0].Tag as PlayMsg);
            }
            tlist.Invalidate(true);
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
