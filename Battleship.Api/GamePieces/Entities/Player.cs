using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities;

public record Player
{
    public Guid Id { get; }
    public string Name { get; }

    public Player(Guid id, string name)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }
}       