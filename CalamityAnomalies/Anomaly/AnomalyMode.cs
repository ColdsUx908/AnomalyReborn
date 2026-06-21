// Developed by ColdsUx

using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.UI.ModeIndicator;
using CalamityMod.World;
using Terraria.GameContent.UI.Elements;
using static CalamityMod.UI.ModeIndicator.ModeIndicatorUI;

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

    public override Asset<Texture2D> Texture => Ultra ? CATextures._anomalyUltraIndicator : CATextures._anomalyModeIndicator;
    public override Asset<Texture2D> OutlineTexture => Ultra ? CATextures._anomalyUltraIndicator_Border : CATextures._anomalyModeIndicator_Border;
    public override Asset<Texture2D> TextureDisabled => Ultra ? CATextures._anomalyUltraIndicator_Off : CATextures._anomalyModeIndicator_Off;

    public override SoundStyle ActivationSound => Main.zenithWorld ? CASounds.AromalyActivate : SupremeCalamitas.BulletHellEndSound;

    public override int BackBoneGameModeID => GameModeID.Master;

    public override bool IsBasedOn(DifficultyMode mode)
    {
        if (mode is MasterDifficulty or DeathDifficulty or MaliceDifficulty)
            return true;
        return false;
    }

    public override float DifficultyScale => 10000f;

    public override LocalizedText Name => this.GetText((Main.zenithWorld ? "Aromaly." : "") + "Name");

    public override Color ChatTextColor => Main.zenithWorld ? CASharedData.AromalyColor : CASharedData.MainColor;

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

    #region 世界内难度管理
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
        if (TOSharedData.NotClient)
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
        if (TOSharedData.NotClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "UltraInvalid_NotLegendary", Color.Red);
    }

    public static void InvalidInfo_Aromaly()
    {
        if (TOSharedData.NotClient)
            TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "UltraInvalid_Aromaly", CASharedData.AromalyColor);
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
                    InvalidInfo_Aromaly();
                    DisableUltra();
                    break;
                case (false, false) when CASharedData.AnomalyUltramundane: //不是传奇难度，且在GFB世界
                    InvalidInfo_NotLegendary();
                    InvalidInfo_Aromaly();
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
    #endregion 世界内难度管理

    #region Detour
    public delegate void Orig_CalculateDifficultyData();

    [DetourMethodTo(typeof(DifficultyModeSystem))]
    public static void Detour_CalculateDifficultyData(Orig_CalculateDifficultyData orig)
    {
        orig();

        List<DifficultyMode[]> difficultyTiers = DifficultyModeSystem.DifficultyTiers;
        bool foundAnomaly = false;

        //强制将异象模式图标置于最后
        for (int i = 0; i < difficultyTiers.Count; i++)
        {
            DifficultyMode[] tier = difficultyTiers[i];
            for (int j = 0; j < tier.Length; j++)
            {
                if (tier[j] is AnomalyMode)
                {
                    if (tier.Length == 1) //该行只有异象模式，直接删除
                        difficultyTiers.RemoveAt(i);
                    else //该行还有其他模式，将异象模式删除，重新整理该行
                        difficultyTiers[i] = [.. tier.Where(m => m is not AnomalyMode)];

                    foundAnomaly = true;
                    break;
                }
            }
        }

        //在最后一行添加异象模式
        if (foundAnomaly)
            difficultyTiers.Add([AnomalyMode.Instance]);
    }

    public delegate void Orig_ManageHexIcons(SpriteBatch spriteBatch, out string text);

    [DetourMethodTo(typeof(ModeIndicatorUI))]
    public static void Detour_ManageHexIcons(Orig_ManageHexIcons orig, SpriteBatch spriteBatch, out string text)
    {
        List<DifficultyMode[]> difficultyTiers = DifficultyModeSystem.DifficultyTiers;
        bool hasAnomaly = difficultyTiers.Any(tier => tier.Any(mode => mode is AnomalyMode));
        bool ultra = Ultra;

        int tiers = difficultyTiers.Count;
        float barLength = 90 * tiers * ModeIndicatorUI_Publicizer.BarExpansionProgress;
        float progress = ModeIndicatorUI_Publicizer.menuOpen ? 1 - ModeIndicatorUI_Publicizer.menuOpenTransitionTime / (float)ModeIndicatorUI_Publicizer.MenuAnimLength : ModeIndicatorUI_Publicizer.menuOpenTransitionTime / (float)ModeIndicatorUI_Publicizer.MenuAnimLength;
        Vector2 basePosition = DrawCenter + (barLength / (float)(tiers + 1f)) * Vector2.UnitY;

        text = string.Empty;
        bool modeHovered = false;
        Vector2 positionOffset = (barLength / (float)(tiers + 1f)) * Vector2.UnitY;
        float progressMult = 0.8f * progress;
        Color progressColor = Color.White * progress;

        Vector2 ultraOffset = new(0f, 40f); //修改点：定义异象超凡偏移量

        for (int i = 0; i < tiers; i++)
        {
            int modesAtTier = difficultyTiers[i].Length;
            float width = WidthForTier(modesAtTier) * 0.5f;
            for (int j = 0; j < modesAtTier; j++)
            {
                DifficultyMode mode = difficultyTiers[i][j];
                Texture2D hexIcon = mode.Enabled ? mode.Texture.Value : mode.TextureDisabled.Value;
                Vector2 hexIconSize = hexIcon.Size();

                // Get position.
                Vector2 iconPosition = basePosition + positionOffset * i;

                if (modesAtTier > 1)
                    iconPosition += Vector2.UnitX * MathHelper.Lerp(width * -1f, width, j / (float)(modesAtTier - 1)) * ModeIndicatorUI_Publicizer.BarWidthExpansionProgress;

                if (ultra) //修改点：针对异象超凡单独调整位置
                {
                    iconPosition += ultraOffset;
                    if (mode is AnomalyMode) //如果当前绘制的正是异象模式（此时必定为异象超凡），则再次调整位置
                        iconPosition += ultraOffset;
                }

                bool hovered = MouseScreenArea.Intersects(Utils.CenteredRectangle(iconPosition, hexIconSize));

                float usedOpacity = 0.85f;
                if (hovered)
                    usedOpacity = MathHelper.Lerp(usedOpacity, 1f, 0.7f);

                // Outline the currently selected difficulty.
                if (mode == DifficultyModeSystem.GetCurrentDifficulty)
                {
                    usedOpacity = 1f;
                    Texture2D outlineTexture = mode.OutlineTexture.Value;
                    Color chatTextColor = mode.ChatTextColor;
                    if (mode is AnomalyMode && !Main.zenithWorld) //修改点：针对异象模式调整为渐变色
                        chatTextColor = Ultra ? CASharedData.UltraIdentifierColor : CASharedData.IdentifierColor;
                    spriteBatch.Draw(outlineTexture, iconPosition, null, chatTextColor * progressMult, 0f, outlineTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                }

                spriteBatch.Draw(hexIcon, iconPosition, null, progressColor * usedOpacity, 0f, hexIconSize * 0.5f, 1f, SpriteEffects.None, 0f);

                if (ModeIndicatorUI_Publicizer.menuOpenTransitionTime == 0 && hovered)
                {
                    if (ModeIndicatorUI_Publicizer.previouslyHoveredMode != mode)
                        SoundEngine.PlaySound(SoundID.MenuTick);

                    ModeIndicatorUI_Publicizer.previouslyHoveredMode = mode;
                    modeHovered = true;

                    text = GetDifficultyText(mode);

                    if (ClickingMouse)
                        SwitchToDifficulty(mode, broadcast: true);
                }
            }
        }

        if (!modeHovered)
            ModeIndicatorUI_Publicizer.previouslyHoveredMode = null;
    }
    #endregion Detour

    void IContentLoader.PostSetupContent()
    {
        DifficultyModeSystem.Difficulties.Add(AnomalyMode.Instance = new());
        DifficultyModeSystem.CalculateDifficultyData();

        //世界难度显示（渐变色）
        On_AWorldListItem.GetDifficulty += On_AWorldListItem_GetDifficulty;

        void On_AWorldListItem_GetDifficulty(On_AWorldListItem.orig_GetDifficulty orig, AWorldListItem self, out string expertText, out Color gameModeColor)
        {
            orig(self, out expertText, out gameModeColor);

            if (gameModeColor == Main.creativeModeColor)
                return;

            if (self.Data.TryGetHeaderData<CASharedData>(out TagCompound tag) && tag.GetBool("Anomaly"))
            {
                expertText = Language.GetTextValue(LocalizationPrefix + "Name");
                gameModeColor = CASharedData.IdentifierColor;
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

    public override void OnEnterWorld() => CASynchronization.SyncAnomalyModeFromServer();
}