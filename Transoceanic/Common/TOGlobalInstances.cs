// Designed by ColdsUx

using Transoceanic.DataStructures.Particles;
using Transoceanic.Framework.Helpers.Utilities;

namespace Transoceanic.Common;

public sealed class TOPlayer : ModPlayer, IContentLoader
{
    public CommandCallInfo CommandCallInfo { get; internal set; }

    public int GameTime { get; internal set; }

    public bool IsHurt;

    public int TimeWithoutHurt;

    /// <summary>
    /// 提升玩家翅膀飞行时间的乘区。
    /// <br/>每个索引独立计算。
    /// </summary>
    public AddableFloat[] WingTimeMaxMultipliers = new AddableFloat[5];

    public Vector2? ScreenFocusCenter;

    /// <summary>
    /// 屏幕位置更改的插值参数，限制在区间 [0, 1] 内。
    /// <br/>值越大，表示屏幕的真实中心将越靠近 <see cref="ScreenFocusCenter"/>。
    /// <br/>当 <see cref="ScreenFocusCenter"/> 不为 <see langword="null"/> 时每帧递减 0.1，否则自动设为 0。
    /// </summary>
    public float ScreenFocusInterpolant
    {
        get;
        set => field = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// 当此值大于0时，<see cref="ScreenFocusInterpolant"/> 将不会更新。
    /// 每帧递减。
    /// </summary>
    public int ScreenFocusHoldInPlaceTime
    {
        get;
        set => field = Math.Max(value, 0);
    }

    public float CurrentScreenShakePower
    {
        get;
        set => field = Math.Max(value, 0f);
    }

    public override ModPlayer Clone(Player newEntity)
    {
        TOPlayer clone = (TOPlayer)base.Clone(newEntity);

        clone.CommandCallInfo = CommandCallInfo;
        clone.GameTime = GameTime;
        clone.IsHurt = IsHurt;
        clone.TimeWithoutHurt = TimeWithoutHurt;
        Array.Copy(WingTimeMaxMultipliers, clone.WingTimeMaxMultipliers, WingTimeMaxMultipliers.Length);
        clone.ScreenFocusCenter = ScreenFocusCenter;
        clone.ScreenFocusInterpolant = ScreenFocusInterpolant;
        clone.ScreenFocusHoldInPlaceTime = ScreenFocusHoldInPlaceTime;
        clone.CurrentScreenShakePower = CurrentScreenShakePower;

        return clone;
    }
}

public sealed class TOGlobalNPC : GlobalNPC, ITOLoader
{
    public override bool InstancePerEntity => true;

    /// <summary>
    /// 标识符分配器。
    /// </summary>
    private static long _identifierAllocator;

    private long? _identifier;

    /// <summary>
    /// NPC的标识符。
    /// <br/>不同步。
    /// </summary>
    public long Identifier => _identifier ??= ++_identifierAllocator;

    /// <summary>
    /// NPC生成时 <see cref="TOMain.GameTimer"/> 的值。
    /// <br/>不同步。
    /// </summary>
    internal int SpawnTime = -1;

    public readonly List<AfterimageParticle> Afterimages = [];

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    internal readonly Union32[] OceanAI32 = new Union32[AISlot];
    internal readonly Union64[] OceanAI64 = new Union64[AISlot2];

    internal ref BitArray32 AIChanged32 => ref OceanAI32[^1].bits;
    internal ref BitArray64 AIChanged64 => ref OceanAI64[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        TOGlobalNPC clone = (TOGlobalNPC)base.Clone(from, to);

        clone.SpawnTime = SpawnTime;
        Array.Copy(OceanAI32, clone.OceanAI32, AISlot);
        Array.Copy(OceanAI64, clone.OceanAI64, AISlot2);

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TOSharedData.SyncEnabled)
            return;

        TONetUtils.WriteChangedAI32(binaryWriter, OceanAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, OceanAI64, 1);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOSharedData.SyncEnabled)
            return;

        TONetUtils.ReadChangedAI32(binaryReader, OceanAI32);
        TONetUtils.ReadChangedAI64(binaryReader, OceanAI64);
    }

    void ITOLoader.Load() => _identifierAllocator = 0;

    void ITOLoader.Unload() => _identifierAllocator = 0;
}

public sealed class TOGlobalProjectile : GlobalProjectile
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

        TONetUtils.WriteChangedAI32(binaryWriter, OceanAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, OceanAI64, 1);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOSharedData.SyncEnabled)
            return;

        TONetUtils.ReadChangedAI32(binaryReader, OceanAI32);
        TONetUtils.ReadChangedAI64(binaryReader, OceanAI64);
    }
}

public sealed class TOGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public override GlobalItem Clone(Item from, Item to)
    {
        TOGlobalItem clone = (TOGlobalItem)base.Clone(from, to);

        clone.Equip = Equip;
        clone.Equip_Timer = Equip_Timer;

        return clone;
    }

    internal HysteresisBoolean Equip;
    internal SwitchTimer Equip_Timer;

    /// <summary>
    /// 获取一个物品的装备时长。
    /// </summary>
    /// <param name="max"></param>
    /// <returns>装备时长。
    /// <br/>在物品装备时，返回值从0逐渐增加至max；未装备时，从max逐渐减少至0。
    /// </returns>
    public int GetEquippedTimer(int max) => Equip_Timer.GetValue(TOSharedData.GameTimer.TotalTicks, max);
}
