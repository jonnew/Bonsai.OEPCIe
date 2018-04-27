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
        bool ms_word_received = false;
        Queue<ushort> raw_samples = new Queue<ushort>();
        ulong[] remote_clock;
        ulong[] local_clock;
        bool[] measureType;
        bool lastType = false;
        uint[] delay;

        public LightHouseDataBlock(int samples_per_block = 5)
        {
            SamplesPerBlock = samples_per_block;

            AllocateArray1D(ref remote_clock, samples_per_block);
            AllocateArray1D(ref measureType, samples_per_block);
            AllocateArray1D(ref delay, samples_per_block);
        }

        public bool FillFromFrame(oe.Frame frame, int device_index)
        {
            if (index >= SamplesPerBlock)
                throw new IndexOutOfRangeException();

            // [uint64_t remote_clock, uint16_t type, uint32_t delay]
            var sample = frame.Data<ushort>(device_index);

            local_clock[index] = frame.Time();
            remote_clock[index] = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            measureType[index] = (sample[4] != 0);
            delay[index] = ((uint)sample[5] << 16) | ((uint)sample[6] << 0);

            return ++index == SamplesPerBlock;

            //foreach (var s in frame.Data<ushort>(device_index))
            //    raw_samples.Enqueue(s);

            //while (raw_samples.Count() > 0)
            //{
            //    var samp = raw_samples.Dequeue();

            //    bool ms = (samp & 0x8000) == 0;
            //    bool type = (samp & 0x4000) == 0;
            //    ushort delay_part = (ushort)(samp & 0x3FFF);

            //    if (ms && ms_word_received)
            //    {
            //        // We should not reach this state. Reset and skip
            //        ms_word_received = false;
            //        continue;
            //    }
            //    else if (!ms && !ms_word_received)
            //    {
            //        continue; // First packet was LS packet, skip
            //    }
            //    else if (ms && !ms_word_received)
            //    {
            //        if (lastType == type)
            //        {
            //            ms_word_received = false;
            //            continue;
            //        }

            //        timeStamp[index] = frame.Time();
            //        measureType[index] = type;
            //        delay[index] = 0;
            //        delay[index] |= (uint)(delay_part << 14);
            //        lastType = type;
            //        ms_word_received = true;
            //        continue;
            //    }
            //    else if (!ms && ms_word_received)
            //    {
            //        if (lastType ^ type)
            //        {
            //            ms_word_received = false;
            //            continue;
            //        }

            //        delay[index] |= (uint)delay_part;
            //        lastType = type;
            //        ms_word_received = false;
            //        return ++index == SamplesPerBlock;
            //    }
            //}

            return false;
        }

        // Allocates memory for a 1-D array
        void AllocateArray1D<T>(ref T[] array1D, int xSize)
        {
            Array.Resize(ref array1D, xSize);
        }

        public ulong[] LocalClock
        {
            get { return remote_clock; }
        }

        public ulong[] RemoteClock
        {
            get { return remote_clock; }
        }

        public bool[] MeasureType
        {
            get { return measureType; }
        }
        public uint[] Delay
        {
            get { return delay; }
        }
    }
}
