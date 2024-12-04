namespace RouteOptimizer.Models
{
    public class Client : Location
    {
        public int Id { get; set; }
        public double Quantity { get; set; }
        public bool IsVisible { get; set; } = true; // Поле, що вказує на доступність клієнта

        public Client(int id, double latitude, double longitude, double quantity)
        {
            Id = id;
            Latitude = latitude;
            Longitude = longitude;
            Quantity = quantity;
            IsVisible = true; // Спочатку всі клієнти доступні
        }

        public Client()
        {

        }

        public Client DeepCopy()
        {
            // Create a new Client instance
            Client newClient = new Client
            {
                Id = this.Id,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Quantity = this.Quantity,
                IsVisible = this.IsVisible
            };
            return newClient;
        }

        // Метод для зміни статусу видимості клієнта
        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        // Перевизначення Equals для правильного порівняння клієнтів (наприклад, для HashSet)
        public override bool Equals(object obj)
        {
            if (obj is Client otherClient)
            {
                return Id == otherClient.Id;
            }
            return false;
        }

        // Перевизначення GetHashCode для використання в HashSet або інших колекціях
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        // Метод для отримання текстового представлення клієнта
        public override string ToString()
        {
            return $"Клієнт {Id} (Локація: {Latitude}; {Longitude}, Вантаж: {Quantity})";
        }
    }
}
