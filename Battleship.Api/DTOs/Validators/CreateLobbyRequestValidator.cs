using Battleship.Api.DTOs.Requests;
using FluentValidation;

namespace Battleship.Api.DTOs.Validators;

public class CreateLobbyRequestValidator : AbstractValidator<CreateLobbyRequest>
{
    public CreateLobbyRequestValidator()
    {
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required.");

        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .WithMessage("Player name is required.");
    }
}