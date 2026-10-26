namespace Anomalies.Common.SingleBehaviors;

public sealed class AnomalyNPCMisc : AnomalyGlobalNPCBehavior
{
    public override decimal Priority => 500m;

    public override void SetDefaults(NPC npc)
    {
        AnomalyGlobalNPC anomalyNPC = npc.Anomaly;

        anomalyNPC.ShouldRunAnomalyAI = true;
    }

    public override bool PreAI(NPC npc)
    {
        AnomalyGlobalNPC anomalyNPC = npc.Anomaly;

        if (AnomalySharedData.Anomaly)
        {
            anomalyNPC.AnomalyAI_Timer++;
            if (AnomalySharedData.AnomalyUltramundane)
            {
                anomalyNPC.AnomalyAI_UltraTimer++;
                anomalyNPC.BossBar_UltraTimer = Math.Clamp(anomalyNPC.BossBar_UltraTimer + 1, 0, 120);
            }
            else
            {
                anomalyNPC.AnomalyAI_UltraTimer = 0;
                anomalyNPC.BossBar_UltraTimer = Math.Clamp(anomalyNPC.BossBar_UltraTimer - 4, 0, 120);
            }
        }
        else
        {
            anomalyNPC.AnomalyAI_Timer = 0;
            anomalyNPC.AnomalyAI_UltraTimer = 0;
            anomalyNPC.BossBar_UltraTimer = 0;
        }

        return true;
    }
}

