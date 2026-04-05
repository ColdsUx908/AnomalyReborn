using System.Diagnostics.CodeAnalysis;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.UI;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI.BigProgressBar;
using Transoceanic.Framework.Helpers.AbstractionHandlers;
using static CalamityAnomalies.Visuals.BetterBossHealthBar;
using static CalamityMod.UI.BossHealthBarManager;

namespace CalamityAnomalies.Visuals;

public sealed class BetterBossHealthBar : ModBossBarStyleDetour<BossHealthBarManager>, IContentLoader, ILocalizationPrefix
{
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "UI.BetterBossHealthBar";

    public static readonly HashSet<int> _exclusiveNPCTypes = [];

    public delegate bool BetterOverridingNameFunction(BetterBossHPUI bar, [NotNullWhen(true)] out string overridingName);
    public delegate bool BetterLifeFunction(BetterBossHPUI bar);
    public delegate bool BetterSmallTextFunction(BetterBossHPUI bar, [NotNullWhen(true)] out string text, out bool disableOrig);

    public static readonly List<BetterOverridingNameFunction> _overridingNameFunctions = [];
    public static readonly List<BetterLifeFunction> _lifeFunctions = [];
    public static readonly List<BetterSmallTextFunction> _smallTextFunctions = [];

    public static DynamicSpriteFont MouseFont => FontAssets.MouseText?.Value;
    public static DynamicSpriteFont ItemStackFont => FontAssets.ItemStack?.Value;

    public static readonly Dictionary<long, BetterBossHPUI> CurrentBars = [];
    public const int MaxBars = 6;
    public const int MaxActiveBars = 4;

    private static readonly HashSet<long> _validIdentifiers = [];

    public override void Detour_Draw(Orig_Draw orig, BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
    {
        int x = Main.screenWidth
            - (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing || AcidRainEvent.AcidRainEventIsOngoing ? 670 : 420);
        int y = Main.screenHeight - 30;

        int activeCount = 0;

        foreach (BetterBossHPUI newBar in
            from pair in CurrentBars
            let newBar = pair.Value
            orderby newBar.Valid descending, pair.Key ascending
            select newBar)
        {
            y -= newBar.Height;
            if (activeCount >= MaxActiveBars && newBar.Valid)
                continue;
            newBar.Draw(spriteBatch, ref x, ref y);
            if (newBar.Valid)
                activeCount++;
        }
    }

    public override void Detour_Update(Orig_Update orig, BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info)
    {
        _validIdentifiers.Clear();
        foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(n => !BossExclusionList.Contains(n.type)))
        {
            long npcIdentifier = npc.Identifier;
            if (CurrentBars.ContainsKey(npcIdentifier))
                _validIdentifiers.Add(npcIdentifier);
            else if (CurrentBars.Count < MaxBars && ((npc.IsBossEnemy && !_exclusiveNPCTypes.Contains(npc.type)) || MinibossHPBarList.Contains(npc.type) || npc.Calamity.CanHaveBossHealthBar))
                CurrentBars.Add(npcIdentifier, new BetterBossHPUI(npc));
        }

        foreach ((long identifier, BetterBossHPUI newBar) in CurrentBars)
        {
            newBar.Update(_validIdentifiers.Contains(identifier));
            if (newBar.CloseAnimationTimer >= 120)
                CurrentBars.Remove(identifier);
        }
    }

    void IContentLoader.PostSetupContent()
    {
        _exclusiveNPCTypes.Add(NPCID.EaterofWorldsBody);
        _exclusiveNPCTypes.Add(NPCID.EaterofWorldsTail);
        _exclusiveNPCTypes.Add(ModContent.NPCType<Artemis>());

        MinibossHPBarList.Add(NPCID.LunarTowerVortex);
        MinibossHPBarList.Add(NPCID.LunarTowerStardust);
        MinibossHPBarList.Add(NPCID.LunarTowerNebula);
        MinibossHPBarList.Add(NPCID.LunarTowerSolar);
        MinibossHPBarList.Add(NPCID.PirateShip);
        OneToMany[NPCID.SkeletronHead] = [NPCID.SkeletronHand];
        OneToMany[NPCID.SkeletronPrime] = [NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeCannon, NPCID.PrimeLaser];
        OneToMany[NPCID.Golem] = [NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree];
        OneToMany[NPCID.BrainofCthulhu] = [NPCID.Creeper];
        OneToMany[NPCID.MartianSaucerCore] = [NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon];
        OneToMany[NPCID.PirateShip] = [NPCID.PirateShipCannon];
        OneToMany[ModContent.NPCType<CeaselessVoid>()] = [ModContent.NPCType<DarkEnergy>()];
        OneToMany[ModContent.NPCType<RavagerBody>()] =
        [
            ModContent.NPCType<RavagerClawRight>(),
            ModContent.NPCType<RavagerClawLeft>(),
            ModContent.NPCType<RavagerLegRight>(),
            ModContent.NPCType<RavagerLegLeft>(),
            ModContent.NPCType<RavagerHead>()
        ];
        OneToMany[ModContent.NPCType<EbonianPaladin>()] = [];
        OneToMany[ModContent.NPCType<CrimulanPaladin>()] = [];

        //阿波罗
        _overridingNameFunctions.Add((b, out name) =>
        {
            if (b.NPC.ModNPC is Apollo apollo)
            {
                name = Language.GetTextValue(CASharedData.CalamityModLocalizationPrefix + "UI.ExoTwinsName" + (apollo.exoMechdusa ? "Hekate" : "Normal"));
                return true;
            }
            name = null;
            return false;
        });

        //荷兰飞盗船
        _lifeFunctions.Add(b =>
        {
            NPC npc = b.NPC;
            if (npc.type == NPCID.PirateShip)
            {
                long lifeMax = 0L;
                long life = 0L;
                foreach ((long identifier, NPC n) in b.CustomOneToMany)
                {
                    if (n.Identifier == identifier && n.active && n.lifeMax > 0)
                    {
                        lifeMax += n.lifeMax;
                        life += n.life;
                    }
                }
                if (b.CombinedNPCMaxLife != 0L && (b.InitialMaxLife == 0L || b.InitialMaxLife < b.CombinedNPCMaxLife))
                    b.InitialMaxLife = b.CombinedNPCMaxLife;
                return true;
            }
            return false;
        });
    }

    void IContentLoader.OnModUnload()
    {
        _exclusiveNPCTypes.Clear();
        _overridingNameFunctions.Clear();
        _lifeFunctions.Clear();
        _smallTextFunctions.Clear();
    }

    void IContentLoader.OnWorldLoad() => CurrentBars.Clear();

    void IContentLoader.OnWorldUnload() => CurrentBars.Clear();
}

/// <summary>
/// 改进的Boss血条UI类。
/// </summary>
public class BetterBossHPUI : BossHPUI
{
#pragma warning disable CA1822
    public new NPC AssociatedNPC => throw new InvalidOperationException("BetterBossHPUI.AssociatedNPC should not be used. Use BetterBossHPUI.NPC instead.");
    public new void Update() => throw new InvalidOperationException($"BetterBossHPUI.Update() should not be used. Use BetterBossHPUI.Update(bool) instead.");
    public new void Draw(SpriteBatch spriteBatch, int x, int y) => throw new InvalidOperationException($"BetterBossHPUI.Draw(SpriteBatch, int, int) should not be used. Use BetterBossHPUI.Draw(SpriteBatch, ref int, ref int) instead.");
#pragma warning restore CA1822

    public static readonly Color BaseColor = new(240, 240, 255);

    public readonly bool HasOneToMany;

    public readonly int[] CustomOneToManyIndexes;

    // TODO 这个字典是未完成的。在未来的异象模式中，Boss生成时会绑定自己的所有体节NPC。
    public readonly Dictionary<long, NPC> CustomOneToMany = [];

    public readonly bool HasSpecialLifeRequirement;

    public readonly NPCSpecialHPGetFunction HPGetFunction;

    public bool Valid { get; private set; } = true;

    public readonly NPC NPC;

    public readonly long Identifier;

    public readonly CAGlobalNPC AnomalyNPC;

    public readonly CalamityGlobalNPC CalamityNPC;

    public new int NPCType => NPC.type;

    public new long CombinedNPCLife;
    public new long CombinedNPCMaxLife;

    public new float NPCLifeRatio
    {
        get
        {
            if (!Valid)
                return 0f;

            float temp = (float)CombinedNPCLife / InitialMaxLife;

            if (float.IsNaN(temp) || float.IsInfinity(temp))
                return 0f;

            return temp;
        }
    }

    public new bool NPCIsEnraged => Valid && NPC.active && (CalamityNPC.CurrentlyEnraged || (HasOneToMany && CustomOneToMany.Values.Any(n => n.Calamity.CurrentlyEnraged)));

    public new bool NPCIsIncreasingDefenseOrDR => Valid && NPC.active && (CalamityNPC.CurrentlyIncreasingDefenseOrDR || (HasOneToMany && CustomOneToMany.Values.Any(n => n.Calamity.CurrentlyIncreasingDefenseOrDR)));

    public int Height { get; private set; } = 70;

    public float AnimationCompletionRatio { get; private set; }

    public float AnimationCompletionRatio2 { get; private set; }

    public BetterBossHPUI(NPC npc) : base(npc.whoAmI, null)
    {
        NPC = Main.npc[NPCIndex];
        Identifier = NPC.Identifier;
        AnomalyNPC = NPC.Anomaly;
        CalamityNPC = NPC.Calamity;

        HasOneToMany = OneToMany.TryGetValue(NPCType, out int[] value);
        CustomOneToManyIndexes = value;

        foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
        {
            if (requirement(NPC))
            {
                HasSpecialLifeRequirement = true;
                HPGetFunction = func;
            }
        }
    }

    public virtual void Update(bool valid)
    {
        Valid = valid;

        if (PreUpdate())
        {
            CustomOneToMany.Clear();
            if (HasOneToMany)
            {
                foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(n => CustomOneToManyIndexes.Contains(n.type)))
                    CustomOneToMany.TryAdd(npc.Identifier, npc);
            }
            UpdateNPCLife();
            UpdateMaxLife();

            if (CombinedNPCLife != PreviousLife && PreviousLife != 0L)
            {
                if (ComboDamageCountdown <= 0)
                    HealthAtStartOfCombo = CombinedNPCLife;
                ComboDamageCountdown = 30;
            }
            PreviousLife = CombinedNPCLife;

            if (Valid)
            {
                if (ComboDamageCountdown > 0)
                    ComboDamageCountdown--;

                OpenAnimationTimer = Math.Clamp(OpenAnimationTimer + 1, 0, 120); //由80改为120

                EnrageTimer = Math.Clamp(EnrageTimer + (NPCIsEnraged ? 1 : -4), 0, 120);
                IncreasingDefenseOrDRTimer = Math.Clamp(IncreasingDefenseOrDRTimer + (NPCIsIncreasingDefenseOrDR ? 1 : -4), 0, 120);
                CloseAnimationTimer = Math.Clamp(CloseAnimationTimer - 2, 0, 120);
            }
            else
            {
                ComboDamageCountdown = 0;

                EnrageTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                IncreasingDefenseOrDRTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                CloseAnimationTimer++;
            }

            AnimationCompletionRatio = CloseAnimationTimer > 0
                ? 1f - MathHelper.Clamp(CloseAnimationTimer / 120f, 0f, 1f)
                : MathHelper.Clamp(OpenAnimationTimer / 80f, 0f, 1f);
            AnimationCompletionRatio2 = CloseAnimationTimer > 0
                ? 1f - MathHelper.Clamp(CloseAnimationTimer / 80f, 0f, 1f)
                : MathHelper.Clamp(OpenAnimationTimer / 120f, 0f, 1f);
        }

        PostUpdate();
    }

    protected void UpdateNPCLife()
    {
        if (!Valid || !NPC.active)
            CombinedNPCLife = 0L;

        foreach (BetterLifeFunction func in _lifeFunctions)
        {
            if (func(this))
                return;
        }

        foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
        {
            if (requirement(NPC))
            {
                CombinedNPCLife = func(NPC, false);
                return;
            }
        }

        long result = NPC.life;
        foreach ((long identifier, NPC npc) in CustomOneToMany)
        {
            if (npc.Identifier == identifier && npc.active && npc.life > 0)
                result += npc.life;
        }
        CombinedNPCLife = result;
    }

    protected void UpdateMaxLife()
    {
        if (!Valid || !NPC.active)
            CombinedNPCMaxLife = 0L;

        foreach (BetterLifeFunction func in _lifeFunctions)
        {
            if (func(this))
                return;
        }

        foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
        {
            if (requirement(NPC))
            {
                CombinedNPCMaxLife = func(NPC, true);
                goto InitialMaxLife;
            }
        }

        long result = NPC.lifeMax;
        foreach ((long identifier, NPC npc) in CustomOneToMany)
        {
            if (npc.Identifier == identifier && npc.active && npc.life > 0)
                result += npc.lifeMax;
        }
        CombinedNPCMaxLife = result;

    InitialMaxLife:
        if (CombinedNPCMaxLife != 0L && (InitialMaxLife == 0L || InitialMaxLife < CombinedNPCMaxLife))
            InitialMaxLife = CombinedNPCMaxLife;
    }

    protected bool PreUpdate()
    {
        bool result = true;
        bool hasSingle = false;
        if (NPC.ModNPC is ICAModNPC caNPC)
        {
            result &= caNPC.PreUpdateCalBossBar(this);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreUpdateCalBossBar)))
        {
            result &= npcBehavior.PreUpdateCalBossBar(this);
            hasSingle = true;
        }
        foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PreUpdateCalBossBar)))
            result &= anomalyGNPCBehavior.PreUpdateCalBossBar(NPC, this, hasSingle);
        return result;
    }

    protected void PostUpdate()
    {
        bool hasSingle = false;
        if (NPC.ModNPC is ICAModNPC caNPC)
        {
            caNPC.PostUpdateCalBossBar(this);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PostUpdateCalBossBar)))
        {
            npcBehavior.PostUpdateCalBossBar(this);
            hasSingle = true;
        }
        foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PostUpdateCalBossBar)))
            anomalyGNPCBehavior.PostUpdateCalBossBar(NPC, this, hasSingle);
    }

    public virtual void Draw(SpriteBatch spriteBatch, ref int x, ref int y)
    {
        if (PreDraw(spriteBatch, ref x, ref y))
        {
            DrawMainBar(spriteBatch, x, y);

            DrawComboBar(spriteBatch, x, y);

            (float sin, float cos) = TOMathUtils.TimeWrappingFunction.GetTimeSinCos(0.5f, 1f, 0f, true);

            Color seperatorColor;
            if (AnomalyNPC.IsRunningAnomalyAI)
            {
                seperatorColor = Color.Lerp(BaseColor, Color.Lerp(CASharedData.GetGradientColor(0.25f), CASharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * sin), Math.Clamp(AnomalyNPC.AnomalyAITimer / 120f, 0f, 1f));
                if (IncreasingDefenseOrDRTimer > 0)
                    seperatorColor = Color.Lerp(seperatorColor, Color.LightGray * 0.7f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 0.6f));
            }
            else if (EnrageTimer > 0)
                seperatorColor = Color.Lerp(BaseColor, Color.Red * 0.5f, Math.Clamp(EnrageTimer / 80f, 0f, 1f));
            else if (IncreasingDefenseOrDRTimer > 0)
                seperatorColor = Color.Lerp(BaseColor, Color.LightGray * 0.7f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 1f));
            else
                seperatorColor = BaseColor;
            seperatorColor *= AnimationCompletionRatio2;

            DrawSeperatorBar(spriteBatch, x, y, seperatorColor);

            //为了避免NPC名称过长遮挡大生命值数字，二者的绘制顺序在此处被调换了，即先绘制NPC名称，再绘制大生命值数字。
            Color? mainColor;
            if (AnomalyNPC.IsRunningAnomalyAI)
            {
                mainColor = Color.Lerp(CASharedData.GetGradientColor(0.1f), CASharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * cos * 0.8f);
                if (IncreasingDefenseOrDRTimer > 0)
                    mainColor = Color.Lerp(mainColor.Value, Color.LightGray * 0.7f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 0.6f));
                if (EnrageTimer > 0)
                    mainColor = Color.Lerp(mainColor.Value, Color.Red * 0.6f, Math.Clamp(EnrageTimer / 80f, 0f, 0.6f));
            }
            else if (EnrageTimer > 0)
                mainColor = Color.Red * 0.6f;
            else if (IncreasingDefenseOrDRTimer > 0)
                mainColor = Color.LightGray * 0.7f;
            else
                mainColor = null;
            mainColor *= AnimationCompletionRatio2;

            Color? borderColor;
            if (AnomalyNPC.IsRunningAnomalyAI)
            {
                borderColor = Color.Lerp(CASharedData.GetGradientColor(0.1f), CASharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * sin * 0.8f);
                if (IncreasingDefenseOrDRTimer > 0)
                    borderColor = Color.Lerp(borderColor.Value, Color.LightGray * 0.2f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 0.6f));
                if (EnrageTimer > 0)
                    borderColor = Color.Lerp(borderColor.Value, Color.Gray * 0.2f, Math.Clamp(EnrageTimer / 80f, 0f, 0.6f));
            }
            else if (EnrageTimer > 0 || IncreasingDefenseOrDRTimer > 0)
                borderColor = Color.Black * 0.2f;
            else
                borderColor = null;
            borderColor *= AnimationCompletionRatio2;

            float borderWidth;
            if (AnomalyNPC.IsRunningAnomalyAI)
                borderWidth = (1.5f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.75f, 1f, 0f, true)) * Math.Clamp(AnomalyNPC.AnomalyAITimer / 120f, 0f, 1f);
            else if (EnrageTimer > 0)
                borderWidth = (1f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.75f, 1f, 0f, true)) * Math.Clamp(EnrageTimer / 80f, 0f, 1f);
            else if (IncreasingDefenseOrDRTimer > 0)
                borderWidth = (1f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.75f, 1f, 0f, true)) * Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 1f);
            else
                borderWidth = 0f;

            DrawNPCName(spriteBatch, x, y, null, mainColor, borderColor, borderWidth);
            DrawBigLifeText(spriteBatch, x, y);
            DrawExtraSmallText(spriteBatch, x, y);
            PostDraw(spriteBatch, x, y);
        }
    }

    protected bool PreDraw(SpriteBatch spriteBatch, ref int x, ref int y)
    {
        bool result = true;
        bool hasSingle = false;
        if (NPC.ModNPC is ICAModNPC caNPC)
        {
            result &= caNPC.PreDrawCalBossBar(this, spriteBatch, ref x, ref y);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreDrawCalBossBar)))
        {
            result &= npcBehavior.PreDrawCalBossBar(this, spriteBatch, ref x, ref y);
            hasSingle = true;
        }
        foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PreDrawCalBossBar)))
            result &= anomalyGNPCBehavior.PreDrawCalBossBar(NPC, this, spriteBatch, ref x, ref y, hasSingle);
        return result;
    }

    protected void PostDraw(SpriteBatch spriteBatch, int x, int y)
    {
        bool hasSingle = false;
        if (NPC.ModNPC is ICAModNPC caNPC)
        {
            caNPC.PostDrawCalBossBar(this, spriteBatch, x, y);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PostDrawCalBossBar)))
        {
            npcBehavior.PostDrawCalBossBar(this, spriteBatch, x, y);
            hasSingle = true;
        }
        foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PostDrawCalBossBar)))
            anomalyGNPCBehavior.PostDrawCalBossBar(NPC, this, spriteBatch, x, y, hasSingle);
    }

    #region 公共绘制方法
    public void DrawMainBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
        spriteBatch.Draw(BossMainHPBar, new Rectangle(x, y + 28, mainBarWidth, BossMainHPBar.Height), newColor ?? Color.White);
    }

    public void DrawComboBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        if (ComboDamageCountdown <= 0)
            return;

        int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
        int comboHPBarWidth = (int)(400 * (float)HealthAtStartOfCombo / InitialMaxLife) - mainBarWidth;
        if (ComboDamageCountdown < 6)
            comboHPBarWidth = comboHPBarWidth * ComboDamageCountdown / 6;

        spriteBatch.Draw(BossComboHPBar, new Rectangle(x + mainBarWidth, y + 28, comboHPBarWidth, BossComboHPBar.Height), newColor ?? Color.White);
    }

    public void DrawSeperatorBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null) =>
        spriteBatch.Draw(BossSeperatorBar, new Rectangle(x, y + 18, 400, 6), newColor ?? BaseColor * AnimationCompletionRatio);

    public void DrawNPCName(SpriteBatch spriteBatch, int x, int y, string overrideText = null, Color? mainColor = null, Color? borderColor = null, float borderWidth = 0f)
    {
        string name = overrideText;
        if (name is null)
        {
            foreach (BetterOverridingNameFunction func in _overridingNameFunctions)
            {
                if (func(this, out name))
                    break;
            }
        }
        name ??= NPC.FullName;
        Vector2 npcNameSize = MouseFont.MeasureString(name);
        Vector2 baseDrawPosition = new(x + 400 - npcNameSize.X, y + 23 - npcNameSize.Y);
        DrawBorderStringEightWay_Loop(spriteBatch, MouseFont, name, baseDrawPosition, mainColor, borderColor, Color.White * AnimationCompletionRatio2, Color.Black * 0.2f * AnimationCompletionRatio2, 8, borderWidth, 1f);
    }

    public void DrawBigLifeText(SpriteBatch spriteBatch, int x, int y, string overrideText = null)
    {
        string bigLifeText = overrideText ?? (NPCLifeRatio == 0f ? "0%" : (NPCLifeRatio * 100f).ToString("N1") + "%");
        Vector2 bigLifeTextSize = HPBarFont.MeasureString(bigLifeText);
        TODrawUtils.DrawBorderString(spriteBatch, HPBarFont, bigLifeText, new Vector2(x, y + 22 - bigLifeTextSize.Y), MainColor * AnimationCompletionRatio2, MainBorderColour * 0.25f * AnimationCompletionRatio2);
    }

    public void DrawExtraSmallText(SpriteBatch spriteBatch, int x, int y, string overrideText = null, bool ignoreConfig = false)
    {
        if (!ignoreConfig && !CanDrawExtraSmallText)
            return;

        float whiteColorAlpha = OpenAnimationTimer switch
        {
            4 or 8 or 16 => Main.rand.NextFloat(0.7f, 0.8f),
            3 or 7 or 15 => Main.rand.NextFloat(0.4f, 0.5f),
            _ => AnimationCompletionRatio
        };
        int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);

        string smallText = "";
        if (overrideText is not null)
        {
            smallText = overrideText;
            goto Orig;
        }

        foreach (BetterSmallTextFunction func in _smallTextFunctions)
        {
            if (func(this, out smallText, out bool disableOrig))
            {
                if (disableOrig)
                    goto Draw;
                goto Orig;
            }
        }

        if (EntityExtensionHandler.TryGetValue(NPCType, out BossEntityExtension extraEntityData))
        {
            string extensionName = extraEntityData.NameOfExtensions.ToString();
            int extraEntities = NPC.ActiveNPCs.Count(n => extraEntityData.TypesToSearchFor.Contains(n.type));
            smallText = $"({extensionName}: {extraEntities}) ";
        }

    Orig:
        smallText += $"({CombinedNPCLife} / {InitialMaxLife})";
    Draw:
        TODrawUtils.DrawBorderString(spriteBatch, ItemStackFont, smallText, new Vector2(Math.Max(x, x + mainBarWidth - (ItemStackFont.MeasureString(smallText) * 0.75f).X), y + 45), Color.White * whiteColorAlpha, Color.Black * whiteColorAlpha * 0.24f, scale: 0.75f);
    }

    public static void DrawBorderStringEightWay_Loop(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 baseDrawPosition,
        Color? mainColor, Color? borderColor, Color mainColor2, Color borderColor2,
        int round, float borderWidth, float scale = 1f)
    {
        if (mainColor is not null && borderColor is not null && borderWidth > 0f)
        {
            for (int i = 0; i < round; i++)
                TODrawUtils.DrawBorderString(spriteBatch, font, text, baseDrawPosition + new PolarVector2(borderWidth, MathHelper.TwoPi / round * i), mainColor.Value, borderColor.Value, scale: scale);
        }
        TODrawUtils.DrawBorderString(spriteBatch, font, text, baseDrawPosition, mainColor2, borderColor2, scale: scale);
    }
    #endregion 公共绘制方法
}