namespace ShootingStars.Models
{
    public class Tile
    {
        public TileType Type { get; set; }
        public Position Position { get; set; }

        public Tile(Position position, TileType type = TileType.Grass)
        {
            Position = position;
            Type = type;
        }

        public bool IsWalkable()
        {
            return Type != TileType.Wall && Type != TileType.Water;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    public enum TileType
    {
        Grass,
        Wall,
        Water,
        Rock,
        Lava,
        Bush,
        Sand
    }
}
