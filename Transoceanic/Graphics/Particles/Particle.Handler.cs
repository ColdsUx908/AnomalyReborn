using Transoceanic.Graphics.Drawing;
using Transoceanic.Runtime.Reflection;
namespace Transoceanic.Graphics.Particles;

/// <summary>
/// 粒子系统全局处理器，负责粒子的更新、绘制以及生命周期管理。
/// </summary>
/// <remarks>粒子队列不提供线程安全保证；生成、更新和绘制在游戏相应主线程阶段调用。</remarks>
public sealed class ParticleHandler : ModSystem, IContentLoader
{
    /// <summary>
    /// 粒子纹理资源的基础路径。
    /// </summary>
    public const string BaseParticleTexturePath = "Transoceanic/Assets/Textures/Particles/";

    /// <summary>
    /// 粒子数量限制。
    /// <br/>当活动、待加入和回调预留的粒子总数达到该值时，除非新生成的粒子被标记为重要粒子，否则将不会被生成。
    /// </summary>
    public static int ParticleLimit { get; set; } = 5000;

    internal static Dictionary<int, ParticleDefinition> _ParticleCache;
    internal static Dictionary<Type, int> _ParticleTypes;

    private static List<Particle> _Particles;
    private static List<Particle> _PendingParticles;
    private static int _ReservedParticles;

    private static List<Particle> _ParticlesToDrawAlphaBlend;
    private static List<Particle> _ParticlesToDrawNonPremultiplied;
    private static List<Particle> _ParticlesToDrawAdditive;
    private static List<Particle> _ParticlesToDrawOpaque;

    /// <summary>
    /// 绘制所有活跃粒子。根据不同混合状态分组绘制以减少渲染状态切换。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch 实例。</param>
    public static void Draw(SpriteBatch spriteBatch)
    {
        if (Main.dedServ || _Particles is null)
            return;

        if (_Particles.Count == 0)
            return;

        try
        {
            //提前分类粒子以减少spriteBatch状态切换次数
            foreach (Particle particle in _Particles)
            {
                if (particle is null || !IsOnScreen(particle))
                    continue;

                BlendState blendState = particle.DrawBlendState;
                if (blendState == BlendState.AlphaBlend)
                    _ParticlesToDrawAlphaBlend.Add(particle);
                else if (blendState == BlendState.NonPremultiplied)
                    _ParticlesToDrawNonPremultiplied.Add(particle);
                else if (blendState == BlendState.Additive)
                    _ParticlesToDrawAdditive.Add(particle);
                else if (blendState == BlendState.Opaque)
                    _ParticlesToDrawOpaque.Add(particle);
            }

            if (_ParticlesToDrawAlphaBlend.Count > 0)
            {
                EnterDrawRegion_AlphaBlend(spriteBatch);

                foreach (Particle particle in _ParticlesToDrawAlphaBlend)
                    DrawVisibleParticle(spriteBatch, particle);
            }

            if (_ParticlesToDrawNonPremultiplied.Count > 0)
            {
                EnterDrawRegion_NonPremultiplied(spriteBatch);

                foreach (Particle particle in _ParticlesToDrawNonPremultiplied)
                    DrawVisibleParticle(spriteBatch, particle);
            }

            if (_ParticlesToDrawAdditive.Count > 0)
            {
                EnterDrawRegion_Additive(spriteBatch);

                foreach (Particle particle in _ParticlesToDrawAdditive)
                    DrawVisibleParticle(spriteBatch, particle);
            }

            if (_ParticlesToDrawOpaque.Count > 0)
            {
                EnterDrawRegion_Opaque(spriteBatch);

                foreach (Particle particle in _ParticlesToDrawOpaque)
                    DrawVisibleParticle(spriteBatch, particle);
            }
        }
        finally
        {
            _ParticlesToDrawAlphaBlend.Clear();
            _ParticlesToDrawNonPremultiplied.Clear();
            _ParticlesToDrawAdditive.Clear();
            _ParticlesToDrawOpaque.Clear();
            TODrawUtils.ResetSpriteBatch(spriteBatch);
        }
    }

    /// <summary>
    /// 绘制单个粒子实例。根据粒子自身的属性决定是否受光照影响，并调用粒子的预绘制和后绘制方法。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch 实例。</param>
    /// <param name="particle">要绘制的粒子实例。</param>
    /// <param name="drawOffset">可选的绘制偏移量。</param>
    public static void DrawParticle(SpriteBatch spriteBatch, Particle particle, Vector2 drawOffset = default)
    {
        if (!IsOnScreen(particle, drawOffset))
            return;
        DrawVisibleParticle(spriteBatch, particle, drawOffset);
    }

    private static void DrawVisibleParticle(SpriteBatch spriteBatch, Particle particle, Vector2 drawOffset = default)
    {
        if (particle.PreDraw(spriteBatch, drawOffset))
        {
            Texture2D texture = particle.Texture;
            Rectangle? frame = particle.GetFrame(texture);
            Color color = particle.Color;
            if (particle.AffectedByLight)
                color.MultiplyWithWorldLight(particle.Center);
            spriteBatch.DrawFromCenter(texture, particle.Center + drawOffset - Main.screenPosition, frame, color, particle.Rotation, particle.Scale, SpriteEffects.None, 0f);
        }

        particle.PostDraw(spriteBatch, drawOffset);
    }

    /// <summary>
    /// 按缩放后的摄像机视野检查粒子中心；可见区域扩大为两倍宽高，再加粒子额外边距。
    /// drawOffset 与粒子中心相加，支持局部 ParticleSet 的世界基准位置。
    /// </summary>
    internal static bool IsOnScreen(Particle particle, Vector2 drawOffset = default)
    {
        Vector2 size = Main.Camera.ScaledSize;
        Vector2 position = Main.Camera.ScaledPosition;
        Vector2 center = particle.Center + drawOffset;
        float fluff = Math.Max(0f, particle.DrawScreenCheckFluff);
        float marginX = size.X * 0.5f + fluff;
        float marginY = size.Y * 0.5f + fluff;
        return center.X >= position.X - marginX && center.X <= position.X + size.X + marginX
            && center.Y >= position.Y - marginY && center.Y <= position.Y + size.Y + marginY;
    }

    /// <summary>
    /// 进入 AlphaBlend 混合状态的绘制区域。
    /// </summary>
    public static void EnterDrawRegion_AlphaBlend(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <summary>
    /// 进入 NonPremultiplied 混合状态的绘制区域。
    /// </summary>
    public static void EnterDrawRegion_NonPremultiplied(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <summary>
    /// 进入 Additive 混合状态的绘制区域。
    /// </summary>
    public static void EnterDrawRegion_Additive(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <summary>
    /// 进入 Opaque 混合状态的绘制区域。
    /// </summary>
    public static void EnterDrawRegion_Opaque(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 在所有实体更新后处理粒子的更新与移除。
    /// </remarks>
    public override void PostUpdateEverything()
    {
        if (Main.dedServ || _Particles is null)
            return;

        _Particles.AddRange(_PendingParticles);
        _PendingParticles.Clear();

        foreach (Particle particle in _Particles)
        {
            if (particle is null)
                continue;
            UpdateParticle(particle);
        }

        _Particles.RemoveAll(particle => particle is null || (particle.Timer >= particle.Lifetime && particle.AutoKillByLifeTime) || particle.Dead);
    }

    /// <summary>
    /// 先增加 Timer，再调用 Update，最后在 AutoUpdatePosition 开启时累加 Velocity；不在此移除粒子。
    /// </summary>
    internal static void UpdateParticle(Particle particle)
    {
        particle.Timer++;
        particle.Update();
        if (particle.AutoUpdatePosition)
            particle.Center += particle.Velocity;
    }

    [LoadPriority(1)]
    void IContentLoader.PostSetupContent()
    {
        _ParticleCache = [];
        _ParticleTypes = [];
        _Particles = [];
        _PendingParticles = [];
        _ReservedParticles = 0;
        _ParticlesToDrawAlphaBlend = [];
        _ParticlesToDrawNonPremultiplied = [];
        _ParticlesToDrawAdditive = [];
        _ParticlesToDrawOpaque = [];

        int id = 0;
        foreach (ParticleDefinition definition in TOReflectionUtils.GetAllSupportedMods().SelectMany(mod => mod.GetContent<ParticleDefinition>()))
        {
            definition.Initialize(id);
            _ParticleCache.Add(id, definition);
            _ParticleTypes.Add(definition.ParticleType, id++);
        }

        if (Main.dedServ)
            return;

        //在绘制狱火药水效果前绘制粒子
        On_Main.DrawInfernoRings += (orig, self) =>
        {
            Draw(Main.spriteBatch);
            orig(self);
        };
    }

    void IContentLoader.OnModUnload()
    {
        _ParticleCache = null;
        _ParticleTypes = null;
        _Particles = null;
        _PendingParticles = null;
        _ReservedParticles = 0;
        _ParticlesToDrawAlphaBlend = null;
        _ParticlesToDrawNonPremultiplied = null;
        _ParticlesToDrawAdditive = null;
        _ParticlesToDrawOpaque = null;
    }

    /// <summary>
    /// 向 <see cref="_Particles"/> 中添加一个粒子实例以生成该粒子。
    /// 生成成功后先进入缓冲区，下一次统一更新开始时合并；PostSpawn 仅在接受生成时调用。
    /// </summary>
    /// <param name="particle">已构造的新粒子；提交不重置 Timer 或其他实例字段，不检查是否重复提交。</param>
    /// <remarks>仅客户端在未暂停且缓存就绪时接受；专用服务器、暂停或未初始化时不生成。回调异常原样传播，PostSpawn 异常发生时粒子可能已提交。</remarks>
    /// <exception cref="ArgumentNullException">粒子或列表为 null。</exception>
    public static void SpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    /// <summary>
    /// 尝试向 <see cref="_Particles"/> 中添加一个粒子实例以生成该粒子。
    /// PreSpawn 否决或容量不足时返回 false；容量计算包含缓冲区及回调中预留的位置。
    /// </summary>
    /// <param name="particle">已构造的新粒子；提交不重置 Timer 或其他实例字段，不检查是否重复提交。</param>
    /// <remarks>仅客户端在未暂停且缓存就绪时接受；专用服务器、暂停或未初始化时不生成。回调异常原样传播，PostSpawn 异常发生时粒子可能已提交。</remarks>
    /// <returns>所有候选都被接受时为 true；false 可能表示环境不允许、容量不足或 PreSpawn 否决。</returns>
    /// <exception cref="ArgumentNullException">粒子或列表为 null。</exception>
    public static bool TrySpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    /// <summary>
    /// 向 <see cref="_Particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    /// <param name="particles">候选粒子列表；调用时创建快照，不能包含 null。</param>
    /// <param name="onlySpawnWhenSpaceEnough">true 时先预留整批容量并执行全部候选的 PreSpawn，再统一提交；任一否决不提交本批，回调自身副作用不回滚。false 时逐个尝试，允许部分生成。</param>
    /// <remarks>仅客户端在未暂停且缓存就绪时接受；专用服务器、暂停或未初始化时不生成。回调异常原样传播，PostSpawn 异常发生时粒子可能已提交。</remarks>
    /// <exception cref="ArgumentNullException">粒子或列表为 null。</exception>
    /// <exception cref="ArgumentException">候选列表在可生成环境下包含 null。</exception>
    public static void SpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, false, onlySpawnWhenSpaceEnough);

    /// <summary>
    /// 尝试向 <see cref="_Particles"/> 中添加一组粒子实例以生成这些粒子。
    /// 返回值表示全部候选均被接受；允许部分生成时返回 false 仍可能已接受其中一部分。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    /// <param name="particles">候选粒子列表；调用时创建快照，不能包含 null。</param>
    /// <param name="onlySpawnWhenSpaceEnough">true 时先预留整批容量并执行全部候选的 PreSpawn，再统一提交；任一否决不提交本批，回调自身副作用不回滚。false 时逐个尝试，允许部分生成。</param>
    /// <remarks>仅客户端在未暂停且缓存就绪时接受；专用服务器、暂停或未初始化时不生成。回调异常原样传播，PostSpawn 异常发生时粒子可能已提交。</remarks>
    /// <returns>所有候选都被接受时为 true；false 可能表示环境不允许、容量不足或 PreSpawn 否决。</returns>
    /// <exception cref="ArgumentNullException">粒子或列表为 null。</exception>
    /// <exception cref="ArgumentException">候选列表在可生成环境下包含 null。</exception>
    public static bool TrySpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, false, onlySpawnWhenSpaceEnough);

    private static bool SpawnParticle_Inner(Particle particle, bool forceSpawn)
    {
        ArgumentNullException.ThrowIfNull(particle);
        if (Main.gamePaused || Main.dedServ || _PendingParticles is null || particle.IsTemplate)
            return false;
        if (_Particles.Count + _PendingParticles.Count + _ReservedParticles >= ParticleLimit && !particle.Important && !forceSpawn)
            return false;

        _ReservedParticles++;
        bool accepted;
        try
        {
            accepted = particle.PreSpawn();
        }
        finally
        {
            _ReservedParticles--;
        }
        if (!accepted)
            return false;
        _PendingParticles.Add(particle);
        particle.PostSpawn();
        return true;
    }

    private static bool SpawnParticles_Inner(List<Particle> particles, bool forceSpawn, bool onlySpawnWhenSpaceEnough)
    {
        ArgumentNullException.ThrowIfNull(particles);
        if (Main.gamePaused || Main.dedServ || _PendingParticles is null)
            return false;
        Particle[] candidates = particles.ToArray();
        if (candidates.Any(particle => particle is null))
            throw new ArgumentException("Particle batches cannot contain null entries.", nameof(particles));
        if (candidates.Any(particle => particle.IsTemplate))
            return false;
        if (!onlySpawnWhenSpaceEnough)
        {
            bool allAccepted = true;
            foreach (Particle particle in candidates)
                allAccepted &= SpawnParticle_Inner(particle, forceSpawn);
            return allAccepted;
        }
        if (!forceSpawn && (long)_Particles.Count + _PendingParticles.Count + _ReservedParticles + candidates.Length > ParticleLimit)
            return false;

        // 预留整组容量，防止 PreSpawn 中递归生成其他粒子占用已承诺的空间。
        _ReservedParticles += candidates.Length;
        try
        {
            foreach (Particle particle in candidates)
                if (!particle.PreSpawn())
                    return false;
        }
        finally
        {
            _ReservedParticles -= candidates.Length;
        }
        _PendingParticles.AddRange(candidates);
        foreach (Particle particle in candidates)
            particle.PostSpawn();
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 清空活动粒子、待加入缓冲区与混合模式绘制列表，避免跨世界保留粒子。
    /// </remarks>
    public override void OnWorldUnload()
    {
        _Particles?.Clear();
        _PendingParticles?.Clear();
        _ParticlesToDrawAlphaBlend?.Clear();
        _ParticlesToDrawNonPremultiplied?.Clear();
        _ParticlesToDrawAdditive?.Clear();
        _ParticlesToDrawOpaque?.Clear();
    }

    /// <summary>
    /// 获取指定粒子类型的模板实例。
    /// </summary>
    public static T GetTemplateInstance<T>() where T : Particle => (T)GetDefinition<T>().TemplateInstance;

    /// <summary>
    /// 获取已初始化的粒子定义；禁用、服务器或缓存未就绪时抛出异常。
    /// </summary>
    /// <typeparam name="T">具体运行粒子类型。</typeparam>
    public static ParticleDefinition GetDefinition<T>() where T : Particle => GetDefinition(typeof(T));

    internal static ParticleDefinition GetDefinition(Type type)
    {
        if (_ParticleTypes is null || !_ParticleTypes.TryGetValue(type, out int id)
            || !_ParticleCache.TryGetValue(id, out ParticleDefinition definition) || definition.ID < 0)
            throw new InvalidOperationException($"Particle {type} is disabled or its registered definition is not initialized.");
        return definition;
    }

    /// <summary>
    /// 获取指定粒子类型的纹理。
    /// </summary>
    public static Texture2D GetTexture<T>() where T : Particle => GetDefinition<T>().TemplateInstance.Texture;
}
