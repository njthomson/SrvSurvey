using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SrvSurvey
{
    internal partial class FormGenus : PlotBase
    {
        public FormGenus()
        {
            InitializeComponent();
            Util.useLastLocation(this, Game.settings.formGenusGuideLocation);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formGenusGuideLocation != rect)
            {
                Game.settings.formGenusGuideLocation = rect;
                Game.settings.Save();
            }
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            try
            {
                e.Graphics.DrawString("hello", this.Font, Brushes.Cyan, 44, 44);

            }
            catch (Exception ex)
            {
                Game.log($"FormGenus.OnPaintBackground error: {ex}");
            }
        }

        public override void reposition(Rectangle gameRect)
        {
            // no op
        }
    }
}