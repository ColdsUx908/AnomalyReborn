namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>无伤害的收尾流星；ai[0]为主人索引+1，位置保存命中点，寿命独立于主人。</summary>
public sealed class LegendFinalMeteor : AnomalyModProjectile
{
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = SkeletronLegendHead.Mechanic7Data.MeteorFallDuration + 1;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;

    private int Age => LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true)
        ? head.Phase2MusicElapsedTime - SkeletronLegendHead.Mechanic7Data.T8MusicTime
        : SkeletronLegendHead.Mechanic7Data.MeteorFallDuration + 1 - Projectile.timeLeft;

    public override void AI()
    {
        if (LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true))
            Projectile.Center = head.NPC.Top + new Vector2(0f, 8f);
        if (Age >= SkeletronLegendHead.Mechanic7Data.MeteorFallDuration)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SkeletronLegendHead.DrawDescendingStar(Main.spriteBatch, Main.screenPosition, Math.Max(0, Age), Projectile.scale, PositionAtAge);
        return false;
    }

    private Vector2 PositionAtAge(int age)
    {
        float progress = MathHelper.Clamp(age / (float)SkeletronLegendHead.Mechanic7Data.MeteorFallDuration, 0f, 1f);
        return Projectile.Center - Vector2.UnitY * (1600f * (1f - progress * progress));
    }

    public override void OnKill(int timeLeft)
    {
        // 正常到达T10且主人已经击杀时只播粒子；不会调用任何攻击生成入口。
        if (Main.dedServ || Age < SkeletronLegendHead.Mechanic7Data.MeteorFallDuration
            || LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], oneBased: true))
            return;
        SoundEngine.PlaySound(Meteor.ExplosionSound, Projectile.Center);
        for (int i = 0; i < 100; i++)
        {
            Color color = Color.Lerp(LegendaryNightHandler.StarColor, Color.White, Main.rand.NextFloat(0.6f));
            Vector2 velocity = Main.rand.NextPolarVector2(2f, 15f);
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, velocity, Main.rand.Next(30, 65),
                Main.rand.NextFloat(0.35f, 1f), color, lifeEndRatio: 0.2f));
            if (i % 2 == 0)
                ParticleHandler.SpawnParticle(new GenericSparkle(Projectile.Center, velocity * 0.75f, color,
                    color * 0.35f, Main.rand.NextFloat(0.2f, 0.55f), Main.rand.Next(30, 65),
                    rotationSpeed: 0.02f, bloomScale: 1.2f));
        }
    }
}
