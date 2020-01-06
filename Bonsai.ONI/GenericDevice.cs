using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using OpenCV.Net;
using System.Reactive.Disposables;

namespace Bonsai.ONI
{
    [Description("Low-level access to any  device (used for debugging).")]
    public class GenericDevice : ONIFrameReaderDevice<Mat>
    {

        public GenericDevice() : base(oni.Device.DeviceID.NULL) { }

        int size_k = 1;
        Depth element_depth = Depth.U8;

        internal override IObservable<Mat> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            return source.Select(frame =>
            {
                var dat = frame.Data<byte>(DeviceIndex.SelectedIndex, ReadSize);
                var mat = new Mat(dat.Length, 1, ElementDepth, 1);

                return Mat.CreateMatHeader(dat, dat.Length / size_k, 1, element_depth, 1);
            });
        }

        [Description("The data read size (bytes).")]
        public int ReadSize { get; set; }

        [Description("The type of each element the data matrix holds.")]
        public Depth ElementDepth {
            get { return element_depth; }
            set
            {
                element_depth = value;
                switch (element_depth)
                {
                    case Depth.U16:
                    case Depth.S16:
                        size_k = 2;
                        break;
                    case Depth.F32:
                    case Depth.S32:
                        size_k = 4;
                        break;
                    case Depth.F64:
                        size_k = 8;
                        break;
                    default:
                        size_k = 1;
                        break;
                }
            }
        }

    }
}
