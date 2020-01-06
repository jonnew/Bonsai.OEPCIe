
namespace Bonsai.ONI
{
    public class LinkInfoDataFrame
    {
        public LinkInfoDataFrame(oni.Frame frame, int device_index, double hardware_clock_hz)
        {
            // NB: Data contents: [uint64_t remote_clock, uint16_t code]
            var sample = frame.Data<ushort>(device_index);

            Clock = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            Time = Clock / hardware_clock_hz;
            Code = sample[4];
        }

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
                        return "Lost lock.";
                    case 1:
                        return "Malformed packet during SERDES demultiplexing.";
                    case 2:
                        return "Remote initialization error.";
                    case 3:
                        return "Too many remote devices for host to support.";
                    case 4:
                        return "Serialized data CRC failed. Data corrupt.";
                    case 5:
                        return "SERDES hardware-level parity error. Data corrupt.";
                    case 6:
                        return "Watchdog barked. Where are your data sources?";
                    default:
                        return "Unknown code.";
                }
            }
        }
    }
}
