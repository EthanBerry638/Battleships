import './GameOver.css';

interface GameOverProps {
    isWinner: boolean;
    opponentName: string;
    onBackToHome: () => void;
}

function GameOver({ isWinner, opponentName, onBackToHome }: GameOverProps) {
    return (
        <section className="card game-over" aria-label="Game over">
            <h2>{isWinner ? 'You win!' : `${opponentName} wins!`}</h2>
            <button type="button" onClick={onBackToHome}>
                Back to home
            </button>
        </section>
    );
}

export default GameOver;
