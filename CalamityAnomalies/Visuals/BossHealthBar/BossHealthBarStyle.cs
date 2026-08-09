// Developed by ColdsUx

using System.Diagnostics.CodeAnalysis;
using CalamityMod.Events;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI.BigProgressBar;

namespace CalamityAnomalies.Visuals;

/// <summary>
/// CA Boss 血条系统的主管理类，负责血条资源加载、数据绑定、更新与绘制调度。
/// 继承自 <see cref="ModBossBarStyle"/> 并实现 <see cref="IContentLoader"/>。
/// </summary>
public class BossHealthBarStyle : ModBossBarStyle, IContentLoader, ILocalizationPrefix
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
    /// 判断指定 NPC 是否满足特殊血量获取条件的委托。
    /// </summary>
    /// <param name="npc">待检测的 NPC。</param>
    /// <returns>满足条件返回 <see langword="true"/>。</returns>
    public delegate bool NPCSpecialHPGetRequirement(NPC npc);

    /// <summary>
    /// 获取指定 NPC 特殊血量值的委托。
    /// </summary>
    /// <param name="npc">目标 NPC。</param>
    /// <param name="checkingForMaxLife">是否为获取最大生命值。</param>
    /// <returns>血量值。</returns>
    public delegate long NPCSpecialHPGetFunction(NPC npc, bool checkingForMaxLife);

    /// <summary>
    /// 用于自定义覆盖 Boss 名称的委托。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    /// <param name="overridingName">输出参数，用于提供覆盖名称。</param>
    /// <returns>返回 <see langword="true"/> 以替代默认 NPC 全名。</returns>
    public delegate bool BetterOverridingNameFunction(BossHealthBar bar, [NotNullWhen(true)] out string overridingName);

    /// <summary>
    /// 用于自定义或补充血量统计逻辑的委托。返回 <see langword="true"/> 表示拦截了默认的血量计算。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    public delegate bool BetterLifeFunction(BossHealthBar bar);

    /// <summary>
    /// 用于在血量条下方额外显示小文本，可完全禁用原始小文本。
    /// </summary>
    /// <param name="bar">Boss 血条。</param>
    /// <param name="text">输出参数，用于提供额外小文本。</param>
    /// <param name="disableOrig">输出参数，指示是否禁用原始的生命值小文本。</param>
    /// <returns>返回 <see langword="true"/> 表示拦截了灾厄默认的拓展小文本显示逻辑。</returns>
    public delegate bool BetterSmallTextFunction(BossHealthBar bar, [NotNullWhen(true)] out string text, out bool disableOrig);

    /// <summary>
    /// 血条 UI 纹理资源的路径前缀。
    /// </summary>
    public const string Path = "CalamityAnomalies/Visuals/BossHealthBar/";

    /// <summary>
    /// 最大可同时存在的血条记录数量。
    /// </summary>
    public const int MaxBars = 6;

    /// <summary>
    /// 最大同时显示的活动 NPC 的血条数量。
    /// </summary>
    public const int MaxActiveBars = 4;

    /// <summary>
    /// 一对多映射，将主体 NPC 类型映射到其附属 NPC 类型数组，用于合并血量。
    /// </summary>
    public static Dictionary<int, int[]> OneToMany = [];

    /// <summary>
    /// 血条排除列表，此列表中的 NPC 永远不会显示血条。
    /// </summary>
    public static HashSet<int> ExclusionList = [];

    /// <summary>
    /// 小 Boss 血条列表，这些 NPC 即使不是标准 Boss 也会显示血条。
    /// </summary>
    public static HashSet<int> Minibosses = [];

    /// <summary>
    /// 实体扩展信息处理器，存储特定 Boss 的额外附属实体名称与类型。
    /// </summary>
    public static Dictionary<int, BossEntityExtension> EntityExtensionHandler = [];

    /// <summary>
    /// 注册的自定义名称覆盖函数列表。
    /// </summary>
    public static List<BetterOverridingNameFunction> OverridingNameFunctions = [];

    /// <summary>
    /// 注册的自定义血量统计函数列表。先匹配到的函数将阻止后续默认计算。
    /// </summary>
    public static List<BetterLifeFunction> LifeFunctions = [];

    /// <summary>
    /// 注册的额外小文本生成函数列表。
    /// </summary>
    public static List<BetterSmallTextFunction> SmallTextFunctions = [];

    /// <summary>
    /// 鼠标文字字体（用于绘制 Boss 名称）。
    /// </summary>
    public static DynamicSpriteFont MouseFont => FontAssets.MouseText?.Value;

    /// <summary>
    /// 物品堆叠数字字体（用于额外小文本）。
    /// </summary>
    public static DynamicSpriteFont ItemStackFont => FontAssets.ItemStack?.Value;

    /// <summary>
    /// 当前活跃的 BetterBossHPUI 实例，以 NPC 的 Identifier 为键。
    /// </summary>
    public static readonly Dictionary<long, BossHealthBar> CurrentBars = [];

    /// <summary>
    /// 每帧更新时用于标记当前仍有效的 NPC 标识符，无效的血条将在后续被移除。
    /// </summary>
    public static readonly HashSet<long> CurrentValidIdentifiers = [];

    /// <summary>
    /// 指示阻止原版绘制，完全使用自定义血条系统进行绘制。
    /// </summary>
    public override bool PreventDraw => true;

    /// <summary>
    /// 本地化前缀，用于在多语言环境下正确加载血条相关文本。
    /// </summary>
    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "Visuals.BossHealthBar";

    /// <summary>
    /// 每帧更新血条系统，清理无效条目，为符合条件的 NPC 创建新血条并推进动画。
    /// </summary>
    /// <param name="currentBar">原版大进度条接口，此处未使用。</param>
    /// <param name="info">进度条信息，此处未使用。</param>
    public override void Update(IBigProgressBar currentBar, ref BigProgressBarInfo info)
    {
        CurrentValidIdentifiers.Clear();
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            long npcIdentifier = npc.Identifier;
            if (CurrentBars.ContainsKey(npcIdentifier))
                CurrentValidIdentifiers.Add(npcIdentifier);
            else if (CurrentBars.Count < MaxBars && ((npc.IsBossEnemy && !ExclusionList.Contains(npc.type)) || Minibosses.Contains(npc.type) || npc.CalamityNPC.CanHaveBossHealthBar))
            {
                CurrentBars.Add(npcIdentifier, new BossHealthBar(npc));
                CurrentValidIdentifiers.Add(npcIdentifier);
            }
        }

        foreach ((long identifier, BossHealthBar newBar) in CurrentBars)
        {
            newBar.Update(CurrentValidIdentifiers.Contains(identifier));
            if (newBar.CloseAnimationTimer >= 120)
                CurrentBars.Remove(identifier);
        }
    }

    /// <summary>
    /// 绘制所有活跃的血条，按有效状态排序，并自动调整纵向布局。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="currentBar">原版大进度条接口，此处未使用。</param>
    /// <param name="info">进度条信息，此处未使用。</param>
    public override void Draw(SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
    {
        int x = Main.screenWidth
            - (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing || AcidRainEvent.AcidRainEventIsOngoing ? 670 : 420);
        int y = Main.screenHeight - 25;

        int activeCount = 0;

        foreach (BossHealthBar newBar in
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
    /// 模组加载时完成资源请求和数据初始化，确保血条系统在游戏中可用。
    /// </summary>
    void IContentLoader.PostSetupContent()
    {
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
            [ModContent.NPCType<CeaselessVoid>()] = [ModContent.NPCType<DarkEnergy>()],
            [ModContent.NPCType<RavagerBody>()] =
            [
                ModContent.NPCType<RavagerClawRight>(),
                ModContent.NPCType<RavagerClawLeft>(),
                ModContent.NPCType<RavagerLegRight>(),
                ModContent.NPCType<RavagerLegLeft>(),
                ModContent.NPCType<RavagerHead>()
            ],
        };

        ExclusionList =
        [
            NPCID.None,
            NPCID.EaterofWorldsBody,
            NPCID.EaterofWorldsTail,
            NPCID.MoonLordFreeEye,
            NPCID.MoonLordHead,
            NPCID.MoonLordHand,
            NPCID.WyvernLegs,
            NPCID.WyvernBody,
            NPCID.WyvernBody2,
            NPCID.WyvernBody3,
            NPCID.WyvernTail,
            ModContent.NPCType<AquaticScourgeBody>(),
            ModContent.NPCType<AquaticScourgeBodyAlt>(),
            ModContent.NPCType<AquaticScourgeTail>(),
            ModContent.NPCType<AstrumDeusBody>(),
            ModContent.NPCType<AstrumDeusTail>(),
            ModContent.NPCType<BrainIllusion>(),
            ModContent.NPCType<DesertScourgeBody>(),
            ModContent.NPCType<DesertScourgeTail>(),
            ModContent.NPCType<FalseBrain>(),
            ModContent.NPCType<SlimeGodCore>(),
            ModContent.NPCType<StormWeaverBody>(),
            ModContent.NPCType<StormWeaverTail>(),
            ModContent.NPCType<DevourerofGodsBody>(),
            ModContent.NPCType<DevourerofGodsTail>(),
            ModContent.NPCType<ThanatosBody1>(),
            ModContent.NPCType<ThanatosBody2>(),
            ModContent.NPCType<ThanatosTail>(),
            ModContent.NPCType<AresGaussNuke>(),
            ModContent.NPCType<AresLaserCannon>(),
            ModContent.NPCType<AresPlasmaFlamethrower>(),
            ModContent.NPCType<AresTeslaCannon>(),
            ModContent.NPCType<Artemis>(),
        ];

        Minibosses =
        [
            NPCID.DD2Betsy,
            NPCID.DD2OgreT2,
            NPCID.DD2OgreT3,
            NPCID.DD2DarkMageT1,
            NPCID.DD2DarkMageT3,
            NPCID.DungeonGuardian,
            NPCID.GoblinSummoner,
            NPCID.WyvernHead,
            NPCID.Paladin,
            NPCID.IceGolem,
            NPCID.SandElemental,
            NPCID.BigMimicCorruption,
            NPCID.BigMimicCrimson,
            NPCID.BigMimicHallow,
            NPCID.BloodNautilus,
            NPCID.MourningWood,
            NPCID.Pumpking,
            NPCID.Everscream,
            NPCID.SantaNK1,
            NPCID.IceQueen,
            NPCID.Mothron,
            NPCID.MartianSaucerCore,
            NPCID.LunarTowerSolar,
            NPCID.LunarTowerVortex,
            NPCID.LunarTowerNebula,
            NPCID.LunarTowerStardust,
            NPCID.PirateShip,
            ModContent.NPCType<GiantClam>(),
            ModContent.NPCType<PerforatorHeadSmall>(),
            ModContent.NPCType<PerforatorHeadMedium>(),
            ModContent.NPCType<PerforatorHeadLarge>(),
            ModContent.NPCType<CloudElemental>(),
            ModContent.NPCType<EarthElemental>(),
            ModContent.NPCType<GreatSandShark>(),
            ModContent.NPCType<PlaguebringerMiniboss>(),
            ModContent.NPCType<Cataclysm>(),
            ModContent.NPCType<Catastrophe>(),
            ModContent.NPCType<SupremeCataclysm>(),
            ModContent.NPCType<SupremeCatastrophe>(),
            ModContent.NPCType<ProvSpawnDefense>(),
            ModContent.NPCType<ProvSpawnOffense>(),
            ModContent.NPCType<ProvSpawnHealer>(),
            ModContent.NPCType<ProfanedGuardianDefender>(),
            ModContent.NPCType<ProfanedGuardianHealer>()
        ];

        EntityExtensionHandler = new Dictionary<int, BossEntityExtension>()
        {
            [NPCID.EaterofWorldsHead] = new BossEntityExtension(this.GetText("ExtensionName.Segments"), NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail),
            [NPCID.BrainofCthulhu] = new BossEntityExtension(this.GetText("ExtensionName.Creepers"), NPCID.Creeper),
            [NPCID.SkeletronHead] = new BossEntityExtension(this.GetText("ExtensionName.Hands"), NPCID.SkeletronHand),
            [NPCID.SkeletronPrime] = new BossEntityExtension(this.GetText("ExtensionName.Arms"), NPCID.PrimeCannon, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeLaser),
            [NPCID.MartianSaucerCore] = new BossEntityExtension(this.GetText("ExtensionName.Guns"), NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon),
            [NPCID.PirateShip] = new BossEntityExtension(this.GetText("ExtensionName.Cannons"), NPCID.PirateShipCannon),
            [ModContent.NPCType<CeaselessVoid>()] = new BossEntityExtension(this.GetText("ExtensionName.DarkEnergy"), ModContent.NPCType<DarkEnergy>()),
            [ModContent.NPCType<RavagerBody>()] = new BossEntityExtension(this.GetText("ExtensionName.BodyParts"), ModContent.NPCType<RavagerClawLeft>(), ModContent.NPCType<RavagerClawRight>(), ModContent.NPCType<RavagerLegLeft>(), ModContent.NPCType<RavagerLegRight>()),
        };

        // 阿波罗名称覆盖
        OverridingNameFunctions.Add((b, out name) =>
        {
            if (b.NPC.ModNPC is Apollo apollo)
            {
                name = Language.GetTextValue(CASharedData.CalamityModLocalizationPrefix + "UI.ExoTwinsName" + (apollo.exoMechdusa ? "Hekate" : "Normal"));
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

                    if (n.CalamityNPC.newAI[0] == 1f)
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
    }

    /// <summary>
    /// 模组卸载时清理所有静态数据。
    /// </summary>
    void IContentLoader.OnModUnload()
    {
        OverridingNameFunctions.Clear();
        LifeFunctions.Clear();
        SmallTextFunctions.Clear();
        ExclusionList.Clear();
        Minibosses.Clear();
        OneToMany.Clear();
        EntityExtensionHandler.Clear();
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