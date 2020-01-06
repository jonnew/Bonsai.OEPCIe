using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Description("BNO055 inertial measurement unit.")]
    public class BNO055Device : ONIFrameReaderDevice<BNO055DataFrame>
    {
        // Control registers (see oedevices.h)
        //enum Register
        //{
        //      TODO
        //}

        public BNO055Device() : base(oni.Device.DeviceID.BNO055) { }

        // oni.Frame to BNODataFrame
        internal override IObservable<BNO055DataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            return source.Select(frame =>
            {
                return new BNO055DataFrame(frame, DeviceIndex.SelectedIndex, HardwareClockHz);
            });
        }
    }
}
