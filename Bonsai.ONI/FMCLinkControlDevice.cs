using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.ONI
{
    [Description("Controls the power state of the Open Ephys FMC Host version 1.3. THIS NODE CAN DAMAGE YOUR HARDWARE: BE CAREFUL!")]
    public class FMCLinkControlDevice : ONIContextSinkDevice
    {
        const double VLIM = 6.1;

        // Control registers (see oedevices.h)
        public enum Register
        {
            LINKAVOLTAGE = 0,
            LINKBVOLTAGE = 1,
            SAVELINKA = 2,
            SAVELINKB = 3,
            DESAPND = 4,
            DESBPND = 5,
        }

        public FMCLinkControlDevice() : base(oni.Device.DeviceID.FMCVCTRL) { }

        [Description("Link A main power enabled or disabled.")]
        public bool LinkAPowerEnabled { get; set; } = true;

        [Description("Link B main power enabled or disabled.")]
        public bool LinkBPowerEnabled { get; set; } = true;

        [Description("Save link A voltage to internal EEPROM.")]
        public bool SaveLinkAVoltage { get; set; } = false;

        [Description("Save link B voltage to internal EEPROM.")]
        public bool SaveLinkBVoltage { get; set; } = false;

        [Description("Deserializer A power enabled or disabled.")]
        public bool DesAPowerEnabled { get; set; } = true;

        [Description("Deserializer B power enabled or disabled.")]
        public bool DesBPowerEnabled { get; set; } = true;

        bool extended_v_enabled = false;
        string extended_v_enabled_str = "";
        [Description("Type \"BE CAREFUL\" here to enable the extended link voltage range.")]
        public string EnableExtendedVoltageRange {
            get { return extended_v_enabled_str; }
            set {
                extended_v_enabled_str = value;
                if (extended_v_enabled_str == "BE CAREFUL")
                    extended_v_enabled = true;
                else
                    extended_v_enabled = false;
            }
        }

        double link_a_v = 5.0;
        [Range(3.3, 11.0)]
        [Precision(1, 0.1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Link A voltage.")]
        public double LinkAVoltage {
            get { return link_a_v; }
            set {
                link_a_v = !extended_v_enabled & value > VLIM ? VLIM : value;
            }
        }

        double link_b_v = 5.0;
        [Range(3.3, 11.0)]
        [Precision(1, 0.1)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("Link B voltage.")]
        public double LinkBVoltage {
            get { return link_b_v; }
            set {
                link_b_v = !extended_v_enabled & value > VLIM? VLIM : value;
            }
        }

        internal override void SideEffect(ONIContext ctx)
        {
            // TODO: inefficient because only one of these changes at time
            double av = LinkAPowerEnabled ? LinkAVoltage : 0.0;
            double bv = LinkBPowerEnabled ? LinkBVoltage : 0.0;

            // Hardware side effects
            ctx.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKAVOLTAGE, (uint)(av * 10));
            ctx.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.LINKBVOLTAGE, (uint)(bv * 10));

            if (SaveLinkAVoltage) ctx.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.SAVELINKA, 0);
            if (SaveLinkBVoltage) ctx.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.SAVELINKB, 0);

            ctx.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.DESAPND, DesAPowerEnabled ? (uint)1 : 0);
            ctx.Environment.AcqContext.WriteRegister((uint)DeviceIndex.SelectedIndex, (int)Register.DESBPND, DesBPowerEnabled ? (uint)1 : 0);
        }
    }
}
