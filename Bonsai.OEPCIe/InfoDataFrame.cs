using System.Linq;
using OpenCV.Net;

namespace Bonsai.OEPCIe
{
    public class InfoDataFrame
    {
        public InfoDataFrame(oe.Frame frame, int device_index, int hardware_clock_hz, int sys_clock_hz)
        {
            // NB: Data contents: [uint64_t remote_clock, uint16_t code]
            var sample = frame.Data<ushort>(device_index);

            FrameClock = frame.Clock();
            FrameTime = FrameClock / (double)sys_clock_hz;
            Clock = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            Time = Clock / (double)hardware_clock_hz;
            Code = sample[4];

        }

        public ulong FrameClock { get; private set; }

        public double FrameTime { get; private set; }

        public ulong Clock { get; private set; }

        public double Time { get; private set; }

        public int Code { get; private set; }

        // (see oedevices.h)
        public string CodeStr
        {
            get
            {
                switch (Code)
                {
                    case 0:
                        return "Heartbeat.";
                    case 1:
                        return "Watchdog barked. Where are your data sources?";
                    case 2:
                        return "SERDES hardware-level parity error. Data corrupt.";
                    case 3:
                        return "Serialized data CRC failed. Data corrupt.";
                    case 4:
                        return "Too many remote devices for host to support.";
                    case 5:
                        return "Remote initialization error.";
                    case 6:
                        return "Malformed packet during SERDES demultiplexing.";
                    default:
                        return "Unknown code.";
                }
            }
        }
    }
}
