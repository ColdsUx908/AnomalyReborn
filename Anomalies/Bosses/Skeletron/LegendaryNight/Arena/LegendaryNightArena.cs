namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 一阶段十六边形竞技场。
/// </summary>
/// <remarks>
/// <para>ai[0]：主体 NPC 索引 + 1；0 为空。ai[1..2]：未使用。</para>
/// <para>localAI：未使用；框架 Timer1..5：未使用。</para>
/// <para>ExtraAI：基类 Radius、OutofArenaTimer，随后 IsActivated、Intensity，顺序保持不变。</para>
/// <para>客户端状态：_PreviousActivation 为同步变化检测；_VisualPolygon 与初始化标记保存淡出几何，不同步。</para>
/// </remarks>
public sealed class LegendaryNightArena : SkeletronBoneArena
{
    private bool IsMasterValid => LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], oneBased: true) && Master.life > 0;

    public SkeletronLegendHead MasterBehavior => LegendaryNightHandler.TryGetLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true) && head.NPC.life > 0 ? head : null;

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override Player Target => MasterBehavior?.Target;

    public override void AI()
    {
        if (!IsMasterValid)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        if (MasterBehavior.Phase2ArenaRequested)
        {
            if (TONetworkEnvironment.NotClient)
            {
                bool created = false;
                Projectile.NewProjectileAction<LegendaryNightPhase2Arena>(SourceAI, Master.Center,
                    Vector2.Zero, SkeletronLegendHead.Phase2ArenaDamage, 0f, action: p =>
                    {
                        LegendaryNightPhase2Arena arena = p.GetModProjectile<LegendaryNightPhase2Arena>();
                        arena.Master = Master;
                        arena.AppearanceStartMusicTime = MasterBehavior.Phase2ArenaStartMusicTime;
                        MasterBehavior.ArenaProjectile = p;
                        created = true;
                    });
                if (created)
                    Projectile.Kill();
            }
            return;
        }

        Projectile.Center = Master.Center;
        UpdateBoundary();
    }
}
