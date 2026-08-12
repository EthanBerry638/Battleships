using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Battleship.Api.DTOs.Requests;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Tests.Integration_Tests;

public class BattleshipHubIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HubConnection _connection = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5179/gameHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await _connection.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateLobby_ShouldThrowHubException_WhenPlayerNameIsInvalid()
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), "");

        Func<Task> act = () => _connection.InvokeAsync<string>("CreateLobby", request);

        var exception = await act.Should().ThrowAsync<HubException>();
        exception.Which.Message.Should().Contain("Player name is required");
    }

    [Fact]
    public async Task CreateLobby_ShouldThrowHubException_WhenRequestIsNull()
    {
        Func<Task> act = () => _connection.InvokeAsync<string>("CreateLobby", (CreateLobbyRequest)null!);

        var exception = await act.Should().ThrowAsync<HubException>();
        exception.Which.Message.Should().Contain("request is required and cannot be null");
    }

    [Fact]
    public async Task CreateLobby_ShouldSucceed_WhenRequestIsValid()
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), "Player 1");

        string gameCode = await _connection.InvokeAsync<string>("CreateLobby", request);

        gameCode.Should().NotBeNullOrWhiteSpace();
    }
}