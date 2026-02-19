using ShootingStars.Models;

namespace ShootingStars.Game
{
    public class GameEngine
    {
        public GameState GameState { get; set; }
        public bool IsRunning { get; private set; }
        private Random _random;

        public GameEngine(GameState gameState)
        {
            GameState = gameState;
            IsRunning = false;
            _random = new Random();
        }

        public void Start()
        {
            IsRunning = true;
            GameState.IsGameRunning = true;
            GameState.Events.Add(new GameEvent(
                $"Game started! Mode: {GameState.CurrentGameMode.Name}, Map: {GameState.CurrentMap.Name}",
                GameEventType.GameStart
            ));
        }

        public void Update()
        {
            if (!IsRunning || !GameState.IsGameRunning) return;

            GameState.Update();

            // Simulate AI or process game logic
            SimulateGameLogic();
        }

        private void SimulateGameLogic()
        {
            foreach (var player in GameState.Players)
            {
                if (!player.IsAlive) continue;

                // Randomly move player
                if (_random.NextDouble() > 0.5)
                {
                    MovePlayer(player, GetRandomDirection());
                }

                // Try to attack nearest enemy
                var nearestEnemy = GetNearestAliveEnemy(player);
                if (nearestEnemy != null)
                {
                    double distance = player.Position.DistanceTo(nearestEnemy.Position);
                    
                    if (distance <= player.Shooter.NormalAttack.Range)
                    {
                        if (player.CanAttack() && _random.NextDouble() > 0.3)
                        {
                            GameState.ProcessAttack(player, nearestEnemy);
                        }
                    }
                    else if (distance > 0 && distance < 15)
                    {
                        // Move closer
                        MoveTowards(player, nearestEnemy.Position);
                    }
                }

                // Use ultimate if charged
                if (player.CanUltimate() && _random.NextDouble() > 0.7)
                {
                    var enemiesNearby = GameState.Players.Where(p =>
                        p != player &&
                        p.IsAlive &&
                        p.Position.DistanceTo(player.Position) <= player.Shooter.UltimateAttack.Range
                    ).ToList();

                    if (enemiesNearby.Count > 0)
                    {
                        GameState.ProcessUltimate(player, enemiesNearby[0].Position);
                    }
                }
            }
        }

        private Player? GetNearestAliveEnemy(Player player)
        {
            return GameState.Players
                .Where(p => p != player && p.IsAlive)
                .OrderBy(p => player.Position.DistanceTo(p.Position))
                .FirstOrDefault();
        }

        public void MovePlayer(Player player, Position direction)
        {
            if (!player.IsAlive) return;

            Position newPos = new Position(player.Position.X + direction.X, player.Position.Y + direction.Y);

            if (GameState.CurrentMap.IsWalkable(newPos))
            {
                player.Position = newPos;
                GameState.Events.Add(new GameEvent(
                    $"{player.Name} moved to {newPos}",
                    GameEventType.PlayerMove
                ));
            }
        }

        private void MoveTowards(Player player, Position target)
        {
            int dx = target.X - player.Position.X;
            int dy = target.Y - player.Position.Y;

            Position direction = new Position(
                dx > 0 ? 1 : (dx < 0 ? -1 : 0),
                dy > 0 ? 1 : (dy < 0 ? -1 : 0)
            );

            MovePlayer(player, direction);
        }

        private Position GetRandomDirection()
        {
            int[] dirs = { -1, 0, 1 };
            return new Position(dirs[_random.Next(3)], dirs[_random.Next(3)]);
        }

        public void Stop()
        {
            IsRunning = false;
            GameState.IsGameRunning = false;
            GameState.Events.Add(new GameEvent(
                "Game Ended!",
                GameEventType.GameEnd
            ));
        }

        public string GetGameStatus()
        {
            var alivePlayers = GameState.Players.Count(p => p.IsAlive);
            return $"Frame: {GameState.CurrentFrame} | Time: {GameState.TimeRemaining}s | Players Alive: {alivePlayers}/{GameState.Players.Count}";
        }
    }
}
