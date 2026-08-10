// Developed by ColdsUx

//不全局引用任何灾厄命名空间

global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using Anomalies.Assets;
global using Anomalies.Common;
global using Anomalies.ModCompatibility;
global using Anomalies.Visuals;
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
global using Transoceanic.Common;
global using Transoceanic.DataStructures;
global using Transoceanic.DataStructures.Assets;
global using Transoceanic.DataStructures.Geometry;
global using Transoceanic.DataStructures.Particles;
global using Transoceanic.Framework;
global using Transoceanic.Framework.Abstractions;
global using Transoceanic.Framework.Helpers;
global using static Anomalies.Common.AnomalySharedData.QuickAccess;

namespace Anomalies;

public sealed class AnomalyMain : Mod
{
    internal static AnomalyMain Instance { get; private set; }

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

            foreach (IAnomalyLoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IAnomalyLoader>(AnomalySharedData.Assembly)
                orderby pair.Type.GetMethod(nameof(IAnomalyLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
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
                foreach (IAnomalyLoader loader in (
                    from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IAnomalyLoader>(AnomalySharedData.Assembly)
                    orderby pair.Type.GetMethod(nameof(IAnomalyLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
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

    public override object Call(params object[] args) => AnomalyModCall.Call(args);

    public override void HandlePacket(BinaryReader reader, int whoAmI) => AnomalySynchronization.HandlePacket(this, reader, whoAmI);
}