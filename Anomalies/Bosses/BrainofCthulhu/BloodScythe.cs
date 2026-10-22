namespace Anomalies.Bosses.BrainofCthulhu;

public class BloodScythe : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public override string Texture => "Anomalies/Bosses/BrainofCthulhu/Assets/VerticalSmearRagged";

    public Vector2 InitialVelocity
    {
        get => AnomalyProjectile.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            AnomalyProjectile.AnomalyAI64[0].SetValue(value);
            AnomalyProjectile.AIChanged64[0] = true;
        }
    }

    public Vector2 AcceleratingVelocity
    {
        get => AnomalyProjectile.AnomalyAI64[1].GetValue<Vector2>();
        set
        {
            AnomalyProjectile.AnomalyAI64[1].SetValue(value);
            AnomalyProjectile.AIChanged64[1] = true;
        }
    }

    public static float RotationSpeed => MathHelper.Pi / 8f;
    public static float Acceleration => 0.175f;
    public static int Lifetime => 240;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 16;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 300;
        Projectile.height = 300;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = Lifetime;
        Projectile.damage = BrainofCthulhu.BloodScytheDamage;
        Projectile.scale = 0.1f;
        Projectile.hostile = true;
        BrainCalamityBridge.EnableDefenseDamage(Projectile);
    }

    public override void OnSpawn(IEntitySource source)
    {
        InitialVelocity = Projectile.velocity;
        Projectile.rotation = Projectile.velocity.ToRotation();
        for (int i = 0; i < 3; i++)
        {
            BloodParticle p = new(Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
            ParticleHandler.SpawnParticle(p);
        }

        BloodParticle2 p2 = new(Projectile.Center, Projectile.velocity * 0.75f, 16, 0.5f, Color.Red);
        ParticleHandler.SpawnParticle(p2);
    }

    public override void AI()
    {
        int UpTime = Lifetime - Projectile.timeLeft;
        InitialVelocity *= 0.925f;
        if (UpTime > 15)
        {
            AcceleratingVelocity += Projectile.velocity.SafeNormalize(InitialVelocity.SafeNormalize(Vector2.UnitX)) * Acceleration;
            if (Main.rand.NextBool(1 + Projectile.timeLeft / 32))
            {
                BloodParticle p = new(Projectile.Center + Main.rand.NextVector2CircularEdge(32, 32), (-Projectile.velocity).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.25f, 0.75f), Main.rand.Next(10, 17), 1f, Color.Red);
                ParticleHandler.SpawnParticle(p);
            }
        }

        Projectile.velocity = InitialVelocity + AcceleratingVelocity;
        Projectile.rotation += RotationSpeed;
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.WritePackedVector2(InitialVelocity);
        writer.WritePackedVector2(AcceleratingVelocity);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        InitialVelocity = reader.ReadPackedVector2();
        AcceleratingVelocity = reader.ReadPackedVector2();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawScythe(Projectile);
        return false;
    }

    internal static void DrawScythe(Projectile projectile)
    {
        // Alpha blending preserves the red silhouette against bright backgrounds.
        Transoceanic.DataStructures.Rendering.SpriteBatchSnapshot state = Main.spriteBatch.ChangeState(blendState: BlendState.AlphaBlend);
        try
        {
            Texture2D texture = BrainOfCthulhuSystem.GetBloodScytheMask();
            Vector2 origin = texture.Size() * 0.5f;
            Color red = ChildSafety.Disabled ? new Color(255, 32, 32) : Main.DiscoColor;
            Color outline = ChildSafety.Disabled ? new Color(110, 4, 4) : red * 0.35f;
            Vector2 offset = projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            // Draw oldest first so the current blade stays crisp above its trail.
            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (projectile.oldPos[i] == Vector2.Zero)
                    continue;
                float completion = 1f - i / (float)projectile.oldPos.Length;
                float opacity = completion * completion * 0.6f * projectile.Opacity;
                Vector2 position = projectile.oldPos[i] + offset;
                float scale = projectile.scale * completion;
                Main.spriteBatch.Draw(texture, position, null, outline * opacity, projectile.oldRot[i], origin, scale * 1.08f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, position, null, red * opacity, projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
            }

            Vector2 center = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            // A narrow dark rim supplies contrast without whitening the red blade.
            for (int i = 0; i < 8; i++)
            {
                Vector2 rimOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 1f;
                Main.spriteBatch.Draw(texture, center + rimOffset, null, outline * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(texture, center, null, red * projectile.Opacity, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, center, null, red * (0.75f * projectile.Opacity), projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
        }
        finally
        {
            Main.spriteBatch.ResetState(state);
        }
    }

}

public class CirclingBloodScythe : AnomalyModProjectile
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public override string Texture => "Anomalies/Bosses/BrainofCthulhu/Assets/VerticalSmearRagged";
    public static float RotationSpeed => MathHelper.Pi / 8f;
    public static int Lifetime => BrainofCthulhu.CrimsonEyeAttackDuration;
    public static float MaxCircleSpeed => MathHelper.Pi / 30f; //1 rev per second
    public ref float CircleAngle => ref Projectile.ai[0];
    public ref float CircleRadius => ref Projectile.ai[1];
    public ref float CircleRadiusVelocity => ref Projectile.ai[2];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 16;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 300;
        Projectile.height = 300;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = Lifetime;
        Projectile.damage = BrainofCthulhu.BloodScytheDamage;
        Projectile.scale = 0.1f;
        Projectile.hostile = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.rotation = CircleAngle;
        for (int i = 0; i < 3; i++)
        {
            BloodParticle p = new(Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
            ParticleHandler.SpawnParticle(p);
        }

        BloodParticle2 p2 = new(Projectile.Center, Projectile.velocity * 0.75f, 16, 0.5f, Color.Red);
        ParticleHandler.SpawnParticle(p2);
    }

    public override void AI()
    {
        if (!BrainBehaviorAccess.ActiveBrain)
        {
            Projectile.active = false;
            return;
        }

        int UpTime = Lifetime - Projectile.timeLeft;
        if (UpTime < 30f)
            CircleRadius = MathHelper.Lerp(0f, 128f, BrainCombatUtils.CircOutEasing(UpTime / 30f, 1));
        else if (UpTime < Lifetime - 180)
            CircleRadius = 128f;
        else
        {
            if (UpTime == Lifetime - 180)
                CircleRadiusVelocity = -3f;
            CircleRadius += CircleRadiusVelocity;
            CircleRadiusVelocity += 0.1f;
        }

        if (UpTime >= 15f)
        {
            if (UpTime < 30f)
                CircleAngle += MathHelper.Lerp(0f, MaxCircleSpeed, BrainCombatUtils.SineInEasing((UpTime - 15) / 15f, 1));
            else if (UpTime < 870)
                CircleAngle += MaxCircleSpeed;
            else if (UpTime < 900)
                CircleAngle += MathHelper.Lerp(MaxCircleSpeed, MaxCircleSpeed / 3f, BrainCombatUtils.SineOutEasing((UpTime - 870) / 30f, 1));
            else
                CircleAngle += MaxCircleSpeed / 3f;
            if (Main.rand.NextBool(6))
            {
                BloodParticle p = new(Projectile.Center + Main.rand.NextVector2CircularEdge(32, 32), (Projectile.DirectionTo(Main.npc[NPC.crimsonBoss].Center).RotatedBy(MathHelper.PiOver2) * 16f).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.25f, 0.75f), Main.rand.Next(10, 17), 1f, Color.Red);
                ParticleHandler.SpawnParticle(p);
            }
        }

        NPC boss = Main.npc[NPC.crimsonBoss];
        Projectile.Center = boss.Center + (CircleAngle.ToRotationVector2() * CircleRadius);
        Projectile.rotation += RotationSpeed;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        BloodScythe.DrawScythe(Projectile);
        return false;
    }
}
