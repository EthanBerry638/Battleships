import { useState } from 'react';
import { connection, leaveLobby } from '../signalR';
import type { ConnectionStatus } from '../signalR';

interface CreateGameProps {
    playerId: string;
    playerName: string;
    onBack: () => void;
    connectionStatus: ConnectionStatus;
}

function CreateGame({ playerId, playerName, onBack, connectionStatus }: CreateGameProps) {
    const [gameCode, setGameCode] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isCreating, setIsCreating] = useState(false);
    const [isLeaving, setIsLeaving] = useState(false);
    const canCreate = connectionStatus === 'connected' && !isCreating && gameCode === null;
    const canGoBack = !isCreating && !isLeaving;

    async function createGame() {
        setGameCode(null);
        setError(null);
        setIsCreating(true);

        try {
            const code = await connection.invoke<string>(
                'CreateLobby',
                {
                    playerId,
                    playerName: playerName.trim(),
                }
            );

            setGameCode(code);
        } catch (error: unknown) {
            const errorMessage = error instanceof Error ? error.message : String(error);

            if (errorMessage.includes('already in an active lobby or game')) {
                setError('You are already in an active lobby or game.');
            }
            else {
                setError('Unable to generate a game code. Please try again.');
            }
        } finally {
            setIsCreating(false);
        }
    }

    async function handleBack() {
        if (gameCode === null) {
            onBack();
            return;
        }

        setIsLeaving(true);

        let hasLeft: boolean;

        try {
            hasLeft = await leaveLobby();
        } catch {
            onBack();
            return;
        }

        if (!hasLeft) {
            setIsLeaving(false);
            return;
        }

        onBack();
    }

    return (
        <div className="page-container">
            <div className="card">
                <h1>Create Game</h1>
                {gameCode && <p>Game Code: <strong>{gameCode}</strong></p>}
                {error && <p>{error}</p>}
                <button
                    onClick={createGame}
                    disabled={!canCreate}
                >
                    Generate Game Code
                </button>
                <button onClick={handleBack} disabled={!canGoBack}>Back</button>
            </div>
        </div>
    );
}

export default CreateGame;
