namespace CalamityAnomalies.Buffs;

public sealed class DimensionalRend : CalamityModDOT
{
    public override bool HasBuff(Player player) => player.Anomaly.Debuff_DimensionalRend;

    public override bool HasBuff(NPC npc) => npc.Anomaly.Debuff_DimensionalRend;

    public override float GetDamageCalamity(Player player) => GetVelocityInterpolation(player) * 10f;

    public override float GetDamage(NPC npc) => GetVelocityInterpolation(npc) * 1000f;

    public override int GetDamageValue(NPC npc) => (int)(GetVelocityInterpolation(npc) * 400f);

    public override void Update(Player player, ref int buffIndex) => player.Anomaly.Debuff_DimensionalRend = true;

    public override void Update(NPC npc, ref int buffIndex) => npc.Anomaly.Debuff_DimensionalRend = true;

    /// <summary>
    /// 获取维度撕裂二次曲线插值。
    /// <br/>在实体速度为25（127mph）时达到最大值1。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetVelocityInterpolation(Entity entity) => TOMathUtils.Interpolation.QuadraticEaseOut(entity.velocity.Length() / 25f);
}

public sealed class DimensionalRend_Player : CAPlayerBehavior
{
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (AnomalyPlayer.Debuff_DimensionalRend)
            modifiers.FinalDamage *= DimensionalRend.GetVelocityInterpolation(Player) * 0.15f + 1f;
    }
}

public sealed class DimensionalRend_GlobalNPC : CAGlobalNPCBehavior
{
    public override void ResetEffects(NPC npc) => npc.Anomaly.Debuff_DimensionalRend = false;

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (npc.Anomaly.Debuff_DimensionalRend)
            modifiers.FinalDamage *= DimensionalRend.GetVelocityInterpolation(npc) * 0.15f + 1f;
    }
}