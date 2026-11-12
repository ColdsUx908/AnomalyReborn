namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private void PhaseChange_2To3AI()
    {
        NPC.damage = 0;
        NPC.dontTakeDamage = true;
        NPC.chaseable = false;
        NPC.rotation = NPC.rotation.AngleTowards(0f, MathHelper.Pi / Phase3StillDuration);

        switch (CurrentLocalPhase)
        {
            case 0: // 静止，随后开始向玩家上方移动。
                NPC.velocity = Vector2.Zero;
                if (++Timer >= Phase3StillDuration && TONetworkEnvironment.NotClient)
                {
                    CurrentLocalPhase = 1;
                    Timer = 0f;
                }
                break;
            case 1: // 追踪玩家上方，刹停前由服务器锁定停靠点。
                Vector2 destination = Target.Center - Vector2.UnitY * Phase3ApproachHeight;
                Vector2 desiredVelocity = (destination - NPC.Center) * 0.2f;
                float speed = desiredVelocity.Length();
                if (speed > Phase3ApproachSpeed)
                    desiredVelocity *= Phase3ApproachSpeed / speed;
                NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.3f);
                if (++Timer >= Phase3ApproachDuration && TONetworkEnvironment.NotClient)
                {
                    SetBrakeVector(0, NPC.Center + NPC.velocity);
                    SetBrakeVector(1, destination);
                    SetBrakeVector(2, NPC.velocity);
                    CurrentLocalPhase = 2;
                    Timer = 0f;
                }
                break;
            case 2: // 保留进入刹停的速度，抵达停靠点时切线归零。
                if (Timer < Phase3BrakeDuration)
                {
                    Vector2 position = Vector2.Hermite(
                        AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>(),
                        AnomalyNPC.AnomalyAI64[2].GetValue<Vector2>() * Phase3BrakeDuration,
                        AnomalyNPC.AnomalyAI64[1].GetValue<Vector2>(), Vector2.Zero, ++Timer / Phase3BrakeDuration);
                    NPC.velocity = position - NPC.Center;
                    break;
                }

                NPC.velocity = Vector2.Zero;
                if (!TONetworkEnvironment.NotClient)
                    break;

                Projectile.NewProjectileAction<SkeletronPhase3Arena>(SourceAI, Target.Center,
                    Vector2.Zero, SkeletronBoneArena.BoundaryDamage, 0f, action: arena =>
                    {
                        SkeletronPhase3Arena field = arena.GetModProjectile<SkeletronPhase3Arena>();
                        field.Master = NPC;
                        field.IsActivated = true;
                        CurrentLocalPhase = 3;
                    });
                if (CurrentLocalPhase == 3)
                    goto case 3;
                break;
            case 3: // 竞技场已生成，释放冲击波后才开始回血。
                NPC.velocity = Vector2.Zero;
                if (TONetworkEnvironment.NotClient)
                {
                    Projectile.NewProjectileAction<BoneShockwave>(SourceAI, NPC.Center, Vector2.Zero, 0, 0f, action: p =>
                    {
                        p.ai[0] = BoneShockwave.PhaseChangeVariant;
                        p.timeLeft = p.GetModProjectile<BoneShockwave>().LifeTime;
                        p.scale = 0f;
                        SetBrakeVector(0, NPC.Center);
                        CurrentLocalPhase = 4;
                        Timer = 0f;
                    });
                }
                break;
            case 4: // 冲击波后稍作停留，缓慢移至竞技场中心，同时回血。
                NPC.velocity = Vector2.Zero;
                Timer++;
                if (Timer > Phase3CenterMoveDelay && FindActivePhase3Arena() is SkeletronPhase3Arena arena)
                {
                    float progress = MathHelper.Clamp((Timer - Phase3CenterMoveDelay) / Phase3CenterMoveDuration, 0f, 1f);
                    Vector2 position = Vector2.SmoothStep(AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>(),
                        arena.ArenaPolygon.Center, progress);
                    NPC.velocity = position - NPC.Center;
                }
                if (!TONetworkEnvironment.NotClient)
                    break;

                HealDuringTransition();

                if (Timer >= Phase3CenterMoveDelay + Phase3CenterMoveDuration)
                {
                    CurrentPhase = Phase.Phase3;
                    CurrentBehavior = Behavior.Phase3_GuardianRain;
                    Phase3Cycle = 0;
                    CurrentLocalPhase = 0;
                    Timer = 0f;
                }
                break;
        }

        void HealDuringTransition()
        {
            int targetLife = (int)(NPC.lifeMax * Phase3HealLifeRatio);
            float progress = MathHelper.Clamp(Timer / Phase3HealDuration, 0f, 1f);
            int newLife = progress >= 1f ? targetLife : (int)MathHelper.Lerp(
                Phase3HealStartLife, targetLife, TOMathUtils.Interpolation.LogarithmicEaseOut(progress));
            int increasedLife = newLife - NPC.life;
            if (increasedLife <= 0)
                return;

            NPC.life = newLife;
            NPC.HealEffect(increasedLife, true);
            NPC.netUpdate = true;
        }

        void SetBrakeVector(int slot, Vector2 value)
        {
            AnomalyNPC.AnomalyAI64[slot].SetValue(value);
            AnomalyNPC.AIChanged64[slot] = true;
        }
    }
}
