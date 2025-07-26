using SkiaSharp;

namespace DocQualityChecker
{
    /// <summary>
    /// Result produced by the quality checks.
    /// </summary>
    public class DocumentQualityResult
    {
        public double BrisqueScore { get; set; }
        public bool IsBlurry { get; set; }
        public double BlurScore { get; set; }
        public bool HasGlare { get; set; }
        public int GlareArea { get; set; }
        public bool IsValidDocument { get; set; }

        /// <summary>
        /// Optional blur heatmap if generated.
        /// </summary>
        public SKBitmap? BlurHeatmap { get; set; }

        /// <summary>
        /// Optional glare heatmap if generated.
        /// </summary>
        public SKBitmap? GlareHeatmap { get; set; }
    }
}
