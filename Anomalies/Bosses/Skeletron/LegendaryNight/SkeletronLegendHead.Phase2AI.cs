namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead
{
    public void Phase2AI()
    {
        if (Phase2ElapsedTime < Phase2MinimumDuration)
        {
            Phase2ElapsedTime++;
            if (TOSharedData.NotClient && Phase2ElapsedTime % 60 == 0)
                NPC.netUpdate = true;
        }
        Timer1++;
        // 二阶段攻击实现前，暂时使用常规悬浮移动。
        SkeletronHead.NormalMovement(NPC, Target, 20f, 8f, 0.2f, 0.08f, 300f, 20f);
    }
}
