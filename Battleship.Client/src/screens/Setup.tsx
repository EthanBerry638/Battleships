import { useState, type DragEvent } from 'react';
import Board from '../components/Board.tsx';
import {
    clearBoard,
    formatCoordinate,
    isSameCoordinate,
    placeShip,
    ShipType,
    type ConnectionStatus,
    type Coordinate,
} from '../signalR.ts';

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
    type: ShipType;
}

interface PlacedShip extends Ship {
    orientation: Orientation;
    coordinates: Coordinate[];
}

const boardSize = 10;

const fleet: Ship[] = [
    { id: 'carrier', name: 'Carrier', size: 5, type: ShipType.Carrier },
    { id: 'battleship', name: 'Battleship', size: 4, type: ShipType.Battleship },
    { id: 'destroyer', name: 'Destroyer', size: 3, type: ShipType.Destroyer },
    { id: 'submarine', name: 'Submarine', size: 3, type: ShipType.Submarine },
    { id: 'patrol boat', name: 'Patrol Boat', size: 2, type: ShipType.PatrolBoat },
];

function Setup({ playerId, connectionStatus }: SetupProps) {
    const [orientation, setOrientation] = useState<Orientation>('horizontal');
    const [placedCells, setPlacedCells] = useState<Coordinate[]>([]);
    const [placedShips, setPlacedShips] = useState<PlacedShip[]>([]);
    const [isBusy, setIsBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const isConnected = connectionStatus === 'connected';

    const toggleOrientation = () => {
        setOrientation((prev) => (prev === 'horizontal' ? 'vertical' : 'horizontal'));
    };

    const getShipCoordinates = (
        origin: Coordinate,
        size: number,
        shipOrientation: Orientation,
    ): Coordinate[] | null => {
        const coordinates: Coordinate[] = [];

        for (let i = 0; i < size; i++) {
            const x = shipOrientation === 'horizontal' ? origin.x + i : origin.x;
            const y = shipOrientation === 'vertical' ? origin.y + i : origin.y;

            if (x >= boardSize || y >= boardSize) {
                return null;
            }

            coordinates.push({ x, y });
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

    const handleCellDrop = async (coordinate: Coordinate, e: DragEvent) => {
        e.preventDefault();
        setError(null);

        if (!isConnected || isBusy) {
            return;
        }

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
            setError(`${ship.name} does not fit on the board at ${formatCoordinate(coordinate)}.`);
            return;
        }

        const hasOverlap = targetCells.some((cell) =>
            placedCells.some((placed) => isSameCoordinate(placed, cell)),
        );

        if (hasOverlap) {
            setError(`${ship.name} overlaps another ship.`);
            return;
        }

        setIsBusy(true);

        try {
            const result = await placeShip({
                playerId,
                type: ship.type,
                coordinates: targetCells,
            });

            if (!result.isSuccessful) {
                const invalid = result.invalidCoordinates?.map(formatCoordinate) ?? [];
                setError(
                    invalid.length > 0
                        ? `Could not place ${ship.name}. Invalid cells: ${invalid.join(', ')}.`
                        : `Could not place ${ship.name}.`,
                );
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
        } catch {
            setError('Failed to reach the server. Please try again.');
        } finally {
            setIsBusy(false);
        }
    };

    const clearPlacement = async () => {
        setError(null);
        setIsBusy(true);

        try {
            await clearBoard(playerId);
            setPlacedShips([]);
            setPlacedCells([]);
        } catch {
            setError('Failed to clear the board. Please try again.');
        } finally {
            setIsBusy(false);
        }
    };

    const allShipsPlaced = placedShips.length === fleet.length;

    return (
        <div className='page-container'>
            <main>
                <h1>Setup</h1>
                {!isConnected && (
                    <p role='status'>Connection status: {connectionStatus}</p>
                )}
                {error && <p role='alert'>{error}</p>}
                <div className='ship-list'>
                    {fleet.map((ship) => {
                        const isPlaced = placedShips.some(
                            (placedShip) => placedShip.id === ship.id,
                        );

                        return (
                            <div
                                key={ship.id}
                                draggable={!isPlaced && isConnected && !isBusy}
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
                    disabled={placedShips.length === 0 || !isConnected || isBusy}
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
                {allShipsPlaced && <p>All ships placed – you're ready to start.</p>}
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