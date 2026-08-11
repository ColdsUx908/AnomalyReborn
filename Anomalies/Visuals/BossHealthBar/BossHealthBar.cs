// Developed by ColdsUx

using static Anomalies.Visuals.BossHealthBarStyle;

namespace Anomalies.Visuals;

/// <summary>
/// Boss血条类。
/// </summary>
public class BossHealthBar : IContentLoader
{
    internal static Asset<DynamicSpriteFont> _BigLifeFont;
    internal static Asset<Texture2D> _MainBar;
    internal static Asset<Texture2D> _ComboBar;
    internal static Asset<Texture2D> _SeperatorBar;
    internal static Asset<Texture2D> _PhaseIndicator;
    internal static Asset<Texture2D> _SubPhaseIndicator;
    internal static Asset<Texture2D> _PhaseIndicatorBorder;
    internal static Asset<Texture2D> _SubPhaseIndicatorBorder;

    /// <summary>
    /// 血条百分比数字所用字体。
    /// </summary>
    public static DynamicSpriteFont BigLifeFont => _BigLifeFont.Value;

    /// <summary>
    /// 主血量条纹理。
    /// </summary>
    public static Texture2D MainBar => _MainBar.Value;

    /// <summary>
    /// 连击伤害残影条纹理。
    /// </summary>
    public static Texture2D ComboBar => _ComboBar.Value;

    /// <summary>
    /// 血量分隔条纹理。
    /// </summary>
    public static Texture2D SeperatorBar => _SeperatorBar.Value;

    /// <summary>
    /// 阶段血量阈值指示器纹理。
    /// </summary>
    public static Texture2D PhaseIndicator => _PhaseIndicator?.Value;

    /// <summary>
    /// 亚阶段血量阈值指示器纹理。
    /// </summary>
    public static Texture2D SubPhaseIndicator => _SubPhaseIndicator?.Value;

    /// <summary>
    /// 阶段血量阈值指示器边框纹理。
    /// </summary>
    public static Texture2D PhaseIndicatorBorder => _PhaseIndicatorBorder?.Value;

    /// <summary>
    /// 亚阶段血量阈值指示器边框纹理。
    /// </summary>
    public static Texture2D SubPhaseIndicatorBorder => _SubPhaseIndicatorBorder?.Value;

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
    /// 激怒状态过渡计时器，用于平滑颜色变化。
    /// </summary>
    public int EnrageTimer;

    /// <summary>
    /// 防御/伤害减免提升状态过渡计时器。
    /// </summary>
    public int IncreasingDefenseOrDRTimer;

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
                    HealthAtStartOfCombo = CombinedNPCLife;
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

        foreach (BetterLifeFunction func in LifeFunctions)
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

        foreach (BetterLifeFunction func in LifeFunctions)
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
        if (NPC.ModNPC is IAnomalyModNPC caNPC)
        {
            result &= caNPC.PreUpdateCalBossBar(this);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PreUpdateCalBossBar)))
        {
            result &= npcBehavior.PreUpdateCalBossBar(this);
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
        if (NPC.ModNPC is IAnomalyModNPC caNPC)
        {
            caNPC.PostUpdateCalBossBar(this);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PostUpdateCalBossBar)))
        {
            npcBehavior.PostUpdateCalBossBar(this);
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

            DrawSeperatorBar(spriteBatch, x, y, seperatorColor);

            Color? mainColor;
            if (AnomalyNPC.IsRunningAnomalyAI)
            {
                mainColor = Color.Lerp(AnomalySharedData.GetGradientColor(0.1f), AnomalySharedData.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * cos * 0.8f);
                if (Aroma)
                    mainColor = Color.Lerp(seperatorColor, AnomalySharedData.AromalyColor, sin);
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
                    borderColor = Color.Lerp(seperatorColor, AnomalySharedData.AromalyColor, sin);
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
        if (NPC.ModNPC is IAnomalyModNPC caNPC)
        {
            result &= caNPC.PreDrawCalBossBar(this, spriteBatch, ref x, ref y);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PreDrawCalBossBar)))
        {
            result &= npcBehavior.PreDrawCalBossBar(this, spriteBatch, ref x, ref y);
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
        if (NPC.ModNPC is IAnomalyModNPC caNPC)
        {
            caNPC.PostDrawCalBossBar(this, spriteBatch, x, y);
            hasSingle = true;
        }
        if (NPC.TryGetBehavior(out AnomalySingleNPCBehavior npcBehavior, nameof(AnomalySingleNPCBehavior.PostDrawCalBossBar)))
        {
            npcBehavior.PostDrawCalBossBar(this, spriteBatch, x, y);
            hasSingle = true;
        }
        foreach (AnomalyGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<AnomalyGlobalNPCBehavior>(nameof(AnomalyGlobalNPCBehavior.PostDrawCalBossBar)))
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
        spriteBatch.Draw(MainBar, new Rectangle(x, y + 43, mainBarWidth, MainBar.Height), color);
    }

    /// <summary>
    /// 绘制连击伤害指示条（白色残影部分）。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="newColor">可覆盖的颜色值。</param>
    public void DrawComboBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        if (ComboDamageCountdown <= 0)
            return;

        int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
        int comboHPBarWidth = (int)(400 * (float)HealthAtStartOfCombo / InitialMaxLife) - mainBarWidth;
        if (ComboDamageCountdown < 6)
            comboHPBarWidth = comboHPBarWidth * ComboDamageCountdown / 6;
        Color color = newColor ?? Color.White * AnimationCompletionRatio * AnimationCompletionRatio2;

        spriteBatch.Draw(ComboBar, new Rectangle(x + mainBarWidth, y + 43, comboHPBarWidth, ComboBar.Height), color);
    }

    /// <summary>
    /// 绘制分隔条及异象模式下的血量阈值指示器。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="newColor">可覆盖的颜色值。</param>
    public void DrawSeperatorBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
    {
        Color color = newColor ?? BaseColor * AnimationCompletionRatio * AnimationCompletionRatio2;
        spriteBatch.Draw(SeperatorBar, new Rectangle(x, y + 33, 400, 6), color);

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

            Texture2D borderTexture = indicator.IsSubPhaseIndicator ? SubPhaseIndicatorBorder : PhaseIndicatorBorder;
            Texture2D texture = indicator.IsSubPhaseIndicator ? SubPhaseIndicator : PhaseIndicator;
            spriteBatch.DrawFromCenter(borderTexture, center, null, color * thresholdAnimationCompletion, scale: borderIntensity * 0.5f);
            spriteBatch.DrawFromCenter(texture, center, null, Color.White * AnimationCompletionRatio * AnimationCompletionRatio2 * thresholdAnimationCompletion, scale: 0.5f);
        }
    }

    /// <summary>
    /// 绘制 NPC 名称，支持自定义名称覆盖、描边及颜色效果。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="overrideText">覆盖的文本，为 <see langword="null"/> 时使用默认逻辑。</param>
    /// <param name="mainColor">可选的主文字颜色。</param>
    /// <param name="borderColor">可选的描边颜色。</param>
    /// <param name="borderWidth">描边宽度。</param>
    public void DrawNPCName(SpriteBatch spriteBatch, int x, int y, string overrideText = null, Color? mainColor = null, Color? borderColor = null, float borderWidth = 0f)
    {
        string name = overrideText;
        if (name is null)
        {
            foreach (BetterOverridingNameFunction func in OverridingNameFunctions)
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
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="overrideText">覆盖的文本，为 <see langword="null"/> 时自动生成百分比。</param>
    public void DrawBigLifeText(SpriteBatch spriteBatch, int x, int y, string overrideText = null)
    {
        string bigLifeText = overrideText ?? (NPCLifeRatio == 0f ? "0%" : (NPCLifeRatio * 100f).ToString("N1") + "%");
        Vector2 bigLifeTextSize = BigLifeFont.MeasureString(bigLifeText);
        TODrawUtils.DrawBorderString(spriteBatch, BigLifeFont, bigLifeText, new Vector2(x, y + 34 - bigLifeTextSize.Y), MainColor * AnimationCompletionRatio2, MainBorderColor * 0.25f * AnimationCompletionRatio2);
    }

    /// <summary>
    /// 绘制额外小文本（如具体生命数值、附属实体数量等）。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="x">X 坐标。</param>
    /// <param name="y">Y 坐标。</param>
    /// <param name="overrideText">完全覆盖的文本，不为 <see langword="null"/> 时跳过所有自定义和默认逻辑。</param>
    public void DrawExtraSmallText(SpriteBatch spriteBatch, int x, int y, string overrideText = null)
    {
        float whiteColorAlpha = OpenAnimationTimer switch
        {
            4 or 8 or 16 => Main.rand.NextFloat(0.7f, 0.8f),
            3 or 7 or 15 => Main.rand.NextFloat(0.4f, 0.5f),
            _ => AnimationCompletionRatio
        };

        string smallText = "";
        if (overrideText is not null)
        {
            smallText = overrideText;
            goto Draw;
        }

        foreach (BetterSmallTextFunction func in SmallTextFunctions)
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
    #endregion

    void IContentLoader.PostSetupContent()
    {
        AssetRepository assets = AnomalyMain.Instance.Assets;
        const string Path = "Visuals/BossHealthBar/";

        _BigLifeFont = assets.Request<DynamicSpriteFont>(Path + "BigLifeFont");
        _MainBar = assets.Request<Texture2D>(Path + "MainBar");
        _ComboBar = assets.Request<Texture2D>(Path + "ComboBar");
        _SeperatorBar = assets.Request<Texture2D>(Path + "SeperatorBar");
        _PhaseIndicator = assets.Request<Texture2D>(Path + "PhaseIndicator");
        _SubPhaseIndicator = assets.Request<Texture2D>(Path + "SubPhaseIndicator");
        _PhaseIndicatorBorder = assets.Request<Texture2D>(Path + "PhaseIndicatorBorder");
        _SubPhaseIndicatorBorder = assets.Request<Texture2D>(Path + "SubPhaseIndicatorBorder");
    }

    void IContentLoader.OnModUnload()
    {
        _BigLifeFont = null;
        _MainBar = null;
        _ComboBar = null;
        _SeperatorBar = null;
        _PhaseIndicator = null;
        _SubPhaseIndicator = null;
        _PhaseIndicatorBorder = null;
        _SubPhaseIndicatorBorder = null;
    }
}