namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunionDeathMark : ModBuff
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public const int Duration = 360;
    public const float IncomingDamageMultiplier = 1.15f;
    public override string Texture => EventideReunionHoldout.AssetPath + "Death/EventideReunionDeathMark";
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }
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
        // NPC.OnKill 在 OnHitNPC 之前执行；即使目标免疫印记，直接击杀仍应掉治疗球。
        npc.Anomaly.EventideReunionDeathHitOwner = owner;
        if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
            Main.npc[npc.realLife].Anomaly.EventideReunionDeathHitOwner = owner;
        npc.Anomaly.EventideReunionDeathOwner = owner;
        npc.AddBuff(ModContent.BuffType<EventideReunionDeathMark>(), Duration);
        Credit(npc, owner);
    }
    internal static void CompleteHit(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        npc.Anomaly.EventideReunionDeathHitOwner = -1;
        if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs)
            Main.npc[npc.realLife].Anomaly.EventideReunionDeathHitOwner = -1;
    }
}

public sealed class EventideReunionDeathMarkBehavior : AnomalyGlobalNPCBehavior
{
    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (npc.HasBuff<EventideReunionDeathMark>())
            modifiers.FinalDamage *= EventideReunionDeathMark.IncomingDamageMultiplier;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || !npc.HasBuff<EventideReunionDeathMark>())
            return;
        spriteBatch.End();
        EventideReunionDeathDrawing.DrawMark(npc.Center + new Vector2(0f, npc.gfxOffY),
            MathHelper.Clamp(Math.Max(npc.width, npc.height) * 0.55f, 30f, 70f), screenPos);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        if (!npc.HasBuff<EventideReunionDeathMark>())
        {
            npc.Anomaly.EventideReunionDeathOwner = -1;
            return;
        }
        // 排除多节 Boss 的从属部件及不可伤害、友好目标。
        bool boss = npc.boss || npc.realLife >= 0 && Main.npc[npc.realLife].boss;
        if (boss || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5 || npc.life <= 0
            || (long)npc.life * 4 >= npc.lifeMax)
            return;
        npc.StrikeInstantKill();
    }

    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        int hitOwner = npc.Anomaly.EventideReunionDeathHitOwner;
        npc.Anomaly.EventideReunionDeathHitOwner = -1;
        if (hitOwner < 0 && !npc.HasBuff<EventideReunionDeathMark>())
            return;
        int owner = hitOwner >= 0 ? hitOwner : npc.Anomaly.EventideReunionDeathOwner;
        if (owner < 0 || owner >= Main.maxPlayers || !Main.player[owner].active || Main.player[owner].dead
            || npc.friendly || npc.lifeMax <= 5 || npc.realLife >= 0 && npc.realLife != npc.whoAmI)
            return;
        Projectile.NewProjectileAction<EventideReunionDeathSoul>(npc.GetSource_Death(), npc.Center,
            new Vector2(0f, -3f), 0, 0f, owner, p => { p.ai[0] = owner; p.ai[1] = 5f; });
    }
}
