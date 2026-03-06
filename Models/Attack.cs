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
        public int AmmoCapacity { get; set; } // how many shots before reload/empty

        public Attack(string name, int damage, int range, int cooldown, AttackType type, string description = "", int ammoCapacity = int.MaxValue)
        {
            Name = name;
            Damage = damage;
            Range = range;
            CooldownFrames = cooldown;
            Type = type;
            Description = description;
            AmmoCapacity = ammoCapacity;
        }

        public override string ToString()
        {
            return $"{Name} - DMG: {Damage}, RNG: {Range}, CD: {CooldownFrames}, AMMO: {AmmoCapacity}";
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
