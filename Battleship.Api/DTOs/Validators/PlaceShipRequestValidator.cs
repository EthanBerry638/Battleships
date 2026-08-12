using Battleship.Api.DTOs.Requests;
using FluentValidation;

namespace Battleship.Api.DTOs.Validators;

public class PlaceShipRequestValidator : AbstractValidator<PlaceShipRequest>
{
    public PlaceShipRequestValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Ship type must be a valid ship type.");

        RuleFor(x => x.Coordinates)
            .NotEmpty()
            .WithMessage("Coordinates are required.");

        RuleForEach(x => x.Coordinates)
            .NotNull()
            .WithMessage("Coordinates cannot contain null entries.");
    }
}