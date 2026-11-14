# Battleships 

A real-time, multiplayer implementation of the classic Battleship game, built on ASP.NET Core and SignalR, with a React front end. The server can host multiple independent game lobbies and active matches.

---

## Project Overview 

This project provides a game server for playing Battleship live against another player. It handles lobby creation and joining, ship placement and validation, turn-based shooting, win detection, and disconnect handling.

A React client connects to the SignalR hub and supports a full game from start to finish: creating and joining lobbies, placing ships, taking turns shooting, and a game-over screen with a way back to the home screen.

---

## Tech Stack 

### Backend

- **.NET 8 / ASP.NET Core**: API host
- **SignalR**: real-time communication between server and clients
- **FluentValidation**: request DTO validation
- **xUnit, Moq, FluentAssertions**: test suite

### Frontend

- **React 19 + TypeScript**: UI
- **Vite**: dev server and build tooling
- **@microsoft/signalr**: client connection to the game hub
- **Vitest + React Testing Library**: test suite
- **oxlint**: linting

---

## Project Structure 

The repository is organised into three projects:

- **Battleship.Api**: the game server, including SignalR hubs, services, repositories, game sessions, validation, and core game logic
- **Battleship.Tests**: unit and integration tests covering the application from individual domain behaviour through to a complete multiplayer game
- **Battleship.Client**: the React front end

### Client

The client is a single-page React app made up of screens that `App` switches between:

- **`Home`**: enter a player name and choose to create or join a game
- **`CreateGame`**: create a lobby and share the game code
- **`JoinGame`**: join an existing lobby using a game code
- **`Setup`**: place your fleet on the board, with drag-and-drop and horizontal/vertical orientation
- **`Game`**: take turns shooting at the opponent's board, then see the result on the `GameOver` screen

The shared `Board` component renders the 10×10 grid. `signalR.ts` manages the hub connection and its status, and wraps the hub methods and server events the screens use.

The client generates a random player ID (`crypto.randomUUID()`) and sends it only when creating or joining a lobby. In-game calls (`PlaceShip`, `ValidateFleet`, `TryStartGame`, `Shoot`, `ClearBoard`) don't send an ID at all, because the server works out the player from the connection.

### Core Components

#### Hub

`BattleshipHub` is the SignalR entry point used by clients.

It exposes operations including:

- `CreateLobby`
- `JoinLobby`
- `PlaceShip`
- `ValidateFleet`
- `TryStartGame`
- `Shoot`
- `ClearBoard`

It also handles player disconnects and broadcasts game events to connected clients.

#### Hub Filters

- **`ValidationHubFilter`** automatically runs the matching FluentValidation validator against hub method arguments before the method executes
- **`ExceptionHandlingHubFilter`** converts validation and domain exceptions into client-safe `HubException`s while logging unexpected failures

#### Services

- **`SessionService`**: handles lobby creation and joining
- **`GameService`**: orchestrates active game operations such as fleet placement, readiness, starting games, and shooting, and removes a game once it is finished
- **`ConnectionService`**: links each connection to its player, resolves the calling player for in-game requests, and manages disconnect handling
- **`GameSession`**: owns the active `BattleshipEngine`, player readiness state, and the per-game lock used to serialise state-changing operations within a match

#### Repositories

Game state is currently stored in memory using `ConcurrentDictionary`-backed repositories:

- `LobbyRepository`
- `GameRepository`
- `ConnectionRepository`

The repositories manage application-level state mappings while each active `GameSession` controls access to mutable game state.

A lobby is removed when it is joined or when its player disconnects. A game is removed when it finishes or when a player disconnects mid-game.

#### Engine

`BattleshipEngine` contains the core rules for a single match, including:

- Ship placement
- Fleet validation
- Game state transitions
- Turn order
- Shooting
- Duplicate shot detection
- Hit, miss, and sunk results
- Win detection

The engine is independent of SignalR, allowing the game rules to be tested separately from the transport layer.

#### Game Pieces

- **Entities:** `Player`, `Ship`
- **Board:** `GameBoard`, `Tile`
- **Data:** `Coordinate`, `ShipType`, `ShotResult`, `GameState`, `PlacementResult`, `FleetValidationResult`

`Coordinate` acts as a value object and prevents coordinates outside the 10×10 board from being created.

#### DTOs

Request and response/message DTOs are used to keep the SignalR API separate from the internal game model.

Requests include:

- `CreateLobbyRequest`
- `JoinLobbyRequest`
- `PlaceShipRequest`
- `ShootRequest`

Only `CreateLobbyRequest` and `JoinLobbyRequest` carry a `PlayerId`. `ValidateFleet`, `TryStartGame` and `ClearBoard` take no arguments.

Messages and responses include:

- `GameCreatedResponse`
- `StartGameResponse`
- `StartGameMessage`
- `ShotResponse`
- `ShotMessage`

---

## How It Works 

1. Player 1 creates a lobby and receives a game code. Their connection is linked to their player ID
2. Player 2 joins using that code, and their connection is linked the same way. An active game session is created and both clients are notified. From here on the server identifies each player by their connection
3. Both players place their fleets. Ship shape, size, position and placement rules are validated before placement is accepted
4. Each player validates their fleet. If valid they are marked as ready which prevents ship placement whilst waiting for other player
5. The game starts once both players are ready. The server transitions the engine from setup into playing and notifies both clients
6. By default, player 1 starts. Players alternate shots and are notified of the result after each shot
7. When a shot sinks the opponent's last ship, the engine marks the game as finished and the `Shot` broadcast has `IsGameOver` set to true. The shooter (`ShooterId`) is the winner
8. As soon as the game is finished, the server removes it from memory and takes both players out of its SignalR group. Both players can then return home and create or join a new game on the same connection

---

## How to Run 

### Backend

Ensure you have the .NET SDK installed.
Navigate to the `Battleship.Api` directory.
Run the following command:

```bash
dotnet run --launch-profile https
```

The SignalR hub is exposed at `/gameHub` (`https://localhost:7146/gameHub` when using the `https` profile).

### Frontend

Ensure you have Node.js installed and the backend is running.
Navigate to the `Battleship.Client` directory.
Run the following commands:

```bash
npm install
npm run dev
```

The client runs at `http://localhost:5173`. The API's CORS policy allows this origin.

To try it locally, open the client in two browser windows. Create a game in one, then join it with the game code in the other.

---

## Testing 

### Backend

To run the backend test suite, navigate to the repository root directory and execute:

```bash
dotnet test
```

### Frontend

To run the frontend test suite, navigate to the `Battleship.Client` directory and execute:

```bash
npm test
```

---

## Future Ideas

- **Data Persistence:** Transition lobby/game/connection storage to PostgreSQL
- **AI Opponent:** Optional single-player mode against a basic AI, reusing the existing engine
