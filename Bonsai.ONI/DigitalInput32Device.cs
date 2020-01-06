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
    public class DigitalInput32Device : ONIFrameReaderDevice<DigitalInput32DataFrame>
    {
        // TODO
        enum Register
        {
            DINPUT32_NULLPARM = 0, // No command
            DINPUT32_NUM = 1, // Select a digital input pin to control (0-31)
            DINPUT32_TERM = 2, // Toggle 50 ohm termination (0 = Off, other = On)
            DINPUT32_LLEVEL = 3, // Set logic threshold level (0-255, actual voltage depends on circuitry)
        }

        public DigitalInput32Device() : base(oni.Device.DeviceID.DINPUT32) { }

        internal override IObservable<DigitalInput32DataFrame> Process(IObservable<oni.Frame> source, ONIContext ctx)
        {
            var data_block = new DigitalInput32DataBlock(BlockSize);

            return source.Where(frame =>
            {
                return data_block.FillFromFrame(frame, DeviceIndex.SelectedIndex);
            })
            .Select(frame =>
            {
                var sample = new DigitalInput32DataFrame(data_block, ctx.Environment.AcqContext.SystemClockHz); //TODO: Does this deep copy??
                data_block = new DigitalInput32DataBlock(BlockSize);
                return sample;
            });
        }

        [Range(1, 10000)]
        [Description("The size of data blocks, in samples, that are propogated in the observable sequence.")]
        public int BlockSize { get; set; } = 5;
    }
}
