﻿using System.Collections.Generic;
using System.Linq;

namespace Bonsai.ONI
{
    public static class ExtensionMethods
    {
        public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }
    }
}
