using Terraria.GameContent.UI.BigProgressBar;

namespace Anomalies.Bosses.BrainofCthulhu;

// Keep illusion boss heads and encounter flags without creating a separate health bar.
public sealed class BrainIllusionBossBar : ModBossBar
{
    public override string Texture => TOTextures.InvisibleTexturePath;

    public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax) => false;
}
