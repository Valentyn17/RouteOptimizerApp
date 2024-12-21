import Form from './components/form';
import Map from './components/my-map';
import Result from './components/result';
import { LoadScript } from '@react-google-maps/api';
import './app.css';
import { useState, useRef } from 'react';

const libraries = ['places'];
const apiKey ='Your_APIKEY';

function App() {
  const [depot, setDepot] = useState(null);
  const [vehicles, setVehicles] = useState([]);
  const [alternativeRoutes, setAlternativeRoutes] = useState([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState(null);
  const [alternativeRouteIndex, setAlternativeRouteIndex] = useState(-1);
  const [clientNames, setClientNames] = useState([]);
  const [mapKey, setMapKey] = useState(0);
  const [isLoading, setIsLoading] = useState(false); 
  const [error, setError] = useState(null);

  const mapRef = useRef(null);

  const handleResultReceived = (result, clientNames) => {
    if (result) {
      setDepot(result.depot);
      setVehicles(result.vehicles || []);
      setAlternativeRoutes(result.alternativeRoutes || []);
      setClientNames(clientNames || []);
      setAlternativeRouteIndex(0);
    }
  }

     // Handler to clear the map and reset states
  const handleClear = () => {
      setDepot(null);
      setVehicles([]);
      setAlternativeRoutes([]);
      setMapKey((prevKey) => prevKey + 1);
  };

  const handleGenerateAlternativeRoute = () => {
    if (alternativeRoutes.length > 0) {  
      const nextIndex = (alternativeRouteIndex + 1) % alternativeRoutes.length;
        setVehicles(alternativeRoutes[nextIndex]);
        setAlternativeRouteIndex(nextIndex);
    } else {
        console.log("No alternative routes available");
    }
  };

  const handleRegenerateRoute = () => {
    if (selectedVehicleId !== null) {
      regenerateVehicleRoute(selectedVehicleId);
    } else {
      alert('Please select a vehicle to regenerate its route.');
    }
  };

  const regenerateVehicleRoute = (vehicleId) => {
    const vehicleToRegenerate = vehicles.find((v) => v.id === vehicleId);
    if (!vehicleToRegenerate) {
      alert('Vehicle not found.');
      return;
    }

    // Get the clients assigned to this vehicle
    const clients = vehicleToRegenerate.clients;

    // Perform the nearest neighbor algorithm
    const regeneratedRouteClients = nearestNeighbor(depot, clients);

    const totalDistance = countTotalDistance(depot, regeneratedRouteClients);
    // Update the vehicle's clients with the regenerated route
    const updatedVehicles = vehicles.map((vehicle) => {
      if (vehicle.id === vehicleId) {
        return {
          ...vehicle,
          clients: regeneratedRouteClients,
          totalDistance: totalDistance
        };
      } else {
        return vehicle;
      }
    });

    setVehicles(updatedVehicles);
  };

  const nearestNeighbor = (depot, clients) => {
    if (!depot || clients.length === 0) {
      return clients;
    }
    const variationFactor = 0.3;
    const unvisited = [...clients];
    const route = [];
    let currentLocation = { latitude: depot.latitude, longitude: depot.longitude };
  
    while (unvisited.length > 0) {
      // Calculate distances to all unvisited clients
      const distances = unvisited.map(client => ({
        client,
        distance: getDistance(currentLocation, {
          latitude: client.latitude,
          longitude: client.longitude,
        }),
      }));
  
      // Sort clients by distance
      distances.sort((a, b) => a.distance - b.distance);
  
      // Determine the number of top nearest neighbors to consider
      const k = Math.max(1, Math.floor(variationFactor * unvisited.length));
  
      // Get the top k nearest clients
      const candidates = distances.slice(0, k);
  
      // Randomly select one of the candidates
      const selected = candidates[Math.floor(Math.random() * candidates.length)].client;
  
      // Remove the selected client from unvisited
      const selectedIndex = unvisited.findIndex(c => c.id === selected.id);
      unvisited.splice(selectedIndex, 1);
  
      // Add to the route
      route.push(selected);
      currentLocation = {
        latitude: selected.latitude,
        longitude: selected.longitude,
      };
    }
  
    return route;
  };

  const countTotalDistance = (depot, clients) => {
    if (!depot || clients.length === 0) {
      return 0;
    }
  
    let totalDistance = 0;
    let previousLocation = { latitude: depot.latitude, longitude: depot.longitude };
  
    // Calculate distance from depot to first client
    totalDistance += getDistance(previousLocation, {
      latitude: clients[0].latitude,
      longitude: clients[0].longitude,
    });
  
    // Calculate distances between clients
    for (let i = 1; i < clients.length; i++) {
      const currentClient = clients[i];
      const previousClient = clients[i - 1];
  
      totalDistance += getDistance(
        { latitude: previousClient.latitude, longitude: previousClient.longitude },
        { latitude: currentClient.latitude, longitude: currentClient.longitude }
      );
    }
  
    // Calculate distance from last client back to depot
    totalDistance += getDistance(
      { latitude: clients[clients.length - 1].latitude, longitude: clients[clients.length - 1].longitude },
      { latitude: depot.latitude, longitude: depot.longitude }
    );

    return totalDistance; // Total distance in kilometers
  };


  const getDistance = (loc1, loc2) => {
    const R = 6371; // Radius of the Earth in km
    const dLat = deg2rad(loc2.latitude - loc1.latitude);
    const dLon = deg2rad(loc2.longitude - loc1.longitude);
    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(deg2rad(loc1.latitude)) *
        Math.cos(deg2rad(loc2.latitude)) *
        Math.sin(dLon / 2) *
        Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const distance = R * c;
    return distance;
  };

  const deg2rad = (deg) => {
    return deg * (Math.PI / 180);
  };



  return (
    <LoadScript googleMapsApiKey={apiKey} libraries={libraries} language="uk">
      <div className='app'>
        <section className='form'>
          <Form onResultReceived={handleResultReceived}/>
        </section>
        {isLoading && <div className='loading'>Loading map data...</div>}
        {error && <div className='error-message'>{error}</div>}
        <section className='result controls-section'>
          <Map key={mapKey} depot={depot} vehicles={vehicles} alternativeRouteIndex ={alternativeRouteIndex}/>
          {vehicles.length > 0 && (
            <>
              <button onClick={handleGenerateAlternativeRoute} className='alternative-route-button'>Generate Alternative Route</button>
              <div className='vehicle-selection'>
                <label htmlFor='vehicle-select'>Select Vehicle:</label>
                <select
                  id='vehicle-select'
                  value={selectedVehicleId || ''}
                  onChange={(e) => setSelectedVehicleId(parseInt(e.target.value))}
                >
                  <option value=''>--Select a Vehicle--</option>
                  {vehicles.map((vehicle) => (
                  <option key={vehicle.id} value={vehicle.id}>
                    Vehicle {vehicle.id}
                  </option>
                ))}
                </select>
          </div>
              <button onClick={handleRegenerateRoute} className='regenerate-route-button'>Regenerate Route</button>
            </>
          )}
          <button onClick={handleClear} className='clear-button'>Clear Map</button>
        </section>
        <section className='result result-output'>
          <Result depot={depot} vehicles={vehicles} clientNames={clientNames} />
        </section>
      </div>
    </LoadScript>
  );
}

export default App;
