import './ConnectionLost.css';

interface ConnectionLostProps {
    onBackToHome: () => void;
}

function ConnectionLost({ onBackToHome }: ConnectionLostProps) {
    return (
        <div className="page-container">
            <section className="card connection-lost" aria-label="Connection lost">
                <h2>Connection lost</h2>
                <p>Your game could not be continued.</p>
                <button type="button" onClick={onBackToHome}>
                    Back to home
                </button>
            </section>
        </div>
    );
}

export default ConnectionLost;
