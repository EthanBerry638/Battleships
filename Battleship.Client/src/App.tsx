import { useEffect, useState } from 'react';
import { startConnection } from './signalR';
import JoinGame from './screens/JoinGame';
import CreateGame from './screens/CreateGame';
import Home from './screens/Home';

type Screen = 'home' | 'create' | 'join';

function App() {
    const [screen, setScreen] = useState<Screen>('home');
    const [playerId] = useState(() => crypto.randomUUID());
    const [playerName, setPlayerName] = useState<string>('Player');

    useEffect(() => {
        void startConnection();
    }, []);
    
    switch (screen) {
        case 'create':
            return <CreateGame 
                playerId={playerId} 
                playerName={playerName}
                onBack={() => setScreen('home')} 
            />;
        case 'join':
            return <JoinGame
                playerId={playerId} 
                playerName={playerName}
                onBack={() => setScreen('home')}
            />;
        default:
            return <Home 
                playerName={playerName}
                setPlayerName={setPlayerName}
                onCreateGame={() => setScreen('create')}
                onJoinGame={() => setScreen('join')} 
            />;
    }
}

export default App;