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
        private readonly Mock<IShip> _mockShip = new Mock<IShip>();
        
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
    }
}