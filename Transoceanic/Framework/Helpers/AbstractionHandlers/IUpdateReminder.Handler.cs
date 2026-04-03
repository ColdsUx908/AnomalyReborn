namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

public sealed class UpdateReminderHandler : ModSystem, ILocalizationPrefix, IContentLoader
{
    private static event Action UpdateReminder;
    private static event Action ExternalUpdateReminder;

    public string LocalizationPrefix => TOSharedData.ModLocalizationPrefix + "Core.UpdateReminder";

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

    void IContentLoader.PostSetupContent()
    {
        foreach (IUpdateReminder updateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IUpdateReminder>())
            UpdateReminder += updateReminder.RegisterUpdateReminder();
        foreach (IExternalUpdateReminder externalUpdateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IExternalUpdateReminder>())
            ExternalUpdateReminder += externalUpdateReminder.RegisterUpdateReminder();
    }

    void IContentLoader.OnModUnload() => UpdateReminder = null;
}
