// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Main)
    {
        /// <summary>
        /// 获取或设置屏幕中心的世界坐标。
        /// </summary>
        /// <remarks>
        /// 获取时返回当前屏幕中心的世界坐标；设置时将调整屏幕位置使中心点移动到指定坐标。
        /// </remarks>
        public static Vector2 ScreenCenter
        {
            get => Main.screenPosition + Main.ScreenSize.ToVector2() / 2f;
            set => Main.screenPosition = value - Main.ScreenSize.ToVector2() / 2f;
        }
    }
}