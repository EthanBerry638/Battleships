import './GameOver.css';

interface GameOverProps {
    isWinner: boolean;
    onBackToHome: () => void;
}

function GameOver({ isWinner, onBackToHome }: GameOverProps) {
    return (
        <section className="card game-over" aria-label="Game over">
            <h2>{isWinner ? 'You win!' : 'Opponent wins!'}</h2>
            <button type="button" onClick={onBackToHome}>
                Back to home
            </button>
        </section>
    );
}

export default GameOver;
