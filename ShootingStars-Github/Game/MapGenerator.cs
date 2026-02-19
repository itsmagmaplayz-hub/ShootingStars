using ShootingStars.Models;

namespace ShootingStars.Game
{
    public class MapGenerator
    {
        private Random _random;

        public MapGenerator(int seed = -1)
        {
            _random = seed == -1 ? new Random() : new Random(seed);
        }

        public int RandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }

        public Map GenerateRandomMap(int width, int height)
        {
            string[] mapNames = { "Forest Arena", "Desert Ruins", "Mountain Pass", "Urban Combat", "Frozen Tundra", "Volcanic Isle" };
            string name = mapNames[_random.Next(mapNames.Length)];
            
            Map map = new Map(width, height, name);
            
            // Add random obstacles
            int obstacleCount = _random.Next(10, 20);
            TileType[] obstacleTypes = { TileType.Wall, TileType.Rock, TileType.Bush, TileType.Water };
            
            for (int i = 0; i < obstacleCount; i++)
            {
                int x = _random.Next(width);
                int y = _random.Next(height);
                TileType type = obstacleTypes[_random.Next(obstacleTypes.Length)];
                map.AddObstacle(new Position(x, y), type);
            }

            // Add spawn points (4 corners)
            map.AddSpawnPoint(new Position(1, 1));
            map.AddSpawnPoint(new Position(width - 2, 1));
            map.AddSpawnPoint(new Position(1, height - 2));
            map.AddSpawnPoint(new Position(width - 2, height - 2));

            return map;
        }

        public Map GenerateMapByType(string mapType)
        {
            return mapType.ToLower() switch
            {
                "arena" => CreateArenaMap(),
                "jungle" => CreateJungleMap(),
                "city" => CreateCityMap(),
                "ice" => CreateIceMap(),
                _ => GenerateRandomMap(16, 12)
            };
        }

        private Map CreateArenaMap()
        {
            Map map = new Map(20, 16, "Grand Arena");
            
            // Add circular obstacles in the center
            for (int x = 8; x <= 12; x++)
            {
                for (int y = 6; y <= 10; y++)
                {
                    if ((x - 10) * (x - 10) + (y - 8) * (y - 8) <= 4)
                    {
                        map.AddObstacle(new Position(x, y), TileType.Rock);
                    }
                }
            }

            map.AddSpawnPoint(new Position(2, 2));
            map.AddSpawnPoint(new Position(18, 2));
            map.AddSpawnPoint(new Position(2, 14));
            map.AddSpawnPoint(new Position(18, 14));

            return map;
        }

        private Map CreateJungleMap()
        {
            Map map = new Map(18, 14, "Jungle Outpost");
            
            // Add forest patches
            for (int i = 0; i < 15; i++)
            {
                int x = _random.Next(18);
                int y = _random.Next(14);
                map.AddObstacle(new Position(x, y), TileType.Bush);
            }

            map.AddSpawnPoint(new Position(2, 2));
            map.AddSpawnPoint(new Position(16, 2));
            map.AddSpawnPoint(new Position(2, 12));
            map.AddSpawnPoint(new Position(16, 12));

            return map;
        }

        private Map CreateCityMap()
        {
            Map map = new Map(22, 18, "City Blocks");
            
            // Add city blocks
            for (int x = 4; x < 18; x += 4)
            {
                for (int y = 4; y < 14; y += 4)
                {
                    for (int dx = 0; dx < 3; dx++)
                    {
                        for (int dy = 0; dy < 3; dy++)
                        {
                            map.AddObstacle(new Position(x + dx, y + dy), TileType.Wall);
                        }
                    }
                }
            }

            map.AddSpawnPoint(new Position(1, 1));
            map.AddSpawnPoint(new Position(20, 1));
            map.AddSpawnPoint(new Position(1, 16));
            map.AddSpawnPoint(new Position(20, 16));

            return map;
        }

        private Map CreateIceMap()
        {
            Map map = new Map(16, 12, "Frozen Lake");
            
            // Add water patches (slippery)
            for (int i = 0; i < 10; i++)
            {
                int x = _random.Next(16);
                int y = _random.Next(12);
                map.AddObstacle(new Position(x, y), TileType.Water);
            }

            // Add ice blocks
            for (int i = 0; i < 8; i++)
            {
                int x = _random.Next(16);
                int y = _random.Next(12);
                map.AddObstacle(new Position(x, y), TileType.Rock);
            }

            map.AddSpawnPoint(new Position(2, 2));
            map.AddSpawnPoint(new Position(14, 2));
            map.AddSpawnPoint(new Position(2, 10));
            map.AddSpawnPoint(new Position(14, 10));

            return map;
        }
    }
}
