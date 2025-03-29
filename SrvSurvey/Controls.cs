﻿using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey
{
    internal static class ControlExtensionMethods
    {
        /// <summary> Disables the control before setting the new value </summary>
        public static void SetValue(this NumericUpDown ctrl, decimal newValue)
        {
            ctrl.Enabled = false;
            if (ctrl.Value != newValue)
                ctrl.Value = newValue;
            ctrl.Enabled = true;
        }

        /// <summary> Disables the control before setting the new value </summary>
        public static void SetText(this ComboBox ctrl, string newValue)
        {
            ctrl.Enabled = false;
            if (ctrl.Text != newValue)
                ctrl.Text = newValue;
            ctrl.Enabled = true;
        }

        /// <summary> Creates a ListViewSubItem with the given text and name </summary>
        public static ListViewItem.ListViewSubItem Add(this ListViewItem.ListViewSubItemCollection subItems, string? text, string name)
        {
            var subItem = subItems.Add(text);
            subItem.Name = name;

            return subItem;
        }

        /// <summary> Creates a ToolStripItem with the given text and tag </summary>
        public static ToolStripItem Add(this ToolStripItemCollection items, string text, object tag)
        {
            return Add(items, text, null, tag);
        }

        public static ToolStripItem Add(this ToolStripItemCollection items, string text, string? name, object tag)
        {
            var newItem = items.Add(text);
            if (name != null)
                newItem.Name = name;
            newItem.Tag = tag;

            return newItem;
        }

        /// <summary> Creates a ToolStripItem with the given text and tag </summary>
        public static ListViewItem Add(this ListView.ListViewItemCollection items, string text, string name, object? tag)
        {
            var newItem = items.Add(text);
            newItem.Name = name;
            newItem.Tag = tag;

            return newItem;
        }

        public static void setChildrenEnabled(this Control parent, bool enabled, params Control[] except)
        {
            foreach (Control child in parent.Controls)
                if (!except.Contains(child))
                    child.Enabled = enabled;
        }
    }

    class FlatButton : Button
    {
        public FlatButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.MouseDownBackColor = SystemColors.ActiveCaption;
            this.FlatAppearance.MouseOverBackColor = SystemColors.InactiveCaption;
        }

        [DefaultValue(FlatStyle.Flat)]
        [Localizable(true)]
        public new FlatStyle FlatStyle
        {
            get => base.FlatStyle;
            set => base.FlatStyle = value;
        }

        public static void applyGameTheme(params FlatButton[] buttons)
        {
            foreach (var btn in buttons)
            {
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 64, 64, 64);
                btn.FlatAppearance.MouseDownBackColor = GameColors.OrangeDim;
            }
        }

        private Color? originalBackColor;

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            // Disabled buttons barely look any different - so change BackColor to make it obvious
            if (!this.Enabled && this.originalBackColor == null)
            {
                this.originalBackColor = this.BackColor;
                this.BackColor = SystemColors.WindowFrame;
            }
            else if (this.Enabled && this.originalBackColor != null)
            {
                this.BackColor = this.originalBackColor.Value;
                this.originalBackColor = null;
            }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (this.originalBackColor != null)
                    this.originalBackColor = value;
                else
                    base.BackColor = value;
            }
        }
    }

    public class TreeView2 : TreeView
    {
        public bool doNotPaint = false;

        public TreeView2()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //Debug.WriteLine($"OnPaintBackground: {Name} / {doNotPaint}");
            if (doNotPaint) return;

            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Debug.WriteLine($"OnPaint: {Name} / {doNotPaint}");
            if (doNotPaint) return;

            base.OnPaint(e);
        }
    }

    public class TextBox2 : Panel
    {
        private TextBox tb;
        public Button? eb { get; private set; }

        public TextBox2()
        {
            tb = new TextBox()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
                Font = this.Font,
                ForeColor = this.ForeColor,
                Width = this.ClientSize.Width,
            };
            this.Controls.Add(tb);
            tb.TextChanged += tb_TextChanged;

            // apply some defaults
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
        }

        private void tb_TextChanged(object? sender, EventArgs e)
        {
            this.OnTextChanged(e);
            if (this.TextChanged2 != null) TextChanged2(sender, e);
        }

        #region TextBox overrides

        [AllowNull]
        [Localizable(true)]
        [Browsable(true)]
        public override string Text
        {
            get => tb.Text;
            set => tb.Text = value;
        }

        [Browsable(true)]
        public event EventHandler? TextChanged2;

        public override Color BackColor
        {
            get => base.BackColor;
            set { base.BackColor = value; tb.BackColor = value; }
        }

        public override Color ForeColor
        {
            get => base.ForeColor;
            set { base.ForeColor = value; tb.ForeColor = value; }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            tb.Enabled = this.Enabled;
        }

        [AllowNull]
        public override Font Font
        {
            get => base.Font;
            set { base.Font = value; tb.Font = value; }
        }

        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public bool ReadOnly
        {
            get => tb.ReadOnly;
            set => tb.ReadOnly = value;
        }

        [Localizable(true)]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get => tb.TextAlign;
            set => tb.TextAlign = value;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            tb.Top = Util.centerIn(this.ClientSize.Height, tb.Height);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return tb.GetPreferredSize(proposedSize);
        }

        #endregion

        #region Clear text code

        [Browsable(true)]
        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public EdgeButton UseEdgeButton
        {
            get => _edgeButtonType;
            set
            {
                if (_edgeButtonType == value) return;

                // remove if needed
                if (value == EdgeButton.None && eb != null)
                {
                    this.Controls.Remove(this.eb);
                    this.eb = null;

                    tb.Width = this.ClientSize.Width;
                }
                else if (value != EdgeButton.None && this.eb == null)
                {
                    createEdgeButton();
                }
                else if (eb != null)
                {
                    // force a redraw if the type is changing
                    eb.Invalidate();
                }

                _edgeButtonType = value;
            }
        }
        private EdgeButton _edgeButtonType;

        private void createEdgeButton()
        {
            this.eb = new FlatButton()
            {
                Anchor = AnchorStyles.Right,
                AutoSize = false,
            };

            eb.Size = new Size(16, 16);
            eb.Left = this.ClientSize.Width - eb.Width - 1;
            eb.Top = Util.centerIn(this.Height, eb.Height) - 1;
            eb.Paint += eb_Paint;
            eb.Click += eb_Click;

            tb.Width = eb.Left;

            this.Controls.Add(eb);
            this.Controls.SetChildIndex(eb, 0);
        }

        private void eb_Click(object? sender, EventArgs e)
        {
            if (this._edgeButtonType == EdgeButton.Clear)
            {
                tb.Text = "";
                tb.Focus();
            }
        }

        private void eb_Paint(object? sender, PaintEventArgs e)
        {
            if (eb == null || this.IsDisposed) return;
            const int w = 3;
            var y = eb.Height - 5;

            if (_edgeButtonType == EdgeButton.Clear)
            {
                // Draw an X
                e.Graphics.DrawLine(SystemPens.WindowText, w, w, eb.Width - 1 - w, eb.Height - 1 - w);
                e.Graphics.DrawLine(SystemPens.WindowText, w, eb.Width - 1 - w, eb.Height - 1 - w, w);
            }
            else if (_edgeButtonType == EdgeButton.Dots)
            {
                // 3 dots
                e.Graphics.FillEllipse(SystemBrushes.WindowText, 3, y, 3, -3);
                e.Graphics.FillEllipse(SystemBrushes.WindowText, 6, y, 3, -3);
                e.Graphics.FillEllipse(SystemBrushes.WindowText, 9, y, 3, -3);
            }
        }

        public enum EdgeButton
        {
            None,
            Clear,
            Dots
        }

        #endregion
    }

    /// <summary>
    /// A ComboBox for choosing known Commanders
    /// </summary>
    public class ComboCmdr : ComboBox
    {
        public Dictionary<string, string>? allCmdrs { get; private set; }

        public ComboCmdr() : base()
        {
            // preset properties
            this.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (allCmdrs == null)
                this.loadCmdrs();
        }

        private void loadCmdrs()
        {
            if (this.DesignMode) return;

            if (this.allCmdrs == null)
                this.allCmdrs = CommanderSettings.getAllCmdrs();

            var cmdrs = this.allCmdrs
                .Values
                .Order()
                .ToArray();

            this.Items.Clear();
            this.Items.AddRange(cmdrs);

            if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
                this.SelectedItem = Game.settings.preferredCommander;
            else if (!string.IsNullOrEmpty(Game.settings.lastCommander))
                this.SelectedItem = Game.settings.lastCommander;
            else
                this.SelectedIndex = 0;
        }

        public string cmdrName { get => this.SelectedItem?.ToString()!; }

        public string? cmdrFid
        {
            get => this.allCmdrs?.First(_ => _.Value == (this.SelectedItem as string)).Key;
            set
            {
                if (!string.IsNullOrEmpty(value) && !this.DesignMode)
                {
                    if (allCmdrs == null) this.loadCmdrs();

                    var newItem = this.allCmdrs?.GetValueOrDefault(value);
                    if (newItem != null)
                        this.SelectedItem = newItem;
                }
            }
        }
    }

    internal delegate void StarSystemChanged(StarRef? starSystem);

    /// <summary>
    /// A ComboBox for choosing known star systems
    /// </summary>
    internal class ComboStarSystem : ComboBox
    {
        public event StarSystemChanged? selectedSystemChanged;

        private readonly List<StarRef> matches = new();
        private string lastQuery = "";
        private bool _updateOnJump;
        private Action? unsub = null;
        private bool updating;

        public ComboStarSystem() : base()
        {
            this.DropDownStyle = ComboBoxStyle.DropDown;
            if (!this.DesignMode)
                this.addCurrentSystemItem();
        }

        private string? currentSystemItem
        {
            get
            {
                if (Game.activeGame?.cmdr?.currentSystem == null)
                    return null;
                else
                    return $"(Current: {Game.activeGame.cmdr.currentSystem})";
            }
        }

        private void addCurrentSystemItem()
        {
            if (currentSystemItem != null && false)
                this.Items.Insert(0, currentSystemItem);
        }

        /// <summary>
        /// Automatically update the text on this control when FSD jumps occur.
        /// </summary>
        [Browsable(true)]
        public bool updateOnJump
        {
            get => _updateOnJump;
            set
            {
                if (value && !_updateOnJump)
                    watchForJumps();
                else if (unsub != null)
                    unsub();

                _updateOnJump = value;
            }
        }

        private void watchForJumps()
        {
            if (this.DesignMode) return;

            var journals = Game.activeGame?.journals;
            if (journals == null) return;

            // primary lambda to call when new journal entries
            var func = new OnJournalEntry((JournalEntry entry, int index) =>
            {
                if (this.IsDisposed) return;

                var fsdJump = entry as FSDJump;
                if (fsdJump != null)
                    this.SelectedSystem = StarRef.from(fsdJump);
            });

            // a clean-up lambda with references to the above
            this.unsub = new Action(() =>
            {
                journals.onJournalEntry -= func;
                this.unsub = null;
            });

            journals.onJournalEntry += func;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.unsub != null) unsub();
            }

            base.Dispose(disposing);
        }

        private void Journals_onJournalEntry(JournalEntry entry, int index)
        {
            if (this.IsDisposed) return;

            // Update our text anything we jump to a new system
            var fsdJump = entry as FSDJump;
            if (fsdJump != null)
                this.Text = fsdJump.StarSystem;
        }

        public StarRef? SelectedSystem
        {
            get => _selectedSystem;

            set
            {
                if (_selectedSystem == value)
                    return;

                this.updating = true;

                _selectedSystem = value;

                if (value == null || !this.matches.Contains(value))
                {
                    this.matches.Clear();
                    if (value != null) this.matches.Add(value);

                    this.Items.Clear();
                    this.addCurrentSystemItem();
                }

                if (value != null)
                {
                    if (!this.Items.Contains(value.name))
                        this.Items.Add(value);

                    if (this.SelectedItem as string != value.name)
                    {
                        this.SelectedItem = value;

                        this.SelectionLength = 0;
                        this.SelectionStart = value.name.Length;
                    }
                }

                this.updating = false;

                // finally, fire the event
                this.fireEvent();
            }
        }

        private void fireEvent()
        {
            if (this.selectedSystemChanged != null)
            {
                if (this.InvokeRequired)
                    this.Invoke(() => this.selectedSystemChanged(this._selectedSystem));
                else
                    this.selectedSystemChanged(this._selectedSystem);
            }
        }

        private StarRef? _selectedSystem;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (!this.Enabled || this.updating || this.DesignMode) return;
            //Game.log("OnTextChanged");

            var query = this.Text;
            if (string.IsNullOrWhiteSpace(query)) return;
            if (query == lastQuery) return;

            if (this.Text.StartsWith("(Current:") && Game.activeGame?.cmdr?.currentSystem != null)
            {
                Program.defer(() => this.Text = Game.activeGame.cmdr.currentSystem);
                return;
            }

            // before deferring...

            // TODO: do we have a match with current results?

            Util.deferAfter(250, () =>
            {
                // do we have an exact match already?
                var knownMatch = this.matches.FirstOrDefault(m => m.name.Equals(query, StringComparison.OrdinalIgnoreCase));
                if (knownMatch != null)
                {
                    Game.log($"knownMatch: {knownMatch}");
                    this.SelectedSystem = knownMatch;
                    this.lastQuery = query;
                    return;
                }

                lookupSystems(query);
            });
        }

        private void lookupSystems(string query)
        {
            Game.log($"lookupSystems: {query} / lastQuery: {lastQuery}");

            var parentForm = this.FindForm()!;
            Game.spansh.getSystems(query).continueOnMain(parentForm, results =>
            {
                if (this.Text != query)
                {
                    Game.log($">> NOPE! {query} vs {this.Text}");
                    this.updating = false;
                    return; // stop if things already changed
                }

                this.updating = true;

                var rem = results.values.FirstOrDefault();
                if (rem == null || rem.Length < query.Length)
                {
                    this.updating = false;
                    return;
                }
                rem = rem.Substring(query.Length);

                this.matches.Clear();
                this.matches.AddRange(results.min_max);
                var fireEvent = this._selectedSystem != this.matches.FirstOrDefault();
                this._selectedSystem = this.matches.FirstOrDefault();

                this.Items.Clear();
                this.addCurrentSystemItem();
                this.Items.AddRange(results.values.ToArray());

                this.lastQuery = query;
                //Game.log($">> '{query}'+'{rem}'");


                if (this.ContainsFocus && !this.DroppedDown && !string.IsNullOrWhiteSpace(rem))
                {
                    this.DroppedDown = true;
                    // Force the cursor to reappear
                    parentForm.Cursor = parentForm.Cursor;
                }

                // force cursor to end
                this.Text = query + rem;
                this.SelectionStart = query.Length;
                this.SelectionLength = rem.Length;

                if (rem.Length == 0)
                    this.SelectedItem = this.Text;

                this.updating = false;

                if (fireEvent)
                    this.fireEvent();
            });
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                var starRef = Game.activeGame?.cmdr?.getCurrentStarRef();
                if (starRef != null)
                    SelectedSystem = starRef;
            }
        }
    }

    internal class ButtonContextMenuStrip : ContextMenuStrip
    {
        private Button? _button;
        private bool menuVisible;

        public ButtonContextMenuStrip() : base() { }
        public ButtonContextMenuStrip(IContainer container) : base(container) { }

        /// <summary> Show the menu off of the target button </summary>
        public void ShowOnTarget()
        {
            if (_button != null)
                base.Show(_button, new Point(0, _button!.Height));
        }

        [Browsable(true)]
        public Button? targetButton
        {
            get => _button;

            set
            {
                if (_button != null && !this.DesignMode)
                    _button.MouseDown -= _button_MouseDown;

                _button = value;

                if (_button != null && !this.DesignMode)
                    _button.MouseDown += _button_MouseDown;
            }
        }

        private void _button_MouseDown(object? sender, MouseEventArgs e)
        {
            if (this.DesignMode) return;

            Game.log($"menuVisible: {menuVisible}, _button: {_button}");

            if (!menuVisible && _button != null)
            {
                this.ShowOnTarget();
                //_button.Text = "⏶";
                this.menuVisible = true;
                this.Capture = true;
            }
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            base.OnClosed(e);
            if (this.DesignMode) return;

            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(() =>
                {
                    this.menuVisible = false;
                    //_button.Text = "⏷";
                });
            }
        }

        protected override void OnMouseUp(MouseEventArgs mea)
        {
            if (menuVisible)
                this.AutoClose = false;

            base.OnMouseUp(mea);

            this.AutoClose = true;
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            base.OnItemClicked(e);

            this.AutoClose = true;
            this.Close(ToolStripDropDownCloseReason.ItemClicked);
        }
    }
}
