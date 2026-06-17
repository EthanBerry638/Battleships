using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Ship_Tests;

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

    [Fact]
    public void IsSunk_ReturnsTrue_WhenAllCoordinatesAreHit()
    {
        var coord1 = new Coordinate(0, 0);
        var coord2 = new Coordinate(0, 1);
        var coord3 = new Coordinate(0, 2);
        var ship = new Ship([coord1, coord2, coord3]);
        
        ship.RegisterHit(coord1);
        ship.RegisterHit(coord2);
        ship.RegisterHit(coord3);
        
        ship.IsSunk().Should().BeTrue();
    }

    [Fact]
    public void IsSunk_ReturnsFalse_WhenSameCoordinateIsHitMultipleTimes()
    {
        var coord1 = new Coordinate(0, 0);
        var coord2 = new Coordinate(0, 1);
        var ship = new Ship([coord1, coord2]);
        
        ship.RegisterHit(coord1);
        ship.RegisterHit(coord1); 
        
        ship.IsSunk().Should().BeFalse();
    }
    
    [Fact]
    public void ShipConstructor_ThrowsException_WhenCoordinatesAreNull()
    {
        var action = () => new Ship(null!);

        action.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void ShipConstructor_ThrowsException_WhenCoordinatesAreNotValid()
    {
        List<Coordinate> coordinates = [new(0, 0), new(0, 2)];
            
        var action = () => new Ship(coordinates);

        action.Should().Throw<InvalidShipException>()
            .WithMessage("Coordinates must be adjacent.");
    }
}