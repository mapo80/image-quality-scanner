using System;
using System.ComponentModel;

namespace DocQualityChecker
{
    /// <summary>
    /// Settings used to evaluate document image quality.
    /// </summary>
    public class QualitySettings
    {
        /// <summary>Maximum acceptable BRISQUE score.</summary>
        [DefaultValue(50.0)]
        public double BrisqueMax { get; set; } = 50.0;
        /// <summary>Threshold for blur detection using Laplacian variance.</summary>
        [DefaultValue(100.0)]
        public double BlurThreshold { get; set; } = 100.0;
        /// <summary>Brightness threshold for glare detection.</summary>
        [DefaultValue(240)]
        public int BrightThreshold { get; set; } = 240;
        /// <summary>Area threshold for glare detection.</summary>
        [DefaultValue(500)]
        public int AreaThreshold { get; set; } = 500;

        /// <summary>Minimum accepted average luminance.</summary>
        [DefaultValue(80.0)]
        public double ExposureMin { get; set; } = 80.0;

        /// <summary>Maximum accepted average luminance.</summary>
        [DefaultValue(180.0)]
        public double ExposureMax { get; set; } = 180.0;

        /// <summary>Minimum standard deviation of luminance for sufficient contrast.</summary>
        [DefaultValue(30.0)]
        public double ContrastMin { get; set; } = 30.0;

        /// <summary>Ratio of dominant RGB channel over the average to signal color cast.</summary>
        [DefaultValue(1.5)]
        public double DominanceThreshold { get; set; } = 1.5;

        /// <summary>Maximum acceptable noise level.</summary>
        [DefaultValue(500.0)]
        public double NoiseThreshold { get; set; } = 500.0;

        /// <summary>Threshold for directional blur ratio.</summary>
        [DefaultValue(3.0)]
        public double MotionBlurThreshold { get; set; } = 3.0;

        /// <summary>Threshold for banding detection.</summary>
        [DefaultValue(0.5)]
        public double BandingThreshold { get; set; } = 0.5;

        /// <summary>
        /// When true the checker will generate heatmaps for blur and glare
        /// detection. These maps can be used to locate problematic areas.
        /// </summary>
        [DefaultValue(false)]
        public bool GenerateHeatmaps { get; set; } = false;

        /// <summary>
        /// Optional scale factor applied before processing. Values below 1
        /// downsample the image to speed up computations. Heatmaps and
        /// regions are rescaled back to the original resolution.
        /// </summary>
        [DefaultValue(1.0)]
        public double ProcessingScale { get; set; } = 1.0;
    }
}
