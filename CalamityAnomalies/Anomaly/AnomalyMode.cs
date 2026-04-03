using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.World;
using Terraria.GameContent.UI.Elements;

namespace CalamityAnomalies.Anomaly;

public sealed class AnomalyMode : DifficultyMode, ILocalizationPrefix
{
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "Anomaly.AnomalyMode";

    internal static AnomalyMode Instance;

    public override bool Enabled
    {
        get => CASharedData.Anomaly;
        set => CASharedData.Anomaly = value;
    }

    public override Asset<Texture2D> Texture => CATextures._anomalyModeIndicator;
    public override Asset<Texture2D> OutlineTexture => CATextures._anomalyModeIndicator_Border;
    public override Asset<Texture2D> TextureDisabled => CATextures._anomalyModeIndicator_Off;

    public override SoundStyle ActivationSound => Main.zenithWorld ? CASounds.AnomalyActivate_GFB : SupremeCalamitas.BulletHellEndSound;

    public override int BackBoneGameModeID => GameModeID.Master;

    public override bool IsBasedOn(DifficultyMode mode)
    {
        if (mode is MasterDifficulty or DeathDifficulty or MaliceDifficulty)
            return true;
        return false;
    }

    public override float DifficultyScale => 10000f;

    public override LocalizedText Name => this.GetText("Name" + (Main.zenithWorld ? "_GFB" : ""));

    public override Color ChatTextColor => Main.zenithWorld ? CASharedData.GFBColor : CASharedData.MainColor;

    public override LocalizedText ShortDescription => this.GetText("ShortInfo");
    public override LocalizedText ExpandedDescription => this.GetText("ExpandedInfo");

    public override int[] FavoredDifficultyAtTier(int tier)
    {
        DifficultyMode[] tierList = DifficultyModeSystem.DifficultyTiers[tier];

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

public sealed class AnomalyModeHandler : ModSystem, IContentLoader
{
    public const string LocalizationPrefix = CASharedData.ModLocalizationPrefix + "Anomaly.AnomalyMode.";

    public override void PreUpdateWorld()
    {
        if (CASharedData.Anomaly)
        {
            if (!TOSharedData.MasterMode)
            {
                DisableAnomaly();
                return;
            }

            CalamityWorld.revenge = true;
            CalamityWorld.death = true;
        }

        CheckAnomalyUltra();
    }

    public static void DisableAnomaly()
    {
        if (TOSharedData.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "Invalid", Color.Red);
        CASharedData.Anomaly = false;
    }

    public static void DisableUltra()
    {
        CASharedData.AnomalyUltramundane = false;
    }

    public static void EnableUltra()
    {
        CASharedData.AnomalyUltramundane = true;
    }

    public static void InvalidInfo_NotLegendary()
    {
        if (TOSharedData.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "UltraInvalid_NotLegendary", Color.Red);
    }

    public static void InvalidInfo_Zenith()
    {
        if (TOSharedData.GeneralClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "UltraInvalid_Zenith", CASharedData.MainColor);
        //SoundEngine.PlaySound();
    }

    public static void CheckAnomalyUltra()
    {
        if (CASharedData.Anomaly)
        {
            switch (TOSharedData.LegendaryMode, !Main.zenithWorld)
            {
                case (false, true) when CASharedData.AnomalyUltramundane: //不是传奇难度，不在GFB世界
                    InvalidInfo_NotLegendary();
                    DisableUltra();
                    break;
                case (true, false) when CASharedData.AnomalyUltramundane: //是传奇难度，在GFB世界
                    InvalidInfo_Zenith();
                    DisableUltra();
                    break;
                case (false, false) when CASharedData.AnomalyUltramundane: //不是传奇难度，且在GFB世界
                    InvalidInfo_NotLegendary();
                    InvalidInfo_Zenith();
                    DisableUltra();
                    break;
                case (true, true) when !CASharedData.AnomalyUltramundane: //是传奇难度，且不在GFB世界，应开启异象超凡
                    EnableUltra();
                    break;
                default:
                    break;
            }
        }
        else if (CASharedData.AnomalyUltramundane)
            DisableUltra();
    }

    void IContentLoader.PostSetupContent()
    {
        DifficultyModeSystem.Difficulties.Add(AnomalyMode.Instance = new());
        DifficultyModeSystem.CalculateDifficultyData();

        //世界难度
        On_AWorldListItem.GetDifficulty += On_AWorldListItem_GetDifficulty;

        void On_AWorldListItem_GetDifficulty(On_AWorldListItem.orig_GetDifficulty orig, AWorldListItem self, out string expertText, out Color gameModeColor)
        {
            orig(self, out expertText, out gameModeColor);

            if (gameModeColor == Main.creativeModeColor)
                return;

            if (self.Data.TryGetHeaderData<CASharedData>(out TagCompound tag) && tag.GetBool("Anomaly"))
            {
                expertText = Language.GetTextValue(LocalizationPrefix + "Name");
                gameModeColor = Color.LerpMany(CASharedData.ColorList2, TOMathUtils.Interpolation.QuadraticEaseIn(TOMathUtils.TimeWrappingFunction.GetTimeSin(0.5f, 2.5f, unsigned: true)) / 2f);
            }
        }
    }

    void IContentLoader.OnModUnload()
    {
        if (DifficultyModeSystem.Difficulties.Remove(AnomalyMode.Instance))
            DifficultyModeSystem.CalculateDifficultyData();
        AnomalyMode.Instance = null;
    }
}

public sealed class AnomalyModePlayerSync : CAPlayerBehavior
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => CANetSync.SyncAnomalyModeFromServer();
}