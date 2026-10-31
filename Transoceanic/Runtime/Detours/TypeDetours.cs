using Terraria.Graphics.Effects;

namespace Transoceanic.Runtime.Detours;

/// <summary>
/// 为 tModLoader 的 <c>ModType</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModType))]
public abstract partial class ModTypeDetour<T> : TypeDetour<T> where T : ModType;

/// <summary>
/// 为 tModLoader 的 <c>ModAccessorySlot</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModAccessorySlot))]
public abstract partial class ModAccessorySlotDetour<T> : ModTypeDetour<T> where T : ModAccessorySlot;

/// <summary>
/// 为 tModLoader 的 <c>ModBannerTile</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBannerTile))]
public abstract partial class ModBannerTileDetour<T> : ModTileDetour<T> where T : ModBannerTile;

/// <summary>
/// 为 tModLoader 的 <c>ModBiome</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBiome))]
public abstract partial class ModBiomeDetour<T> : ModSceneEffectDetour<T> where T : ModBiome;

/// <summary>
/// 为 tModLoader 的 <c>ModBiomeConversion</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBiomeConversion))]
public abstract partial class ModBiomeConversionDetour<T> : ModTypeDetour<T> where T : ModBiomeConversion;

/// <summary>
/// 为 tModLoader 的 <c>ModBlockType</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBlockType))]
public abstract partial class ModBlockTypeDetour<T> : ModTexturedTypeDetour<T> where T : ModBlockType;

/// <summary>
/// 为 tModLoader 的 <c>ModBossBar</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBossBar))]
public abstract partial class ModBossBarDetour<T> : ModTexturedTypeDetour<T> where T : ModBossBar;

/// <summary>
/// 为 tModLoader 的 <c>ModBossBarStyle</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBossBarStyle))]
public abstract partial class ModBossBarStyleDetour<T> : ModTypeDetour<T> where T : ModBossBarStyle;

/// <summary>
/// 为 tModLoader 的 <c>ModBuff</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModBuff))]
public abstract partial class ModBuffDetour<T> : ModTexturedTypeDetour<T> where T : ModBuff
{
    // Update (Player)
    /// <summary>
    /// 声明原始 Update Player 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>void</c>。
    /// </summary>
    public delegate void Orig_Update__Player(T self, Player player, ref int buffIndex);
    /// <inheritdoc cref="ModBuff.Update(Player, ref int)"/>
    public virtual void Detour_Update__Player(Orig_Update__Player orig, T self, Player player, ref int buffIndex) => orig(self, player, ref buffIndex);

    // Update (NPC)
    /// <summary>
    /// 声明原始 Update NPC 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>void</c>。
    /// </summary>
    public delegate void Orig_Update__NPC(T self, NPC npc, ref int buffIndex);
    /// <inheritdoc cref="ModBuff.Update(NPC, ref int)"/>
    public virtual void Detour_Update__NPC(Orig_Update__NPC orig, T self, NPC npc, ref int buffIndex) => orig(self, npc, ref buffIndex);

    // ReApply (Player)
    /// <summary>
    /// 声明原始 ReApply Player 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>bool</c>。
    /// </summary>
    public delegate bool Orig_ReApply__Player(T self, Player player, int time, int buffIndex);
    /// <inheritdoc cref="ModBuff.ReApply(Player, int, int)"/>
    public virtual bool Detour_ReApply__Player(Orig_ReApply__Player orig, T self, Player player, int time, int buffIndex) => orig(self, player, time, buffIndex);

    // ReApply (NPC)
    /// <summary>
    /// 声明原始 ReApply NPC 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>bool</c>。
    /// </summary>
    public delegate bool Orig_ReApply__NPC(T self, NPC npc, int time, int buffIndex);
    /// <inheritdoc cref="ModBuff.ReApply(NPC, int, int)"/>
    public virtual bool Detour_ReApply__NPC(Orig_ReApply__NPC orig, T self, NPC npc, int time, int buffIndex) => orig(self, npc, time, buffIndex);

    unsafe partial void ApplyExtraDetour()
    {
        ApplySingleDetour(Detour_Update__Player);
        ApplySingleDetour(Detour_Update__NPC);
        ApplySingleDetour(Detour_ReApply__Player);
        ApplySingleDetour(Detour_ReApply__NPC);
    }
}

/// <summary>
/// 为 tModLoader 的 <c>ModCactus</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModCactus))]
public abstract partial class ModCactusDetour<T> : TypeDetour<T> where T : ModCactus;

/// <summary>
/// 为 tModLoader 的 <c>ModCloud</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModCloud))]
public abstract partial class ModCloudDetour<T> : ModTexturedTypeDetour<T> where T : ModCloud;

/// <summary>
/// 为 tModLoader 的 <c>ModCommand</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModCommand))]
public abstract partial class ModCommandDetour<T> : ModTypeDetour<T> where T : ModCommand;

/// <summary>
/// 为 tModLoader 的 <c>ModDust</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModDust))]
public abstract partial class ModDustDetour<T> : ModTexturedTypeDetour<T> where T : ModDust;

/// <summary>
/// 为 tModLoader 的 <c>ModEmoteBubble</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModEmoteBubble))]
public abstract partial class ModEmoteBubbleDetour<T> : ModTypeDetour<T> where T : ModEmoteBubble;

/// <summary>
/// 为 tModLoader 的 <c>ModGore</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModGore))]
public abstract partial class ModGoreDetour<T> : ModTexturedTypeDetour<T> where T : ModGore;

/// <summary>
/// 为 tModLoader 的 <c>ModHair</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModHair))]
public abstract partial class ModHairDetour<T> : ModTexturedTypeDetour<T> where T : ModHair;

/// <summary>
/// 为 tModLoader 的 <c>ModItem</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModItem))]
public abstract partial class ModItemDetour<T> : ModTypeDetour<T> where T : ModItem;

/// <summary>
/// 为 tModLoader 的 <c>ModMapLayer</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModMapLayer))]
public abstract partial class ModMapLayerDetour<T> : ModTypeDetour<T> where T : ModMapLayer;

/// <summary>
/// 为 tModLoader 的 <c>ModMenu</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModMenu))]
public abstract partial class ModMenuDetour<T> : ModTypeDetour<T> where T : ModMenu;

/// <summary>
/// 为 tModLoader 的 <c>ModMount</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModMount))]
public abstract partial class ModMountDetour<T> : ModTypeDetour<T> where T : ModMount;

/// <summary>
/// 为 tModLoader 的 <c>ModNPC</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModNPC))]
public abstract partial class ModNPCDetour<T> : ModTypeDetour<T> where T : ModNPC;

/// <summary>
/// 为 tModLoader 的 <c>ModPalmTree</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModPalmTree))]
public abstract partial class ModPalmTreeDetour<T> : TypeDetour<T> where T : ModPalmTree;

/// <summary>
/// 为 tModLoader 的 <c>ModPlayer</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModPlayer))]
public abstract partial class ModPlayerDetour<T> : ModTypeDetour<T> where T : ModPlayer;

/// <summary>
/// 为 tModLoader 的 <c>ModPrefix</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModPrefix))]
public abstract partial class ModPrefixDetour<T> : ModTypeDetour<T> where T : ModPrefix;

/// <summary>
/// 为 tModLoader 的 <c>ModProjectile</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModProjectile))]
public abstract partial class ModProjectileDetour<T> : ModTypeDetour<T> where T : ModProjectile;

/// <summary>
/// 为 tModLoader 的 <c>ModPylon</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModPylon))]
public abstract partial class ModPylonDetour<T> : ModTileDetour<T> where T : ModPylon;

/// <summary>
/// 为 tModLoader 的 <c>ModRarity</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModRarity))]
public abstract partial class ModRarityDetour<T> : ModTypeDetour<T> where T : ModRarity;

/// <summary>
/// 为 tModLoader 的 <c>ModResourceDisplaySet</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModResourceDisplaySet))]
public abstract partial class ModResourceDisplaySetDetour<T> : ModTypeDetour<T> where T : ModResourceDisplaySet;

/// <summary>
/// 为 tModLoader 的 <c>ModResourceOverlay</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModResourceOverlay))]
public abstract partial class ModResourceOverlayDetour<T> : ModTypeDetour<T> where T : ModResourceOverlay;

/// <summary>
/// 为 tModLoader 的 <c>ModSceneEffect</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModSceneEffect))]
public abstract partial class ModSceneEffectDetour<T> : ModTypeDetour<T> where T : ModSceneEffect;

/// <summary>
/// 为 tModLoader 的 <c>ModSurfaceBackgroundStyle</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModSurfaceBackgroundStyle))]
public abstract partial class ModSurfaceBackgroundStyleDetour<T> : ModTypeDetour<T> where T : ModSurfaceBackgroundStyle;

/// <summary>
/// 为 tModLoader 的 <c>ModSystem</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModSystem))]
public abstract partial class ModSystemDetour<T> : ModTypeDetour<T> where T : ModSystem;

/// <summary>
/// 为 tModLoader 的 <c>ModTexturedType</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModTexturedType))]
public abstract partial class ModTexturedTypeDetour<T> : ModTypeDetour<T> where T : ModTexturedType;

/// <summary>
/// 为 tModLoader 的 <c>ModTile</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModTile))]
public abstract partial class ModTileDetour<T> : ModBlockTypeDetour<T> where T : ModTile;

/// <summary>
/// 为 tModLoader 的 <c>ModTileEntity</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModTileEntity))]
public abstract partial class ModTileEntityDetour<T> : TypeDetour<T> where T : ModTileEntity;

/// <summary>
/// 为 tModLoader 的 <c>ModTree</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModTree))]
public abstract partial class ModTreeDetour<T> : TypeDetour<T> where T : ModTree;

/// <summary>
/// 为 tModLoader 的 <c>ModUndergroundBackgroundStyle</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModUndergroundBackgroundStyle))]
public abstract partial class ModUndergroundBackgroundStyleDetour<T> : ModTypeDetour<T> where T : ModUndergroundBackgroundStyle;

/// <summary>
/// 为 tModLoader 的 <c>ModWall</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModWall))]
public abstract partial class ModWallDetour<T> : ModBlockTypeDetour<T> where T : ModWall;

/// <summary>
/// 为 tModLoader 的 <c>ModWaterfallStyle</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModWaterfallStyle))]
public abstract partial class ModWaterfallStyleDetour<T> : ModTexturedTypeDetour<T> where T : ModWaterfallStyle;

/// <summary>
/// 为 tModLoader 的 <c>ModWaterStyle</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(ModWaterStyle))]
public abstract partial class ModWaterStyleDetour<T> : ModTexturedTypeDetour<T> where T : ModWaterStyle;

/// <summary>
/// 为 tModLoader 的 <c>GlobalBlockType</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalBlockType))]
public abstract partial class GlobalBlockTypeDetour<T> : ModTypeDetour<T> where T : GlobalBlockType;

/// <summary>
/// 为 tModLoader 的 <c>GlobalBossBar</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalBossBar))]
public abstract partial class GlobalBossBarDetour<T> : ModTypeDetour<T> where T : GlobalBossBar;

/// <summary>
/// 为 tModLoader 的 <c>GlobalBuff</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalBuff))]
public abstract partial class GlobalBuffDetour<T> : ModTypeDetour<T> where T : GlobalBuff
{
    // Update (Player)
    /// <summary>
    /// 声明原始 Update Player 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>void</c>。
    /// </summary>
    public delegate void Orig_Update__Player(T self, int type, Player player, ref int buffIndex);
    /// <inheritdoc cref="GlobalBuff.Update(int, Player, ref int)"/>
    public virtual void Detour_Update__Player(Orig_Update__Player orig, T self, int type, Player player, ref int buffIndex) => orig(self, type, player, ref buffIndex);

    // Update (NPC)
    /// <summary>
    /// 声明原始 Update NPC 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>void</c>。
    /// </summary>
    public delegate void Orig_Update__NPC(T self, int type, NPC npc, ref int buffIndex);
    /// <inheritdoc cref="GlobalBuff.Update(int, NPC, ref int)"/>
    public virtual void Detour_Update__NPC(Orig_Update__NPC orig, T self, int type, NPC npc, ref int buffIndex) => orig(self, type, npc, ref buffIndex);

    // ReApply (Player)
    /// <summary>
    /// 声明原始 ReApply Player 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>bool</c>。
    /// </summary>
    public delegate bool Orig_ReApply__Player(T self, int type, Player player, int time, int buffIndex);
    /// <inheritdoc cref="GlobalBuff.ReApply(int, Player, int, int)"/>
    public virtual bool Detour_ReApply__Player(Orig_ReApply__Player orig, T self, int type, Player player, int time, int buffIndex) => orig(self, type, player, time, buffIndex);

    // ReApply (NPC)
    /// <summary>
    /// 声明原始 ReApply NPC 回调的委托类型，保存被拦截方法的调用入口，供对应 Detour 在需要时继续执行原逻辑。返回类型为 <c>bool</c>。
    /// </summary>
    public delegate bool Orig_ReApply__NPC(T self, int type, NPC npc, int time, int buffIndex);
    /// <inheritdoc cref="GlobalBuff.ReApply(int, NPC, int, int)"/>
    public virtual bool Detour_ReApply__NPC(Orig_ReApply__NPC orig, T self, int type, NPC npc, int time, int buffIndex) => orig(self, type, npc, time, buffIndex);

    unsafe partial void ApplyExtraDetour()
    {
        ApplySingleDetour(Detour_Update__Player);
        ApplySingleDetour(Detour_Update__NPC);
        ApplySingleDetour(Detour_ReApply__Player);
        ApplySingleDetour(Detour_ReApply__NPC);
    }
}

/// <summary>
/// 为 tModLoader 的 <c>GlobalEmoteBubble</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalEmoteBubble))]
public abstract partial class GlobalEmoteBubbleDetour<T> : ModTypeDetour<T> where T : GlobalEmoteBubble;

/// <summary>
/// 为 tModLoader 的 <c>GlobalInfoDisplay</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalInfoDisplay))]
public abstract partial class GlobalInfoDisplayDetour<T> : ModTypeDetour<T> where T : GlobalInfoDisplay;

/// <summary>
/// 为 tModLoader 的 <c>GlobalItem</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalItem))]
public abstract partial class GlobalItemDetour<T> : GlobalTypeDetour<Item, GlobalItem, T> where T : GlobalItem;

/// <summary>
/// 为 tModLoader 的 <c>GlobalNPC</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalNPC))]
public abstract partial class GlobalNPCDetour<T> : GlobalTypeDetour<NPC, GlobalNPC, T> where T : GlobalNPC;

/// <summary>
/// 为 tModLoader 的 <c>GlobalProjectile</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalProjectile))]
public abstract partial class GlobalProjectileDetour<T> : GlobalTypeDetour<Projectile, GlobalProjectile, T> where T : GlobalProjectile;

/// <summary>
/// 为 tModLoader 的 <c>GlobalPylon</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalPylon))]
public abstract partial class GlobalPylonDetour<T> : ModTypeDetour<T> where T : GlobalPylon;

/// <summary>
/// 为 tModLoader 的 <c>GlobalTile</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalTile))]
public abstract partial class GlobalTileDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalTile;

/// <summary>
/// 为 tModLoader 的 <c>GlobalWall</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GlobalWall))]
public abstract partial class GlobalWallDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalWall;

/// <summary>
/// 为 tModLoader 的 <c>GameEffect</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(GameEffect))]
public abstract partial class GameEffectDetour<T> : TypeDetour<T> where T : GameEffect;

/// <summary>
/// 为 tModLoader 的 <c>CustomSky</c> 类型提供可继承的运行时 Detour 扩展点；特性将该基类关联到目标类型，派生类可覆盖支持的回调以插入自定义逻辑。
/// </summary>
[TypeDetourClass(typeof(CustomSky))]
public abstract partial class CustomSkyDetour<T> : GameEffectDetour<T> where T : CustomSky;
