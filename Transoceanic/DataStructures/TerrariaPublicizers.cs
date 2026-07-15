// Developed by ColdsUx

namespace Transoceanic.DataStructures;

//按字母顺序排列

[Publicize(typeof(Main))]
public partial class Main_Publicizer(Main Source) : InstancedPublicizer(Source);

[Publicize(typeof(ModTypeLookup<>))]
public partial class ModTypeLookup_Publicizer<T>() where T : IModType;

[Publicize(typeof(NPC.HitModifiers))]
public partial class NPC_HitModifiers_Publicizer(NPC.HitModifiers Source) : InstancedPublicizer(Source);