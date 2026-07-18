using Battleship.Api.Hubs;
using Battleship.Api.Services;
using Battleship.Api.Engine;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Entities;
using Microsoft.AspNetCore.SignalR;
using Battleship.Api.DTOs;
using FluentAssertions;
using Moq;

namespace Battleship.Tests.Unit_Tests.Hub_Tests;

public class BattleshipHubTests
{
    private readonly Mock<IBattleshipManager> _mockManager = new();
    private readonly Mock<IGroupManager> _mockGroups = new();
    private readonly Mock<HubCallerContext> _mockContext = new();
    private readonly Mock<IHubCallerClients> _mockClients = new();
    private readonly Mock<IClientProxy> _mockClientProxy = new();

    private BattleshipHub CreateHub() => new(_mockManager.Object)
    {
        Groups = _mockGroups.Object,
        Context = _mockContext.Object,
        Clients = _mockClients.Object
    };
    
    private static BattleshipEngine CreateEngine() => new(
        new GameBoard(),
        new GameBoard(),
        new Player(Guid.NewGuid(), "Player 1"), 
        new Player(Guid.NewGuid(), "Player 2")
        );

    [Theory]
    [InlineData("DOESNOTEXIST")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task JoinLobby_ShouldReturnFalseAndNotAddUserToGroup_WhenLobbyDoesNotExist(string? gameCode)
    {
        var request = new JoinLobbyRequest(Guid.NewGuid(), "Player 2");
        _mockManager.Setup(m => m.JoinLobby(It.IsAny<string>(), It.IsAny<Player>()))
            .Returns((BattleshipEngine?)null);

        var result = await CreateHub().JoinLobby(gameCode!, request);

        result.Should().BeFalse();
        
        _mockManager.Verify(m => m.JoinLobby(gameCode!, It.IsAny<Player>()), Times.Once);
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
        var request = new JoinLobbyRequest(Guid.NewGuid(), "Player 2");
        var expectedEngine = CreateEngine(); 
        _mockManager.Setup(m => m.JoinLobby(gameCode, It.IsAny<Player>()))
            .Returns(expectedEngine);
        _mockManager.Setup(m => m.AddConnection(new AddConnectionRequest("test-connection-id", request.PlayerId)));
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(gameCode)).Returns(_mockClientProxy.Object);
        
        var result = await CreateHub().JoinLobby(gameCode, request);
        
        result.Should().BeTrue();
        _mockManager.Verify(m => m.JoinLobby(gameCode, It.IsAny<Player>()), Times.Once);
        _mockManager.Verify(m => m.AddConnection(new AddConnectionRequest("test-connection-id", request.PlayerId)), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Exactly(2));
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id", gameCode, 
            It.IsAny<CancellationToken>()), Times.Once);
        _mockClients.Verify(c => c.Group(gameCode), Times.Once);
        _mockClientProxy.Verify(
            p => p.SendCoreAsync(
                "GameStarted",
                It.Is<object[]>(args => args.Length == 1 && args[0] == expectedEngine),
                CancellationToken.None),
            Times.Once);
    }
    
    [Fact]
    public async Task JoinLobby_ShouldPropagatePlayerAlreadyInSessionException_WhenManagerThrows()
    {
        var request = new JoinLobbyRequest(Guid.NewGuid(), "Player 2");
        _mockManager.Setup(m => m.JoinLobby(It.IsAny<string>(), It.IsAny<Player>()))
            .Throws(new PlayerAlreadyInSessionException("Player is already in an active lobby or game."));
    
        var act = () => CreateHub().JoinLobby("ABC123", request);
    
        await act.Should().ThrowAsync<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        _mockManager.Verify(m => m.JoinLobby(It.IsAny<string>(), It.IsAny<Player>()), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Never);
        _mockGroups.Verify(g => g.AddToGroupAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateLobby_ShouldReturnGameCodeAndAddCallerToGroup_WhenCalled()
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), "Player 1");
        _mockManager.Setup(m => m.CreateLobby(It.IsAny<Player>()))
            .Returns("ABC123");
        _mockManager.Setup(m => m.AddConnection(new AddConnectionRequest("test-connection-id", request.PlayerId)));
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHub().CreateLobby(request);

        result.Should().Be("ABC123");
        _mockManager.Verify(m => m.CreateLobby(It.IsAny<Player>()), Times.Once);
        _mockManager.Verify(m => m.AddConnection(new AddConnectionRequest("test-connection-id", request.PlayerId)), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Exactly(2));
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id", "ABC123", It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateLobby_ShouldPropagatePlayerAlreadyInSessionException_WhenManagerThrows()
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), "Player 1");
        _mockManager.Setup(m => m.CreateLobby(It.IsAny<Player>()))
            .Throws(new PlayerAlreadyInSessionException("Player is already in an active lobby or game."));
        
        var act = () => CreateHub().CreateLobby(request);
        
        await act.Should().ThrowAsync<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        _mockManager.Verify(m => m.CreateLobby(It.IsAny<Player>()), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Never);
        _mockGroups.Verify(g => g.AddToGroupAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateLobbyAndJoinLobby_ShouldPropagateArgumentException_WhenAddConnectionThrows()
    {
        var createRequest = new CreateLobbyRequest(Guid.NewGuid(), "Player 1");
        var joinRequest = new JoinLobbyRequest(Guid.NewGuid(), "Player 2");
        _mockManager.Setup(m => m.CreateLobby(It.IsAny<Player>())).Returns("ABC123");
        _mockManager.Setup(m => m.JoinLobby(It.IsAny<string>(), It.IsAny<Player>())).Returns(CreateEngine());
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockManager.Setup(m => m.AddConnection(It.IsAny<AddConnectionRequest>()))
            .Throws<ArgumentException>();

        var createAct = () => CreateHub().CreateLobby(createRequest);
        var joinAct = () => CreateHub().JoinLobby("ABC123", joinRequest);

        await createAct.Should().ThrowAsync<ArgumentException>();
        await joinAct.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task OnDisconnectedAsync_ShouldPropagateArgumentException_WhenTheManagerThrows()
    {
        _mockManager.Setup(m => m.HandleDisconnectAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
            .Throws<ArgumentException>();

        var act = async () =>await CreateHub().OnDisconnectedAsync(null);
        
        await act.Should().ThrowAsync<ArgumentException>();
        _mockManager.Verify(m => m.HandleDisconnectAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
    }
}