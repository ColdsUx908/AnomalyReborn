namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionDeathMark
{
    public const int Duration = 360;
    internal static bool Active(NPC npc) => npc.Anomaly.EventideReunion_DeathMarkTime > 0;
    internal static void Credit(NPC npc, int owner)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || owner < 0 || owner >= Main.maxPlayers)
            return;
        npc.lastInteraction = owner;
        npc.playerInteraction[owner] = true;
    }
    internal static void Apply(NPC npc, int owner)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || owner < 0 || owner >= Main.maxPlayers)
            return;
        // NPC.OnKill 在 OnHitNPC 之前执行；直接击杀也应掉治疗球。
        npc.Anomaly.EventideReunion_DeathHitOwner = owner;
        if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
            Main.npc[npc.realLife].Anomaly.EventideReunion_DeathHitOwner = owner;
        npc.Anomaly.EventideReunion_DeathOwner = owner;
        npc.Anomaly.EventideReunion_DeathMarkTime = Duration;
        npc.netUpdate = true;
        Credit(npc, owner);
    }
    internal static void CompleteHit(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        npc.Anomaly.EventideReunion_DeathHitOwner = -1;
        if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
            Main.npc[npc.realLife].Anomaly.EventideReunion_DeathHitOwner = -1;
    }
}

public sealed class EventideReunionDeathMarkBehavior : AnomalyGlobalNPCBehavior
{
    private const int TransitionDuration = 12;
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
        => writer.Write((ushort)npc.Anomaly.EventideReunion_DeathMarkTime);

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
        => npc.Anomaly.EventideReunion_DeathMarkTime = reader.ReadUInt16();
    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (EventideReunionDeathMark.Active(npc))
            modifiers.FinalDamage *= EventideReunionDamageConstants.DeathMarkIncomingDamageMultiplier;
    }

    public override void PostAI(NPC npc)
    {
        if (!Main.dedServ)
            npc.Anomaly.EventideReunion_DeathMarkProgress = MathHelper.Clamp(npc.Anomaly.EventideReunion_DeathMarkProgress
                + (EventideReunionDeathMark.Active(npc) ? 1f : -1f) / TransitionDuration, 0f, 1f);
        if (!Main.dedServ && EventideReunionDeathMark.Active(npc)
            && (Main.GameUpdateCount + (ulong)npc.whoAmI) % 4ul == 0)
            EmitMarkedNPCSparkles(npc);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        if (!EventideReunionDeathMark.Active(npc))
        {
            npc.Anomaly.EventideReunion_DeathOwner = -1;
            return;
        }
        // 排除多节 Boss 的从属部件及不可伤害、友好目标。
        bool boss = npc.boss || npc.realLife >= 0 && Main.npc[npc.realLife].boss;
        if (boss || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5 || npc.life <= 0
            || (long)npc.life * 4 >= npc.lifeMax)
            return;
        npc.StrikeInstantKill();
    }

    private static void EmitMarkedNPCSparkles(NPC npc)
    {
        // 发射源是 NPC 中心，不依附标记的四条轴线；每次随机转动整组放射方向。
        float phase = Main.rand.NextFloat(MathHelper.TwoPi);
        const int Count = 3;
        for (int i = 0; i < Count; i++)
        {
            float angle = phase + MathHelper.TwoPi * i / Count + Main.rand.NextFloat(-0.25f, 0.25f);
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f);
            Color purple = Color.Lerp(new Color(95, 25, 185), new Color(180, 85, 255), Main.rand.NextFloat());
            // 复用 Transoceanic 星芒的旋转、减速与平滑淡入淡出，生成后独立向外运动。
            ParticleHandler.SpawnParticle(new GenericSparkle(npc.Center, velocity,
                new Color(250, 240, 255), purple, Main.rand.NextFloat(0.22f, 0.38f),
                Main.rand.Next(24, 37), rotationSpeed: Main.rand.NextFloat(0.025f, 0.06f), bloomScale: 1.3f));
        }
    }

    public override void OnKill(NPC npc)
    {
        EmitDeathFade(npc);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        int hitOwner = npc.Anomaly.EventideReunion_DeathHitOwner;
        npc.Anomaly.EventideReunion_DeathHitOwner = -1;
        if (hitOwner < 0 && !EventideReunionDeathMark.Active(npc))
            return;
        int owner = hitOwner >= 0 ? hitOwner : npc.Anomaly.EventideReunion_DeathOwner;
        if (owner < 0 || owner >= Main.maxPlayers || !Main.player[owner].active || Main.player[owner].dead
            || npc.friendly || npc.lifeMax <= 5 || npc.realLife >= 0 && npc.realLife != npc.whoAmI)
            return;
        Projectile.NewProjectileAction<EventideReunionDeathSoul>(npc.GetSource_Death(), npc.Center,
            new Vector2(0f, -3f), 0, 0f, owner, p => { p.ai[0] = owner; p.ai[1] = 5f; });
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (npc.life <= 0)
            EmitDeathFade(npc);
    }

    private static void EmitDeathFade(NPC npc)
    {
        if (!Main.dedServ && npc.Anomaly.EventideReunion_DeathMarkProgress > 0f)
        {
            // NPC 死亡后不再绘制，由短命本地粒子把缩小动画播放完。
            float radius = MarkRadius(npc) * MathHelper.SmoothStep(0f, 1f, npc.Anomaly.EventideReunion_DeathMarkProgress);
            float companion = EventideReunionMarkDrawing.SamsaraVisibility(npc);
            radius *= MathHelper.Lerp(1f, 0.8f, companion);
            npc.Anomaly.EventideReunion_DeathMarkProgress = 0f;
            ParticleHandler.SpawnParticle(new EventideReunionDeathMarkFade(npc.Center + new Vector2(0f, npc.gfxOffY), radius, TransitionDuration, companion));
        }
    }

    private static float MarkRadius(NPC npc) => MathHelper.Clamp(Math.Max(npc.width, npc.height) * 0.55f, 30f, 70f);
}

// 每游戏帧递减一次，不受 NPC AI 更新频率或 Buff 免疫影响。
public sealed class EventideReunionDeathMarkSystem : ModSystem
{
    public override void PostUpdateNPCs()
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.Anomaly.EventideReunion_DeathMarkTime <= 0)
                continue;
            if (--npc.Anomaly.EventideReunion_DeathMarkTime == 0)
            {
                npc.Anomaly.EventideReunion_DeathOwner = -1;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.netUpdate = true;
            }
        }
    }
}