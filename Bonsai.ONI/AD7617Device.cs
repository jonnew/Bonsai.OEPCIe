using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    using oni;

    [Description("Acquires data from a single RHDxxxx bioamplifier chip.")]
    public class AD7617Device : Source<AD7617DataFrame>
    {
        private ONIDisposable oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<AD7617DataFrame> source;

        public AD7617Device()
        {
            // Reference to context
            this.oni_ref = ONIManager.ReserveDAQ();

            // Find the hardware clock rate
            var sample_clock_hz = (int)250e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.AD7617
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oni.ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<AD7617DataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;
                var data_block = new AD7617DataBlock(NumChannels, BlockSize);

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;
                    //If this frame contaisn data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                    {
                        // Pull the sample
                        if (data_block.FillFromFrame(frame, DeviceIndex.SelectedIndex))
                        {
                            observer.OnNext(new AD7617DataFrame(data_block, sample_clock_hz)); //TODO: Does this deep copy??
                            data_block = new AD7617DataBlock(NumChannels, BlockSize);
                        }
                    }
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Environment.Stop();
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<AD7617DataFrame> Generate()
        {
            return source;
        }

        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The RHD device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get;  set; }

        // TODO: Implement these to affect configuration registers. They dont do anything right now.

        //[Category(BoardCategory)]
        [Range(10, 10000)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 250;

        //[Category(BoardCategory)]
        [Description("Number of channels.")]
        public int NumChannels { get; set; } = 12;
    }
}
