namespace Anomalies.Bosses.Skeletron;

internal static class SkeletronCombatVisuals
{
    internal static readonly Color BoneColor = new(233, 225, 199);

    internal static void SpawnFinalBurst(Vector2 center)
    {
        if (Main.dedServ)
            return;

        const int OrbCount = 160;
        for (int i = 0; i < OrbCount; i++)
        {
            // 均匀覆盖全周，混合快速火星与缓慢的大光球，形成有纵深的爆发。
            float angle = MathHelper.TwoPi * (i + Main.rand.NextFloat()) / OrbCount;
            Vector2 direction = angle.ToRotationVector2();
            bool fast = i % 3 != 0;
            float speed = fast ? Main.rand.NextFloat(9f, 24f) : Main.rand.NextFloat(3f, 10f);
            Color color = Color.Lerp(SkeletronCombatVisuals.BoneColor, new Color(255, 195, 100), Main.rand.NextFloat(0.7f));
            ParticleHandler.SpawnParticle(new OrbParticle(center + direction * Main.rand.NextFloat(8f, 38f),
                direction * speed, Main.rand.Next(35, 76), fast ? Main.rand.NextFloat(0.3f, 0.65f) : Main.rand.NextFloat(0.7f, 1.2f),
                color, lifeEndRatio: 0.15f));
        }
        ParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, new Color(255, 236, 197), 2.8f, 0.4f, 28, 0.1f, true));
    }
}
