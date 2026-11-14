using Battleship.Api.DTOs.Requests;
using FluentValidation;

namespace Battleship.Api.DTOs.Validators;

public class ShootRequestValidator : AbstractValidator<ShootRequest>
{
    public ShootRequestValidator()
    {
        RuleFor(x => x.Coordinate)
            .NotNull()
            .WithMessage("Coordinate is required.");
    }
}