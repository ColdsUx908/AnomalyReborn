namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Rectangle rect)
    {
        public LineSegment TopSide => new(rect.TopLeft(), rect.TopRight());

        public LineSegment BottomSide => new(rect.BottomLeft(), rect.BottomRight());

        public LineSegment LeftSide => new(rect.TopLeft(), rect.BottomLeft());

        public LineSegment RightSide => new(rect.BottomLeft(), rect.BottomRight());

        public bool Contains(Vector2 point) =>
            point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    extension(Rectangle)
    {
        public static Rectangle FromCenter(Vector2 center, float width, float height)
        {
            Vector2 topLeft = center - new Vector2(width / 2f, height / 2f);
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)width, (int)height);
        }
    }
}