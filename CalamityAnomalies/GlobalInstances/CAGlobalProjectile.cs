namespace CalamityAnomalies.GlobalInstances;

public sealed class CAGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64 DebugData = new();
#endif

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot2];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        CAGlobalProjectile clone = (CAGlobalProjectile)base.Clone(from, to);

        Array.Copy(AnomalyAI32, clone.AnomalyAI32, AISlot);
        Array.Copy(AnomalyAI64, clone.AnomalyAI64, AISlot2);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot2);

        return clone;
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        Dictionary<int, float> aiToSend = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (AIChanged32[i])
                aiToSend[i] = AnomalyAI32[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged32 = default;

        Dictionary<int, double> aiToSend2 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged64[i])
                aiToSend2[i] = AnomalyAI64[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged64 = default;

        aiToSend.Clear();
        aiToSend2.Clear();

        for (int i = 0; i < AISlot - 1; i++)
        {
            if (InternalAIChanged32[i])
                aiToSend[i] = InternalAnomalyAI32[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged32 = default;

        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (InternalAIChanged64[i])
                aiToSend2[i] = InternalAnomalyAI64[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged64 = default;
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        int recievedAICount = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount; i++)
        {
            int index = binaryReader.ReadInt32();
            float value = binaryReader.ReadSingle();
            AnomalyAI32[index].f = value;
        }

        int recievedAICount2 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount2; i++)
        {
            int index = binaryReader.ReadInt32();
            double value = binaryReader.ReadDouble();
            AnomalyAI64[index].d = value;
        }

        int recievedAICount3 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount3; i++)
        {
            int index = binaryReader.ReadInt32();
            float value = binaryReader.ReadSingle();
            InternalAnomalyAI32[index].f = value;
        }

        int recievedAICount4 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount4; i++)
        {
            int index = binaryReader.ReadInt32();
            double value = binaryReader.ReadDouble();
            InternalAnomalyAI64[index].d = value;
        }
    }

    #region 额外数据
    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI32[0].bits[0];
        set
        {
            if (InternalAnomalyAI32[0].bits[0] != value)
            {
                InternalAnomalyAI32[0].bits[0] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public int OverrideType
    {
        get => InternalAnomalyAI32[1].i;
        set
        {
            if (InternalAnomalyAI32[1].i != value)
            {
                InternalAnomalyAI32[1].i = value;
                InternalAIChanged32[1] = true;
            }
        }
    }
    #endregion 额外数据
}
