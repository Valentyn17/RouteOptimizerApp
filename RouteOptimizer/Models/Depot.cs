namespace RouteOptimizer.Models
{
    public class Depot : Location
    {
        public Depot(double latitude, double longitude)
        {
            Id = 0;
            Latitude = latitude;
            Longitude = longitude;
        }

        public Depot()
        {
            Id = 0;
            Latitude = 0;
            Longitude = 0;
        }
    }
}
