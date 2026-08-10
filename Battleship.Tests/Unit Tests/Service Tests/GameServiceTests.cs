using Battleship.Api.DTOs;
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
    public void PlaceShip_ShouldThrowPlayerNotFoundException_WhenGameCodeExistsButSessionNotFound()
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
            .Throw<PlayerNotFoundException>()
            .WithMessage($"No active game found for player with id {playerId}.");
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
}