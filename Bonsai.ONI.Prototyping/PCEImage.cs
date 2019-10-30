using System;
using OpenCV.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ONI.Prototyping
{
    public class PCEImage
    {
        double fs;
        bool alignment_needed;
        internal ushort[] image_data;
        const ushort NO_UPDATE = 65535; // 2^16 - 1

        //int col_start = 37; // The first 38 columns of the frame are used for firmware stuff

        public PCEImage(int rows, int cols, ushort[] last_image_data, double sample_rate_hz)
        {
            Rows = rows;
            Cols = cols;

            // Last image is used as starting point
            if (last_image_data.Length != rows * cols)
                throw new IndexOutOfRangeException();
            image_data = last_image_data; // new ushort[rows * cols];

            ImageData = Mat.CreateMatHeader(image_data, rows, cols, Depth.U16, 1);
            Image = ImageData.GetImage();
            fs = sample_rate_hz;
            alignment_needed = true;
        }

        internal bool FillFromFrame(oni.Frame frame, int device_index)
        {
            // [uint16_t row idx, uint16_t frame number, uint8_t px0, uint8_t px1, ....]
            var data = frame.Data<ushort>(device_index);

            // Which row is this data from?
            // Data packed big endian
            var row = data[0];
            
            // Sanity check
            if (row < 0 || row >= Rows)
                throw new IndexOutOfRangeException();

            // We need to start at row 0
            if (alignment_needed && row == 0)
            {
                // Log times
                Clock = frame.Clock();
                Time = Clock / fs;
                alignment_needed = false;
            }

            // Loop through pixels looking for which to update
            for (int i = 0; i < Cols;  i++)
                if (data[i + 1] != NO_UPDATE)
                    image_data[row * Cols + i] = data[i + 1];

            // Copy data into current row
            //Array.Copy(data, 1, image_data, row * Cols, Cols);

            // TODO: remove.....\
            //if (row != last_row + 1)
            //    System.Console.WriteLine("zero row!");

            //last_row = row;

            //int sr = 0;
            //for (int i = 1; i < data.Length; i++)
            //    sr += data[i];

            //if (sr == 0)
            //    System.Console.WriteLine("zero row!");

            //s_raw += sr;

            //var row_data = image_data.Skip(row * Cols).Take(Cols).ToArray();
            //if (Enumerable.SequenceEqual(row_data, Enumerable.Repeat<ushort>(0, row_data.Length).ToArray()))
            //    System.Console.WriteLine("zero row: { }!", row); // throw new IndexOutOfRangeException();

            //int s = 0;
            //for (int i = 0; i < row_data.Length; i++)
            //    s += row_data[i];

            //if (s == 0)
            //    System.Console.WriteLine("zero row: { }!", row);

            // If we are at last row and started at first
            //var done = (row == Rows - 1 && !alignment_needed);

            //if (done) {

            //    int s = 0;
            //    for (int i = 0; i < image_data.Length; i++)
            //        s += image_data[i];



            //    if (s != 256)
            //        System.Console.WriteLine("zero row!");

            //}
            //......

            return row == Rows - 1 && !alignment_needed;
        }

        public ulong Clock { get; private set; }
        public double Time { get; private set; }
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public Mat ImageData { get; private set; }
        public IplImage Image { get; private set; }
    }
}
