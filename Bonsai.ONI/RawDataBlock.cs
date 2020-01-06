using System;
using System.Collections.Generic;

namespace Bonsai.ONI
{
    /// <summary>
    /// Provides an low-level representation of a multi round-robbin sample of an AD7617 Chip
    /// </summary>
    public class RawDataBlock
    {
        private int SamplesPerBlock;
        public readonly int NumChannels;

        private int index = 0;
        ulong[] clock;
        bool[] corrupt;
        List<int> devices;
        List<byte> data;

        public RawDataBlock(int num_channels, int samples_per_block = 250)
        {
            NumChannels = num_channels;
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref clock, samples_per_block);
            AllocateArray1D(ref corrupt, samples_per_block);

            devices =  new List<int>(samples_per_block);
            data = new List<byte>(samples_per_block * 10); // Estimates
        }

        public bool FillFromFrame(oni.Frame frame)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            clock[index] = frame.Clock();
            corrupt[index] = frame.Corrupt();
            data.AddRange(frame.Data());
            devices.AddRange(frame.DeviceIndices);

            return ++index == SamplesPerBlock;
        }

        // Allocates memory for a 1-D array of integers.

        void AllocateArray1D<T>(ref T[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        /// <summary>
        /// Gets the array of 64-bit unsigned frame clock
        /// </summary>
        public ulong[] Clock
        {
            get { return clock; }
        }

        /// <summary>
        /// Gets the array of booleans indicate if frame data is corrupt
        /// </summary>
        public bool[] Corrupt
        {
            get { return corrupt; }
        }

        /// <summary>
        /// Gets the array of device indicies that provided frame data
        /// </summary>
        public int[] Devices
        {
            get { return devices.ToArray(); }
        }

        /// <summary>
        /// Gets the array of raw frame data
        /// </summary>
        public byte[] Data
        {
            get { return data.ToArray(); }
        }
    }
}
