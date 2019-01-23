using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Drawing.Design;

namespace Bonsai.OEPCIE.Prototyping
{
    using oe;
    using OEPCIe;

    [Description("Acquires images from PCE Camera prototypes (version x.x).")]
    public class PCECameraReader : Source<PCEImage>
    {
        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.device_t> devices;
        IObservable<PCEImage> source;
        private int hardware_clock_hz;

        public PCECameraReader()
        {
            // Reference to context
            oepcie = OEPCIeManager.ReserveDAQ();

            // Find the hardware clock rate
            hardware_clock_hz = oepcie.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oepcie.DAQ.DeviceMap.Where(pair => pair.Value.id == 10000).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new oe.OEException((int)oe.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            int rows = (int)oepcie.DAQ.DeviceMap[DeviceIndex.SelectedIndex].num_reads;
            int cols = (int)oepcie.DAQ.DeviceMap[DeviceIndex.SelectedIndex].read_size / 2 - 1; // ushorts, First element is row index

            source = Observable.Create<PCEImage>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;
                var image = new PCEImage(rows, cols, hardware_clock_hz); 

                oepcie.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    //If this frame contaisn data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                    {
                        // Pull the sample
                        if (image.FillFromFrame(frame, DeviceIndex.SelectedIndex))
                        {
                            observer.OnNext(image);
                            image = new PCEImage(rows, cols, hardware_clock_hz);
                        }
                    }
                };

                oepcie.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oepcie.Environment.FrameInputReceived -= inputReceived;
                    oepcie.Environment.Stop();
                    oepcie.Dispose();
                });
            });
        }

        public override IObservable<PCEImage> Generate()
        {
            return source;
        }

        [TypeConverter(typeof(DeviceIndexSelectionTypeConverter))]
        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The PCE Camera handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }
    }
}