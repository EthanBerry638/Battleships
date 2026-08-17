using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Responses;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using System.Threading.Channels;
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

    private static async Task<ShotMessage> ReadShotAsync(
        ChannelReader<ShotMessage> reader)
    {
        Task<ShotMessage> readTask = reader.ReadAsync().AsTask();
        Task completed = await Task.WhenAny(readTask, Task.Delay(5000));

        completed.Should().Be(
            readTask,
            "the client should receive the Shot broadcast");

        return await readTask;
    }

    private async Task ShootAndAssertAsync(
        HubConnection shooterConnection,
        Guid shooterId,
        Coordinate coordinate,
        ShotResult expectedResult,
        ChannelReader<ShotMessage> p1Shots,
        ChannelReader<ShotMessage> p2Shots)
    {
        await shooterConnection.InvokeAsync(
            "Shoot",
            new ShootRequest(shooterId, coordinate));

        ShotMessage p1Message = await ReadShotAsync(p1Shots);
        ShotMessage p2Message = await ReadShotAsync(p2Shots);

        foreach (ShotMessage message in new[] { p1Message, p2Message })
        {
            message.ShooterId.Should().Be(shooterId);
            message.Coordinate.Should().Be(coordinate);
            message.Result.Should().Be(expectedResult);
        }
    }

    [Fact]
    public async Task FullGameFlow_ShouldSucceed_EndToEnd()
    {
        var p1Id = Guid.NewGuid();
        var p2Id = Guid.NewGuid();

        var p1GameStartedTcs =
            new TaskCompletionSource<StartGameMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var p2GameStartedTcs =
            new TaskCompletionSource<StartGameMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var p1WaitingTcs =
            new TaskCompletionSource<StartGameMessage>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var p1Shots = Channel.CreateUnbounded<ShotMessage>();
        var p2Shots = Channel.CreateUnbounded<ShotMessage>();

        _p1Conn.On<StartGameMessage>(
            "GameStarted",
            res => p1GameStartedTcs.TrySetResult(res));

        _p2Conn.On<StartGameMessage>(
            "GameStarted",
            res => p2GameStartedTcs.TrySetResult(res));

        _p1Conn.On<StartGameMessage>(
            "GameNotStarted",
            res => p1WaitingTcs.TrySetResult(res));

        _p1Conn.On<ShotMessage>("Shot", message => { p1Shots.Writer.TryWrite(message); });

        _p2Conn.On<ShotMessage>("Shot", message => { p2Shots.Writer.TryWrite(message); });
        
        string gameCode = await _p1Conn.InvokeAsync<string>(
            "CreateLobby",
            new CreateLobbyRequest(p1Id, "Player 1"));

        gameCode.Should().NotBeNullOrWhiteSpace();

        bool joined = await _p2Conn.InvokeAsync<bool>(
            "JoinLobby",
            new JoinLobbyRequest(gameCode, p2Id, "Player 2"));

        joined.Should().BeTrue();
        
        await PlaceFleet(_p1Conn, p1Id, "Player 1");
        await PlaceFleet(_p2Conn, p2Id, "Player 2");
        
        await _p1Conn.InvokeAsync(
            "ValidateFleet",
            new ValidateFleetRequest(p1Id));

        await _p1Conn.InvokeAsync(
            "TryStartGame",
            new TryStartGameRequest(p1Id));

        Task p1WaitResult =
            await Task.WhenAny(p1WaitingTcs.Task, Task.Delay(5000));

        p1WaitResult.Should().Be(
            p1WaitingTcs.Task,
            "Player 1 should wait until Player 2 is ready");

        StartGameMessage waitMessage = await p1WaitingTcs.Task;
        waitMessage.IsStarted.Should().BeFalse();

        await _p2Conn.InvokeAsync(
            "ValidateFleet",
            new ValidateFleetRequest(p2Id));

        await _p2Conn.InvokeAsync(
            "TryStartGame",
            new TryStartGameRequest(p2Id));

        Task gameStartedForBoth =
            Task.WhenAll(
                p1GameStartedTcs.Task,
                p2GameStartedTcs.Task);

        Task gameStartCompleted =
            await Task.WhenAny(gameStartedForBoth, Task.Delay(5000));

        gameStartCompleted.Should().Be(
            gameStartedForBoth,
            "both players should receive GameStarted");

        StartGameMessage startMessage =
            await p1GameStartedTcs.Task;

        startMessage.IsStarted.Should().BeTrue();
        startMessage.StartingPlayerId.Should().Be(p1Id);
        startMessage.PlayerIds.Should().BeEquivalentTo([p1Id, p2Id]);

        // Player 1 will shoot every occupied coordinate on Player 2's board.
        // These lengths mirror PlaceFleet:
        // Carrier, Battleship, Destroyer, Submarine, PatrolBoat.
        int[] fleetLengths = [5, 4, 3, 3, 2];

        var winningShots =
            new List<(Coordinate Coordinate, ShotResult Result)>();

        for (int row = 0; row < fleetLengths.Length; row++)
        {
            int shipLength = fleetLengths[row];

            for (int x = 0; x < shipLength; x++)
            {
                ShotResult expectedResult =
                    x == shipLength - 1
                        ? ShotResult.Sunk
                        : ShotResult.Hit;

                winningShots.Add(
                    (new Coordinate(x, row), expectedResult));
            }
        }

        var player2HitCoordinate = new Coordinate(0, 0);
        var player2MissCoordinate = new Coordinate(9, 9);

        for (int i = 0; i < winningShots.Count; i++)
        {
            var p1Shot = winningShots[i];

            await ShootAndAssertAsync(
                _p1Conn,
                p1Id,
                p1Shot.Coordinate,
                p1Shot.Result,
                p1Shots.Reader,
                p2Shots.Reader);

            if (i == winningShots.Count - 1)
                break;

            // Give Player 2 one Hit, one Duplicate and one Miss.
            // After that, repeatedly firing at the miss coordinate
            // produces Duplicate while still advancing the turn.
            Coordinate p2Coordinate;
            ShotResult p2ExpectedResult;

            switch (i)
            {
                case 0:
                    p2Coordinate = player2HitCoordinate;
                    p2ExpectedResult = ShotResult.Hit;
                    break;

                case 1:
                    p2Coordinate = player2HitCoordinate;
                    p2ExpectedResult = ShotResult.Duplicate;
                    break;

                case 2:
                    p2Coordinate = player2MissCoordinate;
                    p2ExpectedResult = ShotResult.Miss;
                    break;

                default:
                    p2Coordinate = player2MissCoordinate;
                    p2ExpectedResult = ShotResult.Duplicate;
                    break;
            }

            await ShootAndAssertAsync(
                _p2Conn,
                p2Id,
                p2Coordinate,
                p2ExpectedResult,
                p1Shots.Reader,
                p2Shots.Reader);
        }
        
        Player? winner = await _p1Conn.InvokeAsync<Player?>(
            "GetWinner",
            new GetWinnerRequest(gameCode));
        
        winner!.Id.Should().Be(p1Id);
    }
}