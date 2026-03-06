namespace ShootingStars.Models
{
    public class GameMode
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxPlayers { get; set; }
        public GameModeType Type { get; set; }
        public int TimeLimit { get; set; }

        public GameMode(string name, string description, int maxPlayers, GameModeType type, int timeLimit = 600)
        {
            Name = name;
            Description = description;
            MaxPlayers = maxPlayers;
            Type = type;
            TimeLimit = timeLimit;
        }

        public override string ToString()
        {
            return $"{Name} - {MaxPlayers} Players - {Description}";
        }
    }

    public enum GameModeType
    {
        LastManStanding,
        Survival
    }

    public static class GameModeFactory
    {
        public static List<GameMode> CreateAllGameModes()
        {
            return new List<GameMode>
            {
                new GameMode("Showdown", "10 player battle royale - survive and be last", 10, GameModeType.LastManStanding, 600),
                new GameMode("Endurance Run", "Survive waves of enemies", 1, GameModeType.Survival, 0)
            };
        }
    }
}
