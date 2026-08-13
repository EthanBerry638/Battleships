# Battleships 🚢

A real-time, multiplayer backend implementation of the classic Battleship game, built on ASP.NET Core and SignalR.

---

## Project Overview 📋

This project provides a game server for playing Battleship live against another player. It handles lobby creation and joining, ship placement and validation, turn-based shooting, win detection, and disconnect handling.

---

## Tech Stack 🛠️

- **.NET 8 / ASP.NET Core**: API host
- **SignalR**: real-time communication between server and clients
- **FluentValidation**: request DTO validation
- **xUnit, Moq, FluentAssertions**: test suite

---

## Project Structure 🏗️

The solution is organized into two projects:

- **Battleship.Api:** The game server: hub, hub filters, game logic, in-memory repositories, and supporting types
- **Battleship.Tests:** Unit tests covering the engine, board, hub, services, repositories, ship, player, coordinate, and DTO validator logic, plus integration tests covering the hub end-to-end

Core components of **Battleship.Api**:

- **Hubs:** `BattleshipHub`: the SignalR entry point clients connect to. Exposes `CreateLobby`, `JoinLobby`, `PlaceShip`, `TryStartGame`, and `GetWinner`, and handles disconnects (with a grace period before a game is torn down)
    - **Filters:** `ValidationHubFilter` runs the matching FluentValidation validator against every hub method argument before the method executes, and rejects missing required arguments; `ExceptionHandlingHubFilter` catches validation and domain exceptions and translates them into client-safe `HubException`s, logging anything unexpected before it propagates
- **Services:**
    - `SessionService`: creates and joins lobbies, promoting a lobby into an active game once a second player joins
    - `GameService`: routes ship placement, game-start, and winner lookups to the correct player's `GameSession`
    - `ConnectionService`: tracks which connection belongs to which player and handles the disconnect grace period
    - `GameSession`: wraps a `BattleshipEngine` instance with a lock, so a single game session can be safely accessed by concurrent hub calls
- **Repositories:** in-memory (`ConcurrentDictionary`-backed) storage for lobbies, games, and connections (`LobbyRepository`, `GameRepository`, `ConnectionRepository`)
- **Engine:** `BattleshipEngine`: the core rules engine for a single match: turn order, shooting, ship placement, and win conditions
- **GamePieces:**
    - **Entities:** `Player`, `Ship` (ship placement/adjacency/type validation lives here)
    - **Board:** `GameBoard`: grid state, tile occupancy, fleet validation, and win checks; `Tile`
    - **Data:** value types and results shared across the engine: `Coordinate`, `ShipType`, `ShotResult`, `GameState`, `PlacementResult`, `FleetValidationResult`, `GameStartResult`
- **DTOs:** request/response payloads sent over the hub
    - **Requests:** `CreateLobbyRequest`, `JoinLobbyRequest`, `PlaceShipRequest`, `TryStartGameRequest`, `GetWinnerRequest`
    - **Responses:** `GameCreatedResponse`, `StartGameResponse`
    - **Validators:** a FluentValidation validator for each request DTO, resolved and run automatically by `ValidationHubFilter`
- **Exceptions:** domain-specific exceptions for invalid moves and game state, all deriving from `BattleshipException` (e.g. `NotYourTurnException`, `GameOverException`, `InvalidShipException`, `PlayerAlreadyInSessionException`, `PlayerNotFoundException`, `GameNotFoundException`)

---

## How It Works 🎮

1. A player calls `CreateLobby` and receives a short game code to share with an opponent
2. A second player calls `JoinLobby` with that code, which spins up a `BattleshipEngine` for the match and notifies both clients the game has started
3. Both players place their fleet via `PlaceShip`; the engine validates ship shape, adjacency, and type/size before accepting it
4. Each player calls `TryStartGame` once ready; once both players are ready, the engine validates both fleets and the match begins
5. Players call `GetWinner` to check whether the game has been decided
6. If a player disconnects, the server waits briefly (to allow for reconnects) before ending the game and notifying the remaining player

Every hub method's request DTO is validated by `ValidationHubFilter` before the method body runs, and any validation or domain exception is translated into a client-safe `HubException` by `ExceptionHandlingHubFilter` — hub methods themselves don't need to handle either concern.

Game state currently lives in memory on the server (`ConcurrentDictionary`-backed lobbies/games/connections), there is no persistence layer yet.

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

## Roadmap 🛣️

- **Data Persistence:** Transition lobby/game/connection storage to PostgreSQL
- **Front End:** Develop a modern, responsive UI (React) that talks to the SignalR hub
- **AI Opponent:** Optional single-player mode against a basic AI, reusing the existing engine