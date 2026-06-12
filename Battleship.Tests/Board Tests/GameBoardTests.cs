using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using FluentAssertions;

namespace Battleship.Tests.Board_Tests
{
    public class GameBoardTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(9, 9)]
        public void GetTile_ReturnsTileAtCoordinate_WhenTileExists(int x, int y)
        {
            var gameBoard = new GameBoard();
            var coordinate = new Coordinate(x, y);

            var result = gameBoard.GetTile(coordinate);

            result.Should().NotBeNull();
        }
    }
}