namespace ShootingStars.Models
{
    public class Shooter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxHealth { get; set; }
        public Attack NormalAttack { get; set; }
        public Ultimate UltimateAttack { get; set; }
        public ShooterClass Class { get; set; }
        // simple text/icon representation for UI
        public string Icon { get; set; }

        public Shooter(string name, string description, int maxHealth, Attack normalAttack, Ultimate ultimate, ShooterClass shooterClass, string icon = "?")
        {
            Name = name;
            Description = description;
            MaxHealth = maxHealth;
            NormalAttack = normalAttack;
            UltimateAttack = ultimate;
            Class = shooterClass;
            Icon = icon;
        }

        public override string ToString()
        {
            return $"{Name} ({Class}) - HP: {MaxHealth}\n{Description}";
        }
    }

    public enum ShooterClass
    {
        Sniper,
        Assassin,
        Tank,
        Support,
        Elemental,
        DamageDealer
    }

    public static class ShooterFactory
    {
        public static List<Shooter> CreateAllShooters()
        {
            return new List<Shooter>
            {
                CreateBlastian(),
                CreateShadowShell(),
                CreateInferno(),
                CreateIcebound(),
                CreateThunderStrike(),
                CreateVoidWalker(),
                CreateShelly()
            };
        }

        private static Shooter CreateBlastian()
        {
            var attack = new Attack("Blaster Shot", 15, 8, 30, AttackType.Projectile, "Basic rapid fire projectile", 3);
            var ultimate = new Ultimate("Explosive Burst", 50, 10, 120, UltimateType.Explosion, "Large explosion dealing massive damage");
            return new Shooter("Blastian", "A aggressive shooter with explosive attacks", 100, attack, ultimate, ShooterClass.Tank, "🔫");
        }

        private static Shooter CreateShadowShell()
        {
            var attack = new Attack("Shadow Dart", 12, 12, 40, AttackType.Projectile, "Long range precision shot", 3);
            var ultimate = new Ultimate("Shadow Clone", 40, 6, 100, UltimateType.Blind, "Creates shadow clones that evade attacks");
            return new Shooter("ShadowShell", "A stealthy sniper with precision shots", 80, attack, ultimate, ShooterClass.Sniper, "🎯");
        }

        private static Shooter CreateInferno()
        {
            var attack = new Attack("Flame Burst", 18, 6, 25, AttackType.Explode, "Fiery projectile that explodes on impact", 3);
            var ultimate = new Ultimate("Inferno Rage", 60, 8, 110, UltimateType.Explosion, "Engulfs area in flames");
            return new Shooter("Inferno", "A caster who controls fire", 75, attack, ultimate, ShooterClass.Elemental, "🔥");
        }

        private static Shooter CreateIcebound()
        {
            var attack = new Attack("Frost Bolt", 14, 9, 35, AttackType.Slow, "Slows enemies significantly", 3);
            var ultimate = new Ultimate("Glacial Prison", 35, 7, 130, UltimateType.Slow, "Freezes enemies in place");
            return new Shooter("Icebound", "A mage specializing in crowd control", 85, attack, ultimate, ShooterClass.Elemental, "❄️");
        }

        private static Shooter CreateThunderStrike()
        {
            var attack = new Attack("Lightning Bolt", 16, 10, 32, AttackType.Normal, "Strikes with electricity", 3);
            var ultimate = new Ultimate("Chain Lightning", 55, 12, 115, UltimateType.Stun, "Stuns enemies and chains to nearby foes");
            return new Shooter("ThunderStrike", "An elemental master of electricity", 90, attack, ultimate, ShooterClass.Elemental, "⚡");
        }

        private static Shooter CreateVoidWalker()
        {
            var attack = new Attack("Void Slash", 20, 12, 20, AttackType.Melee, "Long range precision shot", 3);
            var ultimate = new Ultimate("Void Implosion", 70, 6, 125, UltimateType.Knockback, "Knockbacks and damages enemies violently");
            return new Shooter("VoidWalker", "A dark assassin with long-range precision", 60, attack, ultimate, ShooterClass.Sniper, "🌀");
        }

        private static Shooter CreateShelly()
        {
            var attack = new Attack("Mini Shotgun Shot", 10, 7, 28, AttackType.Melee, "A quick, short-range shotgun attack", 3);
            var ultimate = new Ultimate("Shotgun", 20, 14, 50, UltimateType.Explosion, "Fires a powerful shotgun blast that hits all enemies in front");
            return new Shooter("Shelly", "A tanky shooter with strong close-range firepower", 95, attack, ultimate, ShooterClass.DamageDealer, "🔫");
        }
    }
}
