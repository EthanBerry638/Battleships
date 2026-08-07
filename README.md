# Battleships 🚢

A real-time, multiplayer backend implementation of the classic Battleship game, built on ASP.NET Core and SignalR.

---

## Project Overview 📋

This project provides a game server for playing Battleship live against another player. It handles lobby creation and joining, ship placement and validation, turn-based shooting, win detection, and disconnect handling.

---

## Tech Stack 🛠️

- **.NET 8 / ASP.NET Core** — API host
- **SignalR** — real-time communication between server and clients
- **xUnit, Moq, FluentAssertions** — test suite

---

## Project Structure 🏗️

The solution is organized into two projects:

- **Battleship.Api:** The game server: hub, game logic, in-memory session management, and supporting types
- **Battleship.Tests:** Unit tests covering the engine, board, hub, manager, parser, ship, and player logic

Core components of **Battleship.Api**:

- **Hubs:** `BattleshipHub`: the SignalR entry point clients connect to. Exposes `CreateLobby`, `JoinLobby`, and `PlaceShip`, and handles disconnects (with a grace period before a game is torn down)
- **Services:**
    - `BattleshipManager`: coordinates lobbies, active games, and player connections
    - `GameSession`: wraps a `BattleshipEngine` instance with a lock, so a single game session can be safely accessed by concurrent hub calls
- **Engine:** `BattleshipEngine`: the core rules engine for a single match: turn order, shooting, ship placement, and win conditions
- **GamePieces:**
    - **Entities:** `Player`, `Ship` (ship placement/adjacency/type validation lives here)
    - **Board:** `GameBoard`: grid state, tile occupancy, fleet validation, and win checks; `Tile`
    - **Data:** value types and results shared across the engine: `Coordinate`, `ShipType`, `ShotResult`, `GameState`, `PlacementResult`, `FleetValidationResult`, `GameStartResult`
- **DTOs:** request payloads sent over the hub (`CreateLobbyRequest`, `JoinLobbyRequest`, `PlaceShipRequest`, `AddConnectionRequest`)
- **Parsers:** `CoordinateParser`: converts board-style input (e.g. `"B7"`) into a `Coordinate`
- **Exceptions:** domain-specific exceptions for invalid moves and game state (e.g. `NotYourTurnException`, `GameOverException`, `InvalidShipException`, `PlayerAlreadyInSessionException`)

---

## How It Works 🎮

1. A player calls `CreateLobby` and receives a short game code to share with an opponent
2. A second player calls `JoinLobby` with that code, which spins up a `BattleshipEngine` for the match and notifies both clients the game has started
3. Both players place their fleet via `PlaceShip`; the engine validates ship shape, adjacency, and type/size before accepting it
5. If a player disconnects, the server waits briefly (to allow for reconnects) before ending the game and notifying the remaining player

Game state currently lives in memory on the server (`ConcurrentDictionary`-backed lobbies/games/connections in `BattleshipManager`) there is no persistence layer yet.

---

## How to Run 💻

Ensure you have the .NET SDK installed.
Navigate to the `Battleship.Api` directory.
Run the following command:

```bash
dotnet run
```

The SignalR hub is exposed at `/gameHub`.

---

## Testing 🧪

To run the test suite, navigate to the repository root directory and execute:

```bash
dotnet test
```

---

## Current Work 🔧

Actively splitting `BattleshipManager` apart into dedicated **services** and **repositories**, so lobby/game/connection storage is decoupled from the coordination logic that sits on top of it. This is prep work for swapping the in-memory stores out for real persistence.

---

## Roadmap 🛣️

- **Manager Refactor:** Finish splitting `BattleshipManager` into services + repositories
- **Data Persistence:** Transition lobby/game/connection storage to PostgreSQL
- **Front End:** Develop a modern, responsive UI (React) that talks to the SignalR hub
- **AI Opponent:** Optional single-player mode against a basic AI, reusing the existing engine