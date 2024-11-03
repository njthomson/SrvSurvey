using SrvSurvey.game;
using System.ComponentModel;

namespace SrvSurvey.forms
{
    [System.ComponentModel.DesignerCategory("")]
    public class BaseForm : Form
    {
        public BaseForm()
        {
            this.Icon = DraggableForm.logo2;
        }

        #region sizing and locations

        private static Dictionary<string, BaseForm> activeForms = new Dictionary<string, BaseForm>();

        public static T show<T>() where T : BaseForm, new()
        {
            var name = typeof(T).Name;

            var form = activeForms.GetValueOrDefault(name) as T;

            if (form == null)
            {
                form = new T() { Name = name, };

                // can we fit in our last location?
                var savedRect = Game.settings.formLocations.GetValueOrDefault(name);
                var sizable = form.FormBorderStyle.ToString().StartsWith("Sizable");
                if (savedRect != Rectangle.Empty)
                    Util.useLastLocation(form, savedRect, !sizable);

                activeForms[name] = form;
            }

            Util.showForm(form);
            return (T)form;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            activeForms.Remove(this.Name);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var savedRect = Game.settings.formLocations.GetValueOrDefault(this.Name);

            var rect = new Rectangle(this.Location, this.Size);
            if (savedRect != rect)
            {
                Game.settings.formLocations[this.Name] = rect;
                Game.settings.Save();
            }
        }

        #endregion

    }

    [System.ComponentModel.DesignerCategory("Form")]
    public class SizableForm : BaseForm
    {

    }

    [System.ComponentModel.DesignerCategory("Form")]
    public class FixedForm : BaseForm
    {

    }

}
