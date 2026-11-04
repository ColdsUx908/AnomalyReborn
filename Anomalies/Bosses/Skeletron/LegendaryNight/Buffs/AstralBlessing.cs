using System.Globalization;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Buffs;

// 仅提示现有祝福状态；飞行效果由赋予祝福的机制负责。
public sealed class AstralBlessing : ModBuff
{
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
                && head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic6Data.LightOrbFirstMusicTime)
            {
                showStatBlessings = true;
                break;
            }
        }
        if (!showStatBlessings)
            return;

        AnomalyPlayer player = Main.LocalPlayer.Anomaly;
        float damageMultiplier = LegendaryNightFlightPlayer.GetDamageBonusMultiplier(player.LegendaryNight_DamageBonusStacks);
        tip += "\n" + this.GetLocalization("StatBlessings").Format(damageMultiplier.ToString("0.##", CultureInfo.InvariantCulture),
            player.LegendaryNight_RegenerationBonusStacks.ToString("+0;-0;0", CultureInfo.InvariantCulture));
    }

    public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
    {
        AstralBuffDrawing.DrawGlow(spriteBatch, ref drawParams, new Color(255, 205, 90));
        return true;
    }
}
