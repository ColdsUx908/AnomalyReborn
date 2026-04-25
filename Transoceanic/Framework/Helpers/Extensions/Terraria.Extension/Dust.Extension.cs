// Designed by ColdsUx

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Dust)
    {
        /// <summary>
        /// 生成一个新的 <see cref="Dust"/>，并在生成后执行一个 <see cref="Action{Dust}"/>。
        /// </summary>
        /// <param name="position">生成中心位置（非左上角）。</param>
        /// <param name="width">X 轴随机偏移的最大值。</param>
        /// <param name="height">Y 轴随机偏移的最大值。</param>
        /// <param name="type">尘埃类型 ID。</param>
        /// <param name="velocity">初始速度，默认为零向量。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        public static void NewDustAction(Vector2 position, int width, int height, int type, Vector2 velocity = default, Action<Dust> action = null)
        {
            int index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type, velocity.X, velocity.Y);
            if (index < Main.maxDust)
                action?.Invoke(Main.dust[index]);
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModDust"/> 类型的尘埃，并在生成后执行一个 <see cref="Action{Dust}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModDust"/> 的尘埃类型。</typeparam>
        /// <param name="position">生成中心位置（非左上角）。</param>
        /// <param name="width">X 轴随机偏移的最大值。</param>
        /// <param name="height">Y 轴随机偏移的最大值。</param>
        /// <param name="velocity">初始速度，默认为零向量。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        public static void NewDustAction<T>(Vector2 position, int width, int height, Vector2 velocity = default, Action<Dust> action = null) where T : ModDust =>
            NewDustAction(position, width, height, ModContent.DustType<T>(), velocity, action);

        /// <summary>
        /// 生成一个新的 <see cref="Dust"/>，并在生成后执行一个 <see cref="Action{Dust}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <param name="index">输出尘埃在 <see cref="Main.dust"/> 中的索引。</param>
        /// <param name="dust">输出尘埃实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="position">生成中心位置（非左上角）。</param>
        /// <param name="width">X 轴随机偏移的最大值。</param>
        /// <param name="height">Y 轴随机偏移的最大值。</param>
        /// <param name="type">尘埃类型 ID。</param>
        /// <param name="velocity">初始速度，默认为零向量。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果尘埃生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewDustActionCheck(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int width, int height, int type, Vector2 velocity = default, Action<Dust> action = null)
        {
            index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type, velocity.X, velocity.Y);
            if (index < Main.maxDust)
            {
                dust = Main.dust[index];
                action?.Invoke(dust);
                return true;
            }
            else
            {
                dust = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModDust"/> 类型的尘埃，并在生成后执行一个 <see cref="Action{Dust}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModDust"/> 的尘埃类型。</typeparam>
        /// <param name="index">输出尘埃在 <see cref="Main.dust"/> 中的索引。</param>
        /// <param name="dust">输出尘埃实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="position">生成中心位置（非左上角）。</param>
        /// <param name="width">X 轴随机偏移的最大值。</param>
        /// <param name="height">Y 轴随机偏移的最大值。</param>
        /// <param name="velocity">初始速度，默认为零向量。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果尘埃生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewDustActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int width, int height, Vector2 velocity, Action<Dust> action = null) where T : ModDust =>
            NewDustActionCheck(out index, out dust, position, width, height, ModContent.DustType<T>(), velocity, action);

        /// <summary>
        /// 生成一个新的 <see cref="Dust"/>（无随机偏移），并在生成后执行一个 <see cref="Action{Dust}"/>。
        /// </summary>
        /// <param name="position">生成位置（精确中心）。</param>
        /// <param name="type">尘埃类型 ID。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        public static void NewDustPerfectAction(Vector2 position, int type, Action<Dust> action = null)
        {
            Dust dustSpawned = Dust.NewDustPerfect(position, type);
            if (dustSpawned.dustIndex < Main.maxDust)
                action?.Invoke(dustSpawned);
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModDust"/> 类型的尘埃（无随机偏移），并在生成后执行一个 <see cref="Action{Dust}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModDust"/> 的尘埃类型。</typeparam>
        /// <param name="position">生成位置（精确中心）。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        public static void NewDustPerfectAction<T>(Vector2 position, Action<Dust> action = null) where T : ModDust =>
            NewDustPerfectAction(position, ModContent.DustType<T>(), action);

        /// <summary>
        /// 生成一个新的 <see cref="Dust"/>（无随机偏移），并在生成后执行一个 <see cref="Action{Dust}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <param name="index">输出尘埃在 <see cref="Main.dust"/> 中的索引。</param>
        /// <param name="dust">输出尘埃实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="position">生成位置（精确中心）。</param>
        /// <param name="type">尘埃类型 ID。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果尘埃生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewDustPerfectActionCheck(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int type, Action<Dust> action = null)
        {
            Dust dustSpawned = Dust.NewDustPerfect(position, type);
            index = dustSpawned.dustIndex;
            if (index < Main.maxDust)
            {
                dust = dustSpawned;
                action?.Invoke(dust);
                return true;
            }
            else
            {
                dust = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModDust"/> 类型的尘埃（无随机偏移），并在生成后执行一个 <see cref="Action{Dust}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModDust"/> 的尘埃类型。</typeparam>
        /// <param name="index">输出尘埃在 <see cref="Main.dust"/> 中的索引。</param>
        /// <param name="dust">输出尘埃实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="position">生成位置（精确中心）。</param>
        /// <param name="action">生成成功后对尘埃执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果尘埃生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewDustPerfectActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, Action<Dust> action = null) where T : ModDust =>
            NewDustPerfectActionCheck(out index, out dust, position, ModContent.DustType<T>(), action);
    }
}