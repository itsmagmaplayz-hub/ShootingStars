namespace ShootingStars.Models
{
    public class Map
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public Tile[,] Tiles { get; set; }
        public List<Position> SpawnPoints { get; set; }

        public Map(int width, int height, string name)
        {
            Width = width;
            Height = height;
            Name = name;
            Tiles = new Tile[width, height];
            SpawnPoints = new List<Position>();
            InitializeMap();
        }

        private void InitializeMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = new Tile(new Position(x, y), TileType.Grass);
                }
            }
        }

        public bool IsWalkable(Position position)
        {
            if (position.X < 0 || position.X >= Width || position.Y < 0 || position.Y >= Height)
                return false;
            return Tiles[position.X, position.Y].IsWalkable();
        }

        public void AddObstacle(Position position, TileType type)
        {
            if (position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height)
            {
                Tiles[position.X, position.Y].Type = type;
            }
        }

        public void AddSpawnPoint(Position position)
        {
            if (IsWalkable(position))
            {
                SpawnPoints.Add(position);
            }
        }

        public override string ToString()
        {
            return $"{Name} ({Width}x{Height})";
        }
    }
}
