using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Vector2 vector)
    {
        public void Deconstruct(out float x, out float y)
        {
            x = vector.X;
            y = vector.Y;
        }

        public float ToRotation(float rotationOffset) => vector.ToRotation() + rotationOffset;

        /// <summary>
        /// 安全地将向量化为单位向量。
        /// </summary>
        /// <returns>零向量返回零向量，否则返回单位向量。</returns>
        public Vector2 SafeNormalize() => vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);

        /// <summary>
        /// 获取模为特定值的与原向量共线的向量。不改变原向量值。
        /// </summary>
        /// <returns></returns>
        public Vector2 ToCustomLength(float length) => vector.SafeNormalize() * length;

        public Vector2 RotatedByRandom() => vector.RotatedByRandom(MathHelper.Pi);
        public Vector2 RotatedByRandom(float minRadian, float maxRadian) => vector.RotatedByRandom(Main.rand.NextFloat(minRadian, maxRadian));

        public Point WorldCoordinateSafe => new(Math.Clamp((int)vector.X >> 4, 0, Main.maxTilesX), Math.Clamp((int)vector.Y >> 4, 0, Main.maxTilesY));
    }

#pragma warning disable IDE0059 //作为ref扩展方法，其理应修改原向量值，因此不应警告无需赋值。
    extension(ref Vector2 vector)
    {
        public void CopyFrom(Vector2 other)
        {
            vector.X = other.X;
            vector.Y = other.Y;
        }

        public float Modulus
        {
            get => vector.Length();
            set => vector.CopyFrom(vector.ToCustomLength(value));
        }

        public float Rotation
        {
            get => vector.ToRotation();
            set => vector = new PolarVector2(vector.Length(), value);
        }
    }
#pragma warning restore IDE0059

    extension(Vector2)
    {
        public static float GetRotation(Vector2 from, Vector2 to) => (to - from).ToRotation();

        /// <summary>
        /// 计算两个向量的夹角。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float IncludedAngle(Vector2 a, Vector2 b)
        {
            if (a == Vector2.Zero || b == Vector2.Zero)
                return 0f;
            return MathF.Acos(Vector2.Dot(a, b) / (a.Modulus * b.Modulus));
        }

        /// <summary>
        /// 获取两个向量角平分线的单位方向向量。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PolarVector2 UnitAngleBisector(Vector2 a, Vector2 b) => new((a.ToRotation() + b.ToRotation()) / 2);

        public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        /// <summary>
        /// 位似变换。
        /// </summary>
        /// <param name="value">待变换点。</param>
        /// <param name="center">位似中心。</param>
        /// <param name="ratio">位似比。</param>
        /// <returns></returns>
        public static Vector2 Homothetic(Vector2 value, Vector2 center, float ratio) => center + ratio * (value - center);

        public static Vector2 SmootherStep(Vector2 from, Vector2 to, float amount, bool clamped = true) => new(TOMathUtils.Interpolation.SmootherStep(from.X, to.X, amount, clamped), TOMathUtils.Interpolation.SmootherStep(from.Y, to.Y, amount, clamped));

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