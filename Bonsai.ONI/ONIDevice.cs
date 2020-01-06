using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    [Source]
    [Combinator(MethodName = "Generate")]
    [WorkflowElementCategory(ElementCategory.Source)]
    public abstract class ONIFrameReaderDevice<T> : ONIDevice 
    {
        public ONIFrameReaderDevice(oni.Device.DeviceID dev_id) : base(dev_id) { }

        public IObservable<T> Generate(IObservable<ONIContext> source)
        {
            return source.SelectMany(ctx =>
            {
                // Get device inidicies
                var devices = ObservableFrame.FindMachingDevices(ctx, ID);
                if (devices.Count == 0) throw new oni.ONIException(oni.lib.Error.DEVID);
                DeviceIndex.Indices = devices.Keys.ToArray();

                return Process(ObservableFrame.CreateFrameReader(ctx, DeviceIndex.SelectedIndex, ID), ctx);
            });
        }

        internal abstract IObservable<T> Process(IObservable<oni.Frame> source, ONIContext ctx);

        [Description("The rate of the clock governing this device in Hz.")]
        public double HardwareClockHz { get; set; }
    }

    [Combinator(MethodName = "Process")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class ONIContextSinkDevice : ONIDevice
    {
        public ONIContextSinkDevice(oni.Device.DeviceID dev_id) : base(dev_id) { }

        public IObservable<ONIContext> Process(IObservable<ONIContext> source)
        {
            return source.Do(ctx =>
            {
                // Get device inidicies
                var devices = ObservableFrame.FindMachingDevices(ctx, ID);
                if (devices.Count == 0) throw new oni.ONIException(oni.lib.Error.DEVID);
                DeviceIndex.Indices = devices.Keys.ToArray();

                SideEffect(ctx);
            });
        }

        internal abstract void SideEffect(ONIContext ctx);
    }

    public abstract class ONIDevice
    {
        public readonly oni.Device.DeviceID ID = oni.Device.DeviceID.NULL;

        public ONIDevice(oni.Device.DeviceID dev_id)
        {
            DeviceIndex = new DeviceIndexSelection();
            ID = dev_id;
        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The device index handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}
