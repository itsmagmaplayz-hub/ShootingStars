using ShootingStars.Models;

namespace ShootingStars.Game
{
    public class GameEngine
    {
        public GameState GameState { get; set; }
        public bool IsRunning { get; private set; }
        private Random _random;
        public double AISpeed { get; set; } = 0.8; // tiles per tick for AI (tunable, increased)

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

                // Skip player 1 - human controlled
                if (player.Id == 1) continue;

                // Don't move if frozen from attacking
                if (player.AttackFreezeTimer <= 0)
                {
                    // Randomly move player
                    if (_random.NextDouble() > 0.5)
                    {
                        var dir = GetRandomDirectionVec();
                        MovePlayer(player, dir);
                    }
                }

                // Try to attack nearest enemy
                var nearestEnemy = GetNearestAliveEnemy(player);
                if (nearestEnemy != null)
                {
                    double distance = player.PositionF.DistanceTo(nearestEnemy.PositionF);
                    
                    if (distance <= player.Shooter.NormalAttack.Range)
                    {
                        if (player.CanAttack() && _random.NextDouble() > 0.3)
                        {
                            GameState.ProcessAttack(player, nearestEnemy);
                        }
                    }
                    else if (distance > 0 && distance < 15)
                    {
                        // Move closer smoothly
                        MoveTowards(player, nearestEnemy.PositionF);
                    }
                }

                // Use ultimate if charged. auto-activate when any enemy steps into mid-range.
                if (player.CanUltimate())
                {
                    double midRange = 3.0; // tiles considered "mid" distance
                    var midTargets = GameState.Players.Where(p =>
                        p != player &&
                        p.IsAlive &&
                        p.PositionF.DistanceTo(player.PositionF) <= midRange
                    ).ToList();
                    if (midTargets.Count > 0)
                    {
                        // immediately ult at the first mid-range enemy
                        GameState.ProcessUltimate(player, midTargets[0].Position);
                    }
                    else if (_random.NextDouble() > 0.7)
                    {
                        var enemiesNearby = GameState.Players.Where(p =>
                            p != player &&
                            p.IsAlive &&
                            p.PositionF.DistanceTo(player.PositionF) <= player.Shooter.UltimateAttack.Range
                        ).ToList();

                        if (enemiesNearby.Count > 0)
                        {
                            GameState.ProcessUltimate(player, enemiesNearby[0].Position);
                        }
                    }
                }
            }
        }

        private Player? GetNearestAliveEnemy(Player player)
        {
            return GameState.Players
                .Where(p => p != player && p.IsAlive)
                .OrderBy(p => player.PositionF.DistanceTo(p.PositionF))
                .FirstOrDefault();
        }

        // Smooth movement: apply small Vec direction scaled by speed
        public void MovePlayer(Player player, Vec direction)
        {
            if (!player.IsAlive) return;

            double speed = AISpeed; // tunable AI speed
            double nx = player.PositionF.X + direction.X * speed;
            double ny = player.PositionF.Y + direction.Y * speed;
            var newPos = new Vec(nx, ny);

            // if walkable, just move there; otherwise stay put (no sliding)
            if (GameState.CurrentMap.IsWalkable(newPos))
            {
                player.PositionF = newPos;
                player.Position = new Position((int)System.Math.Floor(player.PositionF.X), (int)System.Math.Floor(player.PositionF.Y));
                GameState.Events.Add(new GameEvent($"{player.Name} moved to {player.Position}", GameEventType.PlayerMove));
            }
        }

        private Vec RotateVec(Vec v, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Vec(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }

        private void MoveTowards(Player player, Vec target)
        {
            double dx = target.X - player.PositionF.X;
            double dy = target.Y - player.PositionF.Y;
            double len = System.Math.Sqrt(dx * dx + dy * dy);
            if (len == 0) return;
            var dir = new Vec(dx / len, dy / len);
            MovePlayer(player, dir);
        }

        private Vec GetRandomDirectionVec()
        {
            double rx = _random.NextDouble() * 2 - 1;
            double ry = _random.NextDouble() * 2 - 1;
            double len = System.Math.Sqrt(rx * rx + ry * ry);
            if (len == 0) return new Vec(0, 0);
            return new Vec(rx / len, ry / len);
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
