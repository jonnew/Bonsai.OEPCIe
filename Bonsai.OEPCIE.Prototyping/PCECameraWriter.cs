using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.OEPCIe.Prototyping
{
    using oe;

    [Description("Update exposure pattern on PCE camera prototype.")]
    public class PCECameraWriter : Sink<Mat>
    {
        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.device_t> devices;

        int rows;
        int cols;

        public PCECameraWriter() {

            // Reference to context
            oepcie = OEPCIeManager.ReserveDAQ();

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(pair => pair.Value.id == 10000).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oe.OEException((int)oe.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            //rows = (int)oepcie.DAQ.DeviceMap[DeviceIndex.SelectedIndex].num_writes;
            rows = 256;
            cols = (int)oepcie.DAQ.DeviceMap[DeviceIndex.SelectedIndex].write_size / 4; //4 // ushorts, First element is row index and there is extra garbage at end
        }

        // IplImage case
        public IObservable<Mat> Process(IObservable<IplImage> source)
        {
            // Transform IObservalbe<IplImage> to IObservable<Mat> using GetMat() method
            return Process(source.Select(
                input => {
                    return input.GetMat();
                })
            );
        }

        // Mat case
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            // TODO: what happens if more than one frame needs to be processed befor this finishes?
            return source.Do(
                input =>
                {
                    // Sanity check
                    if (rows < input.Rows || cols < input.Cols)
                        throw new IndexOutOfRangeException();

                    // Data to send (row indicator along with input exposure pattern)
                    var data = new Mat(rows, cols, Depth.S32, 1);//S32

                    var sub_data = data.GetSubRect(new Rect(1, 0, input.Cols, input.Rows));

                    // Convert element type if needed
                    var convertDepth = input.Depth != Depth.S32;//S32
                    if (convertDepth) {
                        CV.Convert(input, sub_data);
                    } else {
                        CV.Copy(input, sub_data);
                    }

                    // Write out matrix, row by row with the first number being an encoded row number
                    for (int i = 0; i < rows; i++) {
                        var row = data.GetRow(i);
                        row[0] = new Scalar(i + 16384, 0, 0, 0);
                        oepcie.DAQ.Write((uint)DeviceIndex.SelectedIndex, row.Data, 4 * (data.Cols));
                    }

                });
        }

        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The PCE Camera handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

    }
}
