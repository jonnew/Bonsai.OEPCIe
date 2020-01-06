using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Xml.Serialization;
using System.Xml;
using System.IO;


namespace Bonsai.ONI
{
    public static class ONIManager
    {
        public const string DefaultConfigurationFile = "ONI.config";

        static readonly Dictionary<string, Tuple<ONIController, RefCountDisposable>> openContexts = new Dictionary<string, Tuple<ONIController, RefCountDisposable>>();
        static readonly object openContextsLock = new object();

        //public static DeviceMapT FindMachingDevices(DeviceMapT map, oni.Device.DeviceID dev_id)
        //{
        //    // Find all matching devices
        //    return map.Where(
        //        pair => pair.Value.id == (uint)dev_id
        //    ).ToDictionary(x => x.Key, x => x.Value);
        //}

        //public static ONIContext ReserveContext(string context_name)
        //{
        //    return ReserveContext(context_name, ONIConfiguration.Default);
        //}


        public static ONIContext ReserveContext(string context_name, ONIConfiguration config)
        {
            var oni_context = default(Tuple<ONIController, RefCountDisposable>);

            lock (openContextsLock)
            {
                if (string.IsNullOrEmpty(context_name))
                {
                    if (!string.IsNullOrEmpty(config.ContextName)) context_name = config.ContextName;
                    else if (openContexts.Count == 1) oni_context = openContexts.Values.Single();
                    else throw new ArgumentException("An context name must be specified.", "ContextName");
                }

                if (!openContexts.TryGetValue(context_name, out oni_context)) // Context has not been opened yet
                {
                    var final_context_name = config.ContextName;
                    if (string.IsNullOrEmpty(final_context_name)) final_context_name = context_name;

                    //ONIController oni; // Does not call constructor 
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(final_context_name)) // There is a configuration file specifying non-default channel paths
                    {
                        config = configuration[final_context_name]; // Nothing used yet
                    }

                    var ctx_controller = new ONIController(config.DriverName, config.HostIndex, config.DriverArgs());
                    ctx_controller.AcqContext.SetBlockReadSize(config.BlockReadSize);

                    var dispose = Disposable.Create(() =>
                    {
                        //ctx_controller.Close();
                        openContexts.Remove(context_name);
                    });

                    var ref_count = new RefCountDisposable(dispose);
                    oni_context = Tuple.Create(ctx_controller, ref_count);
                    openContexts.Add(context_name, oni_context);
                    return new ONIContext(ctx_controller, ref_count);
                }
            }

            return new ONIContext(oni_context.Item1, oni_context.Item2.GetDisposable()); // New reference
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