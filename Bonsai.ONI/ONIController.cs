using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.ONI
{
    public class ONIController //: IDisposable
    {
        //bool disposed;
        public oni.Context AcqContext { get; private set; }
        private Task CollectFrames;
        private CancellationTokenSource TokenSource;
        private CancellationToken CollectFramesToken;

        public event EventHandler<FrameReceivedEventArgs> FrameInputReceived;
        //public event EventHandler<DeviceMapUpdatedEventArgs> ContextUpdated;

        //public ONIController()
        //{
        //    AcqContext = new oni.Context();

        //    // Set block read size
        //    // TODO: this should be a context option along with the paths
        //    AcqContext.SetBlockReadSize(8192);
        //}

        public ONIController(string driver_name, int host_idx, params object[] args)
        {
            AcqContext = new oni.Context(driver_name, host_idx, args); //config_path, read_path, write_path, signal_path);
        }

        public bool Running()
        {
            return CollectFrames.Status == TaskStatus.Running;
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
                AcqContext.Stop(); // Pause the  the hardware
            }
        }

        //public void Close()
        //{
        //    Stop();
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //~ONIController()
        //{
        //    Dispose(false);
        //}

        //private void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            AcqContext.Destroy();
        //            disposed = true;
        //        }
        //    }
        //}

        //void IDisposable.Dispose()
        //{
        //    Close();
        //}

        void OnFrameReceived(FrameReceivedEventArgs e)
        {
            FrameInputReceived?.Invoke(this, e);
        }
    }
}
