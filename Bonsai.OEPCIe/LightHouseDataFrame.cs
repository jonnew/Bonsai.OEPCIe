using System;
using System.Linq;
using OpenCV.Net;

namespace Bonsai.OEPCIe
{
    public class LightHouseDataFrame
    {
        public LightHouseDataFrame(LightHouseDataBlock dataBlock, int hardware_clock_hz)
        {
            LocalClock = GetClock(dataBlock.LocalClock, hardware_clock_hz);
            RemoteClock = GetClock(dataBlock.RemoteClock, 100000000); // TODO: remote clock rate?
            MeasurementType = GetTypeData(dataBlock.MeasureType);
            Delay = GetDelayData(dataBlock.Delay);
        }

        Mat GetClock(ulong[] data, int hardware_clock_hz)
        {
            var ts = new double[data.Count()];
            double period_sec = 1.0 / (double)hardware_clock_hz;

            for (int i = 0; i < data.Count(); i++)
                ts[i] = period_sec * (double)data[i];

            return Mat.FromArray(ts, 1, data.Length, Depth.F64, 1);
        }

        Mat GetTypeData(bool[] data)
        {
            if (data.Length == 0) return null;

            var output = new Mat(1, data.Length, Depth.U8, 1);
            using (var header = Mat.CreateMatHeader(Array.ConvertAll(data, item => item ? 1 : 0)))
            {
                CV.Convert(header, output);
            }

            return output;
        }

        Mat GetDelayData(uint[] data)
        {
            if (data.Length == 0) return null;

            var output = new Mat(1, data.Length, Depth.S32, 1);
            using (var header = Mat.CreateMatHeader(Array.ConvertAll(data, item => (int)item)))
            {
                CV.Convert(header, output);
            }

            return output;
        }

        public Mat LocalClock { get; private set; }

        public Mat RemoteClock { get; private set; }

        public Mat MeasurementType { get; private set; }

        public Mat Delay { get; private set; }
    }
}
