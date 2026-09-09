import { useState, type DragEvent } from 'react';
import Board from '../components/Board.tsx';
import type {ConnectionStatus} from '../signalR.ts';

interface SetupProps {
    playerId: string;
    playerName: string;
    connectionStatus: ConnectionStatus;
}

type Orientation = 'horizontal' | 'vertical';

interface Ship {
    id: string;
    name: string;
    size: number;
}

interface PlacedShip extends Ship {
    orientation: Orientation;
    coordinates: string[];
}

function Setup( _props : SetupProps ) {
    const [orientation, setOrientation] = useState<Orientation>('horizontal');
    const [placedCells, setPlacedCells] = useState<string[]>([]);

    const toggleOrientation = () => {
        setOrientation((prev) => (prev === 'horizontal' ? 'vertical' : 'horizontal'));
    };

    const getShipCoordinates = (
        origin: string,
        size: number,
        shipOrientation: Orientation,
    ): string[] | null => {
        const letters = 'ABCDEFGHIJ';
        const startLetterIndex = letters.indexOf(origin.charAt(0));
        const startNumber = parseInt(origin.slice(1), 10);

        const coordinates: string[] = [];

        for (let i = 0; i < size; i++) {
            const letterIndex =
                shipOrientation === 'vertical'
                    ? startLetterIndex + i
                    : startLetterIndex;

            const number =
                shipOrientation === 'horizontal'
                    ? startNumber + i
                    : startNumber;

            if (letterIndex >= letters.length || number > 10) {
                return null;
            }

            coordinates.push(`${letters[letterIndex]}${number}`);
        }

        return coordinates;
    };
    
    const handleDragStart = (e: DragEvent, itemData: string) => {
        e.dataTransfer.setData('text/plain', itemData);
        e.dataTransfer.effectAllowed = 'move';
    };

    const handleCellDragOver = (e: DragEvent) => {
        e.preventDefault();
    };

    const handleCellDrop = (coordinate: string, e: DragEvent) => {
        e.preventDefault();
        
        const carrierSize = 5;
        const targetCells = getShipCoordinates(coordinate, carrierSize, orientation);
        
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
        <div className='page-container'>
            <main>
                <h1>Setup</h1>
                <div
                    draggable
                    onDragStart={(e) => handleDragStart(e, 'carrier')}
                    className='draggable-ship'
                >
                    Carrier (Size 5)
                </div>
                
                <button type='button' onClick={toggleOrientation} aria-label='Toggle ship orientation'>
                    {orientation === 'horizontal' ? '➡️ Horizontal' : '⬇️ Vertical'}
                </button>
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