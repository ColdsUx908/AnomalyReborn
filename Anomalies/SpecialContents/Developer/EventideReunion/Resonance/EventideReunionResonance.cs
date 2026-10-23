namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunionResonancePlayer : AnomalyPlayerBehavior
{
    // 与现有技能冷却一致，由持有者在成功生成技能时登记，服务器转发快照。
    public static int Release(Player player, EventideReunionAspect aspect, int baseCooldown)
    {
        AnomalyPlayer state = player.Anomaly;
        float previousMultiplier = EventideReunionResonanceRules.DamageMultiplier(state.EventideReunion_ResonanceMask);
        state.EventideReunion_ResonanceMask = EventideReunionResonanceRules.Release(
            state.EventideReunion_ResonanceMask, aspect, max => Main.rand.Next(max), out bool repeated);
        if (!repeated)
            state.EventideReunion_ResonanceTime = EventideReunionResonanceRules.Duration;
        else if (state.EventideReunion_ResonanceMask == 0)
            state.EventideReunion_ResonanceTime = 0;
        player.GetDamage(DamageClass.Melee) *= EventideReunionResonanceRules.DamageMultiplier(state.EventideReunion_ResonanceMask) / previousMultiplier;
        Sync(player);
        return EventideReunionResonanceRules.Cooldown(baseCooldown, state.EventideReunion_ResonanceMask, repeated);
    }

    public override void PostUpdateEquips() => Player.GetDamage(DamageClass.Melee) *=
        EventideReunionResonanceRules.DamageMultiplier(AnomalyPlayer.EventideReunion_ResonanceMask);

    public override void PostUpdate()
    {
        EventideReunionResonanceRules.Tick(ref AnomalyPlayer.EventideReunion_ResonanceMask, ref AnomalyPlayer.EventideReunion_ResonanceTime);
        if (AnomalyPlayer.EventideReunion_ResonanceTime > 0)
            Player.AddBuff(ModContent.BuffType<EventideReunionResonance>(), AnomalyPlayer.EventideReunion_ResonanceTime, quiet: true);
        else
            Player.ClearBuff(ModContent.BuffType<EventideReunionResonance>());
    }

    public override void UpdateDead()
    {
        AnomalyPlayer.EventideReunion_ResonanceMask = 0;
        AnomalyPlayer.EventideReunion_ResonanceTime = 0;
    }

    public static void Cancel(Player player)
    {
        AnomalyPlayer state = player.Anomaly;
        float multiplier = EventideReunionResonanceRules.DamageMultiplier(state.EventideReunion_ResonanceMask);
        state.EventideReunion_ResonanceMask = 0;
        state.EventideReunion_ResonanceTime = 0;
        player.GetDamage(DamageClass.Melee) /= multiplier;
        Sync(player);
    }

    internal static void Sync(Player player, int toWho = -1, int ignore = -1)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        ModPacket packet = AnomalySynchronization.GetCAPacket();
        packet.Write(AnomalySynchronization.ID.EventideResonance);
        packet.Write((byte)player.whoAmI);
        packet.Write(player.Anomaly.EventideReunion_ResonanceMask);
        packet.Write((ushort)player.Anomaly.EventideReunion_ResonanceTime);
        packet.Send(toWho, ignore);
    }

    internal static void Receive(BinaryReader reader, int sender)
    {
        int index = reader.ReadByte();
        byte mask = reader.ReadByte();
        int time = reader.ReadUInt16();
        if ((uint)index >= Main.maxPlayers || mask > EventideReunionResonanceRules.AllAspects
            || time > EventideReunionResonanceRules.Duration || (mask == 0) != (time == 0)
            || (Main.netMode == NetmodeID.Server && index != sender))
            return;
        Player player = Main.player[index];
        player.Anomaly.EventideReunion_ResonanceMask = mask;
        player.Anomaly.EventideReunion_ResonanceTime = time;
        if (Main.netMode == NetmodeID.Server)
            Sync(player, ignore: sender);
    }
}

public sealed class EventideReunionResonance : ModBuff
{
    public override string Texture => EventideReunionAssets.AssetPath + "Resonance/SamsaraResonance";
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;

    public override void SetStaticDefaults() => Main.buffNoSave[Type] = true;
    public override bool RightClick(int buffIndex)
    {
        EventideReunionResonancePlayer.Cancel(Main.LocalPlayer);
        return true;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        int count = EventideReunionResonanceRules.Count(Main.LocalPlayer.Anomaly.EventideReunion_ResonanceMask);
        buffName += $" ({count}/6)";
    }
}
