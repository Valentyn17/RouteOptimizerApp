using RouteOptimizer.Helpers;
using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms
{
    public class NearestNeighbourAlgorithm : IAlgorithm
    {
        private bool _showAlternativeRoutes;

        public NearestNeighbourAlgorithm(bool showAlternativeRoutes = true)
        {
            _showAlternativeRoutes = showAlternativeRoutes;
        }

        public List<List<Vehicle>> OptimizeRoutes(List<Client> clients, Depot depot, int numberOfVehicles, int vehicleCapacity, int numberOfRoutes = 3)
        {
            List<List<Vehicle>> routes = new List<List<Vehicle>>();

            for (int i = 0; i < numberOfRoutes; i++)
            {
                List<Vehicle> vehicles = new List<Vehicle>();

                for (int j = 0; j < numberOfVehicles; j++)
                {
                    vehicles.Add(new Vehicle(j + 1, vehicleCapacity));
                }

                if (i == 0)
                {
                    routes.Add(ApplyAlgorithm(vehicles, clients, depot, vehicleCapacity));
                }
                else
                {
                    routes.Add(CalculateAlternativeRoute(vehicles, clients, depot, vehicleCapacity));
                }
            }

            return routes;
        }

        public List<Vehicle> ApplyAlgorithm(List<Vehicle> vehicles, List<Client> clients, Depot depot, int vehicleCapacity)
        {
            foreach (var vehicle in vehicles)
            {
                Location currentLocation = depot;
                var usedClients = new HashSet<Client>();

                while (clients.Any(c => c.IsVisible))
                {
                    var bestClient = clients
                        .Where(c => c.IsVisible && !usedClients.Contains(c))
                        .OrderBy(c => Helper.GetDistance(currentLocation, c))
                        .FirstOrDefault(c => vehicle.CanAccommodate(c.Quantity));

                    if (bestClient != null)
                    {
                        vehicle.AddClient(bestClient);
                        usedClients.Add(bestClient);
                        bestClient.SetVisible(false);
                        currentLocation = bestClient;
                    }
                    else
                    {
                        break;
                    }
                }

                vehicle.CalculateRouteMetrics(depot); // Calculate route metrics
            }

            return vehicles;
        }

        private List<Vehicle> CalculateAlternativeRoute(List<Vehicle> vehicles, List<Client> clients, Depot depot, int vehicleCapacity)
        {
            List<Vehicle> alternativeVehiclesRoute = new List<Vehicle>();

            Random rand = new Random();

            foreach (var client in clients)
            {
                client.SetVisible(true);
            }

            foreach (Vehicle vehicle in vehicles)
            {
                Location currentLocation = depot;

                while (clients.Where(c => c.IsVisible).Count() > 0)
                {
                    List<Client> choosedClients = clients
                        .Where(c => c.IsVisible && vehicle.CanAccommodate(c.Quantity))
                        .OrderBy(c => Helper.GetDistance(currentLocation, c)).ToList();

                    var client = choosedClients.Skip(rand.Next(choosedClients.Count)).FirstOrDefault();

                    if (client != null)
                    {
                        vehicle.AddClient(client);
                        client.SetVisible(false);
                        currentLocation = client;
                    }
                    else
                    {
                        break;
                    }
                }

                vehicle.CalculateRouteMetrics(depot);

                if (vehicle.Clients.Count > 0)
                {
                    alternativeVehiclesRoute.Add(vehicle);
                }
            }

            return alternativeVehiclesRoute;
        }
    }
}
