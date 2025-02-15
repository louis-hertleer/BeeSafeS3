namespace BeeSafeWeb.Models
{
    public class NestCalculationSettingsViewModel
    {
        // Hornet speed in m/s.
        public double HornetSpeed { get; set; }
        // Correction factor for distance calculation.
        public double CorrectionFactor { get; set; }
        // Geographic threshold (meters) used for clustering.
        public double GeoThreshold { get; set; }
        // If true, the hornet bearing is reversed.
        public double DirectionBucketSize { get; set; }
        // Maximum allowed difference (in degrees) within a bucket.
        public double DirectionThreshold { get; set; }
        // Overlap threshold for merging clusters.
        public double OverlapThreshold { get; set; }
    }
}