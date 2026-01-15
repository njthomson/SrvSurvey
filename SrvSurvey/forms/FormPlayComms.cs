using SrvSurvey.game;
using SrvSurvey.plotters;
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
        public PlayMsg? selectedMsg;
        public bool expandSelected;
        private string mode = "msgs";

        public FormPlayComms()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
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
                    showMsgs();
                    //showQuests();
                    this.setChildrenEnabled(true);
                });
            }
            else
            {
                this.cmdrPlay = Game.activeGame.cmdrPlay;
                showMsgs();
                //showQuests();
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

            expandSelected = false;
            list0.Items.Clear();
            tlist.Controls.Clear();
            tlist.RowStyles.Clear();

            foreach (var pq in cmdrPlay!.activeQuests)
            {
                list0.Items.Add($"{pq.quest.title}", pq.id, pq);
                tlist.Controls.Add(new PanelQuest(this, pq), 0, tlist.Controls.Count - 1);
                tlist.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            if (list0.Items.Count > 0)
                list0.Items[0].Selected = true;
        }

        public void selectQuest(PlayQuest? pq, bool expand = false)
        {
            selectedQuest = pq;
            selectedMsg = null;
            expandSelected = expand;
            tlist.Invalidate(true);
        }

        public void selectMessage(PlayMsg? msg, bool expand = false)
        {
            selectedMsg = msg;
            selectedQuest = null;
            expandSelected = expand;

            // remember this message has now been read
            if (msg?.read == false)
            {
                msg.read = true;
                cmdrPlay.Save();
            }

            tlist.Invalidate(true);

        }

        private void btnMsgs_Click(object sender, EventArgs e)
        {
            this.mode = "msgs";
            expandSelected = false;
            showMsgs();
        }

        private void showMsgs()
        {
            if (!(cmdrPlay.activeQuests.Count > 0)) return;
            list0.Items.Clear();
            tlist.Controls.Clear();
            tlist.RowStyles.Clear();

            var allMsgs = cmdrPlay.activeQuests
                .SelectMany(q => q.msgs)
                .OrderByDescending(m => m.received)
                .ToList();
            Game.log($"showing {allMsgs.Count} msgs");

            foreach (var msg in allMsgs)
            {
                tlist.Controls.Add(new PanelMsg(this, msg.parent, msg), 0, tlist.Controls.Count);
                tlist.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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

    class PanelQuest : Panel
    {
        public readonly FormPlayComms form;
        public readonly PlayQuest pq;

        public PanelQuest(FormPlayComms form, PlayQuest pq)
        {
            this.form = form;
            this.pq = pq;
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;
            this.ForeColor = C.orange;
            this.BackColor = Color.Black;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.ResizeRedraw = true;
            this.DoubleBuffered = true;

            addButtons();
        }

        private void addButtons()
        {
            var btnB = new FlatButton()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Text = "Back",
                Left = 6,
                Visible = false,
            };
            btnB.Top = this.Height - btnB.Height - 6;
            btnB.Click += BtnB_Click;
            this.Controls.Add(btnB);

            var btnP = new FlatButton()
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Text = "Pause",
                Visible = false,
            };
            btnP.Left = this.Width - btnP.Width - 6;
            btnP.Top = this.Height - btnP.Height - 6;
            this.Controls.Add(btnP);
        }

        private void BtnB_Click(object? sender, EventArgs e)
        {
            form.selectQuest(null, false);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            form.selectQuest(pq, !form.expandSelected);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var tt = new TextCursor(e.Graphics, this);

            var selected = form.selectedQuest == pq;
            foreach (Control child in Controls) child.Visible = selected && form.expandSelected;

            if (!form.expandSelected)
                this.Height = drawCollapsed(g, tt, selected);
            else if (selected)
                this.Height = drawExpanded(g, tt);
            else
                this.Height = 1;
        }

        int drawCollapsed(Graphics g, TextCursor tt, bool selected)
        {
            var c = selected ? Color.Black : C.orange;
            g.Clear(selected ? C.orange : Color.Black);
            tt.dty = N.four;

            // title
            tt.draw(N.eight, pq.quest.title, c, GameColors.Fonts.gothic_12B);
            tt.newLine(N.six);

            // visible objectives
            if (pq.objectives.Any())
            {
                foreach (var (key, obj) in pq.objectives)
                    if (obj.state == PlayObjective.State.visible)
                        drawObjective(g, tt, c, key, obj);
            }
            else
            {
                // no active objectives?
                tt.draw(N.eight, "► No objectives", c, GameColors.Fonts.gothic_9);
                tt.newLine(N.four);
            }

            return (int)(tt.dty + N.six);
        }

        void drawObjective(Graphics g, TextCursor tt, Color c, string key, PlayObjective obj)
        {
            var cc = c;

            var prefix = " ▶";
            if (obj.state == PlayObjective.State.complete) { prefix = "✔️"; }
            else if (obj.state == PlayObjective.State.failed) { prefix = "❌"; cc = C.orangeDark; }
            tt.draw(N.eight, prefix, cc, GameColors.Fonts.gothic_9);
            tt.draw(N.twoSix, pq.quest.strings.GetValueOrDefault(key, key), cc);
            tt.newLine();

            if (obj.total > 0)
            {
                tt.dty -= N.three;
                var sz = tt.draw(N.fourForty, $"{obj.current} / {obj.total}", c);
                tt.dty += N.three;
                var w = N.fourHundred;

                g.SmoothingMode = SmoothingMode.None;
                g.DrawRectangle(C.Pens.orangeDark1, N.twoSix, tt.dty, w + 5, N.ten);

                w = w / obj.total * obj.current;
                g.FillRectangle(C.Brushes.orangeDark, N.twoSix + 3, tt.dty + 3, w, N.ten - 5);
                g.SmoothingMode = SmoothingMode.HighQuality;
                tt.dty += N.oneFour;
            }
        }

        int drawExpanded(Graphics g, TextCursor tt)
        {
            var c = C.orange;
            g.Clear(Color.Black);
            tt.dty = N.four;
            tt.font = GameColors.Fonts.gothic_10;

            // ID + title
            tt.drawRight(tt.containerWidth - N.six, $"ID: {pq.quest.id}", C.orangeDark, GameColors.Fonts.gothic_7);
            tt.draw(N.eight, pq.quest.title, c, GameColors.Fonts.gothic_16B);
            tt.newLine(N.six);

            // top line
            g.SmoothingMode = SmoothingMode.None;
            g.DrawLine(C.Pens.orangeDark2, N.four, tt.dty - N.two, tt.containerWidth - N.six, tt.dty - N.two);
            g.SmoothingMode = SmoothingMode.HighQuality;
            tt.dty += N.two;


            // objectives
            tt.draw(N.eight, "Objectives:", C.orangeDark);
            tt.newLine(N.four);
            if (pq.objectives.Any())
            {
                foreach (var (key, obj) in pq.objectives)
                    drawObjective(g, tt, c, key, obj);
            }
            else
            {
                tt.draw(N.eight, "► No active objectives");
                tt.newLine(N.four);
            }

            tt.dty += N.ten;

            tt.draw(N.eight, "Description:", C.orangeDark);
            tt.newLine(N.four);

            tt.drawWrapped(N.eight, pq.quest.desc, c);
            tt.newLine(N.ten);

            g.SmoothingMode = SmoothingMode.None;
            g.DrawLine(C.Pens.orangeDark2, N.four, tt.dty, tt.containerWidth - N.six, tt.dty);
            g.SmoothingMode = SmoothingMode.HighQuality;
            tt.dty += N.ten;

            return (int)(tt.dty + N.six) + Controls[0].Height;
        }
    }

    class PanelMsg : Panel
    {
        public readonly FormPlayComms form;
        public readonly PlayQuest pq;
        public readonly PlayMsg pm;
        public readonly Msg? qm;

        public PanelMsg(FormPlayComms form, PlayQuest pq, PlayMsg msg)
        {
            this.form = form;
            this.pq = pq;
            this.pm = msg;
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;
            this.ForeColor = C.orange;
            this.BackColor = Color.Black;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.ResizeRedraw = true;
            this.DoubleBuffered = true;

            this.qm = pq.quest.msgs.Find(m => m.id == pm.id)!;
            addButtons();
        }

        private void addButtons()
        {
            var btnB = new FlatButton()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Text = "Back",
                Left = 6,
                Visible = false,
            };
            btnB.Top = this.Height - btnB.Height - 6;
            btnB.Click += BtnB_Click;
            this.Controls.Add(btnB);

            var btnD = new FlatButton()
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Text = "Delete",
                Visible = false,
            };
            btnD.Left = this.Width - btnD.Width - 6;
            btnD.Top = this.Height - btnD.Height - 6;
            btnD.Click += BtnD_Click;
            this.Controls.Add(btnD);


            if (qm?.actions == null) return;

            foreach (var (key, txt) in qm.actions)
            {
                var btn = new FlatButton()
                {
                    Tag = key,
                    Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                    Text = $"◊ {txt}",
                    Left = 24,
                    AutoSize = true,
                    Visible = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    UseCompatibleTextRendering = true,
                };
                btn.Click += Btn_Click;
                this.Controls.Add(btn);
            }
        }

        private void BtnB_Click(object? sender, EventArgs e)
        {
            form.selectMessage(null, false);
        }

        private void BtnD_Click(object? sender, EventArgs e)
        {
            pq.deleteMsg(pm.id);
            pq.updateIfDirty();
            form.selectMessage(null, false);
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            var btn = sender as Button;
            var actionId = btn?.Tag as string;
            if (actionId == null) return;

            pq.invokeMessageAction(pm.chapter!, actionId);
            pq.updateIfDirty();
            form.selectMessage(pm, false);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            form.selectMessage(pm, !form.expandSelected);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var tt = new TextCursor(e.Graphics, this);
            tt.dty = N.four;

            var selected = form.selectedMsg == pm;
            foreach (Control child in Controls) child.Visible = selected && form.expandSelected;

            if (!form.expandSelected)
                this.Height = drawCollapsed(g, tt, selected);
            else if (selected)
                this.Height = drawExpanded(g, tt);
            else
                this.Height = 1;
        }

        int drawCollapsed(Graphics g, TextCursor tt, bool selected)
        {
            tt.color = pm.read ? C.orange : C.cyan;

            PlotQuestMini.drawMsgsN(g, 6, 10, 16, !pm.read);

            var time = pm.received.Subtract(DateTime.Now).TotalDays < 1
                ? pm.received.ToString("HH:mm")
                : pm.received.AddYears(1286).UtcDateTime.ToString("dd MMM yyyy - HH:mm");
            tt.drawRight(Width - 20, time, null, GameColors.Fonts.gothic_7);
            tt.draw(40, pm.from ?? qm?.from, GameColors.Fonts.gothic_8);
            tt.newLine(-4);

            var subject = pm.subject ?? qm?.subject ?? pm.body ?? qm?.body;
            tt.draw(40, subject, GameColors.Fonts.gothic_10);
            tt.newLine(4);

            return (int)tt.dty;
        }

        int drawExpanded(Graphics g, TextCursor tt)
        {
            tt.font = GameColors.Fonts.gothic_12B;
            tt.color = C.orange;

            PlotQuestMini.drawMsgsN(g, 6, 10, 24, false);

            tt.draw(64, "Sent:", GameColors.Fonts.gothic_8);
            tt.draw(120, pm.received.AddYears(1286).UtcDateTime.ToString("dd MMM yyyy - HH:mm"));
            tt.newLine();

            tt.draw(64, "From:", GameColors.Fonts.gothic_8);
            tt.draw(120, pm.from ?? qm?.from);
            tt.newLine();

            var subject = pm.subject ?? qm?.subject;
            if (subject != null)
            {
                tt.draw(64, "Subject:", GameColors.Fonts.gothic_8);
                tt.draw(120, subject);
                tt.newLine();
            }
            tt.dty += 4;

            // body with lines
            g.DrawLine(C.Pens.orangeDark2, 6, tt.dty, Width - 4, tt.dty);
            tt.dty += 6;
            tt.drawWrapped(12, Width - 12, pm.body ?? qm?.body, GameColors.Fonts.gothic_12);
            tt.newLine(6);
            g.DrawLine(C.Pens.orangeDark2, 6, tt.dty, Width - 4, tt.dty);
            tt.dty += 4;

            // any response actions?
            var actions = pm.actions ?? qm?.actions?.Keys.ToArray();
            if (actions != null)
            {
                tt.draw(12, "Respond:", C.orangeDark, GameColors.Fonts.gothic_10);
                tt.newLine(-4);
                foreach (Button btn in Controls)
                {
                    if (btn.Tag == null) continue;

                    btn.Top = (int)tt.dty + 8;
                    tt.dty += btn.Height + 8;
                }

                // final line
                tt.dty += 10;
                g.DrawLine(C.Pens.orangeDark2, 6, tt.dty, Width - 4, tt.dty);
                tt.dty += 4;
            }

            // allow space for buttons along the bottom
            tt.dty += 12 + Controls[0].Height;
            return (int)tt.dty;
        }
    }
}
