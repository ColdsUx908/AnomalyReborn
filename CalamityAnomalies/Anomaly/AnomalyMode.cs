// Developed by ColdsUx

using System.Text.RegularExpressions;
using CalamityMod;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Systems;
using CalamityMod.UI.ModeIndicator;
using CalamityMod.World;
using Terraria.GameContent.UI.Elements;
using Terraria.UI.Chat;
using static CalamityAnomalies.ModCompatibility.DifficultyModeSystem_Publicizer;
using static CalamityAnomalies.ModCompatibility.ModeIndicatorUI_Publicizer;
using static CalamityMod.Systems.DifficultyModeSystem;
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

        List<DifficultyMode[]> difficultyTiers = DifficultyTiers;
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
        List<DifficultyMode[]> difficultyTiers = DifficultyTiers;
        bool hasAnomaly = difficultyTiers.Any(tier => tier.Any(mode => mode is AnomalyMode));
        bool ultra = Ultra;

        int tiers = difficultyTiers.Count;
        float barLength = 90 * tiers * BarExpansionProgress;
        float progress = menuOpen ? 1 - menuOpenTransitionTime / (float)MenuAnimLength : menuOpenTransitionTime / (float)MenuAnimLength;
        Vector2 basePosition = DrawCenter + barLength / (float)(tiers + 1f) * Vector2.UnitY;

        text = string.Empty;
        bool modeHovered = false;
        Vector2 positionOffset = barLength / (float)(tiers + 1f) * Vector2.UnitY;
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
                    iconPosition += Vector2.UnitX * MathHelper.Lerp(width * -1f, width, j / (float)(modesAtTier - 1)) * BarWidthExpansionProgress;

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
                if (mode == GetCurrentDifficulty)
                {
                    usedOpacity = 1f;
                    Texture2D outlineTexture = mode.OutlineTexture.Value;
                    Color chatTextColor = mode.ChatTextColor;
                    if (mode is AnomalyMode && !Main.zenithWorld) //修改点：针对异象模式调整为渐变色
                        chatTextColor = Ultra ? CASharedData.UltraIdentifierColor : CASharedData.IdentifierColor;
                    spriteBatch.Draw(outlineTexture, iconPosition, null, chatTextColor * progressMult, 0f, outlineTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                }

                spriteBatch.Draw(hexIcon, iconPosition, null, progressColor * usedOpacity, 0f, hexIconSize * 0.5f, 1f, SpriteEffects.None, 0f);

                if (menuOpenTransitionTime == 0 && hovered)
                {
                    if (previouslyHoveredMode != mode)
                        SoundEngine.PlaySound(SoundID.MenuTick);

                    previouslyHoveredMode = mode;
                    modeHovered = true;

                    text = GetDifficultyText(mode);

                    if (ClickingMouse)
                        SwitchToDifficulty(mode, broadcast: true);
                }
            }
        }

        if (!modeHovered)
            previouslyHoveredMode = null;
    }

    public delegate void Orig_Draw(SpriteBatch spriteBatch);
    [DetourMethodTo(typeof(ModeIndicatorUI))]
    public static void Detour_Draw(Orig_Draw orig, SpriteBatch spriteBatch)
    {
        // The mode indicator should only be displayed when the inventory is open, to prevent obstruction.
        if (!Main.playerInventory)
        {
            ClearVariables();
            return;
        }

        Texture2D indicatorTexture = GetCurrentDifficulty.Texture.Value;

        GetDifficultyStatus(out LocalizedText difficultyText);
        GetLockStatus(out LocalizedText lockText, out bool locked);

        //Grows the icon when hovering it.
        if (MouseScreenArea.Intersects(MainClickArea))
        {
            if (!_hasCheckedItOutYet)
            {
                GlowFadeTime = GlowFadeAnimLength;
                _hasCheckedItOutYet = true;
            }

            if (!previouslyHoveringMainIcon)
            {
                previouslyHoveringMainIcon = true;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            if (iconHoverScaleBoost < MaxIconHoverScaleBoost)
            {
                iconHoverScaleBoost = Math.Min(iconHoverScaleBoost + IconHoverScaleIncrement + IconHoverScaleDecrement, MaxIconHoverScaleBoost);

                if (ClickingMouse && menuOpenTransitionTime == 0 && !locked)
                {
                    SoundEngine.PlaySound(menuOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                    menuOpenTransitionTime = MenuAnimLength;
                    menuOpen = !menuOpen;
                }
            }
        }
        else
            previouslyHoveringMainIcon = false;

        if (!_hasCheckedItOutYet || GlowFadeTime > 0)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

            Texture2D bloomTex = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/BloomFlare").Value;
            float opacity = !_hasCheckedItOutYet ? 1f : 1f * GlowFadeTime / GlowFadeAnimLength;
            float scale = 0.4f + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.05f;
            float rot = Main.GlobalTimeWrappedHourly * 0.5f;

            spriteBatch.Draw(bloomTex, DrawCenter, null, Color.Crimson * opacity, rot, new Vector2(123, 124), scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

            /*
            Texture2D outlineTexture = ModContent.Request<Texture2D>("CalamityMod/UI/ModeIndicator/ModeIndicatorOutline").Value;
            spriteBatch.Draw(outlineTexture, DrawCenter, null, Color.White * opacity, 0f, outlineTexture.Size() * 0.5f, MainIconScale, SpriteEffects.None, 0f);
            */
        }

        string extraDescText = string.Empty;
        if (menuOpenTransitionTime > 0 || menuOpen)
            ManageHexIcons(spriteBatch, out extraDescText);

        //TODO: 对于异象模式，不绘制锁，而是绘制特殊的锁定材质

        //Draw the indicator itself.
        spriteBatch.Draw(indicatorTexture, DrawCenter, null, Color.White, 0f, indicatorTexture.Size() * 0.5f, MainIconScale, SpriteEffects.None, 0f);

        if (locked)
            DrawLock(spriteBatch);

        if (difficultyText != LocalizedText.Empty || extraDescText != string.Empty)
        {
            string textToDisplay = difficultyText.ToString();
            if (difficultyText != LocalizedText.Empty)
            {
                if (lockText != LocalizedText.Empty)
                    textToDisplay += "\n" + lockText.ToString();
            }

            else
                textToDisplay = extraDescText;

            //Get the size of the textbox
            //Get a "regexed" size which matches the text properly.
            //Indeed, there is some scuffery in the code that makes it so that chat tags still get accounted as extra size, so we have to use 2 different values
            //One for the displacement, which is the improper size vanilla uses, and another for the actual visual size, which is the one the textbox will use
#pragma warning disable SYSLIB1045 //转换为“GeneratedRegexAttribute”
            int numLines = 1 + Regex.Matches(textToDisplay, "\n").Count; //It's inconsistent. Adding one by default makes some textboxes too large, not adding one can make some too small
#pragma warning restore SYSLIB1045 //转换为“GeneratedRegexAttribute”
            string heightCalculator = string.Concat(Enumerable.Repeat("mis nuevos los gatos \n", numLines));
            Vector2 regexedBoxSize = new(ChatManager.GetStringSize(FontAssets.MouseText.Value, textToDisplay, Vector2.One).X, ChatManager.GetStringSize(FontAssets.MouseText.Value, heightCalculator, Vector2.One).Y);

            Vector2 textboxStart = new Vector2(Main.mouseX, Main.mouseY) + Vector2.One * 14;
            if (Main.ThickMouse)
                textboxStart += Vector2.One * 6;

            if (!Main.mouseItem.IsAir)
                textboxStart.X += 34;

            if (textboxStart.X + regexedBoxSize.X + 4f > Main.screenWidth)
                textboxStart.X = Main.screenWidth - regexedBoxSize.X - 4f;

            if (textboxStart.Y + regexedBoxSize.Y + 4f > Main.screenHeight)
                textboxStart.Y = Main.screenHeight - regexedBoxSize.Y - 4f;

            //It'd be great to be able to add a background to it but i don't think i know how to get the position of the text for that.
            //Also the "get string size" thing breaks with colored lines so :(
            Utils.DrawInvBG(spriteBatch, new Rectangle((int)textboxStart.X - 10, (int)textboxStart.Y - 10, (int)regexedBoxSize.X + 20, (int)regexedBoxSize.Y + 16), new Color(50, 20, 35) * 0.925f);

            //Add the hover text.
            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(textToDisplay);
        }
        TickVariables();
    }

    public delegate void Orig_GetDifficultyStatus(out LocalizedText text);
    [DetourMethodTo(typeof(ModeIndicatorUI))]
    public static void Detour_GetDifficultyStatus(Orig_GetDifficultyStatus orig, out LocalizedText text)
    {
        text = LocalizedText.Empty;
        if (MouseScreenArea.Intersects(MainClickArea))
        {
            //Display the first non-none difficulty by default
            string modeToDisplay = Main.getGoodWorld && Difficulties[0].FTWName is not null ? Difficulties[0].FTWName.ToString() : Difficulties[1].Name.ToString();
            bool anyActiveMode = Main.getGoodWorld;

            for (int i = 1; i < Difficulties.Count; i++)
            {
                DifficultyMode difficulty = Difficulties[i];
                if (GetCurrentDifficulty == difficulty)
                {
                    modeToDisplay = Main.getGoodWorld && difficulty.FTWName is not null ? difficulty.FTWName.ToString() : difficulty.Name.ToString();

                    if (difficulty is AnomalyMode && Ultra) //异象超凡显示“超凡”后缀
                        modeToDisplay += " " + Language.GetTextValue(LocalizationPrefix + "UltraSuffix") + " ";

                    anyActiveMode = true;
                }
            }
            string modeStr = CalamityUtils.GetText("UI.ModeAppend").Format(modeToDisplay);
            string activeText = CalamityUtils.GetTextValue("UI." + (anyActiveMode ? "Active" : "NotActive"));
            text = CalamityUtils.GetText("UI.DifficultyStatusText").WithFormatArgs(modeStr, activeText.ToLower());
        }
    }

    public delegate string Orig_GetDifficultyText(DifficultyMode mode);
    [DetourMethodTo(typeof(ModeIndicatorUI))]
    public static string Detour_GetDifficultyText(Orig_GetDifficultyText orig, DifficultyMode mode)
    {
        bool useFTWName = mode.FTWName is not null && Main.getGoodWorld;
        LocalizedText preface = useFTWName ? mode.FTWName : mode.Name;
        if (mode == GetCurrentDifficulty)
            preface = CalamityUtils.GetText("UI.CurrentlySelected").WithFormatArgs(useFTWName ? mode.FTWName.ToString() : mode.Name.ToString());

        string text = "\n" + mode.ShortDescription.ToString();

        // Not scuffed anymore.
        if (mode.ExpandedDescription != LocalizedText.Empty)
        {
            // Show the description either if the player is holding shift.
            if (Main.keyState.PressingShift())
            {
                text += "\n" + mode.ExpandedDescription.ToString();
                if (mode is AnomalyMode && Ultra)
                    text += "\n\n" + Language.GetTextValue(LocalizationPrefix + "UltraInfo");
            }
            else
                text += "\n" + CalamityUtils.GetTextValue("UI.DifficultyShiftText");
        }

        string name = preface.ToString();
        if (mode is AnomalyMode && Ultra) //异象超凡显示“超凡”后缀
            name += " " + Language.GetTextValue(LocalizationPrefix + "UltraSuffix");
        return name + text;
    }
    #endregion Detour

    void IContentLoader.PostSetupContent()
    {
        Difficulties.Add(AnomalyMode.Instance = new());
        CalculateDifficultyData();

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
        if (Difficulties.Remove(AnomalyMode.Instance))
            CalculateDifficultyData();
        AnomalyMode.Instance = null;
    }
}

public sealed class AnomalyModePlayerSync : CAPlayerBehavior
{
    public override decimal Priority => 100m;

    public override void OnEnterWorld() => CASynchronization.SyncAnomalyModeFromServer();
}