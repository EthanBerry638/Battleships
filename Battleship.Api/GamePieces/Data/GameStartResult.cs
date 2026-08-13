using System.Text.Json.Serialization;

namespace Battleship.Api.GamePieces.Data;

public record GameStartResult
{
    public GameStartStatus Status { get; }
    public FleetValidationResult[]? ValidationErrors { get; }

    [JsonConstructor]
    private GameStartResult(GameStartStatus status, FleetValidationResult[]? validationErrors)
    {
        Status = status;
        ValidationErrors = validationErrors;
    }

    public static GameStartResult Ok() => new(GameStartStatus.Started, null);
    public static GameStartResult WaitingForOpponent() => new(GameStartStatus.WaitingForOpponent, null);
    public static GameStartResult Invalid(FleetValidationResult[] errors) => new(GameStartStatus.InvalidFleet, errors);
}

public enum GameStartStatus
{
    Started,
    WaitingForOpponent,
    InvalidFleet
}