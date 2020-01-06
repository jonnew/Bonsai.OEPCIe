using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.ONI
{
    [Description("Event information device.")]
    public class LinkInfoDevice : ONIFrameReaderDevice<LinkInfoDataFrame>
    {

        public LinkInfoDevice() : base(oni.Device.DeviceID.INFO) { }

        internal override IObservable<LinkInfoDataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            return source.Select(frame =>
            {
                return new LinkInfoDataFrame(frame, DeviceIndex.SelectedIndex, HardwareClockHz);
            });
        }
    }
}
