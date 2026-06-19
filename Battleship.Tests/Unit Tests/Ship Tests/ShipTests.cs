using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Ship_Tests;

public class ShipTests
{
    [Fact]
    public void IsSunk_ReturnsFalse_WhenOnlySomeCoordinatesAreHit()
    {
        var coord1 = new Coordinate(0, 0);
        var coord2 = new Coordinate(0, 1);
        var ship = new Ship(ShipType.PatrolBoat, [coord1, coord2]);
        
        ship.RegisterHit(coord1);
        
        ship.IsSunk().Should().BeFalse();
    }

    [Fact]
    public void IsSunk_ReturnsTrue_WhenAllCoordinatesAreHit()
    {
        var coord1 = new Coordinate(0, 0);
        var coord2 = new Coordinate(0, 1);
        var coord3 = new Coordinate(0, 2);
        var ship = new Ship(ShipType.Destroyer, [coord1, coord2, coord3]);
        
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
        var ship = new Ship(ShipType.PatrolBoat, [coord1, coord2]);
        
        ship.RegisterHit(coord1);
        ship.RegisterHit(coord1); 
        
        ship.IsSunk().Should().BeFalse();
    }
    
    [Fact]
    public void ShipConstructor_ThrowsException_WhenCoordinatesAreNull()
    {
        var action = () => new Ship(ShipType.PatrolBoat, null!);

        action.Should().Throw<ArgumentNullException>();
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidCoordinates))]
    public void ShipConstructor_ThrowsException_WhenCoordinatesAreNotAdjacent(List<Coordinate> coordinates)
    {
        var action = () => new Ship(ShipType.PatrolBoat, coordinates);

        action.Should().Throw<InvalidShipException>()
            .WithMessage("Coordinates must be adjacent.");
    }

    public static IEnumerable<object[]> GetInvalidCoordinates()
    {
        yield return [new List<Coordinate> { new(0, 0), new(0, 2) }];
        yield return [new List<Coordinate> { new(0, 0), new(2, 0) }];
        yield return [new List<Coordinate> { new(1, 1), new(3, 1) }];
    }
    
    [Fact]
    public void ShipConstructor_ThrowsException_WhenCoordinatesAreDiagonal()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(1, 1) };
        var action = () => new Ship(ShipType.PatrolBoat, coordinates);

        action.Should().Throw<InvalidShipException>()
            .WithMessage("Coordinates must be in a straight line.");
    }
    
    [Theory]
    [MemberData(nameof(GetTooSmallCoordinates))]
    public void ShipConstructor_ThrowsException_WhenCoordinateCountIsTooSmall(List<Coordinate> coordinates)
    {
        var action = () => new Ship(ShipType.PatrolBoat, coordinates);

        action.Should().Throw<InvalidShipException>()
            .WithMessage("A ship must occupy at least 2 coordinates.");
    }

    public static IEnumerable<object[]> GetTooSmallCoordinates()
    {
        yield return [new List<Coordinate>()]; 
        yield return [new List<Coordinate> { new(0, 0) }]; 
    }
    
    [Fact]
    public void ShipConstructor_ThrowsException_WhenCoordinatesContainDuplicates()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(0, 1), new(0, 0) };
        var action = () => new Ship(ShipType.Destroyer, coordinates);

        action.Should().Throw<InvalidShipException>()
            .WithMessage("Coordinates must be unique.");
    }
    
    [Fact]
    public void ShipConstructor_AllowsUnorderedCoordinates_WhenTheyFormAValidLine()
    {
        var coordinates = new List<Coordinate> { new(0, 0), new(0, 2), new(0, 1) };
            
        var action = () => new Ship(ShipType.Destroyer, coordinates);

        action.Should().NotThrow();
    }
    
    [Theory]
    [InlineData(ShipType.Carrier, 5)]
    [InlineData(ShipType.Battleship, 4)]
    [InlineData(ShipType.Destroyer, 3)]
    [InlineData(ShipType.Submarine, 3)]
    [InlineData(ShipType.PatrolBoat, 2)] 
    public void ShipConstructor_ShouldNotThrow_WhenShipTypeMatchesSize(ShipType type, int size)
    {
        var coordinates = new List<Coordinate>();
        for (int i = 0; i < size; i++)
        {
            coordinates.Add(new Coordinate(0, i));
        }
        
        var action = () => new Ship(type, coordinates);
        
        action.Should().NotThrow();
    }
    
    [Theory]
    [InlineData(ShipType.Carrier, 4)]
    [InlineData(ShipType.Battleship, 5)]
    [InlineData(ShipType.Destroyer, 2)]
    [InlineData(ShipType.Submarine, 4)]
    [InlineData(ShipType.PatrolBoat, 3)]
    public void ShipConstructor_ThrowsException_WhenSizeIsInvalidForShipType(ShipType type, int size)
    {
        var coordinates = new List<Coordinate>();
        for (int i = 0; i < size; i++)
        {
            coordinates.Add(new Coordinate(0, i));
        }

        var action = () => new Ship(type, coordinates);

        action.Should().Throw<InvalidShipException>()
            .WithMessage($"Invalid ship type: {type} for ship of size {size}.");
    }
}