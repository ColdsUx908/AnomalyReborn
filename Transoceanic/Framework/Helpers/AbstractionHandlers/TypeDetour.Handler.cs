// Developed by ColdsUx

using Terraria.Graphics.Effects;
using Transoceanic.Framework.RuntimeEditing;

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 用于检测类型 Detour 覆盖完整性的更新提醒器。
/// <para>在游戏开始时检查所有 Mod 类型与对应 Detour 类型之间的虚方法匹配情况，若缺失则输出警告。</para>
/// </summary>
internal sealed class TypeDetourUpdateReminder : IUpdateReminder
{
    private static bool DefaultDetourMatch(MethodInfo source, MethodInfo detour) => source.Name == detour.Name;
    private static bool DetourMatch(MethodInfo source, MethodInfo detour) => TODetourHandler.EvaluateDetourName(detour, out string sourceNameGot) && sourceNameGot == source.Name;
    private static bool ShouldMethodBeChecked(MethodInfo method) => method.IsRealVirtualOrAbstract && !method.IsGenericMethod && !method.HasAttribute<ObsoleteAttribute>() && method.CanBeAccessedOutsideAssembly;

    /// <summary>
    /// 表示一个需要检查的 Detour 类型配对。
    /// </summary>
    /// <param name="Source">原始 Mod 类型。</param>
    /// <param name="Detour">对应的 Detour 类型。</param>
    /// <param name="SourceIgnore">可选的源方法忽略过滤器。</param>
    /// <param name="DetourIgnore">可选的 Detour 方法忽略过滤器。</param>
    private readonly record struct DetourTypeContainer(Type Source, Type Detour, Func<MethodInfo, bool> SourceIgnore = null, Func<MethodInfo, bool> DetourIgnore = null)
    {
        /// <summary>
        /// 比较源类型与 Detour 类型的虚方法列表，返回缺失的方法名称集合。
        /// </summary>
        /// <param name="match">用于判断两个方法是否对应的匹配逻辑。</param>
        /// <returns>包含源缺失方法和 Detour 缺失方法的元组。</returns>
        public (List<string> sourceMissing, List<string> targetMissing) CompareVirtualMethods(Func<MethodInfo, MethodInfo, bool> match)
        {
            match ??= DefaultDetourMatch;
            List<string> sourceMissing = [];
            List<string> detourMissing = [];
            IEnumerable<MethodInfo> sourceMethods = Source.GetRealMethods(TOReflectionUtils.InstanceBindingFlags).Where(ShouldMethodBeChecked);
            IEnumerable<MethodInfo> detourMethods = Detour.GetRealMethods(TOReflectionUtils.InstanceBindingFlags).Where(ShouldMethodBeChecked);
            foreach (MethodInfo sourceMethod in sourceMethods)
            {
                if (SourceIgnore?.Invoke(sourceMethod) ?? false)
                    continue;
                if (!detourMethods.Any(m => match(sourceMethod, m)))
                    detourMissing.Add(sourceMethod.Name);
            }
            foreach (MethodInfo targetMethod in detourMethods)
            {
                if (DetourIgnore?.Invoke(targetMethod) ?? false)
                    continue;
                if (!sourceMethods.Any(m => match(m, targetMethod)))
                    sourceMissing.Add(targetMethod.Name);
            }
            return (sourceMissing, detourMissing);
        }
    }

    /// <summary>
    /// 注册更新提醒回调：检查所有关键类型对的 Detour 完整性，若有缺失则记录警告并在聊天中提示。
    /// </summary>
    Action IUpdateReminder.RegisterUpdateReminder()
    {
        bool hasWarn = false;

        StringBuilder builder = new();
        builder.AppendLine("Update Required: TypeDetour.cs");

        ReadOnlySpan<DetourTypeContainer> typesToCheck =
        [
            new(typeof(ModType), typeof(ModTypeDetour<>)),
            new(typeof(ModAccessorySlot), typeof(ModAccessorySlotDetour<>)),
            new(typeof(ModBannerTile), typeof(ModBannerTileDetour<>)),
            new(typeof(ModBiome), typeof(ModBiomeDetour<>)),
            new(typeof(ModBiomeConversion), typeof(ModBiomeConversionDetour<>)),
            new(typeof(ModBlockType), typeof(ModBlockTypeDetour<>)),
            new(typeof(ModBossBar), typeof(ModBossBarDetour<>)),
            new(typeof(ModBossBarStyle), typeof(ModBossBarStyleDetour<>)),
            new(typeof(ModBuff), typeof(ModBuffDetour<>)),
            new(typeof(ModCactus), typeof(ModCactusDetour<>)),
            new(typeof(ModCloud), typeof(ModCloudDetour<>)),
            new(typeof(ModCommand), typeof(ModCommandDetour<>)),
            new(typeof(ModDust), typeof(ModDustDetour<>)),
            new(typeof(ModEmoteBubble), typeof(ModEmoteBubbleDetour<>)),
            new(typeof(ModGore), typeof(ModGoreDetour<>)),
            new(typeof(ModHair), typeof(ModHairDetour<>)),
            new(typeof(ModItem), typeof(ModItemDetour<>)),
            new(typeof(ModMapLayer), typeof(ModMapLayerDetour<>)),
            new(typeof(ModMenu), typeof(ModMenuDetour<>)),
            new(typeof(ModMount), typeof(ModMountDetour<>)),
            new(typeof(ModNPC), typeof(ModNPCDetour<>)),
            new(typeof(ModPalmTree), typeof(ModPalmTreeDetour<>)),
            new(typeof(ModPlayer), typeof(ModPlayerDetour<>)),
            new(typeof(ModPrefix), typeof(ModPrefixDetour<>)),
            new(typeof(ModProjectile), typeof(ModProjectileDetour<>)),
            new(typeof(ModPylon), typeof(ModPylonDetour<>)),
            new(typeof(ModRarity), typeof(ModRarityDetour<>)),
            new(typeof(ModResourceDisplaySet), typeof(ModResourceDisplaySetDetour<>)),
            new(typeof(ModResourceOverlay), typeof(ModResourceOverlayDetour<>)),
            new(typeof(ModSceneEffect), typeof(ModSceneEffectDetour<>)),
            new(typeof(ModSurfaceBackgroundStyle), typeof(ModSurfaceBackgroundStyleDetour<>)),
            new(typeof(ModSystem), typeof(ModSystemDetour<>)),
            new(typeof(ModTexturedType), typeof(ModTexturedTypeDetour<>)),
            new(typeof(ModTile), typeof(ModTileDetour<>)),
            new(typeof(ModTileEntity), typeof(ModTileEntityDetour<>)),
            new(typeof(ModTree), typeof(ModTreeDetour<>)),
            new(typeof(ModUndergroundBackgroundStyle), typeof(ModUndergroundBackgroundStyleDetour<>)),
            new(typeof(ModWall), typeof(ModWallDetour<>)),
            new(typeof(ModWaterfallStyle), typeof(ModWaterfallStyleDetour<>)),
            new(typeof(ModWaterStyle), typeof(ModWaterStyleDetour<>)),
            new(typeof(GlobalType<,>), typeof(GlobalTypeDetour<,,>)),
            new(typeof(GlobalBlockType), typeof(GlobalBlockTypeDetour<>)),
            new(typeof(GlobalBossBar), typeof(GlobalBossBarDetour<>)),
            new(typeof(GlobalBuff), typeof(GlobalBuffDetour<>)),
            new(typeof(GlobalEmoteBubble), typeof(GlobalEmoteBubbleDetour<>)),
            new(typeof(GlobalInfoDisplay), typeof(GlobalInfoDisplayDetour<>)),
            new(typeof(GlobalItem), typeof(GlobalItemDetour<>)),
            new(typeof(GlobalNPC), typeof(GlobalNPCDetour<>)),
            new(typeof(GlobalProjectile), typeof(GlobalProjectileDetour<>)),
            new(typeof(GlobalPylon), typeof(GlobalPylonDetour<>)),
            new(typeof(GlobalTile), typeof(GlobalTileDetour<>)),
            new(typeof(GlobalWall), typeof(GlobalWallDetour<>)),
            new(typeof(GameEffect), typeof(GameEffectDetour<>)),
            new(typeof(CustomSky), typeof(CustomSkyDetour<>))
        ];

        foreach (DetourTypeContainer container in typesToCheck)
        {
            (List<string> sourceMissing, List<string> detourMissing) = container.CompareVirtualMethods(DetourMatch);
            if (sourceMissing.Count > 0)
            {
                hasWarn = true;
                builder.AppendLine(new string(' ', 3) + $"[{container.Detour.RealName}] Source type method missing: " + string.Join(", ", sourceMissing));
            }
            if (detourMissing.Count > 0)
            {
                hasWarn = true;
                builder.AppendLine(new string(' ', 3) + $"[{container.Detour.RealName}] Detour type method missing: " + string.Join(", ", detourMissing));
            }
        }

        if (hasWarn)
        {
            TOMain.Instance.Logger.Warn(builder.ToString());
            return () => TOLocalizationUtils.ChatLiteralText("TypeDetour.cs", TOSharedData.TODebugWarnColor, Main.LocalPlayer);
        }
        else
            return null;
    }
}