namespace Transoceanic.Graphics.Particles;

/// <summary>
/// 使用相对坐标的粒子集合，绘制时由调用方指定基准位置。集合独立于全局粒子管理器，不占用全局粒子容量，须由调用方更新和绘制。
/// </summary>
/// <remarks>活动列表和待加入缓冲不提供线程安全保证；生成、更新、清空和绘制在游戏相应主线程阶段调用。</remarks>
public abstract class ParticleSet
{
    /// <summary>
    /// 集合已执行的客户端更新次数，由 Update 每次递增。
    /// </summary>
    public int LocalTimer { get; internal set; }
    /// <summary>
    /// 集合允许持续生成的更新帧数；不大于零时不按集合寿命停止生成。
    /// </summary>
    public int SetLifetime { get; internal set; }
    /// <summary>
    /// 每次更新自动生成粒子的概率，通常取 0 到 1；0 表示不生成，1 表示在未到停止生成时刻时每次更新都尝试生成。
    /// </summary>
    public float ParticleSpawnRate;
    /// <summary>
    /// 当前活动粒子的只读视图；提交后在下一次更新开始时可见。
    /// </summary>
    public IReadOnlyList<Particle> Particles { get; }

    private readonly List<Particle> _Particles = [];
    private readonly List<Particle> _PendingParticles = [];
    private readonly HashSet<Particle> _OwnedParticles = new(ReferenceEqualityComparer.Instance);
    private bool _Updating;
    private int _Generation;

    /// <summary>
    /// 本集合独立的粒子上限，计入活动、待加入及 PreSpawn 预留；重要粒子可豁免。默认 5000。
    /// </summary>
    public int ParticleLimit { get; set; } = 5000;

    /// <summary>
    /// 生成粒子的预期寿命，单位为更新帧；用于计算停止生成的时刻。
    /// 集合不自动销毁自身，也不强制覆盖粒子的 Lifetime；调用方须管理集合生命周期。
    /// </summary>
    public abstract int ParticleLifetime { get; }
    /// <summary>
    /// 创建集合自动生成所需的新粒子；本方法本身不负责将粒子提交到集合。
    /// </summary>
    /// <returns>要提交至本集合的新粒子实例，不得为 null；自动生成通过 TrySpawnParticle 执行生成回调。</returns>
    public abstract Particle SpawnParticle();

    /// <summary>
    /// 设置集合更新寿命与每次更新的生成概率；寿命大于零时在末期停止生成，为已有粒子留出生存时间。
    /// </summary>
    public ParticleSet(int setLifetime, float particleSpawnRate)
    {
        Particles = _Particles.AsReadOnly();
        SetLifetime = setLifetime;
        ParticleSpawnRate = particleSpawnRate;
    }

    /// <summary>
    /// 将粒子提交到待加入缓冲，调用 PreSpawn/PostSpawn；容量不足、重复对象、否决或专用服务器返回 false。
    /// PreSpawn 的外部副作用不回滚；PostSpawn 抛错时粒子已经提交。不计入全局粒子容量。
    /// </summary>
    public bool TrySpawnParticle(Particle particle)
    {
        ArgumentNullException.ThrowIfNull(particle);
        if (Main.dedServ || particle.IsTemplate || (!particle.Important && _OwnedParticles.Count >= ParticleLimit)
            || !_OwnedParticles.Add(particle))
            return false;
        int generation = _Generation;
        bool committed = false;
        try
        {
            if (!particle.PreSpawn() || generation != _Generation)
                return false;
            _PendingParticles.Add(particle);
            committed = true;
            particle.PostSpawn();
            return true;
        }
        finally
        {
            if (!committed && generation == _Generation)
                _OwnedParticles.Remove(particle);
        }
    }

    /// <summary>
    /// 清空活动与待加入粒子；更新或绘制回调期间禁止调用。集合计时保持不变。
    /// </summary>
    public void Clear()
    {
        if (_Updating)
            throw new InvalidOperationException("Cannot clear a particle set during update or drawing.");
        _Generation++;
        _Particles.Clear();
        _PendingParticles.Clear();
        _OwnedParticles.Clear();
    }

    /// <summary>
    /// 先按概率提交自动生成粒子，再合并缓冲并更新活动粒子。更新中新提交的粒子在下次 Update 开始时加入。
    /// 回调异常时仍移除死亡粒子并解除更新保护；不推进集合计时，不重复处理本帧剩余粒子。
    /// </summary>
    public virtual void Update()
    {
        if (Main.dedServ)
            return;
        if (_Updating)
            throw new InvalidOperationException("Particle set updates cannot be nested with update or drawing.");
        _Updating = true;
        try
        {
            bool closeToDeath = LocalTimer >= SetLifetime - ParticleLifetime && SetLifetime > 0;
            if (Main.rand.NextProbability(ParticleSpawnRate) && !closeToDeath)
                TrySpawnParticle(SpawnParticle());
            _Particles.AddRange(_PendingParticles);
            _PendingParticles.Clear();
            foreach (Particle particle in _Particles)
                ParticleHandler.UpdateParticle(particle);
            LocalTimer++;
        }
        finally
        {
            try
            {
                _Particles.RemoveAll(particle =>
                {
                    bool expired = (particle.Timer >= particle.Lifetime && particle.AutoKillByLifeTime) || particle.Dead;
                    if (expired)
                        _OwnedParticles.Remove(particle);
                    return expired;
                });
            }
            finally
            {
                _Updating = false;
            }
        }
    }

    /// <summary>
    /// 在客户端按提交顺序绘制集合，以 basePosition 作为像素偏移；调用方负责准备 SpriteBatch 状态。
    /// </summary>
    public virtual void DrawSet(Vector2 basePosition)
    {
        if (Main.dedServ)
            return;

        if (_Updating)
            throw new InvalidOperationException("Particle set drawing cannot be nested with update or drawing.");
        _Updating = true;
        try
        {
            foreach (Particle particle in _Particles)
                ParticleHandler.DrawParticle(Main.spriteBatch, particle, basePosition);
        }
        finally
        {
            _Updating = false;
        }
    }
}
