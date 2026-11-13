import { useEffect, useState } from 'react';
import Board, { type ShotMarks } from '../components/Board.tsx';
import {
    formatCoordinate,
    onShot,
    shoot,
    ShotResult,
    type ConnectionStatus,
    type Coordinate,
    type ShotMessage,
} from '../signalR.ts';
import './Game.css';

interface GameProps {
    playerId: string;
    connectionStatus: ConnectionStatus;
    startingPlayerId: string | null;
    fleetCells: Coordinate[];
}

function describeShot({ result, coordinate }: ShotMessage, isMine: boolean): string {
    const who = isMine ? 'You' : 'Opponent';
    const cell = formatCoordinate(coordinate);

    switch (result) {
        case ShotResult.Hit:
            return `${who} hit a ship at ${cell}.`;
        case ShotResult.Sunk:
            return `${who} sank a ship at ${cell}.`;
        case ShotResult.Duplicate:
            return `${who} fired at ${cell} again and wasted the turn.`;
        default:
            return `${who} missed at ${cell}.`;
    }
}

function describeError(error: unknown): string {
    // SignalR wraps server errors as "... HubException: <message>".
    const message = error instanceof Error ? error.message : '';
    const serverMessage = message.split('HubException: ')[1];

    return serverMessage ?? 'Failed to reach the server. Please try again.';
}

function Game({ playerId, connectionStatus, startingPlayerId, fleetCells }: GameProps) {
    const [isMyTurn, setIsMyTurn] = useState(startingPlayerId === playerId);
    const [incomingShots, setIncomingShots] = useState<ShotMarks>({});
    const [outgoingShots, setOutgoingShots] = useState<ShotMarks>({});
    const [lastShot, setLastShot] = useState<string | null>(null);
    const [isBusy, setIsBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const isConnected = connectionStatus === 'connected';
    const canShoot = isMyTurn && isConnected && !isBusy;

    useEffect(() => {
        // The server is the authority: boards and turn only change on its Shot events.
        return onShot((message) => {
            const isMine = message.shooterId === playerId;

            if (message.result !== ShotResult.Duplicate) {
                const mark = message.result === ShotResult.Miss ? 'miss' : 'hit';
                const setShots = isMine ? setOutgoingShots : setIncomingShots;

                setShots((previous) => ({
                    ...previous,
                    [formatCoordinate(message.coordinate)]: mark,
                }));
            }

            // The turn passes after every shot.
            setIsMyTurn(!isMine);
            setLastShot(describeShot(message, isMine));
        });
    }, [playerId]);

    const handleShoot = async (coordinate: Coordinate) => {
        if (!canShoot || outgoingShots[formatCoordinate(coordinate)]) {
            return;
        }

        setError(null);
        setIsBusy(true);

        try {
            await shoot(playerId, coordinate);
        } catch (e) {
            setError(describeError(e));
        } finally {
            setIsBusy(false);
        }
    };

    return (
        <div className="page-container">
            <div className="game">
                <h1>Battleships</h1>
                <p role="status" className="game-turn">
                    {isMyTurn ? 'Your turn' : "Opponent's turn"}
                </p>
                {!isConnected && (
                    <p role="status">Connection status: {connectionStatus}</p>
                )}
                {error && <p role="alert">{error}</p>}
                <p className="game-last-shot">{lastShot ?? 'No shots fired yet.'}</p>
                <div className="game-boards">
                    <section aria-label="Your fleet">
                        <h2>Your fleet</h2>
                        <Board placedCells={fleetCells} shots={incomingShots} />
                    </section>
                    <section aria-label="Opponent waters">
                        <h2>Opponent waters</h2>
                        <Board
                            shots={outgoingShots}
                            dimmed={!canShoot}
                            onCellClick={handleShoot}
                        />
                    </section>
                </div>
            </div>
        </div>
    );
}

export default Game;
