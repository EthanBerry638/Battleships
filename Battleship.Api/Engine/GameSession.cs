namespace Battleship.Api.Engine;

public class GameSession(BattleshipEngine engine)
{
    private readonly HashSet<Guid> _readyPlayers = [];
    public object Lock { get; } = new();
    public BattleshipEngine Engine { get; } = engine;
    public bool BothPlayersReady => _readyPlayers.Count == 2;

    public void SetPlayerReady(Guid playerId)
    { 
        _readyPlayers.Add(playerId);
    }
}