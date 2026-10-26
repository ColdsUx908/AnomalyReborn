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
        get => NPC.GetNPCFromIndex(AI_Union_0.Byte0 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.Byte0 = (byte)(value is null || value.whoAmI >= Main.maxNPCs ? 0 : value.whoAmI + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidHead => Head.active && Head.type == NPCID.EaterofWorldsHead;

    public EaterofWorldsHead HeadBehavior => EaterofWorldsHead.GetInstance(Head);

    public NPC LocalHead
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.Byte1 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.Byte1 = (byte)(value is null || value.whoAmI >= Main.maxNPCs ? 0 : value.whoAmI + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidLocalHead => LocalHead.active && LocalHead.type == NPCID.EaterofWorldsHead;

    public NPC Previous
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.Byte2 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.Byte2 = (byte)(value is null || value.whoAmI >= Main.maxNPCs ? 0 : value.whoAmI + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidPrevious => Previous.active && Previous.EoW;

    public NPC Next
    {
        get => NPC.GetNPCFromIndex(AI_Union_0.Byte3 - 1);
        set
        {
            Union32 union = AI_Union_0;
            union.Byte3 = (byte)(value is null || value.whoAmI >= Main.maxNPCs ? 0 : value.whoAmI + 1);
            AI_Union_0 = union;
        }
    }
    public bool ValidNext => Next.active && Next.EoW;

    public bool IsFirstSegment => SegmentIndex <= 1;
    public bool IsLastSegment => SegmentIndex == EaterofWorldsHead.TotalSegmentCount;

    public int SegmentIndex
    {
        get => AI_Union_1.I;
        set
        {
            Union32 union = AI_Union_1;
            union.I = value;
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

    protected int GetContactDamage()
    {
        if (!ValidHead)
            return 0;
        EaterofWorldsHead owner = HeadBehavior;
        if (owner.ContactSuppressed)
            return 0;
        if (IsUnderCoilMask)
            return 0;
        if (owner.CurrentBehavior == EaterofWorldsHead.Behavior.Phase1_CrossSweep)
        {
            if (owner.CurrentLocalPhase < 2 || !ValidLocalHead)
                return 0;
            int group = (EaterofWorldsHead.GetInstance(LocalHead).SegmentIndex - 1) / (EaterofWorldsHead.TotalSegmentCount / 3);
            if (group == 2 || group == 1 && owner.CurrentLocalPhase == 2 && owner.Timer1 <= 24)
                return 0;
        }
        if (owner.CurrentBehavior == EaterofWorldsHead.Behavior.Phase1_TripleHunt && ValidLocalHead)
        {
            int group = (EaterofWorldsHead.GetInstance(LocalHead).SegmentIndex - 1) / (EaterofWorldsHead.TotalSegmentCount / 3);
            if (owner.CurrentLocalPhase < 2 || group != owner.AttackIndex % 3)
                return 0;
        }
        if (owner.CurrentBehavior is EaterofWorldsHead.Behavior.Phase1_Split or EaterofWorldsHead.Behavior.Phase1_Combine
            && owner.CurrentLocalPhase == 3)
        {
            bool combine = owner.CurrentBehavior == EaterofWorldsHead.Behavior.Phase1_Combine;
            int ordinal = ValidLocalHead ? EaterofWorldsHead.GetInstance(LocalHead).SegmentIndex : 1;
            int group = (ordinal - 1) / (EaterofWorldsHead.TotalSegmentCount / 3);
            if (owner.Timer1 <= (combine ? 42 : 30 + group * EaterofWorldsHead.SplitLaunchDelay))
                return 0;
        }
        return SegmentType switch
        {
            EaterofWorldsSegmentType.Head => EaterofWorldsHead.HeadContactDamage,
            EaterofWorldsSegmentType.Body => EaterofWorldsHead.BodyContactDamage,
            _ => EaterofWorldsHead.TailContactDamage,
        };
    }

    protected bool IsUnderCoilMask => ValidHead && HeadBehavior.CoilMaskStrength > 0f
        && NPC.Distance(HeadBehavior.CoilingCenter) < 180f;

    protected void UpdateCoilDR()
    {
        AnomalyNPC.DR = IsUnderCoilMask ? 0.95f : 0f;
    }

    protected void BodyAndTailMovementAI()
    {
        NPC.damage = GetContactDamage();
        NPC.defense = 8;
        Vector2 delta = Previous.Center - NPC.Center;
        float spacing = Main.getGoodWorld ? 62f : NPC.width * NPC.scale;
        Vector2 direction = delta.SafeNormalize(Vector2.UnitY);
        // Pull only: pushing compressed segments apart unfolds a coil instantly, even with a stationary head.
        NPC.velocity = direction * Math.Max(0f, delta.Length() - spacing);
        if (HeadBehavior.CurrentBehavior is EaterofWorldsHead.Behavior.Phase1_Split or EaterofWorldsHead.Behavior.Phase1_Combine
            && delta.Length() <= spacing + Previous.velocity.Length() + 8f)
        {
            // Follow the predecessor's travelled arc, not a chord cutting across its tight orbit.
            // Otherwise the inner segments stop moving and the distant tail can never enter the coil.
            Vector2 point = Previous.Center;
            float remaining = spacing;
            foreach (Vector2 oldPosition in Previous.oldPos)
            {
                Vector2 past = oldPosition + Previous.Size * 0.5f;
                float distance = Vector2.Distance(point, past);
                if (distance >= remaining && distance > 0.001f)
                {
                    Vector2 destination = Vector2.Lerp(point, past, remaining / distance);
                    Vector2 movement = destination - NPC.Center;
                    float speed = movement.Length();
                    NPC.velocity = movement.SafeNormalize(Vector2.Zero) * Math.Min(speed, 40f);
                    break;
                }
                remaining -= distance;
                point = past;
            }
        }
        NPC.rotation += MathHelper.WrapAngle(direction.ToRotation() + MathHelper.PiOver2 - NPC.rotation) * 0.28f;
    }
}
