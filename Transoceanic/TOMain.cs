// Designed by ColdsUx

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
global using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace Transoceanic;

/// <summary>
/// Transoceanic 模组的主入口类。负责模组的加载、卸载生命周期管理，
/// 并通过反射自动发现并调用所有实现 <see cref="ITOLoader"/> 接口的加载器。
/// </summary>
public sealed class TOMain : Mod
{
    /// <summary>
    /// 获取当前 <see cref="TOMain"/> 模组的唯一实例。
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
    /// 模组加载入口点。在此方法中，通过反射发现所有实现 <see cref="ITOLoader"/> 的类型，
    /// 并按 <see cref="LoadPriorityAttribute"/> 指定的优先级降序调用其 <c>Load()</c> 方法。
    /// 加载过程的状态由 <see cref="Loading"/> 和 <see cref="Loaded"/> 标志跟踪。
    /// </summary>
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

    /// <summary>
    /// 在所有模组的内容加载完成后调用。在此方法中，通过反射发现所有实现 <see cref="IContentLoader"/> 的类型，
    /// 并按 <see cref="LoadPriorityAttribute"/> 指定的优先级降序调用其 <c>PostSetupContent()</c> 方法。
    /// </summary>
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

    /// <summary>
    /// 模组卸载入口点。如果模组先前已加载，则按加载时优先级的相反顺序调用所有 <see cref="ITOLoader"/> 的 <c>Unload()</c> 方法。
    /// 卸载过程的状态由 <see cref="Unloading"/> 和 <see cref="Unloaded"/> 标志跟踪。
    /// </summary>
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