using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Source]
    [Combinator(MethodName = "Generate")]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Acquires raw ONI read frames for debugging purposes.")]
    public class RawFrameDevice
    {
        public IObservable<RawDataFrame> Generate(IObservable<ONIContext> source)
        {

            var data_block = new RawDataBlock(BlockSize);

            return source.SelectMany(ctx =>
            {
                return ObservableFrame.CreateFrameReader(ctx)
                .Where(frame =>
                {
                    return data_block.FillFromFrame(frame);
                })
                .Select(frame =>
                {
                    var sample = new RawDataFrame(data_block, ctx.Environment.AcqContext.SystemClockHz); //TODO: Does this deep copy??
                    data_block = new RawDataBlock(BlockSize);
                    return sample;
                });
            });

        }

        [Range(10, 10000)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 250;

        //[Description("Should raw data be included in the output stream?")]
        //public bool IncludeRawData { get; set; } = false;
    }
}
