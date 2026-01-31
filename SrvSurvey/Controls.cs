using SrvSurvey.game;
using SrvSurvey.units;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Diagnostics;
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

        public static Control? last(this Control.ControlCollection controls)
        {
            if (controls.Count == 0)
                return null;
            else
                return controls[controls.Count - 1];
        }

        public static IEnumerable<T> findAll<T>(this Control ctrl) where T : Control
        {
            foreach (Control child in ctrl.Controls)
            {
                if (child is T thing)
                    yield return thing;
                foreach (var thing2 in findAll<T>(child))
                    yield return thing2;
            }
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
        private Button? eb;

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
            else if (this._edgeButtonType == EdgeButton.Paste)
            {
                tb.SelectAll();
                tb.Paste();
                tb.Focus();
                tb.SelectionStart = tb.Text.Length;
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
            else if (_edgeButtonType == EdgeButton.Paste)
            {
                // the paste image
                e.Graphics.DrawImage(Properties.ImageResources.paste1, 2, 2, 12, 12);
            }
            else if (_edgeButtonType != EdgeButton.None)
            {
                throw new Exception($"Unexpected EdgeButton type: {_edgeButtonType}");
            }
        }

        public enum EdgeButton
        {
            None,
            Clear,
            Dots,
            Paste
        }

        #endregion
    }

    /// <summary>
    /// A ComboBox for choosing known Commanders
    /// </summary>
    public class ComboCmdr : ComboBox
    {
        /// <summary> Fid => Commander </summary>
        public Dictionary<string, string> allCmdrs { get; private set; } = new();

        public ComboCmdr() : base()
        {
            // preset properties
            this.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (allCmdrs.Count == 0)
                this.loadCmdrs();
        }

        private void loadCmdrs()
        {
            if (this.DesignMode) return;

            if (this.allCmdrs.Count == 0)
                this.allCmdrs = CommanderSettings.getAllCmdrs();

            var cmdrs = this.allCmdrs
                .Values
                .Order()
                .ToArray();

            this.Items.Clear();
            this.Items.AddRange(cmdrs);

            if (this.Items.Count == 0)
            {
                this.Items.Add("No Commanders found");
                this.SelectedIndex = 0;
                this.Enabled = false;
                return;
            }

            if (Program.forceFid != null)
                this.SelectedItem = allCmdrs.GetValueOrDefault(Program.forceFid);
            else if (!string.IsNullOrEmpty(Game.settings.preferredCommander))
                this.SelectedItem = Game.settings.preferredCommander;
            else if (!string.IsNullOrEmpty(Game.settings.lastCommander))
                this.SelectedItem = Game.settings.lastCommander;
            else if (this.Items.Count > 0)
                this.SelectedIndex = 0;
        }

        public string cmdrName { get => this.SelectedItem?.ToString()!; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? cmdrFid
        {
            get => this.allCmdrs.FirstOrDefault(_ => _.Value == (this.SelectedItem as string)).Key;
            set
            {
                if (!string.IsNullOrEmpty(value) && !this.DesignMode)
                {
                    if (allCmdrs.Count == 0) this.loadCmdrs();

                    var newItem = this.allCmdrs.GetValueOrDefault(value);
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
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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

    public class DrawButton : Button
    {
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackColorHover { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackColorPressed { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackColorDisabled { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color ForeColorHover { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color ForeColorPressed { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color ForeColorDisabled { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool DrawBorder { get; set; }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool AnimateOnPress { get; set; }
        public bool mouseDown { get; private set; }
        public bool highlight { get; private set; }
        private Pen? borderPen;

        public DrawButton()
        {
            FlatStyle = FlatStyle.Flat;
            ForeColor = Color.Black;
            if (DesignMode)
            {
                BackColor = Color.Orange;
                BackColorHover = Color.Gold;
                BackColorPressed = Color.Yellow;
                BackColorPressed = Color.Gray;
            }
            else
            {
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.UserMouse, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.ResizeRedraw, true);

                setGameColors();
            }
        }

        public void setGameColors()
        {
#if DEBUG
            // This stops logging code from starting up when custom controls are created in Visual Studio designer
            if (Process.GetCurrentProcess().ProcessName != "SrvSurvey") return;
#endif

            FlatStyle = FlatStyle.Flat;

            BackColor = C.orangeDarker;
            BackColorHover = C.orangeDark;
            BackColorPressed = C.orange;
            BackColorDisabled = C.grey;

            ForeColor = C.orange;
            ForeColorHover = C.menuGold;
            ForeColorPressed = C.black;
            ForeColorDisabled = C.black;
        }

        public void setThemeColors(bool dark, bool black)
        {
            this.FlatStyle = FlatStyle.Flat;
            this.BackColor = black ? C.orangeDarker : dark ? SystemColors.ControlDark : SystemColors.ControlLight;

            this.BackColorHover = black ? C.orangeDark : SystemColors.InactiveCaption;
            this.BackColorPressed = black ? C.orange : SystemColors.ActiveCaption;
            this.BackColorDisabled = black ? C.grey : SystemColors.ScrollBar;

            this.ForeColor = black ? C.orange : SystemColors.ControlText;
            this.ForeColorHover = black ? C.menuGold : SystemColors.ControlText;
            this.ForeColorPressed = black ? C.black : SystemColors.ControlText;
            this.ForeColorDisabled = black ? C.black : SystemColors.GrayText;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            highlight = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            highlight = false;
            if (DesignMode) return;
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            mouseDown = true;
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            mouseDown = false;
            if (DesignMode) return;
            this.Invalidate();

            if (AnimateOnPress)
                this.doClickAnimation();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            highlight = true;
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            highlight = false;
            mouseDown = false;
            if (DesignMode) return;
            this.Invalidate();
        }

        public Pen pen(Pen normal, Pen hover, Pen pressed)
        {
            if (mouseDown)
                return pressed;
            else if (highlight)
                return hover;
            else
                return normal;
        }

        public Brush brush(Brush normal, Brush hover, Brush pressed)
        {
            if (mouseDown)
                return pressed;
            else if (highlight)
                return hover;
            else
                return normal;
        }

        public Color BackColor2
        {
            get
            {
                if (!Enabled)
                    return BackColorDisabled;
                else if (mouseDown)
                    return BackColorPressed;
                else if (highlight)
                    return BackColorHover;
                else
                    return BackColor;
            }
        }

        public Color ForeColor2
        {
            get
            {
                if (!Enabled)
                    return ForeColorDisabled;
                else if (mouseDown)
                    return ForeColorPressed;
                else if (highlight)
                    return ForeColorHover;
                else
                    return ForeColor;
            }
        }

        private int foo = 0;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (AnimateOnPress && keyData == Keys.Enter)
                doClickAnimation();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void doClickAnimation()
        {
            foo = 1;
            this.Invalidate();

            Util.deferAfter(50, () =>
            {
                foo = 2;
                this.Invalidate();
            });

            Util.deferAfter(250, () =>
            {
                foo = 0;
                this.Invalidate();
            });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                return;
            }

            var bc = BackColor2;
            if (foo == 1)
                e.Graphics.Clear(C.yellow);
            else if (foo == 2)
                e.Graphics.Clear(C.menuGold);
            else if (bc == Color.Transparent)
                ButtonRenderer.DrawParentBackground(e.Graphics, ClientRectangle, this);
            else
                e.Graphics.Clear(bc);

            var fc = ForeColor2;
            if (foo > 0)
                fc = C.black;

            if (DrawBorder)
            {
                if (borderPen?.Color != fc) borderPen = fc.toPen(1);
                e.Graphics.DrawRectangle(borderPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

                if (Focused)
                {
                    var r = new Rectangle(2, 2, ClientSize.Width - 4, ClientSize.Height - 4);
                    ControlPaint.DrawFocusRectangle(e.Graphics, r, ForeColor, BackColor);
                }
            }

            if (Text != null)
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, fc, TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            base.RaisePaintEvent(this, e);
        }
    }

    public class GroupBox2 : GroupBox
    {
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color LineColor { get; set; } = SystemColors.ActiveBorder;
        private Pen? linePen;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;

            var h = TextRenderer.MeasureText(g, Text, Font).Height / 2;

            if (linePen?.Color != LineColor) linePen = LineColor.toPen(1);
            g.DrawRectangle(linePen, 0, h, ClientRectangle.Width - 1, ClientRectangle.Height - h - 2);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, Text, Font, new Point(6, 0), ForeColor, BackColor);

            base.RaisePaintEvent(this, e);
        }
    }

    public class CheckBox2 : CheckBox
    {
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color LineColor { get; set; } = SystemColors.ActiveBorder;
        private Pen? linePen;
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color CheckColor { get; set; } = SystemColors.ControlText;
        private Pen? checkPen;
        private SolidBrush? checkBrush;

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color CheckBackColor;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;
            g.Clear(BackColor);

            var isBlack = BackColor == Color.Black;

            var sz = CheckBoxRenderer.GetGlyphSize(g, System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
            TextRenderer.DrawText(g, Text, Font, new Point(sz.Width + 2, 1), Enabled ? ForeColor : SystemColors.GrayText, BackColor);

            var lc = Enabled ? LineColor : isBlack ? ControlPaint.Dark(LineColor) : ControlPaint.LightLight(LineColor);
            if (linePen?.Color != lc) linePen = lc.toPen(1);

            var r = new Rectangle(1, 1, sz.Width, sz.Height);
            if (CheckBackColor != Color.Empty)
            {
                if (checkBrush?.Color != CheckBackColor) checkBrush = new SolidBrush(CheckBackColor);
                g.FillRectangle(checkBrush, r);
            }
            g.DrawRectangle(linePen, r);

            if (Checked)
            {
                var cc = Enabled ? CheckColor : isBlack ? ControlPaint.Dark(CheckColor) : ControlPaint.LightLight(CheckColor);
                if (checkPen?.Color != cc) checkPen = cc.toPen(1.5f, System.Drawing.Drawing2D.LineCap.Square);

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var h = 1 + (sz.Height / 2);
                g.DrawLineR(checkPen, 4, h, 3, 3);
                g.DrawLineR(checkPen, 7, h + 3, 4, -5.5f);
            }

            if (Focused)
                ControlPaint.DrawFocusRectangle(g, ClientRectangle, ForeColor, BackColor);

            base.RaisePaintEvent(this, e);
        }
    }
}
