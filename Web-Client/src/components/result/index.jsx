import './styles.css';
 
const Result = ({ depot, vehicles, clientNames }) => {
    
    const TotalDistance = vehicles.reduce((accumulator, vehicle) => {
        return accumulator + vehicle.totalDistance;
      }, 
    0);

    if (!vehicles || vehicles.length === 0) {
        return null;
    }
    
    return (
        <div className='result-container'>
            <h2>Optimized Routes</h2>
            <p><strong>Total Distance:</strong> {TotalDistance.toFixed(2)} km</p>
            <div className='result-list'>
            {vehicles.map((vehicle) => (
                <div key={vehicle.id} className='vehicle-result'>
                    <h3>Vehicle {vehicle.id}</h3>
                    <p><strong>Total Load:</strong> {vehicle.totalLoad}</p>
                    <p><strong>Total Distance:</strong> {vehicle.totalDistance.toFixed(2)} km</p>
                    <p><strong>Route steps:</strong> </p>
                    <ol>
                        <li>Depot {clientNames.length > 0 ? clientNames[0] : 'Start'}</li>
                        {vehicle.clients.map((client) => (
                            <li key={client.id}>
                                Client: {clientNames.length > 0 ? clientNames[client.id] : client.id} - Quantity: {client.quantity}
                            </li>
                        ))}
                        <li>Depot {clientNames.length > 0 ? clientNames[0] : 'End'}</li>
                    </ol>
                </div>
            ))}
            </div>
        </div>
    );

}

export default Result;