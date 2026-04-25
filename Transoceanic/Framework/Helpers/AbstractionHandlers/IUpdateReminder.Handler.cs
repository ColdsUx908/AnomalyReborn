// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

/// <summary>
/// 处理更新提醒的 <see cref="ModSystem"/>。
/// <para>在游戏运行特定时间后，触发所有注册的更新提醒回调。</para>
/// </summary>
public sealed class UpdateReminderHandler : ModSystem, ILocalizationPrefix, IContentLoader
{
    private static event Action UpdateReminder;
    private static event Action ExternalUpdateReminder;

    /// <summary>
    /// 获取此更新提醒使用的本地化前缀。
    /// </summary>
    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Core.UpdateReminder";

    /// <summary>
    /// 在实体更新前检查游戏时间，到达指定时刻后调用所有注册的更新提醒。
    /// </summary>
    public override void PreUpdateEntities()
    {
        if (TOSharedData.GameTimer == 180 && (UpdateReminder is not null || ExternalUpdateReminder is not null))
        {
            TOLocalizationUtils.ChatLiteralText($"[{this.GetTextValue("Header")}]", TOSharedData.TODebugWarnColor, Main.LocalPlayer);
            UpdateReminder?.Invoke();
            if (ExternalUpdateReminder is not null)
            {
                TOLocalizationUtils.ChatLiteralText(this.GetTextValue("ExternalHeader"), TOSharedData.TODebugWarnColor, Main.LocalPlayer);
                ExternalUpdateReminder();
            }
            TOLocalizationUtils.ChatLiteralText($"[Transoceanic] {this.GetTextValue("Message")}", TOSharedData.TODebugWarnColor, Main.LocalPlayer);
        }
    }

    /// <summary>
    /// 内容设置后阶段：扫描所有实现了 <see cref="IUpdateReminder"/> 和 <see cref="IExternalUpdateReminder"/> 的类型，
    /// 并订阅其注册的回调。
    /// </summary>
    void IContentLoader.PostSetupContent()
    {
        foreach (IUpdateReminder updateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IUpdateReminder>())
            UpdateReminder += updateReminder.RegisterUpdateReminder();
        foreach (IExternalUpdateReminder externalUpdateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IExternalUpdateReminder>())
            ExternalUpdateReminder += externalUpdateReminder.RegisterUpdateReminder();
    }

    /// <summary>
    /// 模组卸载时清理事件订阅。
    /// </summary>
    void IContentLoader.OnModUnload() => UpdateReminder = null;
}