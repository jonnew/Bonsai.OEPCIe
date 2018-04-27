using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;

namespace Bonsai.OEPCIe.Design
{
    public class DeviceIndexCollectionEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            
            if (editorService != null)
            {
                var control = new DeviceIndexSelectionControl();
                control.SelectedValue = value;
                control.SelectedValueChanged += delegate { editorService.CloseDropDown(); };
                editorService.DropDownControl(control);
                return control.SelectedValue;
            }

            return base.EditValue(context, provider, value);
        }
    }
}