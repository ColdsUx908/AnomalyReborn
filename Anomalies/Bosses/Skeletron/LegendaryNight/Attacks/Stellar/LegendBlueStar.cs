namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 环绕中心恒星的蓝星，拥有公转与吸积流。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1]：结算冲击波已生成标志，0未生成、1已生成；ai[2]：未使用。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；公转由主体音乐绝对帧确定。</para>
/// <para>ExtraAI：PrimaryProjectile 的 owner + identity 网络引用；客户端吸积轨迹和顶点缓存不参与同步。</para>
/// <para>服务器生成蓝星和冲击波；撞击后保留60帧以完成吸积流尾部。</para>
/// </remarks>
public sealed partial class LegendBlueStar : AnomalyModProjectile
{
    /// <summary>伴星半径比例。</summary>
    internal const float CompanionRadiusRatio = 0.55f;
    /// <summary>伴星初始公转半径，单位为世界像素。</summary>
    private const float CompanionInitialOrbitRadius = 1900f;
    /// <summary>自转或公转角速度，单位为弧度/游戏帧。</summary>
    private const float CompanionInitialAngularSpeed = 0.009f;
    /// <summary>伴星相机距离，单位为世界像素。</summary>
    internal const float CompanionCameraDistance = 1400f;
    /// <summary>伴星相机倾角。</summary>
    private const float CompanionCameraInclination = 0.24f;
    /// <summary>伴星表面颜色。</summary>
    internal static Vector3 CompanionSurfaceColor => new(0.36f, 0.56f, 1f);
    /// <summary>伴星核心颜色。</summary>
    internal static Vector3 CompanionCoreColor => new(0.85f, 0.92f, 1f);

    /// <summary>伴星公转角度缓存。</summary>
    private static readonly float[] _CompanionOrbitAngles = CreateCompanionOrbitAngles();

    private ProjectileNetReference _PrimaryReference;
    public Projectile PrimaryProjectile
    {
        get => _PrimaryReference.Resolve();
        set => _PrimaryReference.Set(value);
    }
    private LegendEnergySphere Primary => PrimaryProjectile?.ModProjectile as LegendEnergySphere;
    private SkeletronLegendHead Head => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) ? head : null;
    private float SphereRadius => Primary?.SphereRadius ?? 0f;
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;
    public override bool PreDraw(ref Color lightColor) => false;
    protected override void SendAnomalyExtraAI(BinaryWriter writer) => _PrimaryReference.Write(writer);
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader) => _PrimaryReference = ProjectileNetReference.Read(reader);
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.hide = true;
        Projectile.timeLeft = 100;
    }
    public override void AI()
    {
        SkeletronLegendHead head = Head;
        LegendEnergySphere primary = Primary;
        if (head is not { StellarMechanicsActive: true } || primary is null
            || head.CurrentBehavior == SkeletronLegendHead.Behavior.Despawn)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int time = head.Phase2MusicElapsedTime;
        Projectile.timeLeft = 100;
        Vector3 position = CompanionOrbitPosition(time - SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime);
        ProjectCompanion(position, out Vector2 offset, out _, out _);
        Projectile.Center = primary.Projectile.Center + offset;
        if (!Main.dedServ)
            PrepareCompanionMatter(time);
        EnsureImpactShockwave();
        // 蓝星撞击后保留60帧，让已经离开星面的吸积流自然结束。
        if (time >= SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime + 60 && TONetworkEnvironment.NotClient)
            Projectile.Kill();

        void EnsureImpactShockwave()
        {
            if (time >= SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime && Projectile.ai[1] == 0f && TONetworkEnvironment.NotClient
                && head.Phase2Arena is LegendaryNightPhase2Arena arena)
            {
                bool created = false;
                Projectile.NewProjectileAction<LegendCompanionShockwave>(SourceAI, arena.Projectile.Center, Vector2.Zero,
                    SkeletronLegendHead.CompanionShockwaveDamage(primary.AbsorbedLightOrbCount), 0f, action: p =>
                    {
                        p.ai[0] = SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime;
                        p.ai[1] = arena.Radius * LegendStellarShockwave.CompanionShockwaveRadiusRatio(primary.AbsorbedLightOrbCount);
                        p.ai[2] = head.NPC.whoAmI + 1;
                        created = true;
                    });
                if (created)
                {
                    Projectile.ai[1] = 1f;
                    Projectile.netUpdate = true;
                }
            }
        }
    }
    public override void OnKill(int timeLeft)
    {
        _CompanionMatter = null;
        _MatterStreamPath = null;
        _MatterRibbonVertices = null;
    }
    internal void DrawBlueCompanion(SpriteBatch spriteBatch, bool behind)
    {
        int time = Head?.Phase2MusicElapsedTime ?? -1;
        if (Primary is not LegendEnergySphere primary || time < SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime || time >= SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime)
            return;
        Vector3 position = CompanionOrbitPosition(time - SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime);
        if ((position.Z > 0f) != behind || !ProjectCompanion(position, out Vector2 offset, out float scale, out float opacity))
            return;
        primary.DrawBody(spriteBatch, primary.Projectile.Center + offset, SphereRadius * CompanionRadiusRatio * scale, blue: true, opacity: opacity);
    }
    internal static float CompanionOrbitRadius(float age)
    {
        const float Duration = SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime - SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime;
        const float ContactRadius = SkeletronLegendHead.Mechanic6Parameters.SphereRadius * (1f + CompanionRadiusRatio);
        float progress = MathHelper.Clamp(age / Duration, 0f, 1f);
        // 前70%时间仍在场地外；径向衰减集中在最后约4秒，连续加速而非突然切换。
        return MathHelper.Lerp(CompanionInitialOrbitRadius, ContactRadius, MathF.Pow(progress, 5f));
    }
    internal static float CompanionAngularSpeed(float age)
    {
        float radius = CompanionOrbitRadius(age);
        return CompanionInitialAngularSpeed * CompanionInitialOrbitRadius * CompanionInitialOrbitRadius / (radius * radius);
    }

    /// <summary>计算伴星公转角度缓存。</summary>
    private static float[] CreateCompanionOrbitAngles()
    {
        const int Duration = SkeletronLegendHead.Mechanic6Timeline.CompanionImpactMusicTime - SkeletronLegendHead.Mechanic6Timeline.CompanionStartMusicTime;
        float[] angles = new float[Duration + 1];
        double angle = -MathHelper.PiOver2;
        angles[0] = (float)angle;
        for (int i = 1; i <= Duration; i++)
        {
            // 固定音乐帧的中点积分表，各端独立恢复相同相位，不依赖AI帧推进。
            angle += CompanionAngularSpeed(i - 0.5f);
            angles[i] = (float)angle;
        }
        return angles;
    }
    internal static float CompanionOrbitAngle(float age)
    {
        float elapsed = MathHelper.Clamp(age, 0f, _CompanionOrbitAngles.Length - 1);
        int frame = (int)elapsed;
        return MathHelper.Lerp(_CompanionOrbitAngles[frame],
            _CompanionOrbitAngles[Math.Min(frame + 1, _CompanionOrbitAngles.Length - 1)], elapsed - frame);
    }
    internal static Vector3 CompanionOrbitPosition(float age)
    {
        float radius = CompanionOrbitRadius(age);
        float angle = CompanionOrbitAngle(age);
        return new Vector3(MathF.Cos(angle) * radius, 0f, MathF.Sin(angle) * radius);
    }
    internal static bool ProjectCompanion(Vector3 position, out Vector2 offset, out float scale, out float opacity)
    {
        const float NearPlane = 180f;
        const float FadeDepth = 900f;
        // 摄像机略向下俯视，轨道本身始终为 Y=0 的 xOz 圆周；近侧放大，远侧缩小。
        float depth = CompanionCameraDistance + position.Z * MathF.Cos(CompanionCameraInclination) + position.Y * MathF.Sin(CompanionCameraInclination);
        scale = CompanionCameraDistance / Math.Max(depth, NearPlane);
        offset = new Vector2(position.X, position.Y * MathF.Cos(CompanionCameraInclination) - position.Z * MathF.Sin(CompanionCameraInclination)) * scale;
        opacity = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((depth - NearPlane) / FadeDepth, 0f, 1f));
        return depth > NearPlane;
    }
}