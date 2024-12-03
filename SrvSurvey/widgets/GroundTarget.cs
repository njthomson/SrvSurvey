using DecimalMath;
using SrvSurvey.game;
using SrvSurvey.units;
using System.Drawing.Drawing2D;

namespace SrvSurvey.widgets
{
    internal class GroundTarget : BaseWidget
    {
        #region stationary

        /// <summary> width of pen for angle-of-attack </summary>
        const float pw = 2.2f;
        private PenBrush shallow = new PenBrush(C.orange, pw, LineCap.Flat);
        private PenBrush ideal = new PenBrush(C.cyan, pw, LineCap.Flat);
        private PenBrush steep = new PenBrush(C.red, pw, LineCap.Flat);
        private PenBrush tooSteep = new PenBrush(C.redDark, pw, LineCap.Flat);

        const float bw = 4f;
        private PenBrush ahead = new PenBrush(C.cyan, bw, LineCap.Flat);
        private PenBrush aside = new PenBrush(C.orange, bw, LineCap.Flat);
        private PenBrush behind = new PenBrush(C.red, bw, LineCap.Flat);

        #endregion

        /// <summary> Minimal angle needed to render angle-of-attack widget </summary>
        public int angleMin = 5;
        /// <summary> Minimal IDEAL angle needed for ideal angle-of-attack </summary>
        public int angleIdeal = 30;
        /// <summary> Minimal STEEP angle needed for ideal angle-of-attack </summary>
        public int angleSteep = 50;
        /// <summary> Minimal TOO STEEP angle needed for ideal angle-of-attack </summary>
        public int angleTooSteep = 60;


        public Font font;

        public GroundTarget(Font font)
        {
            this.font = font;
        }

        public void renderAngleOfAttack(Graphics g, float x, float y, decimal radius, LatLong2 target, LatLong2 cmdr)
        {
            if (game?.status == null) return;

            // calculate as if a 2d plane
            var angle2d = Util.getBearing(cmdr, target);
            var dist2d = Util.getDistance(cmdr, target, radius);
            // calculate angle of decline
            var angle3d = DecimalEx.ToDeg(DecimalEx.ATan(game.status.Altitude / dist2d));

            var deg = angle2d - game.status.Heading;
            if (deg < 0) deg += 360;
            //Game.log($"angle2d: {angle2d}, deg: {deg} / game.status.Heading:{game.status.Heading}");

            if (angle3d > 5) // angleMin)
            {
                // adjust stationary based on angle
                PenBrush pb;
                if (angle3d > angleTooSteep)
                    pb = tooSteep; // red dark
                else if (angle3d > angleSteep)
                    pb = steep; // red
                else if (angle3d > angleIdeal)
                    pb = ideal; // cyan
                else
                    pb = shallow; // orange

                // draw angle of attack - pie
                var attackAngle = deg > 90 && deg < 270 ? 180 - angle3d : angle3d;
                GraphicsPath path = new GraphicsPath();
                path.AddPie(x, y - 18, 36, 36, 180, -(int)attackAngle);
                g.FillPath(pb.brush, path);
                g.DrawPath(pb.pen, path);

                // draw angle of attack - text
                x += forty;
                renderText(g, $"-{(int)angle3d}°", x, y, this.font, pb.pen.Color);
            }
            else
            {
                x += six;
            }

            // calculate angle relative to current heading
            var angleRel = deg; // angle2d - game.status.Heading;
            var bb = behind; // red
            if (angleRel > 320 || angleRel < 20)
                bb = ahead; // cyan
            else if (angleRel > 300 || angleRel < 60)
                bb = aside; // orange

            // draw bearing to target
            x += threeEight;
            renderBearingTo(g, x, y, ten, (double)deg, this.font, bb.brush, bb.pen);
        }
    }
}

