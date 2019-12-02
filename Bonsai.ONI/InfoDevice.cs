using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    [Description("Event information device.")]
    public class InfoDevice : Source<InfoDataFrame>
    {
        // Control registers (see oedevices.h)
        // TODO: set these
        enum Register
        {
            HEARTBEAT = 0, // Heartbeat
            EWATCHDOG = 1,   // Frame not sent withing watchdog threshold
            ESERDESPARITY = 2,   // SERDES parity error detected
            ESERDESCHKSUM = 3,   // SERDES packet CRC error detected
            ETOOMANYREMOTE = 4,   // Too many remote devices for host to support
            EREMOTEINIT = 5,   // Remote initialization error
            EBADPACKET = 6,   // Malformed packet during SERDES demultiplexing
        }

        private ONIDisposable oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<InfoDataFrame> source;

        public InfoDevice()
        {
            // Reference to context
            this.oni_ref = ONIManager.ReserveDAQ();

            // Find the hardware clock rate
            var sys_clock_hz = oni_ref.DAQ.SystemClockHz;
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)oni.Device.DeviceID.INFO
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oni.ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<InfoDataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                        observer.OnNext(new InfoDataFrame(frame, DeviceIndex.SelectedIndex, sample_clock_hz, sys_clock_hz));
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<InfoDataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The information device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}
