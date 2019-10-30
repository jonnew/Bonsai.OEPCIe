using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Bonsai.ONI
{
    public static class ONIManager
    {
        public const string DefaultConfigurationFile = "ONI.config";
        static readonly Dictionary<uint, Tuple<ONIController, RefCountDisposable>> openContexts 
            = new Dictionary<uint, Tuple<ONIController, RefCountDisposable>>();
        static readonly object openContextsLock = new object();

        public static ONIDisposable ReserveDAQ(uint context_index = 0)
        {
            Tuple<ONIController, RefCountDisposable> oni_context;

            lock (openContextsLock)
            {
                if (!openContexts.TryGetValue(context_index, out oni_context)) // Context has not been opened yet
                {
                    ONIController oni; // Does not call constructor 
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(context_index)) // There is a configuration file specifying non-default channel paths
                    {
                        var config = configuration[context_index]; // Nothing used yet
                        oni = new ONIController(config.ConfigurationPath, config.DataInputPath, config.SignalPath); 
                    }
                    else
                    {
                        oni = new ONIController(); // Default params
                    }

                    var dispose = Disposable.Create(() =>
                    {
                        //oni.DAQ.Dispose(false);
                        openContexts.Remove(context_index);
                    });

                    var ref_count = new RefCountDisposable(dispose);
                    oni_context = Tuple.Create(oni, ref_count);
                    openContexts.Add(context_index, oni_context);
                    return new ONIDisposable(oni, ref_count);
                }
            }

            return new ONIDisposable(oni_context.Item1, oni_context.Item2.GetDisposable()); // New reference
        }

        public static ONIConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new ONIConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(ONIConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (ONIConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(ONIConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(ONIConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }

    }
}