using System.Globalization;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

// 仅提示现有祝福状态；飞行效果由赋予祝福的机制负责。
/// <summary>
/// 星神祝福的状态提示与图标。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用 ai、localAI 或框架 Timer；读取统一 AnomalyPlayer 的祝福与奖励层数，Buff 持续时间由原版玩家 Buff 槽管理。</para>
/// </remarks>
public sealed class AstralBlessing : ModBuff
{
    public override string Texture => LegendaryNightHandler.LegendaryNightPath + "Buffs/AstralBlessing";

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override bool RightClick(int buffIndex) => false;

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Description.Value;
        bool showStatBlessings = false;
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            if (!LegendaryNightHandler.IsLegendaryNightHead(npc))
                continue;
            SkeletronLegendHead head = SkeletronLegendHead.GetInstance(npc);
            // 与小光球广播共用音乐节点，后加入的客户端也可从同步状态恢复显示。
            if (head.StarGodBlessingActive
                && head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic6Timeline.LightOrbFirstMusicTime)
            {
                showStatBlessings = true;
                break;
            }
        }
        if (!showStatBlessings)
            return;

        AnomalyPlayer player = Main.LocalPlayer.Anomaly;
        float damageMultiplier = SkeletronLegendHead.GetLightOrbDamageBonusMultiplier(player.LegendaryNight_DamageBonusStacks);
        tip += "\n" + this.GetLocalization("StatBlessings").Format(damageMultiplier.ToString("0.##", CultureInfo.InvariantCulture),
            player.LegendaryNight_RegenerationBonusStacks.ToString("+0;-0;0", CultureInfo.InvariantCulture));
    }

    public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
    {
        AstralBuffDrawing.DrawGlow(spriteBatch, ref drawParams, new Color(255, 205, 90));
        return true;
    }
}