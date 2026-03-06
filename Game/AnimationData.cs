using ShootingStars.Models;

namespace ShootingStars.Game
{
    public class AttackAnimation
    {
        public Vec Position { get; set; }
        public int FrameCreated { get; set; }
        public int DurationFrames { get; set; } = 10; // ~100ms at 100ms per frame
        public bool IsUltimate { get; set; }

        public AttackAnimation(Vec pos, int frame, bool isUlt = false)
        {
            Position = pos;
            FrameCreated = frame;
            IsUltimate = isUlt;
        }

        public bool IsActive(int currentFrame) => currentFrame - FrameCreated < DurationFrames;
        public float GetAlpha(int currentFrame) => 1.0f - ((float)(currentFrame - FrameCreated) / DurationFrames);
    }

    public class DamageIndicator
    {
        public Vec Position { get; set; }
        public int Damage { get; set; }
        public int FrameCreated { get; set; }
        public int DurationFrames { get; set; } = 30; // ~300ms at 100ms per frame
        public bool IsCritical { get; set; }

        public DamageIndicator(Vec pos, int damage, int frame, bool isCrit = false)
        {
            Position = pos;
            Damage = damage;
            FrameCreated = frame;
            IsCritical = isCrit;
        }

        public bool IsActive(int currentFrame) => currentFrame - FrameCreated < DurationFrames;
        public float GetAlpha(int currentFrame) => 1.0f - ((float)(currentFrame - FrameCreated) / DurationFrames);
        public float GetYOffset(int currentFrame) => (currentFrame - FrameCreated) * 1.5f; // Moves up over time
    }

    // new: projectile/shot animation between two points
    public class ShotAnimation
    {
        public Vec Start { get; set; }
        public Vec End { get; set; }
        public int FrameCreated { get; set; }
        public int DurationFrames { get; set; }

        public ShotAnimation(Vec start, Vec end, int frame)
        {
            Start = start;
            End = end;
            FrameCreated = frame;
            // make duration proportional to distance (20 frames per tile)
            double dist = start.DistanceTo(end);
            DurationFrames = Math.Max(10, (int)(dist * 20));
        }

        public bool IsActive(int currentFrame) => currentFrame - FrameCreated < DurationFrames;

        public Vec GetPosition(int currentFrame)
        {
            double t = (double)(currentFrame - FrameCreated) / DurationFrames;
            if (t > 1) t = 1;
            return new Vec(
                Start.X + (End.X - Start.X) * t,
                Start.Y + (End.Y - Start.Y) * t
            );
        }
    }
}
