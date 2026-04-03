namespace CalamityAnomalies.GlobalInstances;

public sealed class CAGlobalNPC : GlobalNPC, IContentLoader
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
    private const int AISlot3 = 132;
    private const int AISlot4 = 33;

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot3];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^4].bits;
    private ref BitArray32 InternalAIChanged32_2 => ref InternalAnomalyAI32[^3].bits;
    private ref BitArray32 InternalAIChanged32_3 => ref InternalAnomalyAI32[^2].bits;
    private ref BitArray32 InternalAIChanged32_4 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        CAGlobalNPC clone = (CAGlobalNPC)base.Clone(from, to);

        Array.Copy(AnomalyAI32, clone.AnomalyAI32, AISlot);
        Array.Copy(AnomalyAI64, clone.AnomalyAI64, AISlot2);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot3);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot4);

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
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

        for (int i = 0; i < 32; i++)
        {
            if (InternalAIChanged32[i])
                aiToSend[i] = InternalAnomalyAI32[i].f;
            if (InternalAIChanged32_2[i])
                aiToSend[i + 32] = InternalAnomalyAI32[i + 32].f;
            if (InternalAIChanged32_3[i])
                aiToSend[i + 64] = InternalAnomalyAI32[i + 64].f;
            if (InternalAIChanged32_4[i])
                aiToSend[i + 96] = InternalAnomalyAI32[i + 96].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged32 = default;
        InternalAIChanged32_2 = default;
        InternalAIChanged32_3 = default;
        InternalAIChanged32_4 = default;

        for (int i = 0; i < AISlot4 - 1; i++)
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

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
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

    public bool Debuff_DimensionalRend
    {
        get => InternalAnomalyAI32[0].bits[1];
        set
        {
            if (InternalAnomalyAI32[0].bits[1] != value)
            {
                InternalAnomalyAI32[0].bits[1] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public int AnomalyKilltime;

    public int AnomalyAITimer;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int AnomalyUltraAITimer;
    public int AnomalyUltraBarTimer;

    /// <summary>
    /// 额外DR，不受任何修改DR的机制影响。
    /// </summary>
    /// <remarks>谨慎使用。</remarks>
    public float ExtraDR
    {
        get => InternalAnomalyAI32[5].f;
        set
        {
            if (InternalAnomalyAI32[5].f != value)
            {
                InternalAnomalyAI32[5].f = value;
                InternalAIChanged32[5] = true;
            }
        }
    }
    #endregion 额外数据
}
