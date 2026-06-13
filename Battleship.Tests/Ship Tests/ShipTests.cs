using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Data;
using FluentAssertions;

namespace Battleship.Tests.Ship_Tests;

public class ShipTests
{
    [Fact]
    public void IsSunk_ReturnsTrue_AfterRegisteringHitOnOnlyOneCoordinate()
    {
        var coord = new Coordinate(0, 0);
        var ship = new Ship([coord]);
        
        ship.RegisterHit(coord);
        
        ship.IsSunk().Should().BeTrue();
    }

    [Fact]
    public void IsSunk_ReturnsFalse_WhenOnlySomeCoordinatesAreHit()
    {
        var coord1 = new Coordinate(0, 0);
        var coord2 = new Coordinate(0, 1);
        var ship = new Ship([coord1, coord2]);
        
        ship.RegisterHit(coord1);
        
        ship.IsSunk().Should().BeFalse();
    }
}