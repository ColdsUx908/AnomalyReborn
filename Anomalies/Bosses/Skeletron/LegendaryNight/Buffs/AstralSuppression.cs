namespace Anomalies.Bosses.Skeletron.LegendaryNight.Buffs;

public sealed class AstralSuppression : ModBuff
{
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
            modifiers.FinalDamage *= 0.5f;
    }
}
