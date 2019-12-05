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

    [Description("Aquires data from a single TS4231 light to digital converter chip.")]
    public class LightHouseDevice : Source<LightHouseDataFrame>
    {
        private ONIDisposableContext oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<LightHouseDataFrame> source;

        public LightHouseDevice() {

            // Reference to context
            this.oni_ref = ONIManager.ReserveContext();

            // Find the hardware clock rate
            var sys_clock_hz = oni_ref.AcqContext.SystemClockHz;
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.AcqContext.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.TS4231
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<LightHouseDataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                        observer.OnNext(new LightHouseDataFrame(frame, DeviceIndex.SelectedIndex, sample_clock_hz, sys_clock_hz));
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<LightHouseDataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The TS4231 optical to digital converter handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

        [Range(1, 100)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 5;

        //[Range(0, (int)1e9)]
        //[Description("The remote clock frequency in Hz.")]
        //public int SampleClockHz { get; set; } = (int)42e6;
    }
}
