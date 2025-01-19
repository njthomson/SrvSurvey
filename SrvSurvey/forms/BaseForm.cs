using SrvSurvey.game;
using SrvSurvey.Properties;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Reflection;

namespace SrvSurvey.forms
{
    [System.ComponentModel.DesignerCategory("")]
    internal class BaseForm : Form
    {
        protected bool trackPosition = true;

        public BaseForm()
        {
            this.DoubleBuffered = true;
            this.Icon = Icons.logo;

            this.isDraggable = this.GetType().GetCustomAttribute<DraggableAttribute>() != null;
            this.trackPosition = this.GetType().GetCustomAttribute<TrackPositionAttribute>() != null;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                if (this.ShowInTaskbar && this.MinimizeBox)
                {
                    cp.ClassStyle |= 0x0008; // CS_DBLCLKS
                    cp.Style |= 0x00020000; // WS_MINIMIZEBOX
                }

                return cp;
            }
        }

        #region sizing and locations

        private static Dictionary<string, BaseForm> activeForms = new Dictionary<string, BaseForm>();

        public static T show<T>(Form? parent = null) where T : BaseForm, new()
        {
            var name = typeof(T).Name;

            var form = activeForms.GetValueOrDefault(name) as T;

            if (form == null)
            {
                form = new T() { Name = name, };
                activeForms[name] = form;
            }

            // apply previous location?
            if (form.trackPosition) form.applySavedLocation();

            form.beforeShowing();

            Util.showForm(form, parent);
            return (T)form;
        }

        /// <summary>
        /// Override to perform some actions when about to be shown by BaseForm.show() 
        /// </summary>
        protected virtual void beforeShowing() { }

        public static T? get<T>() where T : BaseForm
        {
            var name = typeof(T).Name;
            var form = activeForms.GetValueOrDefault(name) as T;
            return form;
        }

        public static void close<T>() where T : BaseForm
        {
            var form = BaseForm.get<T>();
            form?.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (trackPosition) this.saveLocation();

            activeForms.Remove(this.Name);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            if (trackPosition) this.saveLocation();
        }

        private void saveLocation()
        {
            var savedRect = Game.settings.formLocations.GetValueOrDefault(this.Name);

            var rect = new Rectangle(this.Location, this.Size);
            if (savedRect != rect && this.WindowState == FormWindowState.Normal)
            {
                Game.settings.formLocations[this.Name] = rect;
                Game.settings.Save();
            }
        }

        protected void applySavedLocation()
        {
            // can we fit in our last location?
            var savedRect = Game.settings.formLocations.GetValueOrDefault(this.GetType().Name);
            if (savedRect.Size != Size.Empty)
            {
                var sizable = this.FormBorderStyle.ToString().StartsWith("Sizable");
                Util.useLastLocation(this, savedRect, !sizable);
            }
            else
            {
                // start center of screen if we have no saved position
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        #endregion

        protected int scaleBy(int n)
        {
            return (int)(this.DeviceDpi / 96f * n);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.isDraggable)
                foreach (Control ctrl in this.Controls)
                    hookChildMouseEvents(ctrl);
        }

        #region mouse dragging

        /// <summary>
        /// Controls if the form is draggable by any reasonable control
        /// </summary>
        public bool isDraggable;

        private bool mouseDown;
        private Size startPoint;

        private void hookChildMouseEvents(Control ctrl)
        {
            // unless the control has editable text and not one of these ...
            if (ctrl.Cursor != Cursors.IBeam && !(ctrl is Button) && !(ctrl is ComboBox) && !(ctrl is StatusStrip))
            {
                // ... hook into their mouse events
                ctrl.MouseDown += new MouseEventHandler((object? sender, MouseEventArgs e) => startDragging(sender, e));
                ctrl.MouseUp += new MouseEventHandler((object? sender, MouseEventArgs e) => stopDragging());
                ctrl.MouseMove += new MouseEventHandler((object? sender, MouseEventArgs e) => onMouseMove(e));
                ctrl.MouseLeave += new EventHandler((object? sender, EventArgs e) => stopDragging());
            }

            foreach (Control child in ctrl.Controls)
                hookChildMouseEvents(child);
        }

        private void startDragging(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            mouseDown = true;
            startPoint = new Size(e.Location);
        }

        private void stopDragging()
        {
            mouseDown = false;
        }

        private void onMouseMove(MouseEventArgs e)
        {
            if (mouseDown && isDraggable)
            {
                var delta = e.Location - startPoint;
                this.Location = this.Location + (Size)delta;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            startDragging(this, e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            stopDragging();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            onMouseMove(e);
        }

        #endregion
    }

    [System.ComponentModel.DesignerCategory("Form")]
    internal class SizableForm : BaseForm
    {
    }

    [System.ComponentModel.DesignerCategory("Form")]
    internal class FixedForm : BaseForm
    {
        public FixedForm()
        {
            titleHeight = scaleBy(titleHeight);
        }

        #region fake title bar

        private int titleHeight = 24;

        public bool renderOwnTitleBar = true;

        private Button? fakeClose;
        private Button? fakeMinimize;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.DesignMode)
                renderOwnTitleBar = false;

            if (renderOwnTitleBar)
                this.prepFakeTitle();
        }

        private void prepFakeTitle()
        {
            // adjust locations for vertically anchored controls
            foreach (Control ctrl in this.Controls)
            {
                if ((ctrl.Anchor & AnchorStyles.Top) == AnchorStyles.Top)
                {
                    ctrl.Top += titleHeight;

                    if ((ctrl.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                        ctrl.Height -= titleHeight;
                }
            }

            // do this after or Bottom anchored controls shift with it
            this.Height += titleHeight;

            // add fake close
            fakeClose = new Button()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Text = "X",
                Font = GameColors.fontMiddle,
                AutoSize = false,
                Width = titleHeight,
                Height = titleHeight,
                Top = 0,
                Left = this.ClientSize.Width - titleHeight,
                FlatStyle = FlatStyle.Flat,
                BackColor = SystemColors.ControlDarkDark,
            };
            fakeClose.FlatAppearance.BorderSize = 0;
            fakeClose.FlatAppearance.MouseOverBackColor = Color.Firebrick;
            fakeClose.FlatAppearance.MouseDownBackColor = Color.DarkRed;
            fakeClose.Click += new EventHandler((object? sender, EventArgs args) => this.Close());
            this.Controls.Add(fakeClose);

            // add fake minimize
            if (this.MinimizeBox)
            {
                fakeMinimize = new Button()
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Text = "_",
                    Font = GameColors.fontMiddle,
                    AutoSize = false,
                    Width = titleHeight,
                    Height = titleHeight,
                    Top = 0,
                    Left = this.ClientSize.Width - titleHeight * 2,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = SystemColors.ControlDarkDark,
                };
                fakeMinimize.FlatAppearance.BorderSize = 0;
                fakeMinimize.FlatAppearance.MouseOverBackColor = Color.CadetBlue;
                fakeMinimize.FlatAppearance.MouseDownBackColor = Color.SteelBlue;
                fakeMinimize.Click += new EventHandler((object? sender, EventArgs args) => this.WindowState = FormWindowState.Minimized);
                this.Controls.Add(fakeMinimize);
            }

            this.FormBorderStyle = FormBorderStyle.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            if (this.renderOwnTitleBar)
            {
                g.FillRectangle(SystemBrushes.ControlDarkDark, 0, 0, this.Width, titleHeight);
                g.DrawRectangle(SystemPens.ControlDarkDark, 0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);

                // icon top left
                if (this.Icon != null)
                {
                    g.FillRectangle(GameColors.Icon.darkBlue, 0, 0, titleHeight, titleHeight);
                    g.DrawIcon(this.Icon, new Rectangle(0, 0, titleHeight, titleHeight));
                }

                // fake title
                var pt = new Point(scaleBy(28), scaleBy(2));
                TextRenderer.DrawText(g, this.Text ?? this.Text, GameColors.fontMiddleBold, pt, SystemColors.ControlLight);

                //g.DrawRectangle(SystemPens.WindowText, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

    }

    #endregion

    /// <summary> The location and size of this form will be tracked and stored in settings </summary>
    class TrackPositionAttribute : Attribute { }

    /// <summary> This form can be moved by dragging any (non text editable) part of the form </summary>
    class DraggableAttribute : Attribute { }

    // TODO: Attribute for forms that should be themed?
}
