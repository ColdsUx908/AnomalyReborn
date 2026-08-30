namespace Anomalies.Bosses.EaterofWorlds;

public sealed class EaterofWorldsTail : AnomalyNPCBehavior<EaterofWorldsTail>
{
    public NPC Head
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.byte0);
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)(value?.whoAmI ?? Main.maxNPCs);
            AI_Union_0 = union;
        }
    }
    public bool ValidHead => Head.active && Head.type == NPCID.EaterofWorldsHead;

    public NPC Previous
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.byte1);
        set
        {
            Union32 union = AI_Union_0;
            union.byte1 = (byte)(value?.whoAmI ?? Main.maxNPCs);
            AI_Union_0 = union;
        }
    }
    public bool ValidPrevious => Previous.active && Previous.type == NPCID.EaterofWorldsBody;

    public override int ApplyingType => NPCID.EaterofWorldsTail;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public static event Action<EaterofWorldsTail> OnRunningPreAI;

    public override bool PreAI()
    {
        if (!ValidHead || !ValidPrevious)
        {
            EaterofWorldsHandler.DestroySegment(NPC);
            return false;
        }

        NPC.realLife = Head.whoAmI;
        BodyAndTailMovementAI();

        OnRunningPreAI?.Invoke(this);

        return false;

        void BodyAndTailMovementAI()
        {
            // 计算到前一个体节的方向向量
            Vector2 targetPos = Previous.Center - NPC.Center;
            float targetPosX = targetPos.X;
            float targetPosY = targetPos.Y;

            // 设定旋转角度（朝向前一个体节）
            NPC.rotation = (float)Math.Atan2(targetPosY, targetPosX) + MathHelper.PiOver2;

            // 计算当前体节与前一个体节的距离
            float targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);

            // 获取期望的间隔宽度（受缩放影响，特殊情况下固定为62）
            int npcWidth = (int)(NPC.width * NPC.scale);
            if (Main.getGoodWorld)
                npcWidth = 62;

            // 将目标向量缩放到保持固定距离
            targetDistance = (targetDistance - npcWidth) / targetDistance;
            targetPos *= targetDistance;

            // 直接设置位置，速度归零（体节无独立速度）
            NPC.velocity = Vector2.Zero;
            NPC.position += targetPos;
        }
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
