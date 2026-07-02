using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using FluentAssertions;
using Moq;

namespace Battleship.Tests.Unit_Tests.Engine_Tests
{
    public class BattleshipEngineTests
    {
        private readonly Mock<IGameBoard> _mockGameBoard1;
        private readonly Mock<IGameBoard> _mockGameBoard2;
        private readonly Mock<IPlayer> _mockPlayer1;
        private readonly Mock<IPlayer> _mockPlayer2;
        private readonly Mock<IShip> _mockShip;
        private readonly BattleshipEngine _battleshipEngine;

        public BattleshipEngineTests()
        {
            _mockGameBoard1 = new();
            _mockGameBoard2 = new();
            _mockPlayer1 = new();
            _mockPlayer2 = new();
            _mockShip = new();
            _battleshipEngine = new(_mockGameBoard1.Object, _mockGameBoard2.Object, _mockPlayer1.Object, _mockPlayer2.Object);
        }

        // START OF SINGLE BOARD/PLAYER TESTS
        
        [Fact]
        public void Shoot_ReturnsHit_WhenTileHasShip()
        {
            var coordinate = new Coordinate(0, 0);
            _mockShip.Setup(s => s.IsSunk()).Returns(false);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
        
            var result = _battleshipEngine.Shoot(coordinate);
        
            result.Should().Be(ShotResult.Hit);
        
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }   
    
        [Fact]
        public void Shoot_ReturnsMiss_WhenTileHasNoShip()
        {
            var coordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
    
            var result = _battleshipEngine.Shoot(coordinate);

            result.Should().Be(ShotResult.Miss);

            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Never);
            _mockShip.Verify(s => s.IsSunk(), Times.Never);
        }
    
        [Fact]
        public void Shoot_ReturnsSunk_WhenShipIsDestroyed()
        {
            var coordinate = new Coordinate(0, 0);
            _mockShip.Setup(s => s.IsSunk()).Returns(true);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
    
            var result = _battleshipEngine.Shoot(coordinate);
    
            result.Should().Be(ShotResult.Sunk);
    
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
        [Fact]
        public void Shoot_ReturnsDuplicate_WhenSameHitCoordinateIsShotTwice()
        {
            var coordinate = new Coordinate(0, 0);
            _mockShip.Setup(s => s.IsSunk()).Returns(false);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);

            var firstResult = _battleshipEngine.Shoot(coordinate);
            var secondResult = _battleshipEngine.Shoot(coordinate);

            firstResult.Should().Be(ShotResult.Hit);
            secondResult.Should().Be(ShotResult.Duplicate);

            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
        [Fact]
        public void Shoot_ReturnsDuplicate_WhenSameMissCoordinateIsShotTwice()
        {
            var coordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);

            var firstResult = _battleshipEngine.Shoot(coordinate);
            var secondResult = _battleshipEngine.Shoot(coordinate);

            firstResult.Should().Be(ShotResult.Miss);
            secondResult.Should().Be(ShotResult.Duplicate);

            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
        }
        
        [Fact]
        public void Shoot_ReturnsDuplicate_WhenAlreadySunkCoordinateIsShotAgain()
        {
            var coordinate = new Coordinate(0, 0);
            _mockShip.Setup(s => s.IsSunk()).Returns(true);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);

            var firstResult = _battleshipEngine.Shoot(coordinate);
            var secondResult = _battleshipEngine.Shoot(coordinate);

            firstResult.Should().Be(ShotResult.Sunk);
            secondResult.Should().Be(ShotResult.Duplicate);

            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
        // END OF SINGLE BOARD/PLAYER TESTS
        
        // START OF MULTI BOARD/PLAYER TESTS

        [Fact]
        public void Shoot_OnlyAffectsPlayer2sBoard_WhenShootIsCalledOnceByPlayer1AndIsSuccessful()
        {
            var coordinate = new Coordinate(0, 0);
            var tile = new  Tile { OccupyingShip = _mockShip.Object };
            _mockShip.Setup(s => s.IsSunk()).Returns(true);
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
            
            var result = _battleshipEngine.Shoot(coordinate);
            
            result.Should().Be(ShotResult.Sunk);
            
            _mockGameBoard1.Verify(x => x.GetTile(coordinate), Times.Never);
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }

        [Fact]
        public void Shoot_OnlyAffectsPlayer1sBoard_WhenShootIsCalledByPlayer2AndIsSuccessful()
        {
            var player1Coordinate = new Coordinate(1, 1);
            var player2Coordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockShip.Setup(s => s.IsSunk()).Returns(true);
            _mockGameBoard2.Setup(x => x.GetTile(player1Coordinate)).Returns(new Tile { OccupyingShip = null });
            _mockGameBoard1.Setup(x => x.GetTile(player2Coordinate)).Returns(tile);

            _battleshipEngine.Shoot(player1Coordinate); // Player 1 shoots (miss), advances turn to player 2
            var result = _battleshipEngine.Shoot(player2Coordinate); // Player 2 shoots

            result.Should().Be(ShotResult.Sunk);

            _mockGameBoard1.Verify(x => x.GetTile(player2Coordinate), Times.Once);
            _mockGameBoard2.Verify(x => x.GetTile(player2Coordinate), Times.Never);
            _mockShip.Verify(s => s.RegisterHit(player2Coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
        [Fact]
        public void Shoot_DoesNotSwitchTurn_WhenDuplicateShotIsFired()
        {
            var player1FirstCoordinate = new Coordinate(0, 0);
            var player2Coordinate = new Coordinate(1, 1);
            var player1SecondCoordinate = new Coordinate(2, 2);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard2.Setup(x => x.GetTile(player1FirstCoordinate)).Returns(tile);
            _mockGameBoard1.Setup(x => x.GetTile(player2Coordinate)).Returns(tile);
            _mockGameBoard2.Setup(x => x.GetTile(player1SecondCoordinate)).Returns(tile);
            
            var player1FirstMiss = _battleshipEngine.Shoot(player1FirstCoordinate);   // Player 1 shoots, turn switches to player 2
            var player2Miss = _battleshipEngine.Shoot(player2Coordinate);             // Player 2 shoots, turn switches to player 1
            var player1Duplicate = _battleshipEngine.Shoot(player1FirstCoordinate);   // Player 1 shoots duplicate, turn should NOT switch
            var player1SecondMiss = _battleshipEngine.Shoot(player1SecondCoordinate); // Still player 1's turn
            
            player1FirstMiss.Should().Be(ShotResult.Miss);
            player2Miss.Should().Be(ShotResult.Miss);
            player1Duplicate.Should().Be(ShotResult.Duplicate);
            player1SecondMiss.Should().Be(ShotResult.Miss);

            _mockGameBoard2.Verify(x => x.GetTile(player1FirstCoordinate), Times.Once);
            _mockGameBoard2.Verify(x => x.GetTile(player1SecondCoordinate), Times.Once);
            _mockGameBoard1.Verify(x => x.GetTile(player2Coordinate), Times.Once);
            _mockGameBoard1.Verify(x => x.GetTile(player1SecondCoordinate), Times.Never);
            _mockGameBoard1.Verify(x => x.GetTile(player1FirstCoordinate), Times.Never);
        }

        [Fact]
        public void Shoot_ShouldTrackDuplicatesSeparately_WhenPlayer1And2TakeDuplicateShots()
        {
            var coordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard1.Setup(x => x.GetTile(coordinate)).Returns(tile);
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
            
            var player1Miss = _battleshipEngine.Shoot(coordinate);
            var player2Miss = _battleshipEngine.Shoot(coordinate);
            var player1Duplicate = _battleshipEngine.Shoot(coordinate);
            var player2Duplicate = _battleshipEngine.Shoot(coordinate);
            
            player1Miss.Should().Be(ShotResult.Miss);
            player2Miss.Should().Be(ShotResult.Miss);
            player1Duplicate.Should().Be(ShotResult.Duplicate);
            player2Duplicate.Should().Be(ShotResult.Duplicate);
            
            _mockGameBoard1.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
        }
        
        // END OF MULTI BOARD/PLAYER TESTS
    }
}