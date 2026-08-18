import { useEffect, useState } from 'react'
import { connection, startConnection } from './signalR'
import JoinGame from './JoinGame'

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

    async function createGame() {
        const code = await connection.invoke<string>(
            'CreateLobby',
            {
                playerId,
                playerName: 'Player 1'
            }
        )

        setGameCode(code)
        setScreen('create')
    }
    
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
            <h1>Battleship</h1>

            <button onClick={createGame}>
                Create Game
            </button>
            
            <JoinGame playerId={playerId}/>
        </div>
    )
}

export default App