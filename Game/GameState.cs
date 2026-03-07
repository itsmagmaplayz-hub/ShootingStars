using System;
using ShootingStars.Models;

namespace ShootingStars.Game
{
    public class GameState
    {
        private static readonly Random _rand = new Random();
        public List<Player> Players { get; set; }
        public Map CurrentMap { get; set; }
        public GameMode CurrentGameMode { get; set; }
        public int CurrentFrame { get; set; }
        public bool IsGameRunning { get; set; }
        public int TimeRemaining { get; set; }
        public List<GameEvent> Events { get; set; }
        public List<AttackAnimation> Animations { get; set; }
        public List<DamageIndicator> DamageIndicators { get; set; }
        public List<ShotAnimation> Shots { get; set; }
        // Player stats for quests
        public int PlayerKills { get; set; } = 0;
        public int PlayerDamageDealt { get; set; } = 0;
        public int PlayerTimeSurvived { get; set; } = 0;
        
        // Poison zone mechanics
        public double ZoneCenterX { get; set; }
        public double ZoneCenterY { get; set; }
        public double ZoneRadius { get; set; }
        private int _lastZoneDamageFrame = 0;

        public GameState(GameMode gameMode, Map map, List<Player> players)
        {
            CurrentGameMode = gameMode;
            CurrentMap = map;
            Players = players;
            CurrentFrame = 0;
            IsGameRunning = true;
            TimeRemaining = gameMode.TimeLimit;
            Events = new List<GameEvent>();
            Animations = new List<AttackAnimation>();
            DamageIndicators = new List<DamageIndicator>();
            Shots = new List<ShotAnimation>();
            
            // Initialize poison zone at map center
            ZoneCenterX = map.Width / 2.0;
            ZoneCenterY = map.Height / 2.0;
            ZoneRadius = Math.Max(map.Width, map.Height); // Start large enough to not damage initially
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
                    player.UpdateCooldowns(CurrentFrame);
                }
            }

            // Track player survival time
            if (Players[0].IsAlive)
                PlayerTimeSurvived++;

            // Update poison zone (starts shrinking after 5 seconds, shrinks to center)
            UpdatePoisonZone();
            
            // Apply poison damage every 2 seconds (20 frames at 10 FPS) to players in zone
            if (CurrentFrame - _lastZoneDamageFrame >= 20)
            {
                ApplyPoisonDamage();
                _lastZoneDamageFrame = CurrentFrame;
            }

            // Clean up expired animations, damage indicators and shots
            Animations.RemoveAll(a => !a.IsActive(CurrentFrame));
            DamageIndicators.RemoveAll(d => !d.IsActive(CurrentFrame));
            Shots.RemoveAll(s => !s.IsActive(CurrentFrame));

            // Check win conditions
            CheckWinConditions();
        }
        
        private void UpdatePoisonZone()
        {
            int gameStartDelay = 50; // 5 seconds before zone starts shrinking
            int shrinkStartFrame = gameStartDelay;
            int shrinkDurationFrames = 250; // 25 seconds to fully shrink
            double minRadius = 2.0; // Minimum radius at center
            
            if (CurrentFrame > shrinkStartFrame)
            {
                double progress = Math.Min(1.0, (CurrentFrame - shrinkStartFrame) / (double)shrinkDurationFrames);
                double maxRadius = Math.Max(CurrentMap.Width, CurrentMap.Height) / 2.0 + 2;
                ZoneRadius = maxRadius - (maxRadius - minRadius) * progress;
            }
        }
        
        private void ApplyPoisonDamage()
        {
            const int PoisonDamage = 20;
            foreach (var player in Players.Where(p => p.IsAlive))
            {
                double distanceToCenter = player.PositionF.DistanceTo(new Vec(ZoneCenterX, ZoneCenterY));
                if (distanceToCenter > ZoneRadius)
                {
                    player.TakeDamage(PoisonDamage);
                    DamageIndicators.Add(new DamageIndicator(player.PositionF, PoisonDamage, CurrentFrame, false));
                    
                    if (!player.IsAlive && player.Id != 1)
                    {
                        Events.Add(new GameEvent(
                            $"{player.Name} was eliminated by the poison!",
                            GameEventType.PlayerEliminated
                        ));
                    }
                }
            }
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

            double distance = attacker.PositionF.DistanceTo(target.PositionF);

            if (distance <= attacker.Shooter.NormalAttack.Range)
            {
                int damage;
                // New damage system for snipers (health <= 60): two-circle damage
                if (attacker.Shooter.MaxHealth <= 60)
                {
                    double innerRange = 1.5; // inner circle radius in grids
                    if (distance <= innerRange)
                        damage = 5; // weak at close range
                    else
                        damage = 20; // strong at medium-long range
                }
                // Close combat (health >= 90): full damage close, weak far
                else if (attacker.Shooter.MaxHealth >= 90)
                {
                    if (distance <= attacker.Shooter.NormalAttack.Range * 0.5)
                        damage = attacker.Shooter.NormalAttack.Damage;
                    else
                        damage = _rand.Next(2, 5); // 2-4
                }
                // Normal shooters
                else
                {
                    damage = attacker.Shooter.NormalAttack.Damage;
                }
                
                target.TakeDamage(damage);
                if (attacker.Id == 1) PlayerDamageDealt += damage;
                target.LastDamagedFrameTime = CurrentFrame;
                attacker.UseAttack();
                attacker.LastAttackFrameTime = CurrentFrame;

                // Create attack animation at attacker position
                Animations.Add(new AttackAnimation(attacker.PositionF, CurrentFrame, false));
                
                // Create shot animation: travel from attacker toward target, but only to max range
                Vec shotEndpoint = attacker.PositionF;
                if (distance > 0)
                {
                    double dx = target.PositionF.X - attacker.PositionF.X;
                    double dy = target.PositionF.Y - attacker.PositionF.Y;
                    double len = Math.Sqrt(dx * dx + dy * dy);
                    // Normalize and scale to max range
                    double maxRangeDistance = Math.Min(distance, attacker.Shooter.NormalAttack.Range);
                    shotEndpoint = new Vec(
                        attacker.PositionF.X + (dx / len) * maxRangeDistance,
                        attacker.PositionF.Y + (dy / len) * maxRangeDistance
                    );
                }
                Shots.Add(new ShotAnimation(attacker.PositionF, shotEndpoint, CurrentFrame));
                
                // Create damage indicator at target position
                DamageIndicators.Add(new DamageIndicator(target.PositionF, damage, CurrentFrame, false));

                Events.Add(new GameEvent(
                    $"{attacker.Name} hit {target.Name} for {damage} damage!",
                    GameEventType.Attack
                ));

                if (!target.IsAlive)
                {
                    if (attacker.Id == 1) PlayerKills++;
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
            // convert targetPosition (grid) to continuous center
            var tp = new Vec(targetPosition.X + 0.5, targetPosition.Y + 0.5);

            var targetsInRange = Players.Where(p =>
                p != attacker &&
                p.IsAlive &&
                p.PositionF.DistanceTo(tp) <= range
            ).ToList();

            // Create attack animation at attacker continuous position
            Animations.Add(new AttackAnimation(attacker.PositionF, CurrentFrame, true));

            foreach (var target in targetsInRange)
            {
                target.TakeDamage(damage);
                
                // Create damage indicator at target continuous position
                DamageIndicators.Add(new DamageIndicator(target.PositionF, damage, CurrentFrame, true));

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
