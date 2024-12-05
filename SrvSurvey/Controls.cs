using SrvSurvey.game;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static SrvSurvey.net.GetSystemResponse;

namespace SrvSurvey
{
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
        private Button? bc;

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

            // apply some defaults
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
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
        public bool UseClearButton
        {
            get => this.bc != null;
            set
            {
                // remove if needed
                if (value == false && this.bc != null)
                {
                    this.Controls.Remove(this.bc);
                    this.bc = null;

                    tb.Width = this.ClientSize.Width;

                }
                else if (value == true && this.bc == null)
                {
                    createButtonClear();
                }

                //   tb.Visible = bc == null;

                tb.BackColor = Color.Cyan;
            }
        }

        private void createButtonClear()
        {
            this.bc = new FlatButton()
            {
                Anchor = AnchorStyles.Right,
                AutoSize = false,
            };

            bc.Size = new Size(16, 16);
            bc.Left = this.ClientSize.Width - bc.Width - 1;
            bc.Top = Util.centerIn(this.Height, bc.Height) - 1;
            bc.Paint += B_Paint;
            bc.Click += Bc_Click;

            tb.Width = bc.Left;

            this.Controls.Add(bc);
            this.Controls.SetChildIndex(bc, 0);
        }

        private void Bc_Click(object? sender, EventArgs e)
        {
            tb.Text = "";
            tb.Focus();
        }

        private void B_Paint(object? sender, PaintEventArgs e)
        {
            if (bc == null || this.IsDisposed) return;
            const int w = 3;
            e.Graphics.DrawLine(SystemPens.WindowText, w, w, bc.Width - 1 - w, bc.Height - 1 - w);
            e.Graphics.DrawLine(SystemPens.WindowText, w, bc.Width - 1 - w, bc.Height - 1 - w, w);
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

        public string cmdrName { get => this.SelectedText; }

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


    /// <summary>
    /// A ComboBox for choosing known star systems
    /// </summary>
    internal class ComboStarSystem : ComboBox
    {
        private List<MinMax> matches = new();
        private string lastQuery = "";

        public ComboStarSystem() : base()
        {
            this.DropDownStyle = ComboBoxStyle.DropDown;
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            Game.log("OnSelectedValueChanged");
        }

        public MinMax? SelectedSystem
        {
            get => this.matches[this.SelectedIndex];
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Game.log("OnTextChanged");

            var query = this.Text;
            if (string.IsNullOrWhiteSpace(query)) return;

            // before deferring...
            // do we have an exact match already?
            var knownMatch = this.matches.FirstOrDefault(m => m.name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (knownMatch != null)
            {
                Game.log($"knownMatch: {knownMatch}");
                this.SelectedItem = knownMatch.name;
                this.SelectionLength = 0;
                this.SelectionStart = knownMatch.name.Length;
                return;
            }

            // TODO: do we have a match with current results?

            Util.deferAfter(250, () => lookupSystems(query));
        }

        protected override void OnTextUpdate(EventArgs e)
        {
            base.OnTextUpdate(e);
            Game.log("OnTextUpdate");
        }

        private void lookupSystems(string query)
        {
            Game.log($"lookupSystems: {query} / lastQuery: {lastQuery}");

            var parentForm = this.FindForm();
            Game.spansh.getSystems(query).continueOnMain(parentForm, results =>
            {
                if (this.Text != query)
                {
                    Game.log($">> NOPE! {query} vs {this.Text}");
                    return; // stop if things already changed
                }

                var rem = results.values.FirstOrDefault();
                if (rem == null || rem.Length < query.Length) return;
                rem = rem.Substring(query.Length);

                this.matches.Clear();
                this.matches.AddRange(results.min_max);
                this.Items.Clear();
                this.Items.AddRange(results.values.ToArray());

                this.lastQuery = query;
                //Game.log($">> '{query}'+'{rem}'");

                // force cursor to end
                this.Text = query + rem;
                this.SelectionStart = query.Length;
                this.SelectionLength = rem.Length;

                if (!this.DroppedDown && !string.IsNullOrWhiteSpace(rem))
                    this.DroppedDown = true;
            });
        }
    }
}
