using System.Diagnostics.CodeAnalysis;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Entities;
namespace Battleship.Api.GamePieces.Board
{
    public class GameBoard : IGameBoard
    {
        private readonly Tile[,] _board = new Tile[10, 10];
        
        private static readonly List<ShipType> RequiredFleet =
        [
            ShipType.Carrier,
            ShipType.Battleship,
            ShipType.Destroyer,
            ShipType.Submarine,
            ShipType.PatrolBoat
        ];

        public GameBoard()
        {   
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    _board[x, y] = new Tile();
                }
            }
        }

        public Tile GetTile(Coordinate coordinate)
        {
            if (coordinate.X < 0 || coordinate.X >= 10 || coordinate.Y < 0 || coordinate.Y >= 10)
            {
                throw new InvalidCoordinateException($"Invalid coordinate: {coordinate}");
            }
            
            return _board[coordinate.X, coordinate.Y];
        }

        public PlacementResult PlaceShip(IShip ship)
        {
            var invalidCoordinates = ship.Coordinates
                .Where(c => GetTile(c).OccupyingShip != null)
                .ToList();

            if (invalidCoordinates.Count != 0)
            {
                return new PlacementResult(false, invalidCoordinates);
            }

            foreach (var coordinate in ship.Coordinates)
            {
                _board[coordinate.X, coordinate.Y].OccupyingShip = ship;
            }

            return new PlacementResult(true);
        }

        public bool AreAllShipsSunk()
        {
            var ships = new HashSet<IShip>();

            foreach (var tile in _board)
            {
                if (tile.HasShip)
                {
                    ships.Add(tile.OccupyingShip!);
                }
            }

            if (ships.Count == 0) return false;

            return ships.All(s => s.IsSunk());
        }

        public FleetValidationResult ValidateFleet()
        {
            var ships = _board
                .Cast<Tile>()
                .Where(t => t.HasShip)
                .Select(t => t.OccupyingShip!)
                .ToHashSet();
            var placedTypes = ships.Select(s => s.Type).ToList();
            var missingShips = RequiredFleet.Except(placedTypes).ToList();
            var expectedFleetCount = RequiredFleet
                .GroupBy(type => type)
                .ToDictionary(g => g.Key, g => g.Count());
            var actualFleetCount = placedTypes
                .GroupBy(type => type)
                .ToDictionary(g => g.Key, g => g.Count());
            var extraShips = actualFleetCount.SelectMany(placedShipGroup =>
                {
                    int extraCount = placedShipGroup.Value - expectedFleetCount[placedShipGroup.Key];
                    
                    return Enumerable.Repeat(placedShipGroup.Key, extraCount);
                }).ToList();

            return new FleetValidationResult
            (
                false,
                missingShips,
                extraShips
            );
        }
    }
}