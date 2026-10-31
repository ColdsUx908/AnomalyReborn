namespace Transoceanic.Graphics.Particles;

/// <summary>
/// 为普通粒子类型注册对应 ModType 定义；由 tML 的 Autoload 筛选和所属 Mod 驱动。
/// </summary>
public sealed class ParticleAutoloader : IAutoloader
{
    /// <summary>
    /// 无需粒子无参构造函数即可注册定义，不构造运行粒子。
    /// </summary>
    /// <param name="mod">粒子类型所属模组。</param>
    /// <param name="type">由 tML 筛选后的具体粒子类型。</param>
    public static void Autoload(Mod mod, Type type)
    {
        if (!Main.dedServ)
            mod.AddContent(new ParticleDefinition(type));
    }
}
