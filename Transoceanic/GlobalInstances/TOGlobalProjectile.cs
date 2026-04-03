using Transoceanic.DataStructures;
using Transoceanic.DataStructures.Particles;

namespace Transoceanic.GlobalInstances;

public class TOGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public readonly List<AfterimageParticle> Afterimages = [];

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    internal readonly Union32[] OceanAI32 = new Union32[AISlot];
    internal readonly Union64[] OceanAI64 = new Union64[AISlot2];

    internal ref BitArray32 AIChanged32 => ref OceanAI32[^1].bits;
    internal ref BitArray64 AIChanged64 => ref OceanAI64[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        TOGlobalProjectile clone = (TOGlobalProjectile)base.Clone(from, to);

        Array.Copy(OceanAI32, clone.OceanAI32, AISlot);
        Array.Copy(OceanAI64, clone.OceanAI64, AISlot2);

        return clone;
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TOSharedData.SyncEnabled)
            return;

        Dictionary<int, float> aiToSend = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (AIChanged32[i])
                aiToSend[i] = OceanAI32[i].f;
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
                aiToSend2[i] = OceanAI64[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged64 = default;
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOSharedData.SyncEnabled)
            return;

        int recievedAICount = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount; i++)
        {
            int index = binaryReader.ReadInt32();
            float value = binaryReader.ReadSingle();
            OceanAI32[index].f = value;
        }

        int recievedAICount2 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount2; i++)
        {
            int index = binaryReader.ReadInt32();
            double value = binaryReader.ReadDouble();
            OceanAI64[index].d = value;
        }
    }
}
