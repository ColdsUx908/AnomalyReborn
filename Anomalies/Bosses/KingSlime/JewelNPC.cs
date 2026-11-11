namespace Anomalies.Bosses.KingSlime;

public abstract class JewelNPC : AnomalyModNPC
{
    public const float DespawnDistance = 5000f;

    private bool _SpawnEffectPlayed;

    public override void PostAI()
    {
        if (Main.dedServ || _SpawnEffectPlayed)
            return;

        _SpawnEffectPlayed = true;
        SoundEngine.PlaySound(JewelHandler.SpawnSound, NPC.Center);
        for (int i = 0; i < 50; i++)
            JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(5f, 10f), Main.rand.Next(40, 60), Main.rand.NextFloat(0.4f, 0.7f));
    }

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set
        {
            if (NPC.ai[1] != value)
            {
                NPC.ai[1] = value;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.netUpdate = true;
            }
        }
    }

    public bool HasInitialized
    {
        get => AI_Union_2.Bits[0];
        set
        {
            if (HasInitialized != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_2;
            union.Bits[0] = value;
            AI_Union_2 = union;
        }
    }

    public bool HasEnteredPhase2
    {
        get => AI_Union_2.Bits[1];
        set
        {
            if (HasEnteredPhase2 != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_2;
            union.Bits[1] = value;
            AI_Union_2 = union;
        }
    }

    public bool CanAttack
    {
        get => AI_Union_2.Bits[2];
        set
        {
            if (CanAttack != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_2;
            union.Bits[2] = value;
            AI_Union_2 = union;
        }
    }

    public bool MasterDead
    {
        get => AI_Union_2.Bits[3];
        set
        {
            if (MasterDead != value && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
            Union32 union = AI_Union_2;
            union.Bits[3] = value;
            AI_Union_2 = union;
        }
    }

    public bool DeathLocked => Ultra && !MasterDead;

    public override bool CheckActive() => false;

    public override void HitEffect(NPC.HitInfo hit) => JewelHandler.HitEffect(NPC);

    public override void OnKill() => JewelHandler.OnKill(NPC);

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC);
        return false;
    }
}

