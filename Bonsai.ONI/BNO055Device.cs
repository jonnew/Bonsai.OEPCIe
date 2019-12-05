using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;


namespace Bonsai.ONI
{
    [Description("BNO055 inertial measurement unit.")]
    public class BNO055Device : SourceDevice<BNO055DataFrame>
    {
        // Control registers (see oedevices.h)
        //enum Register
        //{
        //      TODO: control device over i2c?
        //}

        public override IObservable<BNO055DataFrame> Generate()
        {
            return Observable.Using(
                () => ONIManager.ReserveContext(ContextName),
                resource =>
                {
                    // Reference to context
                    var ctx = ONIManager.ReserveContext(ContextName);

                    // Get device inidicies
                    var devices = ObservableDevice.FindMachingDevices(ctx, oni.Device.DeviceID.BNO055);
                    DeviceIndex.Indices = devices.Keys.ToArray();

                    return ObservableDevice
                        .CreateFrameReader(ctx, DeviceIndex, oni.Device.DeviceID.BNO055)
                        .Select(frame =>
                        {
                            return new BNO055DataFrame(frame, DeviceIndex.SelectedIndex, (int)50e6, ctx.Environment.AcqContext.SystemClockHz);
                        });
                });
        }
    }
}
