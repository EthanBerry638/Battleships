using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities;

public record Player
{
    public Guid Id { get; }
    public string Name { get; }

    public Player(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty", nameof(id));
        Id = id;
        Name = name;
    }
}       