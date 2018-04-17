﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.OEPCIe
{
    public class FrameInputReceivedEventArgs : EventArgs
    {
        public FrameInputReceivedEventArgs(oe.Frame frame)
        {
            Value = frame;
        }

        public oe.Frame Value { get; private set; }
    }
}
