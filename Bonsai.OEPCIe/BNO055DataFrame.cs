﻿using System.Linq;
using OpenCV.Net;

namespace Bonsai.OEPCIe
{
    public class BNO055DataFrame
    {
        public BNO055DataFrame(oe.Frame frame, int device_index, int sample_clock_hz, int sys_clock_hz)
        {
            // NB: Data contents: [uint64_t remote_clock, uint16_t code]
            var sample = frame.Data<ushort>(device_index);

            FrameClock = frame.Clock();
            FrameTime = FrameClock / (double)sys_clock_hz;
            Clock = ((ulong)sample[0] << 48) | ((ulong)sample[1] << 32) | ((ulong)sample[2] << 16) | ((ulong)sample[3] << 0);
            Time = Clock / (double)sample_clock_hz;

            // Convert data packet (output format is hard coded right now)
            Euler = GetEuler(sample, 4);
            Quaternion = GetQuat(sample, 7);
            LinearAcceleration = GetAcceleration(sample, 11);
            GravityVector = GetAcceleration(sample, 14);
            Temperature = (byte)(sample[17] & 0x00FF); // 1°C = 1 LSB
            Calibration = (byte)((sample[17] & 0xFF00) >> 8);
        }

        public ulong FrameClock { get; private set; }

        public double FrameTime { get; private set; }

        public ulong Clock { get; private set; }

        public double Time { get; private set; }

        public byte Temperature { get; private set; }

        public byte Calibration { get; private set; }

        public Mat Quaternion { get; private set; }

        public Mat LinearAcceleration { get; private set; }

        public Mat GravityVector { get; private set; }

        public Mat Euler { get; private set; }

        

        Mat GetEuler(ushort[] sample, int begin)
        {
            // 1 degree = 16 LSB
            const double scale = 0.0625;
            var vec = new double[3];

            for (int i = 0; i < vec.Count(); i++)
                vec[i] = scale * (short)sample[i + begin];

            return Mat.FromArray(vec, vec.Length, 1, Depth.F64, 1);
        }

        Mat GetAcceleration(ushort[] sample, int begin)
        {
            // 1m/s^2 = 100 LSB
            const double scale = 0.01;
            var vec = new double[3];

            for (int i = 0; i < vec.Count(); i++)
                vec[i] = scale * (short)sample[i + begin];

            return Mat.FromArray(vec, vec.Length, 1, Depth.F64, 1);
        }

        Mat GetQuat(ushort[] sample, int begin)
        {
            // 1 quaterion (unitless) = 2^14 LSB
            const double scale = (1.0 / (1 << 14));
            var vec = new double[4];

            for (int i = 0; i < vec.Count(); i++) {
                var tmp = (short)sample[i + begin];
                vec[i] = scale * tmp;
             }

            return Mat.FromArray(vec, vec.Length, 1,  Depth.F64, 1);
        }

    }
}
