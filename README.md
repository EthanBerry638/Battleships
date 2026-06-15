# Battleships

A backend implementation of the classic Battleship game, structured with clean architecture principles.

---

## Project Overview

This project provides an engine to manage Battleship game logic. Including ship state management, input parsing for coordinates and the ability to shoot at a coordinate on the board.

---

## Project Structure

The solution is organized into the following components:

- **Battleship.Api:** Contains the core engine, game pieces (Entities, Data, Board), and input parsers  
- **Battleship.Tests:** Contains unit tests for the engine, board logic, parsers, and ship entities

Core Components
- **Engine:** Handles the primary game logic  
- **GamePieces:**
  - **Entities:** Represents ships on the board
  - **Board:** Manages game grid state and tile interactions
  - **Data:** Defines essential data structures like Coordinate, ShipType, and ShotResult 
  - **Parsers:** Handles conversion of input coordinates into usable data

---

## How to Run

Ensure you have the .NET SDK installed.  
Navigate to the Battleship.Api directory.  
Run the following command:

```bash
dotnet run
```

---

## Testing

To run the test suite, navigate to the repository root directory and execute:

```bash
dotnet test
```