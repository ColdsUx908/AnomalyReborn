using Anomalies.Assets.Effects;
using ReLogic.Utilities;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Parameters;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 中心恒星，拥有光球生成、拦截奖励与吸收计数。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1..2]：未使用。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用。</para>
/// <para>ExtraAI：_LightOrbsSpawned、AbsorbedLightOrbCount（Int32），随后逐玩家 DamageStacks、RegenerationStacks（Int32）和 ExpectsPlatinum（Boolean）。</para>
/// <para>客户端状态：循环音效、星核闪光时刻和光球尾迹粒子，不同步；轨道与冲击波状态归属独立弹幕。</para>
/// <para>本地伴星查找缓存使用 owner + identity，失效时重新扫描，不加入额外同步载荷。</para>
/// </remarks>
public sealed partial class LegendEnergySphere : AnomalyModProjectile
{
    // 光球与其激光共用的基础色，着色器不再另写一份色值。
    /// <summary>金颜色。</summary>
    public static Vector3 GoldColor => new(1f, 0.82f, 0.06f);
    /// <summary>星光颜色。</summary>
    public static Vector3 StarlightColor => new(1f, 0.98f, 0.8f);
    /// <summary>蓄力音效。</summary>
    private static readonly SoundStyle _ChargeSound = new(LegendaryNightHandler.LegendaryNightPath + "Attacks/Stellar/LegendEnergySphereLoop") { IsLooped = true, Volume = 0.45f };
    /// <summary>蓄力音效最大音量距离，单位为世界像素。</summary>
    private const float ChargeSoundFullVolumeDistance = 350f;
    /// <summary>蓄力音效淡出距离，单位为世界像素。</summary>
    private const float ChargeSoundFadeOutDistance = 600f;
    private SlotId _ChargeSoundSlot;
    private bool _SoundStarted;
    private double _CoreFlashStart = double.NegativeInfinity;
    private LightOrbTrailSet _LightOrbTrails;
    private ProjectileNetReference _CompanionReference;

    private sealed class LightOrbTrailSet : ParticleSet
    {
        public override int ParticleLifetime => 0;

        public LightOrbTrailSet(int limit = 1024) : base(0, 0f) => ParticleLimit = limit;

        public override Particle SpawnParticle() =>
            throw new InvalidOperationException("Light orb trails accept explicit submissions only.");
    }

    private readonly LightOrbBonuses[] _PlayerBonuses = new LightOrbBonuses[Main.maxPlayers];
    private int _LightOrbsSpawned;
    internal int AbsorbedLightOrbCount { get; private set; }

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    private NPC Master => NPC.GetNPCFromIndex((int)Projectile.ai[0] - 1);
    internal SkeletronLegendHead Head => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) ? head : null;
    internal float SphereRadius => Head is SkeletronLegendHead head ? Radius(head.Phase2MusicElapsedTime)
        * SkeletronLegendHead.Mechanic7Parameters.StarScale(head.Phase2MusicElapsedTime) : 0f;
    private float CoreFlash => LightOrbCoreFlash((float)(Main.timeForVisualEffects - _CoreFlashStart));

    internal void FlashCore()
    {
        if (!Main.dedServ)
            _CoreFlashStart = Main.timeForVisualEffects;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.timeLeft = 1200;
    }

    public override bool CanHitPlayer(Player target) => Head is SkeletronLegendHead { StellarMechanicsActive: true } head
        && head.CurrentBehavior != SkeletronLegendHead.Behavior.Despawn && SphereRadius > 0f;

    public override void OnHitPlayer(Player target, Player.HurtInfo info) =>
        target.AddBuff(ModContent.BuffType<AstralSuppression>(), 15 * 60);
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => SkeletronLegendHead.Mechanic7Parameters.StarDissipation(Head?.Phase2MusicElapsedTime ?? 0) < 1f
        && new Circle(Projectile.Center, SphereRadius).Collides(targetHitbox);

    public override void OnKill(int timeLeft)
    {
        _LightOrbTrails?.Clear();
        _LightOrbTrails = null;
        if (!Main.dedServ && _SoundStarted && SoundEngine.TryGetActiveSound(_ChargeSoundSlot, out ActiveSound sound))
            sound.Stop();
    }
}