using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class TryStartGameRequestValidatorTests
{
    private readonly TryStartGameRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveError_WhenPlayerIdIsValid()
    {
        var request = new TryStartGameRequest(Guid.NewGuid());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenPlayerIdIsEmpty()
    {
        var request = new TryStartGameRequest(Guid.Empty);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId)
            .WithErrorMessage("PlayerId is required");
    }
}