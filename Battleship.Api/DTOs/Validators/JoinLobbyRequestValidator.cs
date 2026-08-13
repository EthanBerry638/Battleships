using FluentValidation;
using Battleship.Api.DTOs.Requests;

namespace Battleship.Api.DTOs.Validators;

public class JoinLobbyRequestValidator : AbstractValidator<JoinLobbyRequest>
{
    public JoinLobbyRequestValidator()
    {
        RuleFor(x => x.GameCode)
            .NotEmpty()
            .WithMessage("Game code is required.");
        
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required.");

        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .WithMessage("Player name is required.");
    }
}