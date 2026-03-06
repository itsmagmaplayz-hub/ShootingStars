namespace ShootingStars.Models
{
    public class Ultimate
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Range { get; set; }
        public int CooldownFrames { get; set; }
        public UltimateType Type { get; set; }
        public string Description { get; set; }

        public Ultimate(string name, int damage, int range, int cooldown, UltimateType type, string description = "")
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
            return $"{Name} - DMG: {Damage}, RNG: {Range}, CD: {CooldownFrames}, Effect: {Type}";
        }
    }

    public enum UltimateType
    {
        Explosion,
        Slow,
        Stun,
        Heal,
        Shield,
        Blind,
        Knockback
    }
}
