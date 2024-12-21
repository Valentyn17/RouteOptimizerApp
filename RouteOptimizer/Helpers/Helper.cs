using RouteOptimizer.Models;
using System.Globalization;

namespace RouteOptimizer.Helpers
{
    public static class Helper
    {
        public async static Task<RequestAlgorithmModel> GetDataFromFile(IFormFile file)
        {
            var clients = new List<Client>();
            Depot depot = null;
            int numberOfVehicles = 0;
            int vehicleCapacity = 0;
            CultureInfo culture =  new CultureInfo("uk-UA");
        
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                int lineNumber = 0;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Split the line by whitespace
                    var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (lineNumber == 1)
                    {
                        // First line: Number of vehicles and vehicle capacity
                        if (parts.Length != 2)
                            throw new Exception($"Invalid format in line {lineNumber}. Expected 2 values.");

                        if (!int.TryParse(parts[0], out numberOfVehicles))
                            throw new Exception($"Invalid number of vehicles in line {lineNumber}.");

                        if (!int.TryParse(parts[1], out vehicleCapacity))
                            throw new Exception($"Invalid vehicle capacity in line {lineNumber}.");
                    }
                    else if (lineNumber == 2)
                    {
                        // Second line: Depot information (ID, Latitude, Longitude)
                        if (parts.Length != 3)
                            throw new Exception($"Invalid format in line {lineNumber}. Expected 3 values.");

                        if (!int.TryParse(parts[0], out int depotId))
                            throw new Exception($"Invalid depot ID in line {lineNumber}.");

                        if (!double.TryParse(parts[1], culture, out double depotLatitude))
                            throw new Exception($"Invalid depot latitude in line {lineNumber}.");

                        if (!double.TryParse(parts[2], culture, out double depotLongitude))
                            throw new Exception($"Invalid depot longitude in line {lineNumber}.");

                        depot = new Depot
                        {
                            Id = depotId,
                            Latitude = depotLatitude,
                            Longitude = depotLongitude
                        };
                    }
                    else
                    {
                        // Remaining lines: Client information (ID, Latitude, Longitude, Quantity)
                        if (parts.Length != 4)
                            throw new Exception($"Invalid format in line {lineNumber}. Expected 4 values.");

                        if (!int.TryParse(parts[0], out int clientId))
                            throw new Exception($"Invalid client ID in line {lineNumber}.");

                        if (!double.TryParse(parts[1],culture, out double clientLatitude))
                            throw new Exception($"Invalid client latitude in line {lineNumber}.");

                        if (!double.TryParse(parts[2], culture, out double clientLongitude))
                            throw new Exception($"Invalid client longitude in line {lineNumber}.");

                        if (!double.TryParse(parts[3], culture, out double clientQuantity))
                            throw new Exception($"Invalid client quantity in line {lineNumber}.");

                        var client = new Client(clientId, clientLatitude, clientLongitude, clientQuantity);

                        clients.Add(client);
                    }
                }
            }

            if (depot == null)
                throw new Exception("Depot information is missing or invalid.");

            if (clients.Count == 0)
                throw new Exception("No clients found in the data file.");

            var request = new RequestAlgorithmModel
            {
                Depot = depot,
                Clients = clients.ToArray(),
                NumberOfVehicles = numberOfVehicles,
                VehicleCapacity = vehicleCapacity
            };

            return request;
        }

        public static double GetDistance(Location p1, Location p2)  // Count distance in kilometers using math equation
        {
            const int KilometersPerDegree = 111;

            double latitudeDiff = Math.Abs(p1.Latitude - p2.Latitude);
            double longitudeDiff = Math.Abs(p1.Longitude - p2.Longitude);

            double latitudeAvr = (p1.Latitude + p2.Latitude) / 2;
            double averageLatitudeRadian = latitudeAvr * Math.PI / 180;
            double distanceLatitude = latitudeDiff * KilometersPerDegree;
            double distanceLongitude = longitudeDiff * KilometersPerDegree * Math.Cos(distanceLatitude);


            return Math.Sqrt(Math.Pow(distanceLatitude, 2) + Math.Pow(distanceLongitude, 2));
        }
    }
}
