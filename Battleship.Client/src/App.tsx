import { useEffect, useState } from 'react'
import { connection, startConnection } from './signalR'
import JoinGame from './JoinGame'
import CreateGame from './CreateGame'
import Home from './Home'

type Screen = 'home' | 'create'

function App() {
    const [screen, setScreen] = useState<Screen>('home')
    const [gameCode, setGameCode] = useState<string | null>(null)
    const [playerId] = useState(() => crypto.randomUUID())

    useEffect(() => {
        startConnection()
            .then(() => console.log('SignalR connected'))
            .catch(console.error)
    }, [])
    
    useEffect(() => {
        connection.on('GameCreated', message => {
            console.log('GameCreated:', message)
        })

        return () => {
            connection.off('GameCreated')
        }
    }, [])
    
    if (screen === 'create') {
        return (
            <div>
                <h1>{gameCode}</h1>

                <button onClick={() => setScreen('home')}>
                    Back
                </button>
            </div>
        )
    }

    return (
        <div>
            <Home />
            <CreateGame
                playerId={playerId}
                onGameCreated={(code) => {
                    setGameCode(code);
                    setScreen('create');
                }}
            />
            <JoinGame playerId={playerId} />
        </div>
    )
}

export default App