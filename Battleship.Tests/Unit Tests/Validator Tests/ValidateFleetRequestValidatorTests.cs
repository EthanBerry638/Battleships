using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class ValidateFleetRequestValidatorTests
{
    private readonly ValidateFleetRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var request = new ValidateFleetRequest(Guid.NewGuid());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenPlayerIdIsEmpty()
    {
        var request = new ValidateFleetRequest(Guid.Empty);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
    }
}