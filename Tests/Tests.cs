using NUnit.Framework;
using static VelozientCodeTest.Program;

namespace Tests
{
    [TestFixture]
    public class DeliveryOptimizerTests
    {
        [Test]
        public void TestOptimizeDeliveries()
        {
            // Arrange
            var drones = new List<Drone>
                        {
                            new Drone { Name = "DroneA", MaxWeight = 200 },
                            new Drone { Name = "DroneB", MaxWeight = 250 },
                            new Drone { Name = "DroneC", MaxWeight = 100 }
                        };
            var locations = new List<Location>
                            {
                                new Location { Name = "LocationA", PackageWeight = 200 },
                                new Location { Name = "LocationB", PackageWeight = 150 },
                                new Location { Name = "LocationC", PackageWeight = 50 },
                                new Location { Name = "LocationD", PackageWeight = 150 },
                                new Location { Name = "LocationE", PackageWeight = 100 },
                                new Location { Name = "LocationF", PackageWeight = 200 },
                                new Location { Name = "LocationG", PackageWeight = 50 },
                                new Location { Name = "LocationH", PackageWeight = 80 },
                                new Location { Name = "LocationI", PackageWeight = 70 },
                                new Location { Name = "LocationJ", PackageWeight = 50 },
                                new Location { Name = "LocationK", PackageWeight = 30 },
                                new Location { Name = "LocationL", PackageWeight = 20 },
                                new Location { Name = "LocationM", PackageWeight = 50 },
                                new Location { Name = "LocationN", PackageWeight = 30 },
                                new Location { Name = "LocationO", PackageWeight = 20 },
                                new Location { Name = "LocationP", PackageWeight = 90 }
                            };

            // Act
            var deliveries = OptimizeDeliveries(drones, locations);
            Assert.Multiple(() =>
            {

                // Assert // if all drones has at least one trip
                Assert.That(deliveries[0].Trips, Is.Not.Empty);
                Assert.That(deliveries[1].Trips, Is.Not.Empty);
                Assert.That(deliveries[2].Trips, Is.Not.Empty);
            });
        }
    }
}