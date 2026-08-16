using Battleship.Api.DTOs.Requests;
using FluentValidation;

namespace Battleship.Api.DTOs.Validators;

public class ShootRequestValidator : AbstractValidator<ShootRequest>
{
    public ShootRequestValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required.");

        RuleFor(x => x.Coordinate)
            .NotNull()
            .WithMessage("Coordinate is required.");
    }
}