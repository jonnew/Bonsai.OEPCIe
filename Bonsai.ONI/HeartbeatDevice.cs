using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;

namespace Bonsai.ONI
{
    [Description("Heartbeat device")]
    public class HeartbeatDevice : Source<HeartbeatDataFrame>
    {
        private ONIDisposableContext oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<HeartbeatDataFrame> source;
        private int hardware_clock_hz;

        public HeartbeatDevice()
        {
            // Reference to context
            this.oni_ref = ONIManager.ReserveContext();

            // Find the hardware clock rate
            var sys_clock_hz = oni_ref.AcqContext.SystemClockHz;
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all correct devices
            devices = oni_ref.AcqContext.DeviceMap.Where(
                    pair => pair.Value.id == (uint)oni.Device.DeviceID.HEARTBEAT
            ).ToDictionary(x => x.Key, x => x.Value);


            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<HeartbeatDataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                        observer.OnNext(new HeartbeatDataFrame(frame, DeviceIndex.SelectedIndex, sample_clock_hz, sys_clock_hz));
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<HeartbeatDataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The information device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

    }
}
