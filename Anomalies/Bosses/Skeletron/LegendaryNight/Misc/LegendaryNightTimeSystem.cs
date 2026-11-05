namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed class LegendaryNightTimeSystem : ModSystem
{
    private const double Midnight = 16200d; // 原版夜晚从19:30开始，零点对应经过4.5小时。
    private const int TransitionDuration = 180;

    private int _HeadIndex = -1;
    private int _Timer;
    private double _StartTime;

    public override void PostUpdateTime()
    {
        SkeletronLegendHead head = null;
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            if (!LegendaryNightHandler.IsLegendaryNightHead(npc))
                continue;
            SkeletronLegendHead candidate = SkeletronLegendHead.GetInstance(npc);
            if (candidate.ShouldProcess && candidate.SuppressSkyClouds
                && candidate.CurrentBaseBehavior != SkeletronLegendHead.BaseBehavior.Despawn)
            {
                head = candidate;
                break;
            }
        }

        if (head is null)
        {
            if (_HeadIndex >= 0 && Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SetTime);
            ResetTimeControl();
            return;
        }

        if (_HeadIndex != head.NPC.whoAmI)
        {
            _HeadIndex = head.NPC.whoAmI;
            // 白天使用相邻夜晚前的负时间，避免跨昼夜时跳变；正常战斗起点为夜晚。
            _StartTime = Main.dayTime ? Main.time - Main.dayLength : Main.time;
            _Timer = head.Phase2 ? TransitionDuration : 0;
        }

        bool wasTransitioning = _Timer < TransitionDuration;
        _Timer = Math.Min(_Timer + 1, TransitionDuration);
        double completion = _Timer / (double)TransitionDuration;
        double blend = completion * completion * (3d - 2d * completion);
        double time = _Timer == TransitionDuration ? Midnight : _StartTime + (Midnight - _StartTime) * blend;
        Main.dayTime = time < 0d;
        Main.time = Main.dayTime ? time + Main.dayLength : time;

        // 各端平滑预测；到达零点时由服务器校准，锁定不依赖客户端天空系统。
        if (wasTransitioning && _Timer == TransitionDuration && Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SetTime);
    }

    public override void OnWorldLoad() => ResetTimeControl();

    public override void OnWorldUnload() => ResetTimeControl();

    private void ResetTimeControl()
    {
        _HeadIndex = -1;
        _Timer = 0;
        _StartTime = 0d;
    }
}
