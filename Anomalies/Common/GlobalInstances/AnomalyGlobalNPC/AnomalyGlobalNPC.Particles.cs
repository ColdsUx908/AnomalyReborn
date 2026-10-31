namespace Anomalies.Common;

public sealed partial class AnomalyGlobalNPC
{
    /// <summary>
    /// 向本NPC专属集合提交世界坐标粒子；客户端按需创建集合，并自动更新、在本体之前绘制。
    /// 不再向ParticleHandler提交同一个实例；本集合不承担伤害和同步。
    /// </summary>
    public bool SpawnVisualParticle(Particle particle)
    {
        ArgumentNullException.ThrowIfNull(particle);
        if (Main.dedServ || Main.gamePaused)
            return false;
        Visual_Particles ??= new AnomalyNPCParticleSet();
        return Visual_Particles.TrySpawnParticle(particle);
    }

    public override void SetDefaults(NPC npc) => Visual_Particles = null;

    public override void PostAI(NPC npc)
    {
        if (!npc.active)
        {
            Visual_Particles = null;
            return;
        }
        if (!Main.dedServ)
            Visual_Particles?.Update();
    }

    public override void OnKill(NPC npc)
    {
        Visual_Particles?.Clear();
        Visual_Particles = null;
    }
}
