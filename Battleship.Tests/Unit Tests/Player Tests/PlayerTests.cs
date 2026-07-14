using Battleship.Api.GamePieces.Entities;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Player_Tests;

public class PlayerTests
{
    [Fact]
    public void PlayerConstructor_ShouldCreatePlayer_WhenIdIsNotEmpty()
    {
        var player = new Player(Guid.NewGuid(), "Test Player");
        var act  = () => new Player(Guid.NewGuid(), "Test Player");
        
        player.Should().NotBeNull();
        act.Should().NotThrow();
    }
    
    [Fact]
    public void PlayerConstructor_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        var act = () => new Player(Guid.Empty, "Test Player");
        
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName("id")
            .WithMessage("Id cannot be empty*");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void PlayerConstructor_ShouldThrowArgumentException_WhenNameIsNullOrEmpty(string name)
    {
        var act = () => new Player(Guid.NewGuid(), name);

        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(nameof(name));
    }
}