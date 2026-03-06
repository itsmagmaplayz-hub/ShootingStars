namespace ShootingStars.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Shooter Shooter { get; set; }
        public Position Position { get; set; }
        // Continuous position for smooth movement
        public Vec PositionF { get; set; }
        // Render/interpolated position used for smooth rendering
        public Vec RenderPosition { get; set; }
        public int CurrentHealth { get; set; }
        public int UltimateCharge { get; set; }
        public int NormalAttackCooldown { get; set; }
        public int UltimateCooldown { get; set; }
        public int MaxAmmo { get; set; }
        public int CurrentAmmo { get; set; }
        public int LastAttackFrameTime { get; set; }
        public int LastDamagedFrameTime { get; set; }
        public int AmmoRegenCooldown { get; set; }
        public int AttackFreezeTimer { get; set; } // immobilized when > 0 (0.5 sec = 5 frames at 100ms)
        public bool IsAlive { get; set; }

        public Player(int id, string name, Shooter shooter, Position startPosition)
        {
            Id = id;
            Name = name;
            Shooter = shooter;
            Position = startPosition;
            // initialize continuous position centered on the tile
            // initialize continuous position centered on the tile
            PositionF = new Vec(startPosition.X + 0.5, startPosition.Y + 0.5);
            RenderPosition = new Vec(PositionF.X, PositionF.Y);
            CurrentHealth = shooter.MaxHealth;
            UltimateCharge = 0;
            NormalAttackCooldown = 0;
            UltimateCooldown = 0;
            MaxAmmo = shooter.NormalAttack.AmmoCapacity;
            CurrentAmmo = MaxAmmo;
            LastAttackFrameTime = -1000;
            LastDamagedFrameTime = -1000;
            AmmoRegenCooldown = 0;
            AttackFreezeTimer = 0;
            IsAlive = true;
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsAlive = false;
            }
            // record the frame when damage was taken (for regen cooldown)
            // we'll use a high value and set it properly in update
        }

        public void Heal(int amount)
        {
            CurrentHealth = Math.Min(CurrentHealth + amount, Shooter.MaxHealth);
        }

        public bool CanAttack()
        {
            return NormalAttackCooldown <= 0 && IsAlive && CurrentAmmo > 0;
        }

        public bool CanUltimate()
        {
            return UltimateCharge >= 100 && UltimateCooldown <= 0 && IsAlive;
        }

        public void UseAttack()
        {
            if (CanAttack())
            {
                NormalAttackCooldown = Shooter.NormalAttack.CooldownFrames;
                UltimateCharge = Math.Min(UltimateCharge + 25, 100);
                CurrentAmmo = Math.Max(CurrentAmmo - 1, 0);
                AttackFreezeTimer = 5; // freeze for 0.5 seconds (5 frames at 100ms/tick)
            }
        }

        public void UseUltimate()
        {
            if (CanUltimate())
            {
                UltimateCharge = 0;
                UltimateCooldown = Shooter.UltimateAttack.CooldownFrames;
            }
        }

        public void UpdateCooldowns(int frameNumber)
        {
            if (NormalAttackCooldown > 0)
                NormalAttackCooldown--;
            if (UltimateCooldown > 0)
                UltimateCooldown--;
            if (UltimateCharge < 100 && !CanAttack())
                UltimateCharge = Math.Min(UltimateCharge + 5, 100);

            // ammo regen: 1 ammo every 1 second (10 frames at 100ms/frame)
            if (CurrentAmmo < MaxAmmo)
            {
                if (AmmoRegenCooldown > 0)
                {
                    AmmoRegenCooldown--;
                }
                else
                {
                    CurrentAmmo++;
                    AmmoRegenCooldown = 10; // reset to 1 second
                }
            }

            // attack freeze timer
            if (AttackFreezeTimer > 0)
                AttackFreezeTimer--;

            // health regen: 10 health per second, but only if not attacked/attacking for 2 seconds (20 frames)
            int timeSinceAttack = frameNumber - LastAttackFrameTime;
            int timeSinceDamaged = frameNumber - LastDamagedFrameTime;
            if (timeSinceAttack >= 20 && timeSinceDamaged >= 20 && CurrentHealth < Shooter.MaxHealth)
            {
                // regen 10 health per second (every 10 frames regen 1 point per frame = 10 per second)
                // simpler: just heal 1 per frame when conditions are met
                Heal(1);
            }
        }

        public override string ToString()
        {
            return $"Player {Id}: {Name} ({Shooter.Name}) - HP: {CurrentHealth}/{Shooter.MaxHealth}";
        }
    }
}
