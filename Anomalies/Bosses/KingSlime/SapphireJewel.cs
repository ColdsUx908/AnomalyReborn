// Developed by ColdsUx

namespace Anomalies.Bosses.KingSlime;

public sealed class SapphireJewel : JewelNPC
{
    public override string LocalizationCategory => "Bosses.KingSlime";

    public override void SetStaticDefaults() => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });

    public override void SetDefaults()
    {
        NPC.damage = 10;
        NPC.width = 30;
        NPC.height = 30;
        NPC.defense = 5;

        NPC.lifeMax = 250;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);

        NPC.knockBackResist = 0.4f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = JewelHandler.HitSound;
        NPC.DeathSound = JewelHandler.ShatterSound;

        NPC.IsImportantBossMinion = true;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (MasterDead)
        {
            JewelHandler.Kill(NPC);
            return;
        }

        if (!NPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
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

        JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.5f, 150f, -150f, 50f, -250f);

        NPC.netUpdate = true;
    }

    public override bool CheckDead()
    {
        if (Ultra && !MasterDead)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }
}
