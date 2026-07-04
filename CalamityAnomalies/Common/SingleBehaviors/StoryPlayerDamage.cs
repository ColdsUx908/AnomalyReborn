namespace CalamityAnomalies.Common.SingleBehaviors;

public sealed class StoryPlayerDamage : CAPlayerBehavior
{
    public override bool ShouldProcess => CASharedData.Anomaly && Story;

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        modifiers.SourceDamage *= 1.5f; //玩家受到的伤害增加50%
        modifiers.SourceDamage.Flat += 50f; //玩家受到的最终伤害增加50点
    }
}
