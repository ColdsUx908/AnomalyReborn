// Developed by ColdsUx

namespace CalamityAnomalies.Common;

public sealed class CACommands : ModCommand, ILocalizationPrefix
{
    public override string Command => "ca~storymode";

    public override CommandType Type => CommandType.World;

    public string LocalizationPrefix => CASharedData.ModLocalizationPrefix + "Commands.StoryMode";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (CASharedData.StoryMode)
        {
            CASharedData.StoryMode = false;
            caller.ReplyLocalizedText(this, "Disable", CASharedData.RebornColor);

        }
        else
        {
            CASharedData.StoryMode = true;
            caller.ReplyLocalizedText(this, "Enable", CASharedData.RebornColor);
        }
    }
}
