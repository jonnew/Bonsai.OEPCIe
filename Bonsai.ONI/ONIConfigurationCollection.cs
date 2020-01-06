using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.ONI
{
    [XmlRoot("ONIConfigurationSettings")]
    public class ONIConfigurationCollection : KeyedCollection<string, ONIConfiguration>
    {
        protected override string GetKeyForItem(ONIConfiguration item)
        {
            return item.ContextName;
        }
    }
}