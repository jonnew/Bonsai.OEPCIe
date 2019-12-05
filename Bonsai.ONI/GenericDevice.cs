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
    public class GenericDevice : Source<Mat>
    {
        private ONIDisposableContext oni_ref; // Reference to global oni configuration set
        IObservable<Mat> source;
        private int hardware_clock_hz;

        public GenericDevice()
        {
            // Reference to context
            this.oni_ref = ONIManager.ReserveContext();

            source = Observable.Create<Mat>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex)) {

                        var dat = frame.Data<ushort>(DeviceIndex, ReadSize);
                        var mat = new Mat(1, dat.Length, ElementDepth, 1);

                        using (var header = Mat.CreateMatHeader(dat))
                        {
                            CV.Convert(header, mat);
                        }

                        observer.OnNext(mat);
                    }
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<Mat> Generate()
        {
            return source;
        }

        [Description("The index of the device handled by this node.")]
        public int DeviceIndex { get; set; }

        [Description("The data read size (bytes).")]
        public int ReadSize { get; set; }

        [Description("The type of each element the data matrix holds.")]
        public Depth ElementDepth { get; set; }

    }
}
