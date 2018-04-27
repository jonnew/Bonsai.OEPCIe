using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Bonsai.OEPCIe
{
    using oe.lib;

    public class DeviceIndexSelection
    {
       
        public Dictionary<int, oepcie.device_t> DeviceMap { get; }

        public DeviceIndexSelection(Dictionary<int, oepcie.device_t> dev_map)
        {
            DeviceMap = dev_map;
            SelectedIndex = DeviceMap.Keys.First();
        }

        [Description("The selected device index.")]
        [Range(0, int.MaxValue)]
        public int SelectedIndex { get; set; } = 0;
    }
}
