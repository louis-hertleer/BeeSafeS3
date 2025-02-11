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
        private readonly IRepository<NestEstimate> _nestEstimateRepository;

        public NestLocationService(
            IRepository<DetectionEvent> detectionEventRepository,
            IRepository<NestEstimate> nestEstimateRepository)
        {
            _detectionEventRepository = detectionEventRepository;
            _nestEstimateRepository = nestEstimateRepository;
        }
        
        /// <summary>
        /// Computes nest estimates from detection events.
        /// </summary>
        public async Task<List<NestEstimate>> CalculateNestLocationsAsync()
        {
            // Retrieve detection events including related KnownHornet and Device.
            var detectionEvents = await _detectionEventRepository.GetQueryable()
                .Include(e => e.KnownHornet)
                .Include(e => e.Device)
                .ToListAsync();

            // Filter detection events that meet the criteria.
            var validEvents = detectionEvents
                .Where(e => (e.SecondDetection - e.FirstDetection).TotalMinutes <= 20)
                .ToList();

            var nestEstimates = new List<NestEstimate>();

            // Group events by KnownHornet.Id (skip groups with null KnownHornet).
            var groupedEvents = validEvents
                .GroupBy(e => e.KnownHornet?.Id ?? Guid.Empty)
                .Where(g => g.Key != Guid.Empty);

            foreach (var group in groupedEvents)
            {
                var events = group.ToList();
                if (events.Count < 2)
                    continue;

                // Calculate the intersection point using the helper function.
                var intersection = CalculateIntersection(events);
                if (intersection == null)
                    continue;

                // Define computedLatitude and computedLongitude based on the intersection.
                double computedLatitude = intersection.Value.Latitude;
                double computedLongitude = intersection.Value.Longitude;

                // Assume each detection event has a reference to its Device.
                var device = events.First().Device;

                // Calculate accuracy using a method that blends manual override if set.
                double accuracy = CalculateAccuracy(events, device);

                // Create a new nest estimate using the computed values.
                nestEstimates.Add(new NestEstimate
                {
                    Id = Guid.NewGuid(),
                    EstimatedLatitude = computedLatitude,
                    EstimatedLongitude = computedLongitude,
                    AccuracyLevel = accuracy,
                    IsDestroyed = false,
                    Timestamp = events.Max(e => e.SecondDetection),
                    KnownHornet = events.First().KnownHornet,
                    DisplayLatitude = computedLatitude,
                    DisplayLongitude = computedLongitude,
                    DisplayAccuracy = accuracy
                });
            }

            // Aggregate (cluster) nest estimates that are very close together.
            var aggregatedEstimates = AggregateNestEstimates(nestEstimates, 20.0);
            return aggregatedEstimates;
        }

        /// <summary>
        /// Computes nest estimates and persists them if not already stored.
        /// </summary>
        public async Task<List<NestEstimate>> CalculateAndPersistNestLocationsAsync()
        {
            var computedEstimates = await CalculateNestLocationsAsync();

            foreach (var estimate in computedEstimates)
            {
                if (estimate.KnownHornet != null)
                {
                    // Look for an existing persisted record using the unique KnownHornet Id.
                    var existing = await _nestEstimateRepository.GetQueryable()
                        .FirstOrDefaultAsync(n => n.KnownHornet.Id == estimate.KnownHornet.Id);
                    if (existing != null)
                    {
                        // Update computed/display values (leaving user changes like IsDestroyed intact).
                        existing.DisplayLatitude = estimate.DisplayLatitude;
                        existing.DisplayLongitude = estimate.DisplayLongitude;
                        existing.DisplayAccuracy = estimate.DisplayAccuracy;
                        existing.EstimatedLatitude = estimate.EstimatedLatitude;
                        existing.EstimatedLongitude = estimate.EstimatedLongitude;
                        existing.AccuracyLevel = estimate.AccuracyLevel;
                        // Use the latest timestamp from the detection events.
                        existing.Timestamp = estimate.Timestamp;
                        await _nestEstimateRepository.UpdateAsync(existing);
                        // Use the persisted ID.
                        estimate.Id = existing.Id;
                        // Copy back the persisted IsDestroyed value so the view reflects the user's change.
                        estimate.IsDestroyed = existing.IsDestroyed;
                    }
                    else
                    {
                        // Persist the new nest estimate.
                        await _nestEstimateRepository.AddAsync(estimate);
                    }
                }
            }

            return computedEstimates;
        }

        /// <summary>
        /// Calculates an approximate intersection point from a list of detection events.
        /// Here we simply use the average of the two device coordinates from the first two events.
        /// </summary>
        private (double Latitude, double Longitude)? CalculateIntersection(List<DetectionEvent> events)
        {
            if (events.Count < 2)
                return null;

            var device1 = events[0].Device;
            var device2 = events[1].Device;

            if (device1 == null || device2 == null)
                return null;

            // Simple triangulation: average of the two device coordinates.
            var lat = (device1.Latitude + device2.Latitude) / 2;
            var lon = (device1.Longitude + device2.Longitude) / 2;
            return (lat, lon);
        }

        /// <summary>
        /// Calculates an accuracy value based on detection events and optionally blends a manual override from the device.
        /// </summary>
        private double CalculateAccuracy(List<DetectionEvent> events, Device device)
        {
            // Assume hornet's flight speed is 11.1 m/s.
            const double flightSpeed = 11.1;

            // Calculate average trip time (in seconds) from detection events.
            double avgTripTimeSeconds = events.Average(e => (e.SecondDetection - e.FirstDetection).TotalSeconds);
            double distanceEstimate = flightSpeed * avgTripTimeSeconds;

            // Compute the automatic average hornet direction from the events.
            double autoDirection = events.Average(e => e.HornetDirection);

            // If the device has a manual override, blend it with the automatic value.
            double finalDirection = autoDirection;
            if (device.IsManualDirectionSet && device.ManualHornetDirection.HasValue)
            {
                // For example, use 80% of the manual value and 20% of the automatic value.
                finalDirection = (0.8 * device.ManualHornetDirection.Value + 0.2 * autoDirection) % 360;
            }

            // Compute the standard deviation using the final (blended) direction.
            double directionStdDev = Math.Sqrt(events.Average(e => Math.Pow(e.HornetDirection - finalDirection, 2)));

            // Use the blended direction in the accuracy calculation.
            double rawAccuracy = distanceEstimate * (1 + (directionStdDev / 45.0)) / events.Count;
            double calibrationFactor = 0.2; // Adjust as needed.
            double scaledAccuracy = rawAccuracy * calibrationFactor;
            double maxAccuracy = 200.0;
            double finalAccuracy = Math.Min(scaledAccuracy, maxAccuracy);

            return finalAccuracy;
        }

        /// <summary>
        /// Aggregates nest estimates that are within a specified threshold.
        /// </summary>
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
                double avgLat = cluster.Average(n => n.EstimatedLatitude);
                double avgLng = cluster.Average(n => n.EstimatedLongitude);
                double avgAccuracy = cluster.Average(n => n.AccuracyLevel);
                var aggregatedNest = new NestEstimate
                {
                    Id = Guid.NewGuid(),
                    EstimatedLatitude = avgLat,
                    EstimatedLongitude = avgLng,
                    AccuracyLevel = avgAccuracy,
                    IsDestroyed = cluster.First().IsDestroyed,
                    // Use the maximum (latest) timestamp from the cluster.
                    Timestamp = cluster.Max(n => n.Timestamp),
                    KnownHornet = cluster.First().KnownHornet,
                    DisplayLatitude = avgLat,
                    DisplayLongitude = avgLng,
                    DisplayAccuracy = avgAccuracy
                };
                aggregated.Add(aggregatedNest);
            }
            return aggregated;
        }

        // Haversine formula to compute the distance between two coordinates in meters.
        private double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters.
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
