namespace Battleship.Api.GamePieces.Data;

public record GameStartResult
{
    public GameStartStatus Status;
    public FleetValidationResult[]? ValidationErrors { get; }

    private GameStartResult(GameStartStatus status, FleetValidationResult[]? errors)
    {
        Status = status;
        ValidationErrors = errors;
    }

    public static GameStartResult Ok() => new(GameStartStatus.Started, null);
    public static GameStartResult WaitingForOpponent() => new(GameStartStatus.WaitingForOpponent, null);
    public static GameStartResult AlreadyStarted() => new(GameStartStatus.AlreadyStarted, null);
    public static GameStartResult Invalid(FleetValidationResult[] errors) => new(GameStartStatus.InvalidFleet, errors);
}

public enum GameStartStatus
{
    Started,
    WaitingForOpponent,
    AlreadyStarted,
    InvalidFleet
}