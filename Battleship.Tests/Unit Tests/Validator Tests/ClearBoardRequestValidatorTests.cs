using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class ClearBoardRequestValidatorTests
{
    private readonly ClearBoardRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenPlayerIdIsEmpty()
    {
        var request = new ClearBoardRequest(Guid.Empty);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId)
            .WithErrorMessage("PlayerId is required");
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var request = new ClearBoardRequest(Guid.NewGuid());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PlayerId);
    }
}