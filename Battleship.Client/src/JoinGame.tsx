import { useState } from 'react';
import { connection } from './signalR';

interface JoinGameProps {
    playerId: string;
}

function JoinGame({ playerId }: JoinGameProps) {
    const [joinCode, setJoinCode] = useState('');
    
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
    
    return (
        <div>
            <input
                value={joinCode}
                onChange={(e) => setJoinCode(e.target.value)}
                placeholder="Game code"/>
            
            <button onClick={joinGame}>Join</button>
        </div>
    )
}

export default JoinGame;