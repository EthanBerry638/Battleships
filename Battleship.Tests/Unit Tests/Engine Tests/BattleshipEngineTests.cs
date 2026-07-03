using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using FluentAssertions;
using Moq;
using Battleship.Api.Exceptions;

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
            var player2Coordinate = new Coordinate(1, 1);
            _mockShip.Setup(s => s.IsSunk()).Returns(false);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
            _mockGameBoard1.Setup(x => x.GetTile(player2Coordinate)).Returns(new Tile { OccupyingShip = null });

            var firstResult = _battleshipEngine.Shoot(coordinate);       
            _battleshipEngine.Shoot(player2Coordinate);                   
            var secondResult = _battleshipEngine.Shoot(coordinate);      

            firstResult.Should().Be(ShotResult.Hit);
            secondResult.Should().Be(ShotResult.Duplicate);
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockGameBoard1.Verify(x => x.GetTile(player2Coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
        [Fact]
        public void Shoot_ReturnsDuplicate_WhenSameMissCoordinateIsShotTwice()
        {
            var coordinate = new Coordinate(0, 0);
            var player2Coordinate = new Coordinate(1, 1);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
            _mockGameBoard1.Setup(x => x.GetTile(player2Coordinate)).Returns(tile);

            var firstResult = _battleshipEngine.Shoot(coordinate);        
            _battleshipEngine.Shoot(player2Coordinate);                   
            var secondResult = _battleshipEngine.Shoot(coordinate);       

            firstResult.Should().Be(ShotResult.Miss);
            secondResult.Should().Be(ShotResult.Duplicate);
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockGameBoard1.Verify(x => x.GetTile(player2Coordinate), Times.Once);
        }

        [Fact]
        public void Shoot_ReturnsDuplicate_WhenAlreadySunkCoordinateIsShotAgain()
        {
            var coordinate = new Coordinate(0, 0);
            var player2Coordinate = new Coordinate(1, 1);
            _mockShip.Setup(s => s.IsSunk()).Returns(true);
            var tile = new Tile { OccupyingShip = _mockShip.Object };
            _mockGameBoard2.Setup(x => x.GetTile(coordinate)).Returns(tile);
            _mockGameBoard1.Setup(x => x.GetTile(player2Coordinate)).Returns(new Tile { OccupyingShip = null });

            var firstResult = _battleshipEngine.Shoot(coordinate);        
            _battleshipEngine.Shoot(player2Coordinate);                  
            var secondResult = _battleshipEngine.Shoot(coordinate);       

            firstResult.Should().Be(ShotResult.Sunk);
            secondResult.Should().Be(ShotResult.Duplicate);
            _mockGameBoard2.Verify(x => x.GetTile(coordinate), Times.Once);
            _mockGameBoard1.Verify(x => x.GetTile(player2Coordinate), Times.Once);
            _mockShip.Verify(s => s.RegisterHit(coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
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

            _battleshipEngine.Shoot(player1Coordinate); 
            var result = _battleshipEngine.Shoot(player2Coordinate); 

            result.Should().Be(ShotResult.Sunk);
            _mockGameBoard1.Verify(x => x.GetTile(player2Coordinate), Times.Once);
            _mockGameBoard2.Verify(x => x.GetTile(player2Coordinate), Times.Never);
            _mockShip.Verify(s => s.RegisterHit(player2Coordinate), Times.Once);
            _mockShip.Verify(s => s.IsSunk(), Times.Once);
        }
        
        [Fact]
        public void Shoot_SwitchesTurn_WhenDuplicateShotIsFired()
        {
            var player1FirstCoordinate = new Coordinate(0, 0);
            var player2FirstCoordinate = new Coordinate(1, 1);
            var player2SecondCoordinate = new Coordinate(1, 2);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard1.Setup(x => x.GetTile(player2FirstCoordinate)).Returns(tile);
            _mockGameBoard1.Setup(x => x.GetTile(player2SecondCoordinate)).Returns(tile);
            _mockGameBoard2.Setup(x => x.GetTile(player1FirstCoordinate)).Returns(tile);
            
            var player1FirstMiss= _battleshipEngine.Shoot(player1FirstCoordinate);
            var player2FirstMiss = _battleshipEngine.Shoot(player2FirstCoordinate);
            var player1SecondDuplicateShot = _battleshipEngine.Shoot(player1FirstCoordinate);
            var player2SecondMiss = _battleshipEngine.Shoot(player2SecondCoordinate);
            
            player1FirstMiss.Should().Be(ShotResult.Miss);
            player2FirstMiss.Should().Be(ShotResult.Miss);
            player1SecondDuplicateShot.Should().Be(ShotResult.Duplicate);
            player2SecondMiss.Should().Be(ShotResult.Miss);
            _mockGameBoard1.Verify(x => x.GetTile(player2FirstCoordinate), Times.Once);
            _mockGameBoard1.Verify(x => x.GetTile(player2SecondCoordinate), Times.Once);
            _mockGameBoard2.Verify(x => x.GetTile(player1FirstCoordinate), Times.Once);
        }

        [Fact]
        public void Shoot_ShouldTrackDuplicatesSeparately_WhenPlayer1And2TakeDuplicateShots()
        {
            var firstCoordinate = new Coordinate(0, 0);
            var tile = new Tile { OccupyingShip = null };
            _mockGameBoard1.Setup(x => x.GetTile(firstCoordinate)).Returns(tile);
            _mockGameBoard2.Setup(x => x.GetTile(firstCoordinate)).Returns(tile);
            
            var player1Miss = _battleshipEngine.Shoot(firstCoordinate);
            var player2Miss = _battleshipEngine.Shoot(firstCoordinate);
            var player1Duplicate = _battleshipEngine.Shoot(firstCoordinate);
            var player2Duplicate = _battleshipEngine.Shoot(firstCoordinate);
            
            player1Miss.Should().Be(ShotResult.Miss);
            player2Miss.Should().Be(ShotResult.Miss);
            player1Duplicate.Should().Be(ShotResult.Duplicate);
            player2Duplicate.Should().Be(ShotResult.Duplicate);
            _mockGameBoard1.Verify(x => x.GetTile(firstCoordinate), Times.Once);
            _mockGameBoard2.Verify(x => x.GetTile(firstCoordinate), Times.Once);
        }
        
        [Fact]
        public void Shoot_PropagatesInvalidCoordinateException_WhenBoardThrowsIt()
        {
            var coordinate = new Coordinate(-1, 5);
            _mockGameBoard2
                .Setup(x => x.GetTile(coordinate))
                .Throws(new InvalidCoordinateException($"Invalid coordinate: {coordinate}"));

            var act = () => _battleshipEngine.Shoot(coordinate);

            act.Should().Throw<InvalidCoordinateException>().WithMessage($"Invalid coordinate: {coordinate}");
        }
        
        [Fact]
        public void BattleShipEngineConstructor_ShouldThrowArgumentNullException_WhenAnyArgumentIsNull()
        {
            var board = new Mock<IGameBoard>().Object;
            var player = new Mock<IPlayer>().Object;

            FluentActions.Invoking(() => new BattleshipEngine(null!, board, player, player))
                .Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new BattleshipEngine(board, null!, player, player))
                .Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new BattleshipEngine(board, board, null!, player))
                .Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new BattleshipEngine(board, board, player, null!))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Shoot_ShouldThrowGameOverException_WhenAllOfPlayers1sShipsAreSunk()
        {
            _mockGameBoard1.Setup(x => x.AreAllShipsSunk()).Returns(true);
            _mockGameBoard2.Setup(x => x.AreAllShipsSunk()).Returns(false);

            var act = () => _battleshipEngine.Shoot(new Coordinate(0, 0));

            act.Should()
                .Throw<GameOverException>()
                .WithMessage("Cannot shoot. Player 2 has sunk all of Player 1's ships.");
            _mockGameBoard1.Verify(x => x.AreAllShipsSunk(), Times.Once);
        }

        [Fact]
        public void Shoot_ShouldThrowGameOverException_WhenAllOfPLayer2sShipsAreSunk()
        {
            _mockGameBoard1.Setup(x => x.AreAllShipsSunk()).Returns(false);
            _mockGameBoard2.Setup(x => x.AreAllShipsSunk()).Returns(true);

            var act = () => _battleshipEngine.Shoot(new Coordinate(0, 0));

            act.Should()
                .Throw<GameOverException>()
                .WithMessage("Cannot shoot. Player 1 has sunk all of Player 2's ships.");
            _mockGameBoard2.Verify(x => x.AreAllShipsSunk(), Times.Once);
        }
    }
}