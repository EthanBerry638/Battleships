import './OpponentDisconnected.css';

interface OpponentDisconnectedProps {
    opponentName: string;
    onBackToHome: () => void;
}

function OpponentDisconnected({ opponentName, onBackToHome }: OpponentDisconnectedProps) {
    return (
        <div className="page-container">
            <section className="card opponent-disconnected" aria-label="Opponent disconnected">
                <h2>{opponentName} disconnected</h2>
                <p>The game has ended.</p>
                <button type="button" onClick={onBackToHome}>
                    Back to home
                </button>
            </section>
        </div>
    );
}

export default OpponentDisconnected;
