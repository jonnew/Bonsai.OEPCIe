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
        public RHDDataFrame(RHDDataBlock dataBlock)
        {
            Timestamp = GetTimestampData(dataBlock.Timestamp);
            EphysData = GetEphysData(dataBlock.EphysData);
            AuxiliaryData = GetAuxiliaryData(dataBlock.AuxiliaryData);
            //TtlOut = GetTtlData(dataBlock.TtlOut);
        }

        Mat GetTimestampData(uint[] data)
        {
            return Mat.FromArray(data, 1, data.Length, Depth.S32, 1); // TODO: No ulong option??
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

        public Mat Timestamp { get; private set; }

        public Mat EphysData { get; private set; }

        public Mat AuxiliaryData { get; private set; }

        //public Mat TtlOut { get; private set; }

        //public double BufferCapacity { get; private set; }
    }
}
