namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    private void Phase2AI()
    {
        #region 公共时钟与机制推进

        if (Phase2ElapsedTime < Phase2MinimumDuration)
        {
            Phase2ElapsedTime++;
            if (TONetworkEnvironment.NotClient && Phase2ElapsedTime % 60 == 0)
                NPC.netUpdate = true;
        }

        if (Phase2MusicElapsedTime == Mechanic1Timeline.Phase2ArenaSoundMusicTime)
            SoundEngine.PlaySound(LegendaryNightPhase2Arena.SummonSound);

        AdvanceMechanics();

        if (Phase2ArenaRequested && !ArenaProjectileAlive && !Phase2ArenaProjectileAlive)
            EnsureArenaForPhase2();

        LegendStarburst.EnsureAttack(this);

        Timer1++;

        #endregion

        #region 当前机制分派

        switch (CurrentPhase)
        {
            case Phase.Phase2_1:
                Mechanic1AI();
                break;
            case Phase.Phase2_2:
                Mechanic2AI();
                break;
            case Phase.Phase2_3:
                Mechanic3AI();
                break;
            case Phase.Phase2_4:
                Mechanic4AI();
                break;
            case Phase.Phase2_5:
                Mechanic5AI();
                break;
            case Phase.Phase2_6:
                Mechanic6AI();
                break;
            case Phase.Phase2_7:
                Mechanic7AI();
                if (Phase2Arena is LegendaryNightPhase2Arena elementArena)
                    LegendaryNightElementBarrage.Update(this, elementArena);
                break;
            default:
                // 保留历史阶段值或未知状态的待机回退。
                NormalMovement();
                break;
        }

        #endregion

        #region 跨机制推进

        void AdvanceMechanics()
        {
            // 保留原有预测边界：前两次切换允许客户端跟随音乐，其余由服务器推进。
            // 机制采用半开区间 [StartMusicTime, EndMusicTime)，相邻节点首尾相接。
            while (true)
            {
                (int endMusicTime, Phase nextPhase) = CurrentPhase switch
                {
                    Phase.Phase2_1 => (Mechanic1Timeline.EndMusicTime, Phase.Phase2_2),
                    Phase.Phase2_2 => (Mechanic2Timeline.EndMusicTime, Phase.Phase2_3),
                    Phase.Phase2_3 => (Mechanic3Timeline.EndMusicTime, Phase.Phase2_4),
                    Phase.Phase2_4 => (Mechanic4Timeline.EndMusicTime, Phase.Phase2_5),
                    Phase.Phase2_5 => (Mechanic5Timeline.EndMusicTime, Phase.Phase2_6),
                    Phase.Phase2_6 => (Mechanic6Timeline.EndMusicTime, Phase.Phase2_7),
                    _ => (int.MaxValue, CurrentPhase)
                };
                if (nextPhase == CurrentPhase || Phase2MusicElapsedTime < endMusicTime)
                    return;
                if (!TONetworkEnvironment.NotClient && CurrentPhase is not (Phase.Phase2_1 or Phase.Phase2_2))
                    return;

                ChangePhase2State(nextPhase);
            }
        }

        #endregion
    }

    #region 机制进出与局部步骤切换

    private void ChangePhase2State(Phase phase, int localPhase = 0)
    {
        if (CurrentPhase != phase)
        {
            ExitPhase2Mechanic();
            CurrentPhase = phase;
            EnterPhase2Mechanic();
        }
        ChangePhase2LocalPhase(localPhase);
    }

    // 同一机制的局部步骤必须保留锁定数据；只有机制变化才释放复用槽。
    private void ExitPhase2Mechanic() => ResetPhase2SharedState();

    private void EnterPhase2Mechanic()
    {
        switch (CurrentPhase)
        {
            case Phase.Phase2_1:
                Phase2ElapsedTime = 0;
                break;
            case Phase.Phase2_2:
                Mechanic2StartMusicTime = Mechanic2Timeline.StartMusicTime;
                Mechanic2HandPattern = 0;
                NPC.velocity = Vector2.Zero;
                break;
            case Phase.Phase2_3:
                AnomalyNPC.Z = 0f;
                break;
        }
    }

    private void ChangePhase2LocalPhase(int localPhase)
    {
        CurrentLocalPhase = localPhase;
        Timer1 = 0;
        NPC.netUpdate = true;
    }

    #endregion

    #region 共用运动与场地恢复

    private void NormalMovement() => SkeletronHandler.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);

    private void HideHands() => SkeletronLegendHand.HideOwnedHands(NPC);

    private void EnsureArenaForPhase2()
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        // 正常由现存一阶段竞技场交接；引用缺失时先补回交接方。
        Projectile.NewProjectileAction<LegendaryNightArena>(SourceAI, Target.Center, Vector2.Zero, ArenaDamage, 0f,
            action: p =>
            {
                p.GetModProjectile<LegendaryNightArena>().Master = NPC;
                ArenaProjectile = p;
            });
    }

    #endregion

}