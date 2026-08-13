using FluentValidation.TestHelper;
using Battleship.Api.DTOs;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class CreateLobbyRequestValidatorTests
{
    private readonly CreateLobbyRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), "Player 1");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenPlayerIdIsEmpty()
    {
        var request = new CreateLobbyRequest(Guid.Empty, "Player 1");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldHaveError_WhenPlayerNameIsNullOrWhitespace(string? playerName)
    {
        var request = new CreateLobbyRequest(Guid.NewGuid(), playerName!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerName);
    }
}