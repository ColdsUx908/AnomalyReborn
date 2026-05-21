// Developed by ColdsUx

using System.ComponentModel;
using MonoMod.RuntimeDetour;
using Terraria.Graphics.Effects;
using Transoceanic.Framework.RuntimeEditing;

namespace Transoceanic.Framework.Abstractions;

#region Detour
/// <summary>
/// 标记一个方法为 Detour 的目标源方法，并指定其所在的类型、名称和绑定标志。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourSourceAttribute : Attribute
{
    /// <summary>
    /// 源方法所在的目标类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 源方法的名称。
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 用于反射查找源方法的绑定标志。
    /// </summary>
    public readonly BindingFlags BindingAttr;

    /// <summary>
    /// 源方法的参数类型数组（可选，用于区分重载）。
    /// </summary>
    public readonly Type[] ParameterTypes;

    /// <summary>
    /// 初始化 <see cref="CustomDetourSourceAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceType">源方法所在的目标类型。</param>
    /// <param name="name">源方法的名称。</param>
    /// <param name="bindingAttr">反射绑定时使用的标志。</param>
    /// <param name="parameterTypes">参数类型数组，若为 <see langword="null"/> 则忽略参数类型进行匹配。</param>
    public CustomDetourSourceAttribute(Type sourceType, string name, BindingFlags bindingAttr, Type[] parameterTypes = null)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentException.ThrowIfNullOrEmpty(name);
        SourceType = sourceType;
        Name = name;
        BindingAttr = bindingAttr;
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// 通过反射获取该特性指定的源方法信息。
    /// </summary>
    public MethodInfo Source =>
        ParameterTypes is not null ? SourceType.GetMethod(Name, BindingAttr, ParameterTypes) : SourceType.GetMethod(Name, BindingAttr);
}

/// <summary>
/// 为 Detour 方法指定自定义名称前缀，用于自动解析源方法名称。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourPrefixAttribute : Attribute
{
    /// <summary>
    /// 自定义的前缀字符串。
    /// </summary>
    public readonly string Prefix;

    /// <summary>
    /// 初始化 <see cref="CustomDetourPrefixAttribute"/> 的新实例。
    /// </summary>
    /// <param name="prefix">要使用的前缀。</param>
    public CustomDetourPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

/// <summary>
/// 为 Detour 方法提供配置信息，例如 ID、优先级、前置/后置依赖。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomDetourConfigAttribute : Attribute
{
    /// <summary>
    /// Detour 的唯一标识符。
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// Detour 的优先级，数值越大越先应用。
    /// </summary>
    public readonly int? Priority;

    /// <summary>
    /// 必须在此 Detour 之前应用的其他 Detour 的 ID 列表。
    /// </summary>
    public readonly string[] Before;

    /// <summary>
    /// 必须在此 Detour 之后应用的其他 Detour 的 ID 列表。
    /// </summary>
    public readonly string[] After;

    /// <summary>
    /// 内部使用的子优先级。
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly int SubPriority;

    /// <summary>
    /// 根据当前特性生成对应的 <see cref="MonoMod.RuntimeDetour.DetourConfig"/> 实例。
    /// </summary>
    public DetourConfig DetourConfig => new(Id, Priority, Before, After, SubPriority);

    /// <summary>
    /// 初始化 <see cref="CustomDetourConfigAttribute"/> 的新实例。
    /// </summary>
    /// <param name="id">Detour 的唯一标识符。</param>
    public CustomDetourConfigAttribute(string id) => Id = id ?? throw new ArgumentNullException(nameof(id));
}

/// <summary>
/// 标记一个类，表明该类中的所有静态方法都将尝试作为指定类型的 Detour 方法。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 初始化 <see cref="DetourClassToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceType">目标源类型。</param>
    public DetourClassToAttribute(Type sourceType) => SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
}

/// <summary>
/// 标记一个类，表明该类中的所有静态方法都可能作为多个源类型之一的 Detour 方法，通过方法名前缀中的类型名区分。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DetourClassTo_MultiSourceAttribute : Attribute
{
    /// <summary>
    /// 所有可能的源类型数组。
    /// </summary>
    public readonly Type[] SourceTypes;

    /// <summary>
    /// 初始化 <see cref="DetourClassTo_MultiSourceAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceTypes">源类型数组，不能为空且不能包含空元素。</param>
    public DetourClassTo_MultiSourceAttribute(params Type[] sourceTypes)
    {
        ArgumentException.ThrowIfNullOrEmptyOrAnyNull(sourceTypes);
        SourceTypes = sourceTypes;
    }
}

/// <summary>
/// 标记一个不在 Detour 类中的方法，指明其要 Detour 的目标源类型。
/// </summary>
/// <remarks>
/// 注意：在已由 <see cref="DetourClassToAttribute"/> 标记的类中使用此特性可能导致 Detour 重复应用。
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DetourMethodToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 获取或设置参数偏移量。若为负数则禁用参数偏移机制。
    /// 用于指示目标方法的第一个参数在 Detour 方法中的索引位置。
    /// </summary>
    public int ParamOffset { get; init; } = -1;

    /// <summary>
    /// 初始化 <see cref="DetourMethodToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="targetType">目标源类型。</param>
    public DetourMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

/// <summary>
/// 用于标记一个方法，使其在自动 Detour 应用逻辑中被忽略。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NotDetourMethodAttribute : Attribute;

/// <summary>
/// 实现自定义 Detour 逻辑的提供程序接口。
/// </summary>
public interface ITODetourProvider
{
    /// <summary>
    /// 在此方法中应用 Detour 逻辑。
    /// 应使用 <see cref="TODetourHandler.Modify{TDelegate}(MethodBase, TDelegate)"/> 或类似方法注册 Detour，
    /// 以便框架自动管理 Detour 的加载与卸载。
    /// </summary>
    public abstract void ApplyDetour();

    /// <summary>
    /// 获取 Detour 的加载优先级。数值越大，越早加载。
    /// </summary>
    public virtual decimal LoadPriority => 0m;
}

#region 特定类型 Detour
/// <summary>
/// 为指定类型 <typeparamref name="T"/> 提供 Detour 逻辑的抽象基类。
/// 实现 <see cref="ITODetourProvider"/> 接口，在满足 <see cref="ShouldApplyDetour"/> 条件时将已注册的 Detour 应用到目标类型上。
/// </summary>
/// <typeparam name="T">要应用 Detour 的目标类型。</typeparam>
/// <remarks>
/// 派生类可以重写 <see cref="ShouldApplyDetour"/> 属性来控制是否应用 Detour，
/// 并通过重写 <see cref="ApplyDetour"/> 方法或在其中调用 <see cref="ApplySingleDetour{TDelegate}"/> 来注册具体的 Detour 逻辑。
/// <para>
/// 当此类作为 <see cref="ITODetourProvider"/> 使用时，框架会调用显式接口实现 <see cref="ITODetourProvider.ApplyDetour"/>，
/// 该方法内部将检查 <see cref="ShouldApplyDetour"/>，若为 <see langword="true"/> 则执行 <see cref="ApplyDetour"/>。
/// </para>
/// </remarks>
public abstract class TypeDetour<T> : ITODetourProvider
{
    /// <summary>
    /// 获取当前 Detour 提供者所针对的源类型。
    /// 固定为 <c>typeof(T)</c>，用于所有后续 Detour 操作的目标类型识别。
    /// </summary>
    public static readonly Type SourceType = typeof(T);

    /// <inheritdoc />
    /// <remarks>
    /// 该方法是 <see cref="ITODetourProvider.ApplyDetour"/> 的显式接口实现，
    /// 用于统一触发 Detour 应用流程。它会先评估 <see cref="ShouldApplyDetour"/> 属性，
    /// 仅在返回 <see langword="true"/> 时调用可重写的 <see cref="ApplyDetour"/> 方法。
    /// </remarks>
    void ITODetourProvider.ApplyDetour()
    {
        if (ShouldApplyDetour)
            ApplyDetour();
    }

    /// <summary>
    /// 获取一个值，该值指示是否应该应用 Detour。
    /// 默认返回 <see langword="true"/>。派生类可以重写此属性以实现条件应用。
    /// </summary>
    /// <value>
    /// 如果应该应用 Detour，则为 <see langword="true"/>；否则为 <see langword="false"/>。
    /// </value>
    public virtual bool ShouldApplyDetour => true;

    /// <summary>
    /// 应用所有针对类型 <typeparamref name="T"/> 的 Detour。
    /// 派生类应重写此方法，并在其中调用 <see cref="ApplySingleDetour{TDelegate}"/> 等方法注册具体的 Hook。
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ITODetourProvider.ApplyDetour" path="/summary"/>
    /// <para>
    /// 默认实现为空操作。如果需要重载方法，应使用 <c>Detour_{methodName}__{paramNames}</c> 格式的方法名来定义委托方法，
    /// 该命名规则由 <see cref="TODetourHandler.EvaluateDetourName(System.Reflection.MethodInfo, out string)"/> 解析。
    /// </para>
    /// </remarks>
    public virtual void ApplyDetour() { }

    /// <summary>
    /// 尝试将一个由委托表示的具体 Detour 应用到 <typeparamref name="T"/> 类型的目标方法上。
    /// </summary>
    /// <typeparam name="TDelegate">委托类型，必须与目标方法签名匹配。</typeparam>
    /// <param name="detour">
    /// 表示 Detour 逻辑的委托实例。该委托必须是一个由当前类型定义的具名方法，
    /// 且其方法名必须符合 <see cref="TODetourHandler.EvaluateDetourName(System.Reflection.MethodInfo, out string)"/> 的解析规则
    /// （通常为 <c>Detour_{方法名}__{参数类型简短名}</c> 格式）。
    /// </param>
    /// <param name="hasThis">
    /// <para>指示目标方法是否为实例方法（即是否包含 <see langword="this"/> 指针）。</para>
    /// <para>
    /// 该值会影响获取目标方法时的参数偏移量：
    /// 当 <paramref name="hasThis"/> 为 <see langword="true"/> 时，实例方法的第一个隐式参数（this）会使参数偏移量变为 2；
    /// 若为 <see langword="false"/>（静态方法），参数偏移量为 1。
    /// 同时它也会影响内部使用的绑定标志（<c>bindingAttr</c>）。
    /// </para>
    /// </param>
    /// <returns>
    /// 如果 Detour 成功应用，返回对应的 <see cref="Hook"/> 实例；
    /// 如果委托方法不满足条件（如委托未由当前类型声明、方法名无法解析出有效源方法名等），则返回 <see langword="null"/>。
    /// </returns>
    /// <remarks>
    /// 此方法的默认实现首先检查 <paramref name="detour"/> 是否确在当前类型声明，
    /// 然后尝试通过 <see cref="TODetourHandler.EvaluateDetourName(MethodInfo, out string)"/> 
    /// 从委托方法名中解析出目标源方法的名称（<paramref name="sourceName"/>）。
    /// 如果两者均满足，则调用 <see cref="TODetourHandler.Modify"/> 执行实际的 Detour 应用，并返回生成的 <see cref="Hook"/>；
    /// 否则返回 <see langword="null"/>，表示未执行任何操作。
    /// <para>
    /// 使用此方法的派生类通常应在 <see cref="ApplyDetour"/> 重写中进行调用。
    /// </para>
    /// </remarks>
    protected virtual Hook ApplySingleDetour<TDelegate>(TDelegate detour, bool hasThis = true) where TDelegate : Delegate =>
        detour.Method.DeclaringType == GetType() && TODetourHandler.EvaluateDetourName(detour.Method, out string sourceName)
        ? TODetourHandler.Modify(SourceType, sourceName, hasThis, detour)
        : null;
}

//该类不使用源生成器
public abstract class GlobalTypeDetour<TEntity, TGlobal, TGlobalType> : ModTypeDetour<TGlobalType>
    where TEntity : IEntityWithGlobals<TGlobal>
    where TGlobal : GlobalType<TEntity, TGlobal>
    where TGlobalType : TGlobal
{
    // get_IsCloneable
    public delegate bool Orig_get_IsCloneable(TGlobalType self);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.IsCloneable"/>
    public virtual bool Detour_get_IsCloneable(Orig_get_IsCloneable orig, TGlobalType self) => orig(self);

    // get_CloneNewInstances
    public delegate bool Orig_get_CloneNewInstances(TGlobalType self);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.CloneNewInstances"/>
    public virtual bool Detour_get_CloneNewInstances(Orig_get_CloneNewInstances orig, TGlobalType self) => orig(self);

    // AppliesToEntity
    public delegate bool Orig_AppliesToEntity(TGlobalType self, TEntity entity, bool lateInstantiation);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/>
    public virtual bool Detour_AppliesToEntity(Orig_AppliesToEntity orig, TGlobalType self, TEntity entity, bool lateInstantiation) => orig(self, entity, lateInstantiation);

    // SetDefaults
    public delegate void Orig_SetDefaults(TGlobalType self, TEntity entity);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.SetDefaults"/>
    public virtual void Detour_SetDefaults(Orig_SetDefaults orig, TGlobalType self, TEntity entity) => orig(self, entity);

#nullable enable
    // Clone
    public delegate TGlobal Orig_Clone(TGlobalType self, TEntity? from, TEntity to);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.Clone"/>
    public virtual TGlobal Detour_Clone(Orig_Clone orig, TGlobalType self, TEntity? from, TEntity to) => orig(self, from, to);

    // NewInstance
    public delegate TGlobal? Orig_NewInstance(TGlobalType self, TEntity target);
    /// <inheritdoc cref="GlobalType{TEntity, TGlobal}.NewInstance"/>
    public virtual TGlobal? Detour_NewInstance(Orig_NewInstance orig, TGlobalType self, TEntity target) => orig(self, target);
#nullable restore

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        ApplySingleDetour(Detour_get_IsCloneable);
        ApplySingleDetour(Detour_get_CloneNewInstances);
        ApplySingleDetour(Detour_AppliesToEntity);
        ApplySingleDetour(Detour_SetDefaults);
        ApplySingleDetour(Detour_Clone);
        ApplySingleDetour(Detour_NewInstance);
    }
}

#region 源生成器生成
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

    partial void ApplyExtraDetour()
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

    partial void ApplyExtraDetour()
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
#endregion 源生成器生成
#endregion 特定类型 Detour

#endregion Detour

#region IL 编辑
/// <summary>
/// 为 IL 编辑方法指定自定义名称前缀，用于自动解析目标方法名称。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomManipulatorPrefixAttribute : Attribute
{
    /// <summary>
    /// 自定义的前缀字符串。
    /// </summary>
    public readonly string Prefix;

    /// <summary>
    /// 初始化 <see cref="CustomManipulatorPrefixAttribute"/> 的新实例。
    /// </summary>
    /// <param name="prefix">要使用的前缀。</param>
    public CustomManipulatorPrefixAttribute(string prefix) => Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
}

/// <summary>
/// 标记一个不在 IL 编辑类中的方法，指明其要编辑 IL 的目标源类型。
/// </summary>
/// <remarks>
/// 注意：在已由类似 <c>ILEditingClassToAttribute</c> 特性标记的类中使用此特性可能导致 IL 编辑方法重复应用。
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ILEditingMethodToAttribute : Attribute
{
    /// <summary>
    /// 目标源类型。
    /// </summary>
    public readonly Type SourceType;

    /// <summary>
    /// 初始化 <see cref="ILEditingMethodToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="targetType">目标源类型。</param>
    public ILEditingMethodToAttribute(Type targetType) => SourceType = targetType ?? throw new ArgumentNullException(nameof(targetType));
}
#endregion IL 编辑