import { useEffect } from 'react'
import { startConnection } from './signalR'

function App() {
    useEffect(() => {
        startConnection()
            .then(() => console.log('SignalR connected'))
            .catch(console.error)
    }, [])

    return (
        <div>
            <h1>Battleship</h1>
            <button>Create Game</button>
            <button>Join Game</button>
        </div>
    )
}

export default App