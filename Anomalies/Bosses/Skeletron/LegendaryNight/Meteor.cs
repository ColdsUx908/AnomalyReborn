namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class Meteor : AnomalyModProjectile
{
    public const int TrailLength = 50;

    // ai[0]：目标玩家索引（1-based，0 表示未指定目标）。
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 80;
        Projectile.height = 80;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.scale = 3f;
    }

    public override void AI()
    {
        if (!Main.dedServ && Projectile.soundDelay == 0)
        {
            Projectile.soundDelay = 15 + Main.rand.Next(30);
            SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
        }

        Projectile.rotation += 0.06f * (Projectile.velocity.X >= 0f).ToDirectionInt();
        Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.55f, 0.12f));

        if (TOSharedData.NotClient
            && Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target)
            && Projectile.Top.Y > target.Bottom.Y + 250f)
        {
            Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft)
    {
        if (!Main.dedServ)
        {
            for (int i = 0; i < 150; i++)
            {
                Color color = Color.Lerp(new Color(255, 90, 10), LegendaryNightHandler.StarColor, Main.rand.NextFloat());
                ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Main.rand.NextPolarVector2(5f, 30f), Main.rand.Next(30, 60), Main.rand.NextFloat(0.5f, 1.5f), color, lifeEndRatio: 0.2f));
            }
        }

        if (!TOSharedData.NotClient)
            return;

        const int Amount = 80;
        const float MaxSpeed = 20f;

        // 与 RainbowJewel.Attack_Star 相同：按隔一个顶点的顺序连接五个顶点，
        // 在五条线段上插值速度，使弹幕散开后组成五角星。
        Vector2 velocityTowardsTarget = -Vector2.UnitY * MaxSpeed;
        if (Player.TryGetPlayerFromIndex((int)Projectile.ai[0] - 1, out Player target))
        {
            velocityTowardsTarget = Projectile.GetVelocityTowards(target, MaxSpeed);
            if (velocityTowardsTarget == Vector2.Zero)
                velocityTowardsTarget = -Vector2.UnitY * MaxSpeed;
        }
        PolarVector2 originalVelocity = (PolarVector2)velocityTowardsTarget;
        List<Vector2> vertices = [];
        for (int i = 0; i < 5; i++)
            vertices.Add(originalVelocity.RotatedBy(TOMathUtils.PiOver5 * 4 * i));
        vertices.Add(originalVelocity);

        for (int i = 0; i < Amount; i++)
        {
            Vector2 velocity = Vector2.LerpMany(vertices, (float)i / Amount);
            Projectile.NewProjectileAction<FallingStar>(Projectile.GetSource_Death(), Projectile.Center, velocity, SkeletronLegendHead.FallingStarDamage, Projectile.knockBack, action: p =>
            {
                p.scale = 0.2f;
                p.GetModProjectile<FallingStar>().TrailType = 2;
                p.GetModProjectile<FallingStar>().AccelerationStartTime = 180;
                p.netUpdate = true;
            });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Projectile.Texture;
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Vector2 center = Projectile.Center - Main.screenPosition;
        float opacity = Projectile.Opacity;

        // 从尾端向头部绘制；零位置是尚未填充的轨迹缓存。
        // Alpha 为零、RGB 非零的颜色在原版预乘 Alpha 混合下产生叠加光效。
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            Vector2 start = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
            Vector2 end = i == 0 ? center : Projectile.oldPos[i - 1] + Projectile.Size * 0.5f - Main.screenPosition;
            float strength = 1f - (float)i / TrailLength;
            float diameter = MathHelper.Lerp(8f, 110f, strength) * Projectile.scale;
            float intensity = strength * strength * opacity;
            // 插值填补高速移动时缓存点之间的空隙，并按采样数归一化亮度。
            int steps = Math.Max(1, (int)MathF.Ceiling(Vector2.Distance(start, end) / Math.Max(4f, diameter * 0.15f)));
            for (int step = 0; step < steps; step++)
            {
                Vector2 position = Vector2.Lerp(start, end, (float)step / steps);
                Color outerColor = new Color(255, 65, 8, 0) * (intensity * 0.5f / steps);
                Color innerColor = new Color(255, 205, 75, 0) * (intensity * 0.7f / steps);
                Main.spriteBatch.DrawFromCenter(bloom, position, null, outerColor, 0f, diameter / bloom.Width, SpriteEffects.None, 0f);
                Main.spriteBatch.DrawFromCenter(bloom, position, null, innerColor, 0f, diameter * 0.45f / bloom.Width, SpriteEffects.None, 0f);
            }

            if (i > 0 && i % 3 == 0)
                Main.spriteBatch.DrawFromCenter(texture, start, null, new Color(255, 145, 45, 0) * (intensity * 0.16f), Projectile.oldRot[i], Projectile.scale * MathHelper.Lerp(0.3f, 0.9f, strength), SpriteEffects.None, 0f);
        }

        float pulse = 1f + 0.06f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f);
        Main.spriteBatch.DrawFromCenter(bloom, center, null, new Color(255, 130, 25, 0) * (0.65f * opacity), 0f, 1.3f * Projectile.scale * pulse, SpriteEffects.None, 0f);
        Main.spriteBatch.DrawFromCenter(texture, center, null, Color.White * opacity, Projectile.rotation, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}
