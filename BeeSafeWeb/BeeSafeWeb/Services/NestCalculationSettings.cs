namespace BeeSafeWeb.Services
{
    public static class NestCalculationSettings
    {
        public static double HornetSpeed { get; set; } = 11.1;
        public static double CorrectionFactor { get; set; } = 0.1;
        public static double GeoThreshold { get; set; } = 100.0;
        public static bool ReverseBearing { get; set; } = true;
        public static double DirectionBucketSize { get; set; } = 45.0;
        public static double DirectionThreshold { get; set; } = 45.0;
        public static double OverlapThreshold { get; set; } = 0.5;
    }
}