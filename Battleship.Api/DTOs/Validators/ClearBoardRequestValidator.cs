using FluentValidation;
using Battleship.Api.DTOs.Requests;

namespace Battleship.Api.DTOs.Validators;

public class ClearBoardRequestValidator : AbstractValidator<ClearBoardRequest>
{
    public ClearBoardRequestValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required");
    }
}