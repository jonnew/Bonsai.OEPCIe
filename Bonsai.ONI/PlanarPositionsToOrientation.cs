using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using OpenCV.Net;

namespace Bonsai.ONI
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Estimates an orientation using at least 3 approximately co-planar 3D positions.")]
    public class PlanarPositionsToOrientation
    {
        private Tuple<bool, Mat> Process(IEnumerable<Position3D> positions)
        {
            // If appropriate, filter to find all position measures within the time window
            if (Window > 0) {

                // NB: this step deals with the fact that sometimes occlusions will mean skiped frames from some receivers. 
                // We only want to compare estimated positions that occured close in time.
                // Maybe we want to do this with a reactive operator though...?
                var latest = positions.Select(pos => pos.Time).ToArray().Max();
                positions = positions.Where(pos => latest - pos.Time < Window);

                // If there are not enough good positions remaining, 
                // fail predicate
                if (positions.Count() < 3)
                    return new Tuple<bool, Mat>(false, null);
            }

            Mat data = new Mat(3, positions.Count(), Depth.F64, 1);
            var j = 0;
            foreach (var p in positions)
            {
                CV.Copy(p.Matrix, data.GetCol(j++));
            }

            // Shift the data to 0 mean
            Mat row_mean = new Mat(3, 1, Depth.F64, 1);
            CV.Reduce(data, row_mean, 1, ReduceOperation.Avg);
            data = data - row_mean;

            // SVD
            // See https://www.ltu.se/cms_fs/1.51590!/svd-fitting.pdf
            Mat S = new Mat(3, 1, Depth.F64, 1);
            Mat U = new Mat(3,3, Depth.F64, 1);
            CV.SVD(data, S, U);

            // Get Quaterion
            // See https://math.stackexchange.com/questions/2889712/how-to-calculate-quaternions-from-principal-axes-of-an-ellipsoid

            // Rotation angle
            var theta = Math.Acos(0.5 * (CV.Trace(U).Val0 - 1));

            // Rotation axis
            Mat Ut = new Mat(3, 3, Depth.F64, 1);
            CV.Transpose(U, Ut);
            CV.Sub(U, Ut, U);
            var ax = U[1, 2].Val0;
            var ay = U[2, 0].Val0;
            var az = U[0, 1].Val0;

            // To quaternion
            Mat quat = new Mat(4, 1, Depth.F64, 1);
            var s = Math.Sin(theta / 2);
            quat[0] = new Scalar(ax * s);
            quat[1] = new Scalar(ay * s);
            quat[2] = new Scalar(az * s);
            quat[3] = new Scalar(Math.Cos(theta / 2));

            return new Tuple<bool, Mat>(true, quat);
        }

        public IObservable<Mat> Process(IObservable<Tuple<Position3D, Position3D, Position3D>> source)
        {
            return source
                .Select(input => Process(new[] { input.Item1, input.Item2, input.Item3 }))
                .Where(input => input.Item1)
                .Select(input => input.Item2);
        }

        public IObservable<Mat> Process(IObservable<Tuple<Position3D, Position3D, Position3D, Position3D>> source)
        {
            return source
                .Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4 }))
                .Where(input => input.Item1)
                .Select(input => input.Item2);
        }

        public IObservable<Mat> Process(IObservable<Tuple<Position3D, Position3D, Position3D, Position3D, Position3D>> source)
        {
            return source
                .Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5 }))
                .Where(input => input.Item1)
                .Select(input => input.Item2);
        }

        public IObservable<Mat> Process(IObservable<Tuple<Position3D, Position3D, Position3D, Position3D, Position3D, Position3D>> source)
        {
            return source
                .Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6 }))
                .Where(input => input.Item1)
                .Select(input => input.Item2);
        }

        public IObservable<Mat> Process(IObservable<Tuple<Position3D, Position3D, Position3D, Position3D, Position3D, Position3D, Position3D>> source)
        {
            return source
                .Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6, input.Item7 }))
                .Where(input => input.Item1)
                .Select(input => input.Item2);
        }

        public IObservable<Mat> Process(IObservable<Tuple<Position3D, Position3D, Position3D, Position3D, Position3D, Position3D, Position3D, Position3D>> source)
        {
            return source
                .Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6, input.Item7, input.Rest }))
                .Where(input => input.Item1)
                .Select(input => input.Item2);
        }

        [Description("Time window, in seconds, in which position measurments are considered simultaneous. Setting to 0 turns off window filtering.")]
        public double Window { get; set; } = 0.0;

    }
}
