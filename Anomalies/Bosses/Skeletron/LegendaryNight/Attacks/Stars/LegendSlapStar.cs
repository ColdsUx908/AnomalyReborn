namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 半场拍击产生、对每名玩家仅结算一次的高速落星。
/// </summary>
/// <remarks>
/// <para>ai[0]：拍击序号（0-based）；ai[1]：半场，-1左、+1右；ai[2]：主体 NPC 索引 + 1。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；年龄由主体音乐时钟和拍击节点推导。</para>
/// <para>无实体专属 ExtraAI；碰撞保留本帧扫掠体积与半场限制。</para>
/// </remarks>
public sealed class LegendSlapStar : AnomalyModProjectile
{
    private bool _Hit;

    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Attacks/Stars/FallingStar_Hard";
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = SkeletronLegendHead.Mechanic2Parameters.StarTrailLength;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 28;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = SkeletronLegendHead.Mechanic2Parameters.RainDuration + 2;
        Projectile.scale = 0.22f;
    }

    public override bool? CanDamage() => false;

    public override void AI()
    {
        if (!LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[2], out SkeletronLegendHead head, oneBased: true))
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        LegendaryNightPhase2Arena arena = head.Phase2Arena;
        if (!head.Mechanic2Active || arena is null)
        {
            // 弹幕可能先于主体阶段或竞技场引用到达客户端，由服务器确认退场。
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int age = head.Mechanic2Age - SkeletronLegendHead.Mechanic2Parameters.GetStrikeTime((int)Projectile.ai[0]);
        if (age < 0)
            return;
        float x = Projectile.Center.X - arena.Projectile.Center.X;
        float bottom = MathF.Sqrt(Math.Max(0f, arena.Radius * arena.Radius - x * x));
        if (age >= SkeletronLegendHead.Mechanic2Parameters.RainDuration || Projectile.Center.Y > arena.Projectile.Center.Y + bottom + 32f)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        Projectile.rotation += 0.14f;
        FallingStar.SpawnFlightSparkle(Projectile);
        if (!Main.dedServ)
        {
            Player player = Main.LocalPlayer;
            // 高速落星使用本帧扫掠体积，避免越过玩家；半场边界仍以竞技场圆心为准。
            RotatedRectangle sweep = new(Projectile.Center + Projectile.velocity * 0.5f,
                Projectile.velocity.Length(), 24f, Projectile.velocity.ToRotation());
            bool hit = sweep.Collides(player.Hitbox);
            if (hit && Math.Sign(player.Center.X - arena.Projectile.Center.X) == (int)Projectile.ai[1]
                && player.Distance(arena.Projectile.Center) <= arena.Radius)
            {
                if (_Hit || !Main.LocalPlayer.active || Main.LocalPlayer.dead || Main.LocalPlayer.ghost)
                    return;
                _Hit = true;
                LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, false, ignoreAllImmunity: true);
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        LegendaryNightStarDrawing.DrawTrailType2(Projectile.Center, Projectile.scale, Projectile.velocity, Projectile.oldPos, Projectile.Size * 0.5f);
        Texture2D texture = LegendaryNightStarDrawing.HardTexture;
        Vector2 origin = new(texture.Width / 2f, texture.Height / (1f + MathF.Cos(TOMathUtils.PiOver5)));
        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White,
            Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}