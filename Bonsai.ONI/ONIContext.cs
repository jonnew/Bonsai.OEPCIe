using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        //public oni.Context AcqContext { get; private set; }
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
                disposable.Dispose();
            }
        }

    }
}
