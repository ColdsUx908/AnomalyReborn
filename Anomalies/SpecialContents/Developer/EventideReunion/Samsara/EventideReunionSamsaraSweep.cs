namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 转矛、横抽各一个服务器伤害代理；不参与刺击的标记候选收集。
public sealed class EventideReunionSamsaraSweep : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => EventideReunionAssets.SpearTrailPath;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>().DisplayName;
    private int _Age;
    private Player Caster => Main.player[(int)Projectile.ai[0]];
    private bool IsTurn => Projectile.ai[1] != 0f;
    private int Duration => IsTurn ? EventideReunionSamsaraSkill.TurnDuration : EventideReunionSamsaraSkill.SpinDuration;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = EventideReunionSamsaraSkill.SpinDuration;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => Main.netMode == NetmodeID.MultiplayerClient ? false : null;

    public override void AI()
    {
        _Age++;
        Projectile.Center = Caster.MountedCenter;
        if (!Caster.active || Caster.dead || Caster.CCed || Caster.noItems
            || Caster.HeldItem.type != ModContent.ItemType<EventideReunion>())
            Projectile.Kill();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        int facing = Projectile.ai[2] < 0f ? -1 : 1;
        Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX * facing);
        // 六帧横抽仍覆盖每帧经过的完整弧段，不增加动作时间。
        const int SamplesPerFrame = 32;
        for (int i = 0; i <= SamplesPerFrame; i++)
        {
            float progress = Math.Clamp((_Age - 1f + i / (float)SamplesPerFrame) / Duration, 0f, 1f);
            Vector2 vector = IsTurn
                ? EventideReunionSamsaraVisualGeometry.TurnVector(progress,
                    facing > 0 ? MathHelper.Pi + 0.15f : -0.15f, aim, Caster.gravDir)
                : EventideReunionSamsaraVisualGeometry.SpinVector(progress, facing);
            if (RotatedRectangle.FromLineSegment(Projectile.Center,
                Projectile.Center + vector * EventideReunionSamsaraSkill.WeaponTipDistance, 30f).Collides(targetHitbox))
                return true;
        }
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        int owner = (int)Projectile.ai[0];
        target.lastInteraction = owner;
        target.playerInteraction[owner] = true;
    }

    public override bool PreDraw(ref Color lightColor) => false;
}
