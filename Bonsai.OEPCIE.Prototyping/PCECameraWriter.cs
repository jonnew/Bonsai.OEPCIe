using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;
using OpenCV.Net;

namespace Bonsai.OEPCIE.Prototyping
{
    using oe;
    using OEPCIe;

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

            rows = (int)oepcie.DAQ.DeviceMap[DeviceIndex.SelectedIndex].num_writes;
            cols = (int)oepcie.DAQ.DeviceMap[DeviceIndex.SelectedIndex].write_size / 2; // ushorts, First element is row index and there is extra garbage at end
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
                    var data = new Mat(rows, cols, Depth.U16, 1);

                    var sub_data = data.GetSubRect(new Rect(1, 0, input.Cols, input.Rows));

                    // Convert element type if needed
                    var convertDepth = input.Depth != Depth.U16;
                    if (convertDepth) {
                        CV.Convert(input, sub_data);
                    } else {
                        sub_data = input;
                    }

                    // Make an array of row index codes and assign
                    var row_codes = new ushort[input.Rows];
                    for (int i = 0; i < input.Rows; i++)
                        row_codes[i] = (ushort)i; // (ushort)(i + 16384);

                    // Assign to first column of the data matrix
                    var row_codes_mat = data.GetSubRect(new Rect(0, 0, 1, input.Rows));
                    row_codes_mat = Mat.CreateMatHeader(row_codes);

                    // Write out matrix, row by row with the first number being an encoded row number
                    for (int i = 0; i < rows; i++) {
                        var row = data.GetRow(i);
                        oepcie.DAQ.Write((uint)DeviceIndex.SelectedIndex, data.Data, 2 * (data.Cols));
                    }

                });
        }

        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The PCE Camera handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

    }
}
