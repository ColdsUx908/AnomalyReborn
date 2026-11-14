namespace Transoceanic.Graphics.Drawing;

/// <summary>世界坐标到二维视口的不可变快照；不读取全局相机，也不执行额外的长宽比修正。</summary>
/// <param name="ScreenPosition">视口左上角对应的世界偏移。</param>
/// <param name="Transform">将减去世界偏移后的坐标变换为视口像素的矩阵。</param>
/// <param name="Resolution">视口像素尺寸，必须为有限正数。</param>
public readonly record struct WorldView2D(Vector2 ScreenPosition, Matrix Transform, Vector2 Resolution)
{
    /// <summary>返回世界 X 轴一个单位对应的像素长度，包含相机缩放和旋转。</summary>
    public float Scale => new Vector2(Transform.M11, Transform.M12).Length();

    /// <summary>将世界位置转换为相对于当前视口原点的像素坐标。</summary>
    public Vector2 WorldToScreen(Vector2 position) => Vector2.Transform(position - ScreenPosition, Transform);

    /// <summary>转换为未夹取的视口 UV；调用方负责提供有效的视口尺寸。</summary>
    public Vector2 WorldToUv(Vector2 position) => WorldToScreen(position) / Resolution;
}
