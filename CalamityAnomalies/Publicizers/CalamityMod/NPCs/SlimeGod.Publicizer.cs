using CalamityMod.NPCs.SlimeGod;

namespace CalamityAnomalies.Publicizers.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record SlimeGodCore_Publicizer(SlimeGodCore Source) : Publicizer<SlimeGodCore>(Source)
{
    // slimesSpawned (instance field)
    public static readonly FieldInfo i_f_slimesSpawned = GetInstanceField("slimesSpawned");
    public bool slimesSpawned
    {
        get => (bool)i_f_slimesSpawned.GetValue(Source);
        set => i_f_slimesSpawned.SetValue(Source, value);
    }

    // buffedSlime (instance field)
    public static readonly FieldInfo i_f_buffedSlime = GetInstanceField("buffedSlime");
    public int buffedSlime
    {
        get => (int)i_f_buffedSlime.GetValue(Source);
        set => i_f_buffedSlime.SetValue(Source, value);
    }
}

public record CrimulanPaladin_Publicizer(CrimulanPaladin Source) : Publicizer<CrimulanPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

public record EbonianPaladin_Publicizer(EbonianPaladin Source) : Publicizer<EbonianPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

public record SplitCrimulanPaladin_Publicizer(SplitCrimulanPaladin Source) : Publicizer<SplitCrimulanPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

public record SplitEbonianPaladin_Publicizer(SplitEbonianPaladin Source) : Publicizer<SplitEbonianPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}
