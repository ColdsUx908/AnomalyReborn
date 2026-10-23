namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 每次刺击仅一个服务器代理，刺击完成后按全部实际命中结果选取一个标记目标。
public sealed class EventideReunionSamsaraStrike : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => EventideReunionAssets.SpearTrailPath;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>().DisplayName;
    private readonly HashSet<(NPC Target, AnomalyGlobalNPC Data)> _Candidates = [];
    private int _Age;
    private Player Caster => Main.player[(int)Projectile.ai[0]];
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
        Projectile.timeLeft = EventideReunionSamsaraSkill.ThrustDuration;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        _Age++;
        Projectile.Center = Caster.MountedCenter;
        if (!Caster.active || Caster.dead || Caster.CCed || Caster.noItems
            || Caster.HeldItem.type != ModContent.ItemType<EventideReunion>())
        {
            _Candidates.Clear();
            Projectile.Kill();
        }
    }
    public override bool? CanDamage() => Main.netMode == NetmodeID.MultiplayerClient ? false : null;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float progress = Math.Min(1f, _Age / (float)EventideReunionSamsaraSkill.ThrustDuration);
        float reach = EventideReunionSamsaraVisualGeometry.ThrustTipDistance(progress);
        return RotatedRectangle.FromLineSegment(Projectile.Center,
            Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * reach, EventideReunionSamsaraVisualGeometry.ThrustWidth).Collides(targetHitbox);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        int owner = (int)Projectile.ai[0];
        target.lastInteraction = owner;
        target.playerInteraction[owner] = true;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC root = EventideReunionSamsaraMarkBehavior.Root(target);
            _Candidates.Add((root, root.Anomaly));
        }
    }
    public override void OnKill(int timeLeft)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        NPC best = null;
        foreach ((NPC npc, AnomalyGlobalNPC data) in _Candidates)
        {
            if (!npc.active || npc.life <= 0 || npc.friendly || npc.lifeMax <= 5
                || !ReferenceEquals(Main.npc[npc.whoAmI], npc) || !ReferenceEquals(npc.Anomaly, data)
                || data.EventideReunion_SamsaraRecord.Active)
                continue;
            if (EventideReunionSamsaraRecord.Prefer(npc.boss, npc.lifeMax, npc.whoAmI,
                best?.boss ?? false, best?.lifeMax ?? 0, best?.whoAmI ?? -1))
                best = npc;
        }
        if (best is not null)
            EventideReunionSamsaraMarkBehavior.Apply(best, (int)Projectile.ai[0]);
    }
    public override bool PreDraw(ref Color lightColor) => false;
}
