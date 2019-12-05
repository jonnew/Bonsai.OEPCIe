using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Bonsai.ONI
{
    public class ONIController : IDisposable
    {
        bool disposed;
        public oni.Context AcqContext { get; private set; }
        private Task CollectFrames;
        private CancellationTokenSource TokenSource;
        private CancellationToken CollectFramesToken;

        public event EventHandler<FrameReceivedEventArgs> FrameInputReceived;

        public ONIController()
        {
            AcqContext = new oni.Context();

            // Set block read size
            // TODO: this should be a context option along with the paths
            AcqContext.SetBlockReadSize(8192);
        }

        public ONIController(string config_path,
                      string read_path,
                      string write_path,
                      string signal_path,
                      int block_read_size)
        {
            AcqContext = new oni.Context(config_path, read_path, write_path, signal_path);

            // Set block read size
            // TODO: this should be a context option along with the paths
            AcqContext.SetBlockReadSize(block_read_size);
        }

        public void Start()
        {
            if (CollectFrames == null || CollectFrames.Status == TaskStatus.RanToCompletion)
            {
                AcqContext.Reset();
                AcqContext.Start();
                TokenSource = new CancellationTokenSource();
                CollectFramesToken = TokenSource.Token;

                CollectFrames = Task.Factory.StartNew(() =>
                {
                    while (!CollectFramesToken.IsCancellationRequested)
                    {
                        OnFrameReceived(new FrameReceivedEventArgs(AcqContext.ReadFrame()));
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
                AcqContext.Stop(); // Stop the hardware
                AcqContext.Reset();
            }
        }

        public void Close()
        {
            Stop();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ONIController()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    AcqContext.Destroy();
                    disposed = true;
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        void OnFrameReceived(FrameReceivedEventArgs e)
        {
            FrameInputReceived?.Invoke(this, e);
        }
    }
}
