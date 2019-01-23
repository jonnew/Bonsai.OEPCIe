using System;
using OpenCV.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.OEPCIE.Prototyping
{
    public class PCEImage
    {
        double fs;
        bool alignment_needed;
        ushort[] image_data;

        //int col_start = 37; // The first 38 columns of the frame are used for firmware stuff

        public PCEImage(int rows, int cols, double sample_rate_hz)
        {
            Rows = rows;
            Cols = cols;
            image_data = new ushort[rows * cols];
            ImageData = Mat.CreateMatHeader(image_data, rows, cols, Depth.U16, 1);
            Image = ImageData.GetImage();
            fs = sample_rate_hz;
            alignment_needed = true;
        }

        internal bool FillFromFrame(oe.Frame frame, int device_index)
        {
            // [uint16_t row idx, uint16_t frame number, uint8_t px0, uint8_t px1, ....]
            var data = frame.Data<ushort>(device_index);

            // Which row is this data from?
            // Data packed big endian
            var row = data[0]; // - 16384; // 16384 used internally by firmware

            if (row < 0)
                return false;

            // Sanity check
            if (row >= Rows)
                throw new IndexOutOfRangeException();

            // We need to start at row 0
            if (alignment_needed && row == 0)
            {
                // Log times
                Clock = frame.Clock();
                Time = Clock / fs;
                alignment_needed = false;
            }

            // Copy data into current row
            Array.Copy(data, 1, image_data, row * Cols, Cols); //HDR_COLS

            // If we are at last row and started at first
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
