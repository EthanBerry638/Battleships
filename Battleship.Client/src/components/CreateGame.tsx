import { connection } from './signalR';

interface CreateGameProps {
    playerId: string;
    onGameCreated: (gameCode: string) => void;
}

function CreateGame({ playerId, onGameCreated }: CreateGameProps) {
    async function createGame() {
        const code = await connection.invoke<string>(
            'CreateLobby',
            {
                playerId,
                playerName: 'Player 1'
            }
        )

        onGameCreated(code)
    }

    return (
        <button onClick={createGame}>
            Create Game
        </button>
    )
}

export default CreateGame;