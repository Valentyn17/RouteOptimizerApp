using RouteOptimizer.Helpers;

namespace RouteOptimizer.Models
{
    public class Vehicle
    {
        public int Id { get; private set; }
        public double Capacity { get; private set; }
        public List<Client> Clients { get; private set; }
        public double TotalLoad { get;  set; }
        public double TotalDistance { get; private set; }

        public Vehicle(int id, double capacity)
        {
            Id = id;
            Capacity = capacity;
            Clients = new List<Client>();
            TotalLoad = 0;
            TotalDistance = 0;
        }
        public Vehicle()
        {

        }

        public Vehicle DeepCopy()
        {
            // Create a new Vehicle instance
            Vehicle newVehicle = new Vehicle
            {
                Id = this.Id,
                TotalDistance = this.TotalDistance,
                TotalLoad = this.TotalLoad,
                Capacity = this.Capacity,
                // Deep copy the Clients list
                Clients = this.Clients.Select(client => client.DeepCopy()).ToList()
                // Copy other properties if necessary
            };
            return newVehicle;
        }
        public void ClearClients()
        {
            Clients.Clear();
            TotalLoad = 0;
            TotalDistance = 0; // Очистка TotalDistance при очищении клиентов
        }

        public bool CanAccommodate(double quantity)
        {
            return TotalLoad + quantity <= Capacity;
        }

        public void AddClient(Client client)
        {
            if (CanAccommodate(client.Quantity) && !Clients.Contains(client))
            {
                Clients.Add(client);
                TotalLoad += client.Quantity;
                CalculateRouteMetrics(new Depot()); // Обновляем расстояние после добавления клиента
            }
        }

        public void RemoveClient(Client client)
        {
            if (Clients.Contains(client))
            {
                Clients.Remove(client);
                TotalLoad -= client.Quantity;
                CalculateRouteMetrics(new Depot()); // Обновляем расстояние после удаления клиента
            }
        }

        public void SetClients(List<Client> clientsList)
        {
            Clients = clientsList;
            CalculateRouteMetrics(new Depot());
        }

        public void CalculateRouteMetrics(Location depot)
        {
            TotalDistance = 0;
            Location previousPoint = depot;

            foreach (var client in Clients)
            {
                TotalDistance += Helper.GetDistance(previousPoint, client);
                previousPoint = client;
            }

            TotalDistance += Helper.GetDistance(previousPoint, depot);
        }
    }
}
