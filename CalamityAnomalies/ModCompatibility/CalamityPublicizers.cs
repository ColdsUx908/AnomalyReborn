using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.HiveMind;

namespace CalamityAnomalies.ModCompatibility;

//按字母顺序排列

[Publicize(typeof(AverageDamageClass))]
internal partial class AverageDamageClass_Publicizer;

[Publicize(typeof(CalamityGlobalNPC))]
internal partial class CalamityGlobalNPC_Publicizer(CalamityGlobalNPC Source) : InstancedPublicizer(Source);

[Publicize(typeof(CalamityMod_))]
internal partial class CalamityMod_Publicizer(CalamityMod_ Source) : InstancedPublicizer(Source);

[Publicize(typeof(HiveMind))]
internal partial class HiveMind_Publicizer(HiveMind Source) : InstancedPublicizer(Source);

[Publicize(typeof(TrueMeleeDamageClass))]
internal partial class TrueMeleeDamageClass_Publicizer;

[Publicize(typeof(TrueMeleeNoSpeedDamageClass))]
internal partial class TrueMeleeNoSpeedDamageClass_Publicizer;