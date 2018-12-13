using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.OEPCIe
{
    using oe;

    [Description("BNO055 inertial measurement unit.")]
    public class BNO055Device : Source<BNO055DataFrame>
    {
        // Control registers (see oedevices.h)
        //enum Register
        //{
        //      TODO: control device over i2c?
        //}

        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.device_t> devices;
        IObservable<BNO055DataFrame> source;

        public BNO055Device()
        {

            // Reference to context
            this.oepcie = OEPCIeManager.ReserveDAQ();

            // Find the hardware clock rate
            var sys_clock_hz = oepcie.DAQ.SystemClockHz;
            var hardware_clock_hz = oepcie.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.BNO055
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new OEException((int)oe.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<BNO055DataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oepcie.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                        observer.OnNext(new BNO055DataFrame(frame, DeviceIndex.SelectedIndex, hardware_clock_hz, sys_clock_hz));
                };

                oepcie.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oepcie.Environment.FrameInputReceived -= inputReceived;
                    oepcie.Dispose();
                });
            });
        }

        public override IObservable<BNO055DataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The information device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}
