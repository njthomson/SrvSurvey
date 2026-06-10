using SrvSurvey.game;
using SrvSurvey.plotters;
using SrvSurvey.widgets;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace SrvSurvey.forms.playComms;

// TODO: This class desperately needs a better name

[System.ComponentModel.DesignerCategory("")]
internal abstract class BaseFormZippy : SizableForm, PlotterForm
{
    /// <summary> The set of all visible controls on this form. For perf reasons We manage these manually instead of using actual WinForms controls.handling). </summary>
    public List<Ctrl> ctrls = [];
    /// <summary> Ctrls that participate in scrolling </summary>
    protected List<Ctrl> stack = [];

    /// <summary> The definition of the scrollling zone </summary>
    protected Rectangle scrollZone;
    /// <summary> The concrete adjusted rectangle scrolling occurs within </summary>
    public RectangleF scrollBox;
    public float scrollUp;
    public float scrollMax;
    private bool canScroll => stack.Count > 0;

    /// <summary> The ctrl that is "current" or active / has focus </summary>
    protected Ctrl? ctrlCurrent = null;
    /// <summary> The last ctrl that was "current" but isn't any more </summary>
    private Ctrl? ctrlLast = null;

    protected bool mouseDown = false;
    protected bool mouseOverCtrl = false;

    public BaseFormZippy()
    {
        ctrls.Clear();

        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

        this.Name = this.GetType().Name;
        this.ShowIcon = false;
        this.StartPosition = FormStartPosition.Manual;
        this.MinimizeBox = false;
        this.MaximizeBox = false;
        this.ControlBox = false;
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
        this.BackColor = C.black;
        this.ForeColor = C.orange;
        this.Opacity = 0;

        if (!Debugger.IsAttached)
        {
            // for the sake of debugging sanity ...
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowInTaskbar = true;
        }
        else
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
        }

        // prep for DirectX inputs
        if (KeyboardHook.mappedGameKeyBinds.Count == 0)
            KeyboardHook.parseGameKeybinds();

        this.Activated += (o, s) => KeyboardHook.redirect = true;
        this.Deactivate += (o, s) => KeyboardHook.redirect = false;
        KeyboardHook.buttonsPressed += KeyboardHook_buttonsPressed;
    }

    protected override void Dispose(bool disposing)
    {
        KeyboardHook.redirect = false;
        KeyboardHook.buttonsPressed -= KeyboardHook_buttonsPressed;

        base.Dispose(disposing);
    }

    private void KeyboardHook_buttonsPressed(bool hook, string chord, int analog)
    {
        //Debug.WriteLine($"Hook: {hook}, Chord: {chord}");

        // only process when buttons are pushed down and we have focus
        if (hook || !Elite.focusSrvSurvey) return;

        // mimic a keypress if we have a mapping
        if (KeyboardHook.mappedGameKeyBinds.ContainsKey(chord))
            SendKeys.SendWait(KeyboardHook.mappedGameKeyBinds[chord]);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ctrlCurrent = ctrls.FirstOrDefault(sib => !sib.disabled);
        if (ctrlCurrent != null) ctrlLast = ctrlCurrent;

        // delay fading in to avoid initial renders that are always Window coloured
        Util.deferAfter(100, () => Util.fadeOpacity(this, 0.95f, Game.settings.fadeInDuration));
    }

    protected List<Ctrl> addCtrl(params IEnumerable<Ctrl> newCtrls)
    {
        foreach (var ctrl in newCtrls)
        {
            ctrl.form = this;
            ctrls.Add(ctrl);
        }
        this.Invalidate();

        return newCtrls.ToList();
    }

    protected List<Ctrl> addStack(params IEnumerable<Ctrl> newCtrls)
    {
        foreach (var ctrl in newCtrls)
        {
            ctrl.form = this;
            stack.Add(ctrl);
        }
        this.Invalidate();

        return newCtrls.ToList();
    }

    #region for PlotterForm

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool didFirstPaint { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool showing { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool forceHide { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool fading { get; set; }

    public void reposition(Rectangle gameRect) { /* no op */ }

    public void setOpacity(double newOpacity)
    {
        this.Opacity = newOpacity;
    }

    public void resetOpacity()
    {
        this.Opacity = 0.9f;
    }

    #endregion

    #region sibling navigation + key press handling

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var last = ctrlCurrent ?? ctrlLast;
        //Debug.WriteLine($"ProcessCmdKey: {keyData}");
        if (keyData == Keys.Enter)
        {
            // "click" the current ctrl
            last?.doClick(false);
            return true;
        }
        else if (keyData == Keys.Escape)
        {
            var rslt = onBack();
            this.Invalidate();
            return rslt;
        }

        // find a sibling ctrl
        var allCtrls = ctrls.Concat(stack).ToList();
        Ctrl? next = null;
        if (keyData == Keys.Left)
            next = findSibling(allCtrls, last, (f, s) => s.r.Left < f?.r.Left, Side.Left, Side.Left);
        else if (keyData == Keys.Right)
            next = findSibling(allCtrls, last, (f, s) => s.r.Left > f?.r.Left, Side.Right, Side.Left);
        else if (keyData == Keys.Up)
            next = findSibling(allCtrls, last, (f, s) => s.r.Top < f?.r.Top, Side.Top, Side.Bottom);
        else if (keyData == Keys.Down)
            next = findSibling(allCtrls, last, (f, s) => s.r.Top > f?.r.Top, Side.Bottom, Side.Top);
        else if (keyData == Keys.Tab)
        {
            if (ctrlCurrent == null)
                next = allCtrls.FirstOrDefault();
            else
            {
                var idx = allCtrls.IndexOf(ctrlCurrent);
                if (idx == allCtrls.Count - 1)
                    next = allCtrls.FirstOrDefault();
                else
                    next = allCtrls[idx + 1];
            }
        }

        // only change if we found something
        if (next != null)
        {
            //Debug.WriteLine($"{keyData} => was: {last}, next: {next}");
            this.ctrlCurrent = next;
            this.ctrlLast = next;
            this.Invalidate();
        }
        return true;
    }

    protected virtual bool onBack()
    {
        return true;
    }

    private static Ctrl? findSibling(List<Ctrl> list, Ctrl? from, Func<Ctrl, Ctrl, bool> match, Side fs, Side ss)
    {
        if (from == null) return list.FirstOrDefault(sib => !sib.disabled);

        // find next ctrl to the left
        var sibs = list.Where(sib => sib != from && !sib.hidden && !sib.disabled && !sib.noFocus && match(from, sib)).ToList();

        if (sibs.Count == 0) return null; // TODO: add another func to select by wrapping around
        if (sibs.Count == 1) return sibs[0];

        // choose best choice from possibles
        var next = sibs.MinBy(sib =>
        {
            var szF = getSide(from, fs);
            var szS = getSide(sib, ss);
            var szD = szF - szS;
            var dist = Math.Sqrt((szD.Width * szD.Width) + (szD.Height * szD.Height));
            return dist;
        });

        return next;
    }

    private static SizeF getSide(Ctrl ctrl, Side side)
    {
        switch (side)
        {
            case Side.Top: return new(ctrl.r.Left + (ctrl.r.Width / 2), ctrl.r.Top);
            case Side.Left: return new(ctrl.r.Left, ctrl.r.Top + (ctrl.r.Height / 2));
            case Side.Bottom: return new(ctrl.r.Left + (ctrl.r.Width / 2), ctrl.r.Bottom);
            case Side.Right: return new(ctrl.r.Right, ctrl.r.Top + (ctrl.r.Height / 2));
            default: throw new Exception($"Unexpected side: {side}");
        }
    }

    #endregion

    #region ctrl rendering and mouse states

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // tell any ctrls
        var allCtrls = ctrls.Concat(stack).ToList();
        var doInvalidate = false;
        var newMouseOverCtrl = false;
        foreach (var ctrl in allCtrls)
        {
            var x = e.Location.X;
            var y = e.Location.Y;
            if (canScroll && scrollBox.Contains(e.Location))
                y += (int)scrollUp;

            var match = ctrl.r.Contains(x, y);
            doInvalidate |= ctrl.setHovered(match);
            if (match)
            {
                newMouseOverCtrl = true;
                ctrlCurrent = ctrl;
                ctrlLast = ctrl;
                this.skipDrag = true;
                doInvalidate = true;
            }
        }

        // only clear this if mouse has just left a ctrl
        if (!newMouseOverCtrl && newMouseOverCtrl != mouseOverCtrl && !mouseDown)
        {
            ctrlCurrent = null;
            this.skipDrag = false;
        }

        mouseOverCtrl = newMouseOverCtrl;

        if (doInvalidate)
            this.Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        mouseDown = true;

        if (mouseOverCtrl && ctrlCurrent?.onClick != null && !ctrlCurrent.disabled)
        {
            ctrlCurrent.doClick(true);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        mouseDown = false;
        this.Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (canScroll)
        {
            scrollUp -= e.Delta * 0.5f;
            if (scrollUp < 0) scrollUp = 0;
            if (scrollUp > scrollMax) scrollUp = scrollMax;
            this.Invalidate();
        }
    }

    public void setScroll(int x, int y, int w, int h)
    {
        scrollZone = new Rectangle(x, y, w, h);
        scrollUp = 0;
        scrollBox = Util.applyOffset(scrollZone, ClientSize);
    }

    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        var tt = new TextCursor(g, this);
        tt.flags |= TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform;

        var redraw = drawCommon(g, tt);

        redraw |= render(g, tt);

        // something changed size, or otherwise signaled we should trigger an immediate redraw
        if (redraw)
            Program.defer(() => this.Invalidate());

        // tmp?
        g.DrawRectangle(C.Pens.orangeDark1, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }

    protected virtual bool render(Graphics g, TextCursor tt)
    {
        return false;
    }

    bool renderCtrl(Graphics g, TextCursor tt, List<Ctrl> ctrls, int n)
    {
        var ctrl = ctrls[n];
        if (ctrl.hidden) return false;

        // negative values mean re-position in from the right or bottom edge of the window
        var frameSz = this.ClientSize;
        ctrl.r.X = Util.applyOffset(ctrl.offset.X, ctrl.r.Width, frameSz.Width);
        ctrl.r.Y = Util.applyOffset(ctrl.offset.Y, ctrl.r.Height, frameSz.Height);

        // relocate stack items to the scrollBox
        if (ctrls == stack)
        {
            ctrl.r.X = ctrl.offset.X + scrollBox.X;
            ctrl.r.Y = ctrl.offset.Y + scrollBox.Y;
        }

        // get prior (non-hidden) ctrl
        Ctrl? prior = null;
        for (var nn = n - 1; nn >= 0 && prior?.hidden != false; nn--)
            prior = ctrls[nn];

        // following any side?
        if (prior != null && ctrl.follow != default)
        {
            followPrior(ctrl, prior);
            /* TODO: handle wrapping overflow on tags
            if (!ClientRectangle.Contains(ctrl.r.toRectangle()))
            {
                var idx = ctrls.FindLastIndex(ctrls.IndexOf(ctrl), (x => x.follow != ctrl.follow))+1;
                var o2 = ctrls[idx];
                Game.log($"! {ctrl} => {o2}");
            }*/
        }

        tt.dtx = ctrl.r.X;
        tt.dty = ctrl.r.Y;
        var isCurrent = ctrl == ctrlCurrent;

        return ctrl.render(g, tt, isCurrent, ctrl.isClicking, prior);
    }

    private void followPrior(Ctrl ctrl, Ctrl prior)
    {
        if (ctrl.follow == Side.Top)
        {
            ctrl.r.X = prior.r.Left;
            ctrl.r.Y = prior.r.Bottom + ctrl.gap;
        }
        if (ctrl.follow == Side.Bottom)
        {
            ctrl.r.X = prior.r.Left;
            ctrl.r.Y = prior.r.Top - ctrl.gap - ctrl.r.Height;
        }
        else if (ctrl.follow == Side.Left)
        {
            ctrl.r.X = prior.r.Right + ctrl.gap;
            ctrl.r.Y = prior.r.Y;
        }
        else if (ctrl.follow == Side.Right)
        {
            ctrl.r.X = prior.r.Left - ctrl.gap - ctrl.r.Width;
            ctrl.r.Y = prior.r.Y;
        }
    }

    bool drawCommon(Graphics g, TextCursor tt)
    {
        var redraw = false;
        // render fixed controls
        for (var n = 0; n < ctrls.Count; n++)
            redraw |= renderCtrl(g, tt, ctrls, n);

        // render scrollable controls?
        if (canScroll)
        {
            scrollBox = Util.applyOffset(scrollZone, ClientSize);
            //g.DrawRectangle(Pens.Lime, scrollBox); // TMP!!

            g.Clip = new Region(scrollBox);
            g.TranslateTransform(0, -scrollUp);

            for (var n = 0; n < stack.Count; n++)
                redraw |= renderCtrl(g, tt, stack, n);

            // and draw scrollbar, if needed
            g.ResetTransform();
            g.ResetClip();

            var lastBottom = stack.LastOrDefault()?.r.Bottom;
            if (lastBottom > scrollBox.Bottom)
            {
                scrollMax = lastBottom - scrollBox.Height - scrollBox.Top ?? 0;
                if (scrollUp > scrollMax) scrollUp = scrollMax;

                var ratio = scrollBox.Height / (lastBottom - scrollBox.Top);
                var x = ClientSize.Width - 10;
                var y = scrollUp * ratio;
                var h = scrollBox.Height * ratio;
                g.FillRectangle(C.Brushes.orangeDark, x, scrollBox.Top + (int)y, 6, (int)h);
            }
        }
        return redraw;
    }

    public float scrollWidth => scrollBox.Width;
    public float scrollHeight => scrollBox.Height;
}

public enum Side
{
    Unknown,
    Top,
    Left,
    Bottom,
    Right
}

struct ColorSet
{
    public Color normal;
    public Color current;
    public Color clicking;
    public Color disabled;

    public Color get(bool isCurrent, bool isClicking, bool isDisabled)
    {
        if (isDisabled)
            return disabled;
        else if (isClicking)
            return clicking;
        else if (isCurrent)
            return current;
        else
            return normal;
    }

    public static ColorSet csFore = new()
    {
        normal = C.orange,
        current = C.black,
        clicking = C.black,
        disabled = C.black,
    };

    public static ColorSet csForeIcon = new()
    {
        normal = C.orangeDark,
        current = C.black,
        clicking = C.black,
        disabled = C.black,
    };

    public static ColorSet csBack = new()
    {
        normal = C.orangeDarker,
        current = C.orange,
        clicking = C.white,
        disabled = C.grey,
    };

    public static ColorSet csCyanBack = new()
    {
        normal = C.cyanDarker,
        current = C.cyan,
        clicking = C.white,
        disabled = C.grey,
    };

    public static ColorSet csCyanFore = new()
    {
        normal = C.cyan,
        current = C.black,
        clicking = C.black,
        disabled = C.black,
    };

}

/// <summary>
/// A proxy for control, but windowless.
/// </summary>
class Ctrl
{
    public BaseFormZippy form;
    public PointF offset;
    public RectangleF r;
    public bool disabled;
    public bool hidden;
    public bool noFocus;
    protected bool hovered;
    public bool isCurrent;
    /// <summary> 0: not => 1: starting => 2: going => 3: ending => 0: not</summary>
    protected int clicking;
    public bool isClicking => this.clicking > 0;

    public Side follow;
    public int gap = 8;

    public Action<Ctrl, bool> onClick;
    public Func<Ctrl, Graphics, TextCursor, bool, bool, Ctrl?, bool>? onRender;

    public SizeF sz { get => r.Size; set => r.Size = value; }

    public void doClick(bool byMouse)
    {
        // animate ourself clicked and then do the click action
        if (this.onClick != null && !this.disabled && !this.noFocus)
            this.animateClicking(byMouse);
    }

    private void animateClicking(bool byMouse)
    {
        if (clicking == 0)
        {
            // starting: briefly be GOLD
            this.clicking = 1;
            this.form.Invalidate();
            Task.Delay(50).continueOnMain(this.form, () => animateClicking(byMouse));
        }
        else if (clicking == 1)
        {
            // going: be WHITE
            this.clicking = 2;
            this.form.Invalidate();
            Task.Delay(100).continueOnMain(this.form, () => animateClicking(byMouse));
        }
        else if (clicking == 2)
        {
            // ending: now do the action, and briefly be GOLD
            this.onClick(this, byMouse);

            this.clicking = 3;
            this.form.Invalidate();
            Task.Delay(50).continueOnMain(this.form, () => animateClicking(byMouse));
        }
        else if (clicking == 3)
        {
            // ended
            this.clicking = 0;
            this.form.Invalidate();
        }
    }

    public virtual bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        if (onRender != null)
            return onRender(this, g, tt, isCurrent, isClicking, prior);

        return false;
    }

    public virtual bool setHovered(bool hovered)
    {
        var changed = this.hovered != hovered;
        this.hovered = hovered;

        return changed;
    }

    protected bool adjustHeight(float newHeight)
    {
        if (newHeight == r.Height) return false;

        r.Height = newHeight;
        return true;
    }
}

internal class HorizLine : Ctrl
{
    public HorizLine() : base()
    {
        noFocus = true;
    }

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        r.X = form.scrollBox.Left;
        r.Width = form.scrollWidth;
        r.Height = gap;

        g.DrawLineR(C.Pens.orangeDark2r, r.X, tt.dty, r.Width, 0);

        return false;
    }
}

class TextCtrl : Ctrl
{
    public Color color;
    public float pad = N.four;
    public string text;
    public bool autoSize;
    public Color backColor = Color.Transparent;
    private SolidBrush? backBrush;
    public Font? font;

    public TextCtrl() : base()
    {
        noFocus = true;
    }

    override public string ToString() => this.text;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        // set our size based on text + padding
        tt.dtx += pad;
        tt.dty += pad;

        if (autoSize)
        {
            var sz = TextRenderer.MeasureText(g, this.text, this.font ?? tt.font);
            r.Width = sz.Width + pad + pad;
            r.Height = sz.Height + pad + pad;
        }

        if (backBrush == null || backBrush.Color != backColor)
        {
            backBrush?.Dispose();
            backBrush = null;
            if (backColor != Color.Transparent)
                backBrush = backColor.toBrush();
        }

        if (backBrush != null)
            g.FillRectangle(backBrush, r);

        // finally, draw the text
        tt.drawCentered(this.r.toRectangle(), this.text, this.color, this.font);

        return false;
    }
}

class BtnFillCtrl : Ctrl
{
    public ColorSet csBack = ColorSet.csBack;
    protected SolidBrush? backBrush;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        // choose colours based on state
        var backColor = csBack.get(isCurrent, isClicking, disabled);
        if (clicking == 1 || clicking == 3) backColor = C.menuGold; // gold might be wierd in cases ... maybe add this into ColorSet too?

        if (backBrush == null || backBrush.Color != backColor)
        {
            backBrush?.Dispose();
            backBrush = null;
            if (backColor != Color.Transparent)
                backBrush = backColor.toBrush();
        }

        // draw background
        if (backBrush != null)
            g.FillRectangle(backBrush, r);

        if (onRender != null)
            return onRender(this, g, tt, isCurrent, isClicking, prior);

        return false;
    }
}

class BtnFillTextCtrl : BtnFillCtrl
{
    public ColorSet csFore = ColorSet.csFore;
    public float pad = 8;
    public string text;
    public bool autoSize;
    public Font? font;

    public bool bottomBar;
    private Pen? bottomPen;

    override public string ToString() => this.text;

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        // set our size based on text + padding
        tt.dtx += pad;
        tt.dty += pad;

        if (autoSize)
        {
            var sz = TextRenderer.MeasureText(g, this.text, this.font ?? tt.font);
            r.Width = sz.Width + pad + pad;
            r.Height = sz.Height + pad + pad;
        }

        var redraw = base.render(g, tt, isCurrent, isClicking, prior);
        var color = csFore.get(isCurrent, isClicking, disabled);

        if (bottomBar)
        {
            var bottomColor = isCurrent ? C.orangeDark : C.orange;
            if (bottomPen == null || bottomPen.Color != bottomColor)
            {
                bottomPen?.Dispose();
                bottomPen = bottomColor.toPen(4);
            }

            if (bottomPen != null)
                g.DrawLineR(bottomPen, r.X, r.Bottom, r.Width, 0);
        }

        // finally, draw the text
        tt.drawCentered(this.r.toRectangle(), this.text, color, this.font);

        return redraw;
    }
}

class BtnFillDrawCtrl : BtnFillCtrl
{
    public bool sideBar;
    public string iconName;
    public int iconSize;
    public PointF iconOffset;
    protected Color iconColor;
    protected Pen? iconPen;
    private Pen? sidePen;

    override public string ToString() => this.iconName ?? "?";

    public override bool render(Graphics g, TextCursor tt, bool isCurrent, bool isClicking, Ctrl? prior)
    {
        var redraw = base.render(g, tt, isCurrent, isClicking, prior);

        iconColor = ColorSet.csForeIcon.get(isCurrent, isClicking, disabled);
        if (iconPen == null || iconPen.Color != iconColor)
        {
            iconPen?.Dispose();
            iconPen = null;
            if (iconColor != Color.Transparent)
                iconPen = iconColor.toPen(3, LineCap.Round);
        }

        // draw the icon
        g.SmoothingMode = SmoothingMode.AntiAlias;
        switch (iconName)
        {
            default: throw new Exception($"Unexpected iconName: {iconName}");
            case null: break;

            case "close":
                PlotQuestMini.drawBackArrow(g, r.X + iconOffset.X, r.Y + iconOffset.Y, iconSize, iconPen!);
                break;
            case "envelope":
                PlotQuestMini.drawEnvelope(g, r.X + iconOffset.X, r.Y + iconOffset.Y, iconSize, iconPen!, isCurrent ? C.Brushes.grey : null);
                break;
            case "page":
                PlotQuestMini.drawPage(g, r.X + iconOffset.X, r.Y + iconOffset.Y, iconSize, iconPen!, isCurrent ? C.Brushes.grey : null);
                break;

            case "logo":
                {
                    var fat = isCurrent ? C.Pens.black4 : C.Pens.orange4;
                    var thin = isCurrent ? C.black.toPen(3) : C.orange.toPen(2);
                    var b = isCurrent ? C.Brushes.grey : C.Brushes.orangeDark;
                    PlotQuestMini.drawLogo(g, r.X + iconOffset.X, r.Y + iconOffset.Y, iconSize, fat, thin, b);
                    break;
                }
        }
        g.SmoothingMode = SmoothingMode.Default;

        if (sideBar)
            this.renderSideBar(g, isCurrent);

        return redraw;
    }

    protected void renderSideBar(Graphics g, bool isCurrent, Color? color = null)
    {
        var sideColor = color.HasValue ? color.Value : isCurrent ? C.orangeDark : C.orange;
        if (sidePen == null || sidePen.Color != sideColor)
        {
            sidePen?.Dispose();
            sidePen = sideColor.toPen(4);
        }

        if (sidePen != null)
            g.DrawLineR(sidePen, r.Right - 2, r.Top, 0, r.Height);
    }
}
