namespace Bonsai.ONI
{
    public class HeartbeatDataFrame
    {
        public HeartbeatDataFrame(oni.Frame frame, int device_index, int hardware_clock_hz, int sys_clock_hz)
        {
            // NB: Data contents: [uint64_t remote_clock, uint16_t code]
            var sample = frame.Data<ushort>(device_index);

            FrameClock = frame.Clock();
            FrameTime = FrameClock / (double)sys_clock_hz;
            Clock = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            Time = Clock / (double)hardware_clock_hz;

        }

        public ulong FrameClock { get; private set; }

        public double FrameTime { get; private set; }

        public ulong Clock { get; private set; }

        public double Time { get; private set; }
    }
}
