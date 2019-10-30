using System.ComponentModel;

namespace Bonsai.ONI
{
    public class ONIConfiguration
    {
        public ONIConfiguration()
        {
            ContextIndex = oni.Context.DefaultIndex;
        }

        //[TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The ONI context index.")]
        public uint ContextIndex { get; set; }

        [Description("The ONI context data input channel path.")]
        public string DataInputPath { get; set; }

        [Description("The ONI context data output channel path.")]
        public string DataOutputPath { get; set; }

        [Description("The ONI context configuration channel read path.")]
        public string ConfigurationPath { get; set; }

        [Description("The ONI context signal path.")]
        public string SignalPath { get; set; }

        [Description("The ONI context data input block readsize. Smaller is lower latency. Larger is higher bandwidth.")]
        public uint BlockReadSize { get; set; }
    }
}
