namespace ShootingStars.Models
{
    public class Attack
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Range { get; set; }
        public int CooldownFrames { get; set; }
        public AttackType Type { get; set; }
        public string Description { get; set; }

        public Attack(string name, int damage, int range, int cooldown, AttackType type, string description = "")
        {
            Name = name;
            Damage = damage;
            Range = range;
            CooldownFrames = cooldown;
            Type = type;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Name} - DMG: {Damage}, RNG: {Range}, CD: {CooldownFrames}";
        }
    }

    public enum AttackType
    {
        Normal,
        Projectile,
        Melee,
        Stun,
        Slow,
        Explode
    }
}
