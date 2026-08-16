using Battleship.Api.DTOs.Requests;
using FluentValidation;

namespace Battleship.Api.DTOs.Validators;

public class ValidateFleetRequestValidator : AbstractValidator<ValidateFleetRequest>
{
    public ValidateFleetRequestValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required");
    }
}