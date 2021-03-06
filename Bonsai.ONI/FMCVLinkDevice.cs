﻿using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Controls the link voltage on Open Ephys fmc-host revision 1.3.")]
    public class FMCVLinkDevice
    {
        // Control registers (see oedevices.h)
        public enum Register
        {
            LINKAVOLTAGE = 0,
            LINKBVOLTAGE = 1,
            SAVELINKA = 2,
            SAVELINKB = 3
        }

        private ONIDisposable oni_ref; // Reference to global oni configuration set
        private Dictionary<int, oni.lib.device_t> devices;

        // Controls both together
        public IObservable<bool> Process(IObservable<bool> source)
        {
            return source.Do(
                input =>
                {
                    LinkAEnabled = input;
                    LinkBEnabled = input;
                });
        }

        // Controls A and B separately
        public IObservable<Tuple<bool, bool>> Process(IObservable<Tuple<bool, bool>> source)
        {
            return source.Do(
                input =>
                {
                    if (input.Item1)
                        LinkAVoltage = link_a_voltage;
                    else
                        oni_ref.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKAVOLTAGE, 0);

                    if (input.Item2)
                        LinkAVoltage = link_b_voltage;
                    else
                        oni_ref.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKBVOLTAGE, 0);
                });
        }

        public FMCVLinkDevice()
        {
            // Reference to context
            this.oni_ref = ONIManager.ReserveDAQ(); // TODO: Somehow get the context index from the configuration file

            // Find all estim devices
            devices = oni_ref.DAQ.DeviceMap.Where(pair => pair.Value.id == (uint)oni.Device.DeviceID.FMCVCTRL).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no estim devices to use
            if (devices.Count == 0)
                throw new oni.ONIException((int)oni.lib.Error.DEVIDX);

            // Set device selection
            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

        }

        [Editor("Bonsai.ONI.Design.DeviceIndexCollectionEditor, Bonsai.ONI.Design", typeof(UITypeEditor))]
        [Description("The FMC link voltage controller handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

        private bool allow_extended_voltage = false;
        private double v_high = 6.5;
        [Description("Permit voltages up to 11V. WARNING: overvoltage can damage your headstage!")]
        public bool AllowExtendedVoltageRange
        {
            get { return allow_extended_voltage; }
            set
            {
                if (value)
                    v_high = 11.0;
                else
                    v_high = 6.5;

                allow_extended_voltage = value;
            }
        }

        bool link_a_enabled = true;
        [Description("Enable Link A")]
        //[Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public bool LinkAEnabled {
            get
            {
                return link_a_enabled;
            }

            set
            {
                link_a_enabled = value;
                LinkAVoltage = link_a_voltage;
            }
        }

        bool link_b_enabled = true;
        [Description("Enable Link B")]
        //[Editor(Chec.NumericUpDownEditor, typeof(UITypeEditor))]
        public bool LinkBEnabled {
            get
            {
                return link_b_enabled;
            }

            set
            {
                link_b_enabled = value;
                LinkBVoltage = link_b_voltage;
            }
        }

        private double link_a_voltage = 3.3;
        [Description("Link A voltage (3.3V to 11.0V.")]
        [Range(3.3, 11.0)]
        [Precision(1, 0.1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double LinkAVoltage
        {
            get { return link_a_voltage; }
            set
            {
                if (link_a_enabled)
                {
                    var v_req = value > v_high ? v_high : value;
                    oni_ref.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKAVOLTAGE, (uint)(v_req * 10));
                    link_a_voltage = v_req;
                } else {
                    oni_ref.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKAVOLTAGE, 0);
                }

            }
        }

        private double link_b_voltage = 3.3;
        [Description("Link B voltage (3.3V to 11.0V.")]
        [Range(3.3, 11.0)]
        [Precision(1, 0.1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double LinkBVoltage
        {
            get { return link_b_voltage; }
            set
            {
                if (link_b_enabled)
                {
                    var v_req = value > v_high ? v_high : value;
                    oni_ref.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKBVOLTAGE, (uint)(v_req * 10));
                    link_b_voltage = v_req;
                } else {
                    oni_ref.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKBVOLTAGE, 0);
                }
            }
        }
    }
}