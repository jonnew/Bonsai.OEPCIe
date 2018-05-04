using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Bonsai.OEPCIe
{
    using oe.lib;
    using System;

    public class DeviceIndexSelection
    {
        //public Dictionary<int, oepcie.device_t> DeviceMap { get; }
        public List<int> Indices;

        public DeviceIndexSelection(Dictionary<int, oepcie.device_t> dev_map)
        {
            Indices = new List<int>();
            foreach (var key in dev_map.Keys)
            {
                Indices.Add(key);
            }
            SelectedIndex = dev_map.Keys.First();
        }

        [Description("The selected device index.")]
        [Range(0, int.MaxValue)]
        public int SelectedIndex { get; set; } = 0;

        public override string ToString()
        {
            return SelectedIndex.ToString();
        }

        public void StringToSelection(string str_idx)
        {
            SelectedIndex = int.Parse(str_idx);
        } 
    }
}
