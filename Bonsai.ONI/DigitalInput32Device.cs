using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    using oni;

    [Description("A 32-bit digital input port.")]
    public class DigitalInput32Device : Source<DigitalInput32DataFrame>
    {
        // Control registers (see oedevices.h)
        enum Register
        {
            DINPUT32_NULLPARM = 0, // No command
            DINPUT32_NUM = 1, // Select a digital input pin to control (0-31)
            DINPUT32_TERM = 2, // Toggle 50 ohm termination (0 = Off, other = On)
            DINPUT32_LLEVEL = 3, // Set logic threshold level (0-255, actual voltage depends on circuitry)
        }

        enum Code
        {
            WATCHDOG = 0,         // Frame not sent withing watchdog threshold
            SERDESPARITY,     // SERDES parity error detected
            SERDESCHKSUM,     // SERDES packet checksum error detected
        }

        private ONIDisposableContext oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;
        IObservable<DigitalInput32DataFrame> source;
        private int hardware_clock_hz;

        public DigitalInput32Device() {

            // Reference to context
            //this.oni_ref = ONIManager.ReserveContext();

            // Find the hardware clock rate
            var sample_clock_hz = (int)50e6; // TODO: oni_ref.DAQ.AcquisitionClockHz;

            // Find all RHD devices
            devices = oni_ref.AcqContext.DeviceMap.Where(
                    pair => pair.Value.id == (uint)Device.DeviceID.DINPUT32
            ).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no devices to use
            if (devices.Count == 0)
                throw new ONIException((int)oni.lib.Error.DEVIDX);

            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            source = Observable.Create<DigitalInput32DataFrame>(observer =>
            {
                EventHandler<FrameReceivedEventArgs> inputReceived;
                var data_block = new DigitalInput32DataBlock(BlockSize);

                oni_ref.Environment.Start();

                inputReceived = (sender, e) =>
                {
                    var frame = e.Value;

                    // If this frame contains data from the selected device_index
                    if (frame.DeviceIndices.Contains(DeviceIndex.SelectedIndex))
                    {

                        // Pull the sample
                        if (data_block.FillFromFrame(frame, DeviceIndex.SelectedIndex))
                        {
                            observer.OnNext(new DigitalInput32DataFrame(data_block, sample_clock_hz));
                            data_block = new DigitalInput32DataBlock(BlockSize);
                        }
                    }
                };

                oni_ref.Environment.FrameInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    oni_ref.Environment.FrameInputReceived -= inputReceived;
                    oni_ref.Dispose();
                });
            });
        }

        public override IObservable<DigitalInput32DataFrame> Generate()
        {
            return source;
        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The 32-bit digital input port handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

        [Range(1, 10000)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 5;

        [Range(0, (int)1e9)]
        [Description("The remote clock frequency in Hz.")]
        public int RemoteClockHz { get; set; } = (int)42e6;
    }
}
