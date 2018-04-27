using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.OEPCIe
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DeviceIndexAttribute : Attribute
    {
        public static readonly DeviceIndexAttribute Default = null;

        public DeviceIndexAttribute(int[] device_indices)
        {
            Indices = device_indices;
        }

        public int[] Indices { get; private set; }
    }
}
