using BeeSafeWeb.Utility.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeeSafeWeb.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BeeSafeContext context)
        {
            // Ensure the database is created.
            context.Database.EnsureCreated();

            // If there are already devices, assume the DB is seeded.
            if (context.Devices.Any())
            {
                return;
            }

            // --- Seed Devices ---
            // Create three devices with distinct coordinates.
            var devices = new List<Device>
            {
                new Device
                {
                    Id = Guid.NewGuid(),
                    Latitude = 51.1681,
                    Longitude = 4.9808,
                    IsApproved = true,
                    IsTracking = true,
                    Direction = 45.0,
                    LastActive = DateTime.Now,
                    DetectionEvents = new List<DetectionEvent>(),
                    Name = "device 1"
                },
                new Device
                {
                    Id = Guid.Parse("4b9a002b-3725-4b74-b034-43e98bb52520"),
                    Latitude = 51.1700,
                    Longitude = 4.9850,
                    IsApproved = true,
                    IsTracking = true,
                    Direction = 90.0,
                    LastActive = DateTime.Now,
                    DetectionEvents = new List<DetectionEvent>(),
                    Name = "device 2"

                },
                new Device
                {
                    Id = Guid.NewGuid(),
                    Latitude = 51.1720,
                    Longitude = 4.9820,
                    IsApproved = true,
                    IsTracking = true,
                    Direction = 60.0,
                    LastActive = DateTime.Now,
                    DetectionEvents = new List<DetectionEvent>(),
                    Name = "device 3"

                }
            };

            context.Devices.AddRange(devices);

            // --- Seed Known Hornets ---
            // Create four distinct hornets.
            var knownHornets = new List<KnownHornet>
            {
                new KnownHornet { Id = Guid.NewGuid(), NestEstimates = new List<NestEstimate>() },
                new KnownHornet { Id = Guid.NewGuid(), NestEstimates = new List<NestEstimate>() },
                new KnownHornet { Id = Guid.NewGuid(), NestEstimates = new List<NestEstimate>() },
                new KnownHornet { Id = Guid.NewGuid(), NestEstimates = new List<NestEstimate>() }
            };

            context.KnownHornets.AddRange(knownHornets);

            // --- Seed Detection Events ---
            // Create base detection events for each hornet (2 events per hornet).
            var now = DateTime.Now;
            var detectionEvents = new List<DetectionEvent>
            {
                // For knownHornet 1:
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 330.0,
                    FirstDetection = now.AddMinutes(-15),
                    SecondDetection = now.AddMinutes(-10),
                    Device = devices[0],
                    KnownHornet = knownHornets[0],
                    HornetCount = 1

                },
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 340.0,
                    FirstDetection = now.AddMinutes(-14),
                    SecondDetection = now.AddMinutes(-9),
                    Device = devices[1],
                    KnownHornet = knownHornets[0],
                    HornetCount = 1

                },
                // For knownHornet 2:
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 320.0,
                    FirstDetection = now.AddMinutes(-13),
                    SecondDetection = now.AddMinutes(-8),
                    Device = devices[1],
                    KnownHornet = knownHornets[1],
                    HornetCount = 1

                },
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 310.0,
                    FirstDetection = now.AddMinutes(-12),
                    SecondDetection = now.AddMinutes(-7),
                    Device = devices[2],
                    KnownHornet = knownHornets[1],
                    HornetCount = 1

                },
                // For knownHornet 3:
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 350.0,
                    FirstDetection = now.AddMinutes(-15),
                    SecondDetection = now.AddMinutes(-10),
                    Device = devices[2],
                    KnownHornet = knownHornets[2],
                    HornetCount = 1

                },
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 355.0,
                    FirstDetection = now.AddMinutes(-14),
                    SecondDetection = now.AddMinutes(-9),
                    Device = devices[0],
                    KnownHornet = knownHornets[2],
                    HornetCount = 1

                },
                // For knownHornet 4:
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 300.0,
                    FirstDetection = now.AddMinutes(-10),
                    SecondDetection = now.AddMinutes(-5),
                    Device = devices[0],
                    KnownHornet = knownHornets[3],
                    HornetCount = 1

                },
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 305.0,
                    FirstDetection = now.AddMinutes(-9),
                    SecondDetection = now.AddMinutes(-4),
                    Device = devices[1],
                    KnownHornet = knownHornets[3],
                    HornetCount = 1

                },
                // Additional detection event with null KnownHornet (will be ignored)
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 260.0,
                    FirstDetection = now.AddMinutes(-10),
                    SecondDetection = now.AddMinutes(-5),
                    Device = devices[0],
                    KnownHornet = null,
                    HornetCount = 1

                }
            };

            // --- Add Additional Detection Events ---
            // Hornet 1: Add 8 more events.
            for (int i = 0; i < 8; i++)
            {
                detectionEvents.Add(new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 335.0 + i,  // Slight variation in direction
                    FirstDetection = now.AddMinutes(-16 - i),
                    SecondDetection = now.AddMinutes(-11 - i),
                    Device = devices[i % devices.Count],
                    KnownHornet = knownHornets[0],
                    HornetCount = 1

                });
            }

            // Hornet 2: Add 6 more events.
            for (int i = 0; i < 6; i++)
            {
                detectionEvents.Add(new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 315.0 + i,  // Variation in direction
                    FirstDetection = now.AddMinutes(-14 - i),
                    SecondDetection = now.AddMinutes(-9 - i),
                    Device = devices[i % devices.Count],
                    KnownHornet = knownHornets[1],
                    HornetCount = 1
                });
            }

            // Hornet 3: Add 2 more events.
            for (int i = 0; i < 2; i++)
            {
                detectionEvents.Add(new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 352.0 + i,  // Variation in direction
                    FirstDetection = now.AddMinutes(-15 - i),
                    SecondDetection = now.AddMinutes(-10 - i),
                    Device = devices[i % devices.Count],
                    KnownHornet = knownHornets[2],
                    HornetCount = 1

                });
            }

            // Hornet 4: Add 14 more events.
            for (int i = 0; i < 14; i++)
            {
                detectionEvents.Add(new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = now,
                    HornetDirection = 298.0 + i,  // Variation in direction
                    FirstDetection = now.AddMinutes(-11 - i),
                    SecondDetection = now.AddMinutes(-6 - i),
                    Device = devices[i % devices.Count],
                    KnownHornet = knownHornets[3],
                    HornetCount = 1

                });
            }

            context.DetectionEvents.AddRange(detectionEvents);

            // --- Optionally, Seed Pre-calculated Nest Estimates ---
            // (Note: Your calculation service creates new nest estimates from detection events,
            // so pre-seeded nest estimates may not be used when using the calculation-based controller.)
            /*
            var nestEstimates = new List<NestEstimate>
            {
                new NestEstimate
                {
                    Id = Guid.NewGuid(),
                    EstimatedLatitude = 51.1690,
                    EstimatedLongitude = 4.9830,
                    AccuracyLevel = 10.0,
                    IsDestroyed = false,
                    Timestamp = now,
                    KnownHornet = knownHornets[0]
                }
            };
            context.NestEstimates.AddRange(nestEstimates);
            */

            // --- Seed Color Codes ---
            var colorCodes = new List<ColorCode>
            {
                new ColorCode { Id = Guid.NewGuid(), Color = "Red" },
                new ColorCode { Id = Guid.NewGuid(), Color = "Green" },
                new ColorCode { Id = Guid.NewGuid(), Color = "Blue" }
            };

            context.ColorCodes.AddRange(colorCodes);

            // Save all changes to the database.
            context.SaveChanges();
        }
    }
}
