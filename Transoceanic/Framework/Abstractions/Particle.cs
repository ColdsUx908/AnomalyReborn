// Designed by ColdsUx

namespace Transoceanic.Framework.Abstractions;

/// <summary>
/// 粒子系统的抽象基类，定义了粒子的基本属性、生命周期行为以及与 <see cref="ParticleHandler"/> 的交互接口。
/// <para>派生类应当通过 <see cref="ParticleHandler.SpawnParticle(Particle)"/> 方法进行实例化与生成，
/// 而非直接使用构造函数。粒子的类型 ID、纹理资源等由 <see cref="ParticleHandler"/> 在内容加载阶段自动缓存与管理。</para>
/// </summary>
public abstract class Particle
{
    /// <summary>
    /// 粒子的内部类型标识符。该值在粒子构造时由 <see cref="ParticleHandler"/> 自动分配，
    /// 对于同一具体粒子类型的所有实例均相同。
    /// <para>该字段可用于在自定义逻辑中识别粒子种类，或通过 <see cref="ParticleHandler.GetTemplateInstance{T}"/> 获取模板实例。</para>
    /// </summary>
    public int Type;

    /// <summary>
    /// 粒子自生成以来经过的时间（以游戏帧为单位）。
    /// <para>当粒子被生成时，该值被初始化为 <c>0</c>；在每次 <see cref="Update"/> 调用<strong>之前</strong>，
    /// <see cref="ParticleHandler"/> 会自动将其递增 <c>1</c>。</para>
    /// <para>可通过 <see cref="Lifetime"/> 与 <see cref="LifetimeCompletion"/> 计算生命周期进度。</para>
    /// </summary>
    public int Timer;

    /// <summary>
    /// 指示该粒子是否为“重要粒子”。重要粒子在 <see cref="ParticleHandler.SpawnParticle(Particle)"/>
    /// 或 <see cref="ParticleHandler.TrySpawnParticle(Particle)"/> 被调用时，
    /// 即使当前粒子总数已达到 <see cref="ParticleHandler.ParticleLimit"/> 上限，也会被强制生成。
    /// <para>适用于必须显示的视觉效果（如关键技能特效、剧情相关粒子等）。</para>
    /// <para>默认值为 <see langword="false"/>。</para>
    /// </summary>
    public bool Important;

    /// <summary>
    /// 粒子所使用的纹理资源资产。该字段在粒子构造时由 <see cref="ParticleHandler"/> 根据
    /// <see cref="AutoLoadTexture"/> 与 <see cref="TexturePath"/> 自动加载并赋值。
    /// <para>通过 <see cref="Texture"/> 属性可获取已加载的 <see cref="Texture2D"/> 实例。</para>
    /// </summary>
    public Asset<Texture2D> Asset;

    /// <summary>
    /// 粒子的最大生命周期（以游戏帧为单位）。
    /// <para>若 <see cref="AutoKillByLifeTime"/> 为 <see langword="true"/>（默认行为），
    /// 当 <see cref="Timer"/> 达到此值时，粒子将在下一更新帧被自动移除。</para>
    /// <para>默认值为 <c>0</c>，表示无限生命周期（需配合 <see cref="AutoKillByLifeTime"/> = <see langword="false"/> 使用）。</para>
    /// </summary>
    public int Lifetime = 0;

    /// <summary>
    /// 粒子在世界坐标系中的中心位置。
    /// <para>若 <see cref="AutoUpdatePosition"/> 为 <see langword="true"/>（默认行为），
    /// 则在每次 <see cref="Update"/> 调用后，该位置将自动加上 <see cref="Velocity"/> 的值。</para>
    /// </summary>
    public Vector2 Center;

    /// <summary>
    /// 粒子的每帧移动速度（单位：像素/帧）。
    /// <para>仅在 <see cref="AutoUpdatePosition"/> 为 <see langword="true"/> 时自动应用于 <see cref="Center"/>。</para>
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    /// 粒子的绘制颜色。该颜色将与纹理相乘，实现色调变化或透明度渐变。
    /// <para>默认值为 <see cref="Color.White"/>（需在派生类构造函数中显式赋值）。</para>
    /// </summary>
    public Color Color;

    /// <summary>
    /// 粒子的旋转角度（以弧度为单位）。绘制时围绕粒子中心点进行旋转。
    /// </summary>
    public float Rotation;

    /// <summary>
    /// 粒子的绘制缩放比例。默认值为 <c>1.0f</c>（需在派生类构造函数中显式赋值）。
    /// </summary>
    public float Scale;

    /// <summary>
    /// 指示粒子是否受环境光照影响。
    /// <para>若为 <see langword="true"/>，绘制时颜色将受当前世界光照颜色调制。
    /// 具体实现取决于 <see cref="ParticleHandler"/> 的绘制管线或自定义 <see cref="PreDraw"/> 逻辑。</para>
    /// <para>默认值为 <see langword="false"/>。</para>
    /// </summary>
    public bool AffectedByLight;

    /// <summary>
    /// 获取已加载的粒子纹理。
    /// <para>该属性直接返回 <see cref="Asset"/>.Value，若资源未正确加载可能引发异常。</para>
    /// </summary>
    public Texture2D Texture => Asset.Value;

    /// <summary>
    /// 获取粒子当前生命周期的完成比例。
    /// <para>计算公式：<c>Timer / (float)Lifetime</c>。若 <see cref="Lifetime"/> 为 <c>0</c>，则返回 <c>0</c>。</para>
    /// <para>常用于实现透明度渐变、缩放动画等基于生命周期进度的效果。</para>
    /// </summary>
    public float LifetimeCompletion => Lifetime != 0 ? Timer / (float)Lifetime : 0;

    /// <summary>
    /// 初始化 <see cref="Particle"/> 类的新实例。构造函数会自动从 <see cref="ParticleHandler"/> 缓存中
    /// 获取与该粒子具体类型对应的 <see cref="Type"/> 和 <see cref="Asset"/>。
    /// <para>注意：粒子实例不应直接通过 <c>new</c> 关键字创建，而应通过 <see cref="ParticleHandler.SpawnParticle(Particle)"/>
    /// 或 <see cref="ParticleHandler.TrySpawnParticle(Particle)"/> 方法生成，以确保正确注册到粒子系统中。</para>
    /// </summary>
    public Particle()
    {
        Type = ParticleHandler._particleTypes[GetType()];
        Asset = ParticleHandler._particleCache[Type].TemplateInstance.Asset;
    }

    /// <summary>
    /// 获取一个值，指示是否在每次更新后自动将 <see cref="Velocity"/> 累加到 <see cref="Center"/> 上。
    /// <para>默认返回 <see langword="true"/>。派生类可重写此属性以禁用自动位置更新，
    /// 从而在 <see cref="Update"/> 中实现完全自定义的运动逻辑。</para>
    /// </summary>
    public virtual bool AutoUpdatePosition => true;

    /// <summary>
    /// 获取一个值，指示当 <see cref="Timer"/> 达到或超过 <see cref="Lifetime"/> 时是否自动移除该粒子。
    /// <para>默认返回 <see langword="true"/>。若派生类需要实现无限生命周期或基于其他条件的移除逻辑，
    /// 可重写此属性返回 <see langword="false"/>，并自行调用 <see cref="Kill"/> 方法。</para>
    /// </summary>
    public virtual bool AutoKillByLifeTime => true;

    /// <summary>
    /// 获取一个值，指示是否应自动加载与该粒子关联的纹理资源。
    /// <para>默认返回 <see langword="true"/>。当为 <see langword="true"/> 时，
    /// <see cref="ParticleHandler"/> 会在内容加载阶段根据 <see cref="TexturePath"/> 或默认命名约定
    /// （命名空间 + 类型名）请求纹理资源，并赋值给 <see cref="Asset"/>。</para>
    /// <para>若派生类不使用纹理或需手动管理资源，可重写为 <see langword="false"/>。</para>
    /// </summary>
    public virtual bool AutoLoadTexture => true;

    /// <summary>
    /// 获取自定义纹理路径，相对于模组的根内容目录（通常为模组文件夹下的 "Content" 文件夹）。
    /// <para>若 <see cref="AutoLoadTexture"/> 为 <see langword="true"/> 且此属性返回非空字符串，
    /// <see cref="ParticleHandler"/> 将使用该路径加载纹理；否则将使用默认命名约定：
    /// <c>命名空间.类型名</c>，并将点号 <c>.</c> 替换为路径分隔符 <c>/</c>。</para>
    /// <para>默认返回空字符串，表示使用默认路径规则。</para>
    /// </summary>
    public virtual string TexturePath => "";

    /// <summary>
    /// 在粒子即将被添加到活动粒子列表之前调用。
    /// <para>可在此方法中执行生成前的条件检查或初始化操作。
    /// 若返回 <see langword="false"/>，则粒子不会被生成，且不会触发后续的 <see cref="PostSpawn"/> 或 <see cref="Update"/>。</para>
    /// <para>默认返回 <see langword="true"/>。</para>
    /// </summary>
    /// <returns>若粒子应正常生成，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public virtual bool PreSpawn() => true;

    /// <summary>
    /// 在粒子成功添加到活动粒子列表后立即调用。
    /// <para>可用于执行依赖于粒子已处于活动状态的初始化工作（例如，设置初始位置偏移、播放声音等）。
    /// 注意：此时 <see cref="Timer"/> 仍为 <c>0</c>，且尚未进行第一次 <see cref="Update"/>。</para>
    /// </summary>
    public virtual void PostSpawn() { }

    /// <summary>
    /// 每帧调用一次，用于更新粒子的自定义状态（如颜色变化、自定义运动、碰撞检测等）。
    /// <para>调用时机：在 <see cref="Timer"/> 自动递增<strong>之后</strong>，
    /// 且在因 <see cref="AutoUpdatePosition"/> 为 <see langword="true"/> 而自动应用 <see cref="Velocity"/> <strong>之前</strong>。</para>
    /// <para>派生类可在此方法中自由修改 <see cref="Color"/>、<see cref="Scale"/>、<see cref="Rotation"/>、
    /// <see cref="Velocity"/> 等属性，甚至调用 <see cref="Kill"/> 提前终止粒子。</para>
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// 获取用于绘制的纹理源矩形（即精灵表上的帧区域）。
    /// <para>若返回 <see langword="null"/>，则绘制整个纹理。派生类可重写此方法以实现动画帧选择或纹理裁剪。</para>
    /// <para>默认返回 <see langword="null"/>。</para>
    /// </summary>
    /// <param name="texture">当前粒子所使用的完整纹理。</param>
    /// <returns>要绘制的矩形区域；若为 <see langword="null"/>，则使用整个纹理。</returns>
    public virtual Rectangle? GetFrame(Texture2D texture) => null;

    /// <summary>
    /// 获取绘制该粒子时应使用的混合状态。
    /// <para><see cref="ParticleHandler"/> 会根据此属性将粒子归类到不同的绘制批次，
    /// 以优化 <see cref="SpriteBatch"/> 的状态切换次数。</para>
    /// <para>默认返回 <see cref="BlendState.AlphaBlend"/>，适用于常规透明混合。
    /// 派生类可重写以返回 <see cref="BlendState.Additive"/>、<see cref="BlendState.NonPremultiplied"/> 等。</para>
    /// </summary>
    public virtual BlendState DrawBlendState => BlendState.AlphaBlend;

    /// <summary>
    /// 在 <see cref="ParticleHandler"/> 执行默认粒子绘制代码之前调用。
    /// <para>可在此方法中设置自定义的 <see cref="SpriteBatch"/> 参数（如着色器、变换矩阵等），
    /// 或直接进行自定义绘制。</para>
    /// <para>若返回 <see langword="false"/>，则将跳过 <see cref="ParticleHandler"/> 的默认绘制逻辑，
    /// 此时建议在本方法中完成所有绘制工作，且通常无需再使用 <see cref="PostDraw"/>。</para>
    /// <para>默认返回 <see langword="true"/>，允许执行默认绘制。</para>
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 <see cref="SpriteBatch"/> 实例，
    /// 其状态（如混合模式、变换矩阵）已由 <see cref="ParticleHandler"/> 按照当前批次设置好。</param>
    /// <returns>若应继续执行默认绘制代码，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public virtual bool PreDraw(SpriteBatch spriteBatch) => true;

    /// <summary>
    /// 在 <see cref="ParticleHandler"/> 执行默认粒子绘制代码之后调用。
    /// <para>可用于绘制额外叠加效果（如发光光晕、文字标签等），或恢复在 <see cref="PreDraw"/> 中修改的渲染状态。</para>
    /// <para>注意：如果 <see cref="PreDraw"/> 返回了 <see langword="false"/>，则默认绘制被跳过，
    /// 此方法仍会被调用，但通常不应在此方法中再进行主要绘制，以免逻辑混乱。</para>
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 <see cref="SpriteBatch"/> 实例。</param>
    public virtual void PostDraw(SpriteBatch spriteBatch) { }

    /// <summary>
    /// 标记该粒子在下一更新帧被移除。
    /// <para>调用此方法等效于将粒子添加到 <see cref="ParticleHandler"/> 内部的移除列表中。
    /// 移除操作在 <see cref="ParticleHandler.PostUpdateEverything"/> 中批量执行，
    /// 因此粒子不会在调用后立即从活动列表中消失，但不会再被更新或绘制。</para>
    /// <para>若需立即停止粒子的所有行为（包括当前帧的绘制），可结合条件判断与 <see cref="PreDraw"/> 返回 <see langword="false"/>。</para>
    /// </summary>
    public void Kill() => ParticleHandler.AddToRemoveList(this);
}