using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Bonsai.OEPCIe
{
    public class OEPCIeConfiguration
    {
        public OEPCIeConfiguration()
        {
            ContextIndex = oe.Context.DefaultIndex;
        }

        //[TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The OEPCIe context index.")]
        public uint ContextIndex { get; set; }

        [Description("The OEPCIe context data input channel path.")]
        public string DataInputPath { get; set; }

        [Description("The OEPCIe context configuration channel read path.")]
        public string ConfigurationPath { get; set; }

        [Description("The OEPCIe context signal path.")]
        public string SignalPath { get; set; }
    }
}
