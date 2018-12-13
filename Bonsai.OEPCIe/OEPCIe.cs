using System;
using System.Collections.Generic;
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

        public event EventHandler<FrameReceivedEventArgs> FrameInputReceived;

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

                CollectFrames = Task.Factory.StartNew(() =>
                {
                    while (!CollectFramesToken.IsCancellationRequested)
                    {
                        OnFrameReceived(new FrameReceivedEventArgs(DAQ.ReadFrame()));
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
                DAQ.Reset();
            }
        }

        void OnFrameReceived(FrameReceivedEventArgs e)
        {
            FrameInputReceived?.Invoke(this, e);
        }
    }
}
