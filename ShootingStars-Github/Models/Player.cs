namespace ShootingStars.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Shooter Shooter { get; set; }
        public Position Position { get; set; }
        public int CurrentHealth { get; set; }
        public int UltimateCharge { get; set; }
        public int NormalAttackCooldown { get; set; }
        public int UltimateCooldown { get; set; }
        public bool IsAlive { get; set; }

        public Player(int id, string name, Shooter shooter, Position startPosition)
        {
            Id = id;
            Name = name;
            Shooter = shooter;
            Position = startPosition;
            CurrentHealth = shooter.MaxHealth;
            UltimateCharge = 0;
            NormalAttackCooldown = 0;
            UltimateCooldown = 0;
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
        }

        public void Heal(int amount)
        {
            CurrentHealth = Math.Min(CurrentHealth + amount, Shooter.MaxHealth);
        }

        public bool CanAttack()
        {
            return NormalAttackCooldown <= 0 && IsAlive;
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

        public void UpdateCooldowns()
        {
            if (NormalAttackCooldown > 0)
                NormalAttackCooldown--;
            if (UltimateCooldown > 0)
                UltimateCooldown--;
            if (UltimateCharge < 100 && !CanAttack())
                UltimateCharge = Math.Min(UltimateCharge + 5, 100);
        }

        public override string ToString()
        {
            return $"Player {Id}: {Name} ({Shooter.Name}) - HP: {CurrentHealth}/{Shooter.MaxHealth}";
        }
    }
}
