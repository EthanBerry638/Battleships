using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Moq;
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
            var coordinates = new List<Coordinate> { new(0, 0), new(0, 1), new(0, 2) };
            var ship = new Ship(coordinates);
            
            var result = gameBoard.PlaceShip(ship);

            result.IsSuccessful.Should().BeTrue();
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(ship);
        }
        
        [Fact]
        public void PlaceShip_ReturnsFailure_WhenTileIsAlreadyOccupied()
        {
            var gameBoard = new GameBoard();
            var existingShipCoordinates = new List<Coordinate> { new(0, 0), new(0, 1), new(0, 2) };
            var existingShip = new Ship(existingShipCoordinates);
            gameBoard.PlaceShip(existingShip);
            var newShipCoordinates = new List<Coordinate> { new(0, 1), new(0, 2), new(0, 3) };
            
            var newShip = new Ship(newShipCoordinates);

            var result = gameBoard.PlaceShip(newShip);

            result.IsSuccessful.Should().BeFalse();
            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().Be(existingShip);
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().Be(existingShip);
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(existingShip);
        }
        
        [Fact]
        public void PlaceShip_OnlyOccupiesShipCoordinates_WhenPlacedOnBoard()
        {
            var gameBoard = new GameBoard();
            var coordinates = new List<Coordinate> { new(0, 0), new(0, 1), new(0, 2) };
            var ship = new Ship(coordinates);

            gameBoard.PlaceShip(ship);

            gameBoard.GetTile(new Coordinate(0, 0)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 1)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 2)).OccupyingShip.Should().Be(ship);
            gameBoard.GetTile(new Coordinate(0, 3)).OccupyingShip.Should().BeNull();
            gameBoard.GetTile(new Coordinate(1, 0)).OccupyingShip.Should().BeNull();
        }
    }
}