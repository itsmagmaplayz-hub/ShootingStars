using ShootingStars.Models;

namespace ShootingStars.Game
{
    public class GameState
    {
        public List<Player> Players { get; set; }
        public Map CurrentMap { get; set; }
        public GameMode CurrentGameMode { get; set; }
        public int CurrentFrame { get; set; }
        public bool IsGameRunning { get; set; }
        public int TimeRemaining { get; set; }
        public List<GameEvent> Events { get; set; }

        public GameState(GameMode gameMode, Map map, List<Player> players)
        {
            CurrentGameMode = gameMode;
            CurrentMap = map;
            Players = players;
            CurrentFrame = 0;
            IsGameRunning = true;
            TimeRemaining = gameMode.TimeLimit;
            Events = new List<GameEvent>();
        }

        public void Update()
        {
            if (!IsGameRunning) return;

            CurrentFrame++;

            // Decrease time
            if (TimeRemaining > 0)
                TimeRemaining--;
            else if (CurrentGameMode.Type != GameModeType.Survival)
                IsGameRunning = false;

            // Update all players
            foreach (var player in Players)
            {
                if (player.IsAlive)
                {
                    player.UpdateCooldowns();
                }
            }

            // Check win conditions
            CheckWinConditions();
        }

        private void CheckWinConditions()
        {
            var alivePlayers = Players.Where(p => p.IsAlive).ToList();

            if (alivePlayers.Count <= 1)
            {
                IsGameRunning = false;
                if (alivePlayers.Count == 1)
                {
                    Events.Add(new GameEvent($"Player {alivePlayers[0].Name} wins!", GameEventType.Victory));
                }
            }
        }

        public void ProcessAttack(Player attacker, Player target)
        {
            if (!attacker.CanAttack() || target == attacker) return;

            double distance = attacker.Position.DistanceTo(target.Position);
            
            if (distance <= attacker.Shooter.NormalAttack.Range)
            {
                int damage = attacker.Shooter.NormalAttack.Damage;
                target.TakeDamage(damage);
                attacker.UseAttack();

                Events.Add(new GameEvent(
                    $"{attacker.Name} hit {target.Name} for {damage} damage!",
                    GameEventType.Attack
                ));

                if (!target.IsAlive)
                {
                    Events.Add(new GameEvent(
                        $"{target.Name} has been defeated!",
                        GameEventType.PlayerEliminated
                    ));
                }
            }
        }

        public void ProcessUltimate(Player attacker, Position targetPosition)
        {
            if (!attacker.CanUltimate()) return;

            attacker.UseUltimate();
            int damage = attacker.Shooter.UltimateAttack.Damage;
            int range = attacker.Shooter.UltimateAttack.Range;

            var targetsInRange = Players.Where(p =>
                p != attacker &&
                p.IsAlive &&
                p.Position.DistanceTo(targetPosition) <= range
            ).ToList();

            foreach (var target in targetsInRange)
            {
                target.TakeDamage(damage);
                Events.Add(new GameEvent(
                    $"{attacker.Name}'s {attacker.Shooter.UltimateAttack.Name} hit {target.Name} for {damage} damage!",
                    GameEventType.UltimateUsed
                ));

                if (!target.IsAlive)
                {
                    Events.Add(new GameEvent(
                        $"{target.Name} has been defeated!",
                        GameEventType.PlayerEliminated
                    ));
                }
            }
        }

        public Player? GetWinner()
        {
            var alivePlayers = Players.Where(p => p.IsAlive).ToList();
            return alivePlayers.Count == 1 ? alivePlayers[0] : null;
        }

        public override string ToString()
        {
            return $"Game running: {IsGameRunning}, Players alive: {Players.Count(p => p.IsAlive)}/{Players.Count}";
        }
    }

    public class GameEvent
    {
        public string Message { get; set; }
        public GameEventType Type { get; set; }
        public int Timestamp { get; set; }

        public GameEvent(string message, GameEventType type)
        {
            Message = message;
            Type = type;
            Timestamp = DateTime.Now.Millisecond;
        }

        public override string ToString()
        {
            return $"[{Type}] {Message}";
        }
    }

    public enum GameEventType
    {
        Attack,
        UltimateUsed,
        PlayerMove,
        PlayerEliminated,
        Victory,
        GameStart,
        GameEnd
    }
}
