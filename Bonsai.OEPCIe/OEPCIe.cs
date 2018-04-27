using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Bonsai.OEPCIe
{
    public class OEPCIe
    {
        public oe.Context DAQ { get; private set; }
        private Task CollectFrames;
        private CancellationTokenSource TokenSource;
        private CancellationToken CollectFramesToken;
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
            if (CollectFrames == null || CollectFrames.Status == TaskStatus.RanToCompletion)
            {
                DAQ.Reset();
                DAQ.Start();
                TokenSource = new CancellationTokenSource();
                CollectFramesToken = TokenSource.Token;

                //running = true;
                CollectFrames = Task.Factory.StartNew(() =>
                {
                    while (!CollectFramesToken.IsCancellationRequested)
                    {
                        var frame = DAQ.ReadFrame();

                        foreach (var q in frame_queues)
                        {
                            q.Add(frame);
                        }
                    }
                },
                CollectFramesToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            }
        }

        public void Stop()
        {
            if (CollectFrames != null && !CollectFrames.IsCanceled)
            {
                TokenSource.Cancel();
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
