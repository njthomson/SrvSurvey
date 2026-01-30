using SrvSurvey.game;
using SrvSurvey.net;
using SrvSurvey.widgets;
using System.Drawing.Drawing2D;
using Res = Loc.PlotGalMap;

namespace SrvSurvey.plotters
{
    internal class PlotGalMap : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotGalMap),
            allowed = allowed,
            ctor = (game, def) => new PlotGalMap(game, def),
            defaultSize = new Size(240, 180),
        };

        public static bool allowed(Game game)
        {
            return Game.settings.autoShowPlotGalMap
                // NOT suppressed by buildProjectsSuppressOtherOverlays
                && (game.mode == GameMode.GalaxyMap || game?.musicTrack == "GalaxyMap")
                ;
        }

        #endregion

        private static bool smaller = true; // temp?
        private static GraphicsPath triangle;

        static PlotGalMap()
        {
            triangle = new GraphicsPath();
            triangle.AddPolygon(new Point[]
            {
                N.s(new Point(0, 0)),
                N.s(new Point(0, smaller ? 20 : 36)),
                N.s(new Point(smaller ? 8 : 12,  smaller ? 10 : 18)),
            });
        }

        private static float leftWidth = Util.maxWidth(GameColors.fontSmall2,
                Res.Selected,
                Res.Current,
                Res.Destination,
                Res.NextJump) + N.oneTwo;

        private double distanceJumped;
        /// <summary> Next hop data </summary>
        private NetSysData? nextNetData;
        /// <summary> Destination system data </summary>
        private NetSysData? finalNetData;

        private PlotGalMap(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontSmall2;

            // show existing route, otherwise data for current system
            if (game.navRoute.Route.Count > 1)
                this.onJournalEntry(new NavRoute());
            else if (game.systemData != null)
                this.finalNetData = NetSysData.get(game.systemData.name, game.systemData.address, (source, netData) => this.invalidate());
        }

        protected override void onJournalEntry(FSDTarget entry)
        {
            // exit early when the target is the first hop (this happens when setting a route)
            if (this.nextNetData?.systemAddress == entry.SystemAddress)
                return;

            this.distanceJumped = 0;

            this.finalNetData = NetSysData.get(entry.Name, entry.SystemAddress, (source, netData) => this.invalidate());
            this.nextNetData = null;

            this.invalidate();
        }

        protected override void onJournalEntry(NavRoute entry)
        {
            // lookup if target system has been discovered
            if (game.navRoute.Route.Count < 2)
                return;

            // load data for destination system
            var lastHop = game.navRoute.Route.Last();
            this.finalNetData = NetSysData.get(lastHop.StarSystem, lastHop.SystemAddress, (source, netData) => this.invalidate());

            // load data for first jump
            var firstHop = game.navRoute.Route.Skip(1).FirstOrDefault();
            if (firstHop == lastHop || firstHop == null)
                this.nextNetData = null;
            else
                this.nextNetData = NetSysData.get(firstHop.StarSystem, firstHop.SystemAddress, (source, netData) => this.invalidate());

            this.distanceJumped = 0;
            for (int n = 1; n < game.navRoute.Route.Count; n++)
            {
                var d = Util.getSystemDistance(game.navRoute.Route[n - 1].StarPos, game.navRoute.Route[n].StarPos);
                this.distanceJumped += d;
            }
        }

        protected override void onJournalEntry(NavRouteClear entry)
        {
            this.nextNetData = null;
            this.finalNetData = null;

            this.invalidate();
        }

        protected override SizeF doRender(Graphics g, TextCursor tt)
        {
            if (this.finalNetData == null)
            {
                // TODO: keep hidden until we have a route?
                tt.draw(N.eight, Res.NoRouteSet);
                tt.newLine(+N.ten, true);
            }
            else
            {
                if (this.finalNetData != null)
                    this.drawSystemSummary(g, tt, this.finalNetData);

                if (this.nextNetData != null)
                    this.drawSystemSummary(g, tt, this.nextNetData);

                if (this.distanceJumped > 0)
                {
                    var txt = Res.RouteFooter.format(
                        game.navRoute.Route.Count - 1,
                        this.distanceJumped.ToString("N1"));
                    tt.draw(N.eight, txt, C.orange);
                    tt.newLine(true);
                }

                tt.dty += N.two;
                tt.draw(N.eight, Res.DataFrom, C.orangeDark);
                tt.newLine(true);
            }

            var maxY = tt.dty;

            if (Game.settings.galMapFactions && finalNetData?.spanshDump?.factions.Count > 0)
                drawFactionSummary(g, tt, finalNetData);

            return tt.pad(+N.ten, +N.ten);
        }

        private void drawSystemSummary(Graphics g, TextCursor tt, NetSysData netData)
        {
            var header = Res.Selected;

            if (netData.systemAddress == game.systemData?.address)
                header = Res.Current;
            else if (netData.systemAddress == game.navRoute.Route.LastOrDefault()?.SystemAddress)
                header = Res.Destination;
            else if (netData.systemAddress == nextNetData?.systemAddress)
                header = Res.NextJump;

            tt.draw(N.eight, header);

            // line 1: system name
            tt.draw(leftWidth, $"► {netData.systemName}", GameColors.fontSmall2Bold);
            // quest related?
            if (game.cmdrPlay?.isTagged(netData.systemName) == true)
                PlotQuestMini.drawLogo(g, tt.dtx + N.six, N.eight, true, N.oneSix);

            tt.newLine(true);

            var highlight = netData.discovered == false || netData.scanBodyCount < netData.totalBodyCount || (netData.totalBodyCount == 0 && netData.discovered.HasValue);

            // line 2: status
            var discoveryStatus = netData.discoveryStatus ?? "...";
            if (highlight)
                tt.draw(leftWidth, $"{discoveryStatus}", C.cyan, GameColors.fontSmall2Bold);
            else
                tt.draw(leftWidth, $"{discoveryStatus}");

            // line 3: who discovered + last updated
            if (netData.discoveredBy != null && netData.discoveredDate != null)
            {
                tt.newLine(true);
                tt.draw(leftWidth, Res.DiscoveredBy.format(netData.discoveredBy, netData.discoveredDate?.ToCmdrShortDateTime24Hours(true) ?? "?"));
            }
            if (netData.lastUpdated != null && (netData.lastUpdated > netData.discoveredDate || netData.discoveredDate == null))
            {
                tt.newLine(true);
                tt.draw(leftWidth, Res.LastUpdated.format(netData.lastUpdated?.ToCmdrShortDateTime24Hours(true) ?? "?"));
            }

            // line 4: bio signals?
            if (netData.genusCount > 0)
            {
                tt.newLine(true);
                tt.draw(leftWidth, Res.CountGenus.format(netData.genusCount), C.cyan, GameColors.fontSmall2Bold);
            }

            tt.newLine(+N.ten, true);
        }

        private void drawFactionSummary(Graphics g, TextCursor tt, NetSysData netData)
        {
            var x = tt.frameSize.Width + N.ten;
            tt.dty = N.eight;

            g.DrawLineR(C.Pens.orangeDarker1, x, N.oneSix, 0, tt.containerHeight - 32);
            x += N.six;

            var sorted = netData.spanshDump.factions
                .OrderByDescending(f => f.influence)
                .ToDictionary(f => f.name, f => f.influence);

            if (sorted.Count < 6)
            {
                tt.draw(x, "Factions:", C.orangeDark);
                tt.newLine(true);
            }

            x += N.six;
            var x2 = x + N.ten + Util.maxWidth(tt.font, sorted.Keys);

            foreach (var name in sorted.Keys)
            {
                tt.draw(x, name);

                tt.draw(x2, sorted[name].ToString("p0"));
                tt.newLine(true);
            }
        }
    }
}
