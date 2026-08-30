using CalamityMod.Systems;
using CalamityMod.UI.ModeIndicator;
using Transoceanic.Framework.ExternalAttributes;

namespace Anomalies.ModCompatibility;

//按字母顺序排列

[Publicize(typeof(DifficultyModeSystem))]
[ExtendsFromMod(CalamityModName)]
internal partial class DifficultyModeSystem_Publicizer(DifficultyModeSystem Source) : InstancedPublicizer(Source);

[Publicize(typeof(ModeIndicatorUI))]
[ExtendsFromMod(CalamityModName)]
internal partial class ModeIndicatorUI_Publicizer(ModeIndicatorUI Source) : InstancedPublicizer(Source);
