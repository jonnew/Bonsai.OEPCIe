using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.ONI
{
    static class ObservableFrame
    {
        public static System.Collections.Generic.Dictionary<int, oni.lib.device_t> FindMachingDevices(ONIContext oni_ref, oni.Device.DeviceID dev_id)
        {
            // If ID is NULL, then just return all devices because its used for debugging
            if (dev_id == oni.Device.DeviceID.NULL)
            {
                return oni_ref.Environment.AcqContext.DeviceMap;
            }
            else
            {
                // Find all matching devices
                return oni_ref.Environment.AcqContext.DeviceMap.Where(
                    pair => pair.Value.id == (uint)dev_id
                ).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        // IObservable<oni.Frame> for all frames from a particular context
        public static IObservable<oni.Frame> CreateFrameReader(ONIContext ctx)
        {
            return Observable.Create<oni.Frame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;
                    observer.OnNext(frame);
                };

                ctx.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    ctx.Environment.FrameInputReceived -= inputReceived;
                });
            });
        }

        // IObservable<oni.Frame> pre-filterd for a particular device type
        public static IObservable<oni.Frame> CreateFrameReader(ONIContext ctx, int dev_idx, oni.Device.DeviceID dev_id)
        {
            return Observable.Create<oni.Frame>(observer =>
            {
                // Find all matching devices
                //var devices = FindMachingDevices(ctx, dev_id);

                // Stop here if there are no devices to use
                //if (devices.Count == 0) throw new oni.ONIException(oni.lib.Error.DEVID);

                //var DeviceIndex = new DeviceIndexSelection();
                //DeviceIndex.Indices = devices.Keys.ToArray();

                EventHandler<FrameReceivedEventArgs> inputReceived;

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(dev_idx))
                        observer.OnNext(frame);
                };

                ctx.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    ctx.Environment.FrameInputReceived -= inputReceived;
                });
            });
        }
    }
}
