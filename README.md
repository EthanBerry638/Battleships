# Battleships 

A real-time, multiplayer implementation of the classic Battleship game, built on ASP.NET Core and SignalR, with a React front end in active development. The server can host multiple independent game lobbies and active matches.

---

## Project Overview 

This project provides a game server for playing Battleship live against another player. It handles lobby creation and joining, ship placement and validation, turn-based shooting, win detection, and disconnect handling.

A React client is being built alongside the server. It connects to the SignalR hub and currently supports creating and joining lobbies and placing ships on the board. The rest of the game flow is still being built.

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
- **Battleship.Client**: the React front end, *in active development*

### Client

The client is a single-page React app made up of screens that `App` switches between:

- **`Home`**: enter a player name and choose to create or join a game
- **`CreateGame`**: create a lobby and share the game code
- **`JoinGame`**: join an existing lobby using a game code
- **`Setup`**: place your fleet on the board, with drag-and-drop and horizontal/vertical orientation

The shared `Board` component renders the 10×10 grid. `signalR.ts` manages the hub connection and its status, and wraps the hub methods and server events the screens use.

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
- `GetWinner`

It also handles player disconnects and broadcasts game events to connected clients.

#### Hub Filters

- **`ValidationHubFilter`** automatically runs the matching FluentValidation validator against hub method arguments before the method executes
- **`ExceptionHandlingHubFilter`** converts validation and domain exceptions into client-safe `HubException`s while logging unexpected failures

#### Services

- **`SessionService`**: handles lobby creation and joining
- **`GameService`**: orchestrates active game operations such as fleet placement, readiness, starting games, shooting, and winner lookup
- **`ConnectionService`**: tracks connections and manages disconnect handling
- **`GameSession`**: owns the active `BattleshipEngine`, player readiness state, and the per-game lock used to serialise state-changing operations within a match

#### Repositories

Game state is currently stored in memory using `ConcurrentDictionary`-backed repositories:

- `LobbyRepository`
- `GameRepository`
- `ConnectionRepository`

The repositories manage application-level state mappings while each active `GameSession` controls access to mutable game state.

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
- `ValidateFleetRequest`
- `TryStartGameRequest`
- `ShootRequest`
- `GetWinnerRequest`

Messages and responses include:

- `GameCreatedResponse`
- `StartGameResponse`
- `StartGameMessage`
- `ShotResponse`
- `ShotMessage`

---

## How It Works 

1. Player 1 creates a lobby and receives a game code
2. Player 2 joins using that code. An active game session is created and both clients are notified
3. Both players place their fleets. Ship shape, size, position and placement rules are validated before placement is accepted
4. Each player validates their fleet. If valid they are marked as ready which prevents ship placement whilst waiting for other player
5. The game starts once both players are ready. The server transitions the engine from setup into playing and notifies both clients
6. By default, player 1 starts. Players alternate shots and are notified of the result after each shot
7. The engine detects when a fleet has been destroyed and records the winnning player
8. Clients can call GetWinner to retrieve the winner once the game has finished


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
