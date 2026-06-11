// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Developer.ColdsUx_ColdheartIcicle;

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

    public long NPCIdentifier;

    public bool TargetValid => Target switch
    {
        Player player => player.Alive,
        NPC npc => npc.active && NPCIdentifier == npc.Identifier,
        _ => false
    };

    public int Width;

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
        if (Timer1 == 0 && Target is NPC npc)
            NPCIdentifier = npc.Identifier;

        Timer1++;
        Timer3++; //动画专用

        bool targetValid = TargetValid;
        if (targetValid)
            Projectile.Center = Target.Center;
        Projectile.timeLeft = LifeTime;

        if (Timer2 > 0 || !targetValid || Timer1 > 6000)
            Timer2 = Math.Max(Timer2 + 1, 100 - Timer3 / 3);
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

        float width;

        if (Timer2 <= 0 && TargetValid)
        {
            Rectangle hitbox = Target.Hitbox;
            width = Width = Math.Max(hitbox.Width, hitbox.Height);
        }
        else
            width = Width * TOMathUtils.Interpolation.QuadraticEaseOut(1f - Timer2 / 100f);

        float radius = Math.Clamp(width * 0.5f, 25f, 200f); //圆心半径
        float lerpValue = Utils.GetLerpValue(25f, 200f, radius);

        for (int j = 0; j < 6; j++)
        {
            int localTimer = TOMathUtils.Min(100 - Timer2, Timer3 - j * 10, LifeTime);

            int iteration = (int)Math.Round(MathHelper.Lerp(10, 20, lerpValue));
            float baseScale = MathHelper.Lerp(0.3f, 0.8f, lerpValue);

            for (int i = 0; i <= localTimer * iteration; i++)
            {
                float a = radius * ColdheartIclcle_Handler.AMultiplier; //椭圆半长轴
                float b = radius * ColdheartIclcle_Handler.BMultiplier; //椭圆半短轴
                float amount = i * 1.83f / (LifeTime * iteration);
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
                spriteBatch.DrawFromCenter(texture, position - Main.screenPosition, null, Color.White, 0f, baseScale * Utils.Remap(amount, 0f, 1.83f, 0.4f, 1.2f));
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
