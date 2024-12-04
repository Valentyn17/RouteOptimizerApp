using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RouteOptimizer.Algorithms;
using RouteOptimizer.Helpers;
using RouteOptimizer.Models;

namespace RouteOptimizer.Controllers
{
    [ApiController]
    [Route("api/v1/optimize")]
    public class Controller : ControllerBase
    {
        private readonly NearestNeighbourAlgorithm _NNAlgorithm;
        private readonly GeneticAlgorithm _geneticAlgorithm;
        private readonly SAAlgorithm _SAAlgorithm;

        public Controller()
        {
            _NNAlgorithm = new NearestNeighbourAlgorithm();
            _SAAlgorithm = new SAAlgorithm();
            _geneticAlgorithm = new GeneticAlgorithm();
        }

        [HttpPost("with-algorithm")]
        public async Task<IActionResult> OptimizeWithAlgorithm([FromBody] RequestAlgorithmModel request)
        {
            // Generate route
            List<Client> clients = new List<Client>(request.Clients);

            var routes = _NNAlgorithm.OptimizeRoutes(clients, request.Depot, request.NumberOfVehicles, request.VehicleCapacity);

            // Return the JSON response
            var response = new ResponseAlgorithmModel()
            {
                Depot = request.Depot,
                Vehicles = routes.First().ToArray(),
                AlternativeRoutes = routes.Skip(1).ToList(),
            };

            return Ok(response);
        }

        [HttpPost("from-file")]
        public async Task<IActionResult> OptimizeFromFile(IFormFile dataFile)
        {
            RequestAlgorithmModel request = new RequestAlgorithmModel();
            try
            {
                request = await Helper.GetDataFromFile(dataFile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Generate route
            List<Client> clients = new List<Client>(request.Clients);

            List<List<Vehicle>> routes = new List<List<Vehicle>>();
            try
            {
                routes = _geneticAlgorithm.OptimizeRoutes(clients, request.Depot, request.NumberOfVehicles, request.VehicleCapacity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            // Return the JSON response
            var response = new ResponseAlgorithmModel()
            {
                Depot = request.Depot,
                Vehicles = routes.First().ToArray(),
                AlternativeRoutes = routes.Skip(1).ToList(),
            };

            return Ok(response);
        }

        [HttpPost("specific-route")]
        public async Task<IActionResult> OptimizeSpecificRoute([FromBody] RequestAlgorithmModel request)
        {
            // Generate route
            List<Client> clients = new List<Client>(request.Clients);

            var routes = _NNAlgorithm.OptimizeRoutes(clients, request.Depot, request.NumberOfVehicles, request.VehicleCapacity);

            // Return the JSON response
            var response = new ResponseAlgorithmModel()
            {
                Depot = request.Depot,
                Vehicles = routes.First().ToArray(),
                AlternativeRoutes = routes.Skip(1).ToList(),
            };

            return Ok(response);
        }

    }

}
