using System.Linq;
using OpenCV.Net;

namespace Bonsai.ONI
{
    public class DigitalInput32DataFrame
    {
        public DigitalInput32DataFrame(DigitalInput32DataBlock dataBlock, int hardware_clock_hz)
        {
            Clock = GetClock(dataBlock.Clock);
            Time = GetTime(dataBlock.Clock, hardware_clock_hz);
            PortState = GetState(dataBlock.PortState);
        }

        Mat GetClock(ulong[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.F64, 1); // TODO: abusing double to fit uint64_t
        }

        Mat GetTime(ulong[] data, int hardware_clock_hz)
        {
            var ts = new double[data.Count()];
            double period_sec = 1.0 / (double)hardware_clock_hz;

            for (int i = 0; i < data.Count(); i++)
                ts[i] = period_sec * (double)data[i];

            return Mat.FromArray(ts, 1, data.Length, Depth.F64, 1);
        }

        Mat GetState(int[] data)
        {
            if (data.Length == 0) return null;

            var output = new Mat(1, data.Length, Depth.S32, 1);
            using (var header = Mat.CreateMatHeader(data, 1, data.Length, Depth.S32, 1))
            {
                CV.Convert(header, output);
            }

            return output;
        }

        public Mat Clock { get; private set; }

        public Mat Time { get; private set; }

        public Mat RemoteClock { get; private set; }

        public Mat PortState { get; private set; }
    }
}
