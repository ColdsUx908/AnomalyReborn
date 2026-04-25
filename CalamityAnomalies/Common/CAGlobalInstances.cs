// Designed by ColdsUx

using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using Transoceanic.Framework.Helpers.Utilities;

namespace CalamityAnomalies.Common;

public sealed class CAPlayer : ModPlayer
{
    //数据变量按字母顺序排列
    public int Coldheart_Phase;
    public int Coldheart_SubPhase;

    public bool Debuff_DimensionalRend;

    public PlayerDownedBossCalamity DownedBossCalamity = new();
    public PlayerDownedBossCalamity DownedBossAnomaly = new();

    public HysteresisBoolean YharimsGift;
    //public YharimsGift_CurrentBlessing YharimsGift_Blessing = YharimsGift_CurrentBlessing.None;
    //public readonly SmoothInt[] YharimsGift_Change = new SmoothInt[YharimsGift_Handler._totalBlessings];
    public Item YharimsGift_Last;

    public override ModPlayer Clone(Player newEntity)
    {
        CAPlayer clone = (CAPlayer)base.Clone(newEntity);

        clone.Coldheart_Phase = Coldheart_Phase;
        clone.Coldheart_SubPhase = Coldheart_SubPhase;

        clone.Debuff_DimensionalRend = Debuff_DimensionalRend;
        clone.DownedBossCalamity = DownedBossCalamity;
        clone.DownedBossAnomaly = DownedBossAnomaly;

        clone.YharimsGift = YharimsGift;
        //clone.YharimsGift_Blessing = YharimsGift_Blessing;
        //Array.Copy(YharimsGift_Change, clone.YharimsGift_Change, YharimsGift_Change.Length);
        clone.YharimsGift_Last = YharimsGift_Last;

        return clone;
    }

    public override void ResetEffects()
    {
        Debuff_DimensionalRend = false;
    }
}

public class PlayerDownedBossCalamity : PlayerDownedBoss
{
    public bool DesertScourge { get; set; }
    public bool Crabulon { get; set; }
    public bool EvilBoss2 { get; set; }
    public bool HiveMind { get; set; }
    public bool Perforator { get; set; }
    public bool SlimeGod { get; set; }
    public bool Cryogen { get; set; }
    public bool AquaticScourge { get; set; }
    public bool BrimstoneElemental { get; set; }
    public bool CalamitasClone { get; set; }
    public bool Leviathan { get; set; }
    public bool AstrumAureus { get; set; }
    public bool Goliath { get; set; }
    public bool Ravager { get; set; }
    public bool AstrumDeus { get; set; }
    public bool Guardians { get; set; }
    public bool Dragonfolly { get; set; }
    public bool Providence { get; set; }
    public bool CeaselessVoid { get; set; }
    public bool StormWeaver { get; set; }
    public bool Signus { get; set; }
    public bool Polterghast { get; set; }
    public bool BommerDuke { get; set; }
    public bool DoG { get; set; }
    public bool Yharon { get; set; }
    public bool Ares { get; set; }
    public bool Thanatos { get; set; }
    public bool ArtemisAndApollo { get; set; }
    public bool ExoMechs
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                LastBoss = true;
                if (Calamitas)
                    Focus = true;
            }
        }
    }
    public bool Calamitas
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                LastBoss = true;
                if (ExoMechs)
                    Focus = true;
            }
        }
    }
    /// <summary>
    /// 单锁（击败星流巨械和灾厄之一）。
    /// </summary>
    public bool LastBoss { get; set; }
    /// <summary>
    /// 万物的焦点。
    /// </summary>
    public bool Focus { get; set; }
    public bool PrimordialWyrm { get; set; }

    public bool GreatSandShark { get; set; }
    public bool GiantClam { get; set; }
    public bool GiantClamHardmode { get; set; }
    /// <summary>
    /// 峭咽潭。
    /// </summary>
    public bool CragmawMire { get; set; }
    /// <summary>
    /// 渊海狂鲨。
    /// </summary>
    public bool Mauler { get; set; }
    /// <summary>
    /// 辐核骇兽。
    /// </summary>
    public bool NuclearTerror { get; set; }

    public bool EoCAcidRain { get; set; }
    public bool AquaticScourgeAcidRain { get; set; }

    public bool BossRush { get; set; }

    /// <summary>
    /// 使玩家跟随世界Boss击败状态。
    /// </summary>
    public override void WorldPolluted()
    {
        base.WorldPolluted();

        //灾厄添加的原版Boss跟踪
        if (DownedBossSystem_Bridge.downedDreadnautilus)
            Dreadnautilus = true;
        if (DownedBossSystem_Bridge.downedBetsy)
            Betsy = true;

        //灾厄Boss
        if (DownedBossSystem_Bridge.downedDesertScourge)
            DesertScourge = true;
        if (DownedBossSystem_Bridge.downedCrabulon)
            Crabulon = true;
        if (DownedBossSystem_Bridge.downedHiveMind)
            HiveMind = true;
        if (DownedBossSystem_Bridge.downedPerforator)
            Perforator = true;
        EvilBoss2 = HiveMind || Perforator;
        if (DownedBossSystem_Bridge.downedSlimeGod)
            SlimeGod = true;
        if (DownedBossSystem_Bridge.downedCryogen)
            Cryogen = true;
        if (DownedBossSystem_Bridge.downedAquaticScourge)
            AquaticScourge = true;
        if (DownedBossSystem_Bridge.downedBrimstoneElemental)
            BrimstoneElemental = true;
        if (DownedBossSystem_Bridge.downedCalamitasClone)
            CalamitasClone = true;
        if (DownedBossSystem_Bridge.downedLeviathan)
            Leviathan = true;
        if (DownedBossSystem_Bridge.downedAstrumAureus)
            AstrumAureus = true;
        if (DownedBossSystem_Bridge.downedPlaguebringer)
            Goliath = true;
        if (DownedBossSystem_Bridge.downedRavager)
            Ravager = true;
        if (DownedBossSystem_Bridge.downedAstrumDeus)
            AstrumDeus = true;
        if (DownedBossSystem_Bridge.downedGuardians)
            Guardians = true;
        if (DownedBossSystem_Bridge.downedDragonfolly)
            Dragonfolly = true;
        if (DownedBossSystem_Bridge.downedProvidence)
            Providence = true;
        if (DownedBossSystem_Bridge.downedCeaselessVoid)
            CeaselessVoid = true;
        if (DownedBossSystem_Bridge.downedStormWeaver)
            StormWeaver = true;
        if (DownedBossSystem_Bridge.downedSignus)
            Signus = true;
        if (DownedBossSystem_Bridge.downedPolterghast)
            Polterghast = true;
        if (DownedBossSystem_Bridge.downedBoomerDuke)
            BommerDuke = true;
        if (DownedBossSystem_Bridge.downedDoG)
            DoG = true;
        if (DownedBossSystem_Bridge.downedYharon)
            Yharon = true;
        if (DownedBossSystem_Bridge.downedAres)
            Ares = true;
        if (DownedBossSystem_Bridge.downedThanatos)
            Thanatos = true;
        if (DownedBossSystem_Bridge.downedArtemisAndApollo)
            ArtemisAndApollo = true;
        if (DownedBossSystem_Bridge.downedExoMechs)
            ExoMechs = true;
        if (DownedBossSystem_Bridge.downedCalamitas)
            Calamitas = true;
        if (DownedBossSystem_Bridge.downedPrimordialWyrm)
            PrimordialWyrm = true;

        //灾厄迷你Boss
        if (DownedBossSystem_Bridge.downedGSS)
            GreatSandShark = true;
        if (DownedBossSystem_Bridge.downedCLAM)
            GiantClam = true;
        if (DownedBossSystem_Bridge.downedCLAMHardMode)
            GiantClamHardmode = true;
        if (DownedBossSystem_Bridge.downedCragmawMire)
            CragmawMire = true;
        if (DownedBossSystem_Bridge.downedMauler)
            Mauler = true;
        if (DownedBossSystem_Bridge.downedNuclearTerror)
            NuclearTerror = true;

        //灾厄事件
        if (DownedBossSystem_Bridge.downedEoCAcidRain)
            EoCAcidRain = true;
        if (DownedBossSystem_Bridge.downedAquaticScourgeAcidRain)
            AquaticScourgeAcidRain = true;
        if (DownedBossSystem_Bridge.downedBossRush)
            BossRush = true;
    }

    public override void SaveData(TagCompound tag, string key)
    {
        List<string> downed = [];
        SaveDataToList(downed);
        tag[key] = downed;
    }

    public override void SaveDataToList(List<string> downed)
    {
        base.SaveDataToList(downed);

        if (DesertScourge)
            downed.Add("DesertScourge");
        if (Crabulon)
            downed.Add("Crabulon");
        if (EvilBoss2)
            downed.Add("EvilBoss2");
        if (HiveMind)
            downed.Add("HiveMind");
        if (Perforator)
            downed.Add("Perforator");
        if (SlimeGod)
            downed.Add("SlimeGod");
        if (Cryogen)
            downed.Add("Cryogen");
        if (AquaticScourge)
            downed.Add("AquaticScourge");
        if (BrimstoneElemental)
            downed.Add("BrimstoneElemental");
        if (CalamitasClone)
            downed.Add("CalamitasClone");
        if (Leviathan)
            downed.Add("Leviathan");
        if (AstrumAureus)
            downed.Add("AstrumAureus");
        if (Goliath)
            downed.Add("Plaguebringer");
        if (Ravager)
            downed.Add("Ravager");
        if (AstrumDeus)
            downed.Add("AstrumDeus");
        if (Guardians)
            downed.Add("Guardians");
        if (Dragonfolly)
            downed.Add("Dragonfolly");
        if (Providence)
            downed.Add("Providence");
        if (CeaselessVoid)
            downed.Add("CeaselessVoid");
        if (StormWeaver)
            downed.Add("StormWeaver");
        if (Signus)
            downed.Add("Signus");
        if (Polterghast)
            downed.Add("Polterghast");
        if (BommerDuke)
            downed.Add("BommerDuke");
        if (DoG)
            downed.Add("DoG");
        if (Yharon)
            downed.Add("Yharon");
        if (Ares)
            downed.Add("Ares");
        if (Thanatos)
            downed.Add("Thanatos");
        if (ArtemisAndApollo)
            downed.Add("ArtemisAndApollo");
        if (ExoMechs)
            downed.Add("ExoMechs");
        if (Calamitas)
            downed.Add("Calamitas");
        if (PrimordialWyrm)
            downed.Add("PrimordialWyrm");

        if (GreatSandShark)
            downed.Add("GreatSandShark");
        if (GiantClam)
            downed.Add("GiantClam");
        if (GiantClamHardmode)
            downed.Add("GiantClamHardmode");
        if (CragmawMire)
            downed.Add("CragmawMire");
        if (Mauler)
            downed.Add("Mauler");
        if (NuclearTerror)
            downed.Add("NuclearTerror");

        if (EoCAcidRain)
            downed.Add("EoCAcidRain");
        if (AquaticScourgeAcidRain)
            downed.Add("AquaticScourgeAcidRain");
        if (BossRush)
            downed.Add("BossRush");
    }

    public override void LoadData(TagCompound tag, string key) => LoadDataFromIList(tag.GetList<string>(key));

    public override void LoadDataFromIList(IList<string> downedLoaded)
    {
        base.LoadDataFromIList(downedLoaded);

        if (downedLoaded.Contains("DesertScourge"))
            DesertScourge = true;
        if (downedLoaded.Contains("Crabulon"))
            Crabulon = true;
        if (downedLoaded.Contains("HiveMind"))
            HiveMind = true;
        if (downedLoaded.Contains("Perforator"))
            Perforator = true;
        if (downedLoaded.Contains("EvilBoss2"))
            EvilBoss2 = true;
        if (downedLoaded.Contains("SlimeGod"))
            SlimeGod = true;
        if (downedLoaded.Contains("Cryogen"))
            Cryogen = true;
        if (downedLoaded.Contains("AquaticScourge"))
            AquaticScourge = true;
        if (downedLoaded.Contains("BrimstoneElemental"))
            BrimstoneElemental = true;
        if (downedLoaded.Contains("CalamitasClone"))
            CalamitasClone = true;
        if (downedLoaded.Contains("Leviathan"))
            Leviathan = true;
        if (downedLoaded.Contains("AstrumAureus"))
            AstrumAureus = true;
        if (downedLoaded.Contains("Plaguebringer"))
            Goliath = true;
        if (downedLoaded.Contains("Ravager"))
            Ravager = true;
        if (downedLoaded.Contains("AstrumDeus"))
            AstrumDeus = true;
        if (downedLoaded.Contains("Guardians"))
            Guardians = true;
        if (downedLoaded.Contains("Dragonfolly"))
            Dragonfolly = true;
        if (downedLoaded.Contains("Providence"))
            Providence = true;
        if (downedLoaded.Contains("CeaselessVoid"))
            CeaselessVoid = true;
        if (downedLoaded.Contains("StormWeaver"))
            StormWeaver = true;
        if (downedLoaded.Contains("Signus"))
            Signus = true;
        if (downedLoaded.Contains("Polterghast"))
            Polterghast = true;
        if (downedLoaded.Contains("BommerDuke"))
            BommerDuke = true;
        if (downedLoaded.Contains("DoG"))
            DoG = true;
        if (downedLoaded.Contains("Yharon"))
            Yharon = true;
        if (downedLoaded.Contains("Ares"))
            Ares = true;
        if (downedLoaded.Contains("Thanatos"))
            Thanatos = true;
        if (downedLoaded.Contains("ArtemisAndApollo"))
            ArtemisAndApollo = true;
        if (downedLoaded.Contains("ExoMechs"))
            ExoMechs = true;
        if (downedLoaded.Contains("Calamitas"))
            Calamitas = true;
        if (downedLoaded.Contains("PrimordialWyrm"))
            PrimordialWyrm = true;

        if (downedLoaded.Contains("GreatSandShark"))
            GreatSandShark = true;
        if (downedLoaded.Contains("GiantClam"))
            GiantClam = true;
        if (downedLoaded.Contains("GiantClamHardmode"))
            GiantClamHardmode = true;
        if (downedLoaded.Contains("CragmawMire"))
            CragmawMire = true;
        if (downedLoaded.Contains("Mauler"))
            Mauler = true;
        if (downedLoaded.Contains("NuclearTerror"))
            NuclearTerror = true;

        if (downedLoaded.Contains("EoCAcidRain"))
            EoCAcidRain = true;
        if (downedLoaded.Contains("AquaticScourgeAcidRain"))
            AquaticScourgeAcidRain = true;
        if (downedLoaded.Contains("BossRush"))
            BossRush = true;
    }

    /// <summary>
    /// 击杀Boss时的处理。
    /// </summary>
    /// <param name="npc"></param>
    public void BossesOnKill(NPC npc)
    {
        switch (npc.ModNPC)
        {
            case null:
                switch (npc.type)
                {
                    // 原版Boss
                    case NPCID.KingSlime:
                        KingSlime = true;
                        break;
                    case NPCID.EyeofCthulhu:
                        EyeOfCthulhu = true;
                        break;
                    case NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail when npc.boss:
                        EaterOfWorld = true;
                        break;
                    case NPCID.BrainofCthulhu:
                        BrainOfCthulhu = true;
                        break;
                    case NPCID.QueenBee:
                        QueenBee = true;
                        break;
                    case NPCID.SkeletronHead:
                        Skeletron = true;
                        break;
                    case NPCID.Deerclops:
                        Deerclops = true;
                        break;
                    case NPCID.WallofFlesh:
                        WallOfFlesh = true;
                        break;
                    case NPCID.TheDestroyer:
                        Destroyer = true;
                        break;
                    case int _ when TONPCUtils.IsDefeatingTwins(npc):
                        Twins = true;
                        break;
                    case NPCID.SkeletronPrime:
                        SkeletronPrime = true;
                        break;
                    case NPCID.Plantera:
                        Plantera = true;
                        break;
                    case NPCID.Golem:
                        Golem = true;
                        break;
                    case NPCID.CultistBoss:
                        LunaticCultist = true;
                        break;
                    case NPCID.MoonLordCore:
                        MoonLord = true;
                        break;

                    // 原版事件Boss
                    case NPCID.MourningWood:
                        MourningWood = true;
                        break;
                    case NPCID.Pumpking:
                        Pumpking = true;
                        break;
                    case NPCID.Everscream:
                        Everscream = true;
                        break;
                    case NPCID.SantaNK1:
                        SantaNK1 = true;
                        break;
                    case NPCID.IceQueen:
                        IceQueen = true;
                        break;
                    case NPCID.DD2Betsy:
                        Betsy = true;
                        break;
                    case NPCID.BloodNautilus:
                        Dreadnautilus = true;
                        break;
                    case NPCID.LunarTowerSolar:
                        SolarTower = true;
                        break;
                    case NPCID.LunarTowerVortex:
                        VortexTower = true;
                        break;
                    case NPCID.LunarTowerNebula:
                        NebulaTower = true;
                        break;
                    case NPCID.LunarTowerStardust:
                        StardustTower = true;
                        break;
                }
                break;
            //灾厄Boss
            case DesertScourgeHead:
                DesertScourge = true;
                break;
            case Crabulon _:
                Crabulon = true;
                break;
            case HiveMind _:
                HiveMind = true;
                break;
            case PerforatorHive:
                Perforator = true;
                break;
            case SlimeGodCore:
                SlimeGod = true;
                break;
            case Cryogen _:
                Cryogen = true;
                break;
            case AquaticScourgeHead:
                AquaticScourge = true;
                break;
            case BrimstoneElemental _:
                BrimstoneElemental = true;
                break;
            case CalamitasClone _:
                CalamitasClone = true;
                break;
            case var _ when CAUtils.IsDefeatingLeviathan(npc):
                Leviathan = true;
                break;
            case AstrumAureus _:
                AstrumAureus = true;
                break;
            case PlaguebringerGoliath:
                Goliath = true;
                break;
            case RavagerBody:
                Ravager = true;
                break;
            case AstrumDeusHead:
                AstrumDeus = true;
                break;
            case var _ when CAUtils.IsDefeatingProfanedGuardians(npc):
                Guardians = true;
                break;
            case Dragonfolly _:
                Dragonfolly = true;
                break;
            case Providence _:
                Providence = true;
                break;
            case CeaselessVoid _:
                CeaselessVoid = true;
                break;
            case StormWeaverHead:
                StormWeaver = true;
                break;
            case Signus _:
                Signus = true;
                break;
            case Polterghast _:
                Polterghast = true;
                break;
            case OldDuke:
                BommerDuke = true;
                break;
            case DevourerofGodsHead:
                DoG = true;
                break;
            case Yharon _:
                Yharon = true;
                break;
            case var _ when CAUtils.IsDefeatingExoMechs(npc):
                ExoMechs = true;
                break;
            case SupremeCalamitas:
                Calamitas = true;
                break;
            case EidolonWyrmHead:
                PrimordialWyrm = true;
                break;
            //灾厄迷你Boss
            case GreatSandShark _:
                GreatSandShark = true;
                break;
            case Clam:
                GiantClam = true;
                if (Main.hardMode)
                    GiantClamHardmode = true;
                break;
            case CragmawMire _:
                CragmawMire = true;
                break;
            case Mauler _:
                Mauler = true;
                break;
            case NuclearTerror _:
                NuclearTerror = true;
                break;
        }
    }
}

public sealed class CAGlobalNPC : GlobalNPC, IContentLoader
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int AISlot = 33;
    private const int AISlot2 = 17;
    private const int AISlot3 = 132;
    private const int AISlot4 = 33;

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot3];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot4];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^4].bits;
    private ref BitArray32 InternalAIChanged32_2 => ref InternalAnomalyAI32[^3].bits;
    private ref BitArray32 InternalAIChanged32_3 => ref InternalAnomalyAI32[^2].bits;
    private ref BitArray32 InternalAIChanged32_4 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        CAGlobalNPC clone = (CAGlobalNPC)base.Clone(from, to);

        Array.Copy(AnomalyAI32, clone.AnomalyAI32, AISlot);
        Array.Copy(AnomalyAI64, clone.AnomalyAI64, AISlot2);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot3);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot4);

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.WriteChangedAI32(binaryWriter, AnomalyAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteChangedAI32(binaryWriter, InternalAnomalyAI32, 4);
        TONetUtils.WriteChangedAI64(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReadChangedAI32(binaryReader, AnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, AnomalyAI64);
        TONetUtils.ReadChangedAI32(binaryReader, InternalAnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, InternalAnomalyAI64);
    }

    #region 额外数据
    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI32[0].bits[0];
        set
        {
            if (InternalAnomalyAI32[0].bits[0] != value)
            {
                InternalAnomalyAI32[0].bits[0] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public bool Debuff_DimensionalRend
    {
        get => InternalAnomalyAI32[0].bits[1];
        set
        {
            if (InternalAnomalyAI32[0].bits[1] != value)
            {
                InternalAnomalyAI32[0].bits[1] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public int AnomalyKilltime;

    public int AnomalyAITimer;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int AnomalyUltraAITimer;
    public int AnomalyUltraBarTimer;

    /// <summary>
    /// 额外DR，不受任何修改DR的机制影响。
    /// </summary>
    /// <remarks>谨慎使用。</remarks>
    public float ExtraDR
    {
        get => InternalAnomalyAI32[5].f;
        set
        {
            if (InternalAnomalyAI32[5].f != value)
            {
                InternalAnomalyAI32[5].f = value;
                InternalAIChanged32[5] = true;
            }
        }
    }
    #endregion 额外数据
}

public sealed class CAGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    public readonly Union32[] AnomalyAI32 = new Union32[AISlot];
    public readonly Union64[] AnomalyAI64 = new Union64[AISlot2];

    public ref BitArray32 AIChanged32 => ref AnomalyAI32[^1].bits;
    public ref BitArray64 AIChanged64 => ref AnomalyAI64[^1].bits;

    private readonly Union32[] InternalAnomalyAI32 = new Union32[AISlot];
    private readonly Union64[] InternalAnomalyAI64 = new Union64[AISlot2];

    private ref BitArray32 InternalAIChanged32 => ref InternalAnomalyAI32[^1].bits;
    private ref BitArray64 InternalAIChanged64 => ref InternalAnomalyAI64[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        CAGlobalProjectile clone = (CAGlobalProjectile)base.Clone(from, to);

        Array.Copy(AnomalyAI32, clone.AnomalyAI32, AISlot);
        Array.Copy(AnomalyAI64, clone.AnomalyAI64, AISlot2);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot);
        Array.Copy(InternalAnomalyAI32, clone.InternalAnomalyAI32, AISlot2);

        return clone;
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.WriteChangedAI32(binaryWriter, AnomalyAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, AnomalyAI64, 1);
        TONetUtils.WriteChangedAI32(binaryWriter, InternalAnomalyAI32, 1);
        TONetUtils.WriteChangedAI64(binaryWriter, InternalAnomalyAI64, 1);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReadChangedAI32(binaryReader, AnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, AnomalyAI64);
        TONetUtils.ReadChangedAI32(binaryReader, InternalAnomalyAI32);
        TONetUtils.ReadChangedAI64(binaryReader, InternalAnomalyAI64);
    }

    #region 额外数据
    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI32[0].bits[0];
        set
        {
            if (InternalAnomalyAI32[0].bits[0] != value)
            {
                InternalAnomalyAI32[0].bits[0] = value;
                InternalAIChanged32[0] = true;
            }
        }
    }

    public int OverrideType
    {
        get => InternalAnomalyAI32[1].i;
        set
        {
            if (InternalAnomalyAI32[1].i != value)
            {
                InternalAnomalyAI32[1].i = value;
                InternalAIChanged32[1] = true;
            }
        }
    }
    #endregion 额外数据
}

public sealed class CAGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

#if DEBUG
    /// <summary>
    /// 调试用数据。
    /// <br/>不同实体可能会有不同的用途。
    /// </summary>
    public readonly Union64[] DebugData = new Union64[4];
#endif

    private const int dataSlot = 64;
    private const int dataSlot2 = 32;

    public readonly Union32[] Data = new Union32[dataSlot];
    public readonly Union64[] Data2 = new Union64[dataSlot2];

    public override GlobalItem Clone(Item from, Item to)
    {
        CAGlobalItem clone = (CAGlobalItem)base.Clone(from, to);

        Array.Copy(Data, clone.Data, dataSlot);
        Array.Copy(Data2, clone.Data2, dataSlot2);

        return clone;
    }
}
