using System.Linq;
using OpenCV.Net;

namespace Bonsai.ONI
{
    public class RawDataFrame
    {
        public RawDataFrame(RawDataBlock data_block, double hardware_clock_hz)
        {
            Clock = GetClock(data_block.Clock);
            Time = GetTime(data_block.Clock, hardware_clock_hz);
            Corrupt = GetByteMat(data_block.Corrupt);
            Devices = GetDevs(data_block.Devices);
            Data = GetByteMat(data_block.Data);
        }

        Mat GetClock(ulong[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.F64, 1); // TODO: abusing double to fit uint64_t
        }

        Mat GetTime(ulong[] data, double hardware_clock_hz)
        {
            var ts = new double[data.Count()];
            double period_sec = 1.0 / hardware_clock_hz;

            for (int i = 0; i < data.Count(); i++)
                ts[i] = period_sec * (double)data[i];

            return Mat.FromArray(ts, 1, data.Length, Depth.F64, 1);
        }

        Mat GetByteMat(bool[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.U8, 1);
        }

        Mat GetByteMat(byte[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.U8, 1);
        }

        Mat GetDevs(int[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.U16, 1);
        }

        public Mat Clock { get; private set; }

        public Mat Time { get; private set; }

        public Mat Corrupt { get; private set; }

        public Mat Devices { get; private set; }

        public Mat Data { get; private set; }
    }
}
