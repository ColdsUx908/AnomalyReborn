namespace Anomalies.Visuals;

/// <summary>
/// 单个NPC持有的手动生成粒子集合。粒子使用世界坐标，DrawSet参数只用于绘制偏移（如netOffset）。
/// 由AnomalyGlobalNPC更新，在NPC完整绘制之前调用，兼容各粒子的混合模式及NPC深度投影。
/// </summary>
public sealed class AnomalyNPCParticleSet : ParticleSet
{
    public override int ParticleLifetime => 0;

    public AnomalyNPCParticleSet() : base(0, 0f) { }

    public override Particle SpawnParticle() => throw new InvalidOperationException("NPC particle sets accept explicit submissions only.");

    public override void DrawSet(Vector2 basePosition)
    {
        if (Main.dedServ || Particles.Count == 0)
            return;
        SpriteBatch spriteBatch = Main.spriteBatch;
        // 保留进入NPC绘制时的投影/采样等状态，只按提交顺序切换必要的混合模式。
        using SpriteBatchScope restore = new(spriteBatch, beginWhenDisposed: true);
        BlendState blend = new SpriteBatchSnapshot(spriteBatch).BlendState;
        foreach (Particle particle in Particles)
        {
            if (blend != particle.DrawBlendState)
            {
                blend = particle.DrawBlendState;
                spriteBatch.ChangeState(sortMode: SpriteSortMode.Deferred, blendState: blend);
            }
            ParticleHandler.DrawParticle(spriteBatch, particle, basePosition);
        }
    }
}
