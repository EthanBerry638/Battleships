import { useState } from 'react';
import { connection } from '../signalR';

interface JoinGameProps {
    playerId: string;
    onBack: () => void;
}

function JoinGame({ playerId, onBack }: JoinGameProps) {
    const [joinCode, setJoinCode] = useState('');
    const [message, setMessage] = useState<string | null>(null);

    const canJoin = joinCode.trim().length > 0;

    async function joinGame() {
        setMessage(null);

        try {
            const joined = await connection.invoke<boolean>(
                'JoinLobby',
                {
                    gameCode: joinCode,
                    playerId,
                    playerName: 'Player 2'
                }
            );

            if (!joined) {
                setMessage('Game not found.');
                return;
            }

            setMessage('Joined game successfully.');
        } catch {
            setMessage('You are already in an active lobby or game.');
        }
    }

    return (
        <div>
            <input
                value={joinCode}
                onChange={(e) => setJoinCode(e.target.value)}
                placeholder="Game code"
            />

            <button
                onClick={joinGame}
                disabled={!canJoin}
            >
                Join
            </button>

            {message && <p>{message}</p>}

            <button onClick={onBack}>Back</button>
        </div>
    );
}

export default JoinGame;