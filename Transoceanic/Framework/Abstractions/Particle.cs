namespace Transoceanic.Framework.Abstractions;

public abstract class Particle
{
    /// <summary>
    /// 粒子的内部ID。该值在粒子构造时自动设置，且对于同一粒子类型的所有实例都相同。
    /// </summary>
    public int Type;

    /// <summary>
    /// 粒子存在的时间（单位为帧）。当粒子被生成时被自动设置为 <c>0</c>，并且在每次 <see cref="Update()"/> 调用<strong>前</strong>自动递增。
    /// </summary>
    public int Timer;

    /// <summary>
    /// 如果设置为 <see langword="true"/>，则该粒子在 <see cref="ParticleHandler.SpawnParticle(Particle)"/> 中被视为重要粒子，即使粒子数目已达上限也会被生成。
    /// </summary>
    public bool Important;

    public Asset<Texture2D> Asset;
    public int Lifetime = 0;
    public Vector2 Center;
    public Vector2 Velocity;
    public Color Color;
    public float Rotation;
    public float Scale;
    public bool AffectedByLight;

    public Texture2D Texture => Asset.Value;
    public float LifetimeCompletion => Lifetime != 0 ? Timer / (float)Lifetime : 0;

    public Particle()
    {
        Type = ParticleHandler._particleTypes[GetType()];
        Asset = ParticleHandler._particleCache[Type].TemplateInstance.Asset;
    }

    public virtual bool AutoUpdatePosition => true;
    public virtual bool AutoKillByLifeTime => true;
    public virtual bool AutoLoadTexture => true;
    public virtual string TexturePath => "";

    public virtual bool PreSpawn() => true;

    public virtual void PostSpawn() { }

    /// <summary>
    /// 更新粒子状态。
    /// <br/>该方法在 <see cref="Timer"/> 递增<strong>后</strong>，<see cref="Center"/> 的值加上 <see cref="Velocity"/> 的值<strong>前</strong>被调用。
    /// </summary>
    public virtual void Update() { }

    public virtual Rectangle? GetFrame(Texture2D texture) => null;

    public virtual BlendState DrawBlendState => BlendState.AlphaBlend;

    /// <summary>
    /// 在 <see cref="ParticleHandler"/> 的默认绘制代码调用前调用。
    /// <br/>返回 <see langword="false"/> 来禁止默认绘制代码的调用。
    /// </summary>
    public virtual bool PreDraw(SpriteBatch spriteBatch) => true;

    /// <summary>
    /// 在 <see cref="ParticleHandler"/> 的默认绘制代码调用后调用。
    /// <br/>如果已覆写 <see cref="PreDraw(SpriteBatch)"/> 并返回 <see langword="false"/> 来禁止默认绘制代码的调用，则不建议使用该方法，因为可以在 <see cref="PreDraw(SpriteBatch)"/> 中实现所有绘制逻辑。
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void PostDraw(SpriteBatch spriteBatch) { }

    public void Kill() => ParticleHandler.AddToRemoveList(this);
}