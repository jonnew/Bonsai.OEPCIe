using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.OEPCIe
{
    public class LightHouseDataBlock
    {
        private int SamplesPerBlock;

        private int index = 0;
        Queue<ushort> raw_samples = new Queue<ushort>();
        ulong[] remote_clock;
        ulong[] local_clock;
        ushort[] measureType;
        uint[] delay;
        ushort[] id;

        public LightHouseDataBlock(int samples_per_block = 5)
        {
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref remote_clock, samples_per_block);
            AllocateArray1D(ref local_clock, samples_per_block);
            AllocateArray1D(ref measureType, samples_per_block);
            AllocateArray1D(ref id, samples_per_block);
            AllocateArray1D(ref delay, samples_per_block);
        }

        public bool FillFromFrame(oe.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            // NB: Data contents: [uint16_t id, uint64_t remote_clock, uint16_t type, uint32_t delay]
            var sample = frame.Data<ushort>(device_index);

            local_clock[index] = frame.Time();
            id[index] = sample[0]; // TODO: remove id since its in the device map already
            remote_clock[index] = ((ulong)sample[1] << 48) | ((ulong)sample[2] << 32) | ((ulong)sample[3] << 16) | ((ulong)sample[4] << 0);
            measureType[index] = sample[5];
            delay[index] = ((uint)sample[6] << 16) | ((uint)sample[7] << 0);

            return ++index == SamplesPerBlock;
        }

        // Allocates memory for a 1-D array
        void AllocateArray1D<T>(ref T[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        public ushort[] ID
        {
            get { return id; }
        }

        public ulong[] LocalClock
        {
            get { return remote_clock; }
        }

        public ulong[] RemoteClock
        {
            get { return remote_clock; }
        }

        public ushort[] MeasureType
        {
            get { return measureType; }
        }
        public uint[] Delay
        {
            get { return delay; }
        }
    }
}
