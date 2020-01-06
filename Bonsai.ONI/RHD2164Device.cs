using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Description("Acquires data from a single RHD2164 bioamplifier chip.")]
    public class RHD2164Device : ONIFrameReaderDevice<RHDDataFrame>
    {
        public RHD2164Device() : base(oni.Device.DeviceID.RHD2164)
        {
            // Set defaults here, these settings can be manipulated in the outer scope and affect the functionality of the Task, I think.
            SampleRate = AmplifierSampleRate.SampleRate30000Hz;
            LowerBandwidth = 0.1;
            UpperBandwidth = 7500.0;
            DspCutoffFrequency = 1.0;
            DspEnabled = true;
        }

        internal override IObservable<RHDDataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            var data_block = new RHDDataBlock(64, BlockSize);

            return source.Where(frame =>
            {
                return data_block.FillFromFrame(frame, DeviceIndex.SelectedIndex);
            })
            .Select(frame =>
            {
                var sample = new RHDDataFrame(data_block, ctx.Environment.AcqContext.SystemClockHz); //TODO: Does this deep copy??
                data_block = new RHDDataBlock(64, BlockSize);
                return sample;
            });
        }

        // TODO: Implement these to affect configuration registers. They dont do anything right now.

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
