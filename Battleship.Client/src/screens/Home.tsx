import type {ConnectionStatus} from "../signalR.ts";

interface HomeProps {
    playerName: string;
    setPlayerName: (playerName: string) => void;
    onCreateGame: () => void;
    onJoinGame: () => void;
    connectionStatus: ConnectionStatus;
}

function Home({ playerName, setPlayerName, onCreateGame, onJoinGame, connectionStatus }: HomeProps) {
    const hasPlayerName = playerName.trim().length > 0;
    const canClick = hasPlayerName && connectionStatus === "connected";
    
    return (
        <div className="page-container">
            <div className="card">
                <h1>Battleship</h1>
                <input
                    value={playerName}
                    onChange={(e) => setPlayerName(e.target.value)}
                    placeholder="Player Name"
                />
                <button 
                    onClick={onCreateGame}
                    disabled={!canClick}>
                    Create Game
                </button>
                <button 
                    onClick={onJoinGame}
                    disabled={!canClick}>
                    Join Game
                </button>
            </div>
        </div>
    );
}

export default Home;