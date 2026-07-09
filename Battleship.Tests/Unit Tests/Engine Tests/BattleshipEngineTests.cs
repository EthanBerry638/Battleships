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
        private readonly Player _player1;
        private readonly Player _player2;
        private readonly Mock<IShip> _mockShip;
        private readonly BattleshipEngine _battleshipEngine;

        public BattleshipEngineTests()
        {
            _mockGameBoard1 = new();
            _mockGameBoard2 = new();
            _player1 = new("Player 1");
            _player2 = new("Player 2");
            _mockShip = new();
            _battleshipEngine = new(_mockGameBoard1.Object, _mockGameBoard2.Object, _player1, _player2);
        }
        
        private void StartGame()
        {
            _mockGameBoard1.Setup(x => x.ValidateFleet()).Returns(new FleetValidationResult(true, [], []));
            _mockGameBoard2.Setup(x => x.ValidateFleet()).Returns(new FleetValidationResult(true, [], []));
            _battleshipEngine.TryStartGame();
        }
        
        [Fact]
        public void Shoot_ReturnsHit_WhenTileHasShip()
        {
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            StartGame();
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
            var player = new Player("Test Player");

            FluentActions.Invoking(() => new BattleshipEngine(null!, board, player, player))
                .Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new BattleshipEngine(board, null!, player, player))
                .Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new BattleshipEngine(board, board, null!, player))
                .Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => new BattleshipEngine(board, board, player, null!))
                .Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(true, false)]  
        [InlineData(false, true)]  
        [InlineData(true, true)]   
        public void Shoot_ShouldThrowGameOverException_WhenAnyPlayersShipsAreAllSunk(bool playerOneSunk, bool playerTwoSunk)
        {
            StartGame();
            _mockGameBoard1.Setup(x => x.AreAllShipsSunk()).Returns(playerOneSunk);
            _mockGameBoard2.Setup(x => x.AreAllShipsSunk()).Returns(playerTwoSunk);
            
            var act = () => _battleshipEngine.Shoot(It.IsAny<Coordinate>());
            
            act.Should()
                .Throw<GameOverException>()
                .WithMessage("Cannot shoot when game is over.");
        }
        
        [Fact]
        public void Shoot_ThrowsException_WhenGameIsInSetupPhase()
        {
            var act = () => _battleshipEngine.Shoot(It.IsAny<Coordinate>());
            
            _battleshipEngine.GameState.Should().Be(GameState.Setup);
            act.Should()
                .Throw<GameNotStartedException>()
                .WithMessage("Cannot shoot when game is not started.");
        }

        [Fact]
        public void TryStartGame_ShouldReturnFalse_WhenGameIsAlreadyStarted()
        {
            _mockGameBoard1.Setup(x => x.ValidateFleet()).Returns(new FleetValidationResult(true, [], []));
            _mockGameBoard2.Setup(x => x.ValidateFleet()).Returns(new FleetValidationResult(true, [], []));

            _battleshipEngine.TryStartGame();
            var result = _battleshipEngine.TryStartGame();

            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().BeNull();
            _battleshipEngine.GameState.Should().Be(GameState.Playing);
            _mockGameBoard1.Verify(x => x.ValidateFleet(), Times.Once);
            _mockGameBoard2.Verify(x => x.ValidateFleet(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(InvalidFleetTestData))]
        public void TryStartGame_ShouldReturnFalse_WhenAnyFleetIsInvalid(
            FleetValidationResult board1Result, 
            FleetValidationResult board2Result)
        {
            _mockGameBoard1.Setup(x => x.ValidateFleet()).Returns(board1Result);
            _mockGameBoard2.Setup(x => x.ValidateFleet()).Returns(board2Result);

            var result = _battleshipEngine.TryStartGame();

            result.Success.Should().BeFalse();
            result.ValidationErrors.Should().NotBeNull();
            _battleshipEngine.GameState.Should().Be(GameState.Setup);
            _mockGameBoard1.Verify(x => x.ValidateFleet(), Times.Once);
            _mockGameBoard2.Verify(x => x.ValidateFleet(), Times.Once);
        }

        public static IEnumerable<object[]> InvalidFleetTestData()
        {
            var invalid = new FleetValidationResult(false, [], []);
            var valid = new FleetValidationResult(true, [], []);

            yield return [invalid, valid];  
            yield return [valid, invalid]; 
            yield return [invalid, invalid]; 
        }

        [Fact]
        public void TryStartGame_ShouldReturnTrue_WhenBothFleetsAreValid()
        {
            var validateFleetResult = new FleetValidationResult(true, [], []);
            _mockGameBoard1.Setup(x => x.ValidateFleet()).Returns(validateFleetResult);
            _mockGameBoard2.Setup(x => x.ValidateFleet()).Returns(validateFleetResult);

            var result = _battleshipEngine.TryStartGame();

            result.Success.Should().BeTrue();
            result.ValidationErrors.Should().BeNull();
            _battleshipEngine.GameState.Should().Be(GameState.Playing);
            _mockGameBoard1.Verify(x => x.ValidateFleet(), Times.Once);
            _mockGameBoard2.Verify(x => x.ValidateFleet(), Times.Once);
        }

        [Fact]
        public void GetWinner_ShouldReturnNull_WhenGameIsNotOver()
        {
            var result = _battleshipEngine.GetWinner();

            result.Should().BeNull();
            _battleshipEngine.GameState.Should().NotBe(GameState.Finished);
        }
        
        [Theory]
        [MemberData(nameof(WinnerTestData))]
        public void GetWinner_ShouldReturnCorrectPlayer_WhenGameIsOver(bool board1Sunk, bool board2Sunk, Player expectedWinner)
        {
            StartGame();
            _mockGameBoard1.Setup(x => x.AreAllShipsSunk()).Returns(board1Sunk);
            _mockGameBoard2.Setup(x => x.AreAllShipsSunk()).Returns(board2Sunk);

            var result = _battleshipEngine.GetWinner();

            result.Should().Be(expectedWinner);
            _battleshipEngine.GameState.Should().Be(GameState.Finished);
        }

        public static IEnumerable<object[]> WinnerTestData() =>
        [
            [true,  false, new Player("Player 2")],  
            [false, true,  new Player("Player 1")], 
        ];
    }
}