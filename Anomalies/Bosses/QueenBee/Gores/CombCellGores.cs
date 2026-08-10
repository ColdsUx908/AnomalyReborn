// Developed by ColdsUx

namespace Anomalies.Bosses.QueenBee;

public sealed partial class QueenBee_Handler
{
    public static void SpawnGores(CombCell cell)
    {
        if (Main.dedServ)
            return;

        Projectile projectile = cell.Projectile;

        int index = Main.rand.Next(0, 3);
        int length = GetLength(index);

        IEntitySource source = projectile.GetSource_FromThis();

        for (int i = 0; i < length; i++)
        {
            ModGore templateGore = AnomalyMain.Instance.Find<ModGore>($"CombCellGore_{index + 1}_{i + 1}");
            Gore.NewGorePerfectAction(source, projectile.Center, templateGore.Type, g =>
            {
                g.velocity = projectile.velocity + new PolarVector2(Main.rand.NextFloat(5f, 8f), Main.rand.NextFloat(MathHelper.Pi)); //只获取方向向下的初速度
                g.scale = projectile.scale;
                g.alpha = 175;
                g.timeLeft = 240;
            });
        }

        static int GetLength(int zerobasedIndex) => zerobasedIndex switch
        {
            0 => 7,
            1 => 6,
            2 => 5,
            _ => 0
        };
    }
}