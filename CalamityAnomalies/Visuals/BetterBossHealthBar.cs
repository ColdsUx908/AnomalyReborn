// Developed by ColdsUx

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

/// <summary>
/// 改进的Boss血条样式管理器。此类是对灾厄模组 <see cref="BossHealthBarManager"/> 的 Detour 实现，
/// 用于重定向原有的绘制与更新逻辑，而非从零开始构建全新血条系统，也并非 CalamityAnomalies 自己独立的血条。
/// 它在灾厄原有 Boss 血条的基础上提供增强的视觉效果、多体节生命合并、异象模式特殊渲染等功能。
/// </summary>
public sealed class BetterBossHealthBar : ModBossBarStyleDetour<BossHealthBarManager>, IContentLoader, ILocalizationPrefix
{
    /// <summary>
    /// 获取此 UI 元素的本地化前缀，用于查询语言文件中的文本。
    /// </summary>
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "UI.BetterBossHealthBar";

    /// <summary>
    /// 存储被明确排除、不使用 BetterBossHPUI 进行显示的 NPC 类型 ID。
    /// 例如世界吞噬者的体节、阿尔忒弥斯等。
    /// </summary>
    public static readonly HashSet<int> _exclusiveNPCTypes = [];

    /// <summary>
    /// 用于自定义覆盖 Boss 名称的委托。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    /// <param name="overridingName">输出参数，用于提供覆盖名称。</param>
    /// <returns>返回 <see langword="true"/> 以替代默认 NPC 全名。</returns>
    public delegate bool BetterOverridingNameFunction(BetterBossHPUI bar, [NotNullWhen(true)] out string overridingName);

    /// <summary>
    /// 用于自定义或补充血量统计逻辑的委托。返回 <see langword="true"/> 表示拦截了默认的血量计算。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    public delegate bool BetterLifeFunction(BetterBossHPUI bar);

    /// <summary>
    /// 用于在血量条下方额外显示小文本，可完全禁用原始小文本。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    /// <param name="text">输出参数，用于提供额外小文本。</param>
    /// <param name="disableOrig">输出参数，指示是否禁用原始的生命值小文本。</param>
    /// <returns>返回 <see langword="true"/> 表示拦截了灾厄默认的拓展小文本显示逻辑。</returns>
    public delegate bool BetterSmallTextFunction(BetterBossHPUI bar, [NotNullWhen(true)] out string text, out bool disableOrig);

    /// <summary>
    /// 注册的自定义名称覆盖函数列表。
    /// </summary>
    public static readonly List<BetterOverridingNameFunction> _overridingNameFunctions = [];

    /// <summary>
    /// 注册的自定义血量统计函数列表。先匹配到的函数将阻止后续默认计算。
    /// </summary>
    public static readonly List<BetterLifeFunction> _lifeFunctions = [];

    /// <summary>
    /// 注册的额外小文本生成函数列表。
    /// </summary>
    public static readonly List<BetterSmallTextFunction> _smallTextFunctions = [];

    /// <summary>
    /// 鼠标文字字体（用于绘制 Boss 名称）。
    /// </summary>
    public static DynamicSpriteFont MouseFont => FontAssets.MouseText?.Value;
    /// <summary>
    /// 物品堆叠数字字体（用于额外小文本）。
    /// </summary>
    public static DynamicSpriteFont ItemStackFont => FontAssets.ItemStack?.Value;

    /// <summary>
    /// 最大可同时存在的血条记录数量。
    /// </summary>
    public const int MaxBars = 6;
    /// <summary>
    /// 最大同时显示的活动 NPC 的血条数量。
    /// </summary>
    public const int MaxActiveBars = 4;

    /// <summary>
    /// 当前活跃的 BetterBossHPUI 实例，以 NPC 的 Identifier 为键。
    /// </summary>
    public static readonly Dictionary<long, BetterBossHPUI> CurrentBars = [];

    /// <summary>
    /// 每帧更新时用于标记当前仍有效的 NPC 标识符，无效的血条将在后续被移除。
    /// </summary>
    private static readonly HashSet<long> _validIdentifiers = [];

    /// <summary>
    /// Detour 绘制方法，替代原有的 BossHealthBarManager.Draw，使用 BetterBossHPUI 进行绘制。
    /// </summary>
    public override void Detour_Draw(Orig_Draw orig, BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
    {
        int x = Main.screenWidth
            - (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing || AcidRainEvent.AcidRainEventIsOngoing ? 670 : 420);
        int y = Main.screenHeight - 25;

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

    /// <summary>
    /// Detour 更新方法，替代原有的 BossHealthBarManager.Update，管理 BetterBossHPUI 的生命周期。
    /// </summary>
    public override void Detour_Update(Orig_Update orig, BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info)
    {
        _validIdentifiers.Clear();
        foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(n => !BossExclusionList.Contains(n.type)))
        {
            long npcIdentifier = npc.Identifier;
            if (CurrentBars.ContainsKey(npcIdentifier))
                _validIdentifiers.Add(npcIdentifier);
            else if (CurrentBars.Count < MaxBars && ((npc.IsBossEnemy && !_exclusiveNPCTypes.Contains(npc.type)) || MinibossHPBarList.Contains(npc.type) || npc.CalamityNPC.CanHaveBossHealthBar))
                CurrentBars.Add(npcIdentifier, new BetterBossHPUI(npc));
        }

        foreach ((long identifier, BetterBossHPUI newBar) in CurrentBars)
        {
            newBar.Update(_validIdentifiers.Contains(identifier));
            if (newBar.CloseAnimationTimer >= 120)
                CurrentBars.Remove(identifier);
        }
    }

    /// <summary>
    /// 模组内容加载完成后执行，初始化排除列表、一对多体节关系以及注册各种自定义函数。
    /// </summary>
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

    /// <summary>
    /// 模组卸载时清理所有静态数据。
    /// </summary>
    void IContentLoader.OnModUnload()
    {
        _exclusiveNPCTypes.Clear();
        _overridingNameFunctions.Clear();
        _lifeFunctions.Clear();
        _smallTextFunctions.Clear();
    }

    /// <summary>
    /// 世界加载时清空已有的血条记录，开始新的世界。
    /// </summary>
    void IContentLoader.OnWorldLoad() => CurrentBars.Clear();

    /// <summary>
    /// 世界卸载时清空血条记录，避免数据残留。
    /// </summary>
    void IContentLoader.OnWorldUnload() => CurrentBars.Clear();
}

/// <summary>
/// 改进的Boss血条UI类。继承自灾厄的 <see cref="BossHPUI"/>，在原有基础上强化了视觉效果、
/// 多体节血量合并、异象模式渲染支持等功能。此类不是全新实现，而是对原版的扩展与替换。
/// </summary>
public class BetterBossHPUI : BossHPUI
{
    #region 基类成员隐藏
#pragma warning disable CA1822 //BossHPUI类中的这几个成员是实例成员，此处保留实例设定
    /// <summary>
    /// 禁止直接访问基类的 AssociatedNPC，应使用 <see cref="NPC"/> 属性。
    /// </summary>
    public new NPC AssociatedNPC => throw new InvalidOperationException("BetterBossHPUI.AssociatedNPC should not be used. Use BetterBossHPUI.NPC instead.");
    /// <summary>
    /// 禁止直接调用基类的无参 Update，应使用 <see cref="Update(bool)"/> 方法。
    /// </summary>
    public new void Update() => throw new InvalidOperationException($"BetterBossHPUI.Update() should not be used. Use BetterBossHPUI.Update(bool) instead.");
    /// <summary>
    /// 禁止直接调用基类的 Draw 方法，应使用 <see cref="Draw(SpriteBatch, ref int, ref int)"/>。
    /// </summary>
    public new void Draw(SpriteBatch spriteBatch, int x, int y) => throw new InvalidOperationException($"BetterBossHPUI.Draw(SpriteBatch, int, int) should not be used. Use BetterBossHPUI.Draw(SpriteBatch, ref int, ref int) instead.");
#pragma warning restore CA1822
    #endregion 基类成员隐藏

    /// <summary>
    /// 基础颜色，用于未激怒未强化防御时的血条渲染。
    /// </summary>
    public static readonly Color BaseColor = new(240, 240, 255);

    /// <summary>
    /// 是否在 OneToMany 中注册了附属 NPC 类型。
    /// </summary>
    public readonly bool HasOneToMany;

    /// <summary>
    /// 该 Boss 附带的一对多 NPC 类型列表。
    /// </summary>
    public readonly int[] CustomOneToManyIndexes;

    // TODO 这个字典是未完成的。在未来的异象模式中，Boss生成时会绑定自己的所有体节NPC。
    /// <summary>
    /// 当前活跃的附属 NPC 实例，以 Identifier 为键。用于合并血量。
    /// </summary>
    public readonly Dictionary<long, NPC> CustomOneToMany = [];

    /// <summary>
    /// 是否具有特殊的血量获取需求（通过 <see cref="SpecialHPRequirements"/> 注册的委托）。
    /// </summary>
    public readonly bool HasSpecialLifeRequirement;

    /// <summary>
    /// 自定义血量统计函数，若 <see cref="HasSpecialLifeRequirement"/> 为 <see langword="true"/> 时使用。
    /// </summary>
    public readonly NPCSpecialHPGetFunction HPGetFunction;

    /// <summary>
    /// 指示此血条 UI 当前是否有效（对应的 NPC 仍存活且需要显示）。
    /// </summary>
    public bool Valid { get; private set; } = true;

    /// <summary>
    /// 此血条关联的主 NPC 实例。
    /// </summary>
    public readonly NPC NPC;

    /// <summary>
    /// NPC 的唯一标识符。
    /// </summary>
    public readonly long Identifier;

    /// <summary>
    /// NPC 对应的 Anomaly 全局 NPC 组件。
    /// </summary>
    public readonly CAGlobalNPC AnomalyNPC;

    /// <summary>
    /// NPC 对应的灾厄全局 NPC 组件。
    /// </summary>
    public readonly CalamityGlobalNPC CalamityNPC;

    /// <summary>
    /// 获取 NPC 的类型 ID。
    /// </summary>
    public new int NPCType => NPC.type;

    /// <summary>
    /// 合并后的当前生命值。
    /// </summary>
    public new long CombinedNPCLife;

    /// <summary>
    /// 合并后的最大生命值。
    /// </summary>
    public new long CombinedNPCMaxLife;

    /// <summary>
    /// 当前生命值比例（0~1），基于 <see cref="InitialMaxLife"/> 计算。
    /// </summary>
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

    /// <summary>
    /// 当前 Boss（或附属 NPC）是否处于激怒状态。
    /// </summary>
    public new bool NPCIsEnraged => Valid && NPC.active && (CalamityNPC.CurrentlyEnraged || (HasOneToMany && CustomOneToMany.Values.Any(n => n.CalamityNPC.CurrentlyEnraged)));

    /// <summary>
    /// 当前 Boss（或附属 NPC）是否正在提升防御/伤害减免。
    /// </summary>
    public new bool NPCIsIncreasingDefenseOrDR => Valid && NPC.active && (CalamityNPC.CurrentlyIncreasingDefenseOrDR || (HasOneToMany && CustomOneToMany.Values.Any(n => n.CalamityNPC.CurrentlyIncreasingDefenseOrDR)));

    /// <summary>
    /// 血条区域的高度（像素），用于布局排列。
    /// </summary>
    public int Height { get; private set; } = 85;

    /// <summary>
    /// 开启/关闭动画的完成比例，用于控制缩放和透明度。
    /// </summary>
    public float AnimationCompletionRatio { get; private set; }

    /// <summary>
    /// 第二种动画完成比例，用于区分不同元素的动画节奏。
    /// </summary>
    public float AnimationCompletionRatio2 { get; private set; }

    /// <summary>
    /// 构造 BetterBossHPUI 实例，绑定到指定的 NPC。
    /// </summary>
    /// <param name="npc">要显示血条的 NPC。</param>
    public BetterBossHPUI(NPC npc) : base(npc.whoAmI, null)
    {
        NPC = Main.npc[NPCIndex];
        Identifier = NPC.Identifier;
        AnomalyNPC = NPC.Anomaly;
        CalamityNPC = NPC.CalamityNPC;

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

    /// <summary>
    /// 更新血条状态。根据传入的有效性标记，推进动画计时器、刷新血量数据并更新异象指示器。
    /// </summary>
    /// <param name="valid">若为 <see langword="true"/>，表示关联 NPC 仍活跃且需要显示。</param>
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

            UpdateIndicators();
        }

        PostUpdate();
    }

    /// <summary>
    /// 更新合并后的当前生命值。会依次尝试自定义血量函数、特殊需求函数，最后采用默认合并逻辑。
    /// </summary>
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

    /// <summary>
    /// 更新合并后的最大生命值，并同步更新 <see cref="InitialMaxLife"/> 记录。
    /// </summary>
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

    /// <summary>
    /// 更新血量阈值指示器的计时及生命周期。
    /// </summary>
    protected void UpdateIndicators()
    {
        foreach (HPThresholdIndicator indicator in AnomalyNPC.HPThresholdIndicators)
        {
            if (indicator is null)
                continue;

            if (indicator.CustomUpdateFunction?.Invoke(indicator, NPC, this) == false)
                continue;

            indicator.Timer++;
            if (NPC.LifeRatio <= indicator.GetValue(NPC, this) || indicator.EaseOutTimer > 0)
                indicator.EaseOutTimer++;
        }

        AnomalyNPC.HPThresholdIndicators.RemoveAll(i => i is null || i.EaseOutTimer >= 60);
    }

    /// <summary>
    /// 执行更新前的拦截逻辑，调用相关联的各类行为接口。
    /// </summary>
    /// <returns>返回 <see langword="true"/> 以继续默认更新；<see langword="false"/> 则跳过。</returns>
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

    /// <summary>
    /// 执行更新后的钩子，通知相关行为对象。
    /// </summary>
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

    /// <summary>
    /// 绘制血条的完整流程，包含前置钩子、主体、后置钩子。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch。</param>
    /// <param name="x">绘制区域的 X 坐标（将被方法修改以进行布局）。</param>
    /// <param name="y">绘制区域的 Y 坐标（将被方法修改以进行布局）。</param>
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
                borderWidth = (1f + TOMathUtils.TimeWrappingFunction.GetTimeSin(1f, 1f, 0f, true)) * Math.Clamp(AnomalyNPC.AnomalyAITimer / 120f, 0f, 1f);
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

    /// <summary>
    /// 绘制前的拦截钩子，通过行为接口判断是否继续绘制。
    /// </summary>
    /// <returns>返回 <see langword="true"/> 以继续绘制；<see langword="false"/> 则跳过整个绘制流程。</returns>
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

    /// <summary>
    /// 绘制后通知相关行为对象。
    /// </summary>
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
    /// <summary>
    /// 绘制主血量条。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="newColor">可覆盖的颜色值。</param>
    public void DrawMainBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
        Color color = newColor ?? Color.White * AnimationCompletionRatio * AnimationCompletionRatio2;
        spriteBatch.Draw(BossMainHPBar, new Rectangle(x, y + 43, mainBarWidth, BossMainHPBar.Height), color);
    }

    /// <summary>
    /// 绘制连击伤害指示条（白色残影部分）。
    /// </summary>
    public void DrawComboBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        if (ComboDamageCountdown <= 0)
            return;

        int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
        int comboHPBarWidth = (int)(400 * (float)HealthAtStartOfCombo / InitialMaxLife) - mainBarWidth;
        if (ComboDamageCountdown < 6)
            comboHPBarWidth = comboHPBarWidth * ComboDamageCountdown / 6;
        Color color = newColor ?? Color.White * AnimationCompletionRatio * AnimationCompletionRatio2;

        spriteBatch.Draw(BossComboHPBar, new Rectangle(x + mainBarWidth, y + 43, comboHPBarWidth, BossComboHPBar.Height), color);
    }

    /// <summary>
    /// 绘制分隔条及异象模式下的血量阈值指示器。
    /// </summary>
    public void DrawSeperatorBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        Color color = newColor ?? BaseColor * AnimationCompletionRatio * AnimationCompletionRatio2;
        spriteBatch.Draw(BossSeperatorBar, new Rectangle(x, y + 33, 400, 6), color);

        //绘制血量阈值
        if (!AnomalyNPC.IsRunningAnomalyAI)
            return;

        foreach (HPThresholdIndicator indicator in AnomalyNPC.HPThresholdIndicators)
        {
            if (indicator is null)
                continue;

            float value = indicator.GetValue(NPC, this);
            if (value is <= 0f or >= 1f)
                continue;

            Vector2 center = new(x + 400 * value, y + 38);

            if (indicator.CustomDrawFunction?.Invoke(indicator, NPC, this, spriteBatch, center) == false)
                continue;

            float borderIntensity = Utils.Remap(NPC.LifeRatio - value, 0.07f, 0.02f, 0.5f, 1f, true);
            float thresholdAnimationCompletion = Math.Clamp(indicator.Timer / 60f, 0f, 1f) * (1f - Math.Clamp(indicator.EaseOutTimer / 60f, 0f, 1f));

            Texture2D borderTexture = indicator.IsSubPhaseIndicator ? CATextures.HPThresholdIndicator_SubBorder : CATextures.HPThresholdIndicator_Border;
            Texture2D texture = indicator.IsSubPhaseIndicator ? CATextures.HPThresholdIndicator_Sub : CATextures.HPThresholdIndicator;
            spriteBatch.DrawFromCenter(borderTexture, center, null, color * thresholdAnimationCompletion, scale: borderIntensity);
            spriteBatch.DrawFromCenter(texture, center, null, Color.White * AnimationCompletionRatio * AnimationCompletionRatio2 * thresholdAnimationCompletion);
        }
    }

    /// <summary>
    /// 绘制 NPC 名称，支持自定义名称覆盖、描边及颜色效果。
    /// </summary>
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
        Vector2 baseDrawPosition = new(x + 400 - npcNameSize.X, y + 35 - npcNameSize.Y);
        DrawBorderStringEightWay_Loop(spriteBatch, MouseFont, name, baseDrawPosition, mainColor, borderColor, Color.White * AnimationCompletionRatio2, Color.Black * 0.2f * AnimationCompletionRatio2, 8, borderWidth, 1f);
    }

    /// <summary>
    /// 绘制大型生命百分比文本。
    /// </summary>
    public void DrawBigLifeText(SpriteBatch spriteBatch, int x, int y, string overrideText = null)
    {
        string bigLifeText = overrideText ?? (NPCLifeRatio == 0f ? "0%" : (NPCLifeRatio * 100f).ToString("N1") + "%");
        Vector2 bigLifeTextSize = HPBarFont.MeasureString(bigLifeText);
        TODrawUtils.DrawBorderString(spriteBatch, HPBarFont, bigLifeText, new Vector2(x, y + 34 - bigLifeTextSize.Y), MainColor * AnimationCompletionRatio2, MainBorderColour * 0.25f * AnimationCompletionRatio2);
    }

    /// <summary>
    /// 绘制额外小文本（如具体生命数值、附属实体数量等）。
    /// </summary>
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
            smallText = $"    {extensionName}: {extraEntities}";
        }

    Orig:
        smallText = $"{CombinedNPCLife} / {InitialMaxLife}" + smallText;
    Draw:
        TODrawUtils.DrawBorderString(spriteBatch, ItemStackFont, smallText, new Vector2(x, y + 60), Color.White * whiteColorAlpha, Color.Black * whiteColorAlpha * 0.24f, scale: 0.8f);
    }

    /// <summary>
    /// 以八方向循环的方式绘制带描边的字符串，支持主颜色、描边颜色及宽度控制。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="font">字体。</param>
    /// <param name="text">要绘制的文本。</param>
    /// <param name="baseDrawPosition">基准绘制位置。</param>
    /// <param name="mainColor">主要文字颜色（用于外层循环描边）。</param>
    /// <param name="borderColor">描边颜色（用于外层循环描边）。</param>
    /// <param name="mainColor2">第二层文字颜色（绘制在中心）。</param>
    /// <param name="borderColor2">第二层描边颜色（绘制在中心）。</param>
    /// <param name="round">外层循环的描边方向数量，典型值为8。</param>
    /// <param name="borderWidth">描边宽度。</param>
    /// <param name="scale">绘制缩放。</param>
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

/// <summary>
/// 血量阈值指示器，用于在异象模式下 Boss 血条上标记关键血量百分比。
/// </summary>
public class HPThresholdIndicator
{
    /// <summary>
    /// 获取阈值浮点值（0~1）的委托。返回值代表血量条上的位置比例。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <returns>比例值，0~1 之间。</returns>
    public delegate float HPThresholdIndicatorValueFunction(HPThresholdIndicator indicator, NPC npc, BetterBossHPUI bar);

    /// <summary>
    /// 自定义更新行为委托。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <returns>返回 <see langword="false"/> 表示已处理更新，阻止默认计时逻辑；返回 <see langword="true"/> 则继续默认计时和生命周期控制。</returns>
    public delegate bool HPThresholdIndicatorUpdateFunction(HPThresholdIndicator indicator, NPC npc, BetterBossHPUI bar);

    /// <summary>
    /// 自定义绘制行为委托。
    /// </summary>
    /// <param name="indicator">当前指示器实例。</param>
    /// <param name="npc">关联的 NPC。</param>
    /// <param name="bar">对应的 BetterBossHPUI。</param>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch。</param>
    /// <param name="center">指示器中心点的坐标。</param>
    /// <returns>返回 <see langword="false"/> 表示已处理绘制，阻止默认绘制；返回 <see langword="true"/> 则继续默认绘制逻辑。</returns>
    public delegate bool HPThresholdIndicatorDrawFunction(HPThresholdIndicator indicator, NPC npc, BetterBossHPUI bar, SpriteBatch spriteBatch, Vector2 center);

    /// <summary>
    /// 存在计时器，控制指示器的显现动画。
    /// </summary>
    public int Timer;

    /// <summary>
    /// 淡出计时器，在血量低于阈值后开始计时，控制指示器的消失动画。
    /// </summary>
    public int EaseOutTimer;

    /// <summary>
    /// 获取阈值比例的函数。
    /// </summary>
    /// <remarks>用法参见 <see cref="HPThresholdIndicatorValueFunction"/>。</remarks>
    public HPThresholdIndicatorValueFunction ValueFunction;

    /// <summary>
    /// 自定义更新函数。
    /// </summary>
    /// <remarks>用法参见 <see cref="HPThresholdIndicatorUpdateFunction"/>。</remarks>
    public HPThresholdIndicatorUpdateFunction CustomUpdateFunction;

    /// <summary>
    /// 自定义绘制函数。
    /// </summary>
    /// <remarks>用法参见 <see cref="HPThresholdIndicatorDrawFunction"/>。</remarks>
    public HPThresholdIndicatorDrawFunction CustomDrawFunction;

    /// <summary>
    /// 是否为亚阶段指示器。
    /// <br/>若为 <see langword="true"/>，默认绘制时将使用银色纹理，而非金色纹理。此设定仅影响默认绘制逻辑，不会限制自定义绘制函数的表现形式。
    /// </summary>
    public bool IsSubPhaseIndicator;

    /// <summary>
    /// 获取当前指示器的阈值比例。
    /// </summary>
    /// <returns>比例值，0~1。</returns>
    public float GetValue(NPC npc, BetterBossHPUI bar) => ValueFunction?.Invoke(this, npc, bar) ?? 0f;
}