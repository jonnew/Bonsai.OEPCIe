using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Description("Heartbeat device")]
    public class HeartbeatDevice : ONIFrameReaderDevice<HeartbeatDataFrame>
    {

        public HeartbeatDevice() : base(oni.Device.DeviceID.HEARTBEAT) { }

        internal override IObservable<HeartbeatDataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            return source.Select(frame =>
            {
                return new HeartbeatDataFrame(frame, DeviceIndex.SelectedIndex, HardwareClockHz);
            });
        }
    }
}
