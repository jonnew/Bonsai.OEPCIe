using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Description("Acquires data from a single RHDxxxx bioamplifier chip.")]
    public class AD7617Device : ONIFrameReaderDevice<AD7617DataFrame>
    {
        public AD7617Device() : base(oni.Device.DeviceID.AD7617) { }

        internal override IObservable<AD7617DataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            var data_block = new AD7617DataBlock(NumChannels, BlockSize);

            return source.Where(frame =>
            {
                return data_block.FillFromFrame(frame, DeviceIndex.SelectedIndex);
            })
            .Select(frame =>
            {
                var sample = new AD7617DataFrame(data_block, ctx.Environment.AcqContext.SystemClockHz); //TODO: Does this deep copy??
                data_block = new AD7617DataBlock(NumChannels, BlockSize);
                return sample;
            });
        }

        [Range(10, 10000)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 250;

        [Description("Number of channels.")]
        public int NumChannels { get; set; } = 12;
    }
}
