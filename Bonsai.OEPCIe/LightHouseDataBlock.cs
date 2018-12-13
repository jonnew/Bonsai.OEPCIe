using System;
using System.Collections.Generic;

namespace Bonsai.OEPCIe
{
    public class LightHouseDataBlock
    {
        private int SamplesPerBlock;

        private int index = 0;
        Queue<ushort> raw_samples = new Queue<ushort>();
        ulong[] clock;
        ushort[] pulse_width;
        short[] pulse_type;

        public LightHouseDataBlock(int samples_per_block = 5)
        {
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref clock, samples_per_block);
            AllocateArray1D(ref pulse_width, samples_per_block);
            AllocateArray1D(ref pulse_type, samples_per_block);
        }

        public bool FillFromFrame(oe.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            // NB: Data contents: [uint64_t remote_clock, uint16_t pulse_width, int16_t pulse_type]
            var sample = frame.Data<ushort>(device_index);

            clock[index] = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            pulse_width[index] = sample[4];
            pulse_type[index] = (short)sample[5];

            return ++index == SamplesPerBlock;
        }

        // Allocates memory for a 1-D array
        void AllocateArray1D<T>(ref T[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        public ulong[] Clock
        {
            get { return clock; }
        }

        public ushort[] PulseWidth
        {
            get { return pulse_width; }
        }

        public short[] PulseType
        {
            get { return pulse_type; }
        }
    }
}
