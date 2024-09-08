using SrvSurvey.canonn;
using SrvSurvey.game;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SrvSurvey
{
    internal partial class FormBuilder : Form
    {
        public static FormBuilder? activeForm;

        public static FormBuilder show(CanonnStation station)
        {
            if (activeForm == null)
                FormBuilder.activeForm = new FormBuilder(station);

            Util.showForm(FormBuilder.activeForm);
            return FormBuilder.activeForm;
        }

        protected Game game = Game.activeGame!;
        public CanonnStation station;
        public HumanSiteTemplate template;

        public Building2 building = new Building2();

        public GraphicsPath? nextPath;
        public PointF lastPoint;
        public bool circle;
        public float circleRadius;

        private bool wasShields = false;
        private int floor = 1;
        private int level = 0;

        public FormBuilder(CanonnStation station)
        {
            InitializeComponent();
            this.station = station;

            // can we fit in our last location
            Util.useLastLocation(this, Game.settings.formBuilder.Location);

            game.status.StatusChanged += Status_StatusChanged;

            btnStartPolygon.Enabled = true;
            btnStartCircle.Enabled = true;
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var rect = new Rectangle(this.Location, this.Size);
            if (Game.settings.formBuilder != rect)
            {
                Game.settings.formBuilder = rect;
                Game.settings.Save();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            FormBuilder.activeForm = null;
        }

        private void Status_StatusChanged(bool blink)
        {
            if (wasShields != game.status.ShieldsUp)
            {
                // change!
                addPoint();
                wasShields = game.status.ShieldsUp;
            }
        }

        public decimal radius { get => game.status.PlanetRadius; }


        public PointF offset
        {
            get
            {
                var pf = (PointF)Util.getOffset(radius, station.location, station.heading);
                pf.Y *= -1;
                return pf;
            }

        }

        private void refreshPlotter()
        {
            Program.getPlotter<PlotHumanSite>()?.Invalidate();
            Elite.setFocusED();
        }

        private void btnStartPolygon_Click(object sender, EventArgs e)
        {
            // start polygon
            circle = false;
            this.lastPoint = offset;
            this.nextPath = new GraphicsPath();

            refreshPlotter();

            btnStartPolygon.Enabled = true;
            btnAddPoint.Enabled = true;
            btnEndPolygon.Enabled = true;

            btnStartCircle.Enabled = false;
            numCircle.Enabled = false;
            btnEndCircle.Enabled = false;

            btnCommitBuilding.Enabled = false;
            txtBuildingName.Enabled = false;
        }

        private void btnAddPoint_Click(object sender, EventArgs e)
        {
            addPoint();
        }

        private void addPoint()
        {
            if (nextPath == null) return;

            // stop polygon
            var cmdr = offset;
            nextPath.AddLine(lastPoint, cmdr);
            this.lastPoint = cmdr;

            refreshPlotter();
        }

        private void btnEndPolygon_Click(object sender, EventArgs e)
        {
            // end a building
            lastPoint = PointF.Empty;
            building.paths.Add(nextPath!);

            nextPath = null;
            refreshPlotter();

            btnStartPolygon.Enabled = true;
            btnAddPoint.Enabled = false;
            btnEndPolygon.Enabled = false;

            btnStartCircle.Enabled = true;
            numCircle.Enabled = false;
            btnEndCircle.Enabled = false;

            btnCommitBuilding.Enabled = true;
            txtBuildingName.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //// make last editable again
            //nextPath = building.paths.Last();
            //building.paths.Remove(nextPath);
            //lastPoint = nextPath.GetLastPoint();

            //nextPath.GetLastPoint();

            refreshPlotter();
        }

        private void btnCommitBuilding_Click(object sender, EventArgs e)
        {
            if (this.template!.buildings == null)
                this.template.buildings = new List<Building2>();

            // commit building to templates
            this.building.name = txtBuildingName.Text;
            this.template!.buildings.Add(this.building);

            //var json = JsonConvert.SerializeObject(building);

            this.building = new Building2();

            refreshPlotter();

            btnCommitBuilding.Enabled = false;
            txtBuildingName.Enabled = false;

            HumanSiteTemplate.export();
        }

        private void btnStartCircle_Click(object sender, EventArgs e)
        {
            // start circle
            nextPath = new GraphicsPath();
            lastPoint = offset;
            circle = true;

            refreshPlotter();

            btnStartPolygon.Enabled = false;
            btnAddPoint.Enabled = false;
            btnEndPolygon.Enabled = false;

            btnStartCircle.Enabled = true;
            numCircle.Enabled = true;
            btnEndCircle.Enabled = true;

            btnCommitBuilding.Enabled = false;
            txtBuildingName.Enabled = false;
        }

        private void btnEndCircle_Click(object sender, EventArgs e)
        {
            var rf = new RectangleF(offset, new SizeF(circleRadius * 2, circleRadius * 2));
            rf.Offset(-circleRadius, -circleRadius);
            nextPath!.AddEllipse(rf);

            building.paths.Add(nextPath);
            circle = false;

            nextPath = null;
            refreshPlotter();

            btnStartPolygon.Enabled = true;
            btnAddPoint.Enabled = false;
            btnEndPolygon.Enabled = false;

            btnStartCircle.Enabled = true;
            numCircle.Enabled = false;
            btnEndCircle.Enabled = false;

            btnCommitBuilding.Enabled = true;
            txtBuildingName.Enabled = true;
        }

        private void numCircle_ValueChanged(object sender, EventArgs e)
        {
            circleRadius = (float)numCircle.Value;

            refreshPlotter();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            HumanSiteTemplate.export();

            refreshPlotter();
        }

        private void addNamedPOI(string name)
        {
            var poi = new HumanSitePoi2()
            {
                name = name,
                offset = offset,
                floor = this.floor,
                level = this.level,
            };
            poi.offset.Y *= -1;

            if (this.template!.namedPoi == null) this.template.namedPoi = new List<HumanSitePoi2>();
            this.template.namedPoi.Add(poi);

            refreshPlotter();
            HumanSiteTemplate.export();
        }

        private void btnNamedPoi_Click(object sender, EventArgs e)
        {
            var ctrl = (Control)sender;
            addNamedPOI(ctrl.Text);
        }

        private void btnDataTerminal_Click(object sender, EventArgs e)
        {
            var poi = new HumanSitePoi2()
            {
                offset = offset,
                floor = this.floor,
                level = this.level,
            };
            poi.offset.Y *= -1;

            if (this.template!.dataTerminals == null) this.template.dataTerminals = new List<HumanSitePoi2>();
            this.template.dataTerminals.Add(poi);

            refreshPlotter();
            HumanSiteTemplate.export();
        }

        private void btnDoor(object sender, EventArgs e)
        {
            var aaa = game.status.Heading - station.heading;
            if (aaa < 0) aaa += 360;

            var poi = new HumanSitePoi2()
            {
                offset = offset,
                floor = this.floor,
                level = this.level,
                rot = (int)aaa
            };
            poi.offset.Y *= -1;

            if (this.template!.secureDoors == null) this.template.secureDoors = new List<HumanSitePoi2>();
            this.template.secureDoors.Add(poi);

            refreshPlotter();
            HumanSiteTemplate.export();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            addNamedPOI(comboPOI.Text);
        }

        private void levelZero_CheckedChanged(object sender, EventArgs e)
        {
            var ctrl = (CheckBox)sender;
            if (!ctrl.Checked) return;
            this.level = int.Parse(ctrl.Text);

            levelZero.Checked = this.level == 0;
            levelOne.Checked = this.level == 1;
            levelTwo.Checked = this.level == 2;
            levelThree.Checked = this.level == 3;

            refreshPlotter();
        }

        private void floorOne_CheckedChanged(object sender, EventArgs e)
        {
            var ctrl = (CheckBox)sender;
            if (!ctrl.Checked) return;
            this.floor = int.Parse(ctrl.Text);

            floorOne.Checked = this.floor == 1;
            floorTwo.Checked = this.floor == 2;
            floorThree.Checked = this.floor == 3;

            refreshPlotter();
        }
    }
}
