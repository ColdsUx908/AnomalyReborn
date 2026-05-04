// Developed by ColdsUx

//不全局引用任何灾厄命名空间

global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using CalamityAnomalies.Assets;
global using CalamityAnomalies.Common;
global using CalamityAnomalies.Core;
global using CalamityAnomalies.ModCompatibility;
global using CalamityAnomalies.ModCompatibility.CalamityBridge;
global using CalamityAnomalies.Visuals;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ReLogic.Graphics;
global using Terraria;
global using Terraria.Audio;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;
global using Transoceanic.Common;
global using Transoceanic.DataStructures;
global using Transoceanic.DataStructures.Assets;
global using Transoceanic.DataStructures.Geometry;
global using Transoceanic.DataStructures.Particles;
global using Transoceanic.Framework;
global using Transoceanic.Framework.Abstractions;
global using Transoceanic.Framework.ExternalAttributes;
global using Transoceanic.Framework.Helpers;
global using static CalamityAnomalies.Common.CASharedData.QuickAccess;

namespace CalamityAnomalies;

public sealed class CAMain : Mod, IContentLoader
{
    internal static CAMain Instance { get; private set; }

    internal static bool Loading { get; private set; }

    internal static bool Loaded { get; private set; }

    internal static bool Unloading { get; private set; }

    internal static bool Unloaded { get; private set; }

    public override void Load()
    {
        Loading = true;
        try
        {
            Instance = this;

            foreach (ICALoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ICALoader>(CASharedData.Assembly)
                orderby pair.Type.GetMethod(nameof(ICALoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                select pair.Instance)
            {
                loader.Load();
            }
        }
        finally
        {
            Loaded = true;
            Loading = false;
        }
    }

    public override void Unload()
    {
        Unloading = true;
        try
        {
            if (Loaded)
            {
                foreach (ICALoader loader in (
                    from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ICALoader>(CASharedData.Assembly)
                    orderby pair.Type.GetMethod(nameof(ICALoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                    select pair.Instance).Reverse())
                {
                    loader.Unload();
                }
                Instance = null;
            }
        }
        finally
        {
            Unloaded = true;
            Unloading = false;
        }
    }

    public override object Call(params object[] args) => CAModCall.Call(args);

    public override void HandlePacket(BinaryReader reader, int whoAmI) => CASynchronization.HandlePacket(this, reader, whoAmI);
}