namespace Anomalies.Common;

public sealed class StoryModeCommand : ModCommand, ILocalizationPrefix
{
    public override string Command => "ca~storymode";
    public override CommandType Type => CommandType.World;
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + "Commands.StoryMode";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (AnomalySharedData.StoryMode)
        {
            AnomalySharedData.StoryMode = false;
            caller.ReplyLocalizedText(this, "Disable", AnomalySharedData.RebornColor);

        }
        else
        {
            AnomalySharedData.StoryMode = true;
            caller.ReplyLocalizedText(this, "Enable", AnomalySharedData.RebornColor);
        }
    }
}

public sealed class EXModeCommand : ModCommand, ILocalizationPrefix
{
    public override string Command => "ca~exmode";
    public override CommandType Type => CommandType.World;
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + "Commands.EXMode";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (AnomalySharedData.EXMode)
        {
            AnomalySharedData.EXMode = false;
            caller.ReplyLocalizedText(this, "Disable", Color.Red);
        }
        else
        {
            AnomalySharedData.EXMode = true;
            caller.ReplyLocalizedText(this, "Enable", Color.Red);
        }
    }
}