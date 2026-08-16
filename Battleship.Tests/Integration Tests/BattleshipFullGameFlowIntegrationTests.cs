using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Responses;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using FluentAssertions;

namespace Battleship.Tests.Integration_Tests;

public class BattleshipFullGameFlowIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HubConnection _p1Conn = null!;
    private HubConnection _p2Conn = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();

        _p1Conn = CreateConnection();
        _p2Conn = CreateConnection();

        await _p1Conn.StartAsync();
        await _p2Conn.StartAsync();
    }

    private HubConnection CreateConnection() =>
        new HubConnectionBuilder()
            .WithUrl("http://localhost/gameHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

    public async Task DisposeAsync()
    {
        await _p1Conn.DisposeAsync();
        await _p2Conn.DisposeAsync();
        await _factory.DisposeAsync();
    }

    private static List<Coordinate> FleetRow(int row, int length) =>
        Enumerable.Range(0, length).Select(x => new Coordinate(x, row)).ToList();

    private async Task PlaceFleet(HubConnection connection, Guid playerId, string label)
    {
        (ShipType Type, List<Coordinate> Coordinates)[] fleet =
        [
            (ShipType.Carrier, FleetRow(0, 5)),
            (ShipType.Battleship, FleetRow(1, 4)),
            (ShipType.Destroyer, FleetRow(2, 3)),
            (ShipType.Submarine, FleetRow(3, 3)),
            (ShipType.PatrolBoat, FleetRow(4, 2))
        ];

        foreach (var (type, coordinates) in fleet)
        {
            var result = await connection.InvokeAsync<PlacementResult>(
                "PlaceShip", new PlaceShipRequest(playerId, type, coordinates));

            result.IsSuccessful.Should().BeTrue($"{label}'s {type} placement should succeed");
        }
    }

    [Fact]
    public async Task FullGameSetupFlow_ShouldSucceed_EndToEnd()
    {
        var p1Id = Guid.NewGuid();
        var p2Id = Guid.NewGuid();

        var p1GameStartedTcs = new TaskCompletionSource<StartGameMessage>();
        var p2GameStartedTcs = new TaskCompletionSource<StartGameMessage>();
        var p1WaitingTcs = new TaskCompletionSource<StartGameMessage>();

        _p1Conn.On<StartGameMessage>("GameStarted", res => p1GameStartedTcs.TrySetResult(res));
        _p2Conn.On<StartGameMessage>("GameStarted", res => p2GameStartedTcs.TrySetResult(res));
        _p1Conn.On<StartGameMessage>("GameNotStarted", res => p1WaitingTcs.TrySetResult(res));

        string gameCode = await _p1Conn.InvokeAsync<string>(
            "CreateLobby", new CreateLobbyRequest(p1Id, "Player 1"));
        gameCode.Should().NotBeNullOrWhiteSpace();

        bool joined = await _p2Conn.InvokeAsync<bool>(
            "JoinLobby", new JoinLobbyRequest(gameCode, p2Id, "Player 2"));
        joined.Should().BeTrue();

        await PlaceFleet(_p1Conn, p1Id, "Player 1");
        await PlaceFleet(_p2Conn, p2Id, "Player 2");

        await _p1Conn.InvokeAsync("ValidateFleet", new ValidateFleetRequest(p1Id));
        await _p1Conn.InvokeAsync("TryStartGame", new TryStartGameRequest(p1Id));

        Task p1WaitResult = await Task.WhenAny(p1WaitingTcs.Task, Task.Delay(5000));
        p1WaitResult.Should().Be(p1WaitingTcs.Task, "Player 1 should receive GameNotStarted while waiting for Player 2");

        StartGameMessage p1WaitData = await p1WaitingTcs.Task;
        p1WaitData.IsStarted.Should().BeFalse();

        await _p2Conn.InvokeAsync("ValidateFleet", new ValidateFleetRequest(p2Id));
        await _p2Conn.InvokeAsync("TryStartGame", new TryStartGameRequest(p2Id));

        Task groupBroadcast = Task.WhenAll(p1GameStartedTcs.Task, p2GameStartedTcs.Task);
        Task completed = await Task.WhenAny(groupBroadcast, Task.Delay(5000));
        completed.Should().Be(groupBroadcast, "both players should receive the GameStarted broadcast");

        StartGameMessage finalResult = await p1GameStartedTcs.Task;
        finalResult.IsStarted.Should().BeTrue();
        finalResult.StartingPlayerId.Should().NotBeNull();
        finalResult.PlayerIds.Should().HaveCount(2);

        Player? winner = await _p1Conn.InvokeAsync<Player?>("GetWinner", new GetWinnerRequest(gameCode));
        winner.Should().BeNull();
    }
}