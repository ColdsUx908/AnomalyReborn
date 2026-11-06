namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 跟随目标的头骨发射预警。
/// </summary>
/// <remarks>
/// <para>ai[0]：目标玩家索引 + 1；ai[1]：预警累计帧；ai[2]：主体 NPC 索引 + 1。</para>
/// <para>localAI[0]：本客户端预警音效已启动标志；其余 localAI 未使用，不同步。</para>
/// <para>框架 Timer1..5：未使用；无实体专属 ExtraAI。</para>
/// </remarks>
public sealed class SkullWarning : AnomalyModProjectile
{
    /// <summary>预警时刻。</summary>
    public const int WarningTime = 60;
    /// <summary>头骨速度，以世界像素和游戏帧计量。</summary>
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
        bool validTarget = Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target)
            && target.active && !target.dead;
        if (!LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[2], oneBased: true) || !validTarget)
        {
            if (TONetworkEnvironment.NotClient)
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
        if (Projectile.ai[1] >= WarningTime && TONetworkEnvironment.NotClient)
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