import { useEffect, useState } from 'react'
import { connection, startConnection } from './signalR'

type Screen = 'home' | 'create'

function App() {
    const [screen, setScreen] = useState<Screen>('home')
    const [gameCode, setGameCode] = useState<string | null>(null)
    const [joinCode, setJoinCode] = useState('')
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

    async function joinGame() {
        const joined = await connection.invoke<boolean>(
            'JoinLobby',
            {
                gameCode: joinCode,
                playerId,
                playerName: 'Player 2'
            }
        )

        console.log('joined:', joined)
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

            <div>
                <input
                    value={joinCode}
                    onChange={e => setJoinCode(e.target.value)}
                    placeholder="Game code"
                />

                <button onClick={joinGame}>
                    Join Game
                </button>
            </div>
        </div>
    )
}

export default App