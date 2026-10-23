namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 技能姿态弹幕仍由玩家拥有；伤害代理由服务器拥有，统一结算横斩印记与击杀。
public sealed class EventideReunionDeathStrike : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public ProjectileNetReference Source;
    private Vector2 _PreviousGrip;
    private bool _Initialized;
    private bool Slash => Projectile.ai[0] == 1f;
    private EventideReunionDeathSkill Skill => Source.Resolve().ModProjectile as EventideReunionDeathSkill;
    public override string Texture => EventideReunionAssets.SpearTrailPath;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>().DisplayName;
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
        Projectile.timeLeft = 180;
    }
    public override bool ShouldUpdatePosition() => false;
    protected override void SendAnomalyExtraAI(BinaryWriter writer) => Source.Write(writer);
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader) => Source = ProjectileNetReference.Read(reader);
    public override void AI()
    {
        EventideReunionDeathSkill skill = Skill;
        if (skill is null || (Slash ? !skill.IsSlashing : !skill.IsDashing))
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.Kill();
            return;
        }
        _PreviousGrip = _Initialized ? Projectile.Center : skill.Projectile.Owner.MountedCenter;
        Projectile.Center = skill.Projectile.Owner.MountedCenter;
        _Initialized = true;
        // 命中后的穿身可持续数秒，伤害代理跟随阶段存活，不能先于突进过期。
        Projectile.timeLeft = 2;
    }
    public override bool? CanDamage() => Main.netMode != NetmodeID.MultiplayerClient && Skill is { } skill
        && (Slash ? skill.IsSlashing : skill.IsDashing) ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        EventideReunionDeathSkill skill = Skill;
        if (skill is null)
            return false;
        if (Slash)
            return skill.SlashCollision(targetHitbox);
        Vector2 tip = skill.DashDirection * 219.91f;
        return RotatedRectangle.FromLineSegment(_PreviousGrip,
            Projectile.Center + tip, 28f).Collides(targetHitbox);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Slash)
            EventideReunionDeathMark.Apply(target, (int)Projectile.ai[1]);
        else
            EventideReunionDeathMark.Credit(target, (int)Projectile.ai[1]);
    }
    public override bool PreDraw(ref Color lightColor) => false;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Slash)
            EventideReunionDeathMark.CompleteHit(target);
    }
}
