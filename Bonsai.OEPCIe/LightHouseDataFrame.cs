using System.Linq;
using OpenCV.Net;

namespace Bonsai.OEPCIe
{
    public class LightHouseDataFrame
    {
        public LightHouseDataFrame(LightHouseDataBlock dataBlock, int hardware_clock_hz)
        {
            Time = GetTime(dataBlock.Clock, hardware_clock_hz);
            Clock = GetClock(dataBlock.Clock);
            HighLow = GetHighLowData(dataBlock.HighLow);
        }

        Mat GetTime(ulong[] data, int hardware_clock_hz)
        {
            var ts = new double[data.Count()];
            double period_sec = 1.0 / hardware_clock_hz;

            for (int i = 0; i < data.Count(); i++)
                ts[i] = period_sec * (double)data[i];

            return Mat.FromArray(ts, 1, data.Length, Depth.F64, 1);
        }

        Mat GetClock(ulong[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.F64, 1); // TODO: abusing double to fit uint64_t
        }

        Mat GetHighLowData(ushort[] data)
        {
            if (data.Length == 0) return null;

            var output = new Mat(1, data.Length, Depth.U16, 1);
            using (var header = Mat.CreateMatHeader(data))
            {
                CV.Convert(header, output);
            }

            return output;
        }

        public Mat Clock { get; private set; }

        public Mat Time { get; private set; }

        public Mat HighLow { get; private set; }
    }
}
