using SrvSurvey.game;
using SrvSurvey.widgets;
using Res = Loc.PlotRamTah;

namespace SrvSurvey.plotters
{
    internal class PlotRamTah : PlotBase2
    {
        #region def + statics

        public static PlotDef def = new PlotDef()
        {
            name = nameof(PlotRamTah),
            allowed = allowed,
            ctor = (game, def) => new PlotRamTah(game, def),
            defaultSize = new Size(200, 280),
        };

        public static bool allowed(Game game)
        {
            // TODO: show this earlier, like on approach?
            return Game.settings.autoShowRamTah
                && Game.settings.enableGuardianSites
                && !Game.settings.buildProjectsSuppressOtherOverlays
                && game?.systemBody != null
                && game.cmdr.ramTahActive
                && game.status?.hasLatLong == true
                && game.systemSite?.location != null
                // require isRuins to match decodeTheRuinsMissionActive or !isRuins to match decodeTheLogsMissionActive
                && (game.systemSite.isRuins == (game.cmdr.decodeTheRuinsMissionActive == TahMissionStatus.Active) || game.systemSite.isRuins != (game.cmdr.decodeTheLogsMissionActive == TahMissionStatus.Active))
                && game.isMode(GameMode.InSrv, GameMode.OnFoot, GameMode.Landed, GameMode.Flying, GameMode.InFighter, GameMode.CommsPanel, GameMode.InternalPanel)
                ;
        }

        #endregion

        private PlotRamTah(Game game, PlotDef def) : base(game, def)
        {
            this.font = GameColors.fontMiddle;
        }

        protected override SizeF doRender(Game game, Graphics g, TextCursor tt)
        {
            if (game?.systemSite == null) return frame.Size;

            tt.dtx = N.six;
            tt.dty = N.eight;
            var sz = new SizeF(N.six, N.six);

            var ramTahObelisks = game.systemSite.ramTahObelisks;
            tt.draw(Res.Header.format(ramTahObelisks?.Count ?? 0), GameColors.fontSmall);
            tt.newLine(N.ten, true);

            if (ramTahObelisks?.Count > 0)
            {
                var targetObelisk = PlotBase2.getPlotter<PlotGuardians>()?.targetObelisk;
                foreach (var bar in ramTahObelisks)
                {
                    var obelisk = game.systemSite.getActiveObelisk(bar.Value.First());
                    if (obelisk == null || string.IsNullOrEmpty(obelisk.name) || string.IsNullOrEmpty(bar.Key)) continue;

                    // first, do we have the items needed?
                    var item1 = obelisk.items.First().ToString();
                    var hasItem1 = game.getInventoryItem(item1)?.Count >= 1;

                    var item2 = obelisk.items.Count > 1 ? obelisk.items.Last().ToString() : null;
                    var hasItem2 = item2 == null ? true : game.getInventoryItem(item2)?.Count >= (item1 == item2 ? 2 : 1);

                    var isTargetObelisk = targetObelisk != null && bar.Value.Contains(targetObelisk);
                    var isCurrentObelisk = bar.Value.Any(_ => _ == game.systemSite.currentObelisk?.name);
                    var col = C.orange;
                    if (isTargetObelisk && !isCurrentObelisk && game.systemSite.currentObelisk != null)
                        col = C.cyanDark;
                    else if (isCurrentObelisk || isTargetObelisk)
                        col = C.cyan;
                    // change colours if items are missing? Perhaps overkill?
                    //else if (!hasItem1 || !hasItem2)
                    //    brush = GameColors.brushRed;

                    // draw main text (bigger font)
                    var logName = $"{Util.getLogNameFromChar(bar.Key[0])} #{bar.Key.Substring(1)}".Trim();
                    tt.draw(N.oneFour, logName, col, GameColors.fontMiddle);
                    // strike-through the log name if we do not have sufficient items
                    if (!hasItem1 || !hasItem2)
                        tt.strikeThroughLast(isCurrentObelisk || isTargetObelisk);

                    tt.draw(": ", col, GameColors.fontMiddle);
                    tt.dty += N.six;
                    tt.dtx += drawRamTahDot(g, tt.dtx, tt.dty, item1);
                    tt.draw(Util.getLoc(item1), hasItem1 ? C.orange: C.red, GameColors.fontSmall);

                    if (item2 != null)
                    {
                        tt.dtx += N.two;
                        tt.draw("+", col, GameColors.fontSmall);
                        tt.dtx += N.four;
                        tt.dtx += drawRamTahDot(g, tt.dtx, tt.dty, item2);
                        tt.draw(Util.getLoc(item2), hasItem2 ? C.orange : C.red, GameColors.fontSmall);
                    }

                    tt.newLine(N.eight, true);

                    // draw each obelisk name, highlighting the target one
                    foreach (var ob in bar.Value)
                    {
                        if (targetObelisk == ob || game.systemSite.currentObelisk?.name == ob)
                            tt.draw(N.twoFour, ob, col, GameColors.fontSmallBold);
                        else
                            tt.draw(N.twoFour, ob, col, GameColors.fontSmall);
                    }

                    tt.newLine(N.oneFour, true);
                }

                tt.dtx = N.eight;
                tt.dty += N.ten;
                tt.dty += tt.draw(Res.Footer, GameColors.fontSmall).Height;
                if (tt.dtx > sz.Width) sz.Width = tt.dtx;
            }
            else
            {
                tt.dtx = N.twoFour;
                tt.dty += tt.draw(Res.NoNewLogs, C.orange, GameColors.fontMiddle).Height;
                if (tt.dtx > sz.Width) sz.Width = tt.dtx;
            }

            return tt.pad(N.ten, N.ten);
        }

        public static float drawRamTahDot(Graphics g, float x, float y, string item)
        {
            if (item == POIType.relic.ToString())
            {
                x +=  N.ten;
                y +=  N.two;

                g.TranslateTransform(x, y);
                g.FillPolygon(GameColors.brushPoiPresent, ramTahRelicPoints);
                g.DrawPolygon(GameColors.penPoiRelicPresent, ramTahRelicPoints);

                g.TranslateTransform(-x, -y);
                return N.twoFour;
            }
            else if (itemPoiTypeMap.ContainsKey(item))
            {
                var r = new RectangleF(x, y - N.one, N.ten, N.ten);
                var poiType = itemPoiTypeMap[item];

                g.FillEllipse(GameColors.Map.brushes[poiType][SitePoiStatus.present], r);
                g.DrawEllipse(GameColors.Map.pens[poiType][SitePoiStatus.present], r);
                return N.oneSix;
            }

            // TODO: Thargoid items?
            return 0;
        }

        private static PointF[] ramTahRelicPoints = {
            new PointF(-8, -4),
            new PointF(+8, -4),
            new PointF(0, +10),
            new PointF(-8, -4),
        };

        private static Dictionary<string, POIType> itemPoiTypeMap = new Dictionary<string, POIType>()
        {
            { ObeliskItem.casket.ToString(), POIType.casket },
            { ObeliskItem.orb.ToString(), POIType.orb },
            { ObeliskItem.relic.ToString(), POIType.relic },
            { ObeliskItem.tablet.ToString(), POIType.tablet},
            { ObeliskItem.totem.ToString(), POIType.totem },
            { ObeliskItem.urn.ToString(), POIType.urn },
        };
    }
}

