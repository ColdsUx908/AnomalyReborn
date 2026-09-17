namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsBody : EaterofWorldsSegment<EaterofWorldsBody>
{
    public bool HasInitialized
    {
        get => AnomalyNPC.AnomalyAI32[0].bits[0];
        set
        {
            if (AnomalyNPC.AnomalyAI32[0].bits[0] != value)
            {
                AnomalyNPC.AnomalyAI32[0].bits[0] = value;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public override EaterofWorldsSegmentType SegmentType => EaterofWorldsSegmentType.Body;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public static event Action<EaterofWorldsBody> OnRunningPreAI;

    public override bool PreAI()
    {
        if (!ValidHead || !ValidPrevious)
        {
            EaterofWorldsHandler.DestroySegment(NPC);
            return false;
        }

        if (!HasInitialized)
        {
            HasInitialized = true;
            for (int i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i++)
                NPC.oldPos[i] = NPC.position;
        }

        NPC.realLife = Head.whoAmI;
        BodyAndTailMovementAI();

        OnRunningPreAI?.Invoke(this);

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.DrawFromCenter(NPC.Texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.scale);
        return false;
    }

    public override bool PreKill()
    {
        NPCLoader.blockLoot.Add(ItemID.DemoniteOre);
        NPCLoader.blockLoot.Add(ItemID.ShadowScale);
        return true;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;
}
