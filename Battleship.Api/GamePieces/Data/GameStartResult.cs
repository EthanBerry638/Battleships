namespace Battleship.Api.GamePieces.Data;

public record GameStartResult
{
    public bool Success { get; }
    public FleetValidationResult[]? ValidationErrors { get; }

    private GameStartResult(bool success, FleetValidationResult[]? errors)
    {
        Success = success;
        ValidationErrors = errors;
    }

    public static GameStartResult Ok() => new(true, null);
    public static GameStartResult Invalid(FleetValidationResult[] errors) => new(false, errors);
    public static GameStartResult AlreadyStarted() => new(false, null);
}