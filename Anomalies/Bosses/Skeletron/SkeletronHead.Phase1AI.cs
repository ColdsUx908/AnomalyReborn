namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private void Phase1AI(bool handsDead)
    {
        switch (CurrentBehavior)
        {
            case Behavior.Hover:
                Hover();
                break;
            case Behavior.Spin:
                Spin();
                break;
        }

        void Hover()
        {
            if (TONetworkEnvironment.NotClient && handsDead && CurrentLocalPhase != 2)
            {
                CurrentLocalPhase = 2;
                Timer = 0f;
                ChargeTelegraphTimer = 0f;
            }

            if (CurrentLocalPhase != 2)
                AttackTimer++;
            switch (CurrentLocalPhase)
            {
                case 0 when AttackTimer <= 0f: //悬停静默期，等待攻击计时恢复
                    break;
                case 0: //静默期结束，当帧开始普通射击
                    CurrentLocalPhase = 1;
                    goto case 1;
                case 1: //普通悬停射击并累积旋转蓄力
                    if (AttackTimer <= WithHandsDuration - QuietDuration)
                        FireHoverSkulls(SkullInterval, handsDead);
                    Timer += ChargeGateValue / WithHandsDuration;
                    if (TONetworkEnvironment.NotClient && Timer >= ChargeGateValue)
                    {
                        BeginSpin(Behavior.Spin, 0);
                        return;
                    }
                    break;
                case 2: //悬停期间清空手部，重新静默90帧后直接开始二阶段常态攻击
                    Timer++;
                    if (TONetworkEnvironment.NotClient && Timer >= HandsClearedHoverQuietDuration)
                    {
                        InitializePhase2Hover();
                        CurrentLocalPhase = 0;
                        Timer = AttackTimer = ChargeReadiness = ChargeTelegraphTimer = 0f;
                        NPC.netUpdate = true;
                    }
                    else
                        MoveWhileHovering();
                    return;
            }

            ChargeTelegraphTimer = Math.Min(Timer, ChargeGateValue);
            MoveWhileHovering();
        }

        void Spin()
        {
            BeginSpinFrame(SoundID.ForceRoar);
            FadeChargeTelegraph();

            switch (CurrentLocalPhase)
            {
                case 0 when TONetworkEnvironment.NotClient && handsDead: //旋转中双手被击败，切换到无手旋转计时
                    float remaining = Math.Clamp(WithHandsDuration - Timer, HandsClearedMinSpinRemaining, HandsClearedSpinDuration);
                    Timer = HandsClearedSpinDuration - remaining;
                    CurrentLocalPhase = 1;
                    goto case 1;
                case 0: //双手仍存活，沿用有手旋转时长
                    TrackTarget(WithHandsDuration);
                    break;
                case 1: //无手旋转，追踪玩家
                    TrackTarget(HandsClearedSpinDuration);
                    break;
            }

            void TrackTarget(float duration)
            {
                if (TONetworkEnvironment.NotClient && Timer >= duration)
                    FinishSpin(handsDead);
                else
                    SkeletronKinematics.SpinMovement(NPC, Target, SpinSpeed, SkeletronKinematics.GetSpinMoveSpeed(NPC.Distance(Target.Center), false) * 0.8f);
            }
        }
    }
}
