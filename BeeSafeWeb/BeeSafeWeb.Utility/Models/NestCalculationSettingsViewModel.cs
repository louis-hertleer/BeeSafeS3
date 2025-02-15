namespace BeeSafeWeb.Models
{
    public class NestCalculationSettingsViewModel
    {
        /// <summary>
        /// Hornet speed in m/s.
        /// </summary>
        public double HornetSpeed { get; set; }
        
        /// <summary>
        /// Correction factor for distance calculation.
        /// </summary>
        public double CorrectionFactor { get; set; }
        
        /// <summary>
        /// Geographic threshold (meters) used for clusteri
        /// </summary>
        public double GeoThreshold { get; set; }

        /// <summary>
        /// If true, the hornet bearing is reversed.
        /// </summary>
        public double DirectionBucketSize { get; set; }
                
        /// <summary>
        /// Maximum allowed difference (in degrees) within a bucket.
        /// </summary>
        public double DirectionThreshold { get; set; }
                
        /// <summary>
        /// Overlap threshold for merging clusters.
        /// </summary>
        public double OverlapThreshold { get; set; }
    }
}