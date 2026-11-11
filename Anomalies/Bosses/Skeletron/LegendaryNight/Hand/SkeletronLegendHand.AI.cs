namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHand
{
    public override bool PreAI()
    {
        using BossNetUpdateScope netUpdate = new(NPC);

        //手部无敌，且不可追踪
        NPC.chaseable = false;
        NPC.dontTakeDamage = true;

        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) || !LegendaryNightHandler.IsLegendaryNightHead(master))
        {
            if (TONetworkEnvironment.NotClient)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
            return false;
        }

        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        NPC.timeLeft = Math.Max(NPC.timeLeft, 180);
        if (TryUpdateElementCarrier())
            return false;
        NPC.hide = head.HandsHidden;
        if (head.HandsHidden)
        {
            NPC.damage = 0;
            NPC.dontTakeDamage = true;
            NPC.velocity = master.Center - NPC.Center;
            AnomalyNPC.Skeletron_HandBloodDust = null;
            return false;
        }
        NPC.damage = head.Phase2 ? 0 : NPC.defDamage;
        NPC.spriteDirection = -(int)NPC.ai[0];
        NPC.target = master.target;
        int increasedLife = Math.Min(NPC.lifeMax - NPC.life, Math.Max(1, NPC.lifeMax / 60));
        if (increasedLife > 0)
        {
            NPC.life += increasedLife;
            NPC.HealEffect(increasedLife, true);
        }
        if (!NPC.TargetClosestIfInvalid(true, SkeletronLegendHead.DespawnDistance))
        {
            NPC.velocity *= 0.8f;
            return false;
        }
        switch (head)
        {
            case { StellarMechanicsActive: true }:
            case { Mechanic5Active: true } when head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic5Parameters.GetHandPrepareStartTime(HandSide):
                Mechanic6HandAI(head);
                break;
            case { Mechanic5Active: true }:
                Mechanic5HandAI(head);
                break;
            case { Mechanic2InBackground: true }:
                Mechanic2HandAI(head);
                break;
            default:
                AnomalyNPC.Z = 0f;
                NormalHandAI(head);
                if (CurrentNormalAction is not (NormalAction.Idle or NormalAction.Recover))
                {
                    float rotation = GetPositionRotation(NPC.Center, master.Center, HandSide);
                    NPC.rotation += MathHelper.WrapAngle(rotation - NPC.rotation) * 0.2f;
                }
                break;
        }
        return false;

        void Mechanic6HandAI(SkeletronLegendHead head)
        {
            int time = head.Phase2MusicElapsedTime;
            int side = HandSide;
            float radius = SkeletronLegendHead.Mechanic6Parameters.Radius(time);
            Vector2 sphereOffset = SkeletronLegendHead.Mechanic6Parameters.SphereOffset(time);
            Vector2 offset = sphereOffset + new Vector2(side * (radius + 65f), radius * 0.25f);
            float prepareProgress = 1f;
            if (time < SkeletronLegendHead.Mechanic6Timeline.StartMusicTime)
            {
                if (CurrentScriptedAction != ScriptedAction.PrepareSphere && TONetworkEnvironment.NotClient)
                {
                    StellarPreparationOffset = NPC.Center - master.Center;
                    ChangeScriptedAction(ScriptedAction.PrepareSphere);
                }
                if (CurrentScriptedAction != ScriptedAction.PrepareSphere)
                {
                    NPC.damage = 0;
                    NPC.velocity = Vector2.Zero;
                    return; // 等待服务器锁定的起点，避免读取上一轮挥击数据。
                }
                int prepareStart = SkeletronLegendHead.Mechanic5Parameters.GetHandPrepareStartTime(side);
                prepareProgress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(
                    (time - prepareStart) / (float)(SkeletronLegendHead.Mechanic6Timeline.HandPrepareEndMusicTime - prepareStart), 0f, 1f));
                offset = Vector2.Lerp(StellarPreparationOffset, offset, prepareProgress);
            }
            NPC.damage = 0;
            NPC.dontTakeDamage = true;
            AnomalyNPC.Z = 0f;
            // 收进球内而非普通待机位置，为原版向外伸出的两段骨节留出余量。
            Vector2 concealedOffset = (head.Phase2Arena?.Projectile.Center ?? master.Center + sphereOffset)
                - master.Center + new Vector2(side * 40f, -40f);
            float lower = SkeletronLegendHead.Mechanic6Parameters.HandLowerProgress(time);
            offset = Vector2.Lerp(offset, concealedOffset, lower);
            NPC.velocity = master.Center + offset - NPC.Center;
            const float Tilt = 0.12f;
            float growth = radius / SkeletronLegendHead.Mechanic6Parameters.SphereRadius;
            float chargeRotation = MathHelper.Pi + side * MathHelper.Lerp(-Tilt, Tilt, growth);
            float armRotation = GetPositionRotation(NPC.Center + NPC.velocity, master.Center, side);
            NPC.rotation = chargeRotation + MathHelper.WrapAngle(armRotation - chargeRotation) * (1f - prepareProgress * (1f - lower));
            NPC.Opacity = SkeletronLegendHead.Mechanic6Parameters.HandDepartureOpacity(time);
            if (NPC.Opacity <= 0f)
            {
                AnomalyNPC.Skeletron_HandBloodDust = null;
                NPC.hide = true;
            }
        }

        void Mechanic5HandAI(SkeletronLegendHead head)
        {
            int time = head.Phase2MusicElapsedTime;
            int summonAge = time - SkeletronLegendHead.Mechanic5Timeline.StartMusicTime;
            int side = HandSide;
            Vector2 idle = master.Center + GetIdleOffset(side);
            AnomalyNPC.Z = 0f;
            if (summonAge < SkeletronLegendHead.Mechanic5Parameters.HandReappearanceDuration)
            {
                if (TONetworkEnvironment.NotClient)
                    ChangeScriptedAction(ScriptedAction.Idle);
                NPC.damage = 0;
                float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((summonAge + 1f)
                    / SkeletronLegendHead.Mechanic5Parameters.HandReappearanceDuration, 0f, 1f));
                NPC.velocity = Vector2.Lerp(master.Center, idle, progress) - NPC.Center;
            }
            else
            {
                int attack = SkeletronLegendHead.Mechanic5Parameters.GetHandAttackIndex(time, side);
                int age = attack < 0 ? int.MinValue : time - SkeletronLegendHead.Mechanic5Parameters.GetHandStrikeTime(attack);
                if (attack < 0)
                {
                    if (TONetworkEnvironment.NotClient)
                        ChangeScriptedAction(ScriptedAction.Idle);
                    NPC.velocity = idle - NPC.Center;
                }
                else if (age < 0)
                {
                    if (TONetworkEnvironment.NotClient && Mechanic5LockedAttack != attack + 1)
                    {
                        Mechanic5WindupDirection = (idle - head.Target.Center).SafeNormalize(-Vector2.UnitY);
                        Mechanic5LockedAttack = attack + 1;
                        ChangeScriptedAction(ScriptedAction.Windup);
                        NPC.netUpdate = true;
                    }
                    float progress = MathHelper.SmoothStep(0f, 1f, (age + SkeletronLegendHead.Mechanic5Parameters.HandWindupDuration + 1)
                        / (float)SkeletronLegendHead.Mechanic5Parameters.HandWindupDuration);
                    NPC.velocity = Mechanic5LockedAttack == attack + 1 && CurrentScriptedAction == ScriptedAction.Windup
                        ? idle + Mechanic5WindupDirection * (SkeletronLegendHead.Mechanic5Parameters.HandWindupDistance * progress) - NPC.Center
                        : Vector2.Zero;
                }
                else
                {
                    if (TONetworkEnvironment.NotClient
                        && (Mechanic5LockedAttack != attack + 1 || CurrentScriptedAction == ScriptedAction.Windup))
                    {
                        LegendaryNightPhase2Arena arena = head.Phase2Arena;
                        Vector2 center = arena?.Projectile.Center ?? master.Center;
                        float radius = arena?.Radius ?? LegendaryNightPhase2Arena.MainCircleFullRadius * LegendaryNightPhase2Arena.OuterCircleRadiusRatio;
                        StrikeOrigin = NPC.Center;
                        StrikeDestination = SkeletronLegendHead.Mechanic5Parameters.GetHandStrikeDestination(
                            NPC.Center, head.Target.Center, center, radius);
                        Mechanic5LockedAttack = attack + 1;
                        ChangeScriptedAction(age < SkeletronLegendHead.Mechanic5Parameters.HandStrikeDuration
                            ? ScriptedAction.Strike : ScriptedAction.Return);
                        NPC.netUpdate = true;
                    }
                    if (Mechanic5LockedAttack != attack + 1
                        || CurrentScriptedAction is not (ScriptedAction.Strike or ScriptedAction.Return))
                        NPC.velocity = Vector2.Zero; // 客户端等待本次锁定起终点，不能读取上一轮数据。
                    else if (age < SkeletronLegendHead.Mechanic5Parameters.HandStrikeDuration)
                    {
                        if (TONetworkEnvironment.NotClient)
                            ChangeScriptedAction(ScriptedAction.Strike);
                        NPC.damage = SkeletronLegendHead.Phase2HandContactDamage;
                        float progress = (age + 1f) / SkeletronLegendHead.Mechanic5Parameters.HandStrikeDuration;
                        NPC.velocity = Vector2.Lerp(StrikeOrigin, StrikeDestination, progress) - NPC.Center;
                    }
                    else
                    {
                        if (TONetworkEnvironment.NotClient)
                            ChangeScriptedAction(ScriptedAction.Return);
                        float progress = MathHelper.SmoothStep(0f, 1f,
                            (age - SkeletronLegendHead.Mechanic5Parameters.HandStrikeDuration + 1f)
                                / SkeletronLegendHead.Mechanic5Parameters.HandReturnDuration);
                        NPC.velocity = Vector2.Lerp(StrikeDestination, idle, progress) - NPC.Center;
                    }
                }
            }
            NPC.rotation = CurrentScriptedAction == ScriptedAction.Idle
                ? GetRestingRotation(idle, master.Center, side, NPC.height)
                : GetPositionRotation(NPC.Center + NPC.velocity, master.Center, side);
            NPC.Opacity = SkeletronLegendHead.Mechanic6Parameters.HandDepartureOpacity(time);
            if (NPC.Opacity <= 0f)
            {
                AnomalyNPC.Skeletron_HandBloodDust = null;
                NPC.hide = true;
            }
        }

        // 从原版 NPC.AI 的 aiStyle == 12（type 36）复制，保留 0/3 待机、1/2 下拍、4/5 横扫。
        // 待机复用普通骷髅王手部；拍击保留传说之夜的主体时序和距离计时。
        void NormalHandAI(SkeletronLegendHead head)
        {
            bool canAttack = head.Phase2 && !head.Mechanic1Spinning;
            float windupAcceleration = MathHelper.Lerp(0.2f, 0.4f, 1f - master.LifeRatio) * 1.5f;
            int timer = canAttack ? head.Timer1 % AttackCycleDuration : 0;
            NPC.ai[3] = timer;
            if (!canAttack || timer >= StrikeTime + MaximumStrikeDuration)
            {
                if (CurrentNormalAction is not (NormalAction.Idle or NormalAction.Recover))
                    ChangeNormalAction(NormalAction.Recover);
            }
            else if (timer == WindupStart)
                ChangeNormalAction(head.Timer1 / AttackCycleDuration % 2 == 0 ? NormalAction.Raise : NormalAction.SweepWindup);

            switch (CurrentNormalAction)
            {
                case NormalAction.Idle:
                case NormalAction.Recover:
                    {
                        // 远距离复用普通手部运动，近距离替换为阻尼收敛，消除持续跨越待机点的抖动。
                        float lifeRatio = master.LifeRatio;
                        float velocityMultiplier = MathHelper.Lerp(0.6f, 1f, lifeRatio) * 0.75f;
                        float velocityIncrement = MathHelper.Lerp(0.2f, 0.4f, 1f - lifeRatio) * 1.5f;
                        float deceleration = (Main.getGoodWorld ? 0.78f : 0.82f) * 0.75f;
                        bool raised = head.CurrentBehavior == SkeletronLegendHead.Behavior.Spin || head.Mechanic1Spinning;
                        if (raised)
                        {
                            deceleration *= 0.75f;
                            velocityIncrement *= 1.5f;
                        }
                        Vector2 destination = master.Center + SkeletronHand.GetIdleOffset(HandSide, NPC.height, master.height, raised: raised);
                        SkeletronHand.UpdateIdleMovement(NPC, destination, velocityIncrement, velocityMultiplier, deceleration, master.velocity);
                        SkeletronHand.UpdateIdleRotation(NPC, master);
                        return;
                    }
                case NormalAction.Raise:
                    {
                        SkeletronHand.UpdateVerticalDrawbackMovement(NPC, windupAcceleration);

                        if (head.Timer1 % AttackCycleDuration == StrikeTime)
                        {
                            BeginStrike(NormalAction.Slam);
                        }
                        break;
                    }
                case NormalAction.Slam:
                    {
                        RecoverAfterStrike();
                        break;
                    }
                case NormalAction.SweepWindup:
                    {
                        SkeletronHand.UpdateHorizontalDrawbackMovement(NPC, windupAcceleration);

                        if (head.Timer1 % AttackCycleDuration == StrikeTime)
                        {
                            BeginStrike(NormalAction.Sweep);
                        }
                        break;
                    }
                case NormalAction.Sweep:
                    {
                        RecoverAfterStrike();
                        break;
                    }
            }
            void BeginStrike(NormalAction action)
            {
                if (!TONetworkEnvironment.NotClient)
                    return;
                StrikeOrigin = NPC.Center;
                StrikeDestination = Target.Center;
                NPC.velocity = (StrikeDestination - StrikeOrigin).SafeNormalize(Vector2.UnitY) * StrikeSpeed;
                ChangeNormalAction(action);
            }

            void RecoverAfterStrike()
            {
                // 锁定出手时距离，避免玩家后退使收手时间不断延长。
                float duration = MathHelper.Clamp(Vector2.Distance(StrikeOrigin, StrikeDestination) / StrikeSpeed
                    + StrikeFollowThroughDuration, MinimumStrikeDuration, MaximumStrikeDuration);
                if (TONetworkEnvironment.NotClient && timer - StrikeTime >= duration)
                    ChangeNormalAction(NormalAction.Recover);
            }

            // 仅实际下拍和横扫造成接触伤害，蓄势与回收阶段保持无伤害。
            if (canAttack && CurrentNormalAction is NormalAction.Slam or NormalAction.Sweep)
                NPC.damage = SkeletronLegendHead.Phase2HandContactDamage;
        }

        void ChangeNormalAction(NormalAction action)
        {
            if (CurrentNormalAction == action)
                return;
            CurrentNormalAction = action;
            NPC.netUpdate = true;
        }

        void ChangeScriptedAction(ScriptedAction action)
        {
            if (CurrentScriptedAction == action)
                return;
            CurrentScriptedAction = action;
            NPC.netUpdate = true;
        }

        void Mechanic2HandAI(SkeletronLegendHead head)
        {
            int age = head.Mechanic2Age;
            int side = HandSide;
            int strike = SkeletronLegendHead.Mechanic2Parameters.GetStrikeIndex(age);
            int relativeAge = age - SkeletronLegendHead.Mechanic2Parameters.GetStrikeTime(strike);
            bool selected = head.Mechanic2HandPattern != 0 && side == head.Mechanic2StrikeSide(strike);
            Vector2 center = head.Phase2Arena?.Projectile.Center ?? Master.Center;
            // 非拍击手保持下垂并留出横向骨节空间，从玩家侧旁掠过。
            Vector2 restingCenter = Master.Center + GetIdleOffset(side);
            if (!selected && head.Mechanic2HandPattern != 0 && age < SkeletronLegendHead.Mechanic2Parameters.ReturnStartAge)
            {
                restingCenter = Master.Center + SkeletronLegendHead.Mechanic2Parameters.GetPassingHandOffset(side, GetIdleOffset(side), age);
            }
            Vector2 destination = restingCenter;
            float z = head.AnomalyNPC.Z;
            if (selected && relativeAge >= -SkeletronLegendHead.Mechanic2Parameters.HandRaiseLeadTime && age < SkeletronLegendHead.Mechanic2Parameters.ReturnStartAge)
            {
                float raise = SkeletronLegendHead.Mechanic2Parameters.HandRaiseProgress(relativeAge);
                float slam = SkeletronLegendHead.Mechanic2Parameters.HandSlamProgress(relativeAge);
                float exit = SkeletronLegendHead.Mechanic2Parameters.HandExitProgress(relativeAge);
                Vector2 raised = Master.Center + new Vector2(side * 390f, -300f);
                // 触击时的透视会放大侧向距离，收近世界坐标以让手掌留在画面内。
                Vector2 impact = center + new Vector2(side * 350f, 130f);
                Vector2 strikePosition = Vector2.Lerp(Vector2.Lerp(restingCenter, raised, raise), impact, slam)
                    + new Vector2(side * 120f, 90f) * exit;
                // 手掌穿过观察者后先在屏外并回头部侧边，再随本体重新入画。
                float rejoin = SkeletronLegendHead.Mechanic2Parameters.HandRejoinProgress(relativeAge);
                destination = Vector2.Lerp(strikePosition, restingCenter, rejoin);
                z = SkeletronLegendHead.Mechanic2Parameters.GetStrikingHandDepth(relativeAge, z, slam, exit, rejoin);
            }
            // 固定入场起点和切线，不对逐帧变化的当前位置反复插值，避免把惯性偏移快速拉回。
            float entry = SkeletronLegendHead.Mechanic2Parameters.HandEntryProgress(age);
            if (age < SkeletronLegendHead.Mechanic2Parameters.HandEntryDuration)
            {
                if (CurrentScriptedAction != ScriptedAction.Mechanic2Entry && TONetworkEnvironment.NotClient)
                {
                    Mechanic2EntryOffset = NPC.Center - Master.Center;
                    Mechanic2EntryVelocity = NPC.velocity - Master.velocity;
                    ChangeScriptedAction(ScriptedAction.Mechanic2Entry);
                }
                if (CurrentScriptedAction != ScriptedAction.Mechanic2Entry)
                    return; // 等待服务器锁定入场数据，不能读取常态攻击留下的槽位。
                NPC.Center = Master.Center + SkeletronLegendHead.Mechanic2Parameters.GetHandEntryOffset(
                    Mechanic2EntryOffset, Mechanic2EntryVelocity, destination - Master.Center, age);
            }
            else
            {
                if (TONetworkEnvironment.NotClient)
                    ChangeScriptedAction(ScriptedAction.Idle);
                NPC.Center = destination;
            }
            NPC.velocity = Vector2.Zero;
            AnomalyNPC.Z = MathHelper.Lerp(AnomalyNPC.Z, z, entry);
            if (!selected && head.AnomalyNPC.Depth_DrawAnchor is (Vector2 LocalOffset, Vector2 Target, float Weight) anchor)
            {
                // 仅复制 Z 会漏掉头部为贴近玩家头顶而产生的屏幕平移。
                // 以同一世界锚点投影，使闲置手和头部获得完全相同的视觉平移。
                Vector2 localOffset = Master.Center + Master.netOffset + anchor.LocalOffset - NPC.Center - NPC.netOffset;
                AnomalyNPC.Depth_DrawAnchor = (localOffset, anchor.Target, anchor.Weight);
            }
            // 原版从手掌连接点朝 (±200,130) 绘制第一段骨节；手掌朝向沿同一条轴自然下垂。
            // 始终以闲置位置计算基准，避免抬手时基准朝向随攻击位置变化。
            // 收拢只改变非拍击手的位置，朝向始终沿用标准下垂姿态。
            float restingRotation = GetRestingRotation(Master.Center + GetIdleOffset(side), Master.Center, side, NPC.height);
            float destinationRotation = SkeletronLegendHead.Mechanic2Parameters.GetHandRotation(relativeAge, side, selected, restingRotation);
            NPC.rotation += MathHelper.WrapAngle(destinationRotation - NPC.rotation) * entry;
        }

        bool TryUpdateElementCarrier()
        {
            if (DetachedFromArms(NPC) && head.Phase2Arena is LegendaryNightPhase2Arena arena)
            {
                if (head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic7Timeline.ElementFadeOutStartMusicTime)
                {
                    NPC.Opacity = 0f;
                    NPC.damage = 0;
                    if (TONetworkEnvironment.NotClient)
                    {
                        NPC.active = false;
                        NPC.netUpdate = true;
                    }
                    return true;
                }
                int index = NPC.ai[0] < 0f ? 0 : 1;
                (Vector2 center, _, _) = arena.GetMechanic7Circle(index);
                NPC.hide = false;
                NPC.damage = 0;
                NPC.velocity = Vector2.Zero;
                NPC.Center = center;
                NPC.spriteDirection = -(int)NPC.ai[0];
                NPC.rotation = (arena.Projectile.Center - center).ToRotation() + MathHelper.PiOver2;
                NPC.Opacity = SkeletronLegendHead.Mechanic7Parameters.ElementOpacity(head.Phase2MusicElapsedTime, index);
                AnomalyNPC.Z = 0f;
                AnomalyNPC.Skeletron_HandBloodDust = null;
                return true;
            }
            return false;
        }
    }
}
