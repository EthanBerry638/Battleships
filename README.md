# Battleships

A real-time, two-player implementation of the classic Battleship game, built on ASP.NET Core and SignalR, with a React front end. The server can host multiple independent lobbies and matches at once.

**Status:** version 1 is complete. A full game can be played from start to finish in the browser. It is built to run locally, and a few things are deliberately out of scope for this version. See [Limitations](#limitations).

---

## Features

- Create a lobby and share a 6-character game code, or join one with a code (codes are not case-sensitive)
- Cancel a lobby you created before anyone joins
- Drag-and-drop fleet placement with horizontal and vertical orientation, validated by the server
- Turn-based shooting with hit, miss and sunk results, and sunk ships highlighted on the board
- Game-over screen, with a way back to the home screen to start another game on the same connection
- Opponent-disconnected and connection-lost screens, plus a connection status banner with automatic retry on the initial connection

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

### CI

A GitHub Actions workflow runs on every pull request to `main`. It runs the backend tests, and lints, type-checks, builds and tests the client.

---

## Project Structure

The repository is organised into three projects:

- **Battleship.Api**: the game server, including SignalR hubs, services, repositories, game sessions, validation, and core game logic
- **Battleship.Tests**: unit and integration tests covering the application from individual domain behaviour through to a complete multiplayer game
- **Battleship.Client**: the React front end

### Client

The client is a single-page React app made up of screens that `App` switches between:

- **`Home`**: enter a player name and choose to create or join a game
- **`CreateGame`**: create a lobby and share the game code. Going back cancels the lobby
- **`JoinGame`**: join an existing lobby using a game code
- **`Setup`**: place your fleet on the board, with drag-and-drop and horizontal/vertical orientation
- **`Game`**: take turns shooting at the opponent's board, then see the result on the `GameOver` screen

Smaller components cover the remaining states:

- **`Board`**: renders the 10×10 grid, shared by `Setup` and `Game`
- **`GameOver`**: shows the winner and a button back to the home screen
- **`OpponentDisconnected`**: shown in place of the setup or game screen when the other player drops
- **`ConnectionLost`**: shown when your own connection drops while you are in a lobby or game

`signalR.ts` manages the hub connection and its status, and wraps the hub methods and server events the screens use. On start-up it tries to connect up to four times (immediately, then after 2, 10 and 30 seconds) before giving up. `App` shows a banner while connecting or reconnecting, and when the connection could not be made.

The client generates a random player ID (`crypto.randomUUID()`) and sends it only when creating or joining a lobby. Every other call (`LeaveLobby`, `PlaceShip`, `ValidateFleet`, `TryStartGame`, `Shoot`, `ClearBoard`) doesn't send an ID at all, because the server works out the player from the connection.

### Core Components

#### Hub

`BattleshipHub` is the SignalR entry point used by clients.

It exposes these operations:

- `CreateLobby`
- `JoinLobby`
- `LeaveLobby`
- `PlaceShip`
- `ValidateFleet`
- `TryStartGame`
- `Shoot`
- `ClearBoard`

It sends these events to clients:

- `GameCreated`: both players are in, and setup can begin
- `GameStarted`: both players are ready, and the game is under way
- `GameNotStarted`: sent to the caller when the other player is not ready yet
- `Shot`: the result of a shot, sent to both players
- `OpponentDisconnected`: the other player dropped, and the game has been removed

#### Hub Filters

- **`ValidationHubFilter`** automatically runs the matching FluentValidation validator against hub method arguments before the method executes
- **`ExceptionHandlingHubFilter`** converts validation and domain exceptions into client-safe `HubException`s while logging unexpected failures

#### Services

- **`SessionService`**: handles lobby creation, joining and leaving, and generates game codes that don't collide with an existing lobby or game
- **`GameService`**: orchestrates active game operations such as fleet placement, readiness, starting games, and shooting, and removes a game once it is finished
- **`ConnectionService`**: links each connection to its player, resolves the calling player for in-game requests, and manages disconnect handling
- **`GameSession`**: owns the active `BattleshipEngine`, player readiness state, and the per-game lock used to serialise state-changing operations within a match

#### Repositories

Game state is stored in memory using `ConcurrentDictionary`-backed repositories:

- `LobbyRepository`
- `GameRepository`
- `ConnectionRepository`

The repositories manage application-level state mappings while each active `GameSession` controls access to mutable game state.

A lobby is removed when it is joined, when its player leaves it, or when its player disconnects. A game is removed when it finishes or when a player disconnects.

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
- **Data:** `Coordinate`, `ShipType`, `ShotResult`, `ShotOutcome`, `GameState`, `PlacementResult`, `FleetValidationResult`

`Coordinate` acts as a value object and prevents coordinates outside the 10×10 board from being created.

The fleet is the standard five ships: Carrier (5), Battleship (4), Destroyer (3), Submarine (3) and Patrol Boat (2).

#### DTOs

Request and response/message DTOs are used to keep the SignalR API separate from the internal game model.

Requests include:

- `CreateLobbyRequest`
- `JoinLobbyRequest`
- `PlaceShipRequest`
- `ShootRequest`

Only `CreateLobbyRequest` and `JoinLobbyRequest` carry a `PlayerId`. `LeaveLobby`, `ValidateFleet`, `TryStartGame` and `ClearBoard` take no arguments.

Messages and responses include:

- `GameCreatedResponse`
- `PlayerSummary`
- `StartGameResponse`
- `StartGameMessage`
- `ShotResponse`
- `ShotMessage`

Shared limits live in `GameLimits`: game codes are 6 characters and player names are at most 20. The validators enforce both, and the client caps the length of its name and game code inputs.

---

## How It Works

1. Player 1 creates a lobby and receives a game code. Their connection is linked to their player ID. They can cancel the lobby at any point before it is joined
2. Player 2 joins using that code, and their connection is linked the same way. An active game session is created and both clients are sent each other's names. From here on the server identifies each player by their connection
3. Both players place their fleets. Ship shape, size, position and placement rules are validated before placement is accepted
4. Each player validates their fleet. If it is valid they are marked as ready, which locks their ships while they wait for the other player
5. The game starts once both players are ready. The server transitions the engine from setup into playing and notifies both clients
6. Player 1 always starts. Players alternate shots and both are notified of the result after each shot
7. When a shot sinks the opponent's last ship, the engine marks the game as finished and the `Shot` broadcast has `IsGameOver` set to true. The shooter (`ShooterId`) is the winner
8. As soon as the game is finished, the server removes it from memory and takes both players out of its SignalR group. Both players can then return home and create or join a new game on the same connection

If a player disconnects during setup or play, the server removes the game straight away and sends `OpponentDisconnected` to the player who is left.

---

## How to Run

### Backend

Ensure you have the .NET 8 SDK installed.
Navigate to the `Battleship.Api` directory.
Run the following command:

```bash
dotnet run --launch-profile https
```

The SignalR hub is exposed at `/gameHub` (`https://localhost:7146/gameHub` when using the `https` profile). The client is hard-wired to this address, so use the `https` profile.

If the browser refuses the connection because of the development certificate, trust it once with:

```bash
dotnet dev-certs https --trust
```

### Frontend

Ensure you have Node.js installed and the backend is running.
Navigate to the `Battleship.Client` directory.
Run the following commands:

```bash
npm install
npm run dev
```

The client runs at `http://localhost:5173`. The API's CORS policy allows only this origin.

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

## Limitations

Version 1 is a complete game, but it has known limits.

### Connections

- **No reconnection.** The server ties a player to their SignalR connection, and a reconnect gets a new connection ID. If your connection drops, your lobby or game is lost, even if the client reconnects a moment later. You are sent to the connection-lost screen and have to start a new game
- **A dropped player ends the game for both.** There is no grace period: the opponent is told as soon as the disconnect is detected, and the game is removed. Refreshing or closing the tab counts as a disconnect

### Hosting

- **Local only.** The hub URL in the client and the allowed CORS origin on the server are hard-coded to `localhost`. There is no configuration for deploying it anywhere else
- **In-memory state.** Lobbies, games and connections live in the server process. Restarting the server ends every game, and nothing is kept once a game finishes
- **Single server instance.** State is not shared between processes and there is no SignalR backplane, so the server cannot be scaled out

### Players

- **No accounts or authentication.** The player ID is a random value made up by the client on each page load, and the server trusts it. There are no profiles, statistics or match history
- **Two human players only.** There is no single-player mode and no spectating

### Gameplay

- **Player 1 always shoots first.** The player who created the lobby gets the first turn
- **No rematch.** After a game, both players go back to the home screen and one has to share a new code
- **No timers.** A turn has no time limit, and a lobby stays open until its creator leaves or disconnects
- **Ships can't be moved one at a time.** During setup the only way to change a placement is to clear the board and place every ship again

### Interface

- **Mouse only for ship placement.** Placement uses HTML drag-and-drop, which does not work on most touch screens and has no keyboard alternative
- **Not designed for small screens.** The layout has no mobile-specific styling

---

## Future Ideas

- **Reconnection:** let a dropped player rejoin their game within a grace period
- **Deployment:** move the hub URL and CORS origin into configuration so the game can be hosted
- **Data Persistence:** move lobby/game/connection storage to PostgreSQL
- **AI Opponent:** optional single-player mode against a basic AI, reusing the existing engine
- **Rematch:** start another game against the same opponent without a new code
- **Touch and keyboard placement:** a way to place ships without drag-and-drop
