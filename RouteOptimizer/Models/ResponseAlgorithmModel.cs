namespace RouteOptimizer.Models
{
    public class ResponseAlgorithmModel
    {
        public Depot Depot { get; set; }
        public Vehicle[] Vehicles { get; set; } = new Vehicle[0];
        public List<List<Vehicle>> AlternativeRoutes { get; set; } = new List<List<Vehicle>>();
    }
}
