using System.ComponentModel;

namespace Bonsai.OEPCIe
{
    // This class cannot have a constructor with parameters because we want to automatically serialize to XML
    public class DeviceIndexSelection
    {
        [Description("The selected device index.")]
        [Range(0, int.MaxValue)]
        public int SelectedIndex { get; set; } = -1;

        private int[] indicies;
        [Description("The valid device indices.")]
        public int[] Indices {
            get { return indicies; }
            set {
                indicies = value;
                if (SelectedIndex == -1)
                {
                    SelectedIndex = value[0];
                }
            }
        }

        public override string ToString()
        {
            return SelectedIndex.ToString();
        }

        public void StringToSelection(string str_idx)
        {
            if (str_idx != null)
            {
                SelectedIndex = int.Parse(str_idx);
            }
        }
    }
}
