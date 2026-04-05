global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Text.RegularExpressions;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ReLogic.Graphics;
global using Terraria;
global using Terraria.DataStructures;
global using Terraria.Enums;
global using Terraria.GameContent;
global using Terraria.Graphics;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ModLoader.Core;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;
global using Transoceanic.Commands;
global using Transoceanic.Framework.Abstractions;
global using Transoceanic.Framework.ExternalAttributes;
global using Transoceanic.Framework.Helpers;
global using Transoceanic.Framework.Helpers.AbstractionHandlers;
global using Transoceanic.GlobalInstances;

namespace Transoceanic;

// Designed by ColdsUx

public sealed class TOMain : Mod
{
    internal static TOMain Instance { get; private set; }

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

            foreach (ITOLoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>(TOReflectionUtils.Assembly)
                orderby pair.Type.GetMethod(nameof(ITOLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
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

    public override void PostSetupContent()
    {
        foreach (IContentLoader loader in
            from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IContentLoader>()
            orderby pair.type.GetMethod(nameof(IContentLoader.PostSetupContent), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
            select pair.instance)
        {
            loader.PostSetupContent();
        }
    }

    public override void Unload()
    {
        Unloading = true;
        try
        {
            if (Loaded)
            {
                foreach (ITOLoader loader in (
                    from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<ITOLoader>()
                    orderby pair.type.GetMethod(nameof(ITOLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                    select pair.instance).Reverse())
                {
                    loader.Unload();
                }

                TOSharedData.SyncEnabled = false;
                Instance = null;
            }
        }
        finally
        {
            Loaded = false;
            Unloaded = true;
            Unloading = false;
        }
    }
}
