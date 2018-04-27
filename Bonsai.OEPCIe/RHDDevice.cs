using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.OEPCIe
{
    using oe;
    using System.Drawing.Design;

    [Description("Acquires data from RHDxxxx bioamplifier chips.")]
    public class RHDDevice : Source<RHDDataFrame>
    {
        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.oepcie.device_t> devices;
        IObservable<RHDDataFrame> source;
        private int hardware_clock_hz;

        public RHDDevice() {

            // Reference to context
            this.oepcie = OEPCIeManager.ReserveDAQ();

            // Find the hardware clock rate
            hardware_clock_hz = oepcie.DAQ.GetOption(Context.Option.SYSCLKHZ);

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.RHD2132 || pair.Value.id == (uint)Device.DeviceID.RHD2164
            ).ToDictionary(x => x.Key, x => x.Value);

            //var keys = devices.Keys.ToList();

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oe.OEException((int)oe.lib.oepcie.Error.DEVIDX);

            DeviceSelection = new DeviceIndexSelection(devices); // .SelectedIndex = devices.Keys.First();

            // Set defaults here, these settings can be manipulated in the outer scope and affect the functionality of the Task, I think.
            SampleRate = AmplifierSampleRate.SampleRate30000Hz;
            LowerBandwidth = 0.1;
            UpperBandwidth = 7500.0;
            DspCutoffFrequency = 1.0;
            DspEnabled = true;

            source = Observable.Create<RHDDataFrame>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var frame_queue = oepcie.Environment.Subscribe();

                    try
                    {
                        oepcie.Environment.Start();

                        var data_block = new RHDDataBlock(NumEphysChannels((int)devices[DeviceSelection.SelectedIndex].id), BlockSize);

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            // Blocks until frame available
                            var frame = frame_queue.Take(cancellationToken);

                            // If this frame contaisn data from the selected device_index
                            if (frame.DeviceIndices.Contains(DeviceSelection.SelectedIndex))
                            {
                                // Pull the sample
                                if (data_block.FillFromFrame(frame, DeviceSelection.SelectedIndex))
                                {
                                    observer.OnNext(new RHDDataFrame(data_block, hardware_clock_hz)); //TODO: Does this deep copy??
                                    data_block = new RHDDataBlock(NumEphysChannels((int)devices[DeviceSelection.SelectedIndex].id), BlockSize);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException) { } // Thrown by frame_queue.Take(cancellationToken);
                    finally
                    {
                        oepcie.Environment.Stop();
                        oepcie.Environment.Unsubscribe(frame_queue);
                        //oepcie.Dispose(); // TODO: this should only really dispose if reference count is 0
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            })
            .PublishReconnectable()
            .RefCount();
        }

        public override IObservable<RHDDataFrame> Generate()
        {
            return source;
        }

        public enum Parameter // TODO
        {
            NULLPARM = 0,  // No command
            SAMPLERATE = 1,
        }

        // TODO: Implement these to affect configuration registers. They dont do anything right now.

        // TODO: Constrain based on device map
        [Editor("Bonsai.OEPCIe.Design.IndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The RHD Device handled by this node.")]
        //[DeviceIndexAttribute(devices.Keys.ToArray())]
        public DeviceIndexSelection DeviceSelection { get; set; }
        //public int DeviceIndex { get; set; }

        //[Category(BoardCategory)]
        [Range(10, 10000)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 250;

        //[Category(BoardCategory)]
        [Description("The per-channel sampling rate.")]
        public AmplifierSampleRate SampleRate { get; set; }

        //[Category(BoardCategory)]
        [Description("Specifies whether to fast settle amplifiers when reconfiguring the evaluation board.")]
        public bool FastSettle { get; set; }

        //[Category(BoardCategory)]
        [Description("The lower bandwidth of the amplifier on-board DSP filter (Hz).")]
        public double LowerBandwidth { get; set; }

        //[Category(BoardCategory)]
        [Description("The upper bandwidth of the amplifier on-board DSP filter (Hz).")]
        public double UpperBandwidth { get; set; }

        //[Category(BoardCategory)]
        [Description("The cutoff frequency of the DSP offset removal filter (Hz).")]
        public double DspCutoffFrequency { get; set; }

        //[Category(BoardCategory)]
        [Description("Specifies whether the DSP offset removal filter is enabled.")]
        public bool DspEnabled { get; set; }

        static private int NumEphysChannels(int id)
        {
            switch (id)
            {
                case (int)Device.DeviceID.RHD2132:
                    return 32;
                case (int)Device.DeviceID.RHD2164:
                    return 64;
                default:
                    throw new oe.OEException((int)oe.lib.oepcie.Error.DEVID);
            }
        }

        static private int NumAuxInChannels(int id)
        {
            switch (id)
            {
                case (int)Device.DeviceID.RHD2132:
                case (int)Device.DeviceID.RHD2164:
                    return 3;
                default:
                    throw new oe.OEException((int)oe.lib.oepcie.Error.DEVID);
            }
        }

        // Specifies the available per-channel sampling rates.
        public enum AmplifierSampleRate
        {
            SampleRate1000Hz = 0,
            SampleRate1250Hz = 1,
            SampleRate1500Hz = 2,
            SampleRate2000Hz = 3,
            SampleRate2500Hz = 4,
            SampleRate3000Hz = 5,
            SampleRate3333Hz = 6,
            SampleRate4000Hz = 7,
            SampleRate5000Hz = 8,
            SampleRate6250Hz = 9,
            SampleRate8000Hz = 10,
            SampleRate10000Hz = 11,
            SampleRate12500Hz = 12,
            SampleRate15000Hz = 13,
            SampleRate20000Hz = 14,
            SampleRate25000Hz = 15,
            SampleRate30000Hz = 16
        }
    }
}
