using SrvSurvey.game;
using SrvSurvey.game.RavenColonial;
using SrvSurvey.plotters;
using SrvSurvey.quests;
using SrvSurvey.widgets;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey.forms.playComms;

[Draggable]
[System.ComponentModel.DesignerCategory("")]
internal class FormPlayComms2 : BaseFormZippy
{
    public static void toggleForm()
    {
        var form = BaseForm.get<FormPlayComms2>();
        if (form == null)
        {
            BaseForm.show<FormPlayComms2>();
        }
        else
        {
            if (Elite.focusSrvSurvey)
                form.Close();
            else
                form.Activate();
        }
    }

    enum Mode
    {
        /// <summary> Loading initial PlayState </summary>
        init,
        /// <summary> Show a list of messages </summary>
        msgs,
        /// <summary> Show details for a specific message </summary>
        msgItem,
        /// <summary> Show a catalog of available quests </summary>
        catalog,
        /// <summary> Show details on a specific quest from the catalog </summary>
        defQuest,
        /// <summary> Show a list of quests for current Cmdr </summary>
        cmdrQuests,
    }

    private Mode mode;
    private Action? onBackCustom;
    private BtnFillDrawCtrl btnMsgs;
    private BtnFillDrawCtrl btnCmdr;
    private BtnFillDrawCtrl btnCat;
    /// <summary> Controls not in the stack, but extra along the top </summary>
    private List<Ctrl> tops = [];

    private bool loadingCatalog;
    private static List<DefQuest>? publishedQuests;
    private static List<QuestCmdrStatus> cmdrQuests = [];
    private static string? lastFID;

    public FormPlayComms2() : base()
    {
        this.Font = GameColors.Fonts.arial_12;
        this.ForeColor = C.orange;
        this.Size = new(800, 800);
        this.MinimumSize = new(560, 400);
        setScroll(100, 72, -10, -10);

        this.init();
    }

    private void init()
    {
        // init static ctrls
        btnMsgs = new BtnMsgsEnvelope
        {
            offset = new(10, 72),
            sz = new(72, 72),
            disabled = !(PlayState.current?.messagesTotal > 0),
            onClick = (_, _) => setMode(Mode.msgs),
        };

        btnCmdr = new BtnFillDrawCtrl
        {
            follow = Side.Top,
            sz = new(72, 72),
            iconName = "logo",
            iconSize = 46,
            iconOffset = new(15, 12),
            onClick = (_, _) => setMode(Mode.cmdrQuests),
        };

        btnCat = new BtnFillDrawCtrl
        {
            follow = Side.Top,
            sz = new(72, 72),
            iconName = "page",
            iconSize = 51,
            iconOffset = new(15, 12),
            onClick = (_, _) => setMode(Mode.catalog),
        };

        addCtrl(
            btnMsgs,
            btnCmdr,
            btnCat,
            new BtnFillDrawCtrl { offset = new(10, -10), r = new(0, 0, 72, 32), iconName = "close", iconSize = 18, iconOffset = new(26, 10), onClick = (_, _) => this.Close(), }
        );

#if DEBUG
        addCtrl(
            new BtnFillTextCtrl { follow = Side.Bottom, sz = new(72, 32), text = "(watch)", onClick = (_, _) => BaseForm.show<FormPlayJournal>(), disabled = Game.activeGame == null },
            new BtnFillTextCtrl { follow = Side.Bottom, sz = new(72, 32), text = "(dev)", onClick = (_, _) => BaseForm.show<FormPlayDev>(), }
        );
#endif
    }

    public static async Task fetchCmdrQuests(string fid)
    {
        cmdrQuests = await Game.rcc.getCmdrQuests(fid);
        lastFID = fid;

        BaseForm.get<FormPlayComms2>()?.Invalidate();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // position ourself over the top/right quadrant of the game
        var r = Elite.getWindowRect();
        if (r.Width > 0)
        {
            this.Width = (int)(r.Width * 0.4f);
            this.Height = (int)(r.Height * 0.7f);
            this.Left = r.Right - this.Width - 20;
            this.Top = r.Top + 10 + (PlotBase2.getPlotter<PlotQuestMini>()?.bottom ?? 20);
            Application.DoEvents();
        }
        this.BackgroundImage = GameGraphics.getBackgroundImage(this.ClientSize, true);

        // now do async stuff
        Task.Run(async () =>
        {
            // do we need to load current play-state
            var fid = CommanderSettings.currentOrLastFid;
            if (PlayState.current == null)
                await PlayState.loadAsync(fid);

            if (PlayState.current == null) throw new Exception($"Cannot get PlayState for: {fid}");

            // fetch cmdr's current mission status, if needed
            if (lastFID != fid || true)
                await fetchCmdrQuests(fid);

        }).continueOnMain(this, () =>
        {
            // choose a starting mode
            var ps = PlayState.current;
            if (ps?.messagesTotal > 0)
                setMode(Mode.msgs);
            else if (cmdrQuests.Count > 0)
                setMode(Mode.cmdrQuests);
            else
                setMode(Mode.catalog);

            Application.DoEvents();
            this.Invalidate();
        }).justDoIt();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        if (Elite.isGameRunning && !Elite.eliteMinimized)
            Elite.setFocusED();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);

        // close ourself anytime we lose focus (but not if debugging)
        if (!Debugger.IsAttached)
            Program.defer(() => this.Close());
    }

    private void setMode(Mode mode)
    {
        if (this.mode == mode) return;

        this.mode = mode;
        btnMsgs.sideBar = mode == Mode.msgs;
        btnCmdr.sideBar = mode == Mode.cmdrQuests;
        btnCat.sideBar = mode == Mode.catalog;
        onBackCustom = null;
        // remove scrollable ctrls
        stack.Clear();
        // and any extra stuff
        ctrls.RemoveAll(c => tops.Contains(c));
        tops.Clear();

        if (mode == Mode.catalog)
            loadPublishedQuests();
        else if (mode == Mode.msgs)
            loadMessages();
        else if (mode == Mode.cmdrQuests)
            loadCmdrQuests();

        Application.DoEvents();
        this.Invalidate();
    }

    protected override bool onBack()
    {
        // move 'back' in the UI
        if (this.onBackCustom != null)
        {
            this.onBackCustom();
            return true;
        }

        switch (this.mode)
        {
            case Mode.init:
            case Mode.msgs:
            case Mode.catalog:
            case Mode.cmdrQuests:
                // close the window
                this.Close();
                return true;

            case Mode.defQuest:
                setMode(Mode.catalog);
                return true;

            case Mode.msgItem:
                setMode(Mode.msgs);
                return true;
            default:
                throw new Exception($"Unexpected mode: {mode}");
        }
    }

    protected override bool render(Graphics g, TextCursor tt)
    {
        var ps = PlayState.current;
        if (ps == null) return false;
        btnMsgs.disabled = ps.messagesTotal == 0;

        base.render(g, tt);

        // jump out of msgs mode if there are none
        if (mode == Mode.msgs && ps.messagesTotal == 0)
        {
            Program.defer(() =>
            {
                setMode(Mode.cmdrQuests);
                this.Invalidate();
            });
            return false;
        }

        switch (mode)
        {
            case Mode.init:
                return renderInitializing(g, tt);

            case Mode.msgs:
            case Mode.msgItem:
                return renderMessages(g, tt, ps);

            case Mode.catalog:
            case Mode.defQuest:
                return renderQuestCatalog(g, tt);
        }
        return false;
    }

    private bool renderInitializing(Graphics g, TextCursor tt)
    {
        // TODO: make this more interesting?
        tt.dty = 10;
        tt.dtx = 100;
        tt.draw("Initializing...");
        return false;
    }

    #region catalog

    private bool renderQuestCatalog(Graphics g, TextCursor tt)
    {
        // TODO: make this a ctrl
        if (tops.Count > 0) return false;

        tt.dty = N.ten;
        tt.draw(N.hundred, "Quest Catalog", GameColors.Fonts.arial_20);
        tt.newLine(true);
        tt.draw(N.hundred, "Choose your next adventure:", C.orangeDark);
        tt.newLine(N.threeTwo, true);

        if (loadingCatalog)
            tt.draw(N.hundred, "... loading ...");

        return false;
    }

    private async Task fetchPublishedQuests()
    {
        loadingCatalog = true;
        this.Invalidate();

        // only download once per process
        if (publishedQuests == null)
        {
            // fetch whole catalog
            publishedQuests = await Game.rcc.getPublishedQuests(CommanderSettings.currentOrLastFid);
        }

        if (publishedQuests == null || publishedQuests.Count == 0) throw new Exception("Why are there zero quests available?");

        loadingCatalog = false;
    }

    private void loadPublishedQuests()
    {
        // only download once per process
        if (publishedQuests == null)
        {
            // fetch on a thread
            Task.Run(() => this.fetchPublishedQuests().continueOnMain(this, () => this.loadPublishedQuests()));
            return;
        }

        // and include the in-progress devQuest
        if (PlayState.current?.devQuest != null)
        {
            addStack(new QuestCatalogLine
            {
                qd = PlayState.current.devQuest.quest,
                follow = Side.Top,
                onClick = (_, _) => showQuestDef(PlayState.current.devQuest.quest),
            });
        }

        // hide items if matches devQuest, or cmdr has activated them before
        var items = publishedQuests.Where(qd => !qd.equals(PlayState.current?.devQuest?.quest) && cmdrQuests.Find(x => x.publisher == qd.publisher && x.id == qd.id) == null);
        addStack(items.Select((qd, i) => new QuestCatalogLine
        {
            qd = qd,
            qs = cmdrQuests?.Find(qs => qs.publisher == qd.publisher && qs.id == qd.id),
            follow = Side.Top,
            onClick = (_, _) => showQuestDef(qd),
        }));
        if (stack.Count > 0) ctrlCurrent = stack[0];

        loadingCatalog = false;
        this.Invalidate();
    }

    private void showQuestDef(DefQuest qd)
    {
        var btnBack = new BtnFillDrawCtrl { follow = Side.Top, sz = new(72, 32), iconName = "close", iconSize = 18, iconOffset = new(28, 10) };
        btnBack.onClick = (_, _) =>
        {
            if (onBackCustom != null)
                onBackCustom();
            else
                setMode(Mode.catalog);
        };

        var lblConfirm = new TextCtrl { follow = Side.Top, autoSize = true, color = C.oranger };
        var btnYes = new BtnFillTextCtrl { follow = Side.Top, sz = new(72, 32), text = "Yes", onClick = (_, _) => this.Invalidate() };
        var btnNo = new BtnFillTextCtrl { follow = Side.Left, sz = new(72, 32), text = "No", onClick = (_, _) => this.Invalidate() };
        var btnAct = new BtnFillTextCtrl { follow = Side.Left, sz = new(120, 32) };
        var lblActing = new TextCtrl { follow = Side.Left, autoSize = true, color = C.oranger };

        List<Ctrl> subStack = [];
        btnAct.onClick = (_, _) =>
        {
            // show prompt
            btnBack.hidden = true;
            btnAct.hidden = true;
            subStack = addStack(lblConfirm, btnYes, btnNo);
            ctrlCurrent = btnNo;

            onBackCustom = () =>
            {
                // do the 'No' click action
                btnNo.onClick(btnNo, false);
            };
        };
        btnNo.onClick = (_, _) =>
        {
            // decline
            btnBack.hidden = false;
            btnAct.hidden = false;
            onBackCustom = null;

            stack.RemoveAll(c => subStack.Contains(c));
            ctrlCurrent = btnBack;
        };

        // adjust messages depending on quest state
        var pq = PlayState.current?.activeQuests.Find(pq => pq.quest.equals(qd));
        var qs = cmdrQuests.Find(x => x.publisher == qd.publisher && x.id == qd.id);
        var isPaused = qs?.state == QuestState.paused;
        if (isPaused)
        {
            lblConfirm.text = "Are you sure?";
            btnAct.text = "Resume quest";
            lblActing.text = "Resuming ...";
            btnYes.onClick = (_, _) =>
            {
                // activate quest
                lblConfirm.color = C.grey;
                btnYes.disabled = true;
                btnNo.disabled = true;
                subStack.AddRange(addStack(lblActing));
                onBackCustom = null;

                PlayState.current?.resumeQuest(qd.publisher, qd.id).continueOnMain(this, () =>
                {
                    btnBack.hidden = false;
                    btnAct.hidden = false;
                    stack.Remove(btnAct);
                    stack.RemoveAll(c => subStack.Contains(c));
                    addStack(new TextCtrl { follow = Side.Left, autoSize = true, text = "Enjoy the quest ...", color = C.oranger });
                    ctrlCurrent = btnBack;
                    onBackCustom = () => setMode(PlayState.current?.messagesTotal > 0 ? Mode.msgs : Mode.cmdrQuests);
                    btnBack.onClick = (_, _) => onBackCustom();
                    PlotBase2.renderAll(null, true);

                    // update our internal state (rather than re-hit the server)
                    if (lastFID != null)
                        fetchCmdrQuests(lastFID).justDoIt();
                });
            };
        }
        else if (pq == null)
        {
            lblConfirm.text = "Are you sure?";
            btnAct.text = "Activate quest";
            lblActing.text = "Activating ...";
            btnYes.onClick = (_, _) =>
            {
                // activate quest
                lblConfirm.color = C.grey;
                btnYes.disabled = true;
                btnNo.disabled = true;
                subStack.AddRange(addStack(lblActing));
                onBackCustom = null;

                PlayState.current?.activateQuest(qd.publisher, qd.id).continueOnMain(this, () =>
                {
                    btnBack.hidden = false;
                    btnAct.hidden = false;
                    stack.Remove(btnAct);
                    stack.RemoveAll(c => subStack.Contains(c));
                    addStack(new TextCtrl { follow = Side.Left, autoSize = true, text = "Enjoy the quest ...", color = C.oranger });
                    ctrlCurrent = btnBack;
                    onBackCustom = () => setMode(PlayState.current?.messagesTotal > 0 ? Mode.msgs : Mode.cmdrQuests);
                    btnBack.onClick = (_, _) => onBackCustom();

                    PlotBase2.renderAll(null, true);

                    // update our internal state (rather than re-hit the server)
                    if (lastFID != null)
                        fetchCmdrQuests(lastFID).justDoIt();
                });
            };
        }
        else
        {
            lblConfirm.text = "Are you sure? This will undo any prior progress";
            btnAct.text = "Remove";
            lblActing.text = "Removing ...";
            btnYes.onClick = (_, _) =>
            {
                // remove quest
                lblConfirm.color = C.grey;
                btnYes.disabled = true;
                btnNo.disabled = true;
                subStack.AddRange(addStack(lblActing));
                onBackCustom = null;

                PlayState.current?.removeQuest(pq, QuestState.unknown).continueOnMain(this, () =>
                {
                    btnBack.hidden = false;
                    btnAct.hidden = false;
                    stack.Remove(btnAct);
                    stack.RemoveAll(c => subStack.Contains(c));
                    addStack(new TextCtrl { follow = Side.Left, autoSize = true, text = "Better luck next time ...", color = C.oranger });
                    ctrlCurrent = btnBack;
                    onBackCustom = () => setMode(PlayState.current?.messagesTotal > 0 ? Mode.msgs : Mode.cmdrQuests);
                    btnBack.onClick = (_, _) => onBackCustom();
                    PlotBase2.renderAll(null, true);
                });
            };
        }

        // now, add into the stack
        stack.Clear();
        addStack(
            new QuestCatalogItem { qd = qd, qs = qs },
            new TextCtrl { follow = Side.Top, color = C.orange, autoSize = true, font = GameColors.Fonts.arial_9, pad = 0, text = "Tags:" }
        );
        if (qd.tags?.Count > 0)
            addStack(qd.tags.Select(t => new BtnFillTextCtrl { follow = Side.Left, autoSize = true, pad = 1, font = GameColors.Fonts.arial_9, text = t, onClick = (_, _) => Util.setClipboardText(t) }));
        else
            addStack(new TextCtrl { follow = Side.Left, color = C.grey, autoSize = true, pad = 0, text = "none" });
        addStack(new HorizLine { follow = Side.Top }, btnBack, btnAct);
        ctrlCurrent = btnBack;

        mode = Mode.defQuest;
    }

    #endregion

    #region list msgs

    private bool renderMessages(Graphics g, TextCursor tt, PlayState ps)
    {
        tt.dty = N.ten;
        tt.draw(N.hundred, "Messages:", GameColors.Fonts.arial_20);
        tt.draw(ps.messagesTotal.ToString(), C.oranger, GameColors.Fonts.arial_20);
        tt.draw(" Unread: ", GameColors.Fonts.arial_20);
        tt.draw(ps.messagesUnread.ToString(), C.oranger, GameColors.Fonts.arial_20);
        tt.newLine(true);

        return false;
    }

    private void loadMessages()
    {
        var ps = PlayState.current;
        if (ps == null) return;

        var allMsgs = ps.activeQuests
            .SelectMany(q => q.msgs)
            .OrderByDescending(m => m.received)
            .ToList();
        Game.log($"showing {allMsgs.Count} msgs");

        addStack(allMsgs.Select(pm =>
        {
            var qm = pm.parent.quest.msgs.Find(m => m.id == pm.id);
            return new MessageLine
            {
                pm = pm,
                qm = qm,
                follow = Side.Top,
                onClick = (_, _) => this.showMessage(pm, qm, ps).justDoIt(),
            };
        }));
        if (stack.Count > 0) ctrlCurrent = stack[0];

        this.Invalidate();
    }

    private async Task showMessage(PlayMsg pm, DefMsg? qm, PlayState ps)
    {
        var body = pm.body ?? qm?.body ?? "";

        List<Ctrl> subStack = [];
        var btnBack = new BtnFillDrawCtrl { follow = Side.Top, offset = new(0, 0), sz = new(72, 32), iconName = "close", iconSize = 18, iconOffset = new(28, 10), onClick = (_, _) => setMode(Mode.msgs) };

        // copy-tags
        if (qm?.tags?.Count > 0)
        {
            subStack.Add(new TextCtrl { follow = Side.Top, autoSize = true, font = GameColors.Fonts.arial_9, color = C.oranged, text = "Copy:" });
            subStack.AddRange(qm.tags.Select(t => new BtnFillTextCtrl
            {
                follow = Side.Left,
                autoSize = true,
                text = t,
                onClick = (_, _) => Util.setClipboardText(t),
            }));
            subStack.Add(new HorizLine { follow = Side.Top });
        }

        // reply buttons
        var actions = pm.actions ?? qm?.actions?.Keys.ToArray();
        Ctrl nextCurrent = btnBack;
        if (actions != null && qm?.actions != null)
        {
            if (pm.replied != null)
            {
                var replied = qm.actions.GetValueOrDefault(pm.replied) ?? pm.replied;
                subStack.AddRange(
                    new TextCtrl { follow = Side.Top, autoSize = true, color = C.oranged, font = GameColors.Fonts.arial_9, text = "Responded: " },
                    new TextCtrl { follow = Side.Left, autoSize = true, color = C.oranged, backColor = C.orangeDarker, text = replied }
                );
                subStack.Add(new HorizLine { follow = Side.Top });
            }
            else
            {
                var chapterValid = pm.parent.chapters.FirstOrDefault(x => x.id == pm.chapter) != null;
                subStack.Add(new TextCtrl { follow = Side.Top, autoSize = true, color = chapterValid ? C.oranged : C.grey, font = GameColors.Fonts.arial_9, text = "Respond:" });
                subStack.AddRange(actions.Select((k, i) => new BtnFillTextCtrl
                {
                    follow = i == 0 ? Side.Left : Side.Top,
                    autoSize = true,
                    text = qm.actions.GetValueOrDefault(k) ?? k,
                    disabled = !chapterValid,
                    onClick = (_, _) =>
                    {
                        pm.parent.invokeMessageAction(pm.id, k).continueOnMain(this, () => setMode(Mode.msgs));
                        this.Invalidate();
                    },
                }));
                subStack.Add(new HorizLine { follow = Side.Top });
                if (subStack.Count > 2) nextCurrent = subStack[1];
            }
        }

        stack.Clear();
        addStack(new MessageItem { pm = pm, qm = qm, body = body });
        addStack(subStack);
        addStack(btnBack);
        ctrlCurrent = nextCurrent;

        addStack(new BtnFillCtrl
        {
            follow = Side.Left,
            sz = new(32, 32),
            onRender = (ctrl, g, tt, isCurrent, isClicking, prior) =>
            {
                var fat = isCurrent ? C.Pens.black2 : C.Pens.orange2;
                var thin = isCurrent ? C.black.toPen(2) : C.orange.toPen(1);
                var b = isCurrent ? C.Brushes.grey : C.Brushes.orangeDark;
                PlotQuestMini.drawLogo(g, tt.dtx + 7, tt.dty + 7, 18, fat, thin, b);
                return false;
            },
            onClick = (_, _) =>
            {
                this.onBackCustom = () =>
                {
                    this.showMessage(pm, qm, ps).justDoIt();
                    this.onBackCustom = null;
                };
                showCmdrQuestSummary(pm.parent);
            },
        });

        // remember this message has now been read
        if (!pm.read)
        {
            await pm.parent.onMessageRead(pm.id);
            pm.read = true;
            await pm.parent.save(true);
        }

        mode = Mode.msgItem;
    }

    private void showCmdrQuestSummary(PlayQuest pq)
    {
        var priorStack = new List<Ctrl>(stack);
        var priorCustomBack = this.onBackCustom;
        List<Ctrl> subStack = [];

        var btnBack = new BtnFillDrawCtrl { follow = Side.Top, offset = new(0, 0), sz = new(72, 32), iconName = "close", iconSize = 18, iconOffset = new(28, 10), onClick = (_, _) => priorCustomBack!() };
        var btnMore = new BtnFillTextCtrl { follow = Side.Left, sz = new(32, 32), text = "..." };
        var btnPause = new BtnFillTextCtrl { follow = Side.Left, sz = new(84, 32), text = "Pause", disabled = pq.dev }; // prevent devQuest from being paused
        var btnRemove = new BtnFillTextCtrl { follow = Side.Left, sz = new(84, 32), text = "Remove" };

        var lblConfirm = new TextCtrl { follow = Side.Top, autoSize = true, color = C.cyan };
        var btnYes = new BtnFillTextCtrl { follow = Side.Top, sz = new(72, 32), text = "Yes", onClick = (_, _) => this.Invalidate() };
        var btnNo = new BtnFillTextCtrl { follow = Side.Left, sz = new(72, 32), text = "No", onClick = (_, _) => this.Invalidate() };
        var lblActing = new TextCtrl { follow = Side.Left, autoSize = true, color = C.oranger };

        btnMore.onClick = (_, _) =>
        {
            stack.Remove(btnMore);
            addStack(btnPause, btnRemove);
            ctrlCurrent = btnBack;
        };

        btnNo.onClick = (_, _) =>
        {
            // decline
            btnBack.hidden = false;
            btnPause.hidden = false;
            btnRemove.hidden = false;
            onBackCustom = priorCustomBack;

            stack.RemoveAll(c => subStack.Contains(c));
            ctrlCurrent = btnBack;
        };

        var preAction = (QuestState newState) =>
        {
            // show prompt
            btnBack.hidden = true;
            btnPause.hidden = true;
            btnRemove.hidden = true;
            subStack = addStack(lblConfirm, btnYes, btnNo, lblActing);
            ctrlCurrent = btnNo;
            priorCustomBack = this.onBackCustom;

            lblConfirm.text = newState == QuestState.paused
                ? "Are you sure?"
                : "Are you sure? This will remove all progress.";

            btnYes.onClick = (_, _) =>
            {
                lblConfirm.color = C.grey;
                lblActing.text = newState == QuestState.paused ? "Pausing ..." : "Removing ...";
                this.Invalidate();

                pq.parent.removeQuest(pq, newState).continueOnMain(this, () =>
                {
                    btnBack.hidden = false;
                    btnBack.onClick = (_, _) => this.setMode(Mode.cmdrQuests);
                    stack.RemoveAll(c => subStack.Contains(c));
                    addStack(new TextCtrl { follow = Side.Left, autoSize = true, text = "Better luck next time ...", color = C.oranger });
                    ctrlCurrent = btnBack;
                    onBackCustom = () => setMode(PlayState.current?.messagesTotal > 0 ? Mode.msgs : Mode.cmdrQuests);
                    btnBack.onClick = (_, _) => onBackCustom();
                    this.Invalidate();
                    PlotBase2.renderAll(null, true);

                    // update our internal state (rather than re-hit the server)
                    if (lastFID != null)
                        fetchCmdrQuests(lastFID).justDoIt();
                });
            };

            onBackCustom = () =>
            {
                // do the 'No' click action
                btnNo.onClick(btnNo, false);
            };
        };

        btnPause.onClick = (_, _) => preAction(QuestState.paused);
        btnRemove.onClick = (_, _) => preAction(QuestState.unknown);

        stack.Clear();
        addStack(
            new QuestSummary { pq = pq },
            btnBack,
            btnMore
        );
        ctrlCurrent = btnBack;
    }

    #endregion

    #region Cmdr quests

    private void setFilter(Ctrl btn, QuestState newFilter, bool changeCurrent = false)
    {
        stack.Clear();

        // start loading catalog, if needed
        if (newFilter != QuestState.active && publishedQuests == null)
        {
            // fetch on a thread
            Task.Run(() => this.fetchPublishedQuests().continueOnMain(this, () => this.setFilter(btn, newFilter)));
            return;
        }

        // set bottomBar on the buttons
        tops.ForEach(c => { if (c is BtnFillTextCtrl c2) { c2.bottomBar = c == btn; } });

        // fill stack with filtered lines
        var items = cmdrQuests.Where(qs => qs.state == newFilter).ToList();
        if (newFilter == QuestState.active && PlayState.current?.devQuest != null)
        {
            items.Insert(0, new QuestCmdrStatus
            {
                publisher = PlayState.current.devQuest.publisher,
                id = PlayState.current.devQuest.id,
                ver = PlayState.current.devQuest.ver,
                state = QuestState.active,
                stateChangedOn = PlayState.current.devQuest.startTime.HasValue ? PlayState.current.devQuest.startTime.Value : default,
            });
        }

        if (items.Count == 0)
        {
            addStack(new TextCtrl { offset = new(0, 0), autoSize = true, color = C.grey, text = "None" });
            if (newFilter == QuestState.active)
                addStack(new BtnFillTextCtrl { follow = Side.Top, autoSize = true, text = "Find more in the catalog", onClick = (_, _) => setMode(Mode.catalog) });
        }
        else
        {
            var lines = items.Select(qs =>
            {
                var pq = PlayState.current?.activeQuests.Find(pq => pq.publisher == qs.publisher && pq.id == qs.id);
                var qd = pq?.quest ?? publishedQuests?.Find(x => x.publisher == qs.publisher && x.id == qs.id);

                if (qd == null) return null;
                return new QuestCatalogLine
                {
                    qd = qd,
                    qs = qs,
                    follow = Side.Top,
                    onClick = (_, _) =>
                    {
                        this.onBackCustom = () =>
                        {
                            this.setFilter(btn, newFilter);
                            this.onBackCustom = null;
                        };

                        if (newFilter == QuestState.active && pq != null)
                            showCmdrQuestSummary(pq!);
                        else
                            showQuestDef(qd);
                    },
                };
            }).ToList();
            addStack(lines.Where(c => c != null).Cast<Ctrl>());
            if (changeCurrent)
                ctrlCurrent = stack.FirstOrDefault() ?? ctrlCurrent;

            this.Invalidate();
        }
    }

    private void loadCmdrQuests()
    {
        // count how many in each state + the devQuest too
        var counts = new Dictionary<QuestState, int>();
        if (PlayState.current?.devQuest != null)
            counts[QuestState.active] = counts.GetValueOrDefault(QuestState.active) + 1;
        foreach (var cq in cmdrQuests)
            counts[cq.state] = counts.GetValueOrDefault(cq.state) + 1;

        // show 4 filter buttons
        tops = addCtrl(
            new BtnFillTextCtrl
            {
                offset = new(-10, 14),
                autoSize = true,
                text = "Failed" + (counts.GetValueOrDefault(QuestState.failed) == 0 ? null : $": {counts.GetValueOrDefault(QuestState.failed)}"),
                onClick = (b, _) => setFilter(b, QuestState.failed),
                disabled = counts.GetValueOrDefault(QuestState.failed) == 0
            },
            new BtnFillTextCtrl
            {
                follow = Side.Right,
                autoSize = true,
                text = "Completed" + (counts.GetValueOrDefault(QuestState.complete) == 0 ? null : $": {counts.GetValueOrDefault(QuestState.complete)}"),
                onClick = (b, _) => setFilter(b, QuestState.complete),
                disabled = counts.GetValueOrDefault(QuestState.complete) == 0
            },
            new BtnFillTextCtrl
            {
                follow = Side.Right,
                autoSize = true,
                text = "Paused" + (counts.GetValueOrDefault(QuestState.paused) == 0 ? null : $": {counts.GetValueOrDefault(QuestState.paused)}"),
                onClick = (b, _) => setFilter(b, QuestState.paused),
                disabled = counts.GetValueOrDefault(QuestState.paused) == 0
            },
            new BtnFillTextCtrl
            {
                follow = Side.Right,
                autoSize = true,
                text = "Active" + (counts.GetValueOrDefault(QuestState.active) == 0 ? null : $": {counts.GetValueOrDefault(QuestState.active)}"),
                onClick = (b, _) => setFilter(b, QuestState.active)
            },
            new TextCtrl
            {
                follow = Side.Right,
                autoSize = true,
                color = C.orange,
                font = GameColors.Fonts.arial_16B,
                text = "Quests:",
            }
        );

        // and default to 'Active'
        var btnActive = tops[tops.Count - 2];
        setFilter(btnActive, QuestState.active, true);

        this.Invalidate();
    }

    #endregion
}

class BtnMsgsEnvelope : BtnFillDrawCtrl
{
    override public string ToString() => "MsgsEnvelope";

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        var hasUnread = PlayState.current?.messagesUnread > 0;

        // draw background
        var backColor = hasUnread
            ? ColorSet.csCyanBack.get(isCurrent, isClicking, disabled)
            : ColorSet.csBack.get(isCurrent, isClicking, disabled);
        if (clicking == 1 || clicking == 3) backColor = C.menuGold;

        if (backBrush == null || backBrush.Color != backColor)
        {
            backBrush?.Dispose();
            backBrush = null;
            if (backColor != Color.Transparent)
                backBrush = backColor.toBrush();
        }
        if (backBrush != null)
            g.FillRectangle(backBrush, r);


        // draw foreground
        iconColor = hasUnread
            ? ColorSet.csCyanFore.get(isCurrent, isClicking, disabled)
            : ColorSet.csForeIcon.get(isCurrent, isClicking, disabled);
        if (iconPen == null || iconPen.Color != iconColor)
        {
            iconPen?.Dispose();
            iconPen = iconColor.toPen(3, LineCap.Round);
        }

        var iconBrush = isCurrent ? C.Brushes.grey : hasUnread ? C.Brushes.cyanDark : null;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        PlotQuestMini.drawEnvelope(g, r.X + 9, r.Y + 16, 53, iconPen, iconBrush);
        g.SmoothingMode = SmoothingMode.Default;

        if (sideBar)
        {
            Color? sideColor = !hasUnread ? null : isCurrent ? C.cyanDark : C.cyan;
            this.renderSideBar(g, isCurrent, sideColor);
        }

        return false;
    }
}

class QuestCatalogLine : BtnFillCtrl
{
    public required DefQuest qd;
    public QuestCmdrStatus? qs;

    override public string ToString() => this.qd.title;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        r.Width = form.scrollWidth; // match scrollBox width

        // render background
        var redraw = base.render(g, tt, isCurrent, isClicking, prior);

        // highlight if this is the devQuest
        if (PlayState.current?.devQuest?.quest == this.qd)
            g.FillRectangle(C.Brushes.orangeDiag, r.X, r.Y, 34, r.Height);

        tt.dty = r.Y + 4;
        tt.dtx = r.X + 4;
        PlotQuestMini.drawLogo(g, tt.dtx + (isCurrent ? 0 : 6), tt.dty, isCurrent, isCurrent ? 24 : 18);

        tt.dtx += 32;
        var x = tt.dtx;

        // render current state?
        var state = qd.equals(PlayState.current?.devQuest?.quest) ? "DEV" : qs?.state.ToString();
        if (state != null)
            tt.drawRight(form.scrollBox.Right - 6, state, isCurrent ? C.black : C.menuGold, GameColors.Fonts.arial_8);

        // title
        tt.draw(x, this.qd.title, ColorSet.csFore.get(isCurrent, isClicking, disabled), GameColors.Fonts.arial_16);
        tt.newLine(4, true);
        // draw subTitle as a single line with ...
        var rr = new Rectangle((int)x, (int)(tt.dty), (int)r.Width - 32, 32);
        TextRenderer.DrawText(g, qd.subTitle ?? qd.desc, GameColors.Fonts.arial_12, rr, isCurrent ? C.black : C.oranged, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
        tt.newLine(4, true);

        // set our hight to be as larged as we needed
        return adjustHeight(tt.pad().Height - r.Y);
    }
}


internal class QuestCatalogItem : Ctrl
{
    public required DefQuest qd;
    public QuestCmdrStatus? qs;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        r.Width = form.scrollWidth; // match scrollBox width
        tt.dty = r.Y + 4;
        var x = r.X + 4;
        var w = (int)(r.Width + x - 4);

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 8;

        if (qd.equals(PlayState.current?.devQuest?.quest)) g.FillRectangle(C.Brushes.orangeDiag, r.X, tt.dty - 4, r.Width, 44); // highlight if from DevQuest

        // the title
        tt.draw(x, qd.title, C.oranger, GameColors.Fonts.arial_20);
        tt.newLine(10, true);

        // desc with an orange bar
        var y = tt.dty;
        tt.dty += 8;
        var sz = tt.drawWrapped(x + 10, w, qd.desc);
        tt.newLine(8, true);
        g.FillRectangle(C.Brushes.orangeDark, r.X, y, 10, tt.dty - y);
        tt.dty += 8;

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 10;

        // TODO: make seperate ctrls + publisher is a copy button

        // publisher and version
        tt.font = GameColors.Fonts.arial_9;
        tt.draw(x, "Publisher: ");
        tt.draw(qd.publisher, C.oranger);
        tt.newLine(6, true);
        tt.draw(x, $"Version: ");
        tt.draw(qd.ver.ToString(), C.oranger);
        tt.newLine(6, true);
        tt.draw(x, $"Status: ");
        tt.draw(qs?.state.ToString() ?? "none", C.oranger);
        tt.newLine(10, true);

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 8;

        // duration
        tt.draw(x, "Duration: ");
        tt.draw(qd.duration.ToString(), C.oranger);
        tt.draw($" ({DefQuest.mapQuestDuration.GetValueOrDefault(qd.duration)})", C.oranged);
        tt.newLine(true);
        tt.font = GameColors.Fonts.arial_12;
        return adjustHeight(tt.dty - r.Y);
    }
}


class MessageLine : BtnFillCtrl
{
    public required PlayMsg pm;
    public required DefMsg? qm;

    override public string ToString() => this.pm.subject ?? pm.body ?? "?";

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        r.Width = form.scrollWidth; // match scrollBox width        
        if (pm.read)
            base.render(g, tt, isCurrent, isClicking, prior);

        // choose colours
        var textColor = ColorSet.csFore.get(isCurrent, isClicking, disabled);
        var iconPen = pm.read ? C.Pens.orange2r : C.Pens.menuGold2r;
        var iconBrush = C.Brushes.orangeDark;
        if (!pm.read)
        {
            textColor = C.cyan;
            iconPen = C.Pens.cyan2r;
            iconBrush = C.Brushes.cyanDark;

            // render different background colour if we are unread
            var backColor = isCurrent ? C.cyan : C.cyanDarker;
            if (backBrush == null || backBrush.Color != backColor)
            {
                backBrush?.Dispose();
                backBrush = backColor.toBrush();
            }
            g.FillRectangle(backBrush, r);
        }

        if (isCurrent)
        {
            textColor = C.black;
            iconPen = C.Pens.black3r;
        }

        if (pm.parent.dev) g.FillRectangle(C.Brushes.orangeDiag, r.X, r.Y, 44, r.Height); // highlight if from DevQuest

        // envelope
        tt.dty = r.Y + 4;
        tt.dtx = r.X + 8;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        PlotQuestMini.drawEnvelope(g, tt.dtx, tt.dty + 4, N.twoEight, iconPen, iconBrush);
        g.SmoothingMode = SmoothingMode.Default;

        tt.dtx += 40;
        var x = tt.dtx;

        // received time on right side
        var time = pm.received.Subtract(DateTime.Now).TotalDays < 1
            ? pm.received.ToString("HH:mm")
            : pm.received.AddYears(1286).UtcDateTime.ToString("dd MMM yyyy - HH:mm");
        tt.drawRight(form.scrollBox.Right - 6, time, textColor, GameColors.Fonts.arial_9);

        // message from
        var from = pm.from ?? qm?.from;
        tt.draw(x, string.IsNullOrEmpty(from) ? "?" : from, textColor, GameColors.Fonts.arial_12);
        tt.newLine(4, true);

        // subject
        var subject = pm.subject ?? qm?.subject ?? pm.body ?? qm?.body ?? "";
        var oldFlags = tt.flags;
        tt.flags |= TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform;
        tt.draw(x, subject, textColor, GameColors.Fonts.arial_16);
        tt.flags = oldFlags;
        tt.newLine(4, true);

        // set our hight to be as larged as we needed
        return adjustHeight(tt.pad().Height - r.Y);
    }
}


internal class MessageItem : Ctrl
{
    public required PlayMsg pm;
    public required DefMsg? qm;
    public required string body;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        r.Width = form.scrollWidth; // match scrollBox width
        var w = (int)(r.Width + r.X - 0);
        tt.dty += 1;

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 12;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (pm.parent.dev) g.FillRectangle(C.Brushes.orangeDiag, r.X, r.Y + 2, 60, 84); // highlight if from DevQuest
        PlotQuestMini.drawEnvelope(g, r.X + 2, tt.dty, 56, C.Pens.orange3r, C.Brushes.orangeDark);
        g.SmoothingMode = SmoothingMode.Default;

        var x2 = r.X + 72;
        var x3 = r.X + 122;

        // from
        var from = pm.from ?? qm?.from;
        tt.draw(x2, "From: ", GameColors.Fonts.arial_9);
        tt.draw(x3, string.IsNullOrEmpty(from) ? "?" : from, C.oranger, GameColors.Fonts.arial_12);
        tt.newLine(2, true);

        // sent
        var time = pm.received.AddYears(1286).UtcDateTime.ToString("dd MMM yyyy HH:mm");
        tt.draw(x2, "Sent: ", GameColors.Fonts.arial_9);
        tt.draw(x3, time, C.oranger, GameColors.Fonts.arial_12);
        tt.newLine(2, true);

        // subject
        var subject = pm.subject ?? qm?.subject ?? pm.body;
        if (!string.IsNullOrEmpty(subject))
        {
            tt.draw(x2, "Subject: ", GameColors.Fonts.arial_9);
            tt.draw(x3, subject, C.oranger, GameColors.Fonts.arial_16);
            tt.newLine(8, true);
        }
        else
        {
            tt.dty += 8;
        }

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 8;

        // body
        var y = tt.dty;
        var sz = tt.drawWrapped(r.X + 20, w - 10, body, C.oranger, GameColors.Fonts.arial_13);
        tt.newLine(true);

        g.FillRectangle(C.Brushes.orangeDark, r.X, y, 10, tt.dty - y);
        tt.dty += 8;

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 12;

        // set our hight to be as larged as we needed
        return adjustHeight(tt.pad(0, 12).Height - r.Y);
    }
}

internal class QuestSummary : Ctrl
{
    public required PlayQuest pq;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        r.Width = form.scrollWidth; // match scrollBox width
        var w = (int)(r.Width + r.X - 0);
        tt.dty += 1;

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);
        tt.dty += 12;
        var x = tt.dtx;
        var x2 = tt.dtx + 32;

        if (pq.dev) g.FillRectangle(C.Brushes.orangeDiag, r.X, tt.dty - 4, r.Width, 36); // highlight if from DevQuest

        // title
        tt.draw(x, pq.quest.title, C.oranger, GameColors.Fonts.arial_16B);
        tt.newLine(6, true);

        // list objectives
        tt.draw(x, "Objectives:", C.oranged);
        tt.newLine(10, true);
        tt.dtx = x2;
        foreach (var (key, obj) in pq.objectives)
            if (obj.state == PlayObjective.State.visible)
                PanelQuest.drawObjective(g, tt, C.orange, key, obj, pq, false, null, r.X);

        // time played
        tt.dty += 10;
        tt.draw(x, "Started: ", C.oranged);
        var duration = DateTimeOffset.Now.Subtract(pq.startTime!.Value);
        tt.draw(Util.timeSpanToString(duration), C.oranger);
        tt.newLine(6, true);

        tt.dty += 6;
        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, w, 0);

        // set our hight to be as larged as we needed
        return adjustHeight(tt.pad(0, 12).Height - r.Y);
    }
}
