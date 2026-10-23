using System;
using System.Numerics;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

public static class EventideReunionResonanceRules
{
    public const int Duration = 30 * 60;
    public const byte AllAspects = 0x3F;

    public static int Count(byte mask) => BitOperations.PopCount((uint)(mask & AllAspects));

    public static bool Contains(byte mask, EventideReunionAspect aspect) => (uint)aspect < 6 && (mask & (1 << (int)aspect)) != 0;

    // 随机源由调用方传入；先移除重复相位，再从剩余印记中等概率选一个。
    public static byte Release(byte mask, EventideReunionAspect aspect, Func<int, int> next, out bool repeated)
    {
        mask &= AllAspects;
        repeated = Contains(mask, aspect);
        if ((uint)aspect >= 6)
            return mask;
        int bit = 1 << (int)aspect;
        if (!repeated)
            return (byte)(mask | bit);
        mask = (byte)(mask & ~bit);
        int count = Count(mask);
        if (count == 0)
            return mask;
        int selected = next(count);
        for (int i = 0; i < 6; i++)
            if ((mask & (1 << i)) != 0 && selected-- == 0)
                return (byte)(mask & ~(1 << i));
        return mask;
    }

    public static float DamageMultiplier(byte mask) => MathF.Pow(EventideReunionDamageConstants.ResonanceDamageMultiplierPerAspect, Count(mask));

    public static void Tick(ref byte mask, ref int remaining)
    {
        if (remaining > 0)
            remaining--;
        if (remaining <= 0)
        {
            remaining = 0;
            mask = 0;
        }
    }

    public static int Cooldown(int baseCooldown, byte mask, bool repeated) =>
        Math.Max(1, (int)Math.Ceiling(baseCooldown * (1d - 0.05d * Count(mask)) * (repeated ? 1.2d : 1d)));
}
