using Terraria.Graphics.Effects;

namespace Transoceanic.Runtime.Detours;

[TypeDetourClass(typeof(ModType))]
public abstract partial class ModTypeDetour<T> : TypeDetour<T> where T : ModType;

[TypeDetourClass(typeof(ModAccessorySlot))]
public abstract partial class ModAccessorySlotDetour<T> : ModTypeDetour<T> where T : ModAccessorySlot;

[TypeDetourClass(typeof(ModBannerTile))]
public abstract partial class ModBannerTileDetour<T> : ModTileDetour<T> where T : ModBannerTile;

[TypeDetourClass(typeof(ModBiome))]
public abstract partial class ModBiomeDetour<T> : ModSceneEffectDetour<T> where T : ModBiome;

[TypeDetourClass(typeof(ModBiomeConversion))]
public abstract partial class ModBiomeConversionDetour<T> : ModTypeDetour<T> where T : ModBiomeConversion;

[TypeDetourClass(typeof(ModBlockType))]
public abstract partial class ModBlockTypeDetour<T> : ModTexturedTypeDetour<T> where T : ModBlockType;

[TypeDetourClass(typeof(ModBossBar))]
public abstract partial class ModBossBarDetour<T> : ModTexturedTypeDetour<T> where T : ModBossBar;

[TypeDetourClass(typeof(ModBossBarStyle))]
public abstract partial class ModBossBarStyleDetour<T> : ModTypeDetour<T> where T : ModBossBarStyle;

[TypeDetourClass(typeof(ModBuff))]
public abstract partial class ModBuffDetour<T> : ModTexturedTypeDetour<T> where T : ModBuff
{
    // Update (Player)
    public delegate void Orig_Update__Player(T self, Player player, ref int buffIndex);
    /// <inheritdoc cref="ModBuff.Update(Player, ref int)"/>
    public virtual void Detour_Update__Player(Orig_Update__Player orig, T self, Player player, ref int buffIndex) => orig(self, player, ref buffIndex);

    // Update (NPC)
    public delegate void Orig_Update__NPC(T self, NPC npc, ref int buffIndex);
    /// <inheritdoc cref="ModBuff.Update(NPC, ref int)"/>
    public virtual void Detour_Update__NPC(Orig_Update__NPC orig, T self, NPC npc, ref int buffIndex) => orig(self, npc, ref buffIndex);

    // ReApply (Player)
    public delegate bool Orig_ReApply__Player(T self, Player player, int time, int buffIndex);
    /// <inheritdoc cref="ModBuff.ReApply(Player, int, int)"/>
    public virtual bool Detour_ReApply__Player(Orig_ReApply__Player orig, T self, Player player, int time, int buffIndex) => orig(self, player, time, buffIndex);

    // ReApply (NPC)
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

[TypeDetourClass(typeof(ModCactus))]
public abstract partial class ModCactusDetour<T> : TypeDetour<T> where T : ModCactus;

[TypeDetourClass(typeof(ModCloud))]
public abstract partial class ModCloudDetour<T> : ModTexturedTypeDetour<T> where T : ModCloud;

[TypeDetourClass(typeof(ModCommand))]
public abstract partial class ModCommandDetour<T> : ModTypeDetour<T> where T : ModCommand;

[TypeDetourClass(typeof(ModDust))]
public abstract partial class ModDustDetour<T> : ModTexturedTypeDetour<T> where T : ModDust;

[TypeDetourClass(typeof(ModEmoteBubble))]
public abstract partial class ModEmoteBubbleDetour<T> : ModTypeDetour<T> where T : ModEmoteBubble;

[TypeDetourClass(typeof(ModGore))]
public abstract partial class ModGoreDetour<T> : ModTexturedTypeDetour<T> where T : ModGore;

[TypeDetourClass(typeof(ModHair))]
public abstract partial class ModHairDetour<T> : ModTexturedTypeDetour<T> where T : ModHair;

[TypeDetourClass(typeof(ModItem))]
public abstract partial class ModItemDetour<T> : ModTypeDetour<T> where T : ModItem;

[TypeDetourClass(typeof(ModMapLayer))]
public abstract partial class ModMapLayerDetour<T> : ModTypeDetour<T> where T : ModMapLayer;

[TypeDetourClass(typeof(ModMenu))]
public abstract partial class ModMenuDetour<T> : ModTypeDetour<T> where T : ModMenu;

[TypeDetourClass(typeof(ModMount))]
public abstract partial class ModMountDetour<T> : ModTypeDetour<T> where T : ModMount;

[TypeDetourClass(typeof(ModNPC))]
public abstract partial class ModNPCDetour<T> : ModTypeDetour<T> where T : ModNPC;

[TypeDetourClass(typeof(ModPalmTree))]
public abstract partial class ModPalmTreeDetour<T> : TypeDetour<T> where T : ModPalmTree;

[TypeDetourClass(typeof(ModPlayer))]
public abstract partial class ModPlayerDetour<T> : ModTypeDetour<T> where T : ModPlayer;

[TypeDetourClass(typeof(ModPrefix))]
public abstract partial class ModPrefixDetour<T> : ModTypeDetour<T> where T : ModPrefix;

[TypeDetourClass(typeof(ModProjectile))]
public abstract partial class ModProjectileDetour<T> : ModTypeDetour<T> where T : ModProjectile;

[TypeDetourClass(typeof(ModPylon))]
public abstract partial class ModPylonDetour<T> : ModTileDetour<T> where T : ModPylon;

[TypeDetourClass(typeof(ModRarity))]
public abstract partial class ModRarityDetour<T> : ModTypeDetour<T> where T : ModRarity;

[TypeDetourClass(typeof(ModResourceDisplaySet))]
public abstract partial class ModResourceDisplaySetDetour<T> : ModTypeDetour<T> where T : ModResourceDisplaySet;

[TypeDetourClass(typeof(ModResourceOverlay))]
public abstract partial class ModResourceOverlayDetour<T> : ModTypeDetour<T> where T : ModResourceOverlay;

[TypeDetourClass(typeof(ModSceneEffect))]
public abstract partial class ModSceneEffectDetour<T> : ModTypeDetour<T> where T : ModSceneEffect;

[TypeDetourClass(typeof(ModSurfaceBackgroundStyle))]
public abstract partial class ModSurfaceBackgroundStyleDetour<T> : ModTypeDetour<T> where T : ModSurfaceBackgroundStyle;

[TypeDetourClass(typeof(ModSystem))]
public abstract partial class ModSystemDetour<T> : ModTypeDetour<T> where T : ModSystem;

[TypeDetourClass(typeof(ModTexturedType))]
public abstract partial class ModTexturedTypeDetour<T> : ModTypeDetour<T> where T : ModTexturedType;

[TypeDetourClass(typeof(ModTile))]
public abstract partial class ModTileDetour<T> : ModBlockTypeDetour<T> where T : ModTile;

[TypeDetourClass(typeof(ModTileEntity))]
public abstract partial class ModTileEntityDetour<T> : TypeDetour<T> where T : ModTileEntity;

[TypeDetourClass(typeof(ModTree))]
public abstract partial class ModTreeDetour<T> : TypeDetour<T> where T : ModTree;

[TypeDetourClass(typeof(ModUndergroundBackgroundStyle))]
public abstract partial class ModUndergroundBackgroundStyleDetour<T> : ModTypeDetour<T> where T : ModUndergroundBackgroundStyle;

[TypeDetourClass(typeof(ModWall))]
public abstract partial class ModWallDetour<T> : ModBlockTypeDetour<T> where T : ModWall;

[TypeDetourClass(typeof(ModWaterfallStyle))]
public abstract partial class ModWaterfallStyleDetour<T> : ModTexturedTypeDetour<T> where T : ModWaterfallStyle;

[TypeDetourClass(typeof(ModWaterStyle))]
public abstract partial class ModWaterStyleDetour<T> : ModTexturedTypeDetour<T> where T : ModWaterStyle;

[TypeDetourClass(typeof(GlobalBlockType))]
public abstract partial class GlobalBlockTypeDetour<T> : ModTypeDetour<T> where T : GlobalBlockType;

[TypeDetourClass(typeof(GlobalBossBar))]
public abstract partial class GlobalBossBarDetour<T> : ModTypeDetour<T> where T : GlobalBossBar;

[TypeDetourClass(typeof(GlobalBuff))]
public abstract partial class GlobalBuffDetour<T> : ModTypeDetour<T> where T : GlobalBuff
{
    // Update (Player)
    public delegate void Orig_Update__Player(T self, int type, Player player, ref int buffIndex);
    /// <inheritdoc cref="GlobalBuff.Update(int, Player, ref int)"/>
    public virtual void Detour_Update__Player(Orig_Update__Player orig, T self, int type, Player player, ref int buffIndex) => orig(self, type, player, ref buffIndex);

    // Update (NPC)
    public delegate void Orig_Update__NPC(T self, int type, NPC npc, ref int buffIndex);
    /// <inheritdoc cref="GlobalBuff.Update(int, NPC, ref int)"/>
    public virtual void Detour_Update__NPC(Orig_Update__NPC orig, T self, int type, NPC npc, ref int buffIndex) => orig(self, type, npc, ref buffIndex);

    // ReApply (Player)
    public delegate bool Orig_ReApply__Player(T self, int type, Player player, int time, int buffIndex);
    /// <inheritdoc cref="GlobalBuff.ReApply(int, Player, int, int)"/>
    public virtual bool Detour_ReApply__Player(Orig_ReApply__Player orig, T self, int type, Player player, int time, int buffIndex) => orig(self, type, player, time, buffIndex);

    // ReApply (NPC)
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

[TypeDetourClass(typeof(GlobalEmoteBubble))]
public abstract partial class GlobalEmoteBubbleDetour<T> : ModTypeDetour<T> where T : GlobalEmoteBubble;

[TypeDetourClass(typeof(GlobalInfoDisplay))]
public abstract partial class GlobalInfoDisplayDetour<T> : ModTypeDetour<T> where T : GlobalInfoDisplay;

[TypeDetourClass(typeof(GlobalItem))]
public abstract partial class GlobalItemDetour<T> : GlobalTypeDetour<Item, GlobalItem, T> where T : GlobalItem;

[TypeDetourClass(typeof(GlobalNPC))]
public abstract partial class GlobalNPCDetour<T> : GlobalTypeDetour<NPC, GlobalNPC, T> where T : GlobalNPC;

[TypeDetourClass(typeof(GlobalProjectile))]
public abstract partial class GlobalProjectileDetour<T> : GlobalTypeDetour<Projectile, GlobalProjectile, T> where T : GlobalProjectile;

[TypeDetourClass(typeof(GlobalPylon))]
public abstract partial class GlobalPylonDetour<T> : ModTypeDetour<T> where T : GlobalPylon;

[TypeDetourClass(typeof(GlobalTile))]
public abstract partial class GlobalTileDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalTile;

[TypeDetourClass(typeof(GlobalWall))]
public abstract partial class GlobalWallDetour<T> : GlobalBlockTypeDetour<T> where T : GlobalWall;

[TypeDetourClass(typeof(GameEffect))]
public abstract partial class GameEffectDetour<T> : TypeDetour<T> where T : GameEffect;

[TypeDetourClass(typeof(CustomSky))]
public abstract partial class CustomSkyDetour<T> : GameEffectDetour<T> where T : CustomSky;
