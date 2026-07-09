using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities;

public record Player
{
    public string Name { get; }

    public Player(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}       