using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class JoinLobbyRequestValidatorTests
{
    private readonly JoinLobbyRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var request = new JoinLobbyRequest("GAME1", Guid.NewGuid(), "Player 2");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldHaveError_WhenGameCodeIsNullOrWhitespace(string? gameCode)
    {
        var request = new JoinLobbyRequest(gameCode!, Guid.NewGuid(), "Player 2");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameCode);
    }

    [Fact]
    public void ShouldHaveError_WhenPlayerIdIsEmpty()
    {
        var request = new JoinLobbyRequest("GAME1", Guid.Empty, "Player 2");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldHaveError_WhenPlayerNameIsNullOrWhitespace(string? playerName)
    {
        var request = new JoinLobbyRequest("GAME1", Guid.NewGuid(), playerName!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerName);
    }

    [Fact]
    public void ShouldHaveErrorsForAllFields_WhenEverythingIsInvalid()
    {
        var request = new JoinLobbyRequest("", Guid.Empty, "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameCode);
        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
        result.ShouldHaveValidationErrorFor(x => x.PlayerName);
    }
}