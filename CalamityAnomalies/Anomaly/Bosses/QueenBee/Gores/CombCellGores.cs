// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.Bosses.QueenBee;

public sealed partial class QueenBee_Handler
{
    public sealed class CombCellGores : IContentLoader
    {
        private static ModGore[][] _CombCellGores;

        void IContentLoader.PostSetupContent()
        {
            if (Main.dedServ)
                return;

            _CombCellGores = new ModGore[3][];
            for (int i = 0; i < _CombCellGores.Length; i++)
            {
                int length = GetLength(i);
                _CombCellGores[i] = new ModGore[length];
                for (int j = 0; j < length; j++)
                {
                    string goreName = $"CombCellGore_{i + 1}_{j + 1}";
                    _CombCellGores[i][j] = CAMain.Instance.Find<ModGore>(goreName);
                }
            }
        }

        void IContentLoader.OnModUnload()
        {
            _CombCellGores = null;
        }

        public static int GetLength(int zerobasedIndex) => zerobasedIndex switch
        {
            0 => 7,
            1 => 6,
            2 => 5,
            _ => 0
        };
    }

    public static void SpawnGores(CombCell cell)
    {
        if (Main.dedServ)
            return;

        Projectile projectile = cell.Projectile;

        int index = Main.rand.Next(0, 3);
        int length = CombCellGores.GetLength(index);

        IEntitySource source = projectile.GetSource_FromThis();

        for (int i = 0; i < length; i++)
        {
            ModGore templateGore = /* _CombCellGores[index][i] */ CAMain.Instance.Find<ModGore>($"CombCellGore_{index + 1}_{i + 1}");
            Gore.NewGorePerfectAction(source, projectile.Center, templateGore.Type, g =>
            {
                g.velocity = projectile.velocity + new PolarVector2(Main.rand.NextFloat(5f, 8f), Main.rand.NextFloat(MathHelper.Pi)); //只获取方向向下的初速度
                g.scale = projectile.scale;
                g.alpha = 175;
                g.timeLeft = 240;
            });
        }
    }
}