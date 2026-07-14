using Battleship.Api.GamePieces.Entities;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Player_Tests;

public class PlayerTests
{
    [Fact]
    public void PlayerConstructor_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        var act = () => new Player(Guid.Empty, "Test Player");
        
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("id")
            .WithMessage("Id cannot be empty*");
    }
}