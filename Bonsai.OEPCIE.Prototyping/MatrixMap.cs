using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using OpenCV.Net;

namespace Bonsai.OEPCIe.Prototyping
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Map channels or columns of matrix.")]
    public class MatrixMap
    {

        Mat MatMap(Tuple<Mat, Mat> source)
        {

            var output = new Mat(source.Item1.Size, source.Item1.Depth, source.Item1.Channels);
            var map = source.Item2.Reshape(0, 1);

            // I dont know wtf is going on with this
            if (MapDimension == 0)
            {
                for (int i = 0; i < map.Cols; i++)
                    CV.Copy(source.Item1.GetRow((int)map[i].Val0), output.GetRow(i));
            }
            else
            {
                for (int i = 0; i < map.Cols; i++)
                    CV.Copy(source.Item1.GetCol((int)map[i].Val0), output.GetCol(i));
            }

            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return source.Select(input => MatMap(input));
        }

        [Description("Dimension to map. (e.g. 0 = rows.")]
        public int MapDimension { get; set; } = 0;

    }
}
