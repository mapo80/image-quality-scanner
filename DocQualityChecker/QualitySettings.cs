using System;

namespace DocQualityChecker
{
    /// <summary>
    /// Settings used to evaluate document image quality.
    /// </summary>
    public class QualitySettings
    {
        /// <summary>Maximum acceptable BRISQUE score.</summary>
        public double BrisqueMax { get; set; } = 50.0;
        /// <summary>Threshold for blur detection using Laplacian variance.</summary>
        public double BlurThreshold { get; set; } = 100.0;
        /// <summary>Brightness threshold for glare detection.</summary>
        public int BrightThreshold { get; set; } = 240;
        /// <summary>Area threshold for glare detection.</summary>
        public int AreaThreshold { get; set; } = 500;

        /// <summary>Minimum accepted average luminance.</summary>
        public double ExposureMin { get; set; } = 80.0;

        /// <summary>Maximum accepted average luminance.</summary>
        public double ExposureMax { get; set; } = 180.0;

        /// <summary>Minimum standard deviation of luminance for sufficient contrast.</summary>
        public double ContrastMin { get; set; } = 30.0;

        /// <summary>Ratio of dominant RGB channel over the average to signal color cast.</summary>
        public double DominanceThreshold { get; set; } = 1.5;

        /// <summary>Maximum acceptable noise level.</summary>
        public double NoiseThreshold { get; set; } = 500.0;

        /// <summary>
        /// When true the checker will generate heatmaps for blur and glare
        /// detection. These maps can be used to locate problematic areas.
        /// </summary>
        public bool GenerateHeatmaps { get; set; } = false;
    }
}
