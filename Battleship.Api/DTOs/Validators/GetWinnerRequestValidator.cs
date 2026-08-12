using FluentValidation;
using Battleship.Api.DTOs.Requests;

namespace Battleship.Api.DTOs.Validators;

public class GetWinnerRequestValidator : AbstractValidator<GetWinnerRequest>
{
    public GetWinnerRequestValidator()
    {
        RuleFor(x => x.GameCode)
            .NotEmpty()
            .WithMessage("GameCode is required");
    }
}