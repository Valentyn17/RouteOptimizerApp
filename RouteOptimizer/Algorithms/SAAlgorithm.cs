using RouteOptimizer.Helpers;
using RouteOptimizer.Models;

public class SAAlgorithm
{
    private Random random = new();
    private int _iterations;
    private readonly double _temperature;
    private readonly double _alpha;

    public SAAlgorithm(int iterations = 200, double temperature = 100, double alpha = 0.97)
    {
        _iterations = iterations;
        _temperature = temperature;
        _alpha = alpha;
    }

    public List<Vehicle> OptimizeRoutes(List<Client> clients, Depot depot, int numberOfVehicles, int vehicleCapacity, List<Vehicle> initialSolution)
    {
        List<Vehicle> currentSolution = DeepCopySolution(initialSolution);
        List<Vehicle> bestSolution = DeepCopySolution(initialSolution);

        double currentCost = CalculateTotalDistance(currentSolution, depot);
        double bestCost = currentCost;
        double temperature = _temperature;
        int counter = 0;
        const int MAXCOUNTER = 60;


        for (int iteration = 0; iteration < _iterations; iteration++)
        {
            List<Vehicle> neighborSolution = GenerateNeighbor(currentSolution, vehicleCapacity, numberOfVehicles, depot);

            //List<Vehicle> crossoverSolution = PerformCrossover(currentSolution, neighborSolution, vehicleCapacity);

            if (!IsSolutionFeasible(neighborSolution, clients.Count))
            {
                continue;
            }

            double newCost = CalculateTotalDistance(neighborSolution, depot);
            double delta = newCost - currentCost;

            if (delta < 0)
            {
                currentSolution = neighborSolution;
                currentCost = newCost;

                if (newCost < bestCost)
                {
                    bestSolution = DeepCopySolution(neighborSolution);
                    bestCost = newCost;
                    counter = 0;
                }
            }
            else
            {
                double probability = Math.Exp(-delta / temperature);
                if (random.NextDouble() < probability)
                {
                    currentSolution = neighborSolution;
                    currentCost = newCost;
                }
            }

            if (counter >= MAXCOUNTER) 
            {
                break;
            }

            temperature *= _alpha;
            counter++;
        }

        return bestSolution;
    }

    private double CalculateTotalDistance(List<Vehicle> vehicles, Depot depot)
    {
        double totalDistance = 0;

        foreach (var vehicle in vehicles)
        {
            vehicle.CalculateRouteMetrics(depot);
            totalDistance += vehicle.TotalDistance;
        }

        return totalDistance;
    }

    private List<Vehicle> GenerateNeighbor(List<Vehicle> currentSolution, int vehicleCapacity, int numberOfVehicles, Depot depot)
    {
        var newSolution = DeepCopySolution(currentSolution);

        // Randomly choose an operation: 0 = Swap within route, 1 = Move between routes, 2 = Swap between routes
        int operation = random.Next(0, 3);

        if (operation == 0)
        {
            // Swap two clients within the same route
            var vehicle = newSolution[random.Next(newSolution.Count)];
            if (vehicle.Clients.Count >= 2)
            {
                int index1 = random.Next(vehicle.Clients.Count);
                int index2 = random.Next(vehicle.Clients.Count);
                while (index2 == index1)
                    index2 = random.Next(vehicle.Clients.Count);

                // Swap clients at their indices
                var tempClient = vehicle.Clients[index1];
                vehicle.Clients[index1] = vehicle.Clients[index2];
                vehicle.Clients[index2] = tempClient;

                // Recalculate route metrics
                vehicle.CalculateRouteMetrics(depot);
            }
        }
        else if (operation == 1)
        {
            // Move a client from one route to another
            if (newSolution.Count >= 2)
            {
                int sourceIndex = random.Next(newSolution.Count);
                int targetIndex = random.Next(newSolution.Count);
                while (targetIndex == sourceIndex)
                    targetIndex = random.Next(newSolution.Count);

                var sourceVehicle = newSolution[sourceIndex];
                var targetVehicle = newSolution[targetIndex];

                if (sourceVehicle.Clients.Count >= 1)
                {
                    int clientIndex = random.Next(sourceVehicle.Clients.Count);
                    var client = sourceVehicle.Clients[clientIndex];

                    if (targetVehicle.CanAccommodate(client.Quantity))
                    {
                        sourceVehicle.Clients.RemoveAt(clientIndex);
                        sourceVehicle.TotalLoad -= client.Quantity;

                        int insertIndex = clientIndex;
                        if (insertIndex > targetVehicle.Clients.Count)
                        {
                            insertIndex = targetVehicle.Clients.Count;
                        }
                        targetVehicle.Clients.Insert(insertIndex, client);
                        targetVehicle.TotalLoad += client.Quantity;

                        sourceVehicle.CalculateRouteMetrics(depot);
                        targetVehicle.CalculateRouteMetrics(depot);
                    }
                }
            }
        }
        else
        {
            // Swap clients between two routes at their respective indices
            if (newSolution.Count >= 2)
            {
                int vehicleIndex1 = random.Next(newSolution.Count);
                int vehicleIndex2 = random.Next(newSolution.Count);
                while (vehicleIndex2 == vehicleIndex1)
                    vehicleIndex2 = random.Next(newSolution.Count);

                var vehicle1 = newSolution[vehicleIndex1];
                var vehicle2 = newSolution[vehicleIndex2];

                if (vehicle1.Clients.Count >= 1 && vehicle2.Clients.Count >= 1)
                {
                    int clientIndex1 = random.Next(vehicle1.Clients.Count);
                    int clientIndex2 = random.Next(vehicle2.Clients.Count);

                    var client1 = vehicle1.Clients[clientIndex1];
                    var client2 = vehicle2.Clients[clientIndex2];

                    if (vehicle1.TotalLoad - client1.Quantity + client2.Quantity <= vehicle1.Capacity &&
                        vehicle2.TotalLoad - client2.Quantity + client1.Quantity <= vehicle2.Capacity)
                    {
                        vehicle1.Clients[clientIndex1] = client2;
                        vehicle2.Clients[clientIndex2] = client1;

                        vehicle1.TotalLoad = vehicle1.TotalLoad - client1.Quantity + client2.Quantity;
                        vehicle2.TotalLoad = vehicle2.TotalLoad - client2.Quantity + client1.Quantity;

                        vehicle1.CalculateRouteMetrics(depot);
                        vehicle2.CalculateRouteMetrics(depot);
                    }
                }
            }
        }

        while (newSolution.Count < numberOfVehicles)
        {
            int newVehicleId = newSolution.Count + 1;
            newSolution.Add(new Vehicle(newVehicleId, vehicleCapacity));
        }

        return newSolution;
    }


    private List<Vehicle> PerformCrossover(List<Vehicle> parent1, List<Vehicle> parent2, int vehicleCapacity)
    {
        var parent1ClientIds = parent1.SelectMany(v => v.Clients.Select(c => c.Id)).ToList();
        var parent2ClientIds = parent2.SelectMany(v => v.Clients.Select(c => c.Id)).ToList();

        var childClientIds = Crossover(parent1ClientIds, parent2ClientIds);

        var allClients = GetAllClientsFromParents(parent1, parent2);
        var childSolution = ReconstructRoutes(childClientIds, allClients, vehicleCapacity);

        return childSolution;
    }

    private List<int> Crossover(List<int> parent1, List<int> parent2)
    {
        int length = parent1.Count;
        int crossoverPoint = random.Next(1, length - 1);

        var child = new List<int>();

        child.AddRange(parent1.Take(crossoverPoint));

        foreach (var gene in parent2)
        {
            if (!child.Contains(gene))
            {
                child.Add(gene);
            }
        }

        foreach (var gene in parent1)
        {
            if (!child.Contains(gene))
            {
                child.Add(gene);
            }
        }

        return child;
    }

    private List<Vehicle> ReconstructRoutes(List<int> clientIds, List<Client> allClients, int vehicleCapacity)
    {
        List<Vehicle> vehicles = new List<Vehicle>();
        int vehicleId = 1;

        Vehicle currentVehicle = new Vehicle(vehicleId, vehicleCapacity);

        foreach (int clientId in clientIds)
        {
            var client = allClients.First(c => c.Id == clientId);

            if (currentVehicle.TotalLoad + client.Quantity <= currentVehicle.Capacity)
            {
                currentVehicle.AddClient(client.DeepCopy());

            }
            else
            {
                vehicles.Add(currentVehicle);
                vehicleId++;

                currentVehicle = new Vehicle(vehicleId, vehicleCapacity);

                currentVehicle.Clients.Add(client.DeepCopy());
                currentVehicle.AddClient(client.DeepCopy());
            }
        }

        if (currentVehicle.Clients.Count > 0)
        {
            vehicles.Add(currentVehicle);
        }

        return vehicles;
    }

    private List<Client> GetAllClientsFromParents(List<Vehicle> parent1, List<Vehicle> parent2)
    {
        var clientDict = new Dictionary<int, Client>();

        foreach (var vehicle in parent1)
        {
            foreach (var client in vehicle.Clients)
            {
                if (!clientDict.ContainsKey(client.Id))
                {
                    clientDict[client.Id] = client.DeepCopy();
                }
            }
        }

        foreach (var vehicle in parent2)
        {
            foreach (var client in vehicle.Clients)
            {
                if (!clientDict.ContainsKey(client.Id))
                {
                    clientDict[client.Id] = client.DeepCopy();
                }
            }
        }

        return clientDict.Values.ToList();
    }

    private bool IsSolutionFeasible(List<Vehicle> solution, int totalClients)
    {
        var assignedClientIds = new HashSet<int>();
        foreach (var vehicle in solution)
        {
            foreach (var client in vehicle.Clients)
            {
                if (!assignedClientIds.Add(client.Id))
                {
                    return false;
                }
            }
        }

        return assignedClientIds.Count == totalClients;
    }

    private List<Vehicle> DeepCopySolution(List<Vehicle> vehicles)
    {
        return vehicles.Select(vehicle => vehicle.DeepCopy()).ToList();
    }
}



