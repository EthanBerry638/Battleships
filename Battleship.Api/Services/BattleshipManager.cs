using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public class BattleshipManager
{
    private static readonly char[] Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    public string CreateGame()
    {
        return Guid.NewGuid().ToString("N")[..6].ToUpper();
    }
    
    public BattleshipEngine GetGame(string gameCode)
    {
        throw new NotImplementedException();
    }
}