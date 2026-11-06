namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 星神压制减益。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用 ai、localAI 或框架 Timer；持续时间由原版玩家 Buff 槽管理，不另存同步状态。</para>
/// </remarks>
public sealed class AstralSuppression : ModBuff
{
    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Buffs/AstralSuppression";

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
    {
        AstralBuffDrawing.DrawGlow(spriteBatch, ref drawParams, new Color(185, 95, 255));
        return true;
    }
}

public sealed class AstralSuppressionPlayer : AnomalyPlayerBehavior
{
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Player.HasBuff<AstralSuppression>())
            modifiers.FinalDamage *= SkeletronLegendHead.AstralSuppressionDamageMultiplier;
    }
}