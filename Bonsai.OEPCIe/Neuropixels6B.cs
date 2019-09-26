using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Drawing.Design;

namespace Bonsai.OEPCIe
{
    using oe;

    [Description("Acquires data from a single Neuropixels-6B chip.")]
    public class Neuropixels6B : Source<Neuropixels6BDataFrame>
    {
        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.device_t> devices;
        IObservable<Neuropixels6BDataFrame> source;
        private int hardware_clock_hz;

        public Neuropixels6B()
        {
            // Reference to context
            this.oepcie = OEPCIeManager.ReserveDAQ();

            // Find the hardware clock rate
            hardware_clock_hz = oepcie.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.NEUROPIX6B
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oe.OEException((int)oe.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<Neuropixels6BDataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;
                var data_block = new Neuropixels6BDataBlock(BlockSize);

                oepcie.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;
                    //If this frame contaisn data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                    {
                        // Pull the sample
                        if (data_block.FillFromFrame(frame, DeviceIndex.SelectedIndex))
                        {
                            observer.OnNext(new Neuropixels6BDataFrame(data_block, hardware_clock_hz)); //TODO: Does this deep copy??
                            data_block = new Neuropixels6BDataBlock(BlockSize);
                        }
                    }
                };

                oepcie.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oepcie.Environment.FrameInputReceived -= inputReceived;
                    oepcie.Environment.Stop();
                    oepcie.Dispose();
                });
            });
        }

        public override IObservable<Neuropixels6BDataFrame> Generate()
        {
            return source;
        }

        // TODO: Implement these to affect configuration registers. They dont do anything right now.
        // This will look very similar to the estim device since NP is configured over i2c.

        [Category("Acquisition")]
        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
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
