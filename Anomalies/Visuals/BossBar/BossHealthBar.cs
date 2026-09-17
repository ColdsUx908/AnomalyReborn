using System.Diagnostics.CodeAnalysis;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.Ravager;
using MonoMod.Utils;
using static Terraria.ModLoader.ModContent;

namespace Anomalies.Visuals.BossBar;

/// <summary>
/// Boss血条类。
/// </summary>
public class BossHealthBar : IContentLoader, ILocalizationPrefix
{
    /// <summary>
    /// 用于描述 Boss 附属实体扩展信息的结构。
    /// </summary>
    public struct BossEntityExtension
    {
        /// <summary>
        /// 附属实体的本地化名称。
        /// </summary>
        public LocalizedText NameOfExtensions;

        /// <summary>
        /// 需要搜索统计的 NPC 类型 ID 数组。
        /// </summary>
        public int[] TypesToSearchFor;

        /// <summary>
        /// 构造 BossEntityExtension。
        /// </summary>
        /// <param name="name">本地化名称。</param>
        /// <param name="types">NPC 类型 ID 数组。</param>
        public BossEntityExtension(LocalizedText name, params int[] types)
        {
            NameOfExtensions = name;
            TypesToSearchFor = types;
        }
    }

    /// <summary>
    /// 用于自定义覆盖 Boss 名称的委托。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    /// <param name="overridingName">输出参数，用于提供覆盖名称。</param>
    /// <returns>返回 <see langword="true"/> 以替代默认 NPC 全名。</returns>
    public delegate bool CustomOverridingNameFunction(BossHealthBar bar, [NotNullWhen(true)] out string overridingName);

    /// <summary>
    /// 用于自定义或补充血量统计逻辑的委托。返回 <see langword="true"/> 表示拦截了默认的血量计算。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    public delegate bool CustomLifeFunction(BossHealthBar bar);

    /// <summary>
    /// 用于在血量条下方额外显示小文本，可完全禁用原始小文本。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    /// <param name="text">输出参数，用于提供额外小文本。</param>
    /// <param name="disableOrig">输出参数，指示是否禁用原始的生命值小文本。</param>
    /// <returns>返回 <see langword="true"/> 表示拦截了灾厄默认的拓展小文本显示逻辑。</returns>
    public delegate bool CustomSmallTextFunction(BossHealthBar bar, [NotNullWhen(true)] out string text, out bool disableOrig);

    /// <summary>
    /// 主色调，用于大型生命百分比文本等主要元素。
    /// </summary>
    public static Color MainColor = new(229, 189, 62);

    /// <summary>
    /// 主边框颜色，用于大型文本描边。
    /// </summary>
    public static Color MainBorderColor = new(197, 127, 46);

    /// <summary>
    /// 基础颜色，用于未激怒未强化防御时的血条渲染。
    /// </summary>
    public static readonly Color BaseColor = new(240, 240, 255);

    /// <summary>
    /// 一对多映射，将主体 NPC 类型映射到其附属 NPC 类型数组，用于合并血量。
    /// </summary>
    public static Dictionary<int, int[]> OneToMany = [];

    /// <summary>
    /// 实体扩展信息处理器，存储特定 Boss 的额外附属实体名称与类型。
    /// </summary>
    public static Dictionary<int, BossEntityExtension> EntityExtensionHandler = [];

    /// <summary>
    /// 注册的自定义名称覆盖函数列表。
    /// </summary>
    public static List<CustomOverridingNameFunction> OverridingNameFunctions = [];

    /// <summary>
    /// 注册的自定义血量统计函数列表。先匹配到的函数将阻止后续默认计算。
    /// </summary>
    public static List<CustomLifeFunction> LifeFunctions = [];

    /// <summary>
    /// 注册的额外小文本生成函数列表。
    /// </summary>
    public static List<CustomSmallTextFunction> SmallTextFunctions = [];

    /// <summary>
    /// 鼠标文字字体（用于绘制 Boss 名称）。
    /// </summary>
    public static DynamicSpriteFont MouseFont => FontAssets.MouseText?.Value;

    /// <summary>
    /// 物品堆叠数字字体（用于额外小文本）。
    /// </summary>
    public static DynamicSpriteFont ItemStackFont => FontAssets.ItemStack?.Value;

    /// <summary>
    /// 血条锁动画帧数。
    /// <br/>此数值不包含空帧。因此，调用 <see cref="Utils.Frame(Texture2D, int, int, int, int, int, int)"/> 方法时请使用 <c>LockAnimationFrameCount + 1</c> 作为总帧数参数。
    /// </summary>
    public const int LockAnimationFrameCount = 14;

    /// <summary>
    /// 血条锁动画速度，单位为帧。
    /// <br/>该值表示血条动画推进一帧所需的游戏帧数，值越小动画越快。
    /// </summary>
    public const int LockAnimationSpeed = 2;

    /// <summary>
    /// 本地化前缀，用于在多语言环境下正确加载血条相关文本。
    /// </summary>
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + "Visuals.BossHealthBar";

    /// <summary>
    /// 指示此血条 UI 当前是否有效（对应的 NPC 仍存活且需要显示）。
    /// </summary>
    public bool Valid { get; private set; } = true;

    /// <summary>
    /// 血条开启动画的计时器，控制从无到有的过渡。
    /// </summary>
    public int OpenAnimationTimer;

    /// <summary>
    /// 血条关闭动画的计时器，控制从有到无的过渡。
    /// </summary>
    public int CloseAnimationTimer;

    /// <summary>
    /// 激怒状态过渡计时器。
    /// </summary>
    public int EnrageTimer;

    /// <summary>
    /// 防御/伤害减免提升状态过渡计时器。
    /// </summary>
    public int IncreasingDefenseOrDRTimer;

    /// <summary>
    /// 无敌（或几乎无敌）状态过渡计时器。
    /// </summary>
    public int ImmunityTimer;

    /// <summary>
    /// 连击伤害显示倒计时，控制白色残影条的持续时长。
    /// </summary>
    public int ComboDamageCountdown;

    /// <summary>
    /// 上一帧的合并生命值，用于检测连续伤害。
    /// </summary>
    public long PreviousLife;

    /// <summary>
    /// 当前连击开始时的合并生命值。
    /// </summary>
    public long HealthAtStartOfCombo;

    /// <summary>
    /// 是否在 OneToMany 中注册了附属 NPC 类型。
    /// </summary>
    public readonly bool HasOneToMany;

    /// <summary>
    /// 该 Boss 附带的一对多 NPC 类型列表。
    /// </summary>
    public readonly int[] CustomOneToManyIndexes;

    /// <summary>
    /// 当前活跃的附属 NPC 实例，以 Identifier 为键。用于合并血量。
    /// </summary>
    public readonly Dictionary<long, NPC> CustomOneToMany = [];

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
    public readonly AnomalyGlobalNPC AnomalyNPC;

    /// <summary>
    /// 获取 NPC 的类型 ID。
    /// </summary>
    public int NPCType => NPC.type;

    /// <summary>
    /// 指示此血条是否使用小尺寸布局（封装 AnomalyNPC.BossHealthBarIsSmall）。
    /// </summary>
    public bool IsSmall => AnomalyNPC.BossHealthBarIsSmall;

    /// <summary>
    /// 记录的初始最大生命值，用于计算生命比例，避免因最大生命值动态变化导致血条比例跳动。
    /// </summary>
    public long InitialMaxLife;

    /// <summary>
    /// 合并后的当前生命值。
    /// </summary>
    public long CombinedNPCLife;

    /// <summary>
    /// 合并后的最大生命值。
    /// </summary>
    public long CombinedNPCMaxLife;

    /// <summary>
    /// 当前生命值比例（0~1），基于 <see cref="InitialMaxLife"/> 计算。
    /// </summary>
    public float NPCLifeRatio
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
    public bool NPCIsEnraged => Valid && NPC.active && (AnomalyNPC.CurrentlyEnraged || (HasOneToMany && CustomOneToMany.Values.Any(n => n.Anomaly.CurrentlyEnraged)));

    /// <summary>
    /// 当前 Boss（或附属 NPC）是否正在提升防御/伤害减免。
    /// </summary>
    public bool NPCIsIncreasingDefenseOrDR => Valid && NPC.active && (AnomalyNPC.CurrentlyIncreasingDefenseOrDR || (HasOneToMany && CustomOneToMany.Values.Any(n => n.Anomaly.CurrentlyIncreasingDefenseOrDR)));

    /// <summary>
    /// 当前 Boss（或附属 NPC）是否处于无敌（或几乎无敌）状态。
    /// </summary>
    public bool NPCIsImmune
    {
        get
        {
            return Valid && NPC.active && (CheckImmune(NPC) || (HasOneToMany && CustomOneToMany.Values.Any(CheckImmune)));
            static bool CheckImmune(NPC npc) => npc.Anomaly.CurrentlyImmune || npc.dontTakeDamage || npc.immortal;
        }
    }

    /// <summary>
    /// 获取血条区域的高度（像素），用于布局排列。
    /// </summary>
    /// <param name="isFancy">是否为华丽样式。</param>
    public int GetHeight(bool isFancy) => isFancy ? (IsSmall ? 76 : 100) : 84;

    /// <summary>
    /// 开启/关闭动画的完成比例，用于控制主血条透明度。
    /// </summary>
    public float AnimationCompletionRatio { get; private set; }

    /// <summary>
    /// 第二种动画完成比例，用于控制其他元素的动画完成度或透明度。
    /// </summary>
    public float AnimationCompletionRatio2 => Math.Clamp((AnimationCompletionRatio - 0.4f) * (5f / 3f), 0f, 1f);

    /// <summary>
    /// 构造 BetterBossHPUI 实例，绑定到指定的 NPC。
    /// </summary>
    /// <param name="npc">要显示血条的 NPC。</param>
    public BossHealthBar(NPC npc)
    {
        NPC = npc;
        Identifier = NPC.Identifier;
        AnomalyNPC = NPC.Anomaly;

        HasOneToMany = OneToMany.TryGetValue(NPCType, out int[] value);
        CustomOneToManyIndexes = value;
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
                    HealthAtStartOfCombo = PreviousLife;
                ComboDamageCountdown = 30;
            }
            PreviousLife = CombinedNPCLife;

            if (Valid)
            {
                if (ComboDamageCountdown > 0)
                    ComboDamageCountdown--;

                OpenAnimationTimer = Math.Clamp(OpenAnimationTimer + 1, 0, 120);

                EnrageTimer = Math.Clamp(EnrageTimer + (NPCIsEnraged ? 1 : -4), 0, 120);
                IncreasingDefenseOrDRTimer = Math.Clamp(IncreasingDefenseOrDRTimer + (NPCIsIncreasingDefenseOrDR ? 1 : -4), 0, 120);
                ImmunityTimer = Math.Clamp(ImmunityTimer + (NPCIsImmune ? 1 : -1), 0, LockAnimationFrameCount * LockAnimationSpeed);

                CloseAnimationTimer = Math.Clamp(CloseAnimationTimer - 2, 0, 120);
            }
            else
            {
                ComboDamageCountdown = 0;

                EnrageTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                IncreasingDefenseOrDRTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                ImmunityTimer = Math.Clamp(ImmunityTimer - LockAnimationSpeed, 0, LockAnimationFrameCount * LockAnimationSpeed);
                CloseAnimationTimer++;
            }

            AnimationCompletionRatio = CloseAnimationTimer > 0
                ? 1f - MathHelper.Clamp(CloseAnimationTimer / 120f, 0f, 1f)
                : MathHelper.Clamp(OpenAnimationTimer / 80f, 0f, 1f);

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

        foreach (CustomLifeFunction func in LifeFunctions)
        {
            if (func(this))
                return;
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

        foreach (CustomLifeFunction func in LifeFunctions)
        {
            if (func(this))
                return;
        }

        long result = NPC.lifeMax;
        foreach ((long identifier, NPC npc) in CustomOneToMany)
        {
            if (npc.Identifier == identifier && npc.active && npc.life > 0)
                result += npc.lifeMax;
        }
        CombinedNPCMaxLife = result;

        if (CombinedNPCMaxLife != 0L && (InitialMaxLife == 0L || InitialMaxLife < CombinedNPCMaxLife))
            InitialMaxLife = CombinedNPCMaxLife;
    }

    /// <summary>
    /// 更新血量阈值指示器的计时及生命周期。
    /// </summary>
    protected void UpdateIndicators()
    {
        foreach (HPThresholdIndicator indicator in AnomalyNPC.HPThresholdIndicators)
            indicator?.Update(NPC, this);

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
        if (NPC.ModNPC is IAnomalyModNPC anomalyNPC)
        {
            result &= anomalyNPC.PreUpdateCalBossBar(this);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PreUpdateBossBar)))
        {
            result &= npcBehavior.PreUpdateBossBar(this);
            hasSingle = true;
        }
        foreach (AnomalyGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalNPCBehavior>(nameof(AnomalyGlobalNPCBehavior.PreUpdateCalBossBar)))
            result &= anomalyGNPCBehavior.PreUpdateCalBossBar(NPC, this, hasSingle);
        return result;
    }

    /// <summary>
    /// 执行更新后的钩子，通知相关行为对象。
    /// </summary>
    protected void PostUpdate()
    {
        bool hasSingle = false;
        if (NPC.ModNPC is IAnomalyModNPC anomalyNPC)
        {
            anomalyNPC.PostUpdateCalBossBar(this);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PostUpdateBossBar)))
        {
            npcBehavior.PostUpdateBossBar(this);
            hasSingle = true;
        }
        foreach (AnomalyGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalNPCBehavior>(nameof(AnomalyGlobalNPCBehavior.PostUpdateCalBossBar)))
            anomalyGNPCBehavior.PostUpdateCalBossBar(NPC, this, hasSingle);
    }

    /// <summary>
    /// 绘制血条的完整流程，包含前置钩子、主体、后置钩子。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch。</param>
    /// <param name="x">绘制区域的 X 坐标（将被方法修改以进行布局）。</param>
    /// <param name="y">绘制区域的 Y 坐标（将被方法修改以进行布局）。</param>
    /// <param name="isFancy">是否为华丽样式。</param>
    public virtual void Draw(SpriteBatch spriteBatch, ref int x, ref int y, bool isFancy)
    {
        if (PreDraw(spriteBatch, ref x, ref y))
        {
            int fancyOffsetY = IsSmall ? 26 : 20;

            if (isFancy)
                DrawFancyBar(spriteBatch, x, y + fancyOffsetY, NPC);
            else
                DrawRetroBar(spriteBatch, x, y);

            (float sin, float cos) = TOMathUtils.TimeWrappingFunction.GetTimeSinCos(0.5f, 1f, 0f, true);

            Color? mainColor;
            if (AnomalyNPC.IsRunningAnomalyAI)
            {
                mainColor = Color.Lerp(AnomalySharedData.GetGradientColor(0.1f), AnomalySharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * cos * 0.8f);
                if (Aroma)
                    mainColor = Color.Lerp(mainColor.Value, AnomalySharedData.AromalyColor, sin);
                if (IncreasingDefenseOrDRTimer > 0)
                    mainColor = Color.Lerp(mainColor.Value, Color.LightGray * 0.7f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 0.6f));
                if (EnrageTimer > 0)
                    mainColor = Color.Lerp(mainColor.Value, Color.Red * 0.6f, Math.Clamp(EnrageTimer / 80f, 0f, 0.4f));
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
                borderColor = Color.Lerp(AnomalySharedData.GetGradientColor(0.1f), AnomalySharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * sin * 0.8f);
                if (Aroma)
                    borderColor = Color.Lerp(borderColor.Value, AnomalySharedData.AromalyColor, sin);
                if (IncreasingDefenseOrDRTimer > 0)
                    borderColor = Color.Lerp(borderColor.Value, Color.LightGray * 0.2f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 0.6f));
                if (EnrageTimer > 0)
                    borderColor = Color.Lerp(borderColor.Value, Color.Red * 0.6f, Math.Clamp(EnrageTimer / 80f, 0f, 0.4f));
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

            if (isFancy)
            {
                bool small = IsSmall;
                int x2 = small ? x + 100 : x;
                int y2 = small ? y + fancyOffsetY - 6 : y + fancyOffsetY;
                DrawNPCName(spriteBatch, x + 430, y2 + 12, null, mainColor, borderColor, borderWidth);
                DrawBigLifeText(spriteBatch, x2 + 36, y2 + 13);
                DrawExtraSmallText(spriteBatch, x2 + 38, y2 + 38, true);
            }
            else
            {
                DrawNPCName(spriteBatch, x + 400, y + 35, null, mainColor, borderColor, borderWidth);
                DrawBigLifeText(spriteBatch, x, y + 34);
                DrawExtraSmallText(spriteBatch, x, y + 60, false);
            }

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
        if (NPC.ModNPC is IAnomalyModNPC anomalyNPC)
        {
            result &= anomalyNPC.PreDrawCalBossBar(this, spriteBatch, ref x, ref y);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PreDrawBossBar)))
        {
            result &= npcBehavior.PreDrawBossBar(this, spriteBatch, ref x, ref y);
            hasSingle = true;
        }
        foreach (AnomalyGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalNPCBehavior>(nameof(AnomalyGlobalNPCBehavior.PreDrawCalBossBar)))
            result &= anomalyGNPCBehavior.PreDrawCalBossBar(NPC, this, spriteBatch, ref x, ref y, hasSingle);
        return result;
    }

    /// <summary>
    /// 绘制后通知相关行为对象。
    /// </summary>
    protected void PostDraw(SpriteBatch spriteBatch, int x, int y)
    {
        bool hasSingle = false;
        if (NPC.ModNPC is IAnomalyModNPC anomalyNPC)
        {
            anomalyNPC.PostDrawCalBossBar(this, spriteBatch, x, y);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.PostDrawBossBar)))
        {
            npcBehavior.PostDrawBossBar(this, spriteBatch, x, y);
            hasSingle = true;
        }
        foreach (AnomalyGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalNPCBehavior>(nameof(AnomalyGlobalNPCBehavior.PostDrawCalBossBar)))
            anomalyGNPCBehavior.PostDrawCalBossBar(NPC, this, spriteBatch, x, y, hasSingle);
    }

    #region 公共绘制方法
    /// <summary>
    /// 绘制华丽样式的血条。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="adjustedX">已调整的 X 坐标（调用方应包含固定偏移）。</param>
    /// <param name="adjustedY">已调整的 Y 坐标（调用方应包含固定偏移）。</param>
    /// <param name="newColor">可覆盖的颜色值。</param>
    public void DrawFancyBar(SpriteBatch spriteBatch, int adjustedX, int adjustedY, NPC npc)
    {
        bool small = IsSmall;
        if (small)
            adjustedX += 100;

        //背景板

        Vector2 position = new(adjustedX, adjustedY);

        int fillerOffsetX = 34;
        int fillerOffsetY = small ? 16 : 18;
        Vector2 barBasePosition = position + new Vector2(fillerOffsetX, fillerOffsetY);

        float baseBarOpacity = Math.Clamp(AnimationCompletionRatio * 3f, 0f, 1f);
        spriteBatch.Draw(small ? BossBarTextures.Fancy.BaseBarFillerSmall : BossBarTextures.Fancy.BaseBarFiller, barBasePosition, Color.White * baseBarOpacity);

        //主血条和连击残影条

        float totalWidth = small ? 304f : 404f;
        float mainBarWidth = totalWidth * Math.Min(AnimationCompletionRatio2, NPCLifeRatio);
        float comboBarWidth = Math.Max(totalWidth * HealthAtStartOfCombo / InitialMaxLife * (AnimationCompletionRatio - 0.5f) * 2f - mainBarWidth, 0);
        if (ComboDamageCountdown < 6)
            comboBarWidth = comboBarWidth * ComboDamageCountdown / 6;

        Rectangle mainBarDestinationRectangle = new(adjustedX + fillerOffsetX, adjustedY + fillerOffsetY, (int)mainBarWidth, 10);
        Rectangle comboBarDestinationRectangle = new(adjustedX + fillerOffsetX + (int)mainBarWidth, adjustedY + fillerOffsetY, (int)comboBarWidth, 10);

        if (AnomalySharedData.ShouldUseShaders)
        {
            if (NPC.ModNPC is IAnomalyNPCWithCustomShaderBar anomalyNPCWithCustomShader)
            {
                using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp))
                {
                    anomalyNPCWithCustomShader.ApplyCustomMainBossBarShader(this, spriteBatch, mainBarDestinationRectangle);
                    DrawHPBar(spriteBatch, mainBarDestinationRectangle, true);
                }
            }
            else if (NPC.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.ApplyCustomMainBossBarShader)))
            {
                using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp))
                {
                    npcBehavior.ApplyCustomMainBossBarShader(this, spriteBatch, mainBarDestinationRectangle);
                    DrawHPBar(spriteBatch, mainBarDestinationRectangle, true);
                }
            }
            else
                DrawHPBar(spriteBatch, mainBarDestinationRectangle, false);
        }
        else
            DrawHPBar(spriteBatch, mainBarDestinationRectangle, false);

        spriteBatch.Draw(TOTextures.FillerTexture, comboBarDestinationRectangle, Color.Red);

        //血条外框及血条锁

        spriteBatch.Draw(small ? BossBarTextures.Fancy.BaseBarSmall : BossBarTextures.Fancy.BaseBar, position, null, Color.White * baseBarOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        if (!small && ImmunityTimer > 0)
        {
            Texture2D lockTexture = BossBarTextures.Fancy.BarLock;
            Rectangle frame = Utils.Frame(lockTexture, 1, LockAnimationFrameCount + 1, 0, ImmunityTimer / LockAnimationSpeed);
            spriteBatch.Draw(lockTexture, position, frame, Color.White * baseBarOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        //Boss图标

        if (!small && TextureAssets.NpcHeadBoss.TryGetValue(TONPCUtils.GetBossHeadTextureIndexBetter(npc), out Asset<Texture2D> headAsset) && headAsset?.Value is not null)
        {
            Vector2 bossHeadOffset = new(461f, 23f);
            spriteBatch.DrawFromCenter(headAsset.Value, position + bossHeadOffset, null, Color.White * AnimationCompletionRatio2);
        }

        void DrawHPBar(SpriteBatch spriteBatch, Rectangle destinationRectangle, bool useWhiteColor) =>
            spriteBatch.Draw(TOTextures.FillerTexture, destinationRectangle, useWhiteColor ? Color.White : new Color(0xE4, 0xCD, 0x42));

        //阈值指示器

        if (AnomalyNPC.IsRunningAnomalyAI)
        {
            Vector2 indicatorBasePosition = barBasePosition + new Vector2(2f, 5f);
            float TotalWidth2 = small ? 300f : 400f;

            foreach (HPThresholdIndicator indicator in AnomalyNPC.HPThresholdIndicators)
            {
                if (indicator is null)
                    continue;
                float indicatorPositionX = TotalWidth2 * indicator.GetValue(NPC, this);
                Vector2 indicatorPosition = indicatorBasePosition + new Vector2(indicatorPositionX, 0f);

                indicator.Draw(npc, this, spriteBatch, indicatorPosition);
            }
        }
    }

    /// <summary>
    /// 绘制复古样式的血条。
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="adjustedX"></param>
    /// <param name="adjustedY"></param>
    public void DrawRetroBar(SpriteBatch spriteBatch, int x, int y)
    {
        DrawMainBar(spriteBatch, x, y + 43);
        DrawComboBar(spriteBatch, x, y + 43);

        (float sin, float cos) = TOMathUtils.TimeWrappingFunction.GetTimeSinCos(0.5f, 1f, 0f, true);

        Color seperatorColor;
        if (AnomalyNPC.IsRunningAnomalyAI)
        {
            seperatorColor = Color.Lerp(BaseColor, Color.Lerp(AnomalySharedData.GetGradientColor(0.25f), AnomalySharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * sin), Math.Clamp(AnomalyNPC.AnomalyAITimer / 120f, 0f, 1f));
            if (Aroma)
                seperatorColor = Color.Lerp(seperatorColor, AnomalySharedData.AromalyColor, sin);
            if (IncreasingDefenseOrDRTimer > 0)
                seperatorColor = Color.Lerp(seperatorColor, Color.LightGray * 0.7f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 0.6f));
            if (EnrageTimer > 0)
                seperatorColor = Color.Lerp(seperatorColor, Color.Red * 0.6f, Math.Clamp(EnrageTimer / 80f, 0f, 0.4f));
        }
        else if (EnrageTimer > 0)
            seperatorColor = Color.Lerp(BaseColor, Color.Red * 0.5f, Math.Clamp(EnrageTimer / 80f, 0f, 1f));
        else if (IncreasingDefenseOrDRTimer > 0)
            seperatorColor = Color.Lerp(BaseColor, Color.LightGray * 0.7f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 1f));
        else
            seperatorColor = BaseColor;
        seperatorColor *= AnimationCompletionRatio2;

        DrawSeperatorBar(spriteBatch, x, y + 33, seperatorColor);

        void DrawMainBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
        {
            int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
            Rectangle destinationRectangle = new(x, y, mainBarWidth, BossBarTextures.Retro.MainBar.Height);

            if (AnomalySharedData.ShouldUseShaders)
            {
                if (NPC.ModNPC is IAnomalyNPCWithCustomShaderBar anomalyNPCWithCustomShader)
                {
                    using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp))
                    {
                        anomalyNPCWithCustomShader.ApplyCustomMainBossBarShader(this, spriteBatch, destinationRectangle);
                        DrawCore(spriteBatch, destinationRectangle, null);
                    }
                }
                else if (NPC.TryGetBehavior(out AnomalyNPCBehavior npcBehavior, nameof(AnomalyNPCBehavior.ApplyCustomMainBossBarShader)))
                {
                    using (spriteBatch.Scope(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp))
                    {
                        npcBehavior.ApplyCustomMainBossBarShader(this, spriteBatch, destinationRectangle);
                        DrawCore(spriteBatch, destinationRectangle, null);
                    }
                }
                else
                    DrawCore(spriteBatch, destinationRectangle, newColor);
            }
            else
                DrawCore(spriteBatch, destinationRectangle, newColor);

            void DrawCore(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color? newColor)
            {
                Color color = newColor ?? Color.White * AnimationCompletionRatio * AnimationCompletionRatio2;
                spriteBatch.Draw(BossBarTextures.Retro.MainBar, destinationRectangle, color);
            }
        }

        void DrawComboBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
        {
            if (ComboDamageCountdown <= 0)
                return;

            int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
            int comboHPBarWidth = (int)(400 * (float)HealthAtStartOfCombo / InitialMaxLife) - mainBarWidth;
            if (ComboDamageCountdown < 6)
                comboHPBarWidth = comboHPBarWidth * ComboDamageCountdown / 6;
            Color color = newColor ?? Color.White * AnimationCompletionRatio * AnimationCompletionRatio2;

            spriteBatch.Draw(BossBarTextures.Retro.ComboBar, new Rectangle(x + mainBarWidth, y, comboHPBarWidth, BossBarTextures.Retro.ComboBar.Height), color);
        }

        void DrawSeperatorBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
        {
            Color color = newColor ?? BaseColor * AnimationCompletionRatio * AnimationCompletionRatio2;
            spriteBatch.Draw(BossBarTextures.Retro.SeperatorBar, new Rectangle(x, y, 400, 6), color);
        }
    }

    /// <summary>
    /// 绘制 NPC 名称，支持自定义名称覆盖、描边及颜色效果。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="adjustedX">已调整的 X 坐标（调用方应包含固定偏移）。</param>
    /// <param name="adjustedY">已调整的 Y 坐标（调用方应包含固定偏移）。</param>
    /// <param name="overrideText">覆盖的文本，为 <see langword="null"/> 时使用默认逻辑。</param>
    /// <param name="mainColor">可选的主文字颜色。</param>
    /// <param name="borderColor">可选的描边颜色。</param>
    /// <param name="borderWidth">描边宽度。</param>
    public void DrawNPCName(SpriteBatch spriteBatch, int adjustedX, int adjustedY, string overrideText = null, Color? mainColor = null, Color? borderColor = null, float borderWidth = 0f)
    {
        string name = overrideText;
        if (name is null)
        {
            foreach (CustomOverridingNameFunction func in OverridingNameFunctions)
            {
                if (func(this, out name))
                    break;
            }
        }
        name ??= NPC.FullName;
        Vector2 npcNameSize = MouseFont.MeasureString(name);
        Vector2 baseDrawPosition = new(adjustedX - npcNameSize.X, adjustedY - npcNameSize.Y);
        DrawBorderStringEightWay_Loop(spriteBatch, MouseFont, name, baseDrawPosition, mainColor, borderColor, Color.White * AnimationCompletionRatio2, Color.Black * 0.2f * AnimationCompletionRatio2, 8, borderWidth, 1f);
    }

    /// <summary>
    /// 绘制大型生命百分比文本。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="adjustedX">已调整的 X 坐标（调用方应包含固定偏移）。</param>
    /// <param name="adjustedY">已调整的 Y 坐标（调用方应包含固定偏移）。</param>
    /// <param name="overrideText">覆盖的文本，为 <see langword="null"/> 时自动生成百分比。</param>
    public void DrawBigLifeText(SpriteBatch spriteBatch, int adjustedX, int adjustedY, string overrideText = null)
    {
        string bigLifeText = overrideText ?? (NPCLifeRatio == 0f ? "0%" : (NPCLifeRatio * 100f).ToString("N1") + "%");
        Vector2 bigLifeTextSize = BossBarTextures.BigLifeFont.MeasureString(bigLifeText);
        TODrawUtils.DrawStringWithBorder(spriteBatch, BossBarTextures.BigLifeFont, bigLifeText, new Vector2(adjustedX, adjustedY - bigLifeTextSize.Y), MainColor * AnimationCompletionRatio2, MainBorderColor * 0.25f * AnimationCompletionRatio2);
    }

    /// <summary>
    /// 绘制额外小文本（如具体生命数值、附属实体数量等）。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="adjustedX">已调整的 X 坐标（调用方应包含固定偏移）。</param>
    /// <param name="adjustedY">已调整的 Y 坐标（调用方应包含固定偏移）。</param>
    /// <param name="useNewLine">是否在生命值文本与额外文本之间使用换行符作为分隔。</param>
    /// <param name="overrideText">完全覆盖的文本，不为 <see langword="null"/> 时跳过所有自定义和默认逻辑。</param>
    public void DrawExtraSmallText(SpriteBatch spriteBatch, int adjustedX, int adjustedY, bool useNewLine, string overrideText = null)
    {
        string smallText = "";
        if (overrideText is not null)
        {
            smallText = overrideText;
            goto Draw;
        }

        foreach (CustomSmallTextFunction func in SmallTextFunctions)
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
            smallText = (useNewLine ? Environment.NewLine : "    ") + $"{extensionName}: {extraEntities}";
        }

    Orig:
        smallText = $"{CombinedNPCLife} / {InitialMaxLife}" + smallText;
    Draw:
        TODrawUtils.DrawStringWithBorder(spriteBatch, ItemStackFont, smallText, new Vector2(adjustedX, adjustedY), Color.White * (float)AnimationCompletionRatio2, Color.Black * (float)AnimationCompletionRatio2 * 0.24f, scale: 0.8f);
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
                TODrawUtils.DrawStringWithBorder(spriteBatch, font, text, baseDrawPosition + new PolarVector2(borderWidth, MathHelper.TwoPi / round * i), mainColor.Value, borderColor.Value, scale: scale);
        }
        TODrawUtils.DrawStringWithBorder(spriteBatch, font, text, baseDrawPosition, mainColor2, borderColor2, scale: scale);
    }
    #endregion

    void IContentLoader.PostSetupContent()
    {
        AssetRepository assets = AnomalyMain.Instance.Assets;

        OneToMany = new Dictionary<int, int[]>()
        {
            [NPCID.EaterofWorldsHead] = [NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail],
            [NPCID.MoonLordCore] = [NPCID.MoonLordHead, NPCID.MoonLordHand],
            [NPCID.SkeletronHead] = [NPCID.SkeletronHand],
            [NPCID.SkeletronPrime] = [NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeCannon, NPCID.PrimeLaser],
            [NPCID.Golem] = [NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree],
            [NPCID.BrainofCthulhu] = [NPCID.Creeper],
            [NPCID.MartianSaucerCore] = [NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon],
            [NPCID.PirateShip] = [NPCID.PirateShipCannon],
        };

        EntityExtensionHandler = new Dictionary<int, BossEntityExtension>()
        {
            [NPCID.EaterofWorldsHead] = new BossEntityExtension(this.GetText("ExtensionName.Segments"), NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail),
            [NPCID.BrainofCthulhu] = new BossEntityExtension(this.GetText("ExtensionName.Creepers"), NPCID.Creeper),
            [NPCID.SkeletronHead] = new BossEntityExtension(this.GetText("ExtensionName.Hands"), NPCID.SkeletronHand),
            [NPCID.SkeletronPrime] = new BossEntityExtension(this.GetText("ExtensionName.Arms"), NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser),
            [NPCID.MartianSaucerCore] = new BossEntityExtension(this.GetText("ExtensionName.Guns"), NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon),
            [NPCID.PirateShip] = new BossEntityExtension(this.GetText("ExtensionName.Cannons"), NPCID.PirateShipCannon),
        };

        // 月球领主血量合并
        LifeFunctions.Add(b =>
        {
            NPC npc = b.NPC;
            if (npc.type == NPCID.MoonLordCore)
            {
                long lifeMax = npc.lifeMax;
                long life = npc.life;
                if (npc.ai[0] == 2f)
                    life = 0L;

                foreach (NPC n in NPC.ActiveNPCs)
                {
                    bool isMoonLordPiece = n.type is NPCID.MoonLordHand or NPCID.MoonLordHead;
                    if (!isMoonLordPiece || n.ai[3] != npc.whoAmI)
                        continue;

                    lifeMax = n.lifeMax;
                    life += n.life;
                }

                b.CombinedNPCMaxLife = lifeMax;
                b.CombinedNPCLife = life;
                if (b.CombinedNPCMaxLife != 0L && (b.InitialMaxLife == 0L || b.InitialMaxLife < b.CombinedNPCMaxLife))
                    b.InitialMaxLife = b.CombinedNPCMaxLife;

                return true;
            }

            return false;
        });

        // 荷兰飞盗船血量合并
        LifeFunctions.Add(b =>
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

                b.CombinedNPCMaxLife = lifeMax;
                b.CombinedNPCLife = life;
                if (b.CombinedNPCMaxLife != 0L && (b.InitialMaxLife == 0L || b.InitialMaxLife < b.CombinedNPCMaxLife))
                    b.InitialMaxLife = b.CombinedNPCMaxLife;

                return true;
            }

            return false;
        });

        //异象模式世界吞噬者血量合并
        LifeFunctions.Add(b =>
        {
            NPC npc = b.NPC;
            if (npc.type == NPCID.EaterofWorldsHead)
            {
                b.CombinedNPCMaxLife = b.InitialMaxLife = npc.lifeMax;
                b.CombinedNPCLife = npc.life;
                return true;
            }

            return false;
        });
    }

    void IContentLoader.OnModUnload()
    {
        // BaseBarFiller 已迁移到 BossBarTextures.Fancy，由其自行管理生命周期。

        OneToMany = null;
        EntityExtensionHandler = null;
        OverridingNameFunctions = null;
        LifeFunctions = null;
        SmallTextFunctions = null;
    }

    [ExtendsFromMod(CalamityModName)]
    public sealed class CalamitySupport : IContentLoader, ILocalizationPrefix
    {
        /// <summary>
        /// 本地化前缀，用于在多语言环境下正确加载血条相关文本。
        /// </summary>
        public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + "Visuals.BossHealthBar";

        [LoadPriority(-1)]
        void IContentLoader.PostSetupContent()
        {
            //添加OneToMany
            OneToMany.AddRange(new Dictionary<int, int[]>()
            {
                [NPCType<CeaselessVoid>()] = [NPCType<DarkEnergy>()],
                [NPCType<RavagerBody>()] =
                [
                    NPCType<RavagerClawRight>(),
                    NPCType<RavagerClawLeft>(),
                    NPCType<RavagerLegRight>(),
                    NPCType<RavagerLegLeft>(),
                    NPCType<RavagerHead>()
                ],
            });

            // 添加EntityExtensionHandler
            EntityExtensionHandler.Add(NPCType<CeaselessVoid>(), new BossEntityExtension(this.GetText("ExtensionName.DarkEnergy"), NPCType<DarkEnergy>()));
            EntityExtensionHandler.Add(NPCType<RavagerBody>(), new BossEntityExtension(this.GetText("ExtensionName.BodyParts"), NPCType<RavagerClawLeft>(), NPCType<RavagerClawRight>(), NPCType<RavagerLegLeft>(), NPCType<RavagerLegRight>()));

            // 阿波罗名称覆盖
            OverridingNameFunctions.Add((b, out name) =>
            {
                if (b.NPC.ModNPC is Apollo apollo)
                {
                    name = Language.GetTextValue(AnomalySharedData.CalamityModLocalizationPrefix + "UI.ExoTwinsName" + (apollo.exoMechdusa ? "Hekate" : "Normal"));
                    return true;
                }
                name = null;
                return false;
            });

            // 分裂蠕虫血量合并
            LifeFunctions.Add(b =>
            {
                NPC npc = b.NPC;
                if (npc.CalamityNPC.SplittingWorm)
                {
                    long lifeMax = 0L;
                    long life = 0L;
                    NPC currentSegment = npc;

                    int failsafeCounter = 0;
                    while (Main.npc.IndexInRange((int)currentSegment.ai[0]) && Main.npc[(int)currentSegment.ai[0]].ai[1] == currentSegment.whoAmI)
                    {
                        if (!currentSegment.active)
                            break;

                        lifeMax += currentSegment.lifeMax;
                        life += currentSegment.life;
                        currentSegment = Main.npc[(int)currentSegment.ai[0]];

                        failsafeCounter++;
                        if (failsafeCounter > Main.maxNPCs)
                            break;
                    }

                    b.CombinedNPCMaxLife = lifeMax;
                    b.CombinedNPCLife = life;
                    if (b.CombinedNPCMaxLife != 0L && (b.InitialMaxLife == 0L || b.InitialMaxLife < b.CombinedNPCMaxLife))
                        b.InitialMaxLife = b.CombinedNPCMaxLife;

                    return true;
                }

                return false;
            });
        }
    }
}