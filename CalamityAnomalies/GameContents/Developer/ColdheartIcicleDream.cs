// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Developer;

public sealed class ColdheartIcicleDream : CAModProjectile, IContentLoader
{
    public int TargetIndex = -1;

    public Entity Target
    {
        get => TargetIndex switch
        {
            >= 300 => Main.npc[TargetIndex - 300],
            >= 0 => Main.player[TargetIndex],
            _ => Projectile.Owner
        };
        set => TargetIndex = value switch
        {
            NPC npc => npc.whoAmI + 300,
            Player player => player.whoAmI,
            _ => -1,
        };
    }

    public bool TargetValid => Target switch
    {
        Player player => player.Alive,
        NPC npc => npc.active,
        _ => false
    };

    public const int LifeTime = 50;

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override string LocalizationCategory => "GameContents.Developer";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.netImportant = true;
        Projectile.scale = 1f;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = LifeTime;
        Projectile.extraUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI()
    {
        Timer1++;

        bool targetValid = TargetValid;
        if (targetValid)
            Projectile.Center = Target.Center;
        Projectile.timeLeft = LifeTime;
        if (Timer2 > 0 || !targetValid || Timer1 > 6000)
            Timer2 = Math.Max(Timer2 + 1, 100 - Timer1 / 3);
        if (Timer2 >= 100)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        ParticleHandler.EnterDrawRegion_Additive(spriteBatch);
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        Texture2D texture = ParticleHandler.GetTexture<OrbParticle>();
        float radius = 40f; //圆心半径
        for (int j = 0; j < 6; j++)
        {
            int localTimer = TOMathUtils.Min(100 - Timer2, Timer1 - j * 10, LifeTime);
            for (int i = 0; i <= localTimer * 10; i++)
            {
                float a = radius * ColdheartIclcle_Handler.AMultiplier; //椭圆半长轴
                float b = radius * ColdheartIclcle_Handler.BMultiplier; //椭圆半短轴
                float amount = i * 1.83f / (LifeTime * 10);
                if (amount > 1f)
                {
                    float multiplier = MathHelper.Lerp(0.4f, 1f, 2f - amount);
                    a *= multiplier;
                    b *= multiplier;
                }
                float angle = TOMathUtils.PiOver6 + TOMathUtils.PiOver3 * (j - 2);
                Vector2 circleCenter = Projectile.Center + new PolarVector2(radius, angle);
                (float sin, float cos) = MathF.SinCos(MathHelper.Lerp(-ColdheartIclcle_Handler.MaxAngleOffset, ColdheartIclcle_Handler.MaxAngleOffset, amount));
                Vector2 position = circleCenter + new Vector2(a * cos, b * sin).RotatedBy(angle);
                spriteBatch.DrawFromCenter(texture, position - Main.screenPosition, null, Color.White, 0f, 0.3f * Utils.Remap(amount, 0f, 1.83f, 0.4f, 1.2f));
            }
        }
        ParticleHandler.ExitParticleDrawRegion(spriteBatch);

        return false;
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? CanHitNPC(NPC target) => false;
}
