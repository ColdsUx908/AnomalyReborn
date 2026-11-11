using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronHandler : ILoadable
{
    internal static readonly Color BoneColor = new(233, 225, 199);

    /// <summary>在 tML 内容加载时安装手部死亡 IL 修改，失败日志使用加载入口的所属模组。</summary>
    /// <param name="mod">由 tML 提供的所属模组。</param>
    void ILoadable.Load(Mod mod) => IL_NPC.checkDead += il => RestrictHandDeathCasters(il, mod);

    // IL 钩子交由 tML 卸载，此入口不重复退订。
    void ILoadable.Unload() { }

    private static void RestrictHandDeathCasters(ILContext il, Mod mod)
    {
        // TODO(tModLoader 升级，尤其 1.4.5)：重新核对 checkDead 的手部死亡生成分支和生成源。
        // 当前安装的 1.4.4.9 在 type == SkeletronHand 分支内以 NaturalSpawn 生成三个暗黑法师；
        // 新版源码已改用父实体生成源，届时需调整或替换本 IL 匹配，不能沿用当前布局假设。
        ILCursor cursor = new(il);
        ILLabel end = null;
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdarg0(),
                i => i.MatchLdfld<NPC>(nameof(NPC.type)),
                i => i.MatchLdcI4(NPCID.SkeletronHand),
                i => i.MatchBneUn(out end)))
        {
            AnomalyUtils.ILFailure(mod, nameof(SkeletronHandler), "Could not locate Skeletron hand death caster branch.");
            return;
        }

        // 在进入三次生成/每次千次选点循环之前跳过整个分支，不改世界标志或其他 NPC 的生成。
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<NPC, bool>>(hand => Aroma || !SkeletronHand.GetInstance(hand).ShouldProcess);
        cursor.Emit(OpCodes.Brfalse, end);
    }

    public static void NormalMovement(NPC npc, Player target, float maxSpeedX, float maxSpeedY, float accelerationX, float accelerationY, float targetDistanceAboveTarget, float rotationFactor = 15f)
    {
        npc.Face(target);

        // Y 方向：保持自己在玩家上方
        if (npc.Top.Y > target.Top.Y - targetDistanceAboveTarget)
        {
            if (npc.velocity.Y > 0f)
                npc.velocity.Y *= 0.98f;

            npc.velocity.Y -= accelerationY;
            if (npc.velocity.Y > maxSpeedY)
                npc.velocity.Y = maxSpeedY;
        }
        else if (npc.Top.Y < target.Top.Y - targetDistanceAboveTarget)
        {
            if (npc.velocity.Y < 0f)
                npc.velocity.Y *= 0.98f;

            npc.velocity.Y += accelerationY;
            if (npc.velocity.Y < -maxSpeedY)
                npc.velocity.Y = -maxSpeedY;
        }

        // X 方向：水平追踪玩家
        if (npc.Center.X > target.Center.X)
        {
            if (npc.velocity.X > 0f)
                npc.velocity.X *= 0.98f;

            npc.velocity.X -= accelerationX;
            if (npc.velocity.X > maxSpeedX)
                npc.velocity.X = maxSpeedX;
        }

        if (npc.Center.X < target.Center.X)
        {
            if (npc.velocity.X < 0f)
                npc.velocity.X *= 0.98f;

            npc.velocity.X += accelerationX;
            if (npc.velocity.X < -maxSpeedX)
                npc.velocity.X = -maxSpeedX;
        }

        npc.rotation = npc.velocity.X / rotationFactor;
    }

    public static float GetSpinMoveSpeed(float distance) =>
        distance < SkeletronHead.SpinSlowdownEndDistance
            ? Utils.Remap(distance, SkeletronHead.SpinSlowdownMinDistance, SkeletronHead.SpinSlowdownEndDistance, SkeletronHead.SpinMinMoveSpeed, SkeletronHead.SpinBaseMoveSpeed)
            : Utils.Remap(distance, SkeletronHead.SpinAccelerationStartDistance, SkeletronHead.SpinAccelerationMaxDistance, SkeletronHead.SpinBaseMoveSpeed, SkeletronHead.SpinMaxMoveSpeed);

    public static void SpinMovement(NPC npc, Player target, float spinSpeed, float moveSpeed)
    {
        // 头部持续自转
        npc.rotation += npc.direction * spinSpeed;

        // 计算指向玩家的向量
        Vector2 targetVector = target.Center - npc.Center;
        float distance = targetVector.Length();

        if (distance > 0f)
        {
            // 归一化后乘以移动速度，等价于原代码的 8f / distance 缩放
            float speedFactor = moveSpeed / distance;
            npc.velocity.X = targetVector.X * speedFactor;
            npc.velocity.Y = targetVector.Y * speedFactor;
        }
        else
        {
            npc.velocity = Vector2.Zero;
        }
    }

    internal static void FireAcceleratingSpread(NPC source, Player target, int count, float spreadDegrees, Action<Vector2, Vector2> fire)
    {
        Vector2 baseVelocity = (target.Center - source.Center).SafeNormalize(Vector2.UnitY) * 6f;
        Vector2 position = source.Center + baseVelocity * 5f;
        float halfSpread = spreadDegrees * 0.5f;
        float centerIndex = (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float angle = MathHelper.ToRadians(MathHelper.Lerp(-halfSpread, halfSpread, i / (count - 1f)));
            float speedMultiplier = MathHelper.Lerp(0.5f, 1.5f, MathF.Abs(centerIndex - i) / centerIndex);
            fire(position, baseVelocity.RotatedBy(angle) * speedMultiplier);
        }
    }

    public static void FireSkull(NPC npc, Player target, int amount, float radian, float skullProjSpeed, int damage, bool fireFromMouth = true, bool addNPCVelocity = true, int mode = Skull.Homing)
    {
        Vector2 velocity = npc.GetVelocityTowards(target, skullProjSpeed) + (addNPCVelocity ? npc.velocity : Vector2.Zero);
        Vector2 position = npc.Center + velocity;
        if (fireFromMouth)
            position += new Vector2(0f, 25f * npc.scale).RotatedBy(npc.rotation);

        int type = ProjectileID.Skull;

        if (amount == 1)
            Projectile.NewProjectileAction(npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);
        else
            Projectile.NewProjectilesArc(amount, radian, npc.GetSource_FromAI(), position, velocity, type, damage, 0f, action: FireSkullAction);

        void FireSkullAction(Projectile p)
        {
            Skull.InitializeFlight(p, mode, Skull.UltraMaxSpeed, Skull.UltraAcceleration);
            Skull.SetSource(p, npc);
            p.timeLeft = 600;
        }
    }
}
