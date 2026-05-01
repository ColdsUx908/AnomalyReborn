// Developed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Rectangle rect)
    {
        /// <summary>
        /// 获取矩形顶部边的线段表示。
        /// </summary>
        public LineSegment TopSide => new(rect.TopLeft(), rect.TopRight());

        /// <summary>
        /// 获取矩形底部边的线段表示。
        /// </summary>
        public LineSegment BottomSide => new(rect.BottomLeft(), rect.BottomRight());

        /// <summary>
        /// 获取矩形左侧边的线段表示。
        /// </summary>
        public LineSegment LeftSide => new(rect.TopLeft(), rect.BottomLeft());

        /// <summary>
        /// 获取矩形右侧边的线段表示。
        /// </summary>
        public LineSegment RightSide => new(rect.BottomLeft(), rect.BottomRight());

        /// <summary>
        /// 判断指定二维坐标点是否位于矩形内部（包含边界）。
        /// </summary>
        /// <param name="point">要检测的二维点。</param>
        /// <returns>若点在矩形区域内（含边界）则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool Contains(Vector2 point) =>
            point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    extension(Rectangle)
    {
        /// <summary>
        /// 根据中心点坐标和尺寸创建一个矩形。
        /// </summary>
        /// <param name="center">矩形的中心点坐标。</param>
        /// <param name="width">矩形的宽度。</param>
        /// <param name="height">矩形的高度。</param>
        /// <returns>以 <paramref name="center"/> 为中心、具有指定宽度和高度的矩形。</returns>
        public static Rectangle FromCenter(Vector2 center, float width, float height)
        {
            Vector2 topLeft = center - new Vector2(width / 2f, height / 2f);
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)width, (int)height);
        }
    }
}