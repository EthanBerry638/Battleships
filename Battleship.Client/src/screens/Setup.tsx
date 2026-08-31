import { useState, type DragEvent } from 'react';
import Board from "../components/Board.tsx";
import type {ConnectionStatus} from "../signalR.ts";

interface SetupProps {
    playerId: string;
    playerName: string;
    connectionStatus: ConnectionStatus;
}

function Setup( _props : SetupProps ) {
    const [placedCells, setPlacedCells] = useState<string[]>([]);

    const getShipCoordinates = (origin: string, size: number): string[] | null => {
        const letter = origin.charAt(0);
        const startNum = parseInt(origin.slice(1), 10);

        const coordinates: string[] = [];

        for (let i = 0; i < size; i++) {
            const nextNum = startNum + i;
            if (nextNum > 10) {
                return null;
            }
            coordinates.push(`${letter}${nextNum}`);
        }

        return coordinates;
    };
    
    const handleDragStart = (e: DragEvent, itemData: string) => {
        e.dataTransfer.setData("text/plain", itemData);
        e.dataTransfer.effectAllowed = "move";
    };

    const handleCellDragOver = (e: DragEvent) => {
        e.preventDefault();
    };

    const handleCellDrop = (coordinate: string, e: DragEvent) => {
        e.preventDefault();
        
        const carrierSize = 5;
        const targetCells = getShipCoordinates(coordinate, carrierSize);
        
        if (!targetCells) {
            return;
        }
        
        const hasOverlap = targetCells.some((cell => placedCells.includes(cell)));
        if (hasOverlap) {
            return;
        }

        setPlacedCells((prev) => [...prev, ...targetCells]);
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
                    placedCells={placedCells}
                    onCellDrop={handleCellDrop}
                    onCellDragOver={handleCellDragOver}
                />
            </main>
        </div>
    );
}

export default Setup;