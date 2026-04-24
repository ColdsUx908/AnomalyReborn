namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Gore)
    {
        /// <summary>
        /// 生成一个新的 <see cref="Gore"/>，并在生成后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="type">血污类型 ID。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        public static void NewGoreAction(IEntitySource source, Vector2 position, Vector2 velocity, int type, Action<Gore> action = null)
        {
            int index = Gore.NewGore(source, position, velocity, type);
            if (index < Main.maxGore)
                action?.Invoke(Main.gore[index]);
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModGore"/> 类型的血污，并在生成后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModGore"/> 的血污类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        public static void NewGoreAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreAction(source, position, velocity, ModContent.GoreType<T>(), action);

        /// <summary>
        /// 生成一个新的 <see cref="Gore"/>，并在生成后执行一个 <see cref="Action{Gore}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <param name="index">输出血污在 <see cref="Main.gore"/> 中的索引。</param>
        /// <param name="gore">输出血污实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="type">血污类型 ID。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果血污生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewGoreActionCheck(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Vector2 velocity, int type, Action<Gore> action = null)
        {
            index = Gore.NewGore(source, position, velocity, type);
            if (index < Main.maxGore)
            {
                gore = Main.gore[index];
                action?.Invoke(gore);
                return true;
            }
            else
            {
                gore = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModGore"/> 类型的血污，并在生成后执行一个 <see cref="Action{Gore}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModGore"/> 的血污类型。</typeparam>
        /// <param name="index">输出血污在 <see cref="Main.gore"/> 中的索引。</param>
        /// <param name="gore">输出血污实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果血污生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewGoreActionCheck<T>(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionCheck(out index, out gore, source, position, velocity, ModContent.GoreType<T>(), action);

        /// <summary>
        /// 生成一个新的 <see cref="Gore"/>，并将位置精确设置为指定点（无速度），之后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">精确生成位置。</param>
        /// <param name="type">血污类型 ID。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        public static void NewGoreActionPerfect(IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreAction(source, position, Vector2.Zero, type, g =>
            {
                g.position = position;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        /// <summary>
        /// 生成一个新的 <see cref="ModGore"/> 类型的血污，并将位置精确设置为指定点（无速度），之后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModGore"/> 的血污类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">精确生成位置。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        public static void NewGoreActionPerfect<T>(IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionPerfect(source, position, ModContent.GoreType<T>(), action);

        /// <summary>
        /// 生成一个新的 <see cref="Gore"/>，并将位置精确设置为指定点（无速度），之后执行一个 <see cref="Action{Gore}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <param name="index">输出血污在 <see cref="Main.gore"/> 中的索引。</param>
        /// <param name="gore">输出血污实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">精确生成位置。</param>
        /// <param name="type">血污类型 ID。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果血污生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewGoreActionPerfectCheck(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreActionCheck(out index, out gore, source, position, Vector2.Zero, type, g =>
            {
                g.position = position;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        /// <summary>
        /// 生成一个新的 <see cref="ModGore"/> 类型的血污，并将位置精确设置为指定点（无速度），之后执行一个 <see cref="Action{Gore}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModGore"/> 的血污类型。</typeparam>
        /// <param name="index">输出血污在 <see cref="Main.gore"/> 中的索引。</param>
        /// <param name="gore">输出血污实例，若生成失败则为 <c>null</c>。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">精确生成位置。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <c>null</c>。</param>
        /// <returns>如果血污生成成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool NewGoreActionPerfectCheck<T>(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionPerfectCheck(out index, out gore, source, position, ModContent.GoreType<T>(), action);
    }
}