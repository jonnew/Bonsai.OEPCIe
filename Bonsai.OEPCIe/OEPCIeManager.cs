using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
//using oe;

namespace Bonsai.OEPCIe
{
    public static class OEPCIeManager
    {
        public const string DefaultConfigurationFile = "OEPCIe.config";
        static readonly Dictionary<uint, Tuple<OEPCIe, RefCountDisposable>> openContexts 
            = new Dictionary<uint, Tuple<OEPCIe, RefCountDisposable>>();
        static readonly object openContextsLock = new object();

        public static OEPCIeDisposable ReserveDAQ(uint context_index = 0)
        {
            Tuple<OEPCIe, RefCountDisposable> oepcie_context;

            lock (openContextsLock)
            {
                if (!openContexts.TryGetValue(context_index, out oepcie_context)) // Context has not been opened yet
                {
                    OEPCIe oepcie; // Does not call constructor 
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(context_index)) // There is a configuration file specifying non-default channel paths
                    {
                        var config = configuration[context_index]; // Nothing used yet
                        oepcie = new OEPCIe(config.ConfigurationPath, config.DataInputPath, config.SignalPath); 
                    }
                    else
                    {
                        oepcie = new OEPCIe(); // Default params
                    }

                    var dispose = Disposable.Create(() =>
                    {
                        oepcie.DAQ.Dispose(false);
                        openContexts.Remove(context_index);
                    });

                    var ref_count = new RefCountDisposable(dispose);
                    oepcie_context = Tuple.Create(oepcie, ref_count);
                    openContexts.Add(context_index, oepcie_context);
                    return new OEPCIeDisposable(oepcie, ref_count);
                }
            }

            return new OEPCIeDisposable(oepcie_context.Item1, oepcie_context.Item2.GetDisposable()); // New reference
        }

        public static OEPCIeConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new OEPCIeConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(OEPCIeConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (OEPCIeConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(OEPCIeConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(OEPCIeConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }

    }
}