using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Data;
using FluentAssertions;

namespace Battleship.Tests.Ship_Tests;

public class ShipTests
{
    [Fact]
    public void IsSunk_ReturnsTrue_AfterRegisteringHitOnOnlyCoordinate()
    {
        var coord = new Coordinate(0, 0);
        var ship = new Ship([coord]);
        
        ship.RegisterHit(coord);
        
        ship.IsSunk().Should().BeTrue();
    }
}