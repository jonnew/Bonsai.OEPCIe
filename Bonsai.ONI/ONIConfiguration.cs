using System.ComponentModel;

namespace Bonsai.ONI
{
    public abstract class ONIConfiguration
    {
        //internal static readonly ONIConfiguration Default = new ONIConfiguration();

        public ONIConfiguration()
        {
            ContextName  = "Context 0";
            BlockReadSize = 1024;
        }

        public abstract object[] DriverArgs();

        //[TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The ONI context identifer.")]
        public string ContextName { get; set; }

        [Description("The driver used by this context.")]
        public string DriverName { get; set; }

        [Description("The host index used by this context.")]
        public int HostIndex { get; set; } = 0;

        [Description("The ONI context data input block readsize. Smaller is lower latency. Larger is higher bandwidth.")]
        public int BlockReadSize { get; set; }
    }
}
