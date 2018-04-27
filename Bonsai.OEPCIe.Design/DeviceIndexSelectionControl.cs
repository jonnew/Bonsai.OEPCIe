using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Drawing.Design;

namespace Bonsai.OEPCIe.Design
{
    public partial class DeviceIndexSelectionControl : ConfigurationDropDown
    {
        protected override IEnumerable<string> GetConfigurationNames()
        {
            //return SerialPort.GetPortNames();
            return new string[2];
        }

        protected override object LoadConfiguration()
        {
            //return ArduinoManager.LoadConfiguration();
            return null;
        }

        protected override void SaveConfiguration(object configuration)
        {
            //var arduinoConfiguration = configuration as ArduinoConfigurationCollection;
            //if (arduinoConfiguration == null)
            //{
            //    throw new ArgumentNullException("configuration");
            //}

            //ArduinoManager.SaveConfiguration(arduinoConfiguration);
        }

        protected override UITypeEditor CreateConfigurationEditor(Type type)
        {
            return new DeviceIndexSelectionCollectionEditor(type);
        }

        class DeviceIndexSelectionCollectionEditor : DescriptiveCollectionEditor
        {
            public DeviceIndexSelectionCollectionEditor(Type type)
                : base(type)
            {
            }

            protected override string GetDisplayText(object value)
            {
                var configuration = value as DeviceIndexSelection;
                if (configuration != null)
                {
                    return configuration.SelectedIndex.ToString();
                }

                return base.GetDisplayText(value);
            }
        }
    }
}
