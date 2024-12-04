using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms
{
    public interface IAlgorithm
    {
            List<List<Vehicle>> OptimizeRoutes(List<Client> clients, Depot depot, int numberOfVehicles, int vehicleCapacity, int numberOfRoutes = 3);
    }
}
