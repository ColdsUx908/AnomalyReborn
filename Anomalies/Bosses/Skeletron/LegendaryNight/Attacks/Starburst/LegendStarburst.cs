using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstTimeline;
using static Anomalies.Bosses.Skeletron.LegendaryNight.LegendaryNightStarburstParameters;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 单次星爆或一层步进星爆，负责预警、帧伤与淡出。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；ai[1]：普通星爆序号（0..11）或步进轮次（0..4）；ai[2]：步进层号（1..4），0 为普通星爆。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用；年龄由主体音乐帧减去星爆节点计算。</para>
/// <para>无实体专属 ExtraAI；爆点与半径通过所属竞技场几何恢复，服务器生成。</para>
/// </remarks>
public sealed class LegendStarburst : AnomalyModProjectile
{
    internal const float ImpactMaxOpacity = 0.65f;
    private const int ImpactFadeIn = 10;
    private const int ImpactEndAge = StarburstDamageDuration + StarburstFadeDuration;
    private const int StepRoundCount = 5;
    private const int StepCount = 4;

    internal SkeletronLegendHead Head => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) ? head : null;
    private LegendaryNightPhase2Arena Arena => Head?.Phase2Arena;
    private int Index => (int)Projectile.ai[1];
    private int Step => (int)Projectile.ai[2];
    private bool IsStepped => Step != 0;
    private int ImpactTime => IsStepped ? GetStepImpactTime(Index, Step) : GetStarburstTime(Index);
    private int Age => (Head?.Phase2MusicElapsedTime ?? 0) - ImpactTime;
    private bool IsWarningTime => Age < (UsesCircleVisuals || UsesRingVisuals ? 0 : -ImpactFadeIn);
    private int WarningDuration => (IsStepped ? ImpactTime - GetStepSpawnTime(Index, Step) : StarburstWarningDuration) - ImpactFadeIn;
    private bool IsRing => IsStepped ? Step > 1 : Index % 2 == 1;
    private int VisualEndAge => IsStepped ? ImpactEndAge : GetNormalEndAge(Index);

    private static int GetNormalEndAge(int index) => index % 2 == 0
        ? LegendStarburstCircleVisuals.EndAge : LegendStarburstRingVisuals.EndAge;
    private float Opacity
    {
        get
        {
            int age = Age;
            if ((UsesCircleVisuals || UsesRingVisuals) && age < 0)
                return 0.22f * MathHelper.Clamp(1f + age / (float)StarburstWarningDuration, 0f, 1f);
            if (age < -ImpactFadeIn)
            {
                if (IsStepped && !HasStepWarning(Index, Step))
                    return 0f;

                float progress = MathHelper.Clamp(1f + (age + ImpactFadeIn) / (float)WarningDuration, 0f, 1f);
                return IsStepped ? MathHelper.SmoothStep(0f, 0.22f, progress) : 0.22f * progress;
            }

            float fadeOut = MathHelper.Clamp((ImpactEndAge - age) / (float)StarburstFadeDuration, 0f, 1f);
            float fadeIn = MathHelper.Clamp((age + ImpactFadeIn + 1f) / (ImpactFadeIn + 1f), 0f, 1f);
            return ImpactMaxOpacity * fadeIn * fadeOut;
        }
    }
    private bool _Hit;

    private static int GetStepImpactTime(int round, int step)
    {
        int roundStart = round == StepRoundCount - 1
            ? SkeletronLegendHead.Mechanic6Timeline.LastLaserImpactMusicTime
            : SkeletronLegendHead.Mechanic6Timeline.StartMusicTime + round * StepCount * RapidStarburstInterval;
        return roundStart + (step - 1) * RapidStarburstInterval;
    }

    private static bool HasStepWarning(int round, int step) => round == 0 && step == 1;

    private static int GetStepSpawnTime(int round, int step) => HasStepWarning(round, step)
        ? GetStarburstTime(StarburstCount - 1) : GetStepImpactTime(round, step) - ImpactFadeIn;

    private bool IsActive(SkeletronLegendHead head) => IsStepped
        ? head is { CurrentBehavior: not SkeletronLegendHead.Behavior.Despawn }
            && (head.Mechanic5Active || head.Mechanic6Active)
        : head is { StarburstActive: true };

    private Vector2 GetCenter(LegendaryNightPhase2Arena arena) =>
        IsStepped ? arena.Projectile.Center : arena.PredictStarburstPosition(Index);

    private Vector2 GetRadii(LegendaryNightPhase2Arena arena)
    {
        if (!IsStepped)
            return new Vector2(arena.Radius * (IsRing ? 2.25f : 1.5f), IsRing ? arena.Radius * 0.6f : 0f);

        // 面积与半径平方成正比，每层增加整个竞技场面积的四分之一。
        return arena.Radius * new Vector2(MathF.Sqrt(Step / (float)StepCount), MathF.Sqrt((Step - 1f) / StepCount));
    }

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override bool PreDraw(ref Color lightColor) => false;
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
        if (!IsActive(Head) || Arena is not LegendaryNightPhase2Arena arena || Age >= VisualEndAge)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 100;
        Projectile.Center = GetCenter(arena);
        if (Main.dedServ)
            return;
        if (Age == -StarburstSoundLeadTime)
            SoundEngine.PlaySound(StarburstSound);
        if (!_Hit && Age is >= 0 and < StarburstDamageDuration && Colliding(Projectile.Hitbox, Main.LocalPlayer.Hitbox) == true)
        {
            if (!Main.LocalPlayer.active || Main.LocalPlayer.dead || Main.LocalPlayer.ghost)
                return;
            _Hit = true;
            LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, true);
        }
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Arena is not LegendaryNightPhase2Arena arena)
            return false;
        Vector2 radii = GetRadii(arena);
        if (IsStepped)
        {
            // 与场地边界一样按玩家中心判定，跨边界的命中框只归属一层；边界归内侧。
            float distanceSquared = Vector2.DistanceSquared(targetHitbox.Center.ToVector2(), Projectile.Center);
            return distanceSquared <= radii.X * radii.X && (!IsRing || distanceSquared > radii.Y * radii.Y);
        }

        return IsRing ? new Annulus(Projectile.Center, radii.Y, radii.X).Collides(targetHitbox)
            : new Circle(Projectile.Center, radii.X).Collides(targetHitbox);
    }
    internal static void EnsureAttack(SkeletronLegendHead head)
    {
        if (!TONetworkEnvironment.NotClient || head.Phase2Arena is not LegendaryNightPhase2Arena arena)
            return;
        int time = head.Phase2MusicElapsedTime;
        if (head.StarburstActive)
        {
            int index = GetStarburstIndex(time);
            int endAge = GetNormalEndAge(index);
            if (index >= 0 && time < GetStarburstTime(index) + endAge)
                EnsureBurst(index, 0, arena.PredictStarburstPosition(index));
        }

        if (head.Mechanic5Active || head.Mechanic6Active)
        {
            for (int round = 0; round < StepRoundCount; round++)
            {
                for (int step = 1; step <= StepCount; step++)
                {
                    if (time >= GetStepSpawnTime(round, step)
                        && time < GetStepImpactTime(round, step) + ImpactEndAge)
                        EnsureBurst(round, step, arena.Projectile.Center);
                }
            }
        }

        void EnsureBurst(int index, int step, Vector2 center)
        {
            foreach (Projectile other in Projectile.ActiveProjectiles)
                if (other.ModProjectile is LegendStarburst && (int)other.ai[0] == head.NPC.whoAmI + 1
                    && (int)other.ai[1] == index && (int)other.ai[2] == step)
                    return;

            Projectile.NewProjectileAction<LegendStarburst>(head.SourceAI, center,
                Vector2.Zero, SkeletronLegendHead.StarburstDamage, 0f, action: p =>
                {
                    p.ai[0] = head.NPC.whoAmI + 1;
                    p.ai[1] = index;
                    p.ai[2] = step;
                });
        }
    }
    internal bool TryGetImpact(out Vector2 center, out Vector2 radii, out float opacity)
    {
        center = default;
        radii = default;
        opacity = Opacity;
        if (IsWarningTime || opacity <= 0f || (IsStepped && !IsActive(Head)) || Arena is not LegendaryNightPhase2Arena arena)
            return false;

        center = GetCenter(arena);
        radii = GetRadii(arena);
        return radii.X > radii.Y;
    }

    internal bool UsesCircleVisuals => !IsStepped && !IsRing && LegendStarburstCircleAssets.IsLoaded;

    internal bool UsesRingVisuals => !IsStepped && IsRing && LegendStarburstRingAssets.IsLoaded;

    internal bool TryGetVisualFrame(out LegendStarburstVisuals.Burst burst)
    {
        burst = default;
        int age = Age;
        bool visible = UsesCircleVisuals ? LegendStarburstCircleVisuals.IsVisible(age)
            : UsesRingVisuals && LegendStarburstRingVisuals.IsVisible(age);
        if (!visible || Arena is not LegendaryNightPhase2Arena arena || !IsActive(Head))
            return false;
        Vector2 radii = GetRadii(arena);
        burst = new(GetCenter(arena), radii.X, radii.Y, arena.SmallCircleRadius, age);
        return true;
    }

    internal void Draw(SpriteBatch spriteBatch)
    {
        bool warning = IsWarningTime;
        if (!warning && UsesRingVisuals)
            return;
        // 圆形星爆在法阵线条之后统一提交。
        if (!warning && UsesCircleVisuals)
        {
            // Keep the full hazard boundary readable while the gold wave expands from its center.
            // This is a fading warning outline, not a second early explosion or damage change.
            if (Age < LegendStarburstCircleVisuals.ExpansionDuration && IsActive(Head)
                && Arena is LegendaryNightPhase2Arena circleArena)
                AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, GetCenter(circleArena), GetRadii(circleArena).X,
                    Color.Red, 0.22f * (1f - LegendStarburstCircleVisuals.Expansion(Age)), 3f);
            return;
        }
        if (Arena is not LegendaryNightPhase2Arena arena || (IsStepped && !IsActive(Head))
            || Opacity <= 0f || (!warning && LegendaryNightOverlay.UseImpactFlame))
            return;
        Vector2 center = GetCenter(arena);
        Vector2 radii = GetRadii(arena);
        float opacity = Opacity;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        using RenderTargetLease lease = RenderTargetPool.Shared.Rent(Main.instance.GraphicsDevice,
            Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
        using (lease.Scope(clearColor: Color.Transparent))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.DrawFromCenter(circle, center - Main.screenPosition, null, Color.White,
                scale: radii.X * 2f / circle.Width);
            spriteBatch.End();
            if (IsRing)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, TODrawUtils.EraseBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.DrawFromCenter(circle, center - Main.screenPosition, null, Color.White,
                    scale: radii.Y * 2f / circle.Width);
                spriteBatch.End();
            }
        }
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, (warning ? Color.Red * opacity : new Color(234, 154, 54) * (opacity * 0.45f)));
        spriteBatch.End();
        Color color = warning ? Color.Red : new Color(240, 139, 50);
        AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, radii.X, color, opacity,
            warning ? 3f : LegendaryNightPhase2Arena.LaserWidth);
        if (IsRing)
            AnomalyDrawUtils.DrawGlowingCircle(spriteBatch, center, radii.Y, color, opacity,
                warning ? 3f : LegendaryNightPhase2Arena.LaserWidth);
    }
}
