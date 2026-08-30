using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;
using Terraria.GameContent.Events;
using static Terraria.ModLoader.ModContent;

namespace Anomalies.Visuals.BossBar;

/// <summary>
/// Anomaly Boss 血条系统的主管理类，负责血条资源加载、数据绑定、更新与绘制调度。
/// 继承自 <see cref="ModBossBarStyle"/> 并实现 <see cref="IContentLoader"/>。
/// </summary>
public class BossBarHandler : IContentLoader
{
    /// <summary>
    /// 最大可同时存在的血条记录数量。
    /// </summary>
    public static int MaxBars = 6;

    /// <summary>
    /// 最大同时显示的活动 NPC 的血条数量。
    /// </summary>
    public static int MaxActiveBars = 4;

    /// <summary>
    /// 血条排除列表，此列表中的 NPC 永远不会显示血条。
    /// </summary>
    public static HashSet<int> ExclusionList = [];

    /// <summary>
    /// 小 Boss 血条列表，这些 NPC 即使不是标准 Boss 也会显示血条。
    /// </summary>
    public static HashSet<int> Minibosses = [];

    /// <summary>
    /// 当前活跃的 BetterBossHPUI 实例，以 NPC 的 Identifier 为键。
    /// </summary>
    public static readonly Dictionary<long, BossHealthBar> CurrentBars = [];

    /// <summary>
    /// 每帧更新时用于标记当前仍有效的 NPC 标识符，无效的血条将在后续被移除。
    /// </summary>
    public static readonly HashSet<long> CurrentValidIdentifiers = [];

    /// <summary>
    /// 总更新逻辑，适用于华丽与复古两种样式。
    /// </summary>
    public static void Update()
    {
        CurrentValidIdentifiers.Clear();
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            long npcIdentifier = npc.Identifier;
            if (CurrentBars.ContainsKey(npcIdentifier))
                CurrentValidIdentifiers.Add(npcIdentifier);
            else if (CurrentBars.Count < MaxBars && ((npc.IsBossEnemy && !ExclusionList.Contains(npc.type)) || Minibosses.Contains(npc.type) || npc.Anomaly.CanHaveBossHealthBar))
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
    /// 总绘制逻辑，适用于华丽与复古两种样式。
    /// </summary>
    /// <param name="spriteBatch">画布。</param>
    /// <param name="isFancy">是否为华丽样式。</param>
    public static void Draw(SpriteBatch spriteBatch, bool isFancy)
    {
        int x = Main.screenWidth - 420;
        if (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing)
            x -= 250;
        if (isFancy)
            x -= 90;

        int y = Main.screenHeight - 20;

        int activeCount = 0;

        foreach (BossHealthBar newBar in
            from pair in CurrentBars
            let newBar = pair.Value
            orderby newBar.Valid descending, pair.Key ascending
            select newBar)
        {
            y -= BossHealthBar.GetHeight(isFancy);
            if (activeCount >= MaxActiveBars && newBar.Valid)
                continue;
            newBar.Draw(spriteBatch, ref x, ref y, isFancy);
            if (newBar.Valid)
                activeCount++;
        }
    }

    /// <summary>
    /// 模组加载时完成资源请求和数据初始化，确保血条系统在游戏中可用。
    /// </summary>
    void IContentLoader.PostSetupContent()
    {
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
        ];
    }

    /// <summary>
    /// 模组卸载时清理所有静态数据。
    /// </summary>
    void IContentLoader.OnModUnload()
    {
        ExclusionList = null;
        Minibosses = null;
    }

    /// <summary>
    /// 世界加载时清空已有的血条记录，开始新的世界。
    /// </summary>
    void IContentLoader.OnWorldLoad() => CurrentBars.Clear();

    /// <summary>
    /// 世界卸载时清空血条记录，避免数据残留。
    /// </summary>
    void IContentLoader.OnWorldUnload() => CurrentBars.Clear();

    [ExtendsFromMod(CalamityModName)]
    public class CalamitySupport : IContentLoader
    {
        [LoadPriority(-1)]
        void IContentLoader.PostSetupContent()
        {
            ExclusionList = [..ExclusionList,
            NPCType<AquaticScourgeBody>(),
            NPCType<AquaticScourgeBodyAlt>(),
            NPCType<AquaticScourgeTail>(),
            NPCType<AstrumDeusBody>(),
            NPCType<AstrumDeusTail>(),
            NPCType<BrainIllusion>(),
            NPCType<DesertScourgeBody>(),
            NPCType<DesertScourgeTail>(),
            NPCType<FalseBrain>(),
            NPCType<SlimeGodCore>(),
            NPCType<StormWeaverBody>(),
            NPCType<StormWeaverTail>(),
            NPCType<DevourerofGodsBody>(),
            NPCType<DevourerofGodsTail>(),
            NPCType<ThanatosBody1>(),
            NPCType<ThanatosBody2>(),
            NPCType<ThanatosTail>(),
            NPCType<AresGaussNuke>(),
            NPCType<AresLaserCannon>(),
            NPCType<AresPlasmaFlamethrower>(),
            NPCType<AresTeslaCannon>(),
            NPCType<Artemis>(),
        ];

            Minibosses = [..Minibosses,
            NPCType<GiantClam>(),
            NPCType<PerforatorHeadSmall>(),
            NPCType<PerforatorHeadMedium>(),
            NPCType<PerforatorHeadLarge>(),
            NPCType<CloudElemental>(),
            NPCType<EarthElemental>(),
            NPCType<GreatSandShark>(),
            NPCType<PlaguebringerMiniboss>(),
            NPCType<Cataclysm>(),
            NPCType<Catastrophe>(),
            NPCType<SupremeCataclysm>(),
            NPCType<SupremeCatastrophe>(),
            NPCType<ProvSpawnDefense>(),
            NPCType<ProvSpawnOffense>(),
            NPCType<ProvSpawnHealer>(),
            NPCType<ProfanedGuardianDefender>(),
            NPCType<ProfanedGuardianHealer>()
            ];
        }
    }
}
