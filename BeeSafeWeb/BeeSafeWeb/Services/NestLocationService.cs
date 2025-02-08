using BeeSafeWeb.Utility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BeeSafeWeb.Data;

namespace BeeSafeWeb.Services
{
    public class NestLocationService
    {
        private readonly IRepository<DetectionEvent> _detectionEventRepository;

        public NestLocationService(IRepository<DetectionEvent> detectionEventRepository)
        {
            _detectionEventRepository = detectionEventRepository;
        }
        
        public async Task<List<NestEstimate>> CalculateNestLocationsAsync()
        {
            // Use the GetQueryable method to include the navigation properties.
            var detectionEvents = await _detectionEventRepository.GetQueryable()
                .Include(e => e.KnownHornet)
                .Include(e => e.Device)
                .ToListAsync();

            // Filter detection events that meet our criteria.
            var validEvents = detectionEvents
                .Where(e => (e.SecondDetection - e.FirstDetection).TotalMinutes <= 20)
                .ToList();

            var nestEstimates = new List<NestEstimate>();

            // Group detection events by hornet (ensuring KnownHornet is not null)
            var groupedEvents = validEvents
                .GroupBy(e => e.KnownHornet?.Id ?? Guid.Empty)
                .Where(g => g.Key != Guid.Empty);

            foreach (var group in groupedEvents)
            {
                var events = group.ToList();
                if (events.Count < 2)
                    continue;

                // Calculate the intersection point using triangulation.
                var estimatedLocation = CalculateIntersection(events);
                if (estimatedLocation != null)
                {
                    nestEstimates.Add(new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = estimatedLocation.Value.Latitude,
                        EstimatedLongitude = estimatedLocation.Value.Longitude,
                        AccuracyLevel = CalculateAccuracy(events),
                        IsDestroyed = false,
                        Timestamp = DateTime.Now,
                        KnownHornet = events.First().KnownHornet,
                        // Initially, set the display properties equal to the computed ones.
                        DisplayLatitude = estimatedLocation.Value.Latitude,
                        DisplayLongitude = estimatedLocation.Value.Longitude,
                        DisplayAccuracy = CalculateAccuracy(events)
                    });
                }
            }

            // Now aggregate (cluster) nest estimates that are very close together.
            // For example, any nests within 20 meters of each other will be combined.
            var aggregatedEstimates = AggregateNestEstimates(nestEstimates, 20.0);
            return aggregatedEstimates;
        }

        private (double Latitude, double Longitude)? CalculateIntersection(List<DetectionEvent> events)
        {
            // Implement triangulation logic here
            // This is a simplified example; you may use a more advanced method.
            if (events.Count < 2)
                return null;

            var device1 = events[0].Device;
            var device2 = events[1].Device;

            if (device1 == null || device2 == null)
                return null;

            // Calculate the intersection point as the average of the two device coordinates.
            var lat = (device1.Latitude + device2.Latitude) / 2;
            var lon = (device1.Longitude + device2.Longitude) / 2;

            return (lat, lon);
        }

        private double CalculateAccuracy(List<DetectionEvent> events)
        {
            // Use the hornet's flight speed: 40 km/h ≈ 11.1 m/s.
            const double flightSpeed = 11.1;

            // Calculate the average round-trip time (in seconds).
            double avgTripTimeSeconds = events.Average(e => (e.SecondDetection - e.FirstDetection).TotalSeconds);

            // Estimate the distance the hornet might have flown (in meters).
            double distanceEstimate = flightSpeed * avgTripTimeSeconds;

            // Calculate the variability in the hornet's direction.
            double avgDirection = events.Average(e => e.HornetDirection);
            double directionStdDev = Math.Sqrt(events.Average(e => Math.Pow(e.HornetDirection - avgDirection, 2)));

            // Compute a raw accuracy estimate.
            double rawAccuracy = distanceEstimate * (1 + (directionStdDev / 45.0)) / events.Count;

            // Apply a calibration/scaling factor.
            double calibrationFactor = 0.2; // Adjust as needed.
            double scaledAccuracy = rawAccuracy * calibrationFactor;

            // Clamp the maximum accuracy to 200 meters.
            double maxAccuracy = 200.0;
            double finalAccuracy = Math.Min(scaledAccuracy, maxAccuracy);

            return finalAccuracy;
        }

        // --- Aggregation (Clustering) Code ---
        // This method groups nest estimates that are within the specified threshold (in meters)
        // and returns a new list where each cluster is replaced with an aggregated nest estimate.
        private List<NestEstimate> AggregateNestEstimates(List<NestEstimate> estimates, double thresholdMeters)
        {
            var aggregated = new List<NestEstimate>();
            var used = new bool[estimates.Count];

            for (int i = 0; i < estimates.Count; i++)
            {
                if (used[i]) continue;
                var cluster = new List<NestEstimate> { estimates[i] };
                used[i] = true;
                for (int j = i + 1; j < estimates.Count; j++)
                {
                    if (used[j]) continue;
                    double distance = GetDistanceInMeters(
                        estimates[i].EstimatedLatitude,
                        estimates[i].EstimatedLongitude,
                        estimates[j].EstimatedLatitude,
                        estimates[j].EstimatedLongitude);
                    if (distance < thresholdMeters)
                    {
                        cluster.Add(estimates[j]);
                        used[j] = true;
                    }
                }
                // Compute the average values for the cluster.
                double avgLat = cluster.Average(n => n.EstimatedLatitude);
                double avgLng = cluster.Average(n => n.EstimatedLongitude);
                double avgAccuracy = cluster.Average(n => n.AccuracyLevel);
                // Create a new aggregated nest estimate.
                var aggregatedNest = new NestEstimate
                {
                    Id = Guid.NewGuid(),
                    EstimatedLatitude = avgLat,
                    EstimatedLongitude = avgLng,
                    AccuracyLevel = avgAccuracy,
                    IsDestroyed = cluster.First().IsDestroyed,
                    Timestamp = cluster.First().Timestamp,
                    KnownHornet = cluster.First().KnownHornet,
                    DisplayLatitude = avgLat,
                    DisplayLongitude = avgLng,
                    DisplayAccuracy = avgAccuracy
                };
                aggregated.Add(aggregatedNest);
            }
            return aggregated;
        }

        // Helper method to compute the distance in meters between two latitude/longitude points using the Haversine formula.
        private double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
