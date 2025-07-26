namespace DocQualityChecker.Api.Models
{
    /// <summary>
    /// Simple rectangle representation used in API responses.
    /// </summary>
    public class RegionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
