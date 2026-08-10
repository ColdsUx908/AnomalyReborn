// Developed by ColdsUx

using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Systems;
using CalamityMod.UI.ModeIndicator;
using Transoceanic.Framework.ExternalAttributes;

namespace Anomalies.ModCompatibility;

//按字母顺序排列

[Publicize(typeof(AverageDamageClass))]
[ExtendsFromMod(CalamityModName)]
internal partial class AverageDamageClass_Publicizer;

[Publicize(typeof(CalamityGlobalNPC))]
[ExtendsFromMod(CalamityModName)]
internal partial class CalamityGlobalNPC_Publicizer(CalamityGlobalNPC Source) : InstancedPublicizer(Source);

[Publicize(typeof(CalamityMod_))]
[ExtendsFromMod(CalamityModName)]
internal partial class CalamityMod_Publicizer(CalamityMod_ Source) : InstancedPublicizer(Source);

[Publicize(typeof(DifficultyModeSystem))]
[ExtendsFromMod(CalamityModName)]
internal partial class DifficultyModeSystem_Publicizer(DifficultyModeSystem Source) : InstancedPublicizer(Source);

[Publicize(typeof(ModeIndicatorUI))]
[ExtendsFromMod(CalamityModName)]
internal partial class ModeIndicatorUI_Publicizer(ModeIndicatorUI Source) : InstancedPublicizer(Source);

[Publicize(typeof(RogueDamageClass))]
[ExtendsFromMod(CalamityModName)]
internal partial class RogueDamageClass_Publicizer;

[Publicize(typeof(TrueMeleeDamageClass))]
[ExtendsFromMod(CalamityModName)]
internal partial class TrueMeleeDamageClass_Publicizer;

[Publicize(typeof(TrueMeleeNoSpeedDamageClass))]
[ExtendsFromMod(CalamityModName)]
internal partial class TrueMeleeNoSpeedDamageClass_Publicizer;