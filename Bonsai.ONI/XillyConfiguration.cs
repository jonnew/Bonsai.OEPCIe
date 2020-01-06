using System.ComponentModel;

namespace Bonsai.ONI
{
    public class XillyConfiguration : ONIConfiguration
    {

       public  XillyConfiguration()
            : base()
        {
            DriverName = "xillybus";
            ConfigurationPath = "\\\\.\\xillybus_cmd_32";
            SignalPath = "\\\\.\\xillybus_signal_8";
            DataInputPath = "\\\\.\\xillybus_data_read_32";
            DataOutputPath = "\\\\.\\xillybus_data_write_32";
        }

        public override object[] DriverArgs()
        {
            var arg_list = new object[8];

            arg_list[0] = 0;
            arg_list[2] = 1;
            arg_list[4] = 2;
            arg_list[6] = 3;

            arg_list[1] = ConfigurationPath;
            arg_list[3] = SignalPath;
            arg_list[5] = DataInputPath;
            arg_list[7] = DataOutputPath;

            return arg_list;
        }

        [Description("The ONI context data input channel path.")]
        public string DataInputPath { get; set; }

        [Description("The ONI context data output channel path.")]
        public string DataOutputPath { get; set; }

        [Description("The ONI context configuration channel read path.")]
        public string ConfigurationPath { get; set; }

        [Description("The ONI context signal path.")]
        public string SignalPath { get; set; }

    }
}
