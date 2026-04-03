namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Dust)
    {
        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustAction(Vector2 position, int width, int height, int type, Vector2 velocity = default, Action<Dust> action = null)
        {
            int index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type, velocity.X, velocity.Y);
            if (index < Main.maxDust)
                action?.Invoke(Main.dust[index]);
        }

        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustAction<T>(Vector2 position, int width, int height, Vector2 velocity = default, Action<Dust> action = null) where T : ModDust =>
            NewDustAction(position, width, height, ModContent.DustType<T>(), velocity, action);

        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
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
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int width, int height, Vector2 velocity, Action<Dust> action = null) where T : ModDust =>
            NewDustActionCheck(out index, out dust, position, width, height, ModContent.DustType<T>(), velocity, action);

        /// <summary>
        /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
        /// </summary>
        /// <param name="position">生成位置。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustPerfectAction(Vector2 position, int type, Action<Dust> action = null)
        {
            Dust dustSpawned = Dust.NewDustPerfect(position, type);
            if (dustSpawned.dustIndex < Main.maxDust)
                action?.Invoke(dustSpawned);
        }

        /// <summary>
        /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="position">生成位置。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustPerfectAction<T>(Vector2 position, Action<Dust> action = null) where T : ModDust =>
            NewDustPerfectAction(position, ModContent.DustType<T>(), action);

        /// <summary>
        /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
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
        /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustPerfectActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, Action<Dust> action = null) where T : ModDust =>
            NewDustPerfectActionCheck(out index, out dust, position, ModContent.DustType<T>(), action);
    }
}