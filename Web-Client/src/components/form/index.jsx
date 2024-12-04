import './styles.css';
import React, { useState, useEffect } from 'react';
import LocationSearch from '../location-search';
import DynamicClientsList from '../dynamic-clients-list';
import axios from 'axios';

const Form = ({ onResultReceived }) => {
    const [depot, setDepot] = useState(null);
    const [numberOfVehicles, setNumberOfVehicles] = useState(1);
    const [vehicleCapacity, setVehicleCapacity] = useState(100);
    const [clients, setClients] = useState([]);
    const [file, setFile] = useState(null);

    const [result, setResult] = useState(null);
    const [clientNames, setClientNames] = useState([]);
    const [error, setError] = useState(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        onResultReceived(result, clientNames);
    }, [result, clientNames]);

    useEffect(() => {
        if (depot && clients.length >= 0) {
          const names = [];
          names[0] = depot.formatted_address || depot.name || '';
    
          clients.forEach((client, index) => {
            names[index + 1] = client.place?.formatted_address || client.place?.name || '';
          });
    
          setClientNames(names);
        }
    }, [depot, clients]);

    const isFormValid = () => {
        return (
          depot &&
          clients.length > 0 &&
          numberOfVehicles &&
          vehicleCapacity
        );
      };
    
    const handleSubmit = async (event) => {
        event.preventDefault();
        setError(null);

        setIsSubmitting(true);

        try {
            if (file) {
                // Submit the file
                const formData = new FormData();
                formData.append('dataFile', file);

                const response = await axios.post(
                    "https://localhost:44393/api/v1/optimize/from-file",
                    formData,
                    {
                        headers: {
                            'Content-Type': 'multipart/form-data',
                        },
                    }
                );

                setResult(response.data);
            } else {
                // Validate form inputs
                if (!isFormValid()) {
                    setError('Please fill in all required fields or choose a data file.');
                    setIsSubmitting(false);
                    return;
                }

                // Prepare form data
                const formData = {
                    depot: {
                        id: 0,
                        latitude: depot.geometry.location.lat(),
                        longitude: depot.geometry.location.lng(),
                    },
                    clients: clients.map((client, index) => ({
                        id: index + 1,
                        latitude: client.place.geometry.location.lat(),
                        longitude: client.place.geometry.location.lng(),
                        quantity: parseFloat(client.quantity),
                        isVisible: true,
                    })),
                    numberOfVehicles: parseInt(numberOfVehicles),
                    vehicleCapacity: parseFloat(vehicleCapacity),
                };

                const response = await axios.post(
                    "https://localhost:44393/api/v1/optimize/with-algorithm",
                    formData
                );

                setResult(response.data);
            }

            setError(null);
        } catch (err) {
            setError(err.response ? err.response.data : 'An error occurred');
            setResult(null);
        } finally {
            setIsSubmitting(false);
        }
    };
    

    return (
        <form onSubmit={handleSubmit} className='form-section'>
            <div>
                <div className='input-labels'>Depot</div>
                <LocationSearch onPlaceSelected={setDepot} disabled={file !== null} />
            </div>

            <div>
                <div className='input-labels'>Number of Vehicles</div>
                <input
                    type="number"
                    min="1"
                    value={numberOfVehicles}
                    onChange={(e) => setNumberOfVehicles(e.target.value)}
                    placeholder=""
                    required = {!file} 
                    disabled={file !== null}
                />
            </div>
            <div>
                <div className='input-labels'>Vehicle Capacity</div>
                <input
                    type="number"
                    min="1"
                    value={vehicleCapacity}
                    onChange={(e) => setVehicleCapacity(e.target.value)}
                    placeholder=""
                    required = {!file} 
                    disabled={file !== null}
                />
            </div>

            <div>
                <div className='input-labels'>Clients</div>
                <DynamicClientsList onClientsChange={setClients} disabled={file !== null}/>
            </div>

            <div>
                <div className='input-labels'>Or Upload Data File</div>
                <input
                    type="file"
                    accept=".txt"
                    onChange={(e) => setFile(e.target.files[0])}
                />
            </div>

            <div className='submit-button'>
                <button type='submit' disabled={isSubmitting || (!file && !isFormValid()) }>
                    {isSubmitting ? 'Submitting...' : 'Submit'}
                </button>
            </div>
        </form>
    )
}

export default Form;