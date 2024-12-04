namespace RouteOptimizer.Models
{
    public class RequestAlgorithmModel
    {
        public Depot Depot { get; set; } = new Depot();
        public Client[] Clients { get; set; } = new Client[0];
        public int NumberOfVehicles { get; set; } = 1;
        public int VehicleCapacity { get; set; } = 100;
    }
}
