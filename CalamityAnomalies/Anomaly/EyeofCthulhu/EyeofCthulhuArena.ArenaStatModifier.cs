// Designed by ColdsUx

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed partial class EyeofCthulhuArena
{
    public sealed class ArenaStatModifier
    {
        public enum ModifierType
        {
            ArenaRadius,
            RotationSpeed,
        }

        public ModifierType Type;
        public int LifeTime;
        public bool UseLerp;
        public float Delta;
        public int Timer;

        public float LifeCompletion => (float)Timer / LifeTime;

        public ArenaStatModifier(ModifierType type, int lifeTime, float delta, bool useLerp = false)
        {
            Type = type;
            LifeTime = lifeTime;
            Delta = delta;
            UseLerp = useLerp;
        }

        public float GetDeltaValue() => (UseLerp ? LifeCompletion : TOMathUtils.Interpolation.QuadraticEaseInOut(LifeCompletion)) * Delta;
    }
}
