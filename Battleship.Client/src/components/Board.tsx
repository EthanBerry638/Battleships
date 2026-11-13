import { type DragEvent } from 'react';
import { formatCoordinate, isSameCoordinate, rowLabels, type Coordinate } from '../signalR.ts';
import "./Board.css";

interface BoardProps {
    placedCells: Coordinate[];
    onCellDrop: (coordinate: Coordinate, e: DragEvent) => void;
    onCellDragOver: (e: DragEvent) => void;
}

const indices = Array.from({ length: 10 }, (_, index) => index);

function Board({
    placedCells,
    onCellDrop,
    onCellDragOver,
}: BoardProps) {
    return (
        <div className="board">
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
                        const isOccupied = placedCells.some((cell) =>
                            isSameCoordinate(cell, coordinate),
                        );

                        return (
                            <div
                                className={`cell ${isOccupied ? "cell-occupied" : ""}`}
                                key={`${x}-${y}`}
                                title={formatCoordinate(coordinate)}
                                onDragOver={onCellDragOver}
                                onDrop={(e) => onCellDrop(coordinate, e)}
                            >
                                {!isOccupied && <span className="coordinate-dot" />}
                            </div>
                        );
                    })}
                </div>
            ))}
        </div>
    );
}

export default Board;
