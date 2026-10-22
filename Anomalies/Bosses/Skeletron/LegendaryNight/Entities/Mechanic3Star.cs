namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>跟随小法阵扩大五角星的落星；ai[0] 为线段采样序号，ai[1] 为小圈索引，ai[2] 为主体索引（1-based）。</summary>
public sealed class Mechanic3Star : AnomalyModProjectile
{
    private bool _spawnParticlesReleased;

    public override string Texture => LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar";
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.scale = 0.2f;
        Projectile.timeLeft = 100;
        Projectile.netImportant = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI()
    {
        NPC master = NPC.GetNPCFromIndex((int)Projectile.ai[2] - 1);
        if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1)
        {
            Projectile.Kill();
            return;
        }
        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        LegendaryNightPhase2Arena arena = head.Phase2Arena;
        if (head.CurrentPhase != SkeletronLegendHead.Phase.Phase2_3 || arena is null)
        {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 100;
        // 每组仅由第0颗星在本地首次更新时释放，兼容客户端接收生成同步。
        if (!_spawnParticlesReleased && (int)Projectile.ai[0] == 0 && !Main.dedServ)
        {
            _spawnParticlesReleased = true;
            Vector2 center = arena.GetMagicCircle((int)Projectile.ai[1]).Center;
            for (int i = 0; i < SkeletronLegendHead.Mechanic3Data.SpawnParticleCount; i++)
                ParticleHandler.SpawnParticle(new OrbParticle(center,
                    Main.rand.NextPolarVector2(3f, 18f), Main.rand.Next(30, 61), Main.rand.NextFloat(0.5f, 1.5f),
                    Color.Lerp(SkeletronLegendHead.Mechanic3Data.ConnectionColor, LegendaryNightHandler.StarColor, Main.rand.NextFloat()),
                    lifeEndRatio: 0.2f));
        }
        Projectile.Center = SkeletronLegendHead.Mechanic3Data.GetStarPosition(arena,
            (int)Projectile.ai[0], head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic3Data.GetStartMusicTime((int)Projectile.ai[1]),
            (int)Projectile.ai[1]);
        Projectile.rotation += 0.1f;
        Lighting.AddLight(Projectile.Center, LegendaryNightHandler.StarColor.ToVector3());
    }

    public override bool PreDraw(ref Color lightColor)
    {
        FallingStar.DrawFallingStar(Projectile.Center, Projectile.rotation, Projectile.scale,
            Vector2.Zero, useMediumTexture: true);
        return false;
    }
}
