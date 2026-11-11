using Transoceanic.Core.ModIntegration;

namespace Transoceanic.Graphics.Particles;

/// <summary>
/// 已注册的粒子类型定义，与带参构造的运行粒子分离。
/// 为兼容现有 TexturePath/AutoLoadTexture 重写，元数据模板仍不执行运行构造函数。
/// 客户端与专用服务器均初始化定义和模板资产；服务器的提交、更新及绘制由粒子系统统一跳过。
/// </summary>
[Autoload(false)]
public class ParticleDefinition : RegisteredContent<ParticleDefinition>
{
    /// <summary>
    /// 此定义对应的具体运行粒子类型。
    /// </summary>
    public Type ParticleType { get; }

    /// <summary>
    /// 当前加载周期的本地类型编号；未初始化时为 -1，不用于网络或存档。
    /// </summary>
    public int ID { get; private set; } = -1;

    /// <summary>
    /// 仅用于类型配置与兼容查询的模板，不可提交到粒子队列。
    /// </summary>
    public Particle TemplateInstance { get; private set; }

    /// <inheritdoc/>
    /// <remarks>
    /// 以运行粒子的完整 CLR 类型名标识内容。
    /// </remarks>
    public override string Name => ParticleType.FullName;

    /// <summary>
    /// 为具体粒子创建内容定义；手动注册时在所属 Mod.Load 中调用 AddContent。
    /// </summary>
    /// <param name="particleType">非抽象且不含开放泛型的 Particle 派生类型。</param>
    public ParticleDefinition(Type particleType)
    {
        ArgumentNullException.ThrowIfNull(particleType);
        if (!typeof(Particle).IsAssignableFrom(particleType) || particleType.IsAbstract || particleType.ContainsGenericParameters)
            throw new ArgumentException("A concrete Particle type is required.", nameof(particleType));
        ParticleType = particleType;
    }

    /// <inheritdoc/>
    /// <remarks>客户端与专用服务器均允许注册；派生定义仍可重写以禁用具体粒子。</remarks>
    /// <param name="mod">尝试注册内容的模组。</param>
    public override bool IsLoadingEnabled(Mod mod) => true;

    internal void Initialize(int id)
    {
        if (ID >= 0)
            throw new InvalidOperationException($"Particle definition {FullName} was initialized twice.");
        ID = id;
        TemplateInstance = (Particle)RuntimeHelpers.GetUninitializedObject(ParticleType);
        TemplateInstance.Type = id;
        TemplateInstance.Definition = this;
        if (TemplateInstance.AutoLoadTexture)
        {
            string path = TemplateInstance.TexturePath;
            if (string.IsNullOrEmpty(path))
                path = ParticleType.Namespace.Replace('.', '/') + "/" + ParticleType.Name;
            TemplateInstance.Asset = ModContent.Request<Texture2D>(path);
        }
    }

    /// <inheritdoc/>
    protected override void ClearReferences()
    {
        TemplateInstance?.Asset = null;
        TemplateInstance = null;
        ID = -1;
    }
}
