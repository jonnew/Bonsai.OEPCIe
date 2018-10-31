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

    [Description("Event information device.")]
    public class InfoDevice : Source<InfoDataFrame>
    {
        // Control registers (see oedevices.h)
        // TODO: set these
        enum Register
        {
            NULLPARM = 0, // No command
            WATCHDOGEN = 1, // Enable frame watchdog (0 = off; 1 = 0n)
            WATCHDOGDUR = 2, // Watchdog timer threshold (sysclock ticks, default = SYSCLOCK_HZ, 1 second)
        }

        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.device_t> devices;
        IObservable<InfoDataFrame> source;
        //private int hardware_clock_hz;

        public InfoDevice()
        {

            // Reference to context
            this.oepcie = OEPCIeManager.ReserveDAQ();

            // Find the hardware clock rate
            var sys_clock_hz = oepcie.DAQ.SystemClockHz;
            var hardware_clock_hz = oepcie.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.INFO
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new OEException((int)oe.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<InfoDataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oepcie.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                        observer.OnNext(new InfoDataFrame(frame, DeviceIndex.SelectedIndex, hardware_clock_hz, sys_clock_hz));
                };

                oepcie.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oepcie.Environment.FrameInputReceived -= inputReceived;
                    oepcie.Dispose();
                });
            });
        }

        public override IObservable<InfoDataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The information device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}
