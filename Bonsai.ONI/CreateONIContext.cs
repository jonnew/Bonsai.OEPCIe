using Bonsai.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ONI
{
    [DefaultProperty("Name")]
    [Description("Creates an ONI acqusition context. A boolean input can be used to control the run state for gated acqusition.")]
    public class CreateONIContext : Source<oni.Context>, INamedElement
    {
        readonly ONIConfiguration configuration = new ONIConfiguration();

        [Description("The name for this ONI context.")]
        public string Name { get; set; } = "ONI0";

        [Description("The block read size (bytes). Smaller has shorter latencies. Larger has more bandwidth.")]
        public int BlockReadSize
        {
            get { return configuration.BlockReadSize; }
            set { configuration.BlockReadSize = value; }
        }

        public CreateONIContext()
        {
            // For now, since this will be replaced with driver
            configuration.ConfigurationPath = "\\\\.\\xillybus_cmd_32";
            configuration.SignalPath = "\\\\.\\xillybus_signal_8";
            configuration.DataInputPath = "\\\\.\\xillybus_data_read_32";
            configuration.DataOutputPath = "\\\\.\\xillybus_data_write_32";
        }

        public override IObservable<oni.Context> Generate()
        {
            return Observable.Using(
                () => ONIManager.ReserveContext(Name, configuration),
                ctx =>
                {
                    ctx.Environment.Start();
                    ctx.Environment.AcqContext.Start();
                    return Observable.Return(ctx.Environment.AcqContext)
                                .Concat(Observable.Never(ctx.Environment.AcqContext));
                });
        }

        // Toggle Running
        public IObservable<oni.Context> Generate(IObservable<bool> source)
        {
            return source.SelectMany(x =>
            {
                if (x)
                {
                    return Observable.Using(
                        () => ONIManager.ReserveContext(Name, configuration),
                        ctx =>
                        {
                            ctx.Environment.Start();
                            ctx.Environment.AcqContext.Start();
                            return Observable.Return(ctx.Environment.AcqContext)
                                        .Concat(Observable.Never(ctx.Environment.AcqContext));
                        });
                }
                else
                {
                    return Observable.Using(
                        () => ONIManager.ReserveContext(Name, configuration),
                        ctx =>
                        {
                            ctx.Environment.Start();
                            ctx.Environment.AcqContext.Stop();
                            return Observable.Return(ctx.Environment.AcqContext)
                                        .Concat(Observable.Never(ctx.Environment.AcqContext));
                        });
                }
            });
        }
    }
}
