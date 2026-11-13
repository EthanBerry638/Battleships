import { type DragEvent } from 'react';
import { formatCoordinate, isSameCoordinate, rowLabels, type Coordinate } from '../signalR.ts';
import "./Board.css";

export type ShotMark = 'hit' | 'miss';

export type ShotMarks = Record<string, ShotMark>;

interface BoardProps {
    placedCells?: Coordinate[];
    shots?: ShotMarks;
    dimmed?: boolean;
    onCellDrop?: (coordinate: Coordinate, e: DragEvent) => void;
    onCellDragOver?: (e: DragEvent) => void;
    onCellClick?: (coordinate: Coordinate) => void;
}

const indices = Array.from({ length: 10 }, (_, index) => index);

function Board({
    placedCells = [],
    shots = {},
    dimmed = false,
    onCellDrop,
    onCellDragOver,
    onCellClick,
}: BoardProps) {
    return (
        <div className={`board ${dimmed ? "board-dimmed" : ""}`}>
            <div />

            {indices.map((x) => (
                <div className="label" key={x}>
                    {x + 1}
                </div>
            ))}

            {indices.map((y) => (
                <div className="board-row" key={y}>
                    <div className="label">{rowLabels[y]}</div>

                    {indices.map((x) => {
                        const coordinate: Coordinate = { x, y };
                        const name = formatCoordinate(coordinate);
                        const isOccupied = placedCells.some((cell) =>
                            isSameCoordinate(cell, coordinate),
                        );
                        const shot = shots[name];

                        const className = [
                            "cell",
                            isOccupied ? "cell-occupied" : "",
                            shot ? `cell-${shot}` : "",
                        ].filter(Boolean).join(" ");

                        const marker = shot
                            ? <span className={`shot-marker shot-marker-${shot}`} />
                            : !isOccupied && <span className="coordinate-dot" />;

                        if (onCellClick) {
                            return (
                                <button
                                    type="button"
                                    className={className}
                                    key={name}
                                    title={name}
                                    aria-label={shot ? `${name}, ${shot}` : name}
                                    aria-disabled={dimmed || shot !== undefined}
                                    onClick={() => onCellClick(coordinate)}
                                >
                                    {marker}
                                </button>
                            );
                        }

                        return (
                            <div
                                className={className}
                                key={name}
                                title={name}
                                onDragOver={onCellDragOver}
                                onDrop={onCellDrop && ((e) => onCellDrop(coordinate, e))}
                            >
                                {marker}
                            </div>
                        );
                    })}
                </div>
            ))}
        </div>
    );
}

export default Board;
