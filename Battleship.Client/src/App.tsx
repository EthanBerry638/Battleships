import { useEffect, useState } from 'react';
import {
    type ConnectionStatus,
    startConnection,
    onGameCreated,
    onGameNotStarted,
    onGameStarted,
    onOpponentDisconnected,
    type Coordinate,
} from './signalR';
import OpponentDisconnected from './components/OpponentDisconnected';
import JoinGame from './screens/JoinGame';
import CreateGame from './screens/CreateGame';
import Home from './screens/Home';
import Setup from './screens/Setup';
import Game from './screens/Game';

type Screen = 'home' | 'create' | 'join' | 'setup' | 'game';

const defaultOpponentName = 'Opponent';

function App() {
    const [screen, setScreen] = useState<Screen>('home');
    const [playerId] = useState(() => crypto.randomUUID());
    const [playerName, setPlayerName] = useState<string>('Player');
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('connecting');
    
    const [waitingForOpponent, setWaitingForOpponent] = useState(false);
    const [fleetCells, setFleetCells] = useState<Coordinate[]>([]);
    const [startingPlayerId, setStartingPlayerId] = useState<string | null>(null);
    const [opponentDisconnected, setOpponentDisconnected] = useState(false);
    const [opponentName, setOpponentName] = useState(defaultOpponentName);

    useEffect(() => {
        const unsubscribeGameCreated = onGameCreated((message) => {
            const opponent = message.players.find((player) => player.id !== playerId);

            setOpponentName(opponent?.name ?? defaultOpponentName);
            setOpponentDisconnected(false);
            setScreen('setup');
        });

        const unsubscribeGameStarted = onGameStarted((message) => {
            setStartingPlayerId(message.startingPlayerId);
            setWaitingForOpponent(false);
            setScreen('game');
        });

        const unsubscribeGameNotStarted = onGameNotStarted(() => {
            setWaitingForOpponent(true);
        });

        const unsubscribeOpponentDisconnected = onOpponentDisconnected(() => {
            setOpponentDisconnected(true);
        });

        void startConnection(setConnectionStatus);

        return () => {
            unsubscribeGameCreated();
            unsubscribeGameStarted();
            unsubscribeGameNotStarted();
            unsubscribeOpponentDisconnected();
        };
    }, [playerId]);

    const handleBackToHome = () => {
        setFleetCells([]);
        setStartingPlayerId(null);
        setWaitingForOpponent(false);
        setOpponentDisconnected(false);
        setOpponentName(defaultOpponentName);
        setScreen('home');
    };

    const isInGame = screen === 'setup' || screen === 'game';

    const renderCurrentScreen = () => {
        if (opponentDisconnected && isInGame) {
            return (
                <OpponentDisconnected
                    opponentName={opponentName}
                    onBackToHome={handleBackToHome}
                />
            );
        }

        switch (screen) {
            case 'create':
                return (
                    <CreateGame
                        connectionStatus={connectionStatus}
                        playerId={playerId}
                        playerName={playerName}
                        onBack={() => setScreen('home')}
                    />
                );
            case 'join':
                return (
                    <JoinGame
                        connectionStatus={connectionStatus}
                        playerId={playerId}
                        playerName={playerName}
                        onBack={() => setScreen('home')}
                    />
                );
            case 'setup':
                return (
                    <Setup
                        playerName={playerName}
                        connectionStatus={connectionStatus}
                        waitingForOpponent={waitingForOpponent}
                        onFleetLocked={setFleetCells}
                    />
                );
            case 'game':
                return (
                    <Game
                        playerId={playerId}
                        opponentName={opponentName}
                        connectionStatus={connectionStatus}
                        startingPlayerId={startingPlayerId}
                        fleetCells={fleetCells}
                        onBackToHome={handleBackToHome}
                    />
                );
            default:
                return (
                    <Home
                        connectionStatus={connectionStatus}
                        playerName={playerName}
                        setPlayerName={setPlayerName}
                        onCreateGame={() => setScreen('create')}
                        onJoinGame={() => setScreen('join')}
                    />
                );
        }
    };

    return (
        <>
            <div>
                {connectionStatus === 'connecting' && (
                    <h2 role="status">Connecting to API</h2>
                )}

                {connectionStatus === 'reconnecting' && (
                    <h2 role="status">Reconnecting to API</h2>
                )}

                {connectionStatus === 'disconnected' && (
                    <h2 role="alert">Unable to connect to the API</h2>
                )}
            </div>

            <main>{renderCurrentScreen()}</main>
        </>
    );
}

export default App;