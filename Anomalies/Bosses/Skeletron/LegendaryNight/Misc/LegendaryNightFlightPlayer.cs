namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightFlightPlayer : AnomalyPlayerBehavior
{
    public override void UpdateDead()
    {
        AnomalyPlayer.StarGodBlessingActive = false;
        AnomalyPlayer.StarGodBlessingOpacity = 0f;
    }

    public override void PostUpdateEquips()
    {
        if (!Player.active || Player.dead)
            return;

        // 从同步的 Boss 阶段和计时推导效果，支持复活、半途加入和多个 Boss 同时存在。
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            if (npc.type != NPCID.SkeletronHead || npc.Anomaly.CustomAIType != 1
                || !SkeletronLegendHead.GetInstance(npc).StarGodBlessingActive)
                continue;

            AnomalyPlayer.StarGodBlessingActive = true;
            AnomalyPlayer.StarGodBlessingOpacity = Math.Max(AnomalyPlayer.StarGodBlessingOpacity,
                SkeletronLegendHead.GetInstance(npc).StarGodBlessingOpacity);
        }

        if (!AnomalyPlayer.StarGodBlessingActive)
            return;

        // 功能翅膀优先：已有翅膀时只补充无限飞行，不覆盖其飞行参数。
        if (Player.wingsLogic <= 0)
        {
            AnomalyPlayer.LegendaryNightVirtualBoneWings ??= new Item(ItemID.BoneWings);
            int visibleWings = Player.wings;
            Player.ApplyEquipFunctional(AnomalyPlayer.LegendaryNightVirtualBoneWings, hideVisual: true);
            if (visibleWings > 0)
                Player.wings = visibleWings;
        }
        Player.noFallDmg = true;
    }

    public override void PostUpdateMiscEffects()
    {
        // 在装备及飞行时间加成计算后补满，不修改玩家的实际装备或持久数据。
        if (!AnomalyPlayer.StarGodBlessingActive)
            return;

        Player.wingTime = Player.wingTimeMax;
        if (Main.dedServ)
            return;

        // 淡金色呼吸光与上浮星尘持续提示祝福，避免遮挡玩家和战斗弹幕。
        float glow = TOMathUtils.TimeWrappingFunction.GetTimeSin(0.3f, 3f) + 0.7f;
        Lighting.AddLight(Player.Center, LegendaryNightHandler.StarColor.ToVector3() * (glow * AnomalyPlayer.StarGodBlessingOpacity));
        if (Main.rand.NextBool(3))
        {
            Vector2 position = Player.Center + new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-8f, 20f));
            Vector2 velocity = Player.velocity * 0.15f + new Vector2(0f, -0.8f);
            Dust starDust = Dust.NewDustPerfect(position, DustID.YellowStarDust, velocity, 100, LegendaryNightHandler.StarColor, 0.65f);
            starDust.noGravity = true;
            starDust.noLight = true;
        }
    }
}
