namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 标记使用独立状态而非可被免疫/清除的 Buff；服务器拥有记录和结算。
public sealed class EventideReunionSamsaraMarkBehavior : AnomalyGlobalNPCBehavior
{
    internal static NPC Root(NPC npc)
    {
        if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
            return Main.npc[npc.realLife];
        return npc;
    }

    internal static bool Apply(NPC npc, int owner)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !npc.active || npc.life <= 0
            || !npc.Anomaly.EventideReunion_SamsaraRecord.Begin(npc.life))
            return false;
        npc.Anomaly.EventideReunion_SamsaraOwner = owner;
        npc.Anomaly.EventideReunion_SamsaraStartTick = Main.GameUpdateCount;
        npc.Anomaly.EventideReunion_SamsaraVisualTime = EventideReunionSamsaraRecord.Duration;
        npc.netUpdate = true;
        return true;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
        => writer.Write((ushort)npc.Anomaly.EventideReunion_SamsaraRecord.Remaining);

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
        => npc.Anomaly.EventideReunion_SamsaraVisualTime = reader.ReadUInt16();


}

public sealed class EventideReunionSamsaraSystem : ModSystem
{
    public override void Load()
    {
        On_NPC.StrikeNPC_HitInfo_bool_bool += RecordStrike;
        On_NPC.UpdateNPC_BuffApplyDOTs += RecordDebuffs;
    }

    // 观察共享生命根实体的实际生命差，不能逐个部件叠加命中数值。
    private static void Observe(NPC npc)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC root = EventideReunionSamsaraMarkBehavior.Root(npc);
            root.Anomaly.EventideReunion_SamsaraRecord.Observe(root.life);
        }
    }

    private static int RecordStrike(On_NPC.orig_StrikeNPC_HitInfo_bool_bool orig, NPC npc,
        NPC.HitInfo hit, bool fromNet, bool noPlayerInteraction)
    {
        Observe(npc);
        int result = orig(npc, hit, fromNet, noPlayerInteraction);
        Observe(npc);
        return result;
    }

    private static void RecordDebuffs(On_NPC.orig_UpdateNPC_BuffApplyDOTs orig, NPC npc)
    {
        Observe(npc);
        orig(npc);
        Observe(npc);
    }

    public override void PostUpdateEverything()
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            AnomalyGlobalNPC data = npc.Anomaly;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (data.EventideReunion_SamsaraVisualTime > 0)
                    data.EventideReunion_SamsaraVisualTime--;
                continue;
            }
            EventideReunionSamsaraRecord record = data.EventideReunion_SamsaraRecord;
            if (!record.Active)
                continue;
            if (npc.life <= 0)
            {
                record.Clear();
                data.EventideReunion_SamsaraVisualTime = 0;
                npc.netUpdate = true;
                continue;
            }
            record.Observe(npc.life);
            if (data.EventideReunion_SamsaraStartTick == Main.GameUpdateCount)
                continue;
            bool settle = record.Tick();
            data.EventideReunion_SamsaraVisualTime = record.Remaining;
            if (!settle)
                continue;
            int damage = record.Settlement;
            record.Clear(); // 先结束标记，结算自身绝不能再被计入。
            npc.netUpdate = true;
            if (damage == 0)
                continue;
            int owner = data.EventideReunion_SamsaraOwner;
            if (owner >= 0 && owner < Main.maxPlayers && Main.player[owner].active)
            {
                npc.lastInteraction = owner;
                npc.playerInteraction[owner] = true;
            }
            Projectile.NewProjectileAction<EventideReunionSamsaraEcho>(npc.GetSource_FromAI(), npc.Center + new Vector2(0f, npc.gfxOffY),
                Vector2.Zero, 0, 0f, Main.myPlayer, p =>
                {
                    p.GetModProjectile<EventideReunionSamsaraEcho>().SettledDamage = damage;
                    float companion = MathHelper.SmoothStep(0f, 1f, data.EventideReunion_DeathMarkProgress);
                    p.ai[1] = EventideReunionMarkDrawing.SamsaraRadius(npc) * MathHelper.Lerp(1f, 1.15f, companion);
                    p.ai[2] = companion;
                });
            // 不经过 CalculateHitInfo / StrikeNPC / 玩家命中钩子，避免减伤和附加命中效果。
            // checkDead 保留原版掉落、Boss 死亡/转阶段与其他模组的死亡流程。
            npc.life = (int)Math.Max(int.MinValue, (long)npc.life - damage);
            npc.HitEffect(new NPC.HitInfo { Damage = damage, HitDirection = 0 });
            if (npc.life <= 0)
                npc.checkDead();
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
        }
    }
}
