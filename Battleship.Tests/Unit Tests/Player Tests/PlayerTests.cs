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
    
    [Fact]
    public void PlayerConstructor_ShouldAssignPropertiesCorrectly_WhenArgumentsAreValid()
    {
        var expectedId = Guid.NewGuid();
        string expectedName = "Test Player";
        
        var player = new Player(expectedId, expectedName);
        
        player.Id.Should().Be(expectedId);
        player.Name.Should().Be(expectedName);
    }
    
    [Fact]
    public void PlayerConstructor_ShouldAllowNameWithLeadingOrTrailingWhitespace_ButTrimIt()
    {
        string nameWithSpaces = "  Valid Name  ";
        string nameWithoutSpaces = "Valid Name";
        
        var player = new Player(Guid.NewGuid(), nameWithSpaces);
        
        player.Name.Should().Be(nameWithoutSpaces);
    }
}