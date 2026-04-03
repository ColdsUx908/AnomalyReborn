namespace Transoceanic.DataStructures;

public sealed class ModReference : IContentLoader
{
    /// <summary>
    /// 检测特定Mod是否已加载，可获取实例。
    /// </summary>
    public record Container
    {
        /// <summary>
        /// Mod内部名。
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Mod是否已加载。
        /// </summary>
        public readonly bool IsLoaded;

        /// <summary>
        /// Mod实例。若 <see cref="IsLoaded"/> 为 <see langword="false"/>，值为 <see langword="null"/>。
        /// </summary>
        public readonly Mod Mod;

        public Container(string name = "") => IsLoaded = ModLoader.TryGetMod(Name = name, out Mod);

        public static readonly Container Empty = new();
    }

    //按字母顺序排列
    public static Container CA { get; private set; }
    public static Container Calamity { get; private set; }
    public static Container CalDemutation { get; private set; }
    public static Container CALegacy { get; private set; }
    public static Container CALegacyLegacy { get; private set; }
    public static Container CalFables { get; private set; }
    public static Container CalOverhaul { get; private set; }
    public static Container CalSpear { get; private set; }
    public static Container Catalyst { get; private set; }
    public static Container CelessTest { get; private set; }
    public static Container Example { get; private set; }
    public static Container FargoDLC { get; private set; }
    public static Container FargoMutant { get; private set; }
    public static Container FargoSouls { get; private set; }
    public static Container Gensokyo { get; private set; }
    public static Container HollowKnight { get; private set; }
    public static Container HuntoftheOldGod { get; private set; }
    public static Container Infernum { get; private set; }
    public static Container InfiniteAddPoint { get; private set; }
    public static Container Luiafk { get; private set; }
    public static Container Luminance { get; private set; }
    public static Container LunarVeil { get; private set; }
    public static Container MEAC { get; private set; }
    public static Container Nycro { get; private set; }
    public static Container Overhaul { get; private set; }
    public static Container QoT { get; private set; }
    public static Container Radiance { get; private set; }
    public static Container Redemption { get; private set; }
    public static Container Remnants { get; private set; }
    public static Container Spear { get; private set; }
    public static Container Spirit { get; private set; }
    public static Container StarlightRiver { get; private set; }
    public static Container StarsAbove { get; private set; }
    public static Container StoryofRedCloud { get; private set; }
    public static Container Thorium { get; private set; }
    public static Container ThoriumRework { get; private set; }
    public static Container TO { get; private set; }
    public static Container WrathoftheGods { get; private set; }

    [LoadPriority(1e10)]
    void IContentLoader.PostSetupContent()
    {
        CA = new("CalamityAnomalies");
        Calamity = new("CalamityMod");
        CalDemutation = new("CalamityDemutation");
        CALegacy = new("CAnomalies");
        CALegacyLegacy = new("ACalTweak");
        CalFables = new("CalamityFables");
        CalOverhaul = new("CalamityOverhaul");
        CalSpear = new("CalamityThrowingSpear");
        Catalyst = new("Catalyst");
        CelessTest = new("CelessTest");
        Example = new("ExampleMod");
        FargoDLC = new("FargowiltasCrossMod");
        FargoMutant = new("Fargowiltas");
        FargoSouls = new("FargowiltasSouls");
        Gensokyo = new("Gensokyo");
        HollowKnight = new("TheKnightMod");
        HuntoftheOldGod = new("CalamityHunt");
        Infernum = new("InfernumMode");
        InfiniteAddPoint = new("InfiniteAddPointSystem");
        Luiafk = new("miningcracks_take_on_luiafk");
        Luminance = new("Luminance");
        LunarVeil = new("Stellamod");
        MEAC = new("MEAC");
        Nycro = new("EfficientNohits");
        Overhaul = new("TerrariaOverhaul");
        QoT = new("ImproveGame");
        Radiance = new("Radiance");
        Redemption = new("Redemption");
        Remnants = new("Remnants");
        Spear = new("SpearToJavelin");
        Spirit = new("SpiritMod");
        StarlightRiver = new("StarlightRiver");
        StarsAbove = new("StarsAbove");
        StoryofRedCloud = new("tsorcRevamp");
        Thorium = new("ThoriumMod");
        ThoriumRework = new("ThoriumRework");
        TO = new("Transoceanic");
        WrathoftheGods = new("NoxusBoss");
    }

    void IContentLoader.OnModUnload()
    {
        CA = null;
        Calamity = null;
        CalDemutation = null;
        CALegacy = null;
        CALegacyLegacy = null;
        CalFables = null;
        CalOverhaul = null;
        CalSpear = null;
        Catalyst = null;
        CelessTest = null;
        Example = null;
        FargoDLC = null;
        FargoMutant = null;
        FargoSouls = null;
        Gensokyo = null;
        HollowKnight = null;
        HuntoftheOldGod = null;
        Infernum = null;
        InfiniteAddPoint = null;
        Luiafk = null;
        Luminance = null;
        LunarVeil = null;
        MEAC = null;
        Nycro = null;
        Overhaul = null;
        QoT = null;
        Radiance = null;
        Redemption = null;
        Remnants = null;
        Spear = null;
        Spirit = null;
        StarlightRiver = null;
        StarsAbove = null;
        StoryofRedCloud = null;
        Thorium = null;
        ThoriumRework = null;
        TO = null;
        WrathoftheGods = null;
    }
}
