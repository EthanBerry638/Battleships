import { useState, type DragEvent } from "react";
import Board from "../components/Board.tsx";
import type {ConnectionStatus} from "../signalR.ts";

interface SetupProps {
    playerId: string;
    playerName: string;
    connectionStatus: ConnectionStatus;
}

function Setup( {playerId, playerName, connectionStatus}: SetupProps ) {
    const [placedCells, setPlacedCells] = useState<string[]>([]);
    const handleDragStart = (e: DragEvent, itemData: string) => {
        e.dataTransfer.setData("text/plain", itemData);
        e.dataTransfer.effectAllowed = "move";
    };

    const handleCellDragOver = (e: DragEvent) => {
        e.preventDefault();
    };

    const handleCellDrop = (coordinate: string, e: DragEvent) => {
        e.preventDefault();
        const itemType = e.dataTransfer.getData("text/plain");
        console.log(`Dropped ${itemType} on ${coordinate}`);
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
                <Board 
                    onCellDrop={handleCellDrop} 
                    onCellDragOver={handleCellDragOver} 
                />
            </main>
        </div>
    );
}

export default Setup;