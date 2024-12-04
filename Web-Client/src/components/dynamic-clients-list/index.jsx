import React, { useState, useEffect } from 'react';
import LocationSearch from '../location-search';
import './styles.css';
import { FiPlus } from "react-icons/fi";
import { RiDeleteBin7Line } from "react-icons/ri";

const DynamicClientsList = ({ onClientsChange, disabled }) => {
  const [clients, setClients] = useState([{ place: null, quantity: '' }]);

  const handleClientSelected = (place, index) => {
    const updatedClients = [...clients];
    updatedClients[index].place = place;
    setClients(updatedClients);
  };

  const handleQuantityChange = (quantity, index) => {
    const updatedClients = [...clients];
    updatedClients[index].quantity = quantity;
    setClients(updatedClients);
  };

  const addClient = () => {
    setClients([...clients, { place: null, quantity: '' }]);
  };


  const removeClient = (index) => {
    if (clients.length > 1) {
      const updatedClients = clients.filter((_, i) => i !== index);
      setClients(updatedClients);
    }
  };

  useEffect(() => {
    const validClients = clients.filter(
      (client) => client.place && client.quantity
    );
    onClientsChange(validClients);
  }, [clients]);

  return (
    <div className='clients'>
      {clients.map((client, index) => (
        <div key={index} className='client'>
          <div className='client-inputs'>
            <LocationSearch
              onPlaceSelected={(place) => handleClientSelected(place, index)} disabled={disabled}
            />
            <input
              type='number'
              min='1'
              placeholder='Quantity'
              value={client.quantity}
              onChange={(e) => handleQuantityChange(e.target.value, index)}
              required={!disabled}
              disabled={disabled}
            />
          </div>
          <button
            type="button"
            onClick={addClient}
            className='add-client-button'
            aria-label="Add Client"
          >
            <FiPlus />
          </button>
          <button
            type="button"
            onClick={() => removeClient(index)}
            className='remove-client-button'
            aria-label="Remove Client"
          >
            <RiDeleteBin7Line />
          </button>
        </div>
      ))}
    </div>
  );
};

export default DynamicClientsList;
