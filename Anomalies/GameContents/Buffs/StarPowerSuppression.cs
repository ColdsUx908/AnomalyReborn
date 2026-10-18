namespace Anomalies.GameContents.Buffs;

public sealed class StarPowerSuppression : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_156";

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}

public sealed class StarPowerSuppressionPlayer : AnomalyPlayerBehavior
{
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Player.HasBuff<StarPowerSuppression>())
            modifiers.FinalDamage *= 0.5f;
    }
}
