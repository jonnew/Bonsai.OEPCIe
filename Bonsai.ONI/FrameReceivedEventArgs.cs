using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.ONI
{
    public class FrameReceivedEventArgs : EventArgs
    {
        public FrameReceivedEventArgs(oni.Frame frame)
        {
            Value = frame;
        }

        public oni.Frame Value { get; private set; }
    }
}
