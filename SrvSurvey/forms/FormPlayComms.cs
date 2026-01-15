using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.quests;
using SrvSurvey.widgets;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey.forms
{
    [TrackPosition, Draggable]
    internal partial class FormPlayComms : SizableForm
    {
        public PlayState cmdrPlay;
        private Control? selectedThing;
        private string mode = "quests";
        private Control lastLeftBtn;
        public Control? lastTListItem;

        public FormPlayComms()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            btnQuests.BackColor = C.orangeDark;
            btnMsgs.BackColor = C.orangeDark;
            btnClose.BackColor = C.orangeDark;

            this.lastLeftBtn = btnQuests;

            btnDev.ForeColor = C.orange;
            btnWatch.ForeColor = C.orange;
            btnWatch.Enabled = Game.activeGame != null;
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
                    onQuestChanged();
                    this.setChildrenEnabled(true, btnWatch);
                });
            }
            else
            {
                this.cmdrPlay = Game.activeGame.cmdrPlay;
                onQuestChanged();
            }
        }

        private void FormPlayComms_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            g.DrawLine(C.Pens.orangeDark2, bigPanel.Left, bigPanel.Top - 3, bigPanel.Right, bigPanel.Top - 3);

            g.DrawRectangle(C.Pens.orangeDark2, ClientRectangle);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var hasUnread = cmdrPlay.messagesUnread > 0;
            // PlotQuestMini.drawLogo(e.Graphics, ClientSize.Width - 44, 6, hasUnread, 32);
            // PlotQuestMini.drawLogo(e.Graphics, ClientSize.Width - 44, ClientSize.Height - 44, hasUnread, 32);

            PlotQuestMini.drawLogo(e.Graphics, 100, 6, hasUnread, 32);

            var txt = mode == "quests"
                ? $"Active quests: {cmdrPlay.activeQuests.Count}"
                : hasUnread ? $"Messages: {cmdrPlay.messagesTotal}, unread: {cmdrPlay.messagesUnread}" : $"Messages: {cmdrPlay.messagesTotal}";
            TextRenderer.DrawText(e.Graphics, txt, GameColors.Fonts.gothic_16B, new Point(136, 10), C.orangeDark);
        }

        private void btnClose_Paint(object sender, PaintEventArgs e)
        {
            var btn = (DrawButton)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var p = btn.highlight ? C.Pens.orangeDark2r : C.Pens.orange2r;
            PlotQuestMini.drawBackArrow(e.Graphics, 28, 6, 18, p);
        }

        private void btnQuests_Paint(object sender, PaintEventArgs e)
        {
            var btn = (DrawButton)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var p = btn.highlight ? C.Pens.orangeDark3r : C.Pens.orange3r;
            PlotQuestMini.drawPage(e.Graphics, 15, 12, 51, p);
        }

        private void btnMsgs_Paint(object sender, PaintEventArgs e)
        {
            var btn = (DrawButton)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var p = btn.highlight ? C.Pens.orangeDark3r : C.Pens.orange3r;
            PlotQuestMini.drawEnvelope(e.Graphics, 9, 18, 53, p);
        }

        private void btnQuests_Enter(object sender, EventArgs e)
        {
            // remember which button last had focus
            lastLeftBtn = (Control)sender;
        }

        private void btnQuests_Click(object sender, EventArgs e)
        {
            this.mode = "quests";
            onQuestChanged();
        }

        private void btnMsgs_Click(object sender, EventArgs e)
        {
            this.mode = "msgs";
            onQuestChanged();
        }

        public void onQuestChanged(PlayQuest? pq = null)
        {
            if (mode == "quests")
                showQuests();
            else
                showMsgs();

            // TODO: match by name ... the actual instance will have been replaced
            if (lastTListItem?.Visible == true)
                lastTListItem.Focus();
            else if (tlist.Controls.Count > 0)
                tlist.Controls[0].Focus();

            this.Invalidate(true);
        }

        private void showQuests()
        {
            if (!(cmdrPlay.activeQuests.Count > 0)) return;

            clearSelection();
            tlist.Controls.Clear();
            tlist.RowStyles.Clear();

            var sorted = cmdrPlay!.activeQuests.OrderBy(q => q.startTime);
            foreach (var pq in sorted)
            {
                var btn = new DrawButton()
                {
                    Name = pq.id,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Padding = Padding.Empty,
                    Margin = Padding.Empty,
                    BackColor = Color.Black,
                };
                btn.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var tt = new TextCursor(e.Graphics, this);
                    btn.Height = PanelQuest.drawCollapsed(e.Graphics, tt, btn.highlight, pq);
                };
                btn.Click += (s, e) =>
                {
                    lastTListItem = s as Control;
                    setSelectedThing(new PanelQuest(this, pq));
                };

                addRow(btn);
            }
        }

        private void addRow(Control ctrl)
        {
            tlist.Controls.Add(ctrl, 0, tlist.Controls.Count);
            tlist.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        private void setSelectedThing(Control ctrl)
        {
            selectedThing = ctrl;
            bigPanel.Controls.Add(ctrl);
            ctrl.Width = bigPanel.Width;
            tlist.Hide();
            ctrl.Focus();
        }

        public void clearSelection()
        {
            if (selectedThing != null)
            {
                bigPanel.Controls.Remove(selectedThing);
                selectedThing = null;
            }

            tlist.Show();
            if (lastTListItem != null)
                lastTListItem.Focus();
            else if (tlist.Controls.Count > 0)
                tlist.Controls[0].Focus();
            else
                lastLeftBtn.Focus();
        }

        private void showMsgs()
        {
            if (!(cmdrPlay.activeQuests.Count > 0)) return;
            clearSelection();
            tlist.Controls.Clear();
            tlist.RowStyles.Clear();

            var allMsgs = cmdrPlay.activeQuests
                .SelectMany(q => q.msgs)
                .OrderByDescending(m => m.received)
                .ToList();
            Game.log($"showing {allMsgs.Count} msgs");

            foreach (var pm in allMsgs)
            {
                var qm = pm.parent.quest.msgs.Find(m => m.id == pm.id)!;

                var btn = new DrawButton()
                {
                    Name = pm.id,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Padding = Padding.Empty,
                    Margin = Padding.Empty,
                    BackColor = Color.Black,
                };
                btn.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var tt = new TextCursor(e.Graphics, btn);
                    btn.Height = PanelMsg.drawCollapsed(e.Graphics, tt, btn.highlight, pm, qm);
                };
                btn.Click += (s, e) =>
                {
                    lastTListItem = s as Control;

                    // remember this message has now been read
                    if (!pm.read)
                    {
                        pm.read = true;
                        pm.parent.updateIfDirty(true);
                    }

                    setSelectedThing(new PanelMsg(this, pm, qm));
                };

                addRow(btn);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //Debug.WriteLine($"!1 {keyData} / {this.ActiveControl?.Name} / {selectedThing?.Name}");

            if (keyData == Keys.Enter) return base.ProcessCmdKey(ref msg, keyData);

            if (ActiveControl is PanelQuest)
            {
                if (keyData == Keys.Left)
                    lastLeftBtn.Focus();
                else
                    this.GetNextControl(selectedThing, true)?.Focus();
                return true;
            }
            else if (ActiveControl is PanelMsg)
            {
                if (keyData == Keys.Left && ActiveControl.Left < 40)
                {
                    lastLeftBtn.Focus();
                    return true;
                }
                else
                {
                    this.GetNextControl(selectedThing, true)?.Focus();
                    return true;
                }
            }
            else if (this.ActiveControl?.Parent is PanelMsg)
            {
                if (keyData == Keys.Left && ActiveControl.Left < 40)
                {
                    lastLeftBtn.Focus();
                    return true;
                }
            }
            else if (this.ActiveControl == btnQuests || this.ActiveControl == btnMsgs)
            {
                if (keyData == Keys.Right)
                {
                    if (selectedThing != null)
                    {
                        this.GetNextControl(selectedThing, true)?.Focus();
                    }
                    else
                    {
                        var next = lastTListItem ?? this.GetNextControl(tlist, true);

                        if (next == null)
                        {
                            Debug.WriteLine($"??{this.ActiveControl?.Name}");
                        }

                        next?.Focus();
                    }
                    return true;
                }
                else if (keyData == Keys.Left)
                    return true;
            }
            if (this.ActiveControl?.Parent == tlist)
            {
                if (keyData == Keys.Left)
                {
                    lastLeftBtn.Focus();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnDev_Click(object sender, EventArgs e)
        {
            BaseForm.show<FormPlayDev>(f => f.cmdrPlay = this.cmdrPlay);
        }

        private void btnWatch_Click(object sender, EventArgs e)
        {
            if (Game.activeGame != null)
                BaseForm.show<FormPlayJournal>();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormPlayComms_Activated(object sender, EventArgs e)
        {
            btnWatch.Enabled = Game.activeGame != null;
        }
    }

    class PanelQuest : Panel
    {
        public static int drawCollapsed(Graphics g, TextCursor tt, bool selected, PlayQuest pq)
        {
            tt.dty = N.six;
            var c = selected ? Color.Black : C.orange;

            // title
            tt.draw(N.eight, pq.quest.title, c, GameColors.Fonts.gothic_12B);
            tt.newLine(N.six);

            // visible objectives
            if (pq.objectives.Any())
            {
                foreach (var (key, obj) in pq.objectives)
                    if (obj.state == PlayObjective.State.visible)
                        drawObjective(g, tt, c, key, obj, pq);
            }
            else
            {
                // no active objectives?
                tt.draw(N.eight, "► No objectives", c, GameColors.Fonts.gothic_9);
                tt.newLine(N.four);
            }

            return (int)(tt.dty + N.ten);
        }

        static void drawObjective(Graphics g, TextCursor tt, Color c, string key, PlayObjective obj, PlayQuest pq)
        {
            var cc = c;

            var prefix = " ▶";
            if (obj.state == PlayObjective.State.complete) { prefix = "✔️"; }
            else if (obj.state == PlayObjective.State.failed) { prefix = "❌"; cc = C.orangeDark; }
            tt.draw(N.eight, prefix, cc, GameColors.Fonts.gothic_9);
            tt.draw(N.twoSix, pq.quest.strings.GetValueOrDefault(key, key), cc);
            tt.newLine(N.two);

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
            tt.dty += N.four;
        }

        public readonly FormPlayComms form;
        public readonly PlayQuest pq;

        public PanelQuest(FormPlayComms form, PlayQuest pq)
        {
            this.Name = "PanelQuest";
            this.form = form;
            this.pq = pq;
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;
            this.ForeColor = C.orange;
            this.BackColor = Color.Black;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.ResizeRedraw = true;
            this.DoubleBuffered = true;
            this.TabStop = true;

            addButtons();
        }

        private void addButtons()
        {
            var btnB = new DrawButton()
            {
                Name = "questBack",
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Text = "Back",
                Left = 6,
                ForeColor = C.orange,
            };

            btnB.Top = this.Height - btnB.Height - 6;
            btnB.Click += BtnB_Click;
            this.Controls.Add(btnB);

            var btnP = new FlatButton()
            {
                Name = "questPause",
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
            form.clearSelection();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var tt = new TextCursor(e.Graphics, this);

            this.Height = drawExpanded(g, tt);
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
            g.DrawLine(C.Pens.orangeDark2, 0, tt.dty, tt.containerWidth, tt.dty);
            g.SmoothingMode = SmoothingMode.HighQuality;
            tt.dty += N.eight;

            // objectives
            tt.draw(N.eight, "Objectives:", C.orangeDark);
            tt.newLine(N.eight);
            if (pq.objectives.Any())
            {
                foreach (var (key, obj) in pq.objectives)
                    drawObjective(g, tt, c, key, obj, pq);
            }
            else
            {
                tt.draw(N.eight, "► No active objectives");
                tt.newLine(N.four);
            }

            tt.dty += N.ten;

            tt.draw(N.eight, "Description:", C.orangeDark);
            tt.newLine(N.eight);

            tt.drawWrapped(N.eight, pq.quest.desc, c);
            tt.newLine(N.ten);

            g.SmoothingMode = SmoothingMode.None;
            g.DrawLine(C.Pens.orangeDark2, 0, tt.dty, tt.containerWidth, tt.dty);
            g.SmoothingMode = SmoothingMode.HighQuality;
            tt.dty += N.ten;

            return (int)(tt.dty + N.six) + Controls[0].Height;
        }
    }

    class PanelMsg : Panel
    {
        public static int drawCollapsed(Graphics g, TextCursor tt, bool selected, PlayMsg pm, Msg qm)
        {
            tt.dty = N.four;

            tt.color = pm.read ? C.orange : C.cyan;
            var p = !pm.read ? C.Pens.cyan2r : C.Pens.orange2r;

            if (selected)
            {
                tt.color = Color.Black;
                p = C.Pens.black2;
            }

            if (selected && !pm.read)
                g.Clear(C.cyan);

            // draw envelop logo
            PlotQuestMini.drawEnvelope(g, N.six, N.ten, N.twoEight, p);

            // received time on right side
            var time = pm.received.Subtract(DateTime.Now).TotalDays < 1
                ? pm.received.ToString("HH:mm")
                : pm.received.AddYears(1286).UtcDateTime.ToString("dd MMM yyyy - HH:mm");

            // message from
            tt.drawRight(tt.containerWidth - N.six, time, null, GameColors.Fonts.gothic_7);
            tt.draw(N.forty, pm.from ?? qm?.from, GameColors.Fonts.gothic_8);
            tt.newLine(-N.four);

            // subject (ellipse if too wide)
            var subject = pm.subject ?? qm?.subject ?? pm.body ?? qm?.body;
            var flags = tt.flags;
            tt.flags |= TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            tt.drawWrapped(N.forty, tt.containerWidth - (int)N.ten, subject, GameColors.Fonts.gothic_10);
            tt.flags = flags;
            tt.newLine(N.four);

            return (int)tt.dty;
        }

        public readonly FormPlayComms form;
        public readonly PlayQuest pq;
        public readonly PlayMsg pm;
        public readonly Msg? qm;

        public PanelMsg(FormPlayComms form, PlayMsg msg, Msg qm)
        {
            this.Name = "PanelMsg";
            this.form = form;
            this.pq = msg.parent;
            this.pm = msg;
            this.qm = qm;
            this.Padding = Padding.Empty;
            this.Margin = Padding.Empty;
            this.ForeColor = C.orange;
            this.BackColor = Color.Black;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.ResizeRedraw = true;
            this.DoubleBuffered = true;
            this.TabStop = true;

            addButtons();
        }

        private void addButtons()
        {
            if (qm?.actions != null)
            {
                foreach (var (key, txt) in qm.actions)
                {
                    var btn = new DrawButton()
                    {
                        Name = $"msg-{key}",
                        Tag = key,
                        Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                        Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                        Text = $"◊ {txt}",
                        Left = 24,
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleLeft,
                        UseCompatibleTextRendering = true,
                        TabIndex = this.Controls.Count,
                    };
                    btn.Click += Btn_Click;
                    this.Controls.Add(btn);
                }
            }

            var btnB = new DrawButton()
            {
                Name = "msgBack",
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Text = "Back",
                Left = 6,
                TabIndex = this.Controls.Count,
            };
            btnB.Top = this.Height - btnB.Height - 6;
            btnB.Click += BtnB_Click;
            this.Controls.Add(btnB);

            var btnD = new DrawButton()
            {
                Name = "questDelete",
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Text = "Delete",
                TabIndex = this.Controls.Count,
            };
            btnD.Left = this.Width - btnD.Width - 6;
            btnD.Top = this.Height - btnD.Height - 6;
            btnD.Click += BtnD_Click;
            this.Controls.Add(btnD);
        }

        private void BtnB_Click(object? sender, EventArgs e)
        {
            form.clearSelection();
        }

        private void BtnD_Click(object? sender, EventArgs e)
        {
            pq.deleteMsg(pm.id);
            pq.updateIfDirty();
            form.clearSelection();
        }

        private void Btn_Click(object? sender, EventArgs e)
        {
            var btn = sender as Button;
            var actionId = btn?.Tag as string;
            if (actionId == null) return;

            pq.invokeMessageAction(pm.chapter!, actionId);
            pq.updateIfDirty();
            form.clearSelection();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var tt = new TextCursor(e.Graphics, this);
            tt.dty = N.four;

            this.Height = drawExpanded(g, tt);
        }

        int drawExpanded(Graphics g, TextCursor tt)
        {
            tt.font = GameColors.Fonts.gothic_12B;
            tt.color = C.orange;

            PlotQuestMini.drawEnvelope(g, N.ten, N.ten, N.forty, C.Pens.orange2r);

            tt.draw(N.sixFour, "Sent:", GameColors.Fonts.gothic_8);
            tt.draw(N.oneTwenty, pm.received.AddYears(1286).UtcDateTime.ToString("dd MMM yyyy - HH:mm"));
            tt.newLine();

            tt.draw(N.sixFour, "From:", GameColors.Fonts.gothic_8);
            tt.draw(N.oneTwenty, pm.from ?? qm?.from);
            tt.newLine();

            var subject = pm.subject ?? qm?.subject;
            if (subject != null)
            {
                tt.draw(N.sixFour, "Subject:", GameColors.Fonts.gothic_8);
                tt.draw(N.oneTwenty, subject);
                tt.newLine();
            }
            tt.dty += N.four;

            // body with lines
            g.DrawLine(C.Pens.orangeDark2, 0, tt.dty, Width - N.four, tt.dty);
            tt.dty += N.six;
            tt.drawWrapped(N.oneTwo, Width - (int)N.oneTwo, pm.body ?? qm?.body, GameColors.Fonts.gothic_12);
            tt.newLine(N.six);
            g.DrawLine(C.Pens.orangeDark2, 0, tt.dty, tt.containerWidth, tt.dty);
            tt.dty += N.four;

            // TODO: show list of strings to copy to clipboard?

            // any response actions?
            var actions = pm.actions ?? qm?.actions?.Keys.ToArray();
            if (actions != null)
            {
                tt.draw(N.oneTwo, "Respond:", C.orangeDark, GameColors.Fonts.gothic_10);
                tt.newLine(-N.four);
                foreach (Button btn in Controls)
                {
                    if (btn.Tag == null) continue;

                    btn.Top = (int)(tt.dty + N.eight);
                    tt.dty += btn.Height + N.eight;
                }

                // final line
                tt.dty += N.ten;
                g.DrawLine(C.Pens.orangeDark2, N.six, tt.dty, Width - N.four, tt.dty);
                tt.dty += N.four;
            }

            // allow space for buttons along the bottom
            tt.dty += N.oneTwo + Controls[0].Height;
            return (int)tt.dty;
        }
    }
}
