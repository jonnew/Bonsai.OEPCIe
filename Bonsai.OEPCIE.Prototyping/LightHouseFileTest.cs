using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

using TSource = System.Tuple<long, int, int>;
using TResult = Bonsai.OEPCIe.LightHouseDataFrame;

namespace Bonsai.OEPCIe.Prototyping
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Take tuple of doubles from lighthouse testing and make a data frame")]
    public class MakeLightHouseDataFrame : Transform<TSource, TResult>
    {

        public override IObservable<TResult>  Process(IObservable<TSource> source)
        {
            return source.Select(input =>
            {

                var result = new LightHouseDataFrame();
                result.Time = (double)input.Item1 / 50e6;
                result.PulseWidth = (double)input.Item2 / 50e6;
                result.PulseType = (short)input.Item3;

                return result;
            });
        }
    }
}
