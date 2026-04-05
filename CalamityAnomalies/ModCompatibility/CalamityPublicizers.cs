using CalamityMod;

namespace CalamityAnomalies.ModCompatibility.CalamityPublicizers;

//按字母顺序排列

[Publicize(typeof(AverageDamageClass))]
internal partial class AverageDamageClass_Publicizer;

[Publicize(typeof(CalamityMod_))]
internal partial class CalamityMod_Publicizer(CalamityMod_ Source) : InstancedPublicizer(Source);

[Publicize(typeof(TrueMeleeDamageClass))]
internal partial class TrueMeleeDamageClass_Publicizer;

[Publicize(typeof(TrueMeleeNoSpeedDamageClass))]
internal partial class TrueMeleeNoSpeedDamageClass_Publicizer;