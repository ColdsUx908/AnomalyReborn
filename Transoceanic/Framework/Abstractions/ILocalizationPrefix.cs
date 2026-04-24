namespace Transoceanic.Framework.Abstractions;

//含扩展

/// <summary>
/// 提供本地化前缀的接口，用于统一管理本地化键的命名空间。
/// </summary>
public interface ILocalizationPrefix
{
    /// <summary>
    /// 获取本地化键的前缀字符串。
    /// <br/>不需要以点号结尾，因为使用相关扩展方法时会自动添加点号。
    /// </summary>
    public abstract string LocalizationPrefix { get; }
}