import Board from "../components/Board.tsx";
import type {ConnectionStatus} from "../signalR.ts";

interface SetupProps {
    playerId: string;
    playerName: string;
    connectionStatus: ConnectionStatus;
}

function Setup( _props: SetupProps ) {
    return (
        <div className="page-container">
            <main>
                <h1>Setup</h1>
                <Board />
            </main>
        </div>
    );
}

export default Setup;