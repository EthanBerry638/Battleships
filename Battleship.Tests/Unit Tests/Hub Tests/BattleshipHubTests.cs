using Battleship.Api.Hubs;
using Battleship.Api.Services;
using Battleship.Api.Engine;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Entities;
using Microsoft.AspNetCore.SignalR;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Responses;
using FluentAssertions;
using Moq;
using Battleship.Api.GamePieces.Data;

namespace Battleship.Tests.Unit_Tests.Hub_Tests;

public class BattleshipHubTests
{
    private readonly Mock<IGameService> _mockGameService = new();
    private readonly Mock<IConnectionService> _mockConnectionService = new();
    private readonly Mock<ISessionService> _mockSessionService = new();

    private readonly Mock<IGroupManager> _mockGroups = new();
    private readonly Mock<HubCallerContext> _mockContext = new();
    private readonly Mock<IHubCallerClients> _mockClients = new();
    private readonly Mock<IClientProxy> _mockClientProxy = new();
    private readonly Mock<ISingleClientProxy> _mockCallerProxy = new();

    private BattleshipHub CreateHub()
    {
        return new BattleshipHub(_mockGameService.Object, _mockConnectionService.Object, _mockSessionService.Object)
        {
            Groups = _mockGroups.Object,
            Context = _mockContext.Object,
            Clients = _mockClients.Object
        };
    }

    private static BattleshipEngine CreateEngine()
    {
        return new BattleshipEngine(
            new GameBoard(),
            new GameBoard(),
            new Player(Guid.NewGuid(), "Player 1"),
            new Player(Guid.NewGuid(), "Player 2")
        );
    }

    [Theory]
    [InlineData("DOESNOTEXIST")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task JoinLobby_ShouldReturnFalseAndNotAddUserToGroup_WhenLobbyDoesNotExist(string? gameCode)
    {
        var request = new JoinLobbyRequest(gameCode!, Guid.NewGuid(), "Player 2");
        _mockSessionService.Setup(s => s.JoinLobby(It.IsAny<string>(), It.IsAny<Player>()))
            .Returns((BattleshipEngine?)null);

        bool result = await CreateHub().JoinLobby(request);

        result.Should().BeFalse();

        _mockSessionService.Verify(s => s.JoinLobby(gameCode!, It.IsAny<Player>()), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Never);
        _mockGroups.Verify(g => g.AddToGroupAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("XYZ789")]
    [InlineData("123ABC")]
    public async Task JoinLobby_ShouldReturnTrueAndAddUserToGroupAndNotifyGroup_WhenLobbyExists(string gameCode)
    {
        var request = new JoinLobbyRequest(gameCode, Guid.NewGuid(), "Player 2");
        var expectedEngine = CreateEngine();
        _mockSessionService.Setup(s => s.JoinLobby(gameCode, It.IsAny<Player>()))
            .Returns(expectedEngine);
        _mockConnectionService.Setup(c => c.AddConnection("test-connection-id", request.PlayerId));
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(gameCode)).Returns(_mockClientProxy.Object);

        bool result = await CreateHub().JoinLobby(request);

        result.Should().BeTrue();
        _mockSessionService.Verify(s => s.JoinLobby(gameCode, It.IsAny<Player>()), Times.Once);
        _mockConnectionService.Verify(c => c.AddConnection("test-connection-id", request.PlayerId),
            Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Exactly(2));
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id", gameCode,
            It.IsAny<CancellationToken>()), Times.Once);
        _mockClients.Verify(c => c.Group(gameCode), Times.Once);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync(
                "GameCreated",
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    args[0] is GameCreatedResponse &&
                    ((GameCreatedResponse)args[0]).StartingPlayer == expectedEngine.CurrentPlayer &&
                    ((GameCreatedResponse)args[0]).Player1Id == expectedEngine.Players[0].Id &&
                    ((GameCreatedResponse)args[0]).Player2Id == expectedEngine.Players[1].Id),
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task CreateLobby_ShouldReturnGameCodeAndAddCallerToGroup_WhenCalled()
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), "Player 1");
        _mockSessionService.Setup(s => s.CreateLobby(It.IsAny<Player>()))
            .Returns("ABC123");
        _mockConnectionService.Setup(c => c.AddConnection("test-connection-id", request.PlayerId));
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        string result = await CreateHub().CreateLobby(request);

        result.Should().Be("ABC123");
        _mockSessionService.Verify(s => s.CreateLobby(It.IsAny<Player>()), Times.Once);
        _mockConnectionService.Verify(c => c.AddConnection("test-connection-id", request.PlayerId),
            Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Exactly(2));
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id", "ABC123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldSendMessageToOtherPlayer_WhenConnectionServiceReturnsAGameCode()
    {
        string gameCode = "ABC123";
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockConnectionService.Setup(c => c.HandleDisconnectAsync("test-connection-id", It.IsAny<TimeSpan>()))
            .ReturnsAsync(gameCode);
        _mockClients.Setup(c => c.Group(gameCode)).Returns(_mockClientProxy.Object);

        await CreateHub().OnDisconnectedAsync(null);

        _mockConnectionService.Verify(c => c.HandleDisconnectAsync("test-connection-id", It.IsAny<TimeSpan>()), Times.Once);
        _mockClients.Verify(c => c.Group(gameCode), Times.Once);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync("OpponentDisconnected", It.IsAny<object[]>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldNotSendMessageToOtherPlayer_WhenConnectionServiceReturnsNull()
    {
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockConnectionService.Setup(c => c.HandleDisconnectAsync("test-connection-id", It.IsAny<TimeSpan>()))
            .ReturnsAsync((string?)null);

        await CreateHub().OnDisconnectedAsync(null);

        _mockConnectionService.Verify(c => c.HandleDisconnectAsync("test-connection-id", It.IsAny<TimeSpan>()), Times.Once);
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public void PlaceShip_ShouldSendSuccessResultToCaller_WhenGameServiceReturnsSuccess()
    {
        var expectedResult = new PlacementResult(true);
        _mockGameService.Setup(g => g.PlaceShip(It.IsAny<PlaceShipRequest>())).Returns(expectedResult);

        var result = CreateHub().PlaceShip(new PlaceShipRequest(Guid.NewGuid(), ShipType.Carrier,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        result.Should().Be(expectedResult);
        _mockGameService.Verify(g => g.PlaceShip(It.IsAny<PlaceShipRequest>()), Times.Once);
    }

    [Fact]
    public void PlaceShip_ShouldSendFailureResultToCaller_WhenGameServiceReturnsFailure()
    {
        var expectedResult = new PlacementResult(false, [new Coordinate(0, 0), new Coordinate(1, 1)]);
        _mockGameService.Setup(g => g.PlaceShip(It.IsAny<PlaceShipRequest>())).Returns(expectedResult);
        _mockClients.Setup(c => c.Caller).Returns(_mockCallerProxy.Object);

        var result = CreateHub().PlaceShip(new PlaceShipRequest(Guid.NewGuid(), ShipType.Carrier,
            [new Coordinate(0, 0), new Coordinate(0, 1)]));

        result.Should().Be(expectedResult);
        _mockGameService.Verify(g => g.PlaceShip(It.IsAny<PlaceShipRequest>()), Times.Once);
    }

    [Fact]
    public async Task TryStartGame_ShouldSendResultToGroup_WhenGameStarted()
    {
        var playerId = Guid.NewGuid();
        var expectedResult = GameStartResult.Ok();
        var outcome = new StartGameResponse("ABC123", expectedResult);
        _mockGameService.Setup(g => g.TryStartGame(playerId)).Returns(outcome);
        _mockClients.Setup(c => c.Group("ABC123")).Returns(_mockClientProxy.Object);
        
        await CreateHub().TryStartGame(new TryStartGameRequest(playerId));

        _mockGameService.Verify(g => g.TryStartGame(playerId), Times.Once);
        _mockClients.Verify(c => c.Group("ABC123"), Times.Once);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync(
                "GameStarted",
                It.Is<object[]>(args => args.Length == 1 && (GameStartResult)args[0] == expectedResult),
                CancellationToken.None),
            Times.Once);
        _mockCallerProxy.Verify(
            p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryStartGame_ShouldSendResultToCaller_WhenWaitingForOpponent()
    {
        var playerId = Guid.NewGuid();
        var expectedResult = GameStartResult.WaitingForOpponent();
        var outcome = new StartGameResponse("ABC123", expectedResult);
        _mockGameService.Setup(g => g.TryStartGame(playerId)).Returns(outcome);
        _mockClients.Setup(c => c.Caller).Returns(_mockCallerProxy.Object);

        await CreateHub().TryStartGame(new TryStartGameRequest(playerId));

        _mockGameService.Verify(g => g.TryStartGame(playerId), Times.Once);
        _mockCallerProxy.Verify(
            p => p.SendCoreAsync(
                "GameNotStarted",
                It.Is<object[]>(args => args.Length == 1 && (GameStartResult)args[0] == expectedResult),
                CancellationToken.None),
            Times.Once);
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryStartGame_ShouldSendResultToCaller_WhenFleetIsInvalid()
    {
        var playerId = Guid.NewGuid();
        var expectedResult = GameStartResult.Invalid([new FleetValidationResult(false, [ShipType.Carrier], [])]);
        var outcome = new StartGameResponse("ABC123", expectedResult);
        _mockGameService.Setup(g => g.TryStartGame(playerId)).Returns(outcome);
        _mockClients.Setup(c => c.Caller).Returns(_mockCallerProxy.Object);

        await CreateHub().TryStartGame(new TryStartGameRequest(playerId));

        _mockGameService.Verify(g => g.TryStartGame(playerId), Times.Once);
        _mockCallerProxy.Verify(
            p => p.SendCoreAsync(
                "GameNotStarted",
                It.Is<object[]>(args => args.Length == 1 && (GameStartResult)args[0] == expectedResult),
                CancellationToken.None),
            Times.Once);
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public void GetWinner_ShouldDelegateToService_WhenGameCodeIsValid()
    {
        var player = new Player(Guid.NewGuid(), "winner");
        _mockGameService.Setup(g => g.GetWinner(It.IsAny<string>())).Returns(player);
        
        var result = CreateHub().GetWinner(new GetWinnerRequest("validGameCode"));

        result.Should().Be(player);
        _mockGameService.Verify(g => g.GetWinner(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public void GetWinner_ShouldReturnNull_WhenGameServiceReturnsNull()
    {
        _mockGameService.Setup(g => g.GetWinner(It.IsAny<string>())).Returns((Player?)null);
    
        var result = CreateHub().GetWinner(new GetWinnerRequest("validGameCode"));

        result.Should().BeNull();
        _mockGameService.Verify(g => g.GetWinner(It.IsAny<string>()), Times.Once);
    }
}