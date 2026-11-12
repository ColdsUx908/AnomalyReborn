namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private void TryEnterFinal()
    {
        if (!TONetworkEnvironment.NotClient || !ShouldEnterFinal)
            return;

        NPC.dontTakeDamage = true;
        ApplyFinalDebuffImmunity();
        BeginPhaseTransition(Phase.ToFinal, Behavior.ToFinal);
    }

    private void ApplyFinalDebuffImmunity()
    {
        Array.Fill(NPC.buffImmune, true);
        Array.Clear(NPC.buffType);
        Array.Clear(NPC.buffTime);
        // 清除进入阶段之前累积的负生命恢复，防止旧 DOT 的尾帧继续扣血。
        if (NPC.lifeRegen < 0)
            NPC.lifeRegen = 0;
        if (NPC.lifeRegenCount < 0)
            NPC.lifeRegenCount = 0;
    }

    // LocalPhase：0 Ultra 等待竞技场并归位，普通异象原地开始；1 静默，2 蓄力。
    private void ToFinalAI()
    {
        NPC.damage = 0;
        NPC.rotation = NPC.rotation.AngleTowards(0f, MathHelper.Pi / FinalStillDuration);
        NPC.velocity = Vector2.Zero;
        if (Ultra)
        {
            SkeletronPhase3Arena arena = FindActivePhase3Arena();
            if (arena is null)
                return;

            // 三阶段头部原本就停在中心；网络延迟时先完成归位，再计算静默时间。
            NPC.Center = arena.ArenaPolygon.Center;
        }
        if (CurrentLocalPhase == 0)
        {
            if (TONetworkEnvironment.NotClient)
            {
                CurrentLocalPhase = 1;
                Timer = 0f;
            }
            return;
        }

        if (CurrentLocalPhase == 1)
        {
            if (++Timer >= FinalStillDuration && TONetworkEnvironment.NotClient)
            {
                CurrentLocalPhase = 2;
                Timer = 0f;
            }
            return;
        }

        Timer++;

        if (Timer < FinalChargeDuration || !TONetworkEnvironment.NotClient)
            return;

        Projectile.NewProjectileAction<BoneShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p =>
        {
            p.ai[0] = BoneShockwave.FinalVariant;
            p.timeLeft = p.GetModProjectile<BoneShockwave>().LifeTime;
            p.scale = 0f;
        });
        NPC.dontTakeDamage = false;
        CurrentPhase = Phase.Final;
        CurrentBehavior = Behavior.FinalGuardian;
        CurrentLocalPhase = 0;
        Timer = 0f;
    }

    private void FinalAI()
    {
        // 使用原版地牢守卫的高防御与接触伤害，保留防御结算后的暴击倍率。
        NPC.defense = 9999;
        NPC.damage = 1000;
        AnomalyNPC.BossBar_Enraged = true;
        NPC.Face(Target);
        NPC.rotation += NPC.direction * SpinSpeed;
        NPC.velocity = (Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * FinalChaseSpeed;

        // 共用最终阶段时钟，整周期时两种攻击必定同时发射。
        int age = (int)++Timer;
        if (!TONetworkEnvironment.NotClient)
            return;

        if (NPC.LifeRatio <= FinalSkullLifeRatio && age % Phase3Patterns.SkullInterval == 0)
            SkeletronCombat.FireSkull(NPC, Target, FinalSkullCount, MathHelper.TwoPi / FinalSkullCount,
                FinalSkullSpeed, SkullDamage, addNPCVelocity: false);

        if (NPC.LifeRatio <= FinalGuardianLifeRatio && age % FinalGuardianInterval == 0)
        {
            float spacing = MathHelper.TwoPi / FinalGuardianCount;
            Vector2 centerVelocity = NPC.GetVelocityTowards(Target, FinalGuardianSpeed);
            Projectile.NewProjectilesArc<SkeletronPhase3Guardian>(FinalGuardianCount, spacing, SourceAI, NPC.Center,
                centerVelocity, SkullDamage, 0f, action: p =>
                {
                    p.ai[0] = NPC.whoAmI + 1;
                    p.ai[2] = SkeletronPhase3Guardian.Final;
                    p.timeLeft = 120;
                });
        }
    }
}
