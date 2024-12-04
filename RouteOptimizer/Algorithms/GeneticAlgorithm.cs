using RouteOptimizer.Helpers;
using RouteOptimizer.Models;

namespace RouteOptimizer.Algorithms
{
    public class GeneticAlgorithm : IAlgorithm
    {
        private Random random = new();
        private bool _showAlternativeRoutes;
        private int _populationSize;
        private int _generations;
        private double _mutationProbability;

        public GeneticAlgorithm(int populationSize = 32, int generations = 1000, double mutationProbability = 0.3, bool showAlternativeRoutes = true)
        {
            _populationSize = populationSize;
            _generations = generations;
            _mutationProbability = mutationProbability;
            _showAlternativeRoutes = showAlternativeRoutes;
        }

        public List<List<Vehicle>> OptimizeRoutes(List<Client> clients, Depot depot, int numberOfVehicles, int vehicleCapacity, int numberOfRoutes = 5)
        {
            _generations = clients.Count * 20;
            List<List<Vehicle>> routes = new List<List<Vehicle>>();
            List<Vehicle> vehicles = new List<Vehicle>();

            for (int j = 0; j < numberOfVehicles; j++)
            {
                vehicles.Add(new Vehicle(j + 1, vehicleCapacity));
            }

            routes = ApplyAlgorithm(vehicles, clients, depot, vehicleCapacity, numberOfRoutes);

            return routes;
        }

        private List<List<Vehicle>> ApplyAlgorithm(List<Vehicle> vehicles, List<Client> clients, Depot depot, int vehicleCapacity, int numberOfRoutes = 5)
        {
            NearestNeighbourAlgorithm NNAlgorithm = new NearestNeighbourAlgorithm();
            List<Vehicle> route = NNAlgorithm.ApplyAlgorithm(vehicles.ToList(), clients.ToList(), depot, vehicleCapacity);
            List<List<Vehicle>> population = GenerateInitialPopulation(vehicles, clients, depot);

            population.Add(route);

            List<List<Vehicle>> result = new List<List<Vehicle>>();
            List<Vehicle> bestSolution = route.Select(vehicle => vehicle.DeepCopy()).ToList();

            for (int generation = 0; generation < _generations; generation++)
            {
                var fitnessScores = population.Select(CalculateTotalDistance).ToList();
                List<List<Vehicle>> selectedIndividuals = SelectBestIndividuals(population, fitnessScores);

                if (selectedIndividuals.Count < 2)
                {
                    break;
                }

                population = Reproduce(selectedIndividuals, depot);

                foreach (var individual in population)
                {
                    Mutate(individual, _mutationProbability, depot);
                }

                var bestInPopulation = population.OrderBy(c => CalculateTotalDistance(c)).First();
                if (CalculateTotalDistance(bestSolution) > CalculateTotalDistance(bestInPopulation))
                {
                    bestSolution = bestInPopulation.Select(vehicle => vehicle.DeepCopy()).ToList();
                }
            }

            var bestSolutions = population.Where(c => CalculateTotalDistance(c) != CalculateTotalDistance(bestSolution));

            if (bestSolutions != null)
            {
                result = bestSolutions.OrderBy(c => CalculateTotalDistance(c)).Take(numberOfRoutes).ToList();
            }

            result[0] = bestSolution;

            return result;
        }

        private List<List<Vehicle>> GenerateInitialPopulation(List<Vehicle> vehicles, List<Client> clients, Depot depot)
        {
            List<List<Vehicle>> population = new List<List<Vehicle>>();

            for (int i = 0; i < _populationSize - 1; i++)
            {
                var clonedVehicles = vehicles.Select(v => new Vehicle(v.Id, v.Capacity)).ToList();
                var shuffledClients = clients.OrderBy(c => Helper.GetDistance(depot, c)).ToList();
                var usedClients = new HashSet<int>();

                foreach (var client in shuffledClients)
                {
                    if (usedClients.Contains(client.Id)) continue;

                    var vehicle = clonedVehicles.OrderBy(c => random.Next()).FirstOrDefault(v => v.CanAccommodate(client.Quantity));
                    if (vehicle != null)
                    {
                        vehicle.AddClient(client);
                        usedClients.Add(client.Id);
                    }
                }

                foreach (var vehicle in clonedVehicles)
                {
                    vehicle.CalculateRouteMetrics(depot);
                }

                population.Add(clonedVehicles);
            }

            return population;
        }


        private double CalculateTotalDistance(List<Vehicle> vehicles)
        {
            return vehicles.Sum(v => v.TotalDistance);
        }

        private List<List<Vehicle>> SelectBestIndividuals(List<List<Vehicle>> population, List<double> fitnessScores)
        {
            int selectCount = population.Count / 2;
            var sorted = population.Zip(fitnessScores, (p, f) => new { p, f })
                                   .OrderBy(x => x.f)
                                   .Take(selectCount)
                                   .Select(x => x.p)
                                   .ToList();
            return sorted;
        }

        private List<List<Vehicle>> Reproduce(List<List<Vehicle>> selectedIndividuals, Depot depot)
        {
            var newGeneration = selectedIndividuals.ToList();

            for (int i = 0; i < selectedIndividuals.Count - 1; i += 2)
            {
                var parent1 = selectedIndividuals[i];
                var parent2 = selectedIndividuals[i + 1];

                var child1 = Crossover(parent1, parent2);
                var child2 = Crossover(parent2, parent1);

                foreach (var vehicle in child1)
                {
                    vehicle.CalculateRouteMetrics(depot);
                }

                foreach (var vehicle in child2)
                {
                    vehicle.CalculateRouteMetrics(depot);
                }

                newGeneration.Add(child1);
                newGeneration.Add(child2);
            }

            return newGeneration;
        }
        private List<Vehicle> Crossover(List<Vehicle> parent1, List<Vehicle> parent2)
        {

            var child = parent1.Select(vehicle => vehicle.DeepCopy()).ToList();
            var usedClients = new HashSet<int>();
            var unusedClients = new List<Client>();

            foreach (var vehicle in child)
            {
                var clientsFromParent1 = parent1.First(v => v.Id == vehicle.Id).Clients.ToList();
                var clientsFromParent2 = parent2.First(v => v.Id == vehicle.Id).Clients.ToList();

                var allClients = clientsFromParent1.Concat(clientsFromParent2).Distinct()
                                        .Where(c => !usedClients.Contains(c.Id))
                                        .OrderBy(c => random.Next()).ToList();

                vehicle.ClearClients();

                foreach (var client in allClients)
                {
                    if (vehicle.CanAccommodate(client.Quantity))
                    {
                        vehicle.AddClient(client);
                        usedClients.Add(client.Id); 
                    }
                    else
                    {
                        if (!usedClients.Contains(client.Id))
                        {
                            usedClients.Add(client.Id);
                            unusedClients.Add(client);
                        }
                    }
                }
            }

            int clientsCount = unusedClients.Count;
            unusedClients = unusedClients.OrderByDescending(x => x.Quantity).ToList();
            for (int i = 0; i < clientsCount; i++)
            {
                Client client = unusedClients[i];
                var vehicle = child.OrderBy(x => random.Next()).Where(c => c.CanAccommodate(client.Quantity)).FirstOrDefault();
                if (vehicle == null)
                {
                    continue;
                }
                vehicle.AddClient(client);
            }

            return child;
        }

        private void Mutate(List<Vehicle> individual, double mutationProbability, Depot depot)
        {
            if (random.NextDouble() < mutationProbability)
            {
                var vehicle1 = individual[random.Next(individual.Count)];
                var vehicle2 = individual[random.Next(individual.Count)];

                if (vehicle1.Clients.Any() && vehicle2.Clients.Any())
                {
                    var client1 = vehicle1.Clients[random.Next(vehicle1.Clients.Count)];
                    var client2 = vehicle2.Clients[random.Next(vehicle2.Clients.Count)];

                    if (!vehicle1.Clients.Contains(client2) && !vehicle2.Clients.Contains(client1))
                    {
                        if (vehicle1.CanAccommodate(client2.Quantity - client1.Quantity) &&
                            vehicle2.CanAccommodate(client1.Quantity - client2.Quantity))
                        {
                            vehicle1.RemoveClient(client1);
                            vehicle2.RemoveClient(client2);

                            vehicle1.AddClient(client2);
                            vehicle1.CalculateRouteMetrics(depot);
                            vehicle2.AddClient(client1);
                            vehicle2.CalculateRouteMetrics(depot);
                        }
                    }
                }
            }
        }
    }
}
