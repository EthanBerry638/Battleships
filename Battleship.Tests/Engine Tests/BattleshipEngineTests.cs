using Battleship.Api.GamePieces.Board;
using Moq;
using Battleship.Api.Engine;

namespace Battleship.Tests.Engine_Tests
{
    public class BattleshipEngineTests
    {
        private readonly Mock<IGameBoard> _mockGameBoard;
        private readonly BattleshipEngine _battleshipEngine;

        public BattleshipEngineTests()
        {
            _mockGameBoard = new Mock<IGameBoard>();
            _battleshipEngine = new BattleshipEngine(_mockGameBoard.Object);
        }
    }
}