using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using Battleship.Api.GamePieces.Data;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class ShootRequestValidatorTests
{
    private readonly ShootRequestValidator _validator = new();
    [Fact]
    public void Should_HaveError_When_CoordinateIsNull()
    {
        var request = new ShootRequest(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Coordinate)
            .WithErrorMessage("Coordinate is required.");
    }

    [Fact]
    public void Should_NotHaveError_When_RequestIsValid()
    {
        var request = new ShootRequest(new Coordinate(0, 0));

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
