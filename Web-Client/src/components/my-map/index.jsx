// Map.js
import React, { useState, useEffect, useRef} from 'react';
import { GoogleMap} from '@react-google-maps/api';
import PropTypes from 'prop-types';

const containerStyle = {
  width: '100%',
  height: '500px'
};

// Default center (Kyiv)
const defaultCenter = {
  lat: 50.40,
  lng: 30.47
};

const routeColors = ['#FF0000', '#0000FF', '#008000', '#FFA500', '#800080'];

const Map = ({ depot, vehicles, alternativeRouteIndex }) => {
  const [map, setMap] = useState(null);
  //const [activeMarker, setActiveMarker] = useState(null);
  const [center, setCenter] = useState(defaultCenter);

  const markersRef = useRef([]);
  const polylinesRef = useRef([]);

  useEffect(() => {
    if (depot) {
        setCenter({
            lat: depot.latitude,
            lng: depot.longitude
        });
    } else {
        setCenter(defaultCenter);
    }
}, [depot]);

useEffect(() => {
    if (map) {
      // Clear existing markers
      markersRef.current.forEach(marker => marker.setMap(null));
      markersRef.current = [];

      // Clear existing polylines
      polylinesRef.current.forEach(polyline => polyline.setMap(null));
      polylinesRef.current = [];

      // Add depot marker
      if (depot) {
        const depotMarker = new window.google.maps.Marker({
          position: { lat: depot.latitude, lng: depot.longitude },
          map,
          label: 'Depot',
        });
        markersRef.current.push(depotMarker);
      }

      // Add vehicle routes and client markers
      vehicles.forEach((vehicle, index) => {
        if (!vehicle.clients || vehicle.clients.length === 0) {
          return; // Skip if no clients
        }

        const path = [
          { lat: depot.latitude, lng: depot.longitude },
          ...vehicle.clients.map(client => ({
            lat: client.latitude,
            lng: client.longitude
          })),
          { lat: depot.latitude, lng: depot.longitude }
        ];

        // Create polyline for the route
        const polyline = new window.google.maps.Polyline({
          path: path,
          geodesic: true,
          strokeColor: routeColors[index % routeColors.length],
          strokeOpacity: 1.0,
          strokeWeight: 3,
          map,
        });
        polylinesRef.current.push(polyline);

        // Add client markers
        vehicle.clients.forEach(client => {
          const clientMarker = new window.google.maps.Marker({
            position: {
              lat: client.latitude,
              lng: client.longitude
            },
            map,
            label: `${client.id}`,
          });
          markersRef.current.push(clientMarker);
        });
      });
    }
  }, [map, depot, vehicles]);


  return (
    <GoogleMap
      mapContainerStyle={containerStyle}
      center={center}
      zoom={6}
      onLoad={mapInstance => setMap(mapInstance)}
    >
      {/* No need to render markers or polylines via React components */}
    </GoogleMap>
  );

/*
  return (
    <GoogleMap
        mapContainerStyle={containerStyle}
        center={center}
        zoom={6}
    >
        {depot && (
            <Marker
                position={{
                    lat: depot.latitude,
                    lng: depot.longitude
                }}
                label="Depot"
                onClick={() => setActiveMarker('depot')}
            >
                {activeMarker === 'depot' && (
                    <InfoWindow
                        position={{
                            lat: depot.latitude,
                            lng: depot.longitude
                        }}
                        onCloseClick={() => setActiveMarker(null)}
                    >
                        <div className="info-window">
                            <p><strong>Depot</strong></p>
                        </div>
                    </InfoWindow>
                )}
            </Marker>
        )}
        {vehicles && vehicles.map((vehicle, index) => {
          if (!vehicle.clients || vehicle.clients.length === 0) {
            return null; // Skip if no clients
          }
            const path = [
                { lat: depot.latitude, lng: depot.longitude },
                ...vehicle.clients.map(client => ({
                    lat: client.latitude,
                    lng: client.longitude
                })),
                { lat: depot.latitude, lng: depot.longitude }
            ];

            return (
                <React.Fragment key={`${alternativeRouteIndex}-${vehicle.id}`}>
                    <Polyline
                        key={`${alternativeRouteIndex}-${vehicle.id}`}
                        path={path}
                        options={{
                            strokeColor: routeColors[index % routeColors.length],
                            strokeWeight: 3
                        }}
                    />

                    {vehicle.clients.map(client => (
                        <Marker
                            key={`${alternativeRouteIndex}-${vehicle.id}`}
                            position={{
                                lat: client.latitude,
                                lng: client.longitude
                            }}
                            label={`${client.id}`}
                            onClick={() => setActiveMarker(`client-${client.id}`)}
                        >
                            {activeMarker === `client-${client.id}` && (
                                <InfoWindow
                                    position={{
                                        lat: client.latitude,
                                        lng: client.longitude
                                    }}
                                    onCloseClick={() => setActiveMarker(null)}
                                >
                                    <div className="info-window">
                                        <p><strong>Client ID:</strong> {client.id}</p>
                                        <p><strong>Quantity:</strong> {client.quantity}</p>
                                    </div>
                                </InfoWindow>
                            )}
                        </Marker>
                    ))}
                </React.Fragment>
            );
        })}
    </GoogleMap>
);*/
};

Map.propTypes = {
  depot: PropTypes.shape({
      latitude: PropTypes.number.isRequired,
      longitude: PropTypes.number.isRequired
  }),
  vehicles: PropTypes.arrayOf(PropTypes.shape({
      id: PropTypes.number.isRequired,
      clients: PropTypes.arrayOf(PropTypes.shape({
          id: PropTypes.number.isRequired,
          latitude: PropTypes.number.isRequired,
          longitude: PropTypes.number.isRequired,
          quantity: PropTypes.number.isRequired
      })).isRequired
  })).isRequired
};

export default Map;
