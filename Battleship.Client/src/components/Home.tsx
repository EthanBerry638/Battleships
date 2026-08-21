interface HomeProps {
    onCreateGame: () => void;
    onJoinGame: () => void;
}

function Home({ onCreateGame, onJoinGame }: HomeProps) {
    return (
        <div>
            <h1>Battleship</h1>
            <button onClick={onCreateGame}>Create Game</button>
            <button onClick={onJoinGame}>Join Game</button>
        </div>
    );
}

export default Home;