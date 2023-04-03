using Microsoft.Extensions.Configuration;

namespace VelozientCodeTest
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            // load the configuration file.
            var configBuilder = new ConfigurationBuilder().
               AddJsonFile("appsettings.json").Build();

            var configSection = configBuilder.GetSection("AppSettings");

            // get the configuration values in the section.
            var inputFilePath = configSection["InputFilePath"] ?? null;
            var outputFilePath = configSection["OutputFilePath"] ?? null;

            if (string.IsNullOrEmpty(inputFilePath))
            {
                Console.WriteLine("Please Write the input Path");
                inputFilePath = Console.ReadLine();
            }

            if (!File.Exists(inputFilePath))
            {
                using (FileStream fileStr = File.Create(inputFilePath))
                {

                }
            }

            // Read input file
            string[] lines = File.ReadAllLines(inputFilePath);

            if (!lines.Any())
            {
                Console.WriteLine("File is empty");
                Console.ReadKey();
                return;
            }

            List<Drone> drones = GetDrones(lines);
            List<Location> locations = GetLocations(lines);

            string error = ValidateInput(drones, locations);
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
                Console.ReadKey();
                return;
            }

            // Optimize deliveries
            List<Delivery> deliveries = OptimizeDeliveries(drones, locations);

            // Create OuputFIles
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                writer.WriteLine("***BEGIN OUTPUT FILE***");
                foreach (var delivery in deliveries.OrderBy(x => x.DroneName))
                {
                    writer.WriteLine($"[{delivery.DroneName}]");
                    foreach (Trip trip in delivery.Trips)
                    {
                        writer.WriteLine(trip.Name);
                        writer.WriteLine(string.Join(", ", trip.Locations.Select(l => l.Name)));

                        writer.WriteLine();
                    }
                }
                writer.WriteLine("***END OUTPUT FILE***");
            }
            Console.ReadKey();
        }

        #region Methods

        private static List<Drone> GetDrones(string[] lines)
        {
            var drones = new List<Drone>();
            var inputDrones = lines[0].Split(", ");
            for (int i = 0; i < inputDrones.Length; i += 2)
            {
                string droneName = inputDrones[i];
                if (droneName.Contains("Drone"))
                {
                    drones.Add(new Drone
                    {
                        Name = droneName,
                        MaxWeight = int.Parse(inputDrones[i + 1].Trim('[', ']'))
                    });
                }
            }
            return drones;
        }

        private static List<Location> GetLocations(string[] lines)
        {
            List<Location> locations = new List<Location>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(", ");
                string locationeName = parts[0];
                if (locationeName.Contains("Location"))
                {
                    locations.Add(new Location
                    {
                        Name = locationeName,
                        PackageWeight = int.Parse(parts[1].Trim('[', ']'))
                    });
                }
            }
            return locations;
        }

        public static List<Delivery> OptimizeDeliveries(List<Drone> drones, List<Location> locations)
        {
            List<Delivery> deliveries = new();
            // Sort by maximum weight in descending order
            drones.Sort((d1, d2) => d2.MaxWeight.CompareTo(d1.MaxWeight));
            locations.Sort((l1, l2) => l2.PackageWeight.CompareTo(l1.PackageWeight));

            // Calculate efficient deliveries for all drones combined
            int tripNumber = 1;
            List<Location> remainingLocations = new List<Location>(locations);
            int maxCapacityDrone = drones.Max(d => d.MaxWeight);
            while (remainingLocations.Count > 0)
            {
                foreach (Drone drone in drones)
                {
                    List<Location> locationsForTrip = new List<Location>();
                    int totalWeight = 0;

                    foreach (Location location in remainingLocations)
                    {
                        if (totalWeight + location.PackageWeight <= drone.MaxWeight)
                        {
                            locationsForTrip.Add(location);
                            totalWeight += location.PackageWeight;
                            if (totalWeight >= drone.MaxWeight)
                            {
                                break;
                            }
                        }
                        //this is when the weight of location is greater than of max drone capacity
                        else if (location.PackageWeight > maxCapacityDrone && maxCapacityDrone == drone.MaxWeight)
                        {
                            locationsForTrip.Add(location);
                            totalWeight += drone.MaxWeight;
                            if (totalWeight >= drone.MaxWeight)
                            {
                                break;
                            }
                        }
                    }

                    foreach (Location location in locationsForTrip)
                    {
                        remainingLocations.Remove(location);
                    }

                    if (locationsForTrip.Any())
                    {
                        Trip trip = new Trip { Name = $"Trip # {tripNumber}", Locations = locationsForTrip };

                        //Add to the delivery
                        ManageDeliveries(deliveries, trip, drone.Name);
                    }

                    if (remainingLocations.Count <= 0)
                    {
                        break;
                    }
                }

                tripNumber++;
            }

            return deliveries;
        }

        private static void ManageDeliveries(List<Delivery> deliveries, Trip trip, string droneName)
        {
            //check if the delivery has the drone
            var delivery = deliveries.Where(d => d.DroneName == droneName).FirstOrDefault();
            if (delivery != null)
            {
                delivery.Trips.Add(trip);
            }
            else
            {
                List<Trip> trips = new List<Trip> { trip };
                Delivery newDelivery = new Delivery { DroneName = droneName, Trips = trips };
                deliveries.Add(newDelivery);
            }
        }

        private static string ValidateInput(List<Drone> drones, List<Location> locations)
        {
            int droneCount = drones.Count();
            if (droneCount > 100)
            {
                return "File contains more than 100 drones";
            }

            if (droneCount <= 0)
            {
                return "File not contains drones";
            }

            if (locations.Count <= 0)
            {
                return "File not contains locations";
            }

            return string.Empty;
        }
        #endregion

        #region Structure

        public class Delivery
        {
            public string DroneName { get; set; }
            public List<Trip> Trips { get; set; }
        }

        public class Trip
        {
            public string Name { get; set; }
            public List<Location> Locations { get; set; }
        }
        public class Drone
        {
            public string Name { get; set; }
            public int MaxWeight { get; set; }
        }

        public class Location
        {
            public string Name { get; set; }
            public int PackageWeight { get; set; }
        }

        #endregion
    }
}