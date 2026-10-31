// Whole-host wrappers intentionally omit inaccessible signature types; individual diagnostics are expected here.
#pragma warning disable TOGEN002
using Terraria.Graphics.Light;

namespace Transoceanic.Runtime.Publicizing.Wrappers;

//按字母顺序排列

/// <summary>
/// 为 LightingEngine 提供源生成的非公开成员访问包装。
/// </summary>
[Publicize(typeof(LightingEngine))]
public partial class LightingEngine_Publicizer(LightingEngine Source) : InstancedPublicizer(Source);

/// <summary>
/// 为 Main 提供源生成的非公开成员访问包装。
/// </summary>
[Publicize(typeof(Main))]
public partial class Main_Publicizer(Main Source) : InstancedPublicizer(Source);

/// <summary>
/// 为 ModTypeLookup 提供源生成的非公开成员访问包装。
/// </summary>
[Publicize(typeof(ModTypeLookup<>))]
public partial class ModTypeLookup_Publicizer<T>() where T : IModType;

/// <summary>
/// 为 NPC_HitModifiers 提供源生成的非公开成员访问包装。
/// </summary>
[Publicize(typeof(NPC.HitModifiers))]
public partial class NPC_HitModifiers_Publicizer(NPC.HitModifiers Source) : InstancedPublicizer(Source);

/// <summary>
/// 为 RenderTarget2D 提供源生成的非公开成员访问包装。
/// </summary>
[Publicize(typeof(RenderTarget2D))]
public partial class RenderTarget2D_Publicizer(RenderTarget2D Source) : InstancedPublicizer(Source);

/// <summary>
/// 为 SpriteBatch 提供源生成的非公开成员访问包装。
/// </summary>
[Publicize(typeof(SpriteBatch))]
public partial class SpriteBatch_Publicizer(SpriteBatch Source) : InstancedPublicizer(Source);
