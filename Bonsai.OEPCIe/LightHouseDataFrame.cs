using System.Linq;
using OpenCV.Net;

namespace Bonsai.OEPCIe
{
    public class LightHouseDataFrame
    {
        public LightHouseDataFrame()
        {
            // HACK for LightHouseFileTest
        }
        public LightHouseDataFrame(oe.Frame frame, int device_index, int sample_clock_hz, int sys_clock_hz)
        {
            // NB: Data contents: [uint64_t remote_clock, uint16_t width, int16_t type]
            var sample = frame.Data<ushort>(device_index);

            // Times
            FrameClock = frame.Clock();
            FrameTime = FrameClock / (double)sys_clock_hz;
            Clock = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            Time = Clock / (double)sample_clock_hz;

            // Data
            PulseWidth = sample[4] / (double)sample_clock_hz;
            PulseType = (short)sample[5];
        }

        public ulong FrameClock { get; private set; }

        public double FrameTime { get; private set; }

        public ulong Clock { get; private set; }

        public double Time { get; set; } // HACK for LightHouseFileTest

        public double PulseWidth { get; set; }// HACK for LightHouseFileTest

        public short PulseType { get; set; }// HACK for LightHouseFileTest
    }
}
