namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>机制五中缓慢下坠的雨伞史莱姆。</summary>
/// <remarks>
/// <para>ai、localAI 和框架 Timer 均未使用，无实体专属 ExtraAI。</para>
/// </remarks>
public sealed class LegendUmbrellaSlime : AnomalyModProjectile
{
    private const float MaxFallSpeed = 5f;
    private const int TicksPerFrame = 8;
    public override string Texture => "Terraria/Images/NPC_" + NPCID.UmbrellaSlime;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 34;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 480;
    }

    public override void AI()
    {
        Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + SlimeMount.FallAcceleration, MaxFallSpeed);
        if (++Projectile.frameCounter >= TicksPerFrame)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % Main.npcFrameCount[NPCID.UmbrellaSlime];
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Projectile.Texture;
        Rectangle frame = texture.Frame(1, Main.npcFrameCount[NPCID.UmbrellaSlime], 0, Projectile.frame);
        Main.spriteBatch.DrawFromCenter(texture, Projectile.Center - Main.screenPosition, frame, lightColor, scale: Projectile.scale);
        return false;
    }
}
