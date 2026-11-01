using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.EaterofWorlds;

// DoGP1EndPortal visual, adapted from CalamityMod; Copyright Azafure, LLC.
public sealed class EaterEntryPortal : ModProjectile
{
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => "Bosses.EaterofWorlds";
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 120;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.timeLeft = EaterofWorldsHead.PortalChargeDuration + 45;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        NPC owner = NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
        if (!owner.active || owner.type != NPCID.EaterofWorldsHead
            || EaterofWorldsHead.GetInstance(owner).CurrentBehavior == EaterofWorldsHead.Behavior.Despawn)
        {
            Projectile.Kill();
            return;
        }
        Projectile.scale = Utils.GetLerpValue(225f, 170f, Projectile.timeLeft, true)
            * Utils.GetLerpValue(0f, 55f, Projectile.timeLeft, true);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (AnomalyEffects.Overlays.EaterPortal is not { } shader)
            return false;
        using SpriteBatchScope scope = new(Main.spriteBatch, SpriteSortMode.Immediate);
        Texture2D noise = AnomalyTextures.Noise.Voronoi2;
        shader.Data.UseOpacity(Projectile.scale).UseColor(Color.Cyan).UseSecondaryColor(Color.Fuchsia).Apply();
        Main.EntitySpriteDraw(noise, Projectile.Center - Main.screenPosition, null, Color.White,
            0f, noise.Size() * 0.5f, 3.5f, SpriteEffects.None);
        return false;
    }
}
