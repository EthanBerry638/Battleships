using Battleship.Api.GamePieces.Board;
using Moq;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using FluentAssertions;

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

        [Fact]
        public void Shoot_ReturnsHit_WhenTileHasShip()
        {
            var coordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = new Ship() };
            _mockGameBoard.Setup(x => x.GetTile(coordinate)).Returns(tile);
            
            var result = _battleshipEngine.Shoot(coordinate);
            
            result.Should().Be(ShotResult.Hit);
            
            _mockGameBoard.Verify(x => x.GetTile(coordinate), Times.Once);
        }   
        
        
        [Fact]
        public void Shoot_ReturnsSunk_WhenShipIsDestroyed()
        {
            var coordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = new Ship() }; 
            _mockGameBoard.Setup(x => x.GetTile(coordinate)).Returns(tile);
        
            var result = _battleshipEngine.Shoot(coordinate);
        
            result.Should().Be(ShotResult.Sunk);
        
            _mockGameBoard.Verify(x => x.GetTile(coordinate), Times.Once);
        }
    }
}