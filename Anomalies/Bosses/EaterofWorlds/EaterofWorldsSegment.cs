namespace Anomalies.Bosses.EaterofWorlds;

public enum EaterofWorldsSegmentType : byte
{
    Head,
    Body,
    Tail
}

public interface IEaterofWorldsSegment
{
    public abstract NPC Head { get; set; }
    public abstract bool ValidHead { get; }
    public abstract NPC LocalHead { get; set; }
    public abstract bool ValidLocalHead { get; }
    public abstract NPC Previous { get; set; }
    public abstract bool ValidPrevious { get; }
    public abstract NPC Next { get; set; }
    public abstract bool ValidNext { get; }
    public abstract int SegmentIndex { get; set; }
    public abstract bool IsFirstSegment { get; }
    public abstract bool IsLastSegment { get; }
    public abstract EaterofWorldsSegmentType SegmentType { get; }
}


public abstract class EaterofWorldsSegment<TBehavior> : AnomalyNPCBehavior<TBehavior>, IEaterofWorldsSegment where TBehavior : EaterofWorldsSegment<TBehavior>, new()
{
    //此处所有NPC索引均为whoAmI+1，0表示无效索引
    //这样做的目的是使默认值（0）为空NPC，确保真正的头部能在默认状态下正常工作

    public NPC Head
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.byte0 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)((value?.whoAmI ?? Main.maxNPCs) + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidHead => Head.active && Head.type == NPCID.EaterofWorldsHead;

    public EaterofWorldsHead HeadBehavior => EaterofWorldsHead.GetInstance(Head);

    public NPC LocalHead
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.byte1 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.byte1 = (byte)((value?.whoAmI ?? Main.maxNPCs) + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidLocalHead => LocalHead.active && LocalHead.type == NPCID.EaterofWorldsHead;

    public NPC Previous
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.byte2 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.byte2 = (byte)((value?.whoAmI ?? Main.maxNPCs) + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidPrevious => Previous.active && Previous.EoW;

    public NPC Next
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.byte3 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.byte3 = (byte)((value?.whoAmI ?? Main.maxNPCs) + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidNext => Next.active && Next.EoW;

    public bool IsFirstSegment => !ValidPrevious;
    public bool IsLastSegment => !ValidNext;

    public int SegmentIndex
    {
        get => AI_Union_1.i;
        set
        {
            Union32 union = AI_Union_1;
            union.i = value;
            AI_Union_1 = union;
        }
    }

    public abstract EaterofWorldsSegmentType SegmentType { get; }

    public sealed override int ApplyingType => SegmentType switch
    {
        EaterofWorldsSegmentType.Head => NPCID.EaterofWorldsHead,
        EaterofWorldsSegmentType.Body => NPCID.EaterofWorldsBody,
        EaterofWorldsSegmentType.Tail => NPCID.EaterofWorldsTail,
        _ => NPCID.None
    };

    protected void BodyAndTailMovementAI()
    {
        EaterofWorldsHead headBehavior = HeadBehavior;

        float amount = headBehavior.CurrentBehavior is EaterofWorldsHead.Behavior.Phase1_Split or EaterofWorldsHead.Behavior.Phase1_Combine && headBehavior.CurrentLocalPhase >= 2
            ? Math.Clamp(headBehavior.Timer1 / 60f, 0f, 1f) : 0f;

        NPC.velocity = Vector2.Lerp(GetVelocity_Chain(), GetVelocity_Coil(), amount);
        NPC.rotation = NPC.GetRotationTowards(Previous.Center, MathHelper.PiOver2);

        Vector2 GetVelocity_Chain()
        {
            // 计算到前一个体节的方向向量
            Vector2 targetPos = Previous.Center - NPC.Center;
            float targetPosX = targetPos.X;
            float targetPosY = targetPos.Y;

            // 计算当前体节与前一个体节的距离
            float targetDistance = (float)Math.Sqrt(targetPosX * targetPosX + targetPosY * targetPosY);

            // 获取期望的间隔宽度（受缩放影响，特殊情况下固定为62）
            int npcWidth = (int)(NPC.width * NPC.scale);
            if (Main.getGoodWorld)
                npcWidth = 62;

            // 将目标向量缩放到保持固定距离
            targetDistance = (targetDistance - npcWidth) / targetDistance;
            targetPos *= targetDistance;

            return targetPos;
        }

        Vector2 GetVelocity_Coil()
        {
            int trailLength = NPCID.Sets.TrailCacheLength[NPC.type];

            // 计算要追踪的历史位置索引
            int pastPos = Math.Clamp(trailLength - 7, 0, trailLength - 1);

            // 核心跟随：把自身中心贴到前一个体节某个历史位置的中心
            Vector2 targetPos = Previous.oldPos[pastPos];

            // 距离限制：防止体节之间拉得过长
            int prevIndex = pastPos - 1;
            if (prevIndex >= 0 && prevIndex < trailLength)
            {
                float maxDistance = Main.getGoodWorld ? 62f : NPC.width * NPC.scale;
                Vector2 prevOldPosCenter = NPC.oldPos[prevIndex] + NPC.Size / 2f;

                if (NPC.Distance(prevOldPosCenter) > maxDistance)
                    NPC.oldPos[prevIndex] = targetPos + (NPC.oldPos[prevIndex] - targetPos).ToCustomLength(maxDistance);
            }

            return targetPos - NPC.position;
        }
    }
}
