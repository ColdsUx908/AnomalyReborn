namespace Anomalies.Bosses.Skeletron;

public sealed class SkeletronPhase3Arena : SkeletronBoneArena
{
    // 仅客户端视觉快照；转阶段后冻结预警图案，再平滑退去，不加入 ExtraAI。
    internal SkeletronHead.Behavior WarningBehavior { get; private set; }
    internal float WarningAge { get; private set; }
    internal int WarningDirection { get; private set; }
    internal float WarningStartAngle { get; private set; }
    internal float WarningOpacity { get; private set; }
    private int _WarningFadeAge;

    internal void UpdateWarningVisuals(SkeletronHead head)
    {
        if (head.CurrentPhase == SkeletronHead.Phase.Phase3)
        {
            WarningBehavior = head.CurrentBehavior;
            WarningAge = Math.Max(0f, head.Timer - 1f);
            WarningDirection = head.Phase3PatternDirection;
            WarningStartAngle = head.Phase3PatternStartAngle;
            WarningOpacity = 1f;
            _WarningFadeAge = 0;
        }
        else
            WarningOpacity = Math.Max(0f, 1f - ++_WarningFadeAge / 30f);
    }

    public override string LocalizationCategory => "Bosses.Skeletron";

    private SkeletronHead MasterBehavior => Master is NPC { active: true, life: > 0, type: NPCID.SkeletronHead } head
        && SkeletronHead.GetInstance(head) is SkeletronHead { ShouldProcess: true } behavior
        && behavior.CurrentPhase is SkeletronHead.Phase.PhaseChange_2To3 or SkeletronHead.Phase.Phase3
            or SkeletronHead.Phase.ToFinal or SkeletronHead.Phase.Final
        && behavior.CurrentBehavior != SkeletronHead.Behavior.Despawn ? behavior : null;

    public override Player Target => MasterBehavior?.Target;

    public override void AI()
    {
        if (MasterBehavior is null)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }

        UpdateBoundary();
        if (!Main.dedServ)
            UpdateWarningVisuals(MasterBehavior);
    }

}
