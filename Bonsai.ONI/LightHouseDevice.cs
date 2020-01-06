using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Description("Aquires data from a single TS4231 light to digital converter chip.")]
    public class LightHouseDevice : ONIFrameReaderDevice<LightHouseDataFrame>
    {

        public LightHouseDevice() : base(oni.Device.DeviceID.TS4231) { }

        internal override IObservable<LightHouseDataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            return source.Select(frame =>
            {
                return new LightHouseDataFrame(frame, DeviceIndex.SelectedIndex, HardwareClockHz);
            });
        }
    }
}
