// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 粒子系统全局处理器，负责粒子的更新、绘制以及生命周期管理。
/// </summary>
public sealed class ParticleHandler : ModSystem, IContentLoader
{
    /// <summary>
    /// 粒子纹理资源的基础路径。
    /// </summary>
    public const string BaseParticleTexturePath = "Transoceanic/DataStructures/Particles/";

    /// <summary>
    /// 粒子数量限制。
    /// <br/>当 <see cref="_particles"/> 中的粒子数量达到该值时，除非新生成的粒子被标记为重要粒子，否则将不会被生成。
    /// </summary>
    public static int ParticleLimit { get; set; } = 5000;

    /// <summary>
    /// 存储单个粒子类型的元数据缓存，包括类型、模板实例及唯一 ID。
    /// </summary>
    internal sealed record ParticleDataCache
    {
        public static int _nextID = 0;

        public readonly Type Type;
        public readonly Particle TemplateInstance;
        public readonly int ID;

        private ParticleDataCache(Type type, Particle templateInstance)
        {
            Type = type;
            TemplateInstance = templateInstance;
            ID = _nextID++;
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
            if (_particleTypes.TryGetValue(type, out int existingId) && _particleCache.TryGetValue(existingId, out ParticleDataCache existingCache))
                return existingCache;

            ParticleDataCache newCache = new(type, templateInstance);

            _particleCache[newCache.ID] = newCache;
            _particleTypes[type] = newCache.ID;

            return newCache;
        }
    }

    internal static Dictionary<int, ParticleDataCache> _particleCache;
    internal static Dictionary<Type, int> _particleTypes;

    private static List<Particle> _particles;
    private static List<Particle> _particlesToKill;

    private static List<Particle> _particlesToDraw_AlphaBlend;
    private static List<Particle> _particlesToDraw_NonPremultiplied;
    private static List<Particle> _particlesToDraw_Additive;
    private static List<Particle> _particlesToDraw_Opaque;

    /// <summary>
    /// 绘制所有活跃粒子。根据不同混合状态分组绘制以减少渲染状态切换。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch 实例。</param>
    public static void Draw(SpriteBatch spriteBatch)
    {
        if (Main.dedServ)
            return;

        if (_particles.Count == 0)
            return;

        //提前分类粒子以减少spriteBatch状态切换次数
        foreach (Particle particle in _particles)
        {
            if (particle is null)
                continue;

            BlendState blendState = particle.DrawBlendState;
            if (blendState == BlendState.AlphaBlend)
                _particlesToDraw_AlphaBlend.Add(particle);
            else if (blendState == BlendState.NonPremultiplied)
                _particlesToDraw_NonPremultiplied.Add(particle);
            else if (blendState == BlendState.Additive)
                _particlesToDraw_Additive.Add(particle);
            else if (blendState == BlendState.Opaque)
                _particlesToDraw_Opaque.Add(particle);
        }

        if (_particlesToDraw_AlphaBlend.Count > 0)
        {
            EnterDrawRegion_AlphaBlend(spriteBatch);

            foreach (Particle particle in _particlesToDraw_AlphaBlend)
                DrawParticle(spriteBatch, particle);
        }

        if (_particlesToDraw_NonPremultiplied.Count > 0)
        {
            EnterDrawRegion_NonPremultiplied(spriteBatch);

            foreach (Particle particle in _particlesToDraw_NonPremultiplied)
                DrawParticle(spriteBatch, particle);
        }

        if (_particlesToDraw_Additive.Count > 0)
        {
            EnterDrawRegion_Additive(spriteBatch);

            foreach (Particle particle in _particlesToDraw_Additive)
                DrawParticle(spriteBatch, particle);
        }

        if (_particlesToDraw_Opaque.Count > 0)
        {
            EnterDrawRegion_Opaque(spriteBatch);

            foreach (Particle particle in _particlesToDraw_Opaque)
                DrawParticle(spriteBatch, particle);
        }

        _particlesToDraw_AlphaBlend.Clear();
        _particlesToDraw_NonPremultiplied.Clear();
        _particlesToDraw_Additive.Clear();

        ExitParticleDrawRegion(spriteBatch);

        static void DrawParticle(SpriteBatch spriteBatch, Particle particle)
        {
            if (particle.PreDraw(spriteBatch))
            {
                Texture2D texture = particle.Texture;
                Rectangle? frame = particle.GetFrame(texture);
                Color color = particle.Color;
                if (particle.AffectedByLight)
                    color.MultiplyWithWorldLight(particle.Center);
                spriteBatch.DrawFromCenter(texture, particle.Center - Main.screenPosition, frame, color, particle.Rotation, particle.Scale, SpriteEffects.None, 0f);
            }

            particle.PostDraw(spriteBatch);
        }
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
    /// 退出粒子绘制区域，恢复默认渲染状态。
    /// </summary>
    public static void ExitParticleDrawRegion(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    /// <summary>
    /// 在所有实体更新后处理粒子的更新与移除。
    /// </summary>
    public override void PostUpdateEverything()
    {
        if (Main.dedServ)
            return;

        foreach (Particle particle in _particles)
        {
            if (particle is null)
                continue;
            UpdateParticle(particle);
        }

        _particles.RemoveAll(particle => particle is null || (particle.Timer >= particle.Lifetime && particle.AutoKillByLifeTime) || _particlesToKill.Contains(particle));
        _particlesToKill.Clear();
    }

    internal static void UpdateParticle(Particle particle)
    {
        particle.Timer++;
        particle.Update();
        if (particle.AutoUpdatePosition)
            particle.Center += particle.Velocity;
    }

    void IContentLoader.PostSetupContent()
    {
        _particleCache = [];
        _particleTypes = [];
        _particles = [];
        _particlesToKill = [];
        _particlesToDraw_AlphaBlend = [];
        _particlesToDraw_NonPremultiplied = [];
        _particlesToDraw_Additive = [];
        _particlesToDraw_Opaque = [];

        ParticleDataCache._nextID = 0;

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
        ParticleDataCache._nextID = 0;

        _particleCache = null;
        _particleTypes = null;
        _particles = null;
        _particlesToKill = null;
        _particlesToDraw_AlphaBlend = null;
        _particlesToDraw_NonPremultiplied = null;
        _particlesToDraw_Additive = null;
        _particlesToDraw_Opaque = null;
    }

    /// <summary>
    /// 向 <see cref="_particles"/> 中添加一个粒子实例以生成该粒子。
    /// </summary>
    public static void SpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    /// <summary>
    /// 尝试向 <see cref="_particles"/> 中添加一个粒子实例以生成该粒子。
    /// </summary>
    public static bool TrySpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    /// <summary>
    /// 向 <see cref="_particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    public static void SpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, false, onlySpawnWhenSpaceEnough);

    /// <summary>
    /// 尝试向 <see cref="_particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    public static bool TrySpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, false, onlySpawnWhenSpaceEnough);

    private static bool SpawnParticle_Inner(Particle particle, bool forceSpawn)
    {
        if (Main.gamePaused || Main.dedServ || _particles is null)
            return false;

        if (_particles.Count >= ParticleLimit && !particle.Important && !forceSpawn)
            return false;

        if (particle.PreSpawn())
            _particles.Add(particle);
        particle.PostSpawn();

        return true;
    }

    private static bool SpawnParticles_Inner(List<Particle> particles, bool forceSpawn, bool onlySpawnWhenSpaceEnough)
    {
        if (Main.gamePaused || Main.dedServ || _particles is null)
            return false;

        int newParticlesCount = particles.Count;
        if (!forceSpawn && onlySpawnWhenSpaceEnough && _particles.Count + newParticlesCount > ParticleLimit)
            return false;

        foreach (Particle particle in particles)
        {
            if (particle.PreSpawn())
                _particles.Add(particle);
            particle.PostSpawn();
        }

        return true;
    }

    /// <summary>
    /// 将指定粒子标记为待移除。
    /// </summary>
    public static void AddToRemoveList(Particle particle)
    {
        if (Main.dedServ)
            return;

        _particlesToKill.Add(particle);
    }

    /// <summary>
    /// 获取指定粒子类型的模板实例。
    /// </summary>
    public static T GetTemplateInstance<T>() where T : Particle => (T)_particleCache[_particleTypes[typeof(T)]].TemplateInstance;

    /// <summary>
    /// 获取指定粒子类型的纹理。
    /// </summary>
    public static Texture2D GetTexture<T>() where T : Particle => _particleCache[_particleTypes[typeof(T)]].TemplateInstance.Texture;
}