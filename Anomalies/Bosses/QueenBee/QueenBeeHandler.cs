namespace Anomalies.Bosses.QueenBee;

public sealed class QueenBeeHandler
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

    public static Vector2 GetOwnedCombCellCenter(NPC master) => master.Center + new Vector2(0f, -30f * master.scale);

    public static List<int> HoneyWetCombCells = [];

    public sealed class HoneyWetCombCellManager : ModSystem, IContentLoader
    {
        public override void PreUpdateProjectiles()
        {
            HoneyWetCombCells.Clear();
        }

        void IContentLoader.PostSetupContent()
        {
            On_Collision.WetCollision += On_Collision_WetCollision;
        }

        private static bool On_Collision_WetCollision(On_Collision.orig_WetCollision orig, Vector2 Position, int Width, int Height)
        {
            bool result = orig(Position, Width, Height);

            if (Collision.honey)
                return result;

            Rectangle hitbox = TOMathUtils.Collision.GetAdjustedHitboxForWetCollision(Position, Width, Height);

            foreach (int cellIndex in HoneyWetCombCells)
            {
                if (Projectile.TryGetProjectileFromIndex(cellIndex, out Projectile projectile) && projectile.active && projectile.TryGetModProjectile(out CombCell cell))
                {
                    if (cell.HitBox.Collides(hitbox))
                    {
                        Collision.honey = true;
                        return true;
                    }
                }
            }

            return result;
        }
    }
}

