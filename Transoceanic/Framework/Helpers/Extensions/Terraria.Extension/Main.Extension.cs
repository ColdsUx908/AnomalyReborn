namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Main)
    {
        public static Vector2 ScreenCenter
        {
            get => Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;
            set => Main.screenPosition = value - Main.ScreenSize.ToVector2() / 2f;
        }
    }
}
