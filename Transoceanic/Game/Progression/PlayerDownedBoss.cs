namespace Transoceanic.Game.Progression;

/// <summary>
/// 汇总玩家世界进度所需的 Boss、入侵事件和天界柱击败标记。
/// 可与世界标记合并、克隆及存档往返；邪恶 Boss 和机械 Boss 里程碑由基础标记派生。
/// </summary>
public class PlayerDownedBoss
{
    /// <summary>
    /// 指示是否已击败 史莱姆王；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool KingSlime { get; set; }
    /// <summary>
    /// 指示是否已击败 克苏鲁之眼；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool EyeOfCthulhu { get; set; }
    /// <summary>
    /// 指示邪恶Boss进度是否已完成；世界吞噬者或克苏鲁之脑任一被击败即可满足。
    /// </summary>
    public bool EvilBoss { get; set; }
    /// <summary>
    /// 指示是否已击败 世界吞噬者；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool EaterOfWorld { get; set; }
    /// <summary>
    /// 指示是否已击败 克苏鲁之脑；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool BrainOfCthulhu { get; set; }
    /// <summary>
    /// 指示是否已击败 蜂王；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool QueenBee { get; set; }
    /// <summary>
    /// 指示是否已击败 骷髅王；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool Skeletron { get; set; }
    /// <summary>
    /// 指示是否已击败 鹿角怪；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool Deerclops { get; set; }
    /// <summary>
    /// 指示是否已击败 血肉墙；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool WallOfFlesh { get; set; }
    /// <summary>
    /// 指示是否已击败 猪龙鱼公爵；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool DukeFishron { get; set; }
    /// <summary>
    /// 指示是否已击败 史莱姆皇后；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool QueenSlime { get; set; }
    /// <summary>
    /// 指示是否已击败 毁灭者；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool Destroyer { get; set; }
    /// <summary>
    /// 指示是否已击败 双子魔眼；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool Twins { get; set; }
    /// <summary>
    /// 指示是否已击败 机械骷髅王；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool SkeletronPrime { get; set; }
    /// <summary>
    /// 指示是否至少击败过一个机械Boss；该进度由三个机械Boss的击败记录派生并与世界标记合并。
    /// </summary>
    public bool MechBossAny { get; set; }
    /// <summary>
    /// 指示是否已击败至少两个机械Boss；由毁灭者、双子魔眼和机械骷髅王的记录重新计算。
    /// </summary>
    public bool MechBossAnyTwo { get; set; }
    /// <summary>
    /// 指示三个机械Boss是否全部被击败；由毁灭者、双子魔眼和机械骷髅王的记录重新计算。
    /// </summary>
    public bool MechBossAll { get; set; }
    /// <summary>
    /// 指示是否已击败 世纪之花；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool Plantera { get; set; }
    /// <summary>
    /// 指示是否已击败 光之女皇；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool EmpressOfLight { get; set; }
    /// <summary>
    /// 指示是否已击败 石巨人；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool Golem { get; set; }
    /// <summary>
    /// 指示是否已击败 拜月教邪教徒；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool LunaticCultist { get; set; }
    /// <summary>
    /// 指示是否已击败 月亮领主；世界轮询和存档加载会更新此进度，供解锁及阶段判定使用。
    /// </summary>
    public bool MoonLord { get; set; }
    /// <summary>
    /// 指示是否已完成 哥布林军团 入侵事件。
    /// </summary>
    public bool Goblins { get; set; }
    /// <summary>
    /// 指示是否已完成 雪人军团 入侵事件。
    /// </summary>
    public bool Frost { get; set; }
    /// <summary>
    /// 指示是否已完成 海盗入侵 入侵事件。
    /// </summary>
    public bool Pirates { get; set; }
    /// <summary>
    /// 指示是否已完成 火星暴乱 入侵事件。
    /// </summary>
    public bool Martians { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 哀木。
    /// </summary>
    public bool MourningWood { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 南瓜王。
    /// </summary>
    public bool Pumpking { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 尖树妖。
    /// </summary>
    public bool Everscream { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 圣诞坦克。
    /// </summary>
    public bool SantaNK1 { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 冰雪女王。
    /// </summary>
    public bool IceQueen { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 贝茜。
    /// </summary>
    public bool Betsy { get; set; }
    /// <summary>
    /// 指示是否已击败事件敌人 恐惧鹦鹉螺。
    /// </summary>
    public bool Dreadnautilus { get; set; }
    /// <summary>
    /// 指示是否已摧毁 日耀柱。
    /// </summary>
    public bool SolarTower { get; set; }
    /// <summary>
    /// 指示是否已摧毁 星旋柱。
    /// </summary>
    public bool VortexTower { get; set; }
    /// <summary>
    /// 指示是否已摧毁 星云柱。
    /// </summary>
    public bool NebulaTower { get; set; }
    /// <summary>
    /// 指示是否已摧毁 星尘柱。
    /// </summary>
    public bool StardustTower { get; set; }


    /// <summary>
    /// 创建当前进度对象的浅拷贝。
    /// 所有成员均为布尔标记，因此修改副本不会改变原对象的进度值。
    /// </summary>
    public virtual PlayerDownedBoss Clone() => (PlayerDownedBoss)MemberwiseClone();


    /// <summary>
    /// 根据邪恶 Boss 和机械 Boss 的基础击败标记更新组合里程碑。
    /// 邪恶 Boss 及任意机械 Boss 标记采用累积方式；任意两个和全部机械 Boss 标记按基础值重算。
    /// </summary>
    public void UpdateDerivedProgression()
    {
        EvilBoss |= EaterOfWorld || BrainOfCthulhu;
        MechBossAny |= Destroyer || Twins || SkeletronPrime;
        MechBossAnyTwo = TOMathUtils.AtLeastXTrue(2, Destroyer, Twins, SkeletronPrime);
        MechBossAll = Destroyer && Twins && SkeletronPrime;
    }


    /// <summary>
    /// 将当前世界已完成的 Boss、入侵事件和天界柱进度合并到本对象。
    /// 轮询只会将完成标记设为真，不会清除已有进度；结束时同步组合里程碑。
    /// </summary>
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


    /// <summary>
    /// 将当前为真的进度标记写入 <paramref name="tag"/> 中的 <paramref name="key"/>。存档值采用字符串列表格式，并由 <see cref="LoadData"/> 读取。
    /// </summary>
    public virtual void SaveData(TagCompound tag, string key)
    {
        List<string> downed = [];

        SaveDataToList(downed);

        tag[key] = downed;
    }


    /// <summary>
    /// 将所有已完成的进度名称追加到 <paramref name="downed"/>。列表格式可用于自定义存档封装，名称须与 <see cref="LoadDataFromIList"/> 使用的键一致。
    /// </summary>
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


    /// <summary>
    /// 从 <paramref name="tag"/> 的 <paramref name="key"/> 读取已完成进度并恢复本对象状态。读取后会重算派生里程碑，缺失的标记按未完成处理。
    /// </summary>
    public virtual void LoadData(TagCompound tag, string key) => LoadDataFromIList(tag.GetList<string>(key));


    /// <summary>
    /// 根据保存的名称列表重建各项击败标记。未出现在 <paramref name="downedLoaded"/> 中的标记会清为假，完成后重新计算派生里程碑。
    /// </summary>
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
