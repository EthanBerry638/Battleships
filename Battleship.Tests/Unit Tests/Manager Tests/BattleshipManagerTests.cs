using Battleship.Api.Services;
using FluentAssertions;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Tests.Unit_Tests.Manager_Tests;

public class GameManagerTests
{
    private readonly BattleshipManager _manager = new();
    private readonly Player _dummyPlayer1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _dummyPlayer2 = new(Guid.NewGuid(), "Player 2");

    [Fact]
    public void CreateLobby_ShouldReturnSixCharacterCode_WhenCalled()
    {
        string result = _manager.CreateLobby(_dummyPlayer1);

        result.Length.Should().Be(6);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateLobby_ShouldReturnUniqueCodes_WhenCalledMultipleTimes()
    {
        string code1 = _manager.CreateLobby(_dummyPlayer1);
        string code2 = _manager.CreateLobby(_dummyPlayer1);

        code1.Should().NotBe(code2);
    }

    [Fact]
    public void CreateLobby_ShouldReturnUppercaseCode_WhenCalled()
    {
        string result = _manager.CreateLobby(_dummyPlayer1);

        result.Should().BeUpperCased();
    }

    [Fact]
    public void CreateLobby_ShouldThrowArgumentNullException_WhenPlayerIsNull()
    {
        var act = () => _manager.CreateLobby(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("ABCDEF")]
    [InlineData("123-ABC")]
    [InlineData("abcdef")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetGame_ShouldReturnNull_WhenGameDoesNotExist(string? code)
    {
        var result = _manager.GetGame(code!);

        result.Should().BeNull();
    }
}