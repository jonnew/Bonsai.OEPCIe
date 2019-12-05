using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Drawing.Design;

namespace Bonsai.ONI.Prototyping
{
    [Description("Acquires images from PCE Camera prototypes (version x.x).")]
    public class PCECameraReader : Source<PCEImage>
    {
        private ONIDisposableContext oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<PCEImage> source;

        public PCECameraReader()
        {
            // Reference to context
            //oni_ref = ONIManager.ReserveContext();

            // Find the hardware clock rate
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.AcqContext.DeviceMap.Where(pair => pair.Value.id == 10000).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oni.ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            int rows = (int)oni_ref.AcqContext.DeviceMap[DeviceIndex.SelectedIndex].num_reads;
            int cols = (int)oni_ref.AcqContext.DeviceMap[DeviceIndex.SelectedIndex].read_size / 2 - 1; // ushorts, First element is row index

            var image_data = new ushort[rows * cols];

            source = Observable.Create<PCEImage>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;
                var image = new PCEImage(rows, cols, image_data, sample_clock_hz); 

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // TODO: There is a bug here that needs to be fixed. It potentially applies to all devices.
                    // 1. If we look at low level test programs, and send a fixed pattern of increasing row numbers, we see that all row numbers are read out in order
                    // 2. If we send this same data here, we see that rows are regularly skipped. This seems to indicate that events are either being ingored (contain the wrong device) or they are being dropped.
                
                    //If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                    {
                        // Pull the sample
                        if (image.FillFromFrame(frame, DeviceIndex.SelectedIndex))
                        {
                            observer.OnNext(image);
                            image_data = image.image_data;
                            image = new PCEImage(rows, cols, image_data, sample_clock_hz);
                        }
                    }
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Environment.Stop();
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<PCEImage> Generate()
        {
            return source;
        }

        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The PCE Camera handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}