namespace Transoceanic.Game.Progression;

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

    public virtual PlayerDownedBoss Clone() => (PlayerDownedBoss)MemberwiseClone();

    public void UpdateDerivedProgression()
    {
        EvilBoss |= EaterOfWorld || BrainOfCthulhu;
        MechBossAny |= Destroyer || Twins || SkeletronPrime;
        MechBossAnyTwo = TOMathUtils.AtLeastXTrue(2, Destroyer, Twins, SkeletronPrime);
        MechBossAll = Destroyer && Twins && SkeletronPrime;
    }

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

        if (global::Terraria.GameContent.Events.DD2Event.DownedInvasionT3)
            Betsy = true;

        if (NPC.downedTowerSolar)
            SolarTower = true;
        if (NPC.downedTowerVortex)
            VortexTower = true;
        if (NPC.downedTowerNebula)
            NebulaTower = true;
        if (NPC.downedTowerStardust)
            StardustTower = true;

        UpdateDerivedProgression();
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
        KingSlime = downedLoaded.Contains("KingSlime");
        EyeOfCthulhu = downedLoaded.Contains("EyeOfCthulhu");
        EvilBoss = downedLoaded.Contains("EvilBoss");
        EaterOfWorld = downedLoaded.Contains("EaterOfWorld");
        BrainOfCthulhu = downedLoaded.Contains("BrainOfCthulhu");
        QueenBee = downedLoaded.Contains("QueenBee");
        Skeletron = downedLoaded.Contains("Skeletron");
        Deerclops = downedLoaded.Contains("Deerclops");
        WallOfFlesh = downedLoaded.Contains("WallOfFlesh");
        DukeFishron = downedLoaded.Contains("DukeFishron");
        QueenSlime = downedLoaded.Contains("QueenSlime");
        Destroyer = downedLoaded.Contains("Destroyer");
        Twins = downedLoaded.Contains("Twins");
        SkeletronPrime = downedLoaded.Contains("SkeletronPrime");
        MechBossAny = downedLoaded.Contains("MechBossAny");
        Plantera = downedLoaded.Contains("Plantera");
        EmpressOfLight = downedLoaded.Contains("EmpressOfLight");
        Golem = downedLoaded.Contains("Golem");
        LunaticCultist = downedLoaded.Contains("LunaticCultist");
        MoonLord = downedLoaded.Contains("MoonLord");
        Goblins = downedLoaded.Contains("Goblins");
        Frost = downedLoaded.Contains("Frost");
        Pirates = downedLoaded.Contains("Pirates");
        Martians = downedLoaded.Contains("Martians");
        MourningWood = downedLoaded.Contains("MourningWood");
        Pumpking = downedLoaded.Contains("Pumpking");
        Everscream = downedLoaded.Contains("Everscream");
        SantaNK1 = downedLoaded.Contains("SantaNK1");
        IceQueen = downedLoaded.Contains("IceQueen");
        Betsy = downedLoaded.Contains("Betsy");
        Dreadnautilus = downedLoaded.Contains("Dreadnautilus");
        SolarTower = downedLoaded.Contains("SolarTower");
        VortexTower = downedLoaded.Contains("VortexTower");
        NebulaTower = downedLoaded.Contains("NebulaTower");
        StardustTower = downedLoaded.Contains("StardustTower");

        UpdateDerivedProgression();
    }
}
