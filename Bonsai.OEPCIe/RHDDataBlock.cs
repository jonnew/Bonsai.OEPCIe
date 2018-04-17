using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.OEPCIe
{
    /// <summary>
    /// Provides an low-level representation of a multi round-robbin sample of an RHD Chip
    /// - Largely copied from Rhythm.NET
    /// </summary>
    public class RHDDataBlock
    {
        const int SamplesPerBlock = 256;

        public readonly int NumChannels;
        public readonly int NumAuxInChannels;

        private int index = 0;
        uint[] timeStamp;
        int[,] ephysData;
        int[,] auxiliaryData;
        //int[] ttlOut;

        public RHDDataBlock(int num_ephys_channels, int num_aux_in_channels = 3)
        {
            NumChannels = num_ephys_channels;
            NumAuxInChannels = num_aux_in_channels;

            AllocateArray1D(ref timeStamp, SamplesPerBlock);
            AllocateArray2D(ref ephysData, num_ephys_channels, SamplesPerBlock);
            AllocateArray2D(ref auxiliaryData, num_aux_in_channels, SamplesPerBlock);
            //AllocateIntArray1D(ref ttlOut, samplesPerBlock);
        }

        public bool FillFromFrame(oe.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            timeStamp[index] = (uint)frame.Time();
            var data = frame.Data<UInt16>((int)device_index);

            int chan = 0;
            for (chan = 0; chan < NumChannels; chan++)
            {
                ephysData[chan, index] = data[chan];
            }
            for (int k = 0; k < NumAuxInChannels; k++)
            {
                auxiliaryData[k, index] = data[chan++]; 
            }

            return ++index == SamplesPerBlock;
        }

        public void Reset()
        {
            index = 0;
        }

        // Allocates memory for a 1-D array of integers.
        void AllocateArray1D(ref uint[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        // Allocates memory for a 2-D array of integers.
        void AllocateArray2D(ref int[,] array2D, int xSize, int ySize)
        {
            array2D = new int[xSize, ySize];
        }

        /// <summary>
        /// Gets the array of 64-bit unsigned sample timestamps.
        /// </summary>
        public uint[] Timestamp
        {
            get { return timeStamp; }
        }

        /// <summary>
        /// Gets the array of multidimensional amplifier data samples, indexed by data stream.
        /// </summary>
        public int[,] EphysData
        {
            get { return ephysData; }
        }

        /// <summary>
        /// Gets the array of multidimensional auxiliary data samples, indexed by data stream.
        /// </summary>
        public int[,] AuxiliaryData
        {
            get { return auxiliaryData; }
        }

        // Haha no void casting!
        //int ConvertWord(byte[] buffer, int index)
        //{
        //    uint x1, x2, result;

        //    x1 = (uint)buffer[index];
        //    x2 = (uint)buffer[index + 1];

        //    result = (x2 << 8) | (x1 << 0);

        //    return (int)result;
        //}

        ///// <summary>
        ///// Gets an array indicating the state of the 16 digital TTL output lines on the FPGA.
        ///// </summary>
        //public int[] TtlOut
        //{
        //    get { return ttlOut; }
        //}
    }
}
