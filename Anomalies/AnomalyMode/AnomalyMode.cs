// Developed by ColdsUx

using CalamityMod.Systems;
using static CalamityMod.Systems.DifficultyModeSystem;

namespace Anomalies.AnomalyMode;

[ExtendsFromMod(CalamityModName)]
public sealed class AnomalyMode : DifficultyMode, ILocalizationPrefix
{
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + "AnomalyMode";

    internal static AnomalyMode Instance;

    public override bool Enabled
    {
        get => AnomalySharedData.Anomaly;
        set => AnomalySharedData.Anomaly = value;
    }

    public override Asset<Texture2D> Texture => Ultra ? AnomalyModeHandler._UltraIndicator : AnomalyModeHandler._Indicator;
    public override Asset<Texture2D> OutlineTexture => Ultra ? AnomalyModeHandler._UltraIndicator_Border : AnomalyModeHandler._Indicator_Border;
    public override Asset<Texture2D> TextureDisabled => Ultra ? AnomalyModeHandler._UltraIndicator_Off : AnomalyModeHandler._Indicator_Off;

    public override SoundStyle ActivationSound => Main.zenithWorld ? AnomalyModeHandler.AromalyActivationSound : AnomalyModeHandler.ActivationSound;

    public override int BackBoneGameModeID => GameModeID.Master;

    public override bool IsBasedOn(DifficultyMode mode)
    {
        if (mode is MasterDifficulty or DeathDifficulty or MaliceDifficulty)
            return true;
        return false;
    }

    public override float DifficultyScale => 10000f;

    public override LocalizedText Name => this.GetText((Main.zenithWorld ? "Aromaly." : "") + "Name");

    public override Color ChatTextColor => Main.zenithWorld ? AnomalySharedData.AromalyColor : AnomalySharedData.MainColor;

    public override LocalizedText ShortDescription => this.GetText("ShortInfo");
    public override LocalizedText ExpandedDescription => this.GetText("ExpandedInfo");

    public override int[] FavoredDifficultyAtTier(int tier)
    {
        DifficultyMode[] tierList = DifficultyTiers[tier];

        List<int> difficulties = [];

        for (int i = 0; i < tierList.Length; i++)
        {
            if (tierList[i] is MasterDifficulty or DeathDifficulty)
                difficulties.Add(i);
        }

        if (difficulties.Count <= 0)
            difficulties.Add(0);

        return [.. difficulties];
    }
}
