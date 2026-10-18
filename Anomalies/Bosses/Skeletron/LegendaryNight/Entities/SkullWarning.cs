namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

public sealed class SkullWarning : AnomalyModProjectile
{
    public const int WarningTime = 60;
    private const float SkullSpeed = 18f;

    // ai[0]：目标玩家索引 + 1；ai[1]：预警计时；ai[2]：所属骷髅王索引 + 1。
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override string Texture => TOTextures.InvisibleTexturePath;

    // 与 SlimeMount 一致，避免屏幕外的发射点导致整条预警线被裁除。
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = WarningTime + 10;
    }

    public override void AI()
    {
        NPC master = NPC.GetNPCFromIndex((int)Projectile.ai[2] - 1);
        bool validTarget = Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target)
            && target.active && !target.dead;
        if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1 || !validTarget)
        {
            if (TOSharedData.NotClient)
                Projectile.Kill();
            return;
        }

        Projectile.rotation = Projectile.GetVelocityTowards(target).ToRotation();
        if (Projectile.localAI[0] == 0f && !Main.dedServ)
        {
            Projectile.localAI[0] = 1f;
            int remainingTime = Math.Max(1, WarningTime - (int)Projectile.ai[1]);
            ParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero,
                Color.White, 0.1f, 0.5f, remainingTime, 0.85f));
        }

        Projectile.ai[1]++;
        if (Projectile.ai[1] >= WarningTime && TOSharedData.NotClient)
        {
            // ai[0] = 1 跳过 Skull 的追踪和加速分支，发射后保持直线匀速。
            Projectile.NewProjectileAction(SourceAI, Projectile.Center,
                Projectile.GetVelocityTowards(target, SkullSpeed), ProjectileID.Skull,
                Projectile.damage, 0f, Projectile.owner, action: p =>
                {
                    p.ai[0] = 1f;
                    p.timeLeft = 600;
                    p.netUpdate = true;
                });
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target) || !target.active || target.dead)
            return false;

        float length = Vector2.Distance(Projectile.Center, target.Center) * 1.5f;
        float opacity = MathHelper.Clamp(Projectile.ai[1] / 10f, 0f, 1f);
        AnomalyDrawUtils.DrawLayeredWarning(Main.spriteBatch, Projectile.Center, Projectile.rotation,
            length, 10f, LegendaryNightHandler.BoneColor, Color.White, opacity);
        return false;
    }
}
