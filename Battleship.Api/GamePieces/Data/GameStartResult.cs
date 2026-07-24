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

    public static GameStartResult Ok()
    {
        return new GameStartResult(true, null);
    }

    public static GameStartResult Invalid(FleetValidationResult[] errors)
    {
        return new GameStartResult(false, errors);
    }

    public static GameStartResult AlreadyStarted()
    {
        return new GameStartResult(false, null);
    }
}