using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.OEPCIe
{
    /// <summary>
    /// Provides Bonsai-friendly version of an RHDDataBlock
    /// </summary>
    public class RHDDataFrame
    {
        public RHDDataFrame(RHDDataBlock dataBlock, int hardware_clock_hz)
        {
            LocalClock = GetTimestampData(dataBlock.LocalClock, hardware_clock_hz);
            RemoteClock = GetTimestampData(dataBlock.RemoteClock, 100000000); // TODO: remote clock rate?
            EphysData = GetEphysData(dataBlock.EphysData);
            AuxiliaryData = GetAuxiliaryData(dataBlock.AuxiliaryData);
        }

        Mat GetTimestampData(ulong[] data, int hardware_clock_hz)
        {
            var ts = new double[data.Count()];
            double period_sec = 1.0 / (double)hardware_clock_hz;

            for(int i = 0; i < data.Count(); i++)
                ts[i] = period_sec * (double)data[i];

            return Mat.FromArray(ts, 1, data.Length, Depth.F64, 1); 
        }

        Mat GetEphysData(int[,] data)
        {
            if (data.Length == 0) return null;
            var numChannels = data.GetLength(0);
            var numSamples = data.GetLength(1);

            var output = new Mat(numChannels, numSamples, Depth.U16, 1);
            using (var header = Mat.CreateMatHeader(data))
            {
                CV.Convert(header, output);
            }

            return output;
        }

        Mat GetAuxiliaryData(int[,] data)
        {
            if (data.Length == 0) return null;
            var numChannels = data.GetLength(0);
            var numSamples = data.GetLength(1);

            var output = new Mat(numChannels, numSamples, Depth.U16, 1);
            using (var header = Mat.CreateMatHeader(data))
            {
                CV.Convert(header, output);
            }

            return output;
        }

        public Mat LocalClock { get; private set; }

        public Mat RemoteClock { get; private set; }

        public Mat EphysData { get; private set; }

        public Mat AuxiliaryData { get; private set; }

    }
}
