using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Drawing.Design;
using OpenCV.Net;
using System.IO;
using System.IO.Pipes;
using System.Text.RegularExpressions;

namespace Bonsai.ONI.Prototyping
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Estimates an orientation using at least 3 approximately co-planar 3D positions.")]
    public class PCECameraCalibrate
    {
        bool calibration_set;
        private byte[] offset_raw, slope_raw;

        Mat Calibrate(Tuple<Mat, Mat> source)
        {
            // Do we need to check array sizes or let CV take care of that?
            //if (source.Item1.Size != source.Item2.Size)

            if (!calibration_set)
            {
                // Read the calibration files
                offset_raw = ReadFully(CreateStream(OffsetPath));
                slope_raw = ReadFully(CreateStream(SlopePath));

                calibration_set = true;
            }

            // Use exposures to index into raw data
            var exposures = source.Item2;
            var offset_dat = new double[source.Item1.Rows * source.Item1.Cols];
            var slope_dat = new double[source.Item1.Rows * source.Item1.Cols];

            // Fast copy
            unsafe
            {
                fixed (byte* p_offset_raw = offset_raw)
                fixed (double* p_offset_dat = offset_dat)
                fixed (byte* p_slope_raw = slope_raw)
                fixed (double* p_slope_dat = slope_dat)
                {
                    double* p_offset_raw_d = (double*)p_offset_raw;
                    double* p_slope_raw_d = (double*)p_slope_raw;

                    var numel = exposures.Rows * exposures.Cols;

                    for (int i = 0; i < exposures.Rows; i++)
                    {
                        for (int j = 0; j < exposures.Cols; j++)
                        {
                            var mat_off = (int)exposures[i, j].Val0 * numel;
                            offset_dat[exposures.Cols * i + j] = p_offset_raw_d[mat_off + exposures.Cols * i + j];
                            slope_dat[exposures.Cols * i + j] = p_slope_raw_d[mat_off + exposures.Cols * i + j];
                        }
                    }
                }
            }

            // Generate Mats from data
            var offset = Mat.CreateMatHeader(offset_dat, source.Item1.Rows, source.Item1.Cols, Depth.F64, 1);
            var slope = Mat.CreateMatHeader(slope_dat, source.Item1.Rows, source.Item1.Cols, Depth.F64, 1);

            // Calibrate
            if (source.Item1.Depth != Depth.F64)
            {
                var tmp = new Mat(source.Item1.Size, Depth.F64, 1);
                CV.Convert(source.Item1, tmp);
                return tmp * (slope - offset);
            }
            else {
                return source.Item1 * (slope - offset);
            }
        }

        public  IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return source.Select(input => Calibrate(input))
                         .Finally(() => calibration_set = false);
        }

        const string PipePathPrefix = @"\\";
        static readonly Regex PipePathRegex = new Regex(@"\\\\([^\\]*)\\pipe\\(\w+)");

        /// <summary>
        /// Generate stream for each calibration file
        /// </summary>
        /// <param name="path">Path the calibration data</param>
        /// <returns>File stream</returns>
        static Stream CreateStream(string path)
        {
            if (path.StartsWith(PipePathPrefix))
            {
                var match = PipePathRegex.Match(path);
                if (match.Success)
                {
                    var serverName = match.Groups[1].Value;
                    var pipeName = match.Groups[2].Value;
                    var stream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.In);
                    try { stream.Connect(); }
                    catch { stream.Close(); throw; }
                    return stream;
                }
            }

            return File.OpenRead(path);
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An ONIxception is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        [Description("Path to offset calibration data. Flat binary, row major doubles. N X M X NumExposures, where N = num rows, M = num cols.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string OffsetPath { get; set; } = "";

        [Description("Path to slope calibration data. Flat binary, row major doubles. N X M X NumExposures, where N = num rows, M = num cols.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string SlopePath { get; set; } = "";

    }
}
