using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.ONI
{
    [DefaultProperty("Name")]
    [Description("Creates a Xillybus acqusition controller. A boolean input can be used to control the run state for gated acqusition.")]
    public class CreateXillyController : Source<ONIContext>, INamedElement
    {
        // Background, private value
        XillyConfiguration Value { get; set; }

        private string name = "Xilly 0";
        [Description("The name for this Xillybus acqusition context.")]
        public string Name
        {
            get { return name; }
            set { name = value; RefreshContext(); }
        }

        [Description("The block read size (bytes). Smaller has shorter latencies. Larger has more bandwidth.")]
        public int BlockReadSize
        {
            get { return Value.BlockReadSize; }
            set { Value.BlockReadSize = value; RefreshContext();}
        }

        //// TODO: use this instead of hardcoded paths
        //[Description("The context address.")]
        //public int Address { get; set; }

        [Description("Current device map.")]
        [System.Xml.Serialization.XmlIgnore]
        public Dictionary<int, oni.lib.device_t> DeviceMap { get; private set; }

        bool reset_context = false;
        [Description("Trigger context reset and get up-to-date device map.")]
        [System.Xml.Serialization.XmlIgnore]
        public bool ResetContext {
            get { return reset_context; }
            set { reset_context = false;  RefreshContext();}
        }

        internal void RefreshContext()
        {
            using (var ctx = ONIManager.ReserveContext(Name, Value))
            {
                DeviceMap = ctx.Environment.AcqContext.DeviceMap;
            }
        }

        public CreateXillyController()
        {
            Value = new XillyConfiguration();

            // For now, since this will be replaced with driver
            Value.ConfigurationPath = "\\\\.\\xillybus_cmd_32";
            Value.SignalPath = "\\\\.\\xillybus_signal_8";
            Value.DataInputPath = "\\\\.\\xillybus_data_read_32";
            Value.DataOutputPath = "\\\\.\\xillybus_data_write_32";
        }

        public override IObservable<ONIContext> Generate()
        {
             return Observable.Using(
                () => ONIManager.ReserveContext(Name, Value),
                ctx =>
                {
                    DeviceMap = ctx.Environment.AcqContext.DeviceMap;

                    ctx.Environment.Start();
                    ctx.Environment.AcqContext.Start();

                    return Observable
                        .Return(ctx)
                        .Concat(Observable.Never(ctx))
                        .Finally(() =>
                        {
                            //ctx.Environment.AcqContext.Stop();
                            ctx.Environment.Stop();
                            ctx.Dispose();
                        });
                });
        }

        // Toggle Running
        public IObservable<ONIContext> Generate(IObservable<bool> source)
        {
            var ctx = ONIManager.ReserveContext(Name, Value);
            DeviceMap = ctx.Environment.AcqContext.DeviceMap;

            ctx.Environment.Start();

            return source
                .SelectMany(x =>
                {
                    if (x) ctx.Environment.AcqContext.Start(); else ctx.Environment.AcqContext.Stop();
                    return Observable.Return(ctx);
                })
                .Finally(() =>
                {
                    ctx.Environment.AcqContext.Stop();
                    ctx.Dispose();
                });
        }
    }
}
