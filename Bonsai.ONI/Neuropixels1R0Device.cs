using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Drawing.Design;

namespace Bonsai.ONI
{

    [Description("Acquires data from a single Neuropixels-6B chip.")]
    public class Neuropixels1R0Device : Source<Neuropixels1R0DataFrame>
    {
        private ONIDisposable oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<Neuropixels1R0DataFrame> source;
        private int hardware_clock_hz;

        public Neuropixels1R0Device()
        {
            // Reference to context
            this.oni_ref = ONIManager.ReserveDAQ();

            // Find the hardware clock rate
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)oni.Device.DeviceID.NEUROPIX1R0
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            // TODO: this aliases into some XML error
            if (devices.Count == 0)
                throw new oni.ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<Neuropixels1R0DataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;
                var data_block = new Neuropixels1R0DataBlock(BlockSize);

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
                            observer.OnNext(new Neuropixels1R0DataFrame(data_block, sample_clock_hz)); //TODO: Does this deep copy??
                            data_block = new Neuropixels1R0DataBlock(BlockSize);
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

        public override IObservable<Neuropixels1R0DataFrame> Generate()
        {
            return source;
        }

        // TODO: Implement these to affect configuration registers. They dont do anything right now.
        // This will look very similar to the estim device since NP is configured over i2c.

        [Category("Acquisition")]
        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The RHD device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

        [Category("Acquisition")]
        [Range(0, 100)]
        [Description("The size of data blocks, in round-robin samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 1;

        [Category("Configuration")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [Description("Gain Calibration CSV")]
        public string GainCalCSV { get; set; }

        [Category("Configuration")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [Description("ADC Calibration CSV")]
        public string ADCCalCSV { get; set; }

        [Category("Configuration")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [Description("Active Channels CSV")]
        public string ChannelsCSV { get; set; }

        [Category("Testing")]
        [Description("Enable TEST mode (generate sinewave on specified channel)")]
        public bool TestMode { get; set; }

    }
}
