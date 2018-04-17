using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Threading;

namespace Bonsai.OEPCIe
{
    public sealed class OEPCIeDisposable: ICancelable, IDisposable
    {
        IDisposable resource;

        public OEPCIeDisposable(OEPCIe oepcie, IDisposable disposable)
        {
            if (oepcie == null)
            {
                throw new ArgumentNullException("oepcie");
            }

            if (disposable == null)
            {
                throw new ArgumentNullException("disposable");
            }

            Environment = oepcie;
            DAQ = Environment.DAQ; 
            resource = disposable;
        }

        public oe.Context DAQ { get; private set; }
        public OEPCIe Environment { get; private set; }

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
