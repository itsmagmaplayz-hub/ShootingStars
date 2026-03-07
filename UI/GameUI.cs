using ShootingStars.Models;
using ShootingStars.Game;

namespace ShootingStars.UI
{
    public class GameUI
    {
        public static void DisplayTitle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ███████╗██╗  ██╗ ██████╗ ██████╗ ████████╗██╗███╗   ██╗ ██████╗ ███████╗████████╗ █████╗ ██╗   ██╗███████╗
  ██╔════╝██║  ██║██╔═══██╗██╔═══██╗╚══██╔══╝██║████╗  ██║██╔════╝ ██╔════╝╚══██╔══╝██╔══██╗██║   ██║██╔════╝
  ███████╗███████║██║   ██║██║   ██║   ██║   ██║██╔██╗ ██║██║  ███╗███████╗   ██║   ███████║██║   ██║███████╗
  ╚════██║██╔══██║██║   ██║██║   ██║   ██║   ██║██║╚██╗██║██║   ██║╚════██║   ██║   ██╔══██║██║   ██║╚════██║
  ███████║██║  ██║╚██████╔╝╚██████╔╝   ██║   ██║██║ ╚████║╚██████╔╝███████║   ██║   ██║  ██║╚██████╔╝███████║
  ╚══════╝╚═╝  ╚═╝ ╚═════╝  ╚═════╝    ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝
            ");
            Console.ResetColor();
        }

        public static void DisplayShooterSelection(List<Shooter> shooters)
        {
            Console.WriteLine("\n=== SELECT YOUR SHOOTER ===\n");
            for (int i = 0; i < shooters.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{i + 1}] {shooters[i].Name}");
                Console.ResetColor();
                Console.WriteLine($"    {shooters[i].Description}");
                Console.WriteLine($"    Class: {shooters[i].Class}");
                Console.WriteLine($"    Attack: {shooters[i].NormalAttack}");
                Console.WriteLine($"    Ultimate: {shooters[i].UltimateAttack}\n");
            }
        }

        public static void DisplayGameModeSelection(List<GameMode> gameModes)
        {
            Console.WriteLine("\n=== SELECT GAME MODE ===\n");
            for (int i = 0; i < gameModes.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{i + 1}] {gameModes[i].Name}");
                Console.ResetColor();
                Console.WriteLine($"    {gameModes[i].Description}\n");
            }
        }

        public static void DisplayGameState(GameState gameState)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ Game: {gameState.CurrentGameMode.Name,-55} ║");
            Console.WriteLine($"║ Map: {gameState.CurrentMap.Name,-56} ║");
            Console.WriteLine($"║ Time: {gameState.TimeRemaining}s / {gameState.CurrentGameMode.TimeLimit}s - Frame: {gameState.CurrentFrame,-25} ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine("\n=== PLAYERS STATUS ===\n");
            foreach (var player in gameState.Players)
            {
                Console.ForegroundColor = player.IsAlive ? ConsoleColor.Green : ConsoleColor.Red;
                string status = player.IsAlive ? "ALIVE" : "ELIMINATED";
                Console.WriteLine($"[{status}] {player.Name} ({player.Shooter.Name})");
                Console.ResetColor();

                // Health bar
                int healthPercent = (player.CurrentHealth * 100) / player.Shooter.MaxHealth;
                int filledBars = healthPercent / 10;
                string healthBar = new string('█', filledBars) + new string('░', 10 - filledBars);
                Console.WriteLine($"      HP: {healthBar} {player.CurrentHealth}/{player.Shooter.MaxHealth}");

                // Ultimate bar
                int ultimateBars = player.UltimateCharge / 10;
                string ultimateBar = new string('▓', ultimateBars) + new string('░', 10 - ultimateBars);
                Console.WriteLine($"      ULT: {ultimateBar} {player.UltimateCharge}/100");

                Console.WriteLine($"      Position: {player.Position}");
                Console.WriteLine($"      Attack Cooldown: {player.NormalAttackCooldown} | Ultimate Cooldown: {player.UltimateCooldown}\n");
            }
        }

        public static void DisplayMap(GameState gameState)
        {
            Console.WriteLine("\n=== MAP ===\n");
            Map map = gameState.CurrentMap;

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Position pos = new Position(x, y);
                    var player = gameState.Players.Find(p => p.Position.Equals(pos));

                    if (player != null)
                    {
                        Console.ForegroundColor = player.IsAlive ? ConsoleColor.Cyan : ConsoleColor.Gray;
                        Console.Write("P ");
                    }
                    else
                    {
                        var tile = map.Tiles[x, y];
                        Console.ForegroundColor = tile.Type switch
                        {
                            TileType.Wall => ConsoleColor.DarkGray,
                            TileType.Water => ConsoleColor.Blue,
                            TileType.Bush => ConsoleColor.Green,
                            TileType.Rock => ConsoleColor.Gray,
                            _ => ConsoleColor.White
                        };
                        Console.Write(". ");
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        public static void DisplayGameEvents(GameState gameState, int maxEvents = 10)
        {
            Console.WriteLine("\n=== RECENT EVENTS ===\n");

            var recentEvents = gameState.Events.TakeLast(maxEvents).ToList();
            foreach (var evt in recentEvents)
            {
                Console.ForegroundColor = evt.Type switch
                {
                    GameEventType.Attack => ConsoleColor.Red,
                    GameEventType.UltimateUsed => ConsoleColor.Yellow,
                    GameEventType.PlayerEliminated => ConsoleColor.DarkRed,
                    GameEventType.Victory => ConsoleColor.Green,
                    _ => ConsoleColor.White
                };
                Console.WriteLine($"• {evt.Message}");
            }
            Console.ResetColor();
        }

        public static void DisplayGameOver(GameState gameState)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
  ██████╗  █████╗ ███╗   ███╗███████╗ ██████╗ ██╗   ██╗███████╗██████╗ ██╗
 ██╔════╝ ██╔══██╗████╗ ████║██╔════╝██╔═══██╗██║   ██║██╔════╝██╔══██╗██║
 ██║  ███╗███████║██╔████╔██║█████╗  ██║   ██║██║   ██║█████╗  ██████╔╝██║
 ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝  ██║   ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗╚═╝
 ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗╚██████╔╝ ╚████╔╝ ███████╗██║  ██║██╗
  ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝ ╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝╚═╝
            ");
            Console.ResetColor();

            var winner = gameState.GetWinner();
            if (winner != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n🏆 WINNER: {winner.Name} ({winner.Shooter.Name}) 🏆\n");
                Console.WriteLine($"Final Health: {winner.CurrentHealth}/{winner.Shooter.MaxHealth}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNO WINNER - Game Ended By Timeout!\n");
            }
            Console.ResetColor();

            Console.WriteLine("\n=== FINAL STATISTICS ===\n");
            foreach (var player in gameState.Players)
            {
                Console.WriteLine($"{player.Name} ({player.Shooter.Name}):");
                Console.WriteLine($"  Status: {(player.IsAlive ? "SURVIVED" : "ELIMINATED")}");
                Console.WriteLine($"  Final Health: {player.CurrentHealth}");
                Console.WriteLine();
            }
        }

        public static int GetIntInput(int min, int max)
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int result) && result >= min && result <= max)
                {
                    return result;
                }
                Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.");
            }
        }

        public static void DisplayLoading()
        {
            Console.WriteLine("\nStarting game...");
            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(300);
                Console.Write(".");
            }
            Console.WriteLine();
            System.Threading.Thread.Sleep(500);
        }
    }
}
