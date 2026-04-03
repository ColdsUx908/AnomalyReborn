namespace Transoceanic.DataStructures;

public class PlayerDownedBoss
{
    public bool KingSlime { get; set; }
    public bool EyeOfCthulhu { get; set; }
    public bool EvilBoss { get; set; }
    public bool EaterOfWorld { get; set; }
    public bool BrainOfCthulhu { get; set; }
    public bool QueenBee { get; set; }
    public bool Skeletron { get; set; }
    public bool Deerclops { get; set; }
    public bool WallOfFlesh { get; set; }
    public bool DukeFishron { get; set; }
    public bool QueenSlime { get; set; }
    public bool Destroyer { get; set; }
    public bool Twins { get; set; }
    public bool SkeletronPrime { get; set; }
    public bool MechBossAny { get; set; }
    public bool MechBossAnyTwo { get; set; }
    public bool MechBossAll { get; set; }
    public bool Plantera { get; set; }
    public bool EmpressOfLight { get; set; }
    public bool Golem { get; set; }
    public bool LunaticCultist { get; set; }
    public bool MoonLord { get; set; }

    public bool Goblins { get; set; }
    public bool Frost { get; set; }
    public bool Pirates { get; set; }
    public bool Martians { get; set; }

    public bool MourningWood { get; set; }
    public bool Pumpking { get; set; }
    public bool Everscream { get; set; }
    public bool SantaNK1 { get; set; }
    public bool IceQueen { get; set; }
    public bool Betsy { get; set; }
    public bool Dreadnautilus { get; set; }

    public bool SolarTower { get; set; }
    public bool VortexTower { get; set; }
    public bool NebulaTower { get; set; }
    public bool StardustTower { get; set; }

    public virtual void WorldPolluted()
    {
        if (NPC.downedSlimeKing)
            KingSlime = true;
        if (NPC.downedBoss1)
            EyeOfCthulhu = true;
        if (NPC.downedBoss2)
            EvilBoss = true;
        if (NPC.downedQueenBee)
            QueenBee = true;
        if (NPC.downedBoss3)
            Skeletron = true;
        if (NPC.downedDeerclops)
            Deerclops = true;
        if (Main.hardMode)
            WallOfFlesh = true;
        if (NPC.downedFishron)
            DukeFishron = true;
        if (NPC.downedQueenSlime)
            QueenSlime = true;
        if (NPC.downedMechBoss1)
            Destroyer = true;
        if (NPC.downedMechBoss2)
            Twins = true;
        if (NPC.downedMechBoss3)
            SkeletronPrime = true;
        if (NPC.downedMechBossAny)
            MechBossAny = true;
        MechBossAnyTwo = TOMathUtils.AtLeastXTrue(2, Destroyer, Twins, SkeletronPrime); //完全依靠游戏内更新
        MechBossAll = Destroyer && Twins && SkeletronPrime; //完全依靠游戏内更新
        if (NPC.downedPlantBoss)
            Plantera = true;
        if (NPC.downedEmpressOfLight)
            EmpressOfLight = true;
        if (NPC.downedGolemBoss)
            Golem = true;
        if (NPC.downedAncientCultist)
            LunaticCultist = true;
        if (NPC.downedMoonlord)
            MoonLord = true;

        if (NPC.downedGoblins)
            Goblins = true;
        if (NPC.downedFrost)
            Frost = true;
        if (NPC.downedPirates)
            Pirates = true;
        if (NPC.downedMartians)
            Martians = true;

        if (NPC.downedHalloweenTree)
            MourningWood = true;
        if (NPC.downedHalloweenKing)
            Pumpking = true;
        if (NPC.downedChristmasTree)
            Everscream = true;
        if (NPC.downedChristmasSantank)
            SantaNK1 = true;
        if (NPC.downedChristmasIceQueen)
            IceQueen = true;

        if (NPC.downedTowerSolar)
            SolarTower = true;
        if (NPC.downedTowerVortex)
            VortexTower = true;
        if (NPC.downedTowerNebula)
            NebulaTower = true;
        if (NPC.downedTowerStardust)
            StardustTower = true;
    }

    public virtual void SaveData(TagCompound tag, string key)
    {
        List<string> downed = [];

        SaveDataToList(downed);

        tag[key] = downed;
    }

    public virtual void SaveDataToList(List<string> downed)
    {
        if (KingSlime)
            downed.Add("KingSlime");
        if (EyeOfCthulhu)
            downed.Add("EyeOfCthulhu");
        if (EvilBoss)
            downed.Add("EvilBoss");
        if (EaterOfWorld)
            downed.Add("EaterOfWorld");
        if (BrainOfCthulhu)
            downed.Add("BrainOfCthulhu");
        if (QueenBee)
            downed.Add("QueenBee");
        if (Skeletron)
            downed.Add("Skeletron");
        if (Deerclops)
            downed.Add("Deerclops");
        if (WallOfFlesh)
            downed.Add("WallOfFlesh");
        if (DukeFishron)
            downed.Add("DukeFishron");
        if (QueenSlime)
            downed.Add("QueenSlime");
        if (Destroyer)
            downed.Add("Destroyer");
        if (Twins)
            downed.Add("Twins");
        if (SkeletronPrime)
            downed.Add("SkeletronPrime");
        if (MechBossAny)
            downed.Add("MechBossAny");
        if (Plantera)
            downed.Add("Plantera");
        if (EmpressOfLight)
            downed.Add("EmpressOfLight");
        if (Golem)
            downed.Add("Golem");
        if (LunaticCultist)
            downed.Add("LunaticCultist");
        if (MoonLord)
            downed.Add("MoonLord");
        if (Goblins)
            downed.Add("Goblins");
        if (Frost)
            downed.Add("Frost");
        if (Pirates)
            downed.Add("Pirates");
        if (Martians)
            downed.Add("Martians");
        if (MourningWood)
            downed.Add("MourningWood");
        if (Pumpking)
            downed.Add("Pumpking");
        if (Everscream)
            downed.Add("Everscream");
        if (SantaNK1)
            downed.Add("SantaNK1");
        if (IceQueen)
            downed.Add("IceQueen");
        if (Betsy)
            downed.Add("Betsy");
        if (Dreadnautilus)
            downed.Add("Dreadnautilus");
        if (SolarTower)
            downed.Add("SolarTower");
        if (VortexTower)
            downed.Add("VortexTower");
        if (NebulaTower)
            downed.Add("NebulaTower");
        if (StardustTower)
            downed.Add("StardustTower");
    }

    public virtual void LoadData(TagCompound tag, string key) => LoadDataFromIList(tag.GetList<string>(key));

    public virtual void LoadDataFromIList(IList<string> downedLoaded)
    {
        if (downedLoaded.Contains("KingSlime"))
            KingSlime = true;
        if (downedLoaded.Contains("EyeOfCthulhu"))
            EyeOfCthulhu = true;
        if (downedLoaded.Contains("EvilBoss"))
            EvilBoss = true;
        if (downedLoaded.Contains("EaterOfWorld"))
            EaterOfWorld = true;
        if (downedLoaded.Contains("BrainOfCthulhu"))
            BrainOfCthulhu = true;
        if (downedLoaded.Contains("QueenBee"))
            QueenBee = true;
        if (downedLoaded.Contains("Skeletron"))
            Skeletron = true;
        if (downedLoaded.Contains("Deerclops"))
            Deerclops = true;
        if (downedLoaded.Contains("WallOfFlesh"))
            WallOfFlesh = true;
        if (downedLoaded.Contains("DukeFishron"))
            DukeFishron = true;
        if (downedLoaded.Contains("QueenSlime"))
            QueenSlime = true;
        if (downedLoaded.Contains("Destroyer"))
            Destroyer = true;
        if (downedLoaded.Contains("Twins"))
            Twins = true;
        if (downedLoaded.Contains("SkeletronPrime"))
            SkeletronPrime = true;
        if (downedLoaded.Contains("MechBossAny"))
            MechBossAny = true;
        if (downedLoaded.Contains("Plantera"))
            Plantera = true;
        if (downedLoaded.Contains("EmpressOfLight"))
            EmpressOfLight = true;
        if (downedLoaded.Contains("Golem"))
            Golem = true;
        if (downedLoaded.Contains("LunaticCultist"))
            LunaticCultist = true;
        if (downedLoaded.Contains("MoonLord"))
            MoonLord = true;
        if (downedLoaded.Contains("Goblins"))
            Goblins = true;
        if (downedLoaded.Contains("Frost"))
            Frost = true;
        if (downedLoaded.Contains("Pirates"))
            Pirates = true;
        if (downedLoaded.Contains("Martians"))
            Martians = true;
        if (downedLoaded.Contains("MourningWood"))
            MourningWood = true;
        if (downedLoaded.Contains("Pumpking"))
            Pumpking = true;
        if (downedLoaded.Contains("Everscream"))
            Everscream = true;
        if (downedLoaded.Contains("SantaNK1"))
            SantaNK1 = true;
        if (downedLoaded.Contains("IceQueen"))
            IceQueen = true;
        if (downedLoaded.Contains("Betsy"))
            Betsy = true;
        if (downedLoaded.Contains("Dreadnautilus"))
            Dreadnautilus = true;
        if (downedLoaded.Contains("SolarTower"))
            SolarTower = true;
        if (downedLoaded.Contains("VortexTower"))
            VortexTower = true;
        if (downedLoaded.Contains("NebulaTower"))
            NebulaTower = true;
        if (downedLoaded.Contains("StardustTower"))
            StardustTower = true;
    }
}