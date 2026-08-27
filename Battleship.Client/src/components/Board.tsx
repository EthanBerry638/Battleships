import "./Board.css";

const letters = "ABCDEFGHIJ".split("");
const numbers = Array.from({ length: 10 }, (_, index) => index + 1);

function Board() {
    return (
        <main>
            <h1>Setup</h1>

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

                        {numbers.map((number) => (
                            <div
                                className="cell"
                                key={`${letter}${number}`}
                                title={`${letter}${number}`}
                            >
                                <span className="coordinate-dot" />
                            </div>
                        ))}
                    </div>
                ))}
            </div>
        </main>
    );
}

export default Board;