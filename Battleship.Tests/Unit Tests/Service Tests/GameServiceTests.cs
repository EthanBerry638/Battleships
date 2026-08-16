using Battleship.Api.DTOs;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.Engine;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Repositories;
using Battleship.Api.Services;
using FluentAssertions;
using Moq;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _gameService = new GameService(_gameRepositoryMock.Object);
    }

    private static (GameSession session, Player player1, Player player2, Mock<IGameBoard> board1Mock, Mock<IGameBoard> board2Mock) CreateSession()
    {
        var player1 = new Player(Guid.NewGuid(), "Player 1");
        var player2 = new Player(Guid.NewGuid(), "Player 2");

        var board1 = new Mock<IGameBoard>();
        var board2 = new Mock<IGameBoard>();

        board1.Setup(b => b.PlaceShip(It.IsAny<Ship>())).Returns(new PlacementResult(true));
        board2.Setup(b => b.PlaceShip(It.IsAny<Ship>())).Returns(new PlacementResult(true));

        board1.Setup(b => b.ValidateFleet()).Returns(new FleetValidationResult(true, [], []));
        board2.Setup(b => b.ValidateFleet()).Returns(new FleetValidationResult(true, [], []));

        var engine = new BattleshipEngine(board1.Object, board2.Object, player1, player2);
        var session = new GameSession(engine);

        return (session, player1, player2, board1, board2);
    }

    private void SetupPlayerFoundInGame(Guid playerId, string gameCode, GameSession session)
    {
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out gameCode!))
            .Returns(true);

        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out session!))
            .Returns(true);
    }

    [Fact]
    public void PlaceShip_ShouldReturnSuccessfulResult_WhenShipPlacedOnValidCoordinates()
    {
        var (session, player1, _, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player1.Id, gameCode, session);
        var request = new PlaceShipRequest(player1.Id, ShipType.PatrolBoat, [new Coordinate(0, 0), new Coordinate(0, 1)]);

        var result = _gameService.PlaceShip(request);

        result.IsSuccessful.Should().BeTrue();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldReturnFailedResult_WhenShipCoordinatesAreAlreadyOccupied()
    {
        var (session, player1, _, board1Mock, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player1.Id, gameCode, session);
        var coordinates = new List<Coordinate> { new Coordinate(0, 0), new Coordinate(0, 1) };
        var failureResult = new PlacementResult(false, coordinates);
        board1Mock.Setup(b => b.PlaceShip(It.IsAny<Ship>())).Returns(failureResult);

        var result = _gameService.PlaceShip(new PlaceShipRequest(player1.Id, ShipType.PatrolBoat, coordinates));

        result.IsSuccessful.Should().BeFalse();
        result.InvalidCoordinates.Should().BeEquivalentTo(coordinates);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldRouteToCorrectEngine_WhenMultipleGamesAreActive()
    {
        var (session1, player1, _, _, _) = CreateSession();
        var (session2, player3, _, _, _) = CreateSession();
        string gameCode1 = "GAME1";
        string gameCode2 = "GAME2";
        SetupPlayerFoundInGame(player1.Id, gameCode1, session1);
        SetupPlayerFoundInGame(player3.Id, gameCode2, session2);

        var result = _gameService.PlaceShip(new PlaceShipRequest(player1.Id, ShipType.PatrolBoat, [new Coordinate(0, 0), new Coordinate(0, 1)]));
        var resultInOtherGame = _gameService.PlaceShip(new PlaceShipRequest(player3.Id, ShipType.PatrolBoat, [new Coordinate(0, 0), new Coordinate(0, 1)]));

        result.IsSuccessful.Should().BeTrue();
        resultInOtherGame.IsSuccessful.Should().BeTrue();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode1!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode1, out session1), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player3.Id, out gameCode2!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode2, out session2), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldThrowPlayerNotFoundException_WhenPlayerIsNotInAnyActiveGame()
    {
        var playerId = Guid.NewGuid();
        string? nullCode = null;
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out nullCode))
            .Returns(false);
        var request = new PlaceShipRequest(playerId, ShipType.PatrolBoat, [new Coordinate(0, 0), new Coordinate(0, 1)]);

        var act = () => _gameService.PlaceShip(request);

        act.Should()
            .Throw<PlayerNotFoundException>()
            .WithMessage($"No active game found for player with id {playerId}.");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out nullCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(It.IsAny<string>(), out It.Ref<GameSession?>.IsAny), Times.Never);
    }

    [Fact]
    public void PlaceShip_ShouldThrowGameNotFoundException_WhenGameCodeExistsButSessionNotFound()
    {
        var playerId = Guid.NewGuid();
        string? gameCode = "GHOST";
        GameSession? nullSession = null;
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out gameCode))
            .Returns(true);
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out nullSession))
            .Returns(false);
        var request = new PlaceShipRequest(playerId, ShipType.PatrolBoat, [new Coordinate(0, 0), new Coordinate(0, 1)]);

        var act = () => _gameService.PlaceShip(request);

        act.Should()
            .Throw<GameNotFoundException>()
            .WithMessage($"Game by game code: {gameCode} not found.");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out gameCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out nullSession), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldReturnSuccessfulResult_WhenPlayer2PlacesShipOnValidCoordinates()
    {
        var (session, _, player2, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player2.Id, gameCode, session);
        var request = new PlaceShipRequest(player2.Id, ShipType.PatrolBoat, [new Coordinate(0, 0), new Coordinate(0, 1)]);

        var result = _gameService.PlaceShip(request);

        result.IsSuccessful.Should().BeTrue();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player2.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldReturnFailedResult_WhenPlayer2CoordinatesAreAlreadyOccupied()
    {
        var (session, _, player2, _, board2Mock) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player2.Id, gameCode, session);
        var coordinates = new List<Coordinate> { new Coordinate(0, 0), new Coordinate(0, 1) };
        var failureResult = new PlacementResult(false, coordinates);
        board2Mock.Setup(b => b.PlaceShip(It.IsAny<Ship>())).Returns(failureResult);

        var result = _gameService.PlaceShip(new PlaceShipRequest(player2.Id, ShipType.PatrolBoat, coordinates));

        result.IsSuccessful.Should().BeFalse();
        result.InvalidCoordinates.Should().BeEquivalentTo(coordinates);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player2.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldThrowFleetLockedException_WhenPlayerTriesToPlaceShipAfterReadying()
    {
        var (session, player1, _, board1Mock, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player1.Id, gameCode, session);
        session.SetPlayerReady(player1.Id);
        
        var act = () => _gameService.PlaceShip(new PlaceShipRequest(player1.Id, ShipType.PatrolBoat,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));
        
        act.Should().Throw<FleetLockedException>()
            .WithMessage("You can't place a ship after readying.");
    }

    [Fact]
    public void TryStartGame_ShouldThrowPlayerNotFoundException_WhenPlayerIsNotInAnyActiveGame()
    {
        var playerId = Guid.NewGuid();
        string? nullCode = null;
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out nullCode))
            .Returns(false);

        var act = () => _gameService.TryStartGame(playerId);

        act.Should()
            .Throw<PlayerNotFoundException>()
            .WithMessage($"No active game found for player with id {playerId}.");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out nullCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(It.IsAny<string>(), out It.Ref<GameSession?>.IsAny), Times.Never);
    }

    [Fact]
    public void TryStartGame_ShouldThrowGameNotFoundException_WhenGameCodeExistsButSessionNotFound()
    {
        var playerId = Guid.NewGuid();
        string? gameCode = "GHOST";
        GameSession? nullSession = null;
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out gameCode))
            .Returns(true);
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out nullSession))
            .Returns(false);

        var act = () => _gameService.TryStartGame(playerId);

        act.Should()
            .Throw<GameNotFoundException>()
            .WithMessage($"Game by game code: {gameCode} not found.");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out gameCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out nullSession), Times.Once);
    }

    [Fact]
    public void TryStartGame_ShouldReturnWaitingForOpponent_WhenOnlyPlayerOneIsReady()
    {
        var (session, player1, _, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player1.Id, gameCode, session);
        _gameService.ValidateFleet(player1.Id);
        
        var result = _gameService.TryStartGame(player1.Id);

        result.IsStarted.Should().BeFalse();
        result.GameCode.Should().BeNull();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode!), Times.Exactly(2));
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Exactly(2));
    }
    
    [Fact]
    public void TryStartGame_ShouldReturnWaitingForOpponent_WhenOnlyPlayerTwoIsReady()
    {
        var (session, _, player2, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player2.Id, gameCode, session);
        _gameService.ValidateFleet(player2.Id);

        var result = _gameService.TryStartGame(player2.Id);

        result.IsStarted.Should().BeFalse();
        result.GameCode.Should().BeNull();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player2.Id, out gameCode!), Times.Exactly(2));
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Exactly(2));
    }

    [Fact]
    public void TryStartGame_ShouldStartGame_WhenBothPlayersAreReady()
    {
        var (session, player1, player2, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player1.Id, gameCode, session);
        SetupPlayerFoundInGame(player2.Id, gameCode, session);
        
        _gameService.ValidateFleet(player1.Id);
        var waitResult = _gameService.TryStartGame(player1.Id);
        _gameService.ValidateFleet(player2.Id);
        var result = _gameService.TryStartGame(player2.Id);

        waitResult.IsStarted.Should().BeFalse();
        waitResult.GameCode.Should().BeNull();
        result.GameCode.Should().Be(gameCode);
        result.IsStarted.Should().BeTrue();
        result.StartingPlayerId.Should().NotBeNull();
        result.PlayerIds.Should().HaveCount(2);
    }

    [Fact]
    public void TryStartGame_ShouldRouteToCorrectSession_WhenMultipleGamesAreActive()
    {
        var (session1, player1, _, _, _) = CreateSession();
        var (session2, player3, _, _, _) = CreateSession();
        string gameCode1 = "GAME1";
        string gameCode2 = "GAME2";
        SetupPlayerFoundInGame(player1.Id, gameCode1, session1);
        SetupPlayerFoundInGame(player3.Id, gameCode2, session2);

        var result1 = _gameService.TryStartGame(player1.Id);
        var result2 = _gameService.TryStartGame(player3.Id);
        
        result1.GameCode.Should().BeNull();
        result1.IsStarted.Should().BeFalse();
        result2.GameCode.Should().BeNull();
        result2.IsStarted.Should().BeFalse();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode1!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode1, out session1), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player3.Id, out gameCode2!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode2, out session2), Times.Once);
    }
    
    [Fact]
    public void GetWinner_ShouldThrowGameNotFoundException_WhenGameNotFound()
    {
        var act = () => _gameService.GetWinner("GAME1");

        act.Should()
            .Throw<GameNotFoundException>()
            .WithMessage("Game by game code: GAME1 not found.");
        _gameRepositoryMock.Verify(g => g.TryGetGameByCode("GAME1", out It.Ref<GameSession?>.IsAny), Times.Once);
    }
    
    [Fact]
    public void GetWinner_ShouldReturnPlayer2_WhenPlayer1FleetIsAllSunk()
    {
        var (session, player1, player2, board1Mock, _) = CreateSession();
        string gameCode = "GAME1";
        GameSession? outSession = session;
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out outSession))
            .Returns(true);
        session.Engine.StartGame();
        board1Mock.Setup(b => b.AreAllShipsSunk()).Returns(true);

        var result = _gameService.GetWinner(gameCode);

        result.Should().Be(player2);
        _gameRepositoryMock.Verify(g => g.TryGetGameByCode(gameCode, out It.Ref<GameSession?>.IsAny), Times.Once);
        board1Mock.Verify(b => b.AreAllShipsSunk(), Times.Once);
    }

    [Fact]
    public void GetWinner_ShouldReturnPlayer1_WhenPlayer2FleetIsAllSunk()
    {
        var (session, player1, _, _, board2Mock) = CreateSession();
        string gameCode = "GAME1";
        GameSession? outSession = session;
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out outSession))
            .Returns(true);
        session.Engine.StartGame();
        board2Mock.Setup(b => b.AreAllShipsSunk()).Returns(true);
        
        var result = _gameService.GetWinner(gameCode);
        
        result.Should().Be(player1);
        _gameRepositoryMock.Verify(g => g.TryGetGameByCode(gameCode, out It.Ref<GameSession?>.IsAny), Times.Once);
        board2Mock.Verify(b => b.AreAllShipsSunk(), Times.Once);
    }

    [Fact]
    public void GetWinner_ShouldReturnNull_WhenNeitherPlayersFleetsAreSunk()
    {
        var (session, _, _, board1Mock, board2Mock) = CreateSession();
        string gameCode = "GAME1";
        GameSession? outSession = session;
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out outSession))
            .Returns(true);
        session.Engine.StartGame();
        board1Mock.Setup(b => b.AreAllShipsSunk()).Returns(false);
        board2Mock.Setup(b => b.AreAllShipsSunk()).Returns(false);
        
        var result = _gameService.GetWinner(gameCode);

        result.Should().BeNull();
        _gameRepositoryMock.Verify(g => g.TryGetGameByCode(gameCode, out It.Ref<GameSession?>.IsAny), Times.Once);
        board1Mock.Verify(b => b.AreAllShipsSunk(), Times.Once);
        board2Mock.Verify(b => b.AreAllShipsSunk(), Times.Once);
    }
    
    [Fact]
    public void GetWinner_ShouldReturnNull_WhenGameHasNotStarted()
    {
        var (session, _, _, _, _) = CreateSession();
        string gameCode = "GAME1";
        GameSession? outSession = session;
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out outSession))
            .Returns(true);

        var result = _gameService.GetWinner(gameCode);

        result.Should().BeNull();
        _gameRepositoryMock.Verify(g => g.TryGetGameByCode(gameCode, out It.Ref<GameSession?>.IsAny), Times.Once);
    }

    [Fact]
    public void ValidateFleet_ShouldThrowPlayerNotFoundException_WhenPlayerIsNotInAnyActiveGame()
    {
        var playerId = Guid.NewGuid();
        string? nullCode = null;
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out nullCode))
            .Returns(false);

        var act = () => _gameService.ValidateFleet(playerId);

        act.Should()
            .Throw<PlayerNotFoundException>()
            .WithMessage($"No active game found for player with id {playerId}.");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out nullCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(It.IsAny<string>(), out It.Ref<GameSession?>.IsAny), Times.Never);
    }

    [Fact]
    public void ValidateFleet_ShouldThrowGameNotFoundException_WhenGameCodeExistsButSessionNotFound()
    {
        var playerId = Guid.NewGuid();
        string? gameCode = "GHOST";
        GameSession? nullSession = null;
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out gameCode))
            .Returns(true);
        _gameRepositoryMock
            .Setup(r => r.TryGetGameByCode(gameCode, out nullSession))
            .Returns(false);

        var act = () => _gameService.ValidateFleet(playerId);

        act.Should()
            .Throw<GameNotFoundException>()
            .WithMessage($"Game by game code: {gameCode} not found.");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out gameCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out nullSession), Times.Once);
    }

    [Fact]
    public void ValidateFleet_ShouldReturnValidResult_WhenPlayer1FleetIsValid()
    {
        var (session, player1, _, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player1.Id, gameCode, session);

        var result = _gameService.ValidateFleet(player1.Id);

        result.IsValid.Should().BeTrue();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void ValidateFleet_ShouldReturnValidResult_WhenPlayer2FleetIsValid()
    {
        var (session, _, player2, _, _) = CreateSession();
        string gameCode = "GAME1";
        SetupPlayerFoundInGame(player2.Id, gameCode, session);

        var result = _gameService.ValidateFleet(player2.Id);

        result.IsValid.Should().BeTrue();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player2.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void ValidateFleet_ShouldReturnInvalidResult_WhenPlayer1FleetIsInvalid()
    {
        var (session, player1, _, board1Mock, _) = CreateSession();
        string gameCode = "GAME1";
        board1Mock.Setup(b => b.ValidateFleet()).Returns(new FleetValidationResult(false, [], []));
        SetupPlayerFoundInGame(player1.Id, gameCode, session);

        var result = _gameService.ValidateFleet(player1.Id);

        result.IsValid.Should().BeFalse();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void ValidateFleet_ShouldReturnInvalidResult_WhenPlayer2FleetIsInvalid()
    {
        var (session, _, player2, _, board2Mock) = CreateSession();
        string gameCode = "GAME1";
        board2Mock.Setup(b => b.ValidateFleet()).Returns(new FleetValidationResult(false, [], []));
        SetupPlayerFoundInGame(player2.Id, gameCode, session);

        var result = _gameService.ValidateFleet(player2.Id);

        result.IsValid.Should().BeFalse();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player2.Id, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode, out session), Times.Once);
    }

    [Fact]
    public void ValidateFleet_ShouldNotMarkPlayerReady_WhenFleetIsInvalid()
    {
        var (session, player1, player2, board1Mock, _) = CreateSession();
        string gameCode = "GAME1";
        board1Mock.Setup(b => b.ValidateFleet()).Returns(new FleetValidationResult(false, [], []));
        SetupPlayerFoundInGame(player1.Id, gameCode, session);
        SetupPlayerFoundInGame(player2.Id, gameCode, session);

        _gameService.ValidateFleet(player1.Id);
        _gameService.ValidateFleet(player2.Id);
        var result = _gameService.TryStartGame(player2.Id);

        result.IsStarted.Should().BeFalse();
    }

    [Fact]
    public void ValidateFleet_ShouldRouteToCorrectSession_WhenMultipleGamesAreActive()
    {
        var (session1, player1, _, _, _) = CreateSession();
        var (session2, player3, _, _, _) = CreateSession();
        string gameCode1 = "GAME1";
        string gameCode2 = "GAME2";
        SetupPlayerFoundInGame(player1.Id, gameCode1, session1);
        SetupPlayerFoundInGame(player3.Id, gameCode2, session2);

        var result1 = _gameService.ValidateFleet(player1.Id);
        var result2 = _gameService.ValidateFleet(player3.Id);

        result1.IsValid.Should().BeTrue();
        result2.IsValid.Should().BeTrue();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player1.Id, out gameCode1!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode1, out session1), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(player3.Id, out gameCode2!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryGetGameByCode(gameCode2, out session2), Times.Once);
    }
}