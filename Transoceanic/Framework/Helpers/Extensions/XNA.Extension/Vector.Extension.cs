namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Vector2 vector)
    {
        /// <summary>
        /// 将向量的分量解构为两个 <see langword="float"/> 变量。
        /// </summary>
        /// <param name="x">接收 X 分量的输出变量。</param>
        /// <param name="y">接收 Y 分量的输出变量。</param>
        public void Deconstruct(out float x, out float y)
        {
            x = vector.X;
            y = vector.Y;
        }

        /// <summary>
        /// 获取向量的旋转角度，并加上指定的偏移量。
        /// </summary>
        /// <param name="rotationOffset">要添加的旋转偏移量（弧度）。</param>
        /// <returns>原始向量的旋转角度与偏移量之和。</returns>
        public float ToRotation(float rotationOffset) => vector.ToRotation() + rotationOffset;

        /// <summary>
        /// 安全地将向量转换为单位向量。若为零向量则返回零向量。
        /// </summary>
        /// <returns>若向量非零则返回其单位向量，否则返回 <see cref="Vector2.Zero"/>。</returns>
        public Vector2 SafeNormalize() => vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);

        /// <summary>
        /// 获取与原向量共线且具有指定长度的向量，不改变原向量。
        /// </summary>
        /// <param name="length">目标向量的长度。</param>
        /// <returns>与原向量方向相同、长度为 <paramref name="length"/> 的新向量。若原向量为零向量则返回零向量。</returns>
        public Vector2 ToCustomLength(float length) => vector.SafeNormalize() * length;

        /// <summary>
        /// 将向量绕原点随机旋转，随机角度范围为 [0, 2π)。
        /// </summary>
        /// <returns>随机旋转后的新向量。</returns>
        public Vector2 RotatedByRandom() => vector.RotatedByRandom(MathHelper.Pi);

        /// <summary>
        /// 将向量绕原点随机旋转，随机角度在指定范围内均匀选取。
        /// </summary>
        /// <param name="minRadian">随机旋转的最小弧度。</param>
        /// <param name="maxRadian">随机旋转的最大弧度。</param>
        /// <returns>随机旋转后的新向量。</returns>
        public Vector2 RotatedByRandom(float minRadian, float maxRadian) => vector.RotatedByRandom(Main.rand.NextFloat(minRadian, maxRadian));

        /// <summary>
        /// 获取当前世界坐标对应的安全图格坐标（将世界坐标右移4位并限制在有效图格范围内）。
        /// </summary>
        public Point WorldCoordinateSafe => new(Math.Clamp((int)vector.X >> 4, 0, Main.maxTilesX), Math.Clamp((int)vector.Y >> 4, 0, Main.maxTilesY));
    }

#pragma warning disable IDE0059 //作为ref扩展方法，其理应修改原向量值，因此不应警告无需赋值。
    extension(ref Vector2 vector)
    {
        /// <summary>
        /// 将另一个向量的分量值复制到当前向量。
        /// </summary>
        /// <param name="other">提供分量值的源向量。</param>
        public void CopyFrom(Vector2 other)
        {
            vector.X = other.X;
            vector.Y = other.Y;
        }

        /// <summary>
        /// 获取或设置向量的模长。设置时保持方向不变。
        /// </summary>
        public float Modulus
        {
            get => vector.Length();
            set => vector.CopyFrom(vector.ToCustomLength(value));
        }

        /// <summary>
        /// 获取或设置向量的旋转角度（弧度）。设置时保持模长不变。
        /// </summary>
        public float Rotation
        {
            get => vector.ToRotation();
            set => vector = new PolarVector2(vector.Length(), value);
        }
    }
#pragma warning restore IDE0059

    extension(Vector2)
    {
        /// <summary>
        /// 计算从起点指向终点的方向向量的旋转角度。
        /// </summary>
        /// <param name="from">起点向量。</param>
        /// <param name="to">终点向量。</param>
        /// <returns>方向向量 (to - from) 的弧度角度。</returns>
        public static float GetRotation(Vector2 from, Vector2 to) => (to - from).ToRotation();

        /// <summary>
        /// 计算两个非零向量之间的夹角（弧度）。
        /// </summary>
        /// <param name="a">第一个向量。</param>
        /// <param name="b">第二个向量。</param>
        /// <returns>两向量之间的夹角，范围 [0, π]。若任一向量为零向量则返回 0。</returns>
        public static float IncludedAngle(Vector2 a, Vector2 b)
        {
            if (a == Vector2.Zero || b == Vector2.Zero)
                return 0f;
            return MathF.Acos(Vector2.Dot(a, b) / (a.Modulus * b.Modulus));
        }

        /// <summary>
        /// 获取两个向量角平分线的单位方向向量，表示为极坐标向量。
        /// </summary>
        /// <param name="a">第一个向量。</param>
        /// <param name="b">第二个向量。</param>
        /// <returns>角平分线方向的单位极坐标向量。</returns>
        public static PolarVector2 UnitAngleBisector(Vector2 a, Vector2 b) => new((a.ToRotation() + b.ToRotation()) / 2);

        /// <summary>
        /// 计算两个二维向量的叉积（Z分量）。
        /// </summary>
        /// <param name="a">第一个向量。</param>
        /// <param name="b">第二个向量。</param>
        /// <returns>叉积值 a.X * b.Y - a.Y * b.X。</returns>
        public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        /// <summary>
        /// 对点进行位似变换。
        /// </summary>
        /// <param name="value">待变换的点向量。</param>
        /// <param name="center">位似中心点。</param>
        /// <param name="ratio">位似比（缩放系数）。</param>
        /// <returns>变换后的点向量。</returns>
        public static Vector2 Homothetic(Vector2 value, Vector2 center, float ratio) => center + ratio * (value - center);

        /// <summary>
        /// 在两个向量之间使用 Smootherstep 插值函数进行分量级插值。
        /// </summary>
        /// <param name="from">起始向量。</param>
        /// <param name="to">目标向量。</param>
        /// <param name="amount">插值因子，通常范围 [0, 1]。</param>
        /// <param name="clamped">是否将 <paramref name="amount"/> 钳制在 [0, 1] 范围内。</param>
        /// <returns>插值结果向量。</returns>
        public static Vector2 SmootherStep(Vector2 from, Vector2 to, float amount, bool clamped = true) => new(TOMathUtils.Interpolation.SmootherStep(from.X, to.X, amount, clamped), TOMathUtils.Interpolation.SmootherStep(from.Y, to.Y, amount, clamped));

        /// <summary>
        /// 在多个向量之间进行线性插值。
        /// </summary>
        /// <param name="vectors">包含至少一个向量的列表。若为 <see langword="null"/> 或空列表将引发异常。</param>
        /// <param name="amount">插值比率，范围 [0, 1]。0 对应第一个向量，1 对应最后一个向量。</param>
        /// <returns>插值后的向量。</returns>
        /// <exception cref="ArgumentException">当 <paramref name="vectors"/> 为 <see langword="null"/> 或空列表时抛出。</exception>
        public static Vector2 LerpMany(List<Vector2> vectors, float amount)
        {
            ArgumentException.ThrowIfNullOrEmpty(vectors);

            switch (vectors.Count)
            {
                case 1:
                    return vectors[0];
                case 2:
                    return Vector2.Lerp(vectors[0], vectors[1], amount);
                default:
                    if (amount <= 0f)
                        return vectors[0];
                    if (amount >= 1f)
                        return vectors[^1];
                    (int index, float localRatio) = TOMathUtils.SplitFloat(amount * (vectors.Count - 1));
                    return Vector2.Lerp(vectors[index], vectors[index + 1], localRatio);
            }
        }
    }
}