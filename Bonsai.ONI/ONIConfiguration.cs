using System.ComponentModel;

namespace Bonsai.ONI
{
    public class ONIConfiguration
    {
        internal static readonly ONIConfiguration Default = new ONIConfiguration();

        public ONIConfiguration()
        {
            ContextName  = oni.Context.DefaultName;
            ConfigurationPath = "\\\\.\\xillybus_cmd_32";
            SignalPath = "\\\\.\\xillybus_signal_8";
            DataInputPath = "\\\\.\\xillybus_data_read_32";
            DataOutputPath = "\\\\.\\xillybus_data_write_32";
            BlockReadSize = 2048;
        }

        //[TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The ONI context identifer.")]
        public string ContextName { get; set; }

        [Description("The ONI context data input channel path.")]
        public string DataInputPath { get; set; }

        [Description("The ONI context data output channel path.")]
        public string DataOutputPath { get; set; }

        [Description("The ONI context configuration channel read path.")]
        public string ConfigurationPath { get; set; }

        [Description("The ONI context signal path.")]
        public string SignalPath { get; set; }

        [Description("The ONI context data input block readsize. Smaller is lower latency. Larger is higher bandwidth.")]
        public int BlockReadSize { get; set; }
    }
}
