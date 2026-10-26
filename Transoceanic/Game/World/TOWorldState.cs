namespace Transoceanic.Game.World;

public sealed class TOWorldState : ModSystem, ITOLoader
{
    /// <summary>
    /// 当前日期是否为愚人节（4 月 1 日）。
    /// </summary>
    public static bool AprilFools => DateTime.Now is (int _, 4, 1);

    /// <summary>
    /// 当前世界是否为真正的大师模式（非旅途模式滑块模拟，非专家+FTW）。
    /// </summary>
    public static bool TrueMasterMode => Main.GameMode == GameModeID.Master;

    /// <summary>
    /// 当前旅途模式世界是否通过难度滑块启用了大师难度。
    /// </summary>
    public static bool JourneyMasterMode { get; internal set; }

    /// <summary>
    /// 综合判断当前世界是否处于大师模式（包含真正大师模式和旅途大师滑块）。
    /// </summary>
    public static bool MasterMode => TrueMasterMode || JourneyMasterMode;

    /// <summary>
    /// 综合判断当前世界是否处于传奇模式（FTW 且处于任何大师模式）。
    /// </summary>
    public static bool LegendaryMode => Main.getGoodWorld && MasterMode;

    public override void PreUpdateEntities()
    {
        if (Main.GameMode == GameModeID.Creative)
        {
            global::Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower power = global::Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<global::Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower>();
            bool currentJourneyMaster = power.StrengthMultiplierToGiveNPCs == 3f;
            if (power.GetIsUnlocked())
                JourneyMasterMode = currentJourneyMaster;
            else if (!currentJourneyMaster)
                JourneyMasterMode = false;
        }
        else
            JourneyMasterMode = false;
    }

    void ITOLoader.Unload() => JourneyMasterMode = false;
}
