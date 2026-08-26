import { useEffect, useState } from 'react';
import {type ConnectionStatus, startConnection} from './signalR';
import JoinGame from './screens/JoinGame';
import CreateGame from './screens/CreateGame';
import Home from './screens/Home';

type Screen = 'home' | 'create' | 'join';

function App() {
    const [screen, setScreen] = useState<Screen>('home');
    const [playerId] = useState(() => crypto.randomUUID());
    const [playerName, setPlayerName] = useState<string>('Player');
    const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('connecting');
    
    useEffect(() => {
        void startConnection(setConnectionStatus);
    }, []);
    
    switch (screen) {
        case 'create':
            return <CreateGame 
                connectionStatus={connectionStatus}
                playerId={playerId} 
                playerName={playerName}
                onBack={() => setScreen('home')} 
            />;
        case 'join':
            return <JoinGame
                connectionStatus={connectionStatus}
                playerId={playerId} 
                playerName={playerName}
                onBack={() => setScreen('home')}
            />;
        default:
            return <Home 
                connectionStatus={connectionStatus}
                playerName={playerName}
                setPlayerName={setPlayerName}
                onCreateGame={() => setScreen('create')}
                onJoinGame={() => setScreen('join')} 
            />;
    }
}

export default App;