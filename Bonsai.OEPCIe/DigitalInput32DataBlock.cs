using System;
using System.Collections.Generic;

namespace Bonsai.OEPCIe
{
    public class DigitalInput32DataBlock
    {
        private int SamplesPerBlock;

        private int index = 0;
        Queue<ushort> raw_samples = new Queue<ushort>();
        ulong[] clock;
        int[] port_state;

        public DigitalInput32DataBlock(int samples_per_block = 5)
        {
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref clock, samples_per_block);
            AllocateArray1D(ref port_state, samples_per_block);
        }

        public bool FillFromFrame(oe.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            // NB: Data contents: [uint64_t remote_clock, uint32_t port_state]
            var sample = frame.Data<ushort>(device_index);

            clock[index] = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            port_state[index] = ((int)sample[4] << 16) | ((int) sample[5] << 0);

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

        public int[] PortState
        {
            get { return port_state; }
        }
    }
}
