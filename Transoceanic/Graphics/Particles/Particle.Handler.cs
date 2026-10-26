using Transoceanic.Graphics.Drawing;
using Transoceanic.Runtime.Reflection;
namespace Transoceanic.Graphics.Particles;

/// <summary>
/// 粒子系统全局处理器，负责粒子的更新、绘制以及生命周期管理。
/// </summary>
public sealed class ParticleHandler : ModSystem, IContentLoader
{
    /// <summary>
    /// 粒子纹理资源的基础路径。
    /// </summary>
    public const string BaseParticleTexturePath = "Transoceanic/Assets/Textures/Particles/";

    /// <summary>
    /// 粒子数量限制。
    /// <br/>当 <see cref="_Particles"/> 中的粒子数量达到该值时，除非新生成的粒子被标记为重要粒子，否则将不会被生成。
    /// </summary>
    public static int ParticleLimit { get; set; } = 5000;

    /// <summary>
    /// 存储单个粒子类型的元数据缓存，包括类型、模板实例及唯一 ID。
    /// </summary>
    internal sealed record ParticleDataCache
    {
        public static int _NextID = 0;

        public readonly Type Type;
        public readonly Particle TemplateInstance;
        public readonly int ID;

        private ParticleDataCache(Type type, Particle templateInstance)
        {
            Type = type;
            TemplateInstance = templateInstance;
            ID = _NextID++;
            if (templateInstance.AutoLoadTexture)
            {
                string texturePath = templateInstance.TexturePath != "" ? templateInstance.TexturePath : type.Namespace.Replace('.', '/') + "/" + type.Name;
                Asset<Texture2D> asset = ModContent.Request<Texture2D>(texturePath);
                TemplateInstance.Asset = asset;
            }
        }

        /// <summary>
        /// 创建或获取粒子类型的缓存数据。
        /// </summary>
        /// <param name="type">粒子类型。</param>
        /// <param name="templateInstance">粒子的模板实例。</param>
        /// <returns>对应的缓存数据。</returns>
        public static ParticleDataCache Create(Type type, Particle templateInstance)
        {
            if (_ParticleTypes.TryGetValue(type, out int existingId) && _ParticleCache.TryGetValue(existingId, out ParticleDataCache existingCache))
                return existingCache;

            ParticleDataCache newCache = new(type, templateInstance);

            _ParticleCache[newCache.ID] = newCache;
            _ParticleTypes[type] = newCache.ID;

            return newCache;
        }
    }

    internal static Dictionary<int, ParticleDataCache> _ParticleCache;
    internal static Dictionary<Type, int> _ParticleTypes;

    private static List<Particle> _Particles;

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
        if (Main.dedServ)
            return;

        if (_Particles.Count == 0)
            return;

        //提前分类粒子以减少spriteBatch状态切换次数
        foreach (Particle particle in _Particles)
        {
            if (particle is null)
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
                DrawParticle(spriteBatch, particle);
        }

        if (_ParticlesToDrawNonPremultiplied.Count > 0)
        {
            EnterDrawRegion_NonPremultiplied(spriteBatch);

            foreach (Particle particle in _ParticlesToDrawNonPremultiplied)
                DrawParticle(spriteBatch, particle);
        }

        if (_ParticlesToDrawAdditive.Count > 0)
        {
            EnterDrawRegion_Additive(spriteBatch);

            foreach (Particle particle in _ParticlesToDrawAdditive)
                DrawParticle(spriteBatch, particle);
        }

        if (_ParticlesToDrawOpaque.Count > 0)
        {
            EnterDrawRegion_Opaque(spriteBatch);

            foreach (Particle particle in _ParticlesToDrawOpaque)
                DrawParticle(spriteBatch, particle);
        }

        _ParticlesToDrawAlphaBlend.Clear();
        _ParticlesToDrawNonPremultiplied.Clear();
        _ParticlesToDrawAdditive.Clear();
        TODrawUtils.
                ResetSpriteBatch(spriteBatch);
    }

    /// <summary>
    /// 绘制单个粒子实例。根据粒子自身的属性决定是否受光照影响，并调用粒子的预绘制和后绘制方法。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch 实例。</param>
    /// <param name="particle">要绘制的粒子实例。</param>
    /// <param name="drawOffset">可选的绘制偏移量。</param>
    public static void DrawParticle(SpriteBatch spriteBatch, Particle particle, Vector2 drawOffset = default)
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

    /// <summary>
    /// 在所有实体更新后处理粒子的更新与移除。
    /// </summary>
    public override void PostUpdateEverything()
    {
        if (Main.dedServ)
            return;

        foreach (Particle particle in _Particles)
        {
            if (particle is null)
                continue;
            UpdateParticle(particle);
        }

        _Particles.RemoveAll(particle => particle is null || (particle.Timer >= particle.Lifetime && particle.AutoKillByLifeTime) || particle.Dead);
    }

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
        _ParticlesToDrawAlphaBlend = [];
        _ParticlesToDrawNonPremultiplied = [];
        _ParticlesToDrawAdditive = [];
        _ParticlesToDrawOpaque = [];

        ParticleDataCache._NextID = 0;

        foreach ((Type type, Particle instance) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<Particle>(true))
            ParticleDataCache.Create(type, instance);

        //在绘制狱火药水效果前绘制粒子
        On_Main.DrawInfernoRings += (orig, self) =>
        {
            Draw(Main.spriteBatch);
            orig(self);
        };
    }

    void IContentLoader.OnModUnload()
    {
        ParticleDataCache._NextID = 0;

        _ParticleCache = null;
        _ParticleTypes = null;
        _Particles = null;
        _ParticlesToDrawAlphaBlend = null;
        _ParticlesToDrawNonPremultiplied = null;
        _ParticlesToDrawAdditive = null;
        _ParticlesToDrawOpaque = null;
    }

    /// <summary>
    /// 向 <see cref="_Particles"/> 中添加一个粒子实例以生成该粒子。
    /// </summary>
    public static void SpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    /// <summary>
    /// 尝试向 <see cref="_Particles"/> 中添加一个粒子实例以生成该粒子。
    /// </summary>
    public static bool TrySpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    /// <summary>
    /// 向 <see cref="_Particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    public static void SpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, false, onlySpawnWhenSpaceEnough);

    /// <summary>
    /// 尝试向 <see cref="_Particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    public static bool TrySpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, false, onlySpawnWhenSpaceEnough);

    private static bool SpawnParticle_Inner(Particle particle, bool forceSpawn)
    {
        if (Main.gamePaused || Main.dedServ || _Particles is null)
            return false;

        if (_Particles.Count >= ParticleLimit && !particle.Important && !forceSpawn)
            return false;

        if (particle.PreSpawn())
            _Particles.Add(particle);
        particle.PostSpawn();

        return true;
    }

    private static bool SpawnParticles_Inner(List<Particle> particles, bool forceSpawn, bool onlySpawnWhenSpaceEnough)
    {
        if (Main.gamePaused || Main.dedServ || _Particles is null)
            return false;

        int newParticlesCount = particles.Count;
        if (!forceSpawn && onlySpawnWhenSpaceEnough && _Particles.Count + newParticlesCount > ParticleLimit)
            return false;

        foreach (Particle particle in particles)
        {
            if (particle.PreSpawn())
                _Particles.Add(particle);
            particle.PostSpawn();
        }

        return true;
    }

    /// <summary>
    /// 获取指定粒子类型的模板实例。
    /// </summary>
    public static T GetTemplateInstance<T>() where T : Particle => (T)_ParticleCache[_ParticleTypes[typeof(T)]].TemplateInstance;

    /// <summary>
    /// 获取指定粒子类型的纹理。
    /// </summary>
    public static Texture2D GetTexture<T>() where T : Particle => _ParticleCache[_ParticleTypes[typeof(T)]].TemplateInstance.Texture;
}
