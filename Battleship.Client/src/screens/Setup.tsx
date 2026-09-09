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

const fleet: Ship[] = [
    { id: 'carrier', name: 'Carrier', size: 5 },
    { id: 'battleship', name: 'Battleship', size: 4 },
    { id: 'destroyer', name: 'Destroyer', size: 3 },
    { id: 'submarine', name: 'Submarine', size: 3 },
    { id: 'patrol boat', name: 'Patrol Boat', size: 2 },
];

function Setup( _props : SetupProps ) {
    const [orientation, setOrientation] = useState<Orientation>('horizontal');
    const [placedCells, setPlacedCells] = useState<string[]>([]);
    const [placedShips, setPlacedShips] = useState<PlacedShip[]>([]);

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

        const shipId = e.dataTransfer.getData('text/plain');
        const ship = fleet.find((candidate) => candidate.id === shipId);

        if (!ship) {
            return;
        }

        const isAlreadyPlaced = placedShips.some(
            (placedShip) => placedShip.id === ship.id,
        );

        if (isAlreadyPlaced) {
            return;
        }

        const targetCells = getShipCoordinates(
            coordinate,
            ship.size,
            orientation,
        );

        if (!targetCells) {
            return;
        }

        const hasOverlap = targetCells.some((cell) =>
            placedCells.includes(cell),
        );

        if (hasOverlap) {
            return;
        }

        setPlacedShips((previous) => [
            ...previous,
            {
                ...ship,
                orientation,
                coordinates: targetCells,
            },
        ]);

        setPlacedCells((previous) => [...previous, ...targetCells]);
    };

    const clearPlacement = () => {
        setPlacedShips([]);
        setPlacedCells([]);
    };

    return (
        <div className='page-container'>
            <main>
                <h1>Setup</h1>
                <div className='ship-list'>
                    {fleet.map((ship) => {
                        const isPlaced = placedShips.some(
                            (placedShip) => placedShip.id === ship.id,
                        );

                        return (
                            <div
                                key={ship.id}
                                draggable={!isPlaced}
                                onDragStart={(e) => handleDragStart(e, ship.id)}
                                className={`draggable-ship ${isPlaced ? 'ship-placed' : ''}`}
                            >
                                {ship.name} (Size {ship.size})
                            </div>
                        );
                    })}
                </div>
                <button
                    type='button'
                    onClick={clearPlacement}
                    disabled={placedShips.length === 0}
                >
                    Clear Placement
                </button>
                <button
                    type='button'
                    onClick={toggleOrientation}
                    aria-label='Toggle ship orientation'
                >
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