using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.OEPCIe
{
    using oe;

    [Description("Controls a single headborne microstimulator circuit.")]
    public class ElectricalStimulator : Sink<bool>
    {
        private OEPCIeDisposable oepcie; // Reference to global oepcie configuration set
        private Dictionary<int, oe.lib.device_t> devices;

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return source.Do(
                input =>
                {
                    if (input)
                        oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.TRIGGER, 0x01);
                });
        }

        // Setup context etc
        public ElectricalStimulator() //oe.Context context, uint (uint)DeviceIndex.SelectedIndex)
        {
            // Reference to context
            this.oepcie = OEPCIeManager.ReserveDAQ(); // TODO: Somehow get the context index from the configuration file

            // Find all estim devices
            devices = oepcie.DAQ.DeviceMap.Where(pair => pair.Value.id == (uint)Device.DeviceID.ESTIM).ToDictionary(x => x.Key, x => x.Value);

            // Stop here if there are no estim devices to use
            if (devices.Count == 0)
                throw new oe.OEException((int)oe.lib.Error.DEVIDX);

            // Set device selection
            DeviceIndex = new DeviceIndexSelection();
            DeviceIndex.Indices = devices.Keys.ToArray();

            // Default configuration
            Reset();
        }

        void Reset()
        {
            ResetStimulatorStateMachine();
            Phase1CurrentuA = 500;
            Phase2CurrentuA = -500;
            PulsePhase1DurationuSec = 10;
            InterPulseIntervaluSec = 0;
            PulsePhase2DurationuSec = 10;
            PulsePerioduSec = 1000;
            BurstPulseCount = 100;
            InterBurstIntervaluSec = 0;
            TrainBurstCount = 1;
            TrainDelayuSec = 0;
            PowerOn = true;
            Enable = true;
            RestingCurrentuA = 0;
        }

        void ResetStimulatorStateMachine()
        {
            // TODO: device index ignore currently
            oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.RESET, 0x01);
        }

        private uint currentK(double currentuA)
        {
            const double k = 1500 / 127;
            return (uint)((currentuA + 1500) / k);
        }

        // TODO: fix in firmware
        private uint duration(uint duration_10usecs)
        {
            return duration_10usecs / 10;
        }

        [Editor("Bonsai.OEPCIe.Design.DeviceIndexCollectionEditor, Bonsai.OEPCIe.Design", typeof(UITypeEditor))]
        [Description("The electrical stimulator device handled by this node.")]
        public DeviceIndexSelection DeviceIndex { get; set; }

        private double pulse_phase_one_current_uA;
        [Description("Phase 1 pulse current (-1500 to +1500 uA).")]
        [Range(-1500, 1500)]
        public double Phase1CurrentuA
        {
            get { return pulse_phase_one_current_uA; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.CURRENT1, currentK(value)); pulse_phase_one_current_uA = value; }
        }

        private double pulse_phase_two_current_uA;
        [Description("Phase 2 pulse current (-1500 to +1500 uA).")]
        [Range(-1500, 1500)]
        public double Phase2CurrentuA
        {
            get { return pulse_phase_two_current_uA; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.CURRENT2, currentK(value)); pulse_phase_two_current_uA = value; }
        }

        private double resting_current_uA;
        [Description("Resting current between pulse phases (-1500 to +1500 uA).")]
        public double RestingCurrentuA
        {
            get { return resting_current_uA; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.RESTCURR, currentK(value)); resting_current_uA = value; }
        }

        private uint pulse_phase_one_duration_usec;
        [Description("Phase 1 pulse duration (usec).")]
        [Range(0, int.MaxValue)]
        public uint PulsePhase1DurationuSec
        {
            get { return pulse_phase_one_duration_usec; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.PULSEDUR1, duration(value)); pulse_phase_one_duration_usec = value; }
        }

        private uint ipi;
        [Description("Interpulse interval (usec).")]
        [Range(0, int.MaxValue)]
        public uint InterPulseIntervaluSec
        {
            get { return ipi; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.IPI, duration(value)); ipi = value; }
        }

        private uint pulse_phase_two_duration_usec;
        [Description("Phase 2 pulse duration (usec).")]
        [Range(0, int.MaxValue)]
        public uint PulsePhase2DurationuSec
        {
            get { return pulse_phase_two_duration_usec; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.PULSEDUR2, duration(value)); pulse_phase_two_duration_usec = value; }
        }

        private uint pulse_period_usec;
        [Description("Pulse period (usec).")]
        [Range(0, int.MaxValue)]
        public uint PulsePerioduSec
        {
            get { return pulse_period_usec; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.PULSEPERIOD, duration(value)); pulse_period_usec = value; }
        }

        private uint burst_pulse_count;
        [Description("Number of pulses to deiver in a burst.")]
        [Range(0, int.MaxValue)]
        public uint BurstPulseCount
        {
            get { return burst_pulse_count; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.BURSTCOUNT, value); burst_pulse_count = value; }
        }

        private uint inter_burst_interval_usec;
        [Description("Interburst interval (usec).")]
        [Range(0, int.MaxValue)]
        public uint InterBurstIntervaluSec
        {
            get { return inter_burst_interval_usec; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.IBI, duration(value)); inter_burst_interval_usec = value; }
        }

        private uint train_burst_count;
        [Description("Number of bursts to deliver in a train.")]
        [Range(0, int.MaxValue)]
        public uint TrainBurstCount
        {
            get { return train_burst_count; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.TRAINCOUNT, value); train_burst_count = value; }
        }

        private uint traindelay;
        [Description("Delay between issue of trigger and start of train (usec).")]
        [Range(0, int.MaxValue)]
        public uint TrainDelayuSec
        {
            get { return traindelay; }
            set { oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.TRAINDELAY, duration(value)); traindelay = value; }
        }

        private bool poweron = false;
        [Description("Stimulation sub-circuit power (True = On, False = Off).")]
        public bool PowerOn
        {
            get { return poweron; }
            set
            {
                uint code = value ? (uint)0x01 : (uint)0x00;
                oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.POWERON, code); poweron = value;
            }
        }

        private bool enable = false;
        [Description("Stimulation enable (True = enabled, False = disabled).")]
        public bool Enable
        {
            get { return enable; }
            set
            {
                uint code = value ? (uint)0x01 : (uint)0x00;
                oepcie.DAQ.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Device.EstimRegister.ENABLE, code); enable = value;
            }
        }
    }
}