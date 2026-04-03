namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlimeJewelSapphire : CAModNPC, IKingSlimeJewel
{
    public const float DespawnDistance = 5000f;

    public bool HasInitialized
    {
        get => AI_Union_2.bits[0];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[0] = value;
            AI_Union_2 = union;
        }
    }

    public bool HasEnteredPhase2
    {
        get => AI_Union_2.bits[1];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[1] = value;
            AI_Union_2 = union;
        }
    }

    public bool CanAttack
    {
        get => AI_Union_2.bits[2];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[2] = value;
            AI_Union_2 = union;
        }
    }

    public bool KingSlimeDead
    {
        get => AI_Union_2.bits[3];
        set
        {
            Union32 union = AI_Union_2;
            union.bits[3] = value;
            AI_Union_2 = union;
        }
    }

    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults() => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 10;
        NPC.width = 30;
        NPC.height = 30;
        NPC.defense = 5;

        NPC.lifeMax = 250;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.4f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath15;
        CalamityNPC.VulnerableToSickness = false;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (KingSlimeDead)
        {
            KingSlime_Handler.Kill(NPC);
            return;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            KingSlime_Handler.Despawn(NPC);
            return;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return;
        }

        NPC.damage = 0;
        Lighting.AddLight(NPC.Center, 0f, 0f, 1f);

        if (!HasInitialized)
        {
            CanAttack = true;

            HasInitialized = true;
        }

        KingSlime_Handler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.5f, 150f, -150f, 50f, -250f);

        NPC.netUpdate = true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        KingSlime_Handler.DrawJewel(spriteBatch, screenPos, NPC, 0f);
        return false;
    }

    public override bool CheckActive() => false;

    public override bool CheckDead()
    {
        if (Ultra && !KingSlimeDead)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                KingSlime_Handler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        KingSlime_Handler.HitEffect(NPC);
    }

    public override void OnKill()
    {
        KingSlime_Handler.OnKill(NPC);
    }
}
