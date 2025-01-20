using BeeSafeWeb.Models;
using System;
using System.Collections.Generic;

namespace BeeSafeWeb.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BeeSafeContext context)
        {
            context.Database.EnsureCreated();

            // Check if data is already seeded
            if (context.Devices.Any())
            {
                return; // Database has been seeded
            }

            // Devices
            var devices = new List<Device>
            {
                new Device
                {
                    Id = Guid.NewGuid(),
                    Latitude = 51.168100,
                    Longitude = 4.980800,
                    IsApproved = true,
                    IsTracking = true,
                    Direction = 45.0,
                    DetectionEvents = new List<DetectionEvent>(),
                    LastActive = DateTime.Now
                },
                new Device
                {
                    Id = Guid.NewGuid(),
                    Latitude = 51.168100,
                    Longitude = 4.980980,
                    IsApproved = true,
                    IsTracking = false,
                    Direction = 90.0,
                    DetectionEvents = new List<DetectionEvent>(),
                    LastActive = DateTime.Now
                }
            };

            context.Devices.AddRange(devices);

            // Known Hornets and Nest Estimates
            var knownHornet = new KnownHornet
            {
                Id = Guid.NewGuid(),
                NestEstimates = new List<NestEstimate>
                {
                    // Existing nest in Geel
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.168500,
                        EstimatedLongitude = 4.980980,
                        AccuracyLevel = 13.0,
                        IsDestroyed = false,
                        Timestamp = DateTime.UtcNow
                    },
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.168200,
                        EstimatedLongitude = 4.981586,
                        AccuracyLevel = 15.0,
                        IsDestroyed = true,
                        Timestamp = DateTime.UtcNow
                    },

                    // Additional nest in Geel
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.165000, // Geel
                        EstimatedLongitude = 4.989000,
                        AccuracyLevel = 10.0,
                        IsDestroyed = false,
                        Timestamp = DateTime.UtcNow
                    },

                    // Nests in Antwerpen
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.221109, // Antwerpen Central
                        EstimatedLongitude = 4.399708,
                        AccuracyLevel = 20.0,
                        IsDestroyed = false,
                        Timestamp = DateTime.UtcNow
                    },
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.230020, // Near Antwerp Zoo
                        EstimatedLongitude = 4.421349,
                        AccuracyLevel = 25.0,
                        IsDestroyed = false,
                        Timestamp = DateTime.UtcNow
                    },
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.219999, // Antwerp Market Square
                        EstimatedLongitude = 4.400025,
                        AccuracyLevel = 18.0,
                        IsDestroyed = false,
                        Timestamp = DateTime.UtcNow
                    },
                    new NestEstimate
                    {
                        Id = Guid.NewGuid(),
                        EstimatedLatitude = 51.210217, // Antwerp Harbour
                        EstimatedLongitude = 4.392032,
                        AccuracyLevel = 12.0,
                        IsDestroyed = true,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

// Add the known hornet nests to the database context
            context.KnownHornets.Add(knownHornet);


            // Detection Events
            var detectionEvents = new List<DetectionEvent>
            {
                new DetectionEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    HornetDirection = 50.0,
                    FirstDetection = DateTime.UtcNow.AddMinutes(-10),
                    SecondDetection = DateTime.UtcNow.AddMinutes(-5),
                    Device = devices[0],
                    KnownHornet = knownHornet
                }
            };

            context.DetectionEvents.AddRange(detectionEvents);

            // Color Codes
            var colorCodes = new List<ColorCode>
            {
                new ColorCode
                {
                    Id = Guid.NewGuid(),
                    Color = "Red"
                },
                new ColorCode
                {
                    Id = Guid.NewGuid(),
                    Color = "Green"
                },
                new ColorCode
                {
                    Id = Guid.NewGuid(),
                    Color = "Blue"
                }
            };

            context.ColorCodes.AddRange(colorCodes);

            // Users
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Password = "password123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Password = "securepassword"
                }
            };

            context.Users.AddRange(users);

            context.SaveChanges();
        }
    }
}