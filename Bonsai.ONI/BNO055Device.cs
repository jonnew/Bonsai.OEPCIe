using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    using oni;

    [Description("BNO055 inertial measurement unit.")]
    public class BNO055Device : Source<BNO055DataFrame>
    {
        // Control registers (see oedevices.h)
        //enum Register
        //{
        //      TODO: control device over i2c?
        //}

        private ONIDisposable oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<BNO055DataFrame> source;

        public BNO055Device()
        {

            // Reference to context
            this.oni_ref = ONIManager.ReserveDAQ();

            // Find the hardware clock rate
            var sys_clock_hz = oni_ref.DAQ.SystemClockHz;
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.BNO055
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<BNO055DataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                        observer.OnNext(new BNO055DataFrame(frame, DeviceIndex.SelectedIndex, sample_clock_hz, sys_clock_hz));
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<BNO055DataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The information device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}
