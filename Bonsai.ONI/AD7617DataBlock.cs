﻿using System;

namespace Bonsai.ONI
{
    /// <summary>
    /// Provides an low-level representation of a multi round-robbin sample of an AD7617 Chip
    /// </summary>
    public class AD7617DataBlock
    {
        private int SamplesPerBlock;
        public readonly int NumChannels;

        private int index = 0;
        ulong[] clock;
        short[,] data;

        public AD7617DataBlock(int num_channels, int samples_per_block = 250)
        {
            NumChannels = num_channels;
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref clock, samples_per_block);
            AllocateArray2D(ref data, num_channels, samples_per_block);
        }

        public bool FillFromFrame(oni.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            // [uint64_t local_clock, uint16_t ephys1, uint16_t ephys2, ... , uint16_t aux1, uint16_t aux2, ...]
            var raw = frame.Data<short>(device_index);

            clock[index] = ((ulong)raw[0] << 48) | ((ulong)raw[1] << 32) | ((ulong)raw[2] << 16) | ((ulong)raw[3] << 0);

            for (int chan = 0; chan < NumChannels; chan++)
            {
                data[chan, index] = raw[chan + 4]; // Start at index 4
            }

            return ++index == SamplesPerBlock;
        }

        // Allocates memory for a 1-D array of integers.
        void AllocateArray1D(ref ulong[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        // Allocates memory for a 2-D array of ushorts.
        void AllocateArray2D(ref short[,] array2D, int xSize, int ySize)
        {
            array2D = new short[xSize, ySize];
        }

        /// <summary>
        /// Gets the array of 64-bit unsigned local hardware clock.
        /// </summary>
        public ulong[] Clock
        {
            get { return clock; }
        }

        /// <summary>
        /// Gets the array of multidimensional amplifier data samples, indexed by data stream.
        /// </summary>
        public short[,] Data
        {
            get { return data; }
        }
    }
}
