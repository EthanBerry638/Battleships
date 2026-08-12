using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Battleship.Api.Services;
using Battleship.Api.Exceptions;
using Battleship.Api.DTOs.Requests;
using FluentAssertions;

namespace Battleship.Tests.Integration_Tests;

public class BattleshipHubExceptionFilterTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HubConnection _connection = null!;
    private readonly Mock<IGameService> _mockGameService = new();

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IGameService>();
                    services.AddSingleton(_mockGameService.Object);
                });
            });

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/gameHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
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
    public async Task DomainException_ShouldBeTranslatedToHubException_WithOriginalMessage()
    {
        _mockGameService
            .Setup(s => s.TryStartGame(It.IsAny<Guid>()))
            .Throws(new NotYourTurnException("It is not your turn."));

        var request = new TryStartGameRequest(Guid.NewGuid());
        Func<Task> act = () => _connection.InvokeAsync("TryStartGame", request);

        var exception = await act.Should().ThrowAsync<HubException>();
        exception.Which.Message.Should().Contain("It is not your turn.");
    }

    [Fact]
    public async Task UnexpectedException_ShouldNotLeakItsMessage_ToTheClient()
    {
        _mockGameService
            .Setup(s => s.TryStartGame(It.IsAny<Guid>()))
            .Throws(new InvalidOperationException("boom"));

        var request = new TryStartGameRequest(Guid.NewGuid());
        Func<Task> act = () => _connection.InvokeAsync("TryStartGame", request);

        var exception = await act.Should().ThrowAsync<HubException>();
        exception.Which.Message.Should().NotContain("boom");
    }
}