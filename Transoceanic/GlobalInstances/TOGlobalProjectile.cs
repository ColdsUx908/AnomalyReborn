using Transoceanic.Game.Networking;
using Transoceanic.Graphics.Particles.BuiltIn;

namespace Transoceanic.GlobalInstances;

public sealed class TOGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public readonly List<AfterimageParticle> Afterimages = [];

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    internal Union32[] OceanAI32 { get; private set; } = new Union32[AISlot];
    internal Union64[] OceanAI64 { get; private set; } = new Union64[AISlot2];

    internal ref BitArray32 AIChanged32 => ref OceanAI32[^1].Bits;
    internal ref BitArray64 AIChanged64 => ref OceanAI64[^1].Bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        TOGlobalProjectile clone = (TOGlobalProjectile)base.Clone(from, to);

        clone.OceanAI32 = (Union32[])OceanAI32.Clone();
        clone.OceanAI64 = (Union64[])OceanAI64.Clone();

        return clone;
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.WriteAI32Snapshot(binaryWriter, OceanAI32, 1);
        TONetUtils.WriteAI64Snapshot(binaryWriter, OceanAI64, 1);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TONetworkEnvironment.SyncEnabled)
            return;

        TONetUtils.ReadAI32Snapshot(binaryReader, OceanAI32, 1);
        TONetUtils.ReadAI64Snapshot(binaryReader, OceanAI64, 1);
    }
}
