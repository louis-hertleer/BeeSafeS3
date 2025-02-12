using BeeSafeWeb.Utility.Models;
using BeeSafeWeb.Utility.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BeeSafeWeb.Data;
using System.Diagnostics;

namespace BeeSafeWeb.Services
{
    /// <summary>
    /// Provides functionality for calculating, clustering, and persisting nest location estimates
    /// based on detection events.
    /// </summary>
    public class NestLocationService
    {
        private readonly IRepository<DetectionEvent> _detectionEventRepository;
        private readonly IRepository<NestEstimate> _nestEstimateRepository;
        
        private const double HornetSpeed = 11.1;  // m/s
        private const double CorrectionFactor = 0.1;
        private const double EarthRadius = 6371000;  // m

        private double GeoThreshold = 100;  
        private const int MinPoints = 2;

        private bool ReverseBearing = true;
        private const double DirectionBucketSize = 45.0;
        private const double DirectionThreshold = 45.0;
        private const double OverlapThreshold = 0.5;

        public NestLocationService(
            IRepository<DetectionEvent> detectionEventRepository,
            IRepository<NestEstimate> nestEstimateRepository)
        {
            _detectionEventRepository = detectionEventRepository;
            _nestEstimateRepository = nestEstimateRepository;
        }

   public async Task<List<NestEstimate>> CalculateAndPersistNestLocationsAsync()
{
    Debug.WriteLine("Starting CalculateAndPersistNestLocationsAsync.");

    // Step 1: Store existing nests before recalculating
    var existingNests = await _nestEstimateRepository.GetQueryable().ToListAsync(); 
    Debug.WriteLine($"Stored {existingNests.Count} existing nests.");

    // Step 2: Calculate new nest locations
    var newEstimates = await CalculateNestLocationsAsync();
    Debug.WriteLine($"New nest estimates count: {newEstimates.Count}");

    if (newEstimates.Count == 0)
    {
        Debug.WriteLine("No new nest estimates calculated.");
        return newEstimates;
    }

    // Step 3: Match new estimates with existing nests
    foreach (var newEstimate in newEstimates)
    {
        // Find a nearby existing nest (even if KnownHornet is NULL)
        var matchingExistingNest = existingNests.FirstOrDefault(n =>
            GetDistanceInMeters(n.EstimatedLatitude, n.EstimatedLongitude, 
                                newEstimate.EstimatedLatitude, newEstimate.EstimatedLongitude) < 50); // 50 meters threshold

        if (matchingExistingNest != null)
        {
            Debug.WriteLine($"Matching existing nest found: {matchingExistingNest.Id}");

            // Preserve existing destroyed status
            bool wasDestroyed = matchingExistingNest.IsDestroyed;
            newEstimate.IsDestroyed = wasDestroyed;

            newEstimate.Id = matchingExistingNest.Id;

            _nestEstimateRepository.Detach(matchingExistingNest);
            
            await _nestEstimateRepository.UpdateAsync(newEstimate);
        }

        else
        {
            Debug.WriteLine($"No match found, adding new nest estimate: {newEstimate.Id}");
            await _nestEstimateRepository.AddAsync(newEstimate);
        }
    }

    Debug.WriteLine("Finished updating nest estimates.");
    return newEstimates;
}

        
        private double GetDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = EarthRadius * c;
            Debug.WriteLine($"Distance between ({lat1}, {lon1}) and ({lat2}, {lon2}) = {distance:F2} m");
            return distance;
        }
        
        private double AngleDifference(double angle1, double angle2)
        {
            double diff = Math.Abs(angle1 - angle2) % 360;
            return diff > 180 ? 360 - diff : diff;
        }
        
        private List<NestEstimate> RefineClusterPredictions(List<NestEstimate> estimates)
        {
            Debug.WriteLine("Refining cluster predictions within group.");
            var refinedClusters = new List<NestEstimate>();
            while (estimates.Any())
            {
                var current = estimates.First();
                var overlapping = estimates.Where(e => 
                    GetDistanceInMeters(e.EstimatedLatitude, e.EstimatedLongitude,
                                        current.EstimatedLatitude, current.EstimatedLongitude) < GeoThreshold &&
                    AngleDifference(e.Direction, current.Direction) < DirectionThreshold
                ).ToList();
                
                Debug.WriteLine($"Found {overlapping.Count} overlapping estimates for event with direction {current.Direction:F2}.");
                
                if (overlapping.Count > 1)
                {
                    double totalWeight = overlapping.Sum(e => 1.0 / Math.Pow((e.DisplayAccuracy ?? 1.0), 2));
                    double weightedLat = overlapping.Sum(e => e.EstimatedLatitude * (1.0 / Math.Pow((e.DisplayAccuracy ?? 1.0), 2)));
                    double weightedLon = overlapping.Sum(e => e.EstimatedLongitude * (1.0 / Math.Pow((e.DisplayAccuracy ?? 1.0), 2)));
                    
                    double refinedLat = weightedLat / totalWeight;
                    double refinedLon = weightedLon / totalWeight;
                    double refinedAccuracy = 1.0 / Math.Sqrt(totalWeight);
                    DateTime latestTimestamp = overlapping.Max(e => e.Timestamp);
                    var bestEvent = overlapping.OrderBy(e => e.DisplayAccuracy ?? 1.0).First();
                    double refinedDirection = bestEvent.Direction;
                    
                    var refined = new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = refinedLat,
                        EstimatedLongitude = refinedLon,
                        DisplayAccuracy = refinedAccuracy,
                        Timestamp = latestTimestamp,
                        KnownHornet = overlapping.First().KnownHornet,
                        Direction = refinedDirection
                    };
                    Debug.WriteLine($"Merged cluster: Lat={refinedLat:F6}, Lon={refinedLon:F6}, Direction={refinedDirection:F2}°.");
                    refinedClusters.Add(refined);
                    estimates = estimates.Except(overlapping).ToList();
                }
                else
                {
                    refinedClusters.Add(current);
                    estimates.Remove(current);
                }
            }
            Debug.WriteLine($"Refined clusters count: {refinedClusters.Count}");
            return refinedClusters;
        }
        
        private List<NestEstimate> MergeFullyContainedClusters(List<NestEstimate> clusters)
        {
            Debug.WriteLine("Post-processing clusters for significant overlap.");
            var result = new List<NestEstimate>();
            var used = new bool[clusters.Count];
            for (int i = 0; i < clusters.Count; i++)
            {
                if (used[i]) continue;
                var group = new List<NestEstimate> { clusters[i] };
                used[i] = true;
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    if (used[j]) continue;
                    if (ShouldMerge(clusters[i], clusters[j]))
                    {
                        group.Add(clusters[j]);
                        used[j] = true;
                    }
                }
                var merged = MergeClusters(group);
                Debug.WriteLine($"Merged {group.Count} clusters into one.");
                result.Add(merged);
            }
            Debug.WriteLine($"Post-processed predictions count: {result.Count}");
            return result;
        }
        
        private bool ShouldMerge(NestEstimate a, NestEstimate b)
        {
            double ratio = OverlapRatio(a, b);
            Debug.WriteLine($"Overlap ratio: {ratio:F2}");
            return ratio >= OverlapThreshold;
        }
        
        private double CircleIntersectionArea(double r1, double r2, double d)
        {
            if (d >= r1 + r2)
                return 0.0;
            if (d <= Math.Abs(r1 - r2))
                return Math.PI * Math.Pow(Math.Min(r1, r2), 2);
            double r1Sq = r1 * r1;
            double r2Sq = r2 * r2;
            double alpha = Math.Acos((d * d + r1Sq - r2Sq) / (2 * d * r1)) * 2;
            double beta = Math.Acos((d * d + r2Sq - r1Sq) / (2 * d * r2)) * 2;
            double area1 = 0.5 * r1Sq * (alpha - Math.Sin(alpha));
            double area2 = 0.5 * r2Sq * (beta - Math.Sin(beta));
            return area1 + area2;
        }
        
        private double OverlapRatio(NestEstimate a, NestEstimate b)
        {
            double r1 = (double)a.DisplayAccuracy;
            double r2 = (double)b.DisplayAccuracy;
            double d = GetDistanceInMeters(a.EstimatedLatitude, a.EstimatedLongitude,
                                           b.EstimatedLatitude, b.EstimatedLongitude);
            double intersection = CircleIntersectionArea(r1, r2, d);
            double smallerArea = Math.PI * Math.Pow(Math.Min(r1, r2), 2);
            double ratio = intersection / smallerArea;
            Debug.WriteLine($"Calculated overlap ratio: {ratio:F2}");
            return ratio;
        }
        
        private NestEstimate MergeClusters(List<NestEstimate> clusters)
        {
            double totalWeight = clusters.Sum(c => 1.0 / Math.Pow((c.DisplayAccuracy ?? 1.0), 2));
            double weightedLat = clusters.Sum(c => c.EstimatedLatitude * (1.0 / Math.Pow((c.DisplayAccuracy ?? 1.0), 2)));
            double weightedLon = clusters.Sum(c => c.EstimatedLongitude * (1.0 / Math.Pow((c.DisplayAccuracy ?? 1.0), 2)));
            double mergedLat = weightedLat / totalWeight;
            double mergedLon = weightedLon / totalWeight;
            double mergedAccuracy = 1.0 / Math.Sqrt(totalWeight);
            DateTime latestTimestamp = clusters.Max(c => c.Timestamp);
            var best = clusters.OrderBy(c => c.DisplayAccuracy ?? 1.0).First();
            double mergedDirection = best.Direction;
            Debug.WriteLine($"Merged cluster: Lat={mergedLat:F6}, Lon={mergedLon:F6}, Accuracy={mergedAccuracy:F2}, Direction={mergedDirection:F2}°");
            return new NestEstimate
            {
                Id = Guid.NewGuid(),
                EstimatedLatitude = mergedLat,
                EstimatedLongitude = mergedLon,
                DisplayAccuracy = mergedAccuracy,
                Timestamp = latestTimestamp,
                KnownHornet = clusters.First().KnownHornet,
                Direction = mergedDirection
            };
        }
        
        public async Task<List<NestEstimate>> CalculateNestLocationsAsync()
        {
            Debug.WriteLine("Calculating nest locations.");
            var detectionEvents = await _detectionEventRepository.GetQueryable()
                .Include(e => e.KnownHornet)
                .Include(e => e.Device)
                .ToListAsync();
            Debug.WriteLine($"Total detection events retrieved: {detectionEvents.Count}");

            var validEvents = detectionEvents
                .Where(e => e.Device != null && (e.KnownHornet != null || e.IsManual))
                .Where(e =>
                {
                    var timeDiff = e.SecondDetection - e.FirstDetection;
                    if (timeDiff.TotalSeconds <= 0)
                    {
                        Debug.WriteLine($"Skipping event {e.Id} with non-positive time difference: {timeDiff.TotalSeconds} seconds.");
                        return false;
                    }
                    if (timeDiff.TotalMinutes > 20)
                    {
                        Debug.WriteLine($"Skipping event {e.Id} with time difference > 20 minutes: {timeDiff.TotalMinutes} minutes.");
                        return false;
                    }
                    return true;
                })
                .ToList();
            Debug.WriteLine($"Valid detection events count: {validEvents.Count}");

            var individualEstimates = new List<NestEstimate>();
            foreach (var de in validEvents)
            {
                Debug.WriteLine($"Processing detection event ID {de.Id}. IsManual: {de.IsManual}, HornetCount: {de.HornetCount}");
                var nest = CalculateFromSingleDevice(de);
                if (nest != null)
                {
                    individualEstimates.Add(nest);
                    Debug.WriteLine($"Calculated nest estimate: Lat={nest.EstimatedLatitude:F6}, Lon={nest.EstimatedLongitude:F6}, Direction={nest.Direction:F2}°");
                }
                else
                {
                    Debug.WriteLine($"Detection event {de.Id} skipped due to invalid data or timing.");
                }
            }

            Debug.WriteLine($"Total individual nest estimates: {individualEstimates.Count}");
            if (individualEstimates.Count == 0)
            {
                return new List<NestEstimate>();
            }

            if (individualEstimates.Count > 5)
            {
                GeoThreshold = individualEstimates.Average(e => e.AccuracyLevel) * 1.5;
                Debug.WriteLine($"Adjusted GeoThreshold: {GeoThreshold:F2}");
            }
            else
            {
                Debug.WriteLine($"Using default GeoThreshold: {GeoThreshold}");
            }

            var finalPredictions = new List<NestEstimate>();
            var groupsByDevice = individualEstimates.GroupBy(e => e.KnownHornet?.Id ?? Guid.Empty);
            foreach (var deviceGroup in groupsByDevice)
            {
                Debug.WriteLine($"Processing group for device/hornet ID: {deviceGroup.Key}. Count: {deviceGroup.Count()}");
                var groupsByDirection = deviceGroup.GroupBy(e => Math.Round(e.Direction / DirectionBucketSize) * DirectionBucketSize);
                foreach (var directionGroup in groupsByDirection)
                {
                    Debug.WriteLine($"Direction bucket: {directionGroup.Key}. Count: {directionGroup.Count()}");
                    var mergedClusters = RefineClusterPredictions(directionGroup.ToList());
                    finalPredictions.AddRange(mergedClusters);
                }
            }
            Debug.WriteLine($"Predictions after grouping: {finalPredictions.Count}");

            var postProcessedPredictions = MergeFullyContainedClusters(finalPredictions);
            Debug.WriteLine($"Final predictions after post-processing: {postProcessedPredictions.Count}");
            return postProcessedPredictions;
        }

        private List<NestEstimate> DBSCANCluster(List<NestEstimate> estimates)
        {
            Debug.WriteLine("Running placeholder DBSCAN clustering.");
            return estimates;
        }

        private NestEstimate? CalculateFromSingleDevice(DetectionEvent de)
        {
            if (de.Device == null)
            {
                Debug.WriteLine($"Detection event {de.Id} skipped: Device is null.");
                return null;
            }

            var device = de.Device;
            double flightTimeSeconds = (de.SecondDetection - de.FirstDetection).TotalSeconds;
            if (flightTimeSeconds <= 0)
            {
                Debug.WriteLine($"Detection event {de.Id} skipped: Non-positive flight time.");
                return null;
            }
            double estimatedDistance = flightTimeSeconds * HornetSpeed * CorrectionFactor;
            Debug.WriteLine($"Event {de.Id}: Flight time={flightTimeSeconds:F2} s, Estimated distance={estimatedDistance:F2} m");

            double latRad = device.Latitude * Math.PI / 180.0;
            double lonRad = device.Longitude * Math.PI / 180.0;
            double dirRad = de.HornetDirection * Math.PI / 180.0;
            Debug.WriteLine($"Event {de.Id}: Original hornet direction: {de.HornetDirection:F2}° (radians: {dirRad:F4})");

            double finalDirRad = ReverseBearing ? (dirRad + Math.PI) % (2 * Math.PI) : dirRad;
            double finalBearingDegrees = finalDirRad * 180.0 / Math.PI;
            Debug.WriteLine($"Event {de.Id}: Final bearing: {finalBearingDegrees:F2}° (radians: {finalDirRad:F4})");

            double distanceRatio = estimatedDistance / EarthRadius;
            double predictedLatRad = Math.Asin(
                Math.Sin(latRad) * Math.Cos(distanceRatio) +
                Math.Cos(latRad) * Math.Sin(distanceRatio) * Math.Cos(finalDirRad)
            );
            double predictedLonRad = lonRad + Math.Atan2(
                Math.Sin(finalDirRad) * Math.Sin(distanceRatio) * Math.Cos(latRad),
                Math.Cos(distanceRatio) - Math.Sin(latRad) * Math.Sin(predictedLatRad)
            );

            var nestEstimate = new NestEstimate
            {
                Id = Guid.NewGuid(),
                EstimatedLatitude = predictedLatRad * 180.0 / Math.PI,
                EstimatedLongitude = predictedLonRad * 180.0 / Math.PI,
                AccuracyLevel = estimatedDistance,
                DisplayAccuracy = estimatedDistance,
                DisplayLatitude = predictedLatRad * 180.0 / Math.PI,
                DisplayLongitude = predictedLonRad * 180.0 / Math.PI,
                Timestamp = de.SecondDetection,
                KnownHornet = de.KnownHornet,
                Direction = finalBearingDegrees
            };

            Debug.WriteLine($"Event {de.Id}: Nest estimate calculated: Lat={nestEstimate.EstimatedLatitude:F6}, Lon={nestEstimate.EstimatedLongitude:F6}, Direction={nestEstimate.Direction:F2}°");
            return nestEstimate;
        }
    }
}
