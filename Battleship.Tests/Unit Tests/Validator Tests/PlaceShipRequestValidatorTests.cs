using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Validators;
using Battleship.Api.GamePieces.Data;
using FluentValidation.TestHelper;

namespace Battleship.Tests.Unit_Tests.Validator_Tests;

public class PlaceShipRequestValidatorTests
{
    private readonly PlaceShipRequestValidator _validator = new();

    private static List<Coordinate> ValidCoordinates => [new(0, 0), new(0, 1)];

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var request = new PlaceShipRequest(Guid.NewGuid(), ShipType.PatrolBoat, ValidCoordinates);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveError_WhenPlayerIdIsEmpty()
    {
        var request = new PlaceShipRequest(Guid.Empty, ShipType.PatrolBoat, ValidCoordinates);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
    }

    [Fact]
    public void ShouldHaveError_WhenShipTypeIsNotDefined()
    {
        var request = new PlaceShipRequest(Guid.NewGuid(), (ShipType)999, ValidCoordinates);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void ShouldHaveError_WhenCoordinatesIsNull()
    {
        var request = new PlaceShipRequest(Guid.NewGuid(), ShipType.PatrolBoat, null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Coordinates);
    }

    [Fact]
    public void ShouldHaveError_WhenCoordinatesIsEmpty()
    {
        var request = new PlaceShipRequest(Guid.NewGuid(), ShipType.PatrolBoat, []);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Coordinates);
    }

    [Fact]
    public void ShouldHaveError_WhenCoordinatesContainsNullEntry()
    {
        var request = new PlaceShipRequest(Guid.NewGuid(), ShipType.PatrolBoat, [new Coordinate(0, 0), null!]);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Coordinates[1]");
    }

    [Fact]
    public void ShouldHaveErrorsForAllFields_WhenEverythingIsInvalid()
    {
        var request = new PlaceShipRequest(Guid.Empty, (ShipType)999, []);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.ShouldHaveValidationErrorFor(x => x.Coordinates);
    }
}