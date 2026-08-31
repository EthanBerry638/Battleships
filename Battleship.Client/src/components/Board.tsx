import "./Board.css";
import * as React from "react";

interface BoardProps {
    placedCells: string[];
    onCellDrop: (coordinate: string, e: React.DragEvent) => void;
    onCellDragOver: (e: React.DragEvent) => void;
}

const letters = "ABCDEFGHIJ".split("");
const numbers = Array.from({ length: 10 }, (_, index) => index + 1);

function Board({
    placedCells,
    onCellDrop,
    onCellDragOver,
}: BoardProps) {
    return (
        <div className="board">
            <div />

            {numbers.map((number) => (
                <div className="label" key={number}>
                    {number}
                </div>
            ))}

            {letters.map((letter) => (
                <div className="board-row" key={letter}>
                    <div className="label">{letter}</div>

                    {numbers.map((number) => {
                        const coordinate = `${letter}${number}`;
                        const isOccupied = placedCells.includes(coordinate);
                        
                        return (
                            <div
                                className={`cell ${isOccupied ? "cell-occupied" : ""}`}
                                key={coordinate}
                                title={coordinate}
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