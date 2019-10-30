using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;

namespace Bonsai.OEPCIe.Design
{
    public abstract class DeviceIndexEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            //var deviceAttribute = (DeviceIndexAttribute)context.PropertyDescriptor.Attributes[typeof(DeviceIndexAttribute)];

            // Get selected index
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var control = new IndexSelectionEditor();
                control.populate(value);
                control.SelectedValueChanged += delegate { editorService.CloseDropDown(); };


            }
                using )
            {
                return dialog.IndexList.SelectedItem;
            }
        }
    }
}
