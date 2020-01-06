using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    [Description("Controls a single headborne microstimulator circuit.")]
    [Combinator(MethodName = "Process")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class ElectricalStimulator : ONIDevice
    {
        // Control registers (see oedevices.h)
        public enum Register
        {
            NULLPARM = 0,  // No command
            BIPHASIC = 1,  // Biphasic pulse (0 = monophasic, 1 = biphasic; NB: currently ignored)
            CURRENT1 = 2,  // Phase 1 current, (0 to 255 = -1.5 mA to +1.5mA)
            CURRENT2 = 3,  // Phase 2 voltage, (0 to 255 = -1.5 mA to +1.5mA)
            PULSEDUR1 = 4,  // Phase 1 duration, 10 microsecond steps
            IPI = 5,  // Inter-phase interval, 10 microsecond steps
            PULSEDUR2 = 6,  // Phase 2 duration, 10 microsecond steps
            PULSEPERIOD = 7,  // Inter-pulse interval, 10 microsecond steps
            BURSTCOUNT = 8,  // Burst duration, number of pulses in burst
            IBI = 9,  // Inter-burst interval, microseconds
            TRAINCOUNT = 10, // Pulse train duration, number of bursts in train
            TRAINDELAY = 11, // Pulse train delay, microseconds
            TRIGGER = 12, // Trigger stimulation (1 = deliver)
            POWERON = 13, // Control estim sub-circuit power (0 = off, 1 = on)
            ENABLE = 14, // Control null switch (0 = stim output shorted to ground, 1 = stim output attached to electrode during pulses)
            RESTCURR = 15, // Resting current between pulse phases, (0 to 255 = -1.5 mA to +1.5mA)
            RESET = 16, // Reset all parameters to default
        }

        // Dictionary containing registers and values that need to be updated when possible
        Dictionary<Register, uint> RegistersToUpdate;

        public  IObservable<Tuple<ONIContext, bool>> Process(IObservable<Tuple<ONIContext, bool>> source)
        {
            return source.Do(input =>
            {
                // Get device inidicies
                var devices = ObservableFrame.FindMachingDevices(input.Item1, ID);
                if (devices.Count == 0) throw new oni.ONIException(oni.lib.Error.DEVID);
                DeviceIndex.Indices = devices.Keys.ToArray();

                // If there are changed registers, update
                foreach (var x in RegistersToUpdate)
                    input.Item1.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (uint)x.Key, x.Value);

                // We have updated registers
                RegistersToUpdate.Clear();

                // Stim if requested
                if (input.Item2)
                    input.Item1.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.TRIGGER, 0x01);
            });
        }

        // Setup context etc
        public ElectricalStimulator() : base(oni.Device.DeviceID.ESTIM)
        {
            RegistersToUpdate = new Dictionary<Register, uint>(16);
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

        private uint currentK(double currentuA)
        {
            const double k = 1500 / 127;
            return (uint)((currentuA + 1500) / k);
        }

        private double pulse_phase_one_current_uA;
        [Description("Phase 1 pulse current (-1500 to +1500 uA).")]
        [Range(-1500, 1500)]
        public double Phase1CurrentuA
        {
            get { return pulse_phase_one_current_uA; }
            set
            {
                pulse_phase_one_current_uA = value;
                if (RegistersToUpdate.ContainsKey(Register.CURRENT1))
                    RegistersToUpdate[Register.CURRENT1] = currentK(value);
                else
                    RegistersToUpdate.Add(Register.CURRENT1, currentK(value));
            }
        }

        private double pulse_phase_two_current_uA;
        [Description("Phase 2 pulse current (-1500 to +1500 uA).")]
        [Range(-1500, 1500)]
        public double Phase2CurrentuA
        {
            get { return pulse_phase_two_current_uA; }
            set
            {
                pulse_phase_two_current_uA = value;
                if (RegistersToUpdate.ContainsKey(Register.CURRENT2))
                    RegistersToUpdate[Register.CURRENT2] = currentK(value);
                else
                    RegistersToUpdate.Add(Register.CURRENT2, currentK(value));
            }
        }

        private double resting_current_uA;
        [Description("Resting current between pulse phases (-1500 to +1500 uA).")]
        public double RestingCurrentuA
        {
            get { return resting_current_uA; }
            set
            {
                resting_current_uA = value;
                if (RegistersToUpdate.ContainsKey(Register.RESTCURR))
                    RegistersToUpdate[Register.RESTCURR] = currentK(value);
                else
                    RegistersToUpdate.Add(Register.RESTCURR, currentK(value));
            }
        }

        private uint pulse_phase_one_duration_usec;
        [Description("Phase 1 pulse duration (usec).")]
        [Range(0, int.MaxValue)]
        public uint PulsePhase1DurationuSec
        {
            get { return pulse_phase_one_duration_usec; }
            set
            {
                pulse_phase_one_duration_usec = value;
                if (RegistersToUpdate.ContainsKey(Register.PULSEDUR1))
                    RegistersToUpdate[Register.PULSEDUR1] = value;
                else
                    RegistersToUpdate.Add(Register.PULSEDUR1, value);
            }
        }

        private uint ipi;
        [Description("Interpulse interval (usec).")]
        [Range(0, int.MaxValue)]
        public uint InterPulseIntervaluSec
        {
            get { return ipi; }
            set
            {
                ipi = value;
                if (RegistersToUpdate.ContainsKey(Register.IPI))
                    RegistersToUpdate[Register.IPI] = value;
                else
                    RegistersToUpdate.Add(Register.IPI, value);
            }
        }

        private uint pulse_phase_two_duration_usec;
        [Description("Phase 2 pulse duration (usec).")]
        [Range(0, int.MaxValue)]
        public uint PulsePhase2DurationuSec
        {
            get { return pulse_phase_two_duration_usec; }
            set
            {
                pulse_phase_two_duration_usec = value;
                if (RegistersToUpdate.ContainsKey(Register.PULSEDUR2))
                    RegistersToUpdate[Register.PULSEDUR2] = value;
                else
                    RegistersToUpdate.Add(Register.PULSEDUR2, value);
            }
        }

        private uint pulse_period_usec;
        [Description("Pulse period (usec).")]
        [Range(0, int.MaxValue)]
        public uint PulsePerioduSec
        {
            get { return pulse_period_usec; }
            set
            {
                pulse_period_usec = value;
                if (RegistersToUpdate.ContainsKey(Register.PULSEPERIOD))
                    RegistersToUpdate[Register.PULSEPERIOD] = value;
                else
                    RegistersToUpdate.Add(Register.PULSEPERIOD, value);
            }
        }

        private uint burst_pulse_count;
        [Description("Number of pulses to deiver in a burst.")]
        [Range(0, int.MaxValue)]
        public uint BurstPulseCount
        {
            get { return burst_pulse_count; }
            set
            {
                burst_pulse_count = value;
                if (RegistersToUpdate.ContainsKey(Register.BURSTCOUNT))
                    RegistersToUpdate[Register.BURSTCOUNT] = value;
                else
                    RegistersToUpdate.Add(Register.BURSTCOUNT, value);
            }
        }

        private uint inter_burst_interval_usec;
        [Description("Interburst interval (usec).")]
        [Range(0, int.MaxValue)]
        public uint InterBurstIntervaluSec
        {
            get { return inter_burst_interval_usec; }
            set
            {
                inter_burst_interval_usec = value;
                if (RegistersToUpdate.ContainsKey(Register.IBI))
                    RegistersToUpdate[Register.IBI] = value;
                else
                    RegistersToUpdate.Add(Register.IBI, value);
            }
        }

        private uint train_burst_count;
        [Description("Number of bursts to deliver in a train.")]
        [Range(0, int.MaxValue)]
        public uint TrainBurstCount
        {
            get { return train_burst_count; }
            set
            {
                train_burst_count = value;
                if (RegistersToUpdate.ContainsKey(Register.TRAINCOUNT))
                    RegistersToUpdate[Register.TRAINCOUNT] = value;
                else
                    RegistersToUpdate.Add(Register.TRAINCOUNT, value);
            }
        }

        private uint traindelay;
        [Description("Delay between issue of trigger and start of train (usec).")]
        [Range(0, int.MaxValue)]
        public uint TrainDelayuSec
        {
            get { return traindelay; }
            set
            {
                traindelay = value;
                if (RegistersToUpdate.ContainsKey(Register.TRAINDELAY))
                    RegistersToUpdate[Register.TRAINDELAY] = value;
                else
                    RegistersToUpdate.Add(Register.TRAINDELAY, value);
            }
        }

        private bool poweron = false;
        [Description("Stimulation sub-circuit power (True = On, False = Off).")]
        public bool PowerOn
        {
            get { return poweron; }
            set
            {
                poweron = value;
                uint code = value ? (uint)0x01 : (uint)0x00;
                if (RegistersToUpdate.ContainsKey(Register.POWERON))
                    RegistersToUpdate[Register.POWERON] = code;
                else
                    RegistersToUpdate.Add(Register.POWERON, code);
            }
        }

        private bool enable = false;
        [Description("Stimulation enable (True = enabled, False = disabled).")]
        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;
                uint code = value ? (uint)0x01 : (uint)0x00;
                if (RegistersToUpdate.ContainsKey(Register.POWERON))
                    RegistersToUpdate[Register.POWERON] = code;
                else
                    RegistersToUpdate.Add(Register.POWERON, code);
            }
        }
    }
}