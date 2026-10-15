using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Arena;

public sealed class LegendaryNightArena : BaseRingArenaProjectile
{
    public float Intensity;

    // ai[0]：所属骷髅王的 NPC 索引（1-based）。
    public NPC Master
    {
        get => NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
        set => Projectile.ai[0] = (value?.whoAmI ?? -1) + 1;
    }

    private bool IsMasterValid => Master is { active: true, type: NPCID.SkeletronHead, life: > 0 } master && master.Anomaly.CustomAIType == 1;

    public SkeletronLegendHead MasterBehavior => IsMasterValid ? SkeletronLegendHead.GetInstance(Master) : null;

    public bool IsActivated;

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override float Radius { get; set; } = 1200f;
    public override float Thickness => 40f;
    public override Player Target => MasterBehavior?.Target;
    public override int DustType => DustID.PlatinumCoin;
    public override bool EnableMovementRestriction => IsActivated && Intensity == 1f;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Master = null;
    }

    public override void AI()
    {
        if (!IsMasterValid)
        {
            Projectile.Kill();
            return;
        }

        Projectile.Center = Master.Center;
        Projectile.velocity = Vector2.Zero;
        Projectile.timeLeft = 100;

        Intensity = MathHelper.Clamp(Intensity + (IsActivated ? 1f : -1f) / 60f, 0f, 1f);
        Radius = 1000f;

        if (!Main.dedServ && Intensity > 0f)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 direction = Main.rand.NextPolarVector2(1f);
                Dust.NewDustAction(Projectile.Center + direction * Radius, 0, 0, DustType, action: d =>
                {
                    d.noGravity = true;
                    d.velocity = direction.RotatedBy(MathHelper.PiOver2) * 1.5f;
                    d.alpha = (int)(255f * (1f - Intensity));
                    d.scale = Main.rand.NextFloat(1f, 1.5f) * Intensity;
                });
            }
        }

        if (EnableMovementRestriction)
            MovementRestriction();
        else
            OutofArenaTimer = 0;
    }

    public override bool CanHitPlayer(Player target) => EnableMovementRestriction && base.CanHitPlayer(target);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => EnableMovementRestriction && base.Colliding(projHitbox, targetHitbox) == true;

    public override bool PreDraw(ref Color lightColor) => false;
}
