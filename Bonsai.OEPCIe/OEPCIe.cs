using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

// TODO: background task to collect frames along with start and stop methods
namespace Bonsai.OEPCIe
{
    public class OEPCIe
    {
        private bool running = false;
        //public bool Running { get { return running; } private set { } }  // Atomic by nature in C#
        public oe.Context DAQ { get; private set; }
        private Task CollectFrames;
        // TODO: confirm that oe.Frame is a reference type, so it will not be copied for each queue
        private List<BlockingCollection<oe.Frame>> frame_queues = new List<BlockingCollection<oe.Frame>>();

        public OEPCIe()
        {
            DAQ = new oe.Context();
        }

        public OEPCIe(string config_path,
                       string read_path,
                       string signal_path)
        {
            DAQ = new oe.Context(config_path, read_path, signal_path);
        }

        public void Start()
        {
            if (!running)
            {
                DAQ.Reset();
                DAQ.Start();
                running = true;
                CollectFrames = Task.Factory.StartNew(() => 
                {
                    while (running)
                    {
                        var frame = DAQ.ReadFrame();

                        foreach (var q in frame_queues) {
                            q.Add(frame);
                        }
                    }
                },
                TaskCreationOptions.LongRunning);
            }
        }

        public void Stop()
        {
            if (running)
            {
                running = false; // Knock all threads out of their collection loop
                Task.WaitAll(CollectFrames); // Wait for theads to exit
                DAQ.Stop(); // Stop the hardware
            }
        }

        public BlockingCollection<oe.Frame> Subscribe()
        {
            frame_queues.Add(new BlockingCollection<oe.Frame>());
            return frame_queues.Last();
        }

        public void Unsubscribe(BlockingCollection<oe.Frame> queue)
        {
            frame_queues.Remove(queue);
        }
    }
}
