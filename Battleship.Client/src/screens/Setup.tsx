import Board from "../components/Board.tsx";
import type {ConnectionStatus} from "../signalR.ts";

interface SetupProps {
    playerId: string;
    playerName: string;
    connectionStatus: ConnectionStatus;
}

function Setup( {playerId, playerName, connectionStatus}: SetupProps ) {
    const handleDragStart = (e: React.DragEvent, itemData: string) => {
        e.dataTransfer.setData("text/plain", itemData);
        e.dataTransfer.effectAllowed = "move";
    };
    
    return (
        <div className="page-container">
            <main>
                <h1>Setup</h1>
                <div
                    draggable
                    onDragStart={(e) => handleDragStart(e, "carrier")} 
                    className="draggable-ship"
                >
                    Carrier (Size 5)
                </div>
                <Board />
            </main>
        </div>
    );
}

export default Setup;