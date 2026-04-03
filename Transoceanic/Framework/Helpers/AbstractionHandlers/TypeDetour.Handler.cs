using Terraria.Graphics.Effects;
using Transoceanic.Framework.RuntimeEditing;

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

internal sealed class TypeDetourUpdateReminder : IUpdateReminder
{
    private static bool DefaultDetourMatch(MethodInfo source, MethodInfo detour) => source.Name == detour.Name;
    private static bool DetourMatch(MethodInfo source, MethodInfo detour) => TODetourHandler.EvaluateDetourName(detour, out string sourceNameGot) && sourceNameGot == source.Name;
    private static bool ShouldMethodBeChecked(MethodInfo method) => method.IsRealVirtualOrAbstract && !method.IsGenericMethod && !method.HasAttribute<ObsoleteAttribute>() && method.CanBeAccessedOutsideAssembly;

    private readonly record struct DetourTypeContainer(Type Source, Type Detour, Func<MethodInfo, bool> SourceIgnore = null, Func<MethodInfo, bool> DetourIgnore = null)
    {
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