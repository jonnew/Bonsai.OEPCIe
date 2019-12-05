using System.ComponentModel;

namespace Bonsai.ONI
{
    // This class cannot have a constructor with parameters because we want to automatically serialize to XML
    public class DeviceIndexSelection
    {
        private bool needs_init = true;

        [Description("The selected device index.")]
        [Range(0, int.MaxValue)]
        public int SelectedIndex { get; set; } = 0;

        private int[] indicies;
        [Description("The valid device indices.")]
        public int[] Indices {
            get { return indicies; }
            set {
                indicies = value;
                if (needs_init)
                {
                    SelectedIndex = value[0];
                    needs_init = false;
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
