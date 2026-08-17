# Battleships 

A real-time, multiplayer backend implementation of the classic Battleship game, built on ASP.NET Core and SignalR. Capable of hosting multiple independent game lobbies and active matches.

---

## Project Overview 

This project provides a game server for playing Battleship live against another player. It handles lobby creation and joining, ship placement and validation, turn-based shooting, win detection, and disconnect handling.

---

## Tech Stack 

- **.NET 8 / ASP.NET Core**: API host
- **SignalR**: real-time communication between server and clients
- **FluentValidation**: request DTO validation
- **xUnit, Moq, FluentAssertions**: test suite

---

## Project Structure 

The solution is organised into two projects:

- **Battleship.Api**: the game server, including SignalR hubs, services, repositories, game sessions, validation, and core game logic
- **Battleship.Tests**: unit and integration tests covering the application from individual domain behaviour through to a complete multiplayer game

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

Ensure you have the .NET SDK installed.
Navigate to the `Battleship.Api` directory.
Run the following command:

```bash
dotnet run
```

The SignalR hub is exposed at `/gameHub`.

There is currently no GUI, so the backend can be interacted with a tool such as Postman.

---

## Testing 

To run the test suite, navigate to the repository root directory and execute:

```bash
dotnet test
```

---

## Future Ideas

- **Data Persistence:** Transition lobby/game/connection storage to PostgreSQL
- **Front End:** Develop a modern, responsive UI (React) that talks to the SignalR hub
- **AI Opponent:** Optional single-player mode against a basic AI, reusing the existing engine
