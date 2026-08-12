// Developed by ColdsUx

using Terraria.Graphics.Light;

namespace Transoceanic.DataStructures;

//按字母顺序排列

[Publicize(typeof(LightingEngine))]
public partial class LightingEngine_Publicizer(LightingEngine Source) : InstancedPublicizer(Source);

[Publicize(typeof(Main))]
public partial class Main_Publicizer(Main Source) : InstancedPublicizer(Source);

[Publicize(typeof(ModTypeLookup<>))]
public partial class ModTypeLookup_Publicizer<T>() where T : IModType;

[Publicize(typeof(NPC.HitModifiers))]
public partial class NPC_HitModifiers_Publicizer(NPC.HitModifiers Source) : InstancedPublicizer(Source);

[Publicize(typeof(RenderTarget2D))]
public partial class RenderTarget2D_Publicizer(RenderTarget2D Source) : InstancedPublicizer(Source);

[Publicize(typeof(SpriteBatch))]
public partial class SpriteBatch_Publicizer(SpriteBatch Source) : InstancedPublicizer(Source);