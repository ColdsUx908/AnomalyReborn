using Transoceanic.GlobalInstances;

namespace Transoceanic.Behaviors;

public abstract partial class SingleNPCBehavior : SingleEntityBehavior<NPC>
{
    public NPC NPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity; }

    public TOGlobalNPC OceanNPC { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _Entity.Ocean; }

    public Player Target => NPC.PlayerTarget;

    public int Timer1
    {
        get => NPC.Timer1;
        set => NPC.Timer1 = value;
    }

    public int Timer2
    {
        get => NPC.Timer2;
        set => NPC.Timer2 = value;
    }

    public int Timer3
    {
        get => NPC.Timer3;
        set => NPC.Timer3 = value;
    }

    public int Timer4
    {
        get => NPC.Timer4;
        set => NPC.Timer4 = value;
    }

    public int Timer5
    {
        get => NPC.Timer5;
        set => NPC.Timer5 = value;
    }

    public float Timer6
    {
        get => NPC.Timer6;
        set => NPC.Timer6 = value;
    }

    public float Timer7
    {
        get => NPC.Timer7;
        set => NPC.Timer7 = value;
    }

    public float Timer8
    {
        get => NPC.Timer8;
        set => NPC.Timer8 = value;
    }

    public Union32 AI_Union_0
    {
        get => (Union32)NPC.ai[0];
        set => NPC.ai[0] = value.F;
    }

    public Union32 AI_Union_1
    {
        get => (Union32)NPC.ai[1];
        set => NPC.ai[1] = value.F;
    }

    public Union32 AI_Union_2
    {
        get => (Union32)NPC.ai[2];
        set => NPC.ai[2] = value.F;
    }

    public Union32 AI_Union_3
    {
        get => (Union32)NPC.ai[3];
        set => NPC.ai[3] = value.F;
    }

    public Union32 LocalAI_Union_0
    {
        get => (Union32)NPC.localAI[0];
        set => NPC.localAI[0] = value.F;
    }

    public Union32 LocalAI_Union_1
    {
        get => (Union32)NPC.localAI[1];
        set => NPC.localAI[1] = value.F;
    }

    public Union32 LocalAI_Union_2
    {
        get => (Union32)NPC.localAI[2];
        set => NPC.localAI[2] = value.F;
    }

    public Union32 LocalAI_Union_3
    {
        get => (Union32)NPC.localAI[3];
        set => NPC.localAI[3] = value.F;
    }

    public IEntitySource SourceAI => NPC.GetSource_FromAI();

    /// <inheritdoc cref="GlobalNPC.CanFallThroughPlatforms"/>
    public virtual bool? CanFallThroughPlatforms() => null;

    /// <inheritdoc cref="GlobalNPC.CanBeCaughtBy"/>
    public virtual bool? CanBeCaughtBy(Item item, Player player) => null;

    /// <inheritdoc cref="GlobalNPC.CanChat"/>
    public virtual bool? CanChat() => null;

    /// <inheritdoc cref="GlobalNPC.CanGoToStatue"/>
    public virtual bool? CanGoToStatue(bool toKingStatue) => null;
}
