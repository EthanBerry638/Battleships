namespace Battleship.Api.Engine;

public class GameSession
{
    public object Lock { get; } = new();
    public BattleshipEngine Engine { get; }

    public GameSession(BattleshipEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        
        Engine = engine;
    }
}