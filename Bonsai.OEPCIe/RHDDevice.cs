using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.OEPCIe
{
    using oe;

    public class RHDDevice : Source<RHDDataFrame>
    {
        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.oepcie.device_t> devices;
        IObservable<RHDDataFrame> source;
        private int device_index = 1;

        public RHDDevice() {

            // Reference to context
            this.oepcie = OEPCIeManager.ReserveDAQ();

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.RHD2132 || pair.Value.id == (uint)Device.DeviceID.RHD2164
            ).ToDictionary(x => x.Key, x => x.Value); ;

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oe.OEException((int)oe.lib.oepcie.Error.DEVIDX);

            device_index = devices.Keys.First();

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

                    try { 
                        oepcie.Environment.Start();

                        var data_block = new RHDDataBlock(NumEphysChannels((int)devices[device_index].id));

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            // Blocks until frame available
                            var frame = frame_queue.Take();

                            // If this frame contaisn data from the selected device_index
                            if (frame.DeviceIndices.Contains(DeviceIndex))
                            {
                                // Pull the sample
                                if (data_block.FillFromFrame(frame, DeviceIndex))
                                { 
                                    observer.OnNext(new RHDDataFrame(data_block)); //TODO: Does this deep copy??
                                    data_block.Reset();  
                                }                               
                            }
                        }
                    }
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

        // TODO: Constrain based on device map
        //[Range(0, )]
        public int DeviceIndex { get; set; }

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

        private int NumEphysChannels(int id)
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

        private int NumAuxInChannels(int id)
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
