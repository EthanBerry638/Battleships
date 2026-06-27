using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Board_Tests
{
    public class GameBoardTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(9, 9)]
        public void GetTile_ReturnsTileAtCoordinate_WhenTileIsInsideBoard(int x, int y)
        {
            var gameBoard = new GameBoard();
            var coordinate = new Coordinate(x, y);

            var result = gameBoard.GetTile(coordinate);

            result.Should().NotBeNull();
        }           

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(9, 9)]
        public void GetTile_ReturnsSameTileInstance_WhenCalledWithSameCoordinate(int x, int y)
        {
            var gameBoard = new GameBoard();
            var coordinate = new Coordinate(x, y);

            var firstResult = gameBoard.GetTile(coordinate);
            var secondResult = gameBoard.GetTile(coordinate);

            secondResult.Should().BeSameAs(firstResult);
        }
        
        [Theory]
        [InlineData(-1, 0)]
        [InlineData(10, 0)]
        [InlineData(0, -1)]  
        [InlineData(0, 10)]
        [InlineData(-1, -1)]
        [InlineData(10, 10)]
        public void GetTile_ThrowsException_WhenCoordinateIsOutsideBoard(int x, int y)
        {
            var gameBoard = new GameBoard();
            var coordinate = new Coordinate(x, y);

            var action = () => gameBoard.GetTile(coordinate);

            action.Should().Throw<InvalidCoordinateException>()
                .WithMessage($"Invalid coordinate: {coordinate}");
        }
        
        [Fact]
        public void PlaceShip_ReturnsSuccess_WhenTilesAreEmpty()
        {
            var gameBoard = new GameBoard();
            List<Coordinate> coordinates = [new(0, 0), new(0, 1), new(0, 2)];
            var ship = new Ship(ShipType.Destroyer, coordinates);
        
            var result = gameBoard.PlaceShip(ship);

            result.IsSuccessful.Should().BeTrue();
            result.InvalidCoordinates.Should().BeNull();
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(ship);
        }
    
        [Fact]
        public void PlaceShip_ReturnsFailure_WhenTileIsAlreadyOccupied()
        {
            var gameBoard = new GameBoard();
            List<Coordinate> existingShipCoordinates = [new(0, 0), new(0, 1), new(0, 2)];
            var existingShip = new Ship(ShipType.Destroyer, existingShipCoordinates);
            gameBoard.PlaceShip(existingShip);
            List<Coordinate> newShipCoordinates = [new(0, 1), new(0, 2), new(0, 3)];
        
            var newShip = new Ship(ShipType.Destroyer, newShipCoordinates);

            var result = gameBoard.PlaceShip(newShip);

            result.IsSuccessful.Should().BeFalse();
            result.InvalidCoordinates.Should().BeEquivalentTo([new Coordinate(0, 1), new Coordinate(0, 2)]);
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().Be(existingShip);
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().Be(existingShip);
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(existingShip);
        }
    
        [Fact]
        public void PlaceShip_OnlyOccupiesShipCoordinates_WhenPlacedOnBoard()
        {
            var gameBoard = new GameBoard();
            List<Coordinate> coordinates = [new(0, 0), new(0, 1), new(0, 2)];
            var ship = new Ship(ShipType.Destroyer, coordinates);

            var result = gameBoard.PlaceShip(ship);

            result.IsSuccessful.Should().BeTrue();
            result.InvalidCoordinates.Should().BeNull();
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 3)).OccupyingShip.Should().BeNull();
            gameBoard.GetTile(new Coordinate(1, 0)).OccupyingShip.Should().BeNull();
        }
    
        [Fact]
        public void PlaceShip_DoesNotPartiallyPlaceShip_WhenPlacementFails()
        {
            var gameBoard = new GameBoard();
            List<Coordinate> existingShipCoordinates = [new(0, 2), new(0, 3)];
            var existingShip = new Ship(ShipType.PatrolBoat, existingShipCoordinates);
            gameBoard.PlaceShip(existingShip);
            List<Coordinate> newShipCoordinates = [new(0, 0), new(0, 1), new(0, 2)];
            var newShip = new Ship(ShipType.Destroyer, newShipCoordinates);
        
            var result = gameBoard.PlaceShip(newShip);
        
            result.IsSuccessful.Should().BeFalse();
            result.InvalidCoordinates.Should().BeEquivalentTo([new Coordinate(0, 2)]);
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().BeNull();
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().BeNull();
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(existingShip);
        }
        
        [Theory]
        [InlineData(0, 0, -1, 0)]
        [InlineData(9, 0, 10, 0)]
        [InlineData(0, 0, 0, -1)]
        [InlineData(0, 9, 0, 10)]
        public void PlaceShip_DoesNotPartiallyPlaceShip_WhenCoordinateIsOutOfBounds(int startX, int startY, int invalidX, int invalidY)
        {
            var gameBoard = new GameBoard();
            List<Coordinate> coordinates = [new(startX, startY), new(invalidX, invalidY)];
            var ship = new Ship(ShipType.PatrolBoat, coordinates);

            var action = () => gameBoard.PlaceShip(ship);

            action.Should().Throw<InvalidCoordinateException>(); // Thrown through GetTile
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().BeNull();
        }

        [Fact]
        public void AreAllShipsSunk_ShouldReturnFalse_WhenNoShipsArePlaced()
        {
            var gameBoard = new GameBoard();

            var result = gameBoard.AreAllShipsSunk();
            
            result.Should().BeFalse();
        }
    }
}