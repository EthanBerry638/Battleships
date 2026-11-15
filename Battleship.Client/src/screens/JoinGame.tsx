import { useState } from 'react';
import { connection } from '../signalR';
import type { ConnectionStatus } from '../signalR';

interface JoinGameProps {
    playerId: string;
    playerName: string;
    onBack: () => void;
    connectionStatus: ConnectionStatus;
}

function JoinGame({ playerId, playerName, onBack, connectionStatus }: JoinGameProps) {
    const [joinCode, setJoinCode] = useState('');
    const [message, setMessage] = useState<string | null>(null);
    const [isJoining, setIsJoining] = useState(false);

    const canJoin = joinCode.trim().length > 0;
    const canClick = canJoin && connectionStatus === 'connected' && !isJoining;

    async function joinGame() {
        setMessage(null);
        setIsJoining(true);

        try {
            const joined = await connection.invoke<boolean>(
                'JoinLobby',
                {
                    gameCode: joinCode.trim().toUpperCase(),
                    playerId,
                    playerName: playerName.trim(),
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
        } finally {
            setIsJoining(false);
        }
    }

    return (
        <div className="page-container">
            <div className="card">
                <h1>Join Game</h1>
                <input
                    value={joinCode}
                    onChange={(e) => setJoinCode(e.target.value)}
                    placeholder="Game code"
                />
    
                <button
                    onClick={joinGame}
                    disabled={!canClick}
                >
                    Join
                </button>
    
                {message && <p>{message}</p>}
    
                <button onClick={onBack}>Back</button>
            </div>
        </div>
    );
}

export default JoinGame;