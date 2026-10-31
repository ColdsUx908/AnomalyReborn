namespace Transoceanic.Extensions.Terraria;

/// <summary>
/// 提供 Terraria 实体、生成、世界与着色器的便捷扩展。
/// </summary>
public static partial class TOTerrariaExtensions
{
    extension(Gore)
    {
        /// <summary>
        /// 生成一个新的 <see cref="Gore"/>，并在生成后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成中心的世界像素坐标；生成后减去实际宽高的一半以修正位置。</param>
        /// <param name="velocity">传给原版生成入口的速度；类型初始化仍可能改变速度。Perfect 重载会在回调前归零。</param>
        /// <param name="type">血污类型 ID。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <see langword="null"/>。</param>
        public static void NewGoreAction(IEntitySource source, Vector2 position, Vector2 velocity, int type, Action<Gore> action = null)
        {
            int index = Gore.NewGore(source, position, velocity, type);
            if (index < Main.maxGore)
            {
                Gore gore = Main.gore[index];
                gore.position -= gore.AABBRectangle.Size() / 2f;
                action?.Invoke(gore);
            }
        }

        /// <summary>
        /// 生成一个新的 <see cref="ModGore"/> 类型的血污，并在生成后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModGore"/> 的血污类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成中心的世界像素坐标；生成后减去实际宽高的一半以修正位置。</param>
        /// <param name="velocity">传给原版生成入口的速度；类型初始化仍可能改变速度。Perfect 重载会在回调前归零。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <see langword="null"/>。</param>
        public static void NewGoreAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreAction(source, position, velocity, ModContent.GoreType<T>(), action);

        /// <summary>
        /// 生成一个新的 <see cref="Gore"/>，并将位置精确设置为指定点（无速度），之后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成后强制设置的中心世界像素坐标。</param>
        /// <param name="type">血污类型 ID。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <see langword="null"/>。</param>
        public static void NewGorePerfectAction(IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreAction(source, position, Vector2.Zero, type, g =>
            {
                g.position = position - g.AABBRectangle.Size() / 2f;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        /// <summary>
        /// 生成一个新的 <see cref="ModGore"/> 类型的血污，并将位置精确设置为指定点（无速度），之后执行一个 <see cref="Action{Gore}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModGore"/> 的血污类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成后强制设置的中心世界像素坐标。</param>
        /// <param name="action">生成成功后对血污执行的行为，可为 <see langword="null"/>。</param>
        public static void NewGorePerfectAction<T>(IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGorePerfectAction(source, position, ModContent.GoreType<T>(), action);
    }
}
