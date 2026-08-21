import { useEffect, useState } from 'react';
import { startConnection } from './signalR';
import JoinGame from './screens/JoinGame';
import CreateGame from './screens/CreateGame';
import Home from './screens/Home';

type Screen = 'home' | 'create' | 'join';

function App() {
    const [screen, setScreen] = useState<Screen>('home');
    const [playerId] = useState(() => crypto.randomUUID());

    useEffect(() => {
        startConnection()
            .then(() => console.log('SignalR connected'))
            .catch(console.error);
    }, []);
    
    switch (screen) {
        case 'create':
            return <CreateGame 
                playerId={playerId} 
                onBack={() => setScreen('home')} 
            />;
        case 'join':
            return <JoinGame
                playerId={playerId} 
                onBack={() => setScreen('home')}
            />;
        default:
            return <Home 
                onCreateGame={() => setScreen('create')}
                onJoinGame={() => setScreen('join')} 
            />;
    }
}

export default App;