namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Gore)
    {
        public static void NewGoreAction(IEntitySource source, Vector2 position, Vector2 velocity, int type, Action<Gore> action = null)
        {
            int index = Gore.NewGore(source, position, velocity, type);
            if (index < Main.maxGore)
                action?.Invoke(Main.gore[index]);
        }

        public static void NewGoreAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreAction(source, position, velocity, ModContent.GoreType<T>(), action);

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

        public static bool NewGoreActionCheck<T>(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionCheck(out index, out gore, source, position, velocity, ModContent.GoreType<T>(), action);

        public static void NewGoreActionPerfect(IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreAction(source, position, Vector2.Zero, type, g =>
            {
                g.position = position;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        public static void NewGoreActionPerfect<T>(IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionPerfect(source, position, ModContent.GoreType<T>(), action);

        public static bool NewGoreActionPerfectCheck(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreActionCheck(out index, out gore, source, position, Vector2.Zero, type, g =>
            {
                g.position = position;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        public static bool NewGoreActionPerfectCheck<T>(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionPerfectCheck(out index, out gore, source, position, ModContent.GoreType<T>(), action);
    }
}