using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class GetWinnerRequestValidatorTests
{
    private readonly GetWinnerRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveError_WhenGameCodeIsValid()
    {
        var request = new GetWinnerRequest("GAME1");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldHaveError_WhenGameCodeIsNullOrWhitespace(string? gameCode)
    {
        var request = new GetWinnerRequest(gameCode!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameCode)
            .WithErrorMessage("GameCode is required");
    }
}