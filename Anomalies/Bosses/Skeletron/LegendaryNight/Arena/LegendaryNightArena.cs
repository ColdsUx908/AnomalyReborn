using Anomalies.GameContents.Base;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

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

    private bool _PreviousActivation;

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(IsActivated);
        writer.Write(Intensity);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        IsActivated = reader.ReadBoolean();
        Intensity = reader.ReadSingle();
    }

    public override void PostAI()
    {
        if (TOSharedData.NotClient && IsActivated != _PreviousActivation)
            Projectile.netUpdate = true;
        _PreviousActivation = IsActivated;
    }

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override float Radius { get; set; } = 1000f;
    public override float Thickness => 40f;
    public Hexadecagon ArenaPolygon => new(Projectile.Center, Radius);

    // 视觉快照在激活期间跟随弹幕，淡出期间保留最后一次激活的位置和大小。
    private Hexadecagon _VisualPolygon;
    private bool _VisualPolygonInitialized;
    public Hexadecagon VisualPolygon => _VisualPolygonInitialized ? _VisualPolygon : ArenaPolygon;

    protected override Vector2 ClosestPointInArena(Vector2 point) => ArenaPolygon.ClosestPoint(point);
    public override Player Target => MasterBehavior?.Target;
    public override int DustType => DustID.PlatinumCoin;
    public override bool EnableMovementRestriction => IsActivated && Intensity == 1f;

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
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
            if (TOSharedData.NotClient)
                Projectile.Kill();
            return;
        }

        if (MasterBehavior.Phase2ArenaRequested)
        {
            if (TOSharedData.NotClient)
            {
                bool created = false;
                Projectile.NewProjectileAction<LegendaryNightPhase2Arena>(SourceAI, Target?.Center ?? Master.Center,
                    Vector2.Zero, SkeletronLegendHead.ArenaDamage * 4, 0f, action: p =>
                    {
                        LegendaryNightPhase2Arena arena = p.GetModProjectile<LegendaryNightPhase2Arena>();
                        arena.Master = Master;
                        arena.AppearanceStartMusicTime = MasterBehavior.Phase2ArenaStartMusicTime;
                        MasterBehavior.ArenaProjectile = p;
                        created = true;
                    });
                if (created)
                    Projectile.Kill();
            }
            return;
        }

        Projectile.Center = Master.Center;
        Projectile.velocity = Vector2.Zero;
        Projectile.timeLeft = 100;

        if (IsActivated || Intensity <= 0f || !_VisualPolygonInitialized)
        {
            _VisualPolygon = ArenaPolygon;
            _VisualPolygonInitialized = true;
        }

        Intensity = MathHelper.Clamp(Intensity + (IsActivated ? 1f : -1f) / 60f, 0f, 1f);

        if (EnableMovementRestriction)
            MovementRestriction();
        else
            OutofArenaTimer = 0;
    }

    public override bool CanHitPlayer(Player target) => EnableMovementRestriction && base.CanHitPlayer(target);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => EnableMovementRestriction
        && ArenaPolygon.BoundaryCollides(targetHitbox, Thickness);

    public override bool PreDraw(ref Color lightColor) => false;
}
