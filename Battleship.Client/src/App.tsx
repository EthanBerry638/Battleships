import { useEffect, useState } from 'react';
import { type ConnectionStatus, startConnection, onGameCreated } from './signalR';
import JoinGame from './screens/JoinGame';
import CreateGame from './screens/CreateGame';
import Home from './screens/Home';
import Setup from './screens/Setup';

type Screen = 'home' | 'create' | 'join' | 'setup';

function App() {
    const [screen, setScreen] = useState<Screen>('home');
    const [playerId] = useState(() => crypto.randomUUID());
    const [playerName, setPlayerName] = useState<string>('Player');
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('connecting');
    
    useEffect(() => {
        const unsubscribe = onGameCreated(() => {
            setScreen('setup');
        });
        
        void startConnection(setConnectionStatus);
        
        return unsubscribe;
    }, []);
    
    const renderCurrentScreen = () => {
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
                    <Setup />
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