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
        private int SamplesPerBlock;
        public readonly int NumChannels;
        public readonly int NumAuxInChannels;

        private int index = 0;
        ulong[] local_clock;
        ulong[] remote_clock;
        int[,] ephysData;
        int[,] auxiliaryData;

        public RHDDataBlock(int num_ephys_channels, int samples_per_block = 250, int num_aux_in_channels = 3)
        {
            NumChannels = num_ephys_channels;
            NumAuxInChannels = num_aux_in_channels;
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref local_clock, samples_per_block);
            AllocateArray1D(ref remote_clock, samples_per_block);
            AllocateArray2D(ref ephysData, num_ephys_channels, samples_per_block);
            AllocateArray2D(ref auxiliaryData, num_aux_in_channels, samples_per_block);
        }

        public bool FillFromFrame(oe.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            local_clock[index] = frame.Time();

            // [uint64_t local_clock, uint16_t ephys1, uint16_t ephys2, ... , uint16_t aux1, uint16_t aux2, ...]
            var data = frame.Data<ushort>(device_index);

            // TODO
            //remote_clock[index] = ((ulong)data[0] << 48) | ((ulong)data[1] << 32) | ((ulong)data[2] << 16) | ((ulong)data[3] << 0);

            // TODO int chan = 4;
            int chan = 0;
            for (; chan < NumChannels; chan++)
            {
                ephysData[chan, index] = data[chan];
            }
            for (int k = 0; k < NumAuxInChannels; k++)
            {
                auxiliaryData[k, index] = data[chan++]; 
            }

            return ++index == SamplesPerBlock;
        }

        // Allocates memory for a 1-D array of integers.
        void AllocateArray1D(ref ulong[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        // Allocates memory for a 2-D array of integers.
        void AllocateArray2D(ref int[,] array2D, int xSize, int ySize)
        {
            array2D = new int[xSize, ySize];
        }

        /// <summary>
        /// Gets the array of 64-bit unsigned local hardware clock.
        /// </summary>
        public ulong[] LocalClock
        {
            get { return local_clock; }
        }

        /// <summary>
        /// Gets the array of 64-bit unsigned remote hardware clock.
        /// </summary>
        public ulong[] RemoteClock
        {
            get { return remote_clock; }
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
    }
}
