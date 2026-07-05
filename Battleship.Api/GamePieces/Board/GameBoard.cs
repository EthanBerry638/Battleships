using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Entities;
namespace Battleship.Api.GamePieces.Board
{
    public class GameBoard : IGameBoard
    {
        private readonly Tile[,] _board = new Tile[10, 10];

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
            var ships = new HashSet<IShip>();
            
            var requiredFleet = new List<ShipType>
            {
                ShipType.Carrier,
                ShipType.Battleship,
                ShipType.Destroyer,
                ShipType.Submarine,
                ShipType.PatrolBoat
            };

            foreach (var tile in _board)
            {
                if (tile.HasShip)
                {
                    ships.Add(tile.OccupyingShip!);
                }
            }

            var placedTypes = ships.Select(s => s.Type).ToList();
            var missingShips = requiredFleet.Except(placedTypes).ToList();

            return new FleetValidationResult
            (
                false,
                missingShips,
                []
            );
        }
    }
}