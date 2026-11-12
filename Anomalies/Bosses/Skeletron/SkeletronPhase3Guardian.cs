namespace Anomalies.Bosses.Skeletron;

/// <summary>三阶段小守卫：直落雨幕、十六边形巡卫和向中心收束的骨潮。</summary>
/// <remarks>
/// <para>ai[0]：头部索引 + 1；ai[1]：年龄，负值为最终阶段淡出帧数的相反数；ai[2]：2 雨幕、3 巡卫、4 骨潮、5 最终阶段径向直射。</para>
/// <para>localAI、框架 Timer 未使用。ExtraAI：中心、初始偏移、角速度、场地半径。</para>
/// <para>没有独立客户端状态；位置、缩放和透明度均由同步年龄计算。</para>
/// </remarks>
public sealed class SkeletronPhase3Guardian : AnomalyModProjectile
{
    private const int FinalFadeDuration = 30;
    public enum Behavior : byte
    {
        Rain = 2,
        Patrol = 3,
        Tide = 4,
        Final = 5,
    }

    // 生成入口与 ai[2] 的既有协议值保持一致。
    public const int Rain = (int)Behavior.Rain;
    public const int Patrol = (int)Behavior.Patrol;
    public const int Tide = (int)Behavior.Tide;
    public const int Final = (int)Behavior.Final;
    public const int PreparationTime = SkeletronHead.Phase3Patterns.PreparationTime;
    public const int PatrolTime = SkeletronHead.Phase3Patterns.PatrolTime;
    public const float ExitDistance = 3000f;
    public const float TideSpeed = 7f;
    public const float CenterFadeRadius = 160f;

    public override string Texture => TOAssetUtils.FormatVanillaProjectileTexturePath(ProjectileID.BabySkeletronHead);
    public override string LocalizationCategory => "Bosses.Skeletron";

    public Vector2 Destination { get; set; }
    public Vector2 InitialOffset { get; set; }
    public float AngularSpeed { get; set; }
    public float ArenaRadius { get; set; }
    public Behavior CurrentBehavior => (Behavior)(int)Projectile.ai[2];
    private int SourceHeadIndex => (int)Projectile.ai[0] - 1;
    private ref float Age => ref Projectile.ai[1];
    private bool IsRain => CurrentBehavior == Behavior.Rain;
    private bool IsFinal => CurrentBehavior == Behavior.Final;
    private bool IsPreparing => !IsRain && !IsFinal && Age <= PreparationTime;

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 42;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = 720;
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.WriteVector2(Destination);
        writer.WriteVector2(InitialOffset);
        writer.Write(AngularSpeed);
        writer.Write(ArenaRadius);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        Destination = reader.ReadVector2();
        InitialOffset = reader.ReadVector2();
        AngularSpeed = reader.ReadSingle();
        ArenaRadius = reader.ReadSingle();
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage()
    {
        if (Age < 0f || IsPreparing || Projectile.scale < 0.5f)
            return false;

        // 直接读取主体阶段，不等待本弹幕本帧 AI，避免进入 ToFinal 后仍有碰撞窗口。
        int headIndex = SourceHeadIndex;
        if ((uint)headIndex < Main.maxNPCs && Main.npc[headIndex] is { active: true, type: NPCID.SkeletronHead } head
            && SkeletronHead.GetInstance(head) is { ShouldProcess: true, IsFinalStage: true } && !IsFinal)
            return false;
        return null;
    }

    public override void AI()
    {
        int headIndex = SourceHeadIndex;
        // 主体死亡后沿用同步的轨道数据与寿命，不依赖主体继续存在。
        bool hasHead = (uint)headIndex < Main.maxNPCs
            && Main.npc[headIndex] is { active: true, type: NPCID.SkeletronHead }
            && SkeletronHead.GetInstance(Main.npc[headIndex]).ShouldProcess;
        if (hasHead)
        {
            SkeletronHead head = SkeletronHead.GetInstance(Main.npc[headIndex]);
            if (!(head.CurrentPhase == SkeletronHead.Phase.Phase3 || head.IsFinalStage)
                || head.CurrentBehavior == SkeletronHead.Behavior.Despawn)
            {
                if (TONetworkEnvironment.NotClient)
                    Projectile.Kill();
                return;
            }

            if (head.IsFinalStage && !IsFinal)
            {
                FadeOut();
                return;
            }
        }

        if (Age < 0f)
        {
            FadeOut();
            return;
        }

        int age = (int)Age++;
        Projectile.rotation += 0.3f;
        switch (CurrentBehavior)
        {
            case Behavior.Rain:
                Projectile.Center += Projectile.velocity;
                break;
            case Behavior.Final:
                Projectile.velocity *= 1.02f;
                Projectile.Center += Projectile.velocity;
                break;
            case Behavior.Patrol:
                if (HasOrbitData())
                    UpdatePatrol();
                break;
            case Behavior.Tide:
                if (HasOrbitData())
                    UpdateTide();
                break;
        }

        // 客户端等待完整初始载荷，不能在零半径上推演或删除弹幕。
        bool HasOrbitData() => ArenaRadius > 0f && InitialOffset.LengthSquared() > 0f;

        void FadeOut()
        {
            if (Age >= 0f)
            {
                Age = -1f;
                if (TONetworkEnvironment.NotClient)
                    Projectile.netUpdate = true;
            }

            // 从进入过渡时的实际透明度线性淡出；不再执行准备渐显或骨潮透明度覆盖。
            int fadeAge = Math.Min(FinalFadeDuration, (int)-Age);
            Projectile.Opacity *= (FinalFadeDuration - fadeAge) / (float)(FinalFadeDuration - fadeAge + 1);
            Projectile.Center += Projectile.velocity;
            Projectile.rotation += 0.3f;
            Age--;
            if (fadeAge >= FinalFadeDuration && TONetworkEnvironment.NotClient)
                Projectile.Kill();
        }

        void UpdatePatrol()
        {
            Vector2 offset = GetPatrolOffset(age);
            if (offset.Length() >= Math.Max(ExitDistance, ArenaRadius + 160f) && TONetworkEnvironment.NotClient)
            {
                // 至少飞离中心 3000 像素且越过竞技场后才消失。
                Projectile.Kill();
                return;
            }

            UpdateOrbitPosition(offset, age <= PreparationTime ? offset : GetPatrolOffset(age - 1));
            if (age >= PreparationTime)
                Projectile.Opacity = 1f;
        }

        void UpdateTide()
        {
            Vector2 offset = GetTideOffset(age);
            float radius = offset.Length();
            // 骨潮走到中心，最后 160 像素连续缩小淡出。
            float fade = MathHelper.Clamp(radius / CenterFadeRadius, 0f, 1f);
            Projectile.scale = fade;
            Projectile.Opacity = fade;
            if (radius <= 0f && TONetworkEnvironment.NotClient)
            {
                Projectile.Kill();
                return;
            }

            UpdateOrbitPosition(offset, age <= PreparationTime ? offset : GetTideOffset(age - 1));
        }

        void UpdateOrbitPosition(Vector2 offset, Vector2 previousOffset)
        {
            // 用相邻同步年龄重建扫掠，避免晚到快照将传送修正计入碰撞。
            // 预备阶段渐显且无伤害，不把出生位置到轨道位置算作扫掠。
            Projectile.velocity = age <= PreparationTime ? Vector2.Zero : offset - previousOffset;
            Projectile.Center = Destination + offset;
            if (age < PreparationTime)
                Projectile.Opacity = MathHelper.Clamp((age + 1f) / 20f, 0f, 1f) * 0.45f;
        }

        Vector2 GetTideOffset(int sampleAge)
        {
            float initialRadius = InitialOffset.Length();
            int flightAge = Math.Max(0, sampleAge - PreparationTime);
            float radius = Math.Max(0f, initialRadius - flightAge * TideSpeed);
            return (InitialOffset / initialRadius).RotatedBy(AngularSpeed * sampleAge) * radius;
        }

        Vector2 GetPatrolOffset(int sampleAge)
        {
            float initialRadius = InitialOffset.Length();
            int flightAge = Math.Max(0, sampleAge - PreparationTime);
            // 各层均转过整数个半圈，停止转动时双缺口回到左右。
            float angle = InitialOffset.ToRotation() + AngularSpeed * Math.Min(flightAge, PatrolTime);
            float node = angle / Hexadecagon.VertexAngle;
            int index = (int)MathF.Floor(node);
            Vector2 start = (index * Hexadecagon.VertexAngle).ToRotationVector2();
            Vector2 end = ((index + 1) * Hexadecagon.VertexAngle).ToRotationVector2();
            int layer = (int)MathF.Round((initialRadius / ArenaRadius - SkeletronHead.Phase3Patterns.PatrolRadiusFactor(0)) / (SkeletronHead.Phase3Patterns.PatrolRadiusFactor(1) - SkeletronHead.Phase3Patterns.PatrolRadiusFactor(0)));
            int exitAge = Math.Max(0, flightAge - PatrolTime - layer * SkeletronHead.Phase3Patterns.PatrolExitInterval);
            // 巡行期间始终沿既定十六边形；只有离场时向外飞出。
            int initialNode = ((int)MathF.Round(InitialOffset.ToRotation() / MathHelper.TwoPi * SkeletronHead.Phase3Patterns.PatrolCount)
                + SkeletronHead.Phase3Patterns.PatrolCount) % SkeletronHead.Phase3Patterns.PatrolCount;
            float exitAcceleration = SkeletronHead.Phase3Patterns.PatrolExitAcceleration(layer, initialNode);
            float patrolRadius = initialRadius + 0.5f * exitAcceleration * exitAge * exitAge;
            return Vector2.Lerp(start, end, node - index) * patrolRadius;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CanDamage() == false)
            return false;
        float width = Projectile.width * Projectile.scale;
        Vector2 extent = Projectile.velocity.SafeNormalize(Vector2.UnitY) * (width * 0.5f);
        return RotatedRectangle.FromLineSegment(Projectile.Center - Projectile.velocity - extent,
            Projectile.Center + extent, width).Collides(targetHitbox);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
            SkeletronCombatVisuals.BoneColor * Projectile.Opacity, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale,
            SpriteEffects.None);
        return false;
    }
}
