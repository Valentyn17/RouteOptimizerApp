namespace RouteOptimizer.Models
{
    public abstract class Location
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
