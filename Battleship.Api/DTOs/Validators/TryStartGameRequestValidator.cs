using FluentValidation;
using Battleship.Api.DTOs.Requests;

namespace Battleship.Api.DTOs.Validators;

public class TryStartGameRequestValidator : AbstractValidator<TryStartGameRequest>
{
    public TryStartGameRequestValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required");
    }
}