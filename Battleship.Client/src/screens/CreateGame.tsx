import { useState } from 'react';
import { connection } from '../signalR';

interface CreateGameProps {
    playerId: string;
    onBack: () => void;
}

function CreateGame({ playerId, onBack }: CreateGameProps) {
    const [gameCode, setGameCode] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    async function createGame() {
        setGameCode(null);
        setError(null);

        try {
            const code = await connection.invoke<string>(
                'CreateLobby',
                {
                    playerId,
                    playerName: 'Player 1',
                }
            );

            setGameCode(code);
        } catch {
            setError('Unable to generate a game code. Please try again.');
        }
    }

    return (
        <div>
            <h1>Create Game</h1>
            {gameCode && <p>Game Code: <strong>{gameCode}</strong></p>}
            {error && <p>{error}</p>}
            <button onClick={createGame}>Generate Game Code</button>
            <p><small>Right now, once you generate a code you cannot join another person's game.</small></p>
            <button onClick={onBack}>Back</button>
        </div>
    );
}

export default CreateGame;