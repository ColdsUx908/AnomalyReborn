// Developed by ColdsUx

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
global using Transoceanic.Common;
global using Transoceanic.DataStructures;
global using Transoceanic.Framework.Abstractions;
global using Transoceanic.Framework.ExternalAttributes;
global using Transoceanic.Framework.Helpers;

namespace Transoceanic;

/// <summary>
/// Transoceanic 模组的主入口类。
/// </summary>
public sealed class TOMain : Mod
{
    /// <summary>
    /// 获取当前 Transoceanic 模组的唯一实例。
    /// </summary>
    internal static TOMain Instance { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否正在执行加载过程。
    /// </summary>
    internal static bool Loading { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否已完成加载。
    /// </summary>
    internal static bool Loaded { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否正在执行卸载过程。
    /// </summary>
    internal static bool Unloading { get; private set; }

    /// <summary>
    /// 获取一个值，指示模组是否已完成卸载。
    /// </summary>
    internal static bool Unloaded { get; private set; }

    /// <summary>
    /// 模组加载入口点。
    /// 加载过程的状态由 <see cref="Loading"/> 和 <see cref="Loaded"/> 标志跟踪。
    /// </summary>
    public override void Load()
    {
        Loading = true;
        try
        {
            Instance = this;
            TOLoaderHandler.Loader_Load();
        }
        finally
        {
            Loaded = true;
            Loading = false;
        }
    }

    /// <summary>
    /// 在所有模组的内容加载完成后调用。
    /// </summary>
    public override void PostSetupContent() => TOLoaderHandler.Loader_PostSetupContent();

    /// <summary>
    /// 模组卸载入口点。
    /// 卸载过程的状态由 <see cref="Unloading"/> 和 <see cref="Unloaded"/> 标志跟踪。
    /// </summary>
    public override void Unload()
    {
        Unloading = true;
        try
        {
            if (Loaded)
            {
                TOLoaderHandler.Loader_Unload();
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