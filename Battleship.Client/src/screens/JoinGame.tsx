import { useState } from 'react';
import { connection } from '../signalR';

interface JoinGameProps {
    playerId: string;
    playerName: string;
    onBack: () => void;
}

function JoinGame({ playerId, playerName, onBack }: JoinGameProps) {
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
                    playerName: playerName,
                }
            );

            if (!joined) {
                setMessage('Game not found.');
                return;
            }

            setMessage('Joined game successfully.');
        } catch (error: unknown) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            
            if (errorMessage.includes('already in an active lobby or game')) {
                setMessage('You are already in an active lobby or game.');
            }
            else {
                setMessage('An error occurred while joining the game.');
            }
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