using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Drawing.Design;


namespace Bonsai.ONI
{
    public abstract class SourceDevice<T> : Source<T>
    {
        public SourceDevice()
        {
            DeviceIndex = new DeviceIndexSelection();
        }

        [TypeConverter(typeof(ContextNameConverter))]
        [Description("The name of the ONI context supplying frames to this node.")]
        public string ContextName { get; set; } = "ONI0";

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The device index handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }

    public abstract class Device
    {
        public Device()
        {
            DeviceIndex = new DeviceIndexSelection();
        }

        [TypeConverter(typeof(ContextNameConverter))]
        [Description("The name of the ONI context supplying frames to this node.")]
        public string ContextName { get; set; } = "ONI0";

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The device index handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }

}
