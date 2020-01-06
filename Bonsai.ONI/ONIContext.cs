using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.ONI
{
    public class ONIContext: ICancelable, IDisposable
    {
        IDisposable resource;

        public ONIContext(ONIController oni, IDisposable disposable)
        {
            if (oni == null)
            {
                throw new ArgumentNullException("oni");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            Environment = oni;
            resource = disposable;
        }

        public ONIController Environment { get; private set; }

        public bool IsDisposed
        {
            get { return resource == null; }
        }

        public void Dispose()
        {
            var disposable = Interlocked.Exchange(ref resource, null);
            if (disposable != null)
            {
                Environment.AcqContext.Destroy();
                disposable.Dispose();
            }
        }

    }
}
