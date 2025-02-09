using SrvSurvey.game;
using SrvSurvey.widgets;

namespace SrvSurvey
{
    public partial class MainView : UserControl
    {
        private Graphics g;
        private TextCursor t;

        private CommanderSettings cmdr = CommanderSettings.LoadCurrentOrLast();

        public MainView()
        {
            InitializeComponent();

            this.Font = GameColors.Fonts.gothic_10;
            this.ForeColor = C.orange;
            this.BackColor = Color.Black;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BackgroundImage = GameGraphics.getBackgroundImage(this.Size, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Game.log($"**** OnPaintBackground {DateTime.Now} ****");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Game.log($"**** OnPaint {DateTime.Now} ****");
            base.OnPaint(e);

            g = e.Graphics;
            //g.Clear(Color.Transparent);
            this.t = new TextCursor(this.g, this);

            t.draw(N.eight, $"Commander:");
            t.newLine();
            t.draw(N.oneSix, cmdr.commander);
            t.draw(this.Width - N.oneSix, "more stuff", null, null, true);
        }
    }
}
