using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.OEPCIe
{
    [XmlRoot("OEPCIeConfigurationSettings")]
    public class OEPCIeConfigurationCollection : KeyedCollection<uint, OEPCIeConfiguration>
    {
        protected override uint GetKeyForItem(OEPCIeConfiguration item)
        {
            return item.ContextIndex;
        }
    }
}