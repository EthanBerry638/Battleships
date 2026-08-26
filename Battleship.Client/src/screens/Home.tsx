interface HomeProps {
    playerName: string;
    setPlayerName: (playerName: string) => void;
    onCreateGame: () => void;
    onJoinGame: () => void;
}

function Home({ playerName, setPlayerName, onCreateGame, onJoinGame }: HomeProps) {
    const hasPlayerName = playerName.trim().length > 0;
    
    return (
        <div>
            <h1>Battleship</h1>
            <input
                value={playerName}
                onChange={(e) => setPlayerName(e.target.value)}
                placeholder="Player Name"
            />
            <button 
                onClick={onCreateGame}
                disabled={!hasPlayerName}>
                Create Game
            </button>
            <button 
                onClick={onJoinGame}
                disabled={!hasPlayerName}>
                Join Game
            </button>
        </div>
    );
}

export default Home;