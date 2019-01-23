using System;
using OpenCV.Net;

namespace Bonsai.OEPCIE.Prototyping
{
    public class PCEImage
    {
        public PCEImage(PCEDataBlock block)
        {
            Clock = block.Clock;
            Time = block.Time;

            unsafe {
                fixed (byte* p = block.ImageData) {
                    Image = new IplImage((new Size(block.Cols, block.Rows)), IplDepth.U8, 1, (IntPtr)p);
                }
            }
        }

        public ulong Clock { get; private set; }

        public double Time { get; private set; }

        public IplImage Image { get; private set; }

    }
}
