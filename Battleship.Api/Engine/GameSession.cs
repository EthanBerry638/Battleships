namespace Battleship.Api.Engine;

public class GameSession
{
    private readonly HashSet<Guid> _readyPlayers = [];
    public object Lock { get; } = new();
    public BattleshipEngine Engine { get; }
    public bool BothPlayersReady => _readyPlayers.Count == 2;

    public GameSession(BattleshipEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        
        Engine = engine;
    }

    public void SetPlayerReady(Guid playerId)
    { 
        _readyPlayers.Add(playerId);
    }
}