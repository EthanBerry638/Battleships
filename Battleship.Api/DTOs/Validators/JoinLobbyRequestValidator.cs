using FluentValidation;
using Battleship.Api.DTOs.Requests;

namespace Battleship.Api.DTOs.Validators;

public class JoinLobbyRequestValidator : AbstractValidator<JoinLobbyRequest>
{
    public JoinLobbyRequestValidator()
    {
        RuleFor(x => x.GameCode)
            .NotEmpty()
            .WithMessage("Game code is required.")
            .Must(code => code is null || code.Trim().Length <= GameLimits.GameCodeLength)
            .WithMessage($"Game code must be {GameLimits.GameCodeLength} characters or fewer.");
        
        RuleFor(x => x.PlayerId)
            .NotEmpty()
            .WithMessage("PlayerId is required.");

        RuleFor(x => x.PlayerName)
            .NotEmpty()
            .WithMessage("Player name is required.")
            .MaximumLength(GameLimits.MaxPlayerNameLength)
            .WithMessage($"Player name must be {GameLimits.MaxPlayerNameLength} characters or fewer.");
    }
}